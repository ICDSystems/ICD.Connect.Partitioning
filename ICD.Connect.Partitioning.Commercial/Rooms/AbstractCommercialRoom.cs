using System;
using System.Collections.Generic;
using System.Text;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Services.Scheduler;
using ICD.Connect.API.Commands;
using ICD.Connect.Conferencing.ConferenceManagers;
using ICD.Connect.Conferencing.ConferencePoints;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	public abstract class AbstractCommercialRoom<TSettings> : AbstractRoom<TSettings>, ICommercialRoom
		where TSettings : ICommercialRoomSettings, new()
	{
		/// <summary>
		/// Raised when the conference manager changes.
		/// </summary>
		public event EventHandler<GenericEventArgs<IConferenceManager>> OnConferenceManagerChanged;

		private IConferenceManager m_ConferenceManager;

		#region Properties

		/// <summary>
		/// Gets the scheduler service.
		/// </summary>
		private IActionSchedulerService SchedulerService
		{
			get { return ServiceProvider.TryGetService<IActionSchedulerService>(); }
		}

		/// <summary>
		/// Gets the wake/sleep schedule.
		/// </summary>
		public WakeSchedule WakeSchedule { get; private set; }

		/// <summary>
		/// Gets the path to the loaded dialing plan xml file. Used by fusion :(
		/// </summary>
		public virtual string DialingPlan { get; private set; }

		/// <summary>
		/// Gets the conference manager.
		/// </summary>
		public IConferenceManager ConferenceManager
		{
			get { return m_ConferenceManager; }
			protected set
			{
				if (m_ConferenceManager == value)
					return;

				m_ConferenceManager = value;

				OnConferenceManagerChanged.Raise(this, new GenericEventArgs<IConferenceManager>(m_ConferenceManager));
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractCommercialRoom()
		{
			WakeSchedule = new WakeSchedule();

			Subscribe(WakeSchedule);

			SchedulerService.Add(WakeSchedule);
		}

		/// <summary>
		/// Release resources
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			Unsubscribe(WakeSchedule);

			SchedulerService.Remove(WakeSchedule);
		}

		#region Methods

		/// <summary>
		/// Shuts down the room.
		/// </summary>
		public abstract void Sleep();

		/// <summary>
		/// Wakes up the room.
		/// </summary>
		public abstract void Wake();

		/// <summary>
		/// Returns true if a source is actively routed to a display or we are in a conference.
		/// </summary>
		/// <returns></returns>
		protected abstract bool GetIsInActiveMeeting();

		#endregion

		#region WakeSchedule Callbacks

		/// <summary>
		/// Subscribe to the schedule events.
		/// </summary>
		/// <param name="schedule"></param>
		private void Subscribe(WakeSchedule schedule)
		{
			schedule.OnWakeActionRequested += ScheduleOnWakeActionRequested;
			schedule.OnSleepActionRequested += ScheduleOnSleepActionRequested;
		}

		/// <summary>
		/// Unsubscribe from the schedule events.
		/// </summary>
		/// <param name="schedule"></param>
		private void Unsubscribe(WakeSchedule schedule)
		{
			schedule.OnWakeActionRequested -= ScheduleOnWakeActionRequested;
			schedule.OnSleepActionRequested -= ScheduleOnSleepActionRequested;
		}

		/// <summary>
		/// Called when a sleep action is scheduled.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void ScheduleOnSleepActionRequested(object sender, EventArgs eventArgs)
		{
			if (CombineState)
				return;

			if (GetIsInActiveMeeting())
				return;

			Log(eSeverity.Informational, "Scheduled sleep occurring at {0}", IcdEnvironment.GetLocalTime().ToShortTimeString());
			Sleep();
		}

		/// <summary>
		/// Called when a wake action is scheduled.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void ScheduleOnWakeActionRequested(object sender, EventArgs eventArgs)
		{
			if (CombineState)
				return;

			if (GetIsInActiveMeeting())
				return;

			Log(eSeverity.Informational, "Scheduled wake occurring at {0}", IcdEnvironment.GetLocalTime().ToShortTimeString());
			Wake();
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			// Wake Schedule
			WakeSchedule.Copy(settings.WakeSchedule);

			// Dialing plan
			SetDialingPlan(settings.DialingPlan);
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			WakeSchedule.Clear();
			DialingPlan = null;

			m_ConferenceManager.ClearDialingProviders();
			m_ConferenceManager.Favorites = null;
			m_ConferenceManager.DialingPlan.ClearMatchers();
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.WakeSchedule.Copy(WakeSchedule);

			settings.DialingPlan = DialingPlan;
		}

		/// <summary>
		/// Sets the dialing plan from the settings.
		/// </summary>
		/// <param name="path"></param>
		private void SetDialingPlan(string path)
		{
			DialingPlan = path;

			if (!string.IsNullOrEmpty(path))
				path = PathUtils.GetDefaultConfigPath("DialingPlans", path);

			try
			{
				if (string.IsNullOrEmpty(path))
					Log(eSeverity.Warning, "No Dialing Plan configured");
				else
				{
					string xml = IcdFile.ReadToEnd(path, new UTF8Encoding(false));
					xml = EncodingUtils.StripUtf8Bom(xml);

					m_ConferenceManager.DialingPlan.LoadMatchersFromXml(xml);
				}
			}
			catch (Exception e)
			{
				Log(eSeverity.Error, "failed to load Dialing Plan {0} - {1}", path, e.Message);
			}

			foreach (IConferencePoint conferencePoint in Originators.GetInstancesRecursive<IConferencePoint>())
				m_ConferenceManager.RegisterDialingProvider(conferencePoint);
		}

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("Wake", "Wakes the room", () => Wake());
			yield return new ConsoleCommand("Sleep", "Puts the room to sleep", () => Sleep());
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}