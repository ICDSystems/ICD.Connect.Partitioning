using System;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Conferencing.ConferenceManagers;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Nodes.External;

namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	public sealed class CommercialRoomExternalTelemetryProvider : AbstractExternalTelemetryProvider<ICommercialRoom>
	{
		/// <summary>
		/// Raised when the privacy mute state changed.
		/// </summary>
		[EventTelemetry(CommercialRoomTelemetryNames.MUTE_PRIVACY_CHANGED)]
		public event EventHandler<BoolEventArgs> OnPrivacyMuteChanged;

		private IConferenceManager m_ConferenceManager;
		private bool m_PrivacyMute;

		#region Properties

		/// <summary>
		/// Gets the room privacy mute state.
		/// </summary>
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
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Sets the parent telemetry provider that this instance extends.
		/// </summary>
		/// <param name="parent"></param>
		public override void SetParent(ICommercialRoom parent)
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

		#endregion
	}
}
