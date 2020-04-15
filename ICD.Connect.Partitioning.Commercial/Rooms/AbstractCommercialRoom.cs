﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Services.Scheduler;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Audio.VolumePoints;
using ICD.Connect.Calendaring.Controls;
using ICD.Connect.Conferencing.ConferenceManagers;
using ICD.Connect.Conferencing.ConferencePoints;
using ICD.Connect.Conferencing.Conferences;
using ICD.Connect.Conferencing.EventArguments;
using ICD.Connect.Conferencing.Participants;
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

		/// <summary>
		/// Raised when the wake schedule changes.
		/// </summary>
		public event EventHandler<GenericEventArgs<WakeSchedule>> OnWakeScheduleChanged;

		/// <summary>
		/// Raised when the room wakes or goes to sleep.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnIsAwakeStateChanged;

		/// <summary>
		/// Raised when the calendar control changes.
		/// </summary>
		public event EventHandler<GenericEventArgs<ICalendarControl>> OnCalendarControlChanged;

		[CanBeNull] private IConferenceManager m_ConferenceManager;
		[CanBeNull] private WakeSchedule m_WakeSchedule;
		[CanBeNull] private ICalendarControl m_CalendarControl;

		private bool m_IsAwake;

		#region Properties

		/// <summary>
		/// Gets the scheduler service.
		/// </summary>
		private static IActionSchedulerService SchedulerService
		{
			get { return ServiceProvider.TryGetService<IActionSchedulerService>(); }
		}

		/// <summary>
		/// Gets the path to the loaded dialing plan xml file. Used by fusion :(
		/// </summary>
		public virtual string DialingPlan { get; private set; }

		/// <summary>
		/// Gets the wake/sleep schedule.
		/// </summary>
		[CanBeNull]
		public WakeSchedule WakeSchedule
		{
			get { return m_WakeSchedule; }
			protected set
			{
				if (value == m_WakeSchedule)
					return;

				Unsubscribe(m_WakeSchedule);
				if (m_WakeSchedule != null)
					SchedulerService.Remove(m_WakeSchedule);

				m_WakeSchedule = value;

				Subscribe(m_WakeSchedule);
				if (m_WakeSchedule != null)
					SchedulerService.Add(m_WakeSchedule);

				OnWakeScheduleChanged.Raise(this, new GenericEventArgs<WakeSchedule>(m_WakeSchedule));
			}
		}

		/// <summary>
		/// Gets the conference manager.
		/// </summary>
		[CanBeNull]
		public IConferenceManager ConferenceManager
		{
			get { return m_ConferenceManager; }
			protected set
			{
				if (value == m_ConferenceManager)
					return;

				Unsubscribe(m_ConferenceManager);
				m_ConferenceManager = value;
				Subscribe(m_ConferenceManager);

				OnConferenceManagerChanged.Raise(this, new GenericEventArgs<IConferenceManager>(m_ConferenceManager));
			}
		}

		/// <summary>
		/// Gets the CalendarControl for the room.
		/// </summary>
		public ICalendarControl CalendarControl
		{
			get { return m_CalendarControl; }
			protected set
			{
				if (value == m_CalendarControl)
					return;

				m_CalendarControl = value;

				OnCalendarControlChanged.Raise(this, new GenericEventArgs<ICalendarControl>(m_CalendarControl));
			}
		}

		/// <summary>
		/// Gets the awake state.
		/// </summary>
		public bool IsAwake
		{
			get { return m_IsAwake; }
			protected set
			{
				if (value == m_IsAwake)
					return;

				m_IsAwake = value;

				OnIsAwakeStateChanged.Raise(this, new BoolEventArgs(m_IsAwake));
			}
		}

		/// <summary>
		/// Gets the number of seats for this room.
		/// </summary>
		public int SeatCount { get; private set; }

		#endregion

		/// <summary>
		/// Release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnConferenceManagerChanged = null;
			OnWakeScheduleChanged = null;
			OnIsAwakeStateChanged = null;

			base.DisposeFinal(disposing);
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

		/// <summary>
		/// Gets the volume type for the current context.
		/// </summary>
		public override eVolumePointContext GetVolumeContext()
		{
			eVolumePointContext output =  base.GetVolumeContext();

			// Return ATC/VTC if we are in a call
			IConferenceManager conferenceManager = ConferenceManager;
			IParticipant[] sources =
				conferenceManager == null
					? new IParticipant[0]
					: conferenceManager.Dialers
					                   .ActiveConferences
					                   .SelectMany(c => c.GetOnlineParticipants())
					                   .ToArray();

			bool inAudioCall = sources.Any(s => s.CallType.HasFlag(eCallType.Audio));
			bool inVideoCall = sources.Any(s => s.CallType.HasFlag(eCallType.Video));

			if (inAudioCall)
				output |= eVolumePointContext.Atc;

			if (inVideoCall)
				output |= eVolumePointContext.Vtc;

			return output;
		}

		#endregion

		#region WakeSchedule Callbacks

		/// <summary>
		/// Subscribe to the schedule events.
		/// </summary>
		/// <param name="schedule"></param>
		protected virtual void Subscribe(WakeSchedule schedule)
		{
			if (schedule == null)
				return;

			schedule.OnWakeActionRequested += ScheduleOnWakeActionRequested;
			schedule.OnSleepActionRequested += ScheduleOnSleepActionRequested;
		}

		/// <summary>
		/// Unsubscribe from the schedule events.
		/// </summary>
		/// <param name="schedule"></param>
		protected virtual void Unsubscribe(WakeSchedule schedule)
		{
			if (schedule == null)
				return;

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

			Logger.Log(eSeverity.Informational, "Performing scheduled Sleep");
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

			Logger.Log(eSeverity.Informational, "Performing scheduled Wake");
			Wake();
		}

		#endregion

		#region ConferenceManager Callbacks

		/// <summary>
		/// Subscribe to the conference manager events.
		/// </summary>
		/// <param name="conferenceManager"></param>
		protected virtual void Subscribe(IConferenceManager conferenceManager)
		{
		}

		/// <summary>
		/// Unsubscribe from the conference manager events.
		/// </summary>
		/// <param name="conferenceManager"></param>
		protected virtual void Unsubscribe(IConferenceManager conferenceManager)
		{
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

			SeatCount = settings.SeatCount;

			// Wake Schedule
			if (m_WakeSchedule != null)
				m_WakeSchedule.Copy(settings.WakeSchedule);

			// Dialing plan
			SetDialingPlan(settings.DialingPlan);
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			SeatCount = 0;
			DialingPlan = null;

			if (m_WakeSchedule != null)
				m_WakeSchedule.Clear();

			if (m_ConferenceManager != null)
				m_ConferenceManager.Clear();
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.SeatCount = SeatCount;

			if (m_WakeSchedule != null)
				settings.WakeSchedule.Copy(m_WakeSchedule);

			settings.DialingPlan = DialingPlan;
		}

		/// <summary>
		/// Sets the dialing plan from the settings.
		/// </summary>
		/// <param name="path"></param>
		private void SetDialingPlan(string path)
		{
			if (m_ConferenceManager == null)
				throw new InvalidOperationException("Room has no conference manager");

			DialingPlan = path;

			if (!string.IsNullOrEmpty(path))
				path = PathUtils.GetDefaultConfigPath("DialingPlans", path);

			try
			{
				if (string.IsNullOrEmpty(path))
					Logger.Log(eSeverity.Warning, "No Dialing Plan configured");
				else
				{
					string xml = IcdFile.ReadToEnd(path, new UTF8Encoding(false));
					xml = EncodingUtils.StripUtf8Bom(xml);

					m_ConferenceManager.DialingPlan.LoadMatchersFromXml(xml);
				}
			}
			catch (Exception e)
			{
				Logger.Log(eSeverity.Error, "failed to load Dialing Plan {0} - {1}", path, e.Message);
			}

			// Add the dialing endpoints to the conference manager
			foreach (IConferencePoint conferencePoint in Originators.GetInstancesRecursive<IConferencePoint>())
				m_ConferenceManager.Dialers.RegisterDialingProvider(conferencePoint);

			// Add the volume points to the conference manager
			foreach (IVolumePoint volumePoint in Originators.GetInstancesRecursive<IVolumePoint>())
				m_ConferenceManager.VolumePoints.RegisterVolumePoint(volumePoint);
		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("IsAwake", IsAwake);
			addRow("Seat Count", SeatCount);
		}

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
