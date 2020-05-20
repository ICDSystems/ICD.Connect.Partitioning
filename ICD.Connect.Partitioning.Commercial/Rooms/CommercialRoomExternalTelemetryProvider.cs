﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Calendaring.Bookings;
using ICD.Connect.Calendaring.Controls;
using ICD.Connect.Conferencing.ConferenceManagers;
using ICD.Connect.Conferencing.Conferences;
using ICD.Connect.Conferencing.Controls.Dialing;
using ICD.Connect.Conferencing.DialContexts;
using ICD.Connect.Conferencing.EventArguments;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Nodes.External;

namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	public sealed class CommercialRoomExternalTelemetryProvider : AbstractExternalTelemetryProvider<ICommercialRoom>
	{
		/// <summary>
		/// Raised when the privacy mute state changes.
		/// </summary>
		[EventTelemetry(CommercialRoomTelemetryNames.MUTE_PRIVACY_CHANGED)]
		public event EventHandler<BoolEventArgs> OnPrivacyMuteChanged;

		/// <summary>
		/// Raised when the active conference device changes.
		/// </summary>
		[EventTelemetry(CommercialRoomTelemetryNames.ACTIVE_CONFERENCE_DEVICE_CHANGED)]
		public event EventHandler<GenericEventArgs<Guid>> OnActiveConferenceDeviceChanged;

		/// <summary>
		/// Raised when the active conference device call-in info changes.
		/// </summary>
		[EventTelemetry(CommercialRoomTelemetryNames.CALL_IN_INFO_CHANGED)]
		public event EventHandler<GenericEventArgs<DialContext>> OnCallInInfoChanged;

		/// <summary>
		/// Raised when the bookings change.
		/// </summary>
		[EventTelemetry(CommercialRoomTelemetryNames.BOOKINGS_CHANGED)]
		public event EventHandler OnBookingsChanged;

		private readonly List<Booking> m_Bookings;
		private readonly List<ICalendarControl> m_CalendarControls;

		private IConferenceManager m_ConferenceManager;
		private Guid m_ActiveConferenceDevice;
		private bool m_PrivacyMute;
		private IConferenceDeviceControl m_ActiveConferenceDeviceControl;
		private DialContext m_CallInInfo;

		#region Properties

		/// <summary>
		/// Gets the room privacy mute state.
		/// </summary>
		[PublicAPI("DAV-PRO - Room Dashboard")]
		[PropertyTelemetry(CommercialRoomTelemetryNames.MUTE_PRIVACY, CommercialRoomTelemetryNames.MUTE_PRIVACY_COMMAND,
			CommercialRoomTelemetryNames.MUTE_PRIVACY_CHANGED)]
		public bool PrivacyMute
		{
			get { return m_PrivacyMute; }
			private set
			{
				if (value == m_PrivacyMute)
					return;

				m_PrivacyMute = value;

				OnPrivacyMuteChanged.Raise(this, new BoolEventArgs(m_PrivacyMute));
			}
		}

		/// <summary>
		/// Gets the active conference device control.
		/// </summary>
		[CanBeNull]
		public IConferenceDeviceControl ActiveConferenceDeviceControl
		{
			get { return m_ActiveConferenceDeviceControl; }
			private set
			{
				if (value == m_ActiveConferenceDeviceControl)
					return;

				Unsubscribe(m_ActiveConferenceDeviceControl);
				m_ActiveConferenceDeviceControl = value;
				Subscribe(m_ActiveConferenceDeviceControl);

				ActiveConferenceDevice =
					m_ActiveConferenceDeviceControl == null
						? Guid.Empty
						: m_ActiveConferenceDeviceControl.Parent.Uuid;

				UpdateCallInInfo();
			}
		}

		/// <summary>
		/// Gets the UUID for the active conference device.
		/// </summary>
		[PublicAPI("DAV-PRO - Room Dashboard")]
		[PropertyTelemetry(CommercialRoomTelemetryNames.ACTIVE_CONFERENCE_DEVICE,
			null,
			CommercialRoomTelemetryNames.ACTIVE_CONFERENCE_DEVICE_CHANGED)]
		public Guid ActiveConferenceDevice
		{
			get { return m_ActiveConferenceDevice; }
			private set
			{
				if (value == m_ActiveConferenceDevice)
					return;

				m_ActiveConferenceDevice = value;

				OnActiveConferenceDeviceChanged.Raise(this, new GenericEventArgs<Guid>(m_ActiveConferenceDevice));
			}
		}

		/// <summary>
		/// Gets the call in info for the active conference device..
		/// </summary>
		[PublicAPI("DAV-PRO - Room Dashboard")]
		[PropertyTelemetry(CommercialRoomTelemetryNames.CALL_IN_INFO,
			null, CommercialRoomTelemetryNames.CALL_IN_INFO_CHANGED)]
		public DialContext CallInInfo
		{
			get { return m_CallInInfo; }
			private set
			{
				if (value == m_CallInInfo)
					return;

				m_CallInInfo = value;

				OnCallInInfoChanged.Raise(this, new GenericEventArgs<DialContext>(m_CallInInfo));
			}
		}

		/// <summary>
		/// Gets the current bookings for the room.
		/// </summary>
		[PublicAPI("DAV-PRO - Room Bookings")]
		[PropertyTelemetry(CommercialRoomTelemetryNames.BOOKINGS, null, CommercialRoomTelemetryNames.BOOKINGS_CHANGED)]
		public IEnumerable<Booking> Bookings
		{
			get { return m_Bookings.ToArray(); }
			private set
			{
				m_Bookings.Clear();
				m_Bookings.AddRange(value);

				OnBookingsChanged.Raise(this);
			}
		}

		/// <summary>
		/// Gets/sets the wrapped conference manager.
		/// </summary>
		private IConferenceManager ConferenceManager
		{
			get { return m_ConferenceManager; }
			set
			{
				if (value == m_ConferenceManager)
					return;
				
				Unsubscribe(m_ConferenceManager);
				m_ConferenceManager = value;
				Subscribe(m_ConferenceManager);

				UpdatePrivacyMute();
				UpdateActiveConferenceDevice();
			}
		}

		/// <summary>
		/// Gets/sets the wrapped calendar controls.
		/// </summary>
		public IEnumerable<ICalendarControl> CalendarControls
		{
			get { return m_CalendarControls.ToArray(); }
			private set
			{
				UnsubscribeCalendarControls(m_CalendarControls);
				m_CalendarControls.Clear();
				m_CalendarControls.AddRange(value);
				SubscribeCalendarControls(m_CalendarControls);

				UpdateBookings();
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public CommercialRoomExternalTelemetryProvider()
		{
			m_Bookings = new List<Booking>();
			m_CalendarControls = new List<ICalendarControl>();
		}

		#region Methods

		/// <summary>
		/// Sets the parent telemetry provider that this instance extends.
		/// </summary>
		/// <param name="parent"></param>
		public override void SetParent(ICommercialRoom parent)
		{
			base.SetParent(parent);

			UpdateConferenceManager();
			UpdateCalendarControls();
		}

		/// <summary>
		/// Sets the privacy mute state for the room.
		/// </summary>
		/// <param name="privacyMute"></param>
		[MethodTelemetry(CommercialRoomTelemetryNames.MUTE_PRIVACY_COMMAND)]
		public void SetPrivacyMute(bool privacyMute)
		{
			if (m_ConferenceManager != null)
				m_ConferenceManager.PrivacyMuted = privacyMute;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Updates the wrapped calendar controls.
		/// </summary>
		private void UpdateCalendarControls()
		{
			CalendarControls =
				Parent == null
					? Enumerable.Empty<ICalendarControl>()
					: Parent.GetCalendarControls();
		}

		/// <summary>
		/// Updates the wrapped conference manager.
		/// </summary>
		private void UpdateConferenceManager()
		{
			ConferenceManager = Parent == null ? null : Parent.ConferenceManager;
		}

		/// <summary>
		/// Updates the privacy mute state.
		/// </summary>
		private void UpdatePrivacyMute()
		{
			PrivacyMute = m_ConferenceManager != null && m_ConferenceManager.PrivacyMuted;
		}

		/// <summary>
		/// Updates the active conference device.
		/// </summary>
		private void UpdateActiveConferenceDevice()
		{
			ActiveConferenceDeviceControl =
				m_ConferenceManager == null
					? null
					: m_ConferenceManager.Dialers
					                     .GetDialingProviders()
					                     .FirstOrDefault(d => d.GetConferences()
					                                           .Any(c => c.GetOnlineParticipants()
					                                                      .Any()));
		}

		/// <summary>
		/// Updates the available bookings.
		/// </summary>
		private void UpdateBookings()
		{
			Bookings = m_CalendarControls.SelectMany(c => c.GetBookings()).Select(b => Booking.Copy(b));
		}

		/// <summary>
		/// Updates the current call-in info.
		/// </summary>
		private void UpdateCallInInfo()
		{
			IDialContext callIn = ActiveConferenceDeviceControl == null ? null : ActiveConferenceDeviceControl.CallInInfo;
			CallInInfo = callIn == null ? null : DialContext.Copy(callIn);
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Subscribe(ICommercialRoom parent)
		{
			base.Subscribe(parent);

			if (parent == null)
				return;

			parent.OnConferenceManagerChanged += ParentOnConferenceManagerChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Unsubscribe(ICommercialRoom parent)
		{
			base.Unsubscribe(parent);

			if (parent == null)
				return;

			parent.OnConferenceManagerChanged -= ParentOnConferenceManagerChanged;
		}

		/// <summary>
		/// Called when the conference manager changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="genericEventArgs"></param>
		private void ParentOnConferenceManagerChanged(object sender, GenericEventArgs<IConferenceManager> genericEventArgs)
		{
			UpdateConferenceManager();
		}

		#endregion

		#region Conference Manager Callbacks

		/// <summary>
		/// Subscribe to the conference manager events.
		/// </summary>
		/// <param name="conferenceManager"></param>
		private void Subscribe(IConferenceManager conferenceManager)
		{
			if (conferenceManager == null)
				return;

			conferenceManager.OnPrivacyMuteStatusChange += ConferenceManagerOnPrivacyMuteStatusChange;
			conferenceManager.Dialers.OnInCallChanged += DialersOnInCallChanged;
		}

		/// <summary>
		/// Unsubscribe from the conference manager events.
		/// </summary>
		/// <param name="conferenceManager"></param>
		private void Unsubscribe(IConferenceManager conferenceManager)
		{
			if (conferenceManager == null)
				return;

			conferenceManager.OnPrivacyMuteStatusChange -= ConferenceManagerOnPrivacyMuteStatusChange;
			conferenceManager.Dialers.OnInCallChanged -= DialersOnInCallChanged;
		}

		/// <summary>
		/// Called when the conference manager privacy mute state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="boolEventArgs"></param>
		private void ConferenceManagerOnPrivacyMuteStatusChange(object sender, BoolEventArgs boolEventArgs)
		{
			UpdatePrivacyMute();
		}

		/// <summary>
		/// Called when the conference manager enters/leaves a call.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="inCallEventArgs"></param>
		private void DialersOnInCallChanged(object sender, InCallEventArgs inCallEventArgs)
		{
			UpdateActiveConferenceDevice();
		}

		#endregion

		#region Calendar Control Callbacks

		/// <summary>
		/// Subscribe to the calendar control events.
		/// </summary>
		/// <param name="calendarControls"></param>
		private void SubscribeCalendarControls(IEnumerable<ICalendarControl> calendarControls)
		{
			foreach (ICalendarControl control in calendarControls)
				Subscribe(control);
		}

		/// <summary>
		/// Unsubscribe from the calendar control events.
		/// </summary>
		/// <param name="calendarControls"></param>
		private void UnsubscribeCalendarControls(IEnumerable<ICalendarControl> calendarControls)
		{
			foreach (ICalendarControl control in calendarControls)
				Unsubscribe(control);
		}

		/// <summary>
		/// Subscribe to the calendar control events.
		/// </summary>
		/// <param name="calendarControl"></param>
		private void Subscribe(ICalendarControl calendarControl)
		{
			calendarControl.OnBookingsChanged += CalendarControlOnBookingsChanged;
		}

		/// <summary>
		/// Unsubscribe from the calendar control events.
		/// </summary>
		/// <param name="calendarControl"></param>
		private void Unsubscribe(ICalendarControl calendarControl)
		{
			calendarControl.OnBookingsChanged -= CalendarControlOnBookingsChanged;
		}

		/// <summary>
		/// Called when the control bookings change.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CalendarControlOnBookingsChanged(object sender, EventArgs e)
		{
			UpdateBookings();
		}

		#endregion

		#region Conference Control Callbacks

		/// <summary>
		/// Subscribe to the conference control events.
		/// </summary>
		/// <param name="conferenceControl"></param>
		private void Subscribe(IConferenceDeviceControl conferenceControl)
		{
			if (conferenceControl == null)
				return;

			conferenceControl.OnCallInInfoChanged += ConferenceControlOnCallInInfoChanged;
		}

		/// <summary>
		/// Unsubscribe from the conference control events.
		/// </summary>
		/// <param name="conferenceControl"></param>
		private void Unsubscribe(IConferenceDeviceControl conferenceControl)
		{
			if (conferenceControl == null)
				return;

			conferenceControl.OnCallInInfoChanged -= ConferenceControlOnCallInInfoChanged;
		}

		/// <summary>
		/// Called when the active conference control call-in info changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void ConferenceControlOnCallInInfoChanged(object sender, GenericEventArgs<IDialContext> eventArgs)
		{
			UpdateCallInInfo();
		}

		#endregion
	}
}
