using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Calendaring.Utils;
using ICD.Connect.Conferencing.ConferenceManagers;
using ICD.Connect.Conferencing.Controls.Dialing;
using ICD.Connect.Conferencing.DialContexts;
using ICD.Connect.Conferencing.EventArguments;
using ICD.Connect.Partitioning.Commercial.CalendarOccupancyManagers.History;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers.External;

namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	public sealed class CommercialRoomExternalTelemetryProvider : AbstractExternalTelemetryProvider<ICommercialRoom>
	{
		/// <summary>
		/// Raised when the privacy mute state changes.
		/// </summary>
		[PublicAPI("DAV-PRO")]
		[EventTelemetry(CommercialRoomTelemetryNames.MUTE_PRIVACY_CHANGED)]
		public event EventHandler<BoolEventArgs> OnPrivacyMuteChanged;

		/// <summary>
		/// Raised when the active conference device changes.
		/// </summary>
		[PublicAPI("DAV-PRO")]
		[EventTelemetry(CommercialRoomTelemetryNames.ACTIVE_CONFERENCE_DEVICE_CHANGED)]
		public event EventHandler<GenericEventArgs<Guid>> OnActiveConferenceDeviceChanged;

		/// <summary>
		/// Raised when the active conference device call-in info changes.
		/// </summary>
		[PublicAPI("DAV-PRO")]
		[EventTelemetry(CommercialRoomTelemetryNames.CALL_IN_INFO_CHANGED)]
		public event EventHandler<GenericEventArgs<DialContext>> OnCallInInfoChanged;

		/// <summary>
		/// Raised when the bookings change.
		/// </summary>
		[PublicAPI("DAV-PRO")]
		[EventTelemetry(CommercialRoomTelemetryNames.BOOKINGS_CHANGED)]
		public event EventHandler OnBookingsChanged;

		private BookingRange m_Bookings;

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
		public BookingRange Bookings
		{
			get { return m_Bookings; }
			private set
			{
				if (value == m_Bookings)
					return;

				m_Bookings = value;

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

		#endregion

		#region Methods

		/// <summary>
		/// Sets the parent telemetry provider that this instance extends.
		/// </summary>
		/// <param name="parent"></param>
		protected override void SetParent(ICommercialRoom parent)
		{
			base.SetParent(parent);

			UpdateConferenceManager();
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
					                     .FirstOrDefault(d => d.GetActiveConference() != null);
		}

		/// <summary>
		/// Updates the available bookings.
		/// </summary>
		private void UpdateBookings()
		{
			DateTime now = IcdEnvironment.GetLocalTime();
			DateTime from = now.StartOfDay().ToUniversalTime();
			DateTime to = now.EndOfDay().ToUniversalTime();

			IEnumerable<HistoricalBooking> bookings =
				Parent == null
					? Enumerable.Empty<HistoricalBooking>()
					: Parent.CalendarOccupancyManager.BookingHistory.GetBookings()
					        .Where(b => CalendarUtils.IsInRange(b, from, to))
					        .Select(b => HistoricalBooking.Copy(b));

			Bookings = new BookingRange
			{
				From = from,
				To = to,
				Bookings = bookings
			};
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

		#region Provider Callbacks

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
			parent.CalendarOccupancyManager.BookingHistory.OnBookingsUpdated += BookingHistoryOnBookingsUpdated;
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
			parent.CalendarOccupancyManager.BookingHistory.OnBookingsUpdated -= BookingHistoryOnBookingsUpdated;
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

		private void BookingHistoryOnBookingsUpdated(object sender, EventArgs e)
		{
			UpdateBookings();
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
