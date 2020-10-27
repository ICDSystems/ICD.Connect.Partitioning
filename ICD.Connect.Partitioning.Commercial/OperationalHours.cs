using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers;

namespace ICD.Connect.Partitioning.Commercial
{
	public sealed class OperationalHours : ITelemetryProvider, IConsoleNode
	{
		#region Events
		
		/// <summary>
		/// Raised when the start time changes
		/// </summary>
		[EventTelemetry(OperationalHoursTelemetryNames.START_TIME_CHANGED)]
		public event EventHandler<GenericEventArgs<TimeSpan?>> OnStartTimeChanged;

		/// <summary>
		/// Raised when the end time changes
		/// </summary>
		[EventTelemetry(OperationalHoursTelemetryNames.END_TIME_CHANGED)]
		public event EventHandler<GenericEventArgs<TimeSpan?>> OnEndTimeChanged;

		/// <summary>
		/// Raised when the days changes
		/// </summary>
		[EventTelemetry(OperationalHoursTelemetryNames.DAYS_CHANGED)]
		public event EventHandler<GenericEventArgs<eDaysOfWeek>>  OnDaysChanged;

		#endregion

		#region Fields

		private TimeSpan m_StartTime;
		private TimeSpan m_EndTime;
		private eDaysOfWeek m_Days;

		#endregion

		#region Properties

		/// <summary>
		/// Start time for the operational hours of the room
		/// </summary>
		[PropertyTelemetry(OperationalHoursTelemetryNames.START_TIME, null, OperationalHoursTelemetryNames.START_TIME_CHANGED)]
		public TimeSpan StartTime
		{
			get { return m_StartTime; }
			private set
			{
				if (m_StartTime == value)
					return;

				m_StartTime = value;

				OnStartTimeChanged.Raise(this, m_StartTime);
			}
		}

		/// <summary>
		/// End time for the operational hours of the room
		/// </summary>
		[PropertyTelemetry(OperationalHoursTelemetryNames.END_TIME, null, OperationalHoursTelemetryNames.END_TIME_CHANGED)]
		public TimeSpan EndTime
		{
			get { return m_EndTime; }
			private set
			{
				if (m_EndTime == value)
					return;

				m_EndTime = value;

				OnEndTimeChanged.Raise(this, m_EndTime);
			}
		}

		/// <summary>
		/// Days of the week that the room is operational
		/// </summary>
		[PropertyTelemetry(OperationalHoursTelemetryNames.DAYS, null, OperationalHoursTelemetryNames.DAYS_CHANGED)]
		public eDaysOfWeek Days
		{
			get { return m_Days; }
			private set
			{
				if (m_Days == value)
					return;

				m_Days = value;

				OnDaysChanged.Raise(this, m_Days);
			}
		}

		#endregion

		#region Settings

		/// <summary>
		/// Clears the settings for this instance
		/// </summary>
		public void ClearSettings()
		{
			StartTime = TimeSpan.Zero;
			EndTime = TimeSpan.Zero;
			Days = eDaysOfWeek.None;
		}

		/// <summary>
		/// Copies the properties onto the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		public void CopySettings(OperationalHoursSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			settings.StartTime = StartTime;
			settings.EndTime = EndTime;
			settings.Days = Days;
		}

		/// <summary>
		/// Applies the settings into this instance
		/// </summary>
		/// <param name="settings"></param>
		public void ApplySettings(OperationalHoursSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			StartTime = settings.StartTime;
			EndTime = settings.EndTime;
			Days = settings.Days;
		}

		#endregion

		#region Telemetry

		/// <summary>
		/// Initializes the current telemetry state.
		/// </summary>
		public void InitializeTelemetry()
		{
		}

		#endregion

		#region Console

		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get { return "OperationalHours"; } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get { return "Operational Hours Settings for the room"; } }

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			yield break;
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			addRow("Start Time", StartTime);
			addRow("End Time", EndTime);
			addRow("Days of Week", Days);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			yield break;
		}

		#endregion
	}
}