using System;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers.External;

namespace ICD.Connect.Partitioning.Commercial
{
	public sealed class OperationalHoursExternalTelemetryProvider : AbstractExternalTelemetryProvider<OperationalHours>
	{
		#region Events

		/// <summary>
		/// Raised when the start time changes
		/// </summary>
		[EventTelemetry(OperationalHoursTelemetryNames.START_TIME_CHANGED)]
		public event EventHandler<GenericEventArgs<TimeSpan>> OnUniversalStartTimeChanged;

		/// <summary>
		/// Raised when the end time changes
		/// </summary>
		[EventTelemetry(OperationalHoursTelemetryNames.END_TIME_CHANGED)]
		public event EventHandler<GenericEventArgs<TimeSpan>> OnUniversalEndTimeChanged;

		#endregion

		#region Fields

		private TimeSpan m_UniversalStartTime;
		private TimeSpan m_UniversalEndTime;

		#endregion

		#region Properties

		/// <summary>
		/// Start time for the operational hours of the room
		/// </summary>
		[PropertyTelemetry(OperationalHoursTelemetryNames.START_TIME, null, OperationalHoursTelemetryNames.START_TIME_CHANGED)]
		public TimeSpan UniversalStartTime
		{
			get { return m_UniversalStartTime; }
			private set
			{
				if (m_UniversalStartTime == value)
					return;

				m_UniversalStartTime = value;

				OnUniversalStartTimeChanged.Raise(this, m_UniversalStartTime);
			}
		}

		/// <summary>
		/// End time for the operational hours of the room
		/// </summary>
		[PropertyTelemetry(OperationalHoursTelemetryNames.END_TIME, null, OperationalHoursTelemetryNames.END_TIME_CHANGED)]
		public TimeSpan UniversalEndTime
		{
			get { return m_UniversalEndTime; }
			private set
			{
				if (m_UniversalEndTime == value)
					return;

				m_UniversalEndTime = value;

				OnUniversalEndTimeChanged.Raise(this, m_UniversalEndTime);
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Initializes the current telemetry state.
		/// </summary>
		public override void InitializeTelemetry()
		{
			base.InitializeTelemetry();

			UpdateUniversalStartTime();
			UpdateUniversalEndTime();
		}

		#endregion

		#region Private Methods

		private void UpdateUniversalStartTime()
		{
			UniversalStartTime = Parent == null ? TimeSpan.Zero : Parent.StartTime.ToUniversalTime();
		}

		private void UpdateUniversalEndTime()
		{
			UniversalEndTime = Parent == null ? TimeSpan.Zero : Parent.EndTime.ToUniversalTime();
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Subscribe(OperationalHours parent)
		{
			base.Subscribe(parent);

			parent.OnStartTimeChanged += ParentOnStartTimeChanged;
			parent.OnEndTimeChanged += ParentOnEndTimeChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Unsubscribe(OperationalHours parent)
		{
			base.Unsubscribe(parent);

			parent.OnStartTimeChanged -= ParentOnStartTimeChanged;
			parent.OnEndTimeChanged -= ParentOnEndTimeChanged;
		}

		/// <summary>
		/// Called when the start time changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="genericEventArgs"></param>
		private void ParentOnStartTimeChanged(object sender, GenericEventArgs<TimeSpan> genericEventArgs)
		{
			UpdateUniversalStartTime();
		}

		/// <summary>
		/// Called when the end time changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="genericEventArgs"></param>
		private void ParentOnEndTimeChanged(object sender, GenericEventArgs<TimeSpan> genericEventArgs)
		{
			UpdateUniversalEndTime();
		}

		#endregion
	}
}
