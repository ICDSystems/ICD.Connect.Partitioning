using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICD.Common.Logging.LoggingContexts;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Services.Scheduler;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Audio.VolumePoints;
using ICD.Connect.Calendaring.CalendarManagers;
using ICD.Connect.Calendaring.CalendarPoints;
using ICD.Connect.Conferencing.ConferenceManagers;
using ICD.Connect.Conferencing.ConferencePoints;
using ICD.Connect.Conferencing.Conferences;
using ICD.Connect.Conferencing.EventArguments;
using ICD.Connect.Conferencing.Participants;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;
using ICD.Connect.Partitioning.Commercial.OccupancyPoints;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Utils;

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
		/// Raised when the calendar manager changes.
		/// </summary>
		public event EventHandler<GenericEventArgs<ICalendarManager>> OnCalendarManagerChanged;

		/// <summary>
		/// Raised when the wake schedule changes.
		/// </summary>
		public event EventHandler<GenericEventArgs<WakeSchedule>> OnWakeScheduleChanged;

		/// <summary>
		/// Raised when Touch Free settings changes.
		/// </summary>
		public event EventHandler<GenericEventArgs<TouchFree>> OnTouchFreeChanged;

		/// <summary>
		/// Raised when Touch Free becomes enabled/disabled.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnTouchFreeEnabledChanged;

		/// <summary>
		/// Raised when the room wakes or goes to sleep.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnIsAwakeStateChanged;

		/// <summary>
		/// Raised when the room becomed occupied or vacated.
		/// </summary>
		public event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupiedChanged; 

		private readonly IcdHashSet<IOccupancySensorControl> m_OccupancyControls;
		private readonly SafeCriticalSection m_OccupancyControlsSection;
		[CanBeNull] private IConferenceManager m_ConferenceManager;
		[CanBeNull] private ICalendarManager m_CalendarManager;
		[CanBeNull] private WakeSchedule m_WakeSchedule;
		[CanBeNull] private TouchFree m_TouchFree;

		private eOccupancyState m_OccupancyState;
		private bool m_IsAwake;
		private bool m_TouchFreeEnabled;

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
		/// Gets the Touch Free Settings.
		/// </summary>
		[CanBeNull]
		public TouchFree TouchFree
		{
			get { return m_TouchFree; }
			protected set
			{
				if (value == m_TouchFree)
					return;

				Unsubscribe(m_TouchFree);
				m_TouchFree = value;
				Subscribe(m_TouchFree);

				UpdateTouchFreeEnabled();

				OnTouchFreeChanged.Raise(this, new GenericEventArgs<TouchFree>(m_TouchFree));
			}
		}

		/// <summary>
		/// Returns true if TouchFree is not null and enabled.
		/// </summary>
		public bool TouchFreeEnabled
		{
			get { return m_TouchFreeEnabled; }
			private set
			{
				if (value == m_TouchFreeEnabled)
					return;

				m_TouchFreeEnabled = value;

				OnTouchFreeEnabledChanged.Raise(this, new BoolEventArgs(m_TouchFreeEnabled));
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

				UpdateVolumeContext();
				UpdateInCall();

				OnConferenceManagerChanged.Raise(this, new GenericEventArgs<IConferenceManager>(m_ConferenceManager));
			}
		}

		/// <summary>
		/// Gets the calendar manager
		/// </summary>
		[CanBeNull]
		public ICalendarManager CalendarManager
		{
			get { return m_CalendarManager; }
			protected set
			{
				if (value == m_CalendarManager)
					return;

				Unsubscribe(m_CalendarManager);
				m_CalendarManager = value;
				Subscribe(m_CalendarManager);

				OnCalendarManagerChanged.Raise(this, new GenericEventArgs<ICalendarManager>(m_CalendarManager));
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

				Logger.LogSetTo(eSeverity.Informational, "IsAwake", m_IsAwake);

				OnIsAwakeStateChanged.Raise(this, new BoolEventArgs(m_IsAwake));
			}
		}

		/// <summary>
		/// Gets the number of seats for this room.
		/// </summary>
		public int SeatCount { get; private set; }

		/// <summary>
		/// Gets the current occupancy state for the room.
		/// </summary>
		public eOccupancyState Occupied
		{
			get { return m_OccupancyState; }
			private set
			{
				if (value == m_OccupancyState)
					return;

				m_OccupancyState = value;
				OccupiedTime = IcdEnvironment.GetUtcTime();

				Logger.LogSetTo(eSeverity.Informational, "Occupied", m_OccupancyState);

				HandleOccupiedChanged(m_OccupancyState);

				OnOccupiedChanged.Raise(this, new GenericEventArgs<eOccupancyState>(m_OccupancyState));
			}
		}

		/// <summary>
		/// Gets the time, in UTC, that the occupancy state last changed.
		/// </summary>
		public DateTime OccupiedTime { get; private set; }

		#endregion

		/// <summary>
		/// Release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnConferenceManagerChanged = null;
			OnWakeScheduleChanged = null;
			OnTouchFreeChanged = null;
			OnIsAwakeStateChanged = null;
			OnOccupiedChanged = null;

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

		#endregion

		#region Private Methods

		/// <summary>
		/// Called when an originator is added to/removed from the room.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void OriginatorsOnChildrenChanged(object sender, EventArgs args)
		{
			base.OriginatorsOnChildrenChanged(sender, args);

			SubscribeOccupancyControls();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractCommercialRoom()
		{
			m_OccupancyControls = new IcdHashSet<IOccupancySensorControl>();
			m_OccupancyControlsSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Returns true if a source is actively routed to a display or we are in a conference.
		/// </summary>
		/// <returns></returns>
		protected abstract bool GetIsInActiveMeeting();

		/// <summary>
		/// Override to handle the room becoming occupied or vacated.
		/// </summary>
		/// <param name="occupancyState"></param>
		protected virtual void HandleOccupiedChanged(eOccupancyState occupancyState)
		{
		}

		/// <summary>
		/// Updates the current volume context.
		/// </summary>
		private void UpdateVolumeContext()
		{
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
				VolumeContext |= eVolumePointContext.Atc;
			else
				VolumeContext &= ~eVolumePointContext.Atc;

			if (inVideoCall)
				VolumeContext |= eVolumePointContext.Vtc;
			else
				VolumeContext &= ~eVolumePointContext.Vtc;
		}

		#endregion

		#region WakeSchedule Callbacks

		/// <summary>
		/// Subscribe to the schedule events.
		/// </summary>
		/// <param name="schedule"></param>
		private void Subscribe(WakeSchedule schedule)
		{
			if (schedule == null)
				return;

			schedule.WakeActionRequested = ScheduleOnWakeActionRequested;
			schedule.SleepActionRequested = ScheduleOnSleepActionRequested;
		}

		/// <summary>
		/// Unsubscribe from the schedule events.
		/// </summary>
		/// <param name="schedule"></param>
		private void Unsubscribe(WakeSchedule schedule)
		{
			if (schedule == null)
				return;

			schedule.WakeActionRequested = null;
			schedule.SleepActionRequested = null;
		}

		/// <summary>
		/// Called when a sleep action is scheduled.
		/// </summary>
		private TimeSpan? ScheduleOnSleepActionRequested()
		{
			if (CombineState)
				return null;

			if (GetIsInActiveMeeting())
				return null;

			// Defer if there is an upcoming booking today
			DateTime now = IcdEnvironment.GetUtcTime();
			DateTime endOfDay = IcdEnvironment.GetLocalTime().EndOfDay().ToUniversalTime();
            DateTime endTime;

			bool hasBookings = Originators.GetInstancesRecursive<ICalendarPoint>()
			                              .Select(p => p.Control)
			                              .Where(c => c != null)
			                              .SelectMany(c => c.GetBookings())
			                              .Select(b => b.EndTime)
			                              .Where(d => d > now && d < endOfDay)
			                              .Order()
			                              .TryLast(out endTime);

			if (hasBookings)
			{
				TimeSpan deferredTime = endTime - now;

				string message = string.Format("Sleep time has been deferred {0}, due to a meeting", deferredTime.ToReadableString());
				Logger.Log(eSeverity.Informational, message);

				return deferredTime;
			}

			Logger.Log(eSeverity.Informational, "Performing scheduled Sleep");
			Sleep();

			return null;
		}

		/// <summary>
		/// Called when a wake action is scheduled.
		/// </summary>
		private TimeSpan? ScheduleOnWakeActionRequested()
		{
			if (CombineState)
				return null;

			if (GetIsInActiveMeeting())
				return null;

			Logger.Log(eSeverity.Informational, "Performing scheduled Wake");
			Wake();

			return null;
		}

		#endregion

		#region TouchFree Callbacks

		/// <summary>
		/// Subscribe to the TouchFree events.
		/// </summary>
		/// <param name="touchFree"></param>
		private void Subscribe(TouchFree touchFree)
		{
			if (touchFree == null)
				return;

			touchFree.OnEnabledChanged += TouchFreeOnEnabledChanged;
		}

		/// <summary>
		/// Unsubscribe from the TouchFree events.
		/// </summary>
		/// <param name="touchFree"></param>
		private void Unsubscribe(TouchFree touchFree)
		{
			if (touchFree == null)
				return;

			touchFree.OnEnabledChanged -= TouchFreeOnEnabledChanged;
		}

		/// <summary>
		/// Called when the TouchFree becomes enabled/disabled.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="boolEventArgs"></param>
		protected virtual void TouchFreeOnEnabledChanged(object sender, BoolEventArgs boolEventArgs)
		{
			UpdateTouchFreeEnabled();
		}

		/// <summary>
		/// Updates the TouchFreeEnabled property.
		/// </summary>
		private void UpdateTouchFreeEnabled()
		{
			TouchFreeEnabled = TouchFree != null && TouchFree.Enabled;
		}

		#endregion

		#region ConferenceManager Callbacks

		/// <summary>
		/// Subscribe to the conference manager events.
		/// </summary>
		/// <param name="conferenceManager"></param>
		protected virtual void Subscribe(IConferenceManager conferenceManager)
		{
			if (conferenceManager == null)
				return;

			conferenceManager.Dialers.OnInCallChanged += DialersOnInCallChanged;
		}

		/// <summary>
		/// Unsubscribe from the conference manager events.
		/// </summary>
		/// <param name="conferenceManager"></param>
		protected virtual void Unsubscribe(IConferenceManager conferenceManager)
		{
			if (conferenceManager == null)
				return;

			conferenceManager.Dialers.OnInCallChanged -= DialersOnInCallChanged;
		}

		/// <summary>
		/// Called when the in call state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="inCallEventArgs"></param>
		private void DialersOnInCallChanged(object sender, InCallEventArgs inCallEventArgs)
		{
			UpdateVolumeContext();
			UpdateInCall();
		}

		/// <summary>
		/// Updates the in call activity.
		/// </summary>
		private void UpdateInCall()
		{
			eInCall inCall = ConferenceManager == null ? eInCall.None : ConferenceManager.Dialers.IsInCall;
			Activities.LogActivity(CommercialRoomActivities.GetCallActivity(inCall));
		}

		#endregion

		#region CalendarManager Callbacks

		/// <summary>
		/// Subscribe to the calendar manager events.
		/// </summary>
		/// <param name="calendarManager"></param>
		protected virtual void Subscribe(ICalendarManager calendarManager)
		{
			if (calendarManager == null)
				return;

			calendarManager.OnBookingsChanged += CalendarManagerOnBookingsChanged;
		}

		/// <summary>
		/// Unsubscribe from the calendar manager events.
		/// </summary>
		/// <param name="calendarManager"></param>
		protected virtual void Unsubscribe(ICalendarManager calendarManager)
		{
			if (calendarManager == null)
				return;

			calendarManager.OnBookingsChanged -= CalendarManagerOnBookingsChanged;
		}

		/// <summary>
		/// Called when the calendar bookings change.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		protected virtual void CalendarManagerOnBookingsChanged(object sender, EventArgs eventArgs)
		{
		}

		#endregion

		#region Occupancy Control Callbacks

		private void SubscribeOccupancyControls()
		{
			m_OccupancyControlsSection.Enter();

			try
			{
				foreach (IOccupancySensorControl control in m_OccupancyControls)
					Unsubscribe(control);

				IEnumerable<IOccupancySensorControl> controls =
					Originators.GetInstancesRecursive<IOccupancyPoint>()
					           .Select(p => p.Control)
					           .Where(c => c != null);

				m_OccupancyControls.Clear();
				m_OccupancyControls.AddRange(controls);

				foreach (IOccupancySensorControl control in m_OccupancyControls)
					Subscribe(control);
			}
			finally
			{
				m_OccupancyControlsSection.Leave();
			}

			UpdateOccupancy();
		}

		private void Subscribe(IOccupancySensorControl control)
		{
			if (control == null)
				return;

			control.OnOccupancyStateChanged += ControlOnOccupancyStateChanged;
		}

		private void Unsubscribe(IOccupancySensorControl control)
		{
			if (control == null)
				return;

			control.OnOccupancyStateChanged -= ControlOnOccupancyStateChanged;
		}

		private void ControlOnOccupancyStateChanged(object sender, GenericEventArgs<eOccupancyState> genericEventArgs)
		{
			UpdateOccupancy();
		}

		private void UpdateOccupancy()
		{
			Occupied = Originators.GetInstancesRecursive<IOccupancyPoint>()
			                      .Select(p => p.Control)
			                      .Where(c => c != null)
			                      .Select(c => c.OccupancyState)
			                      .MaxOrDefault();
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

			// Touch Free
			if (m_TouchFree != null)
				m_TouchFree.ApplySettings(settings.TouchFree, factory);

			AddOriginatorsSkipExceptions<IConferencePoint>(settings.ConferencePoints, factory);
			AddOriginatorsSkipExceptions<ICalendarPoint>(settings.CalendarPoints, factory);
			AddOriginatorsSkipExceptions<IOccupancyPoint>(settings.OccupancyPoints, factory);

			// Dialing plan
			SetDialingPlan(settings.DialingPlan);

			// Calendars
			RegisterCalendarPoints();

			// Generate occupancy points
			GenerateOccupancyPoints(factory);
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

			if (m_TouchFree != null)
				m_TouchFree.ClearSettings();

			if (m_ConferenceManager != null)
				m_ConferenceManager.Clear();

			if (m_CalendarManager != null)
				m_CalendarManager.Clear();
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

			if (m_TouchFree != null)
				m_TouchFree.CopySettings(settings.TouchFree);

			settings.DialingPlan = DialingPlan;

			settings.ConferencePoints.Clear();
			settings.CalendarPoints.Clear();
			settings.OccupancyPoints.Clear();

			settings.ConferencePoints.AddRange(GetSerializableChildren<IConferencePoint>());
			settings.CalendarPoints.AddRange(GetSerializableChildren<ICalendarPoint>());
			settings.OccupancyPoints.AddRange(GetSerializableChildren<IOccupancyPoint>());
		}

		/// <summary>
		/// Registers the calendar points with the calendar manager.
		/// </summary>
		private void RegisterCalendarPoints()
		{
			if (m_CalendarManager == null)
				throw new InvalidOperationException("Room has no calendar manager");

			// Add the calendar endpoints to the calendar manager
			foreach (ICalendarPoint calendarPoint in Originators.GetInstancesRecursive<ICalendarPoint>())
				m_CalendarManager.RegisterCalendarProvider(calendarPoint);
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
			{
				if (conferencePoint.Control == null)
					Logger.Log(eSeverity.Error, "Failed to register conference point with no control - {0}", conferencePoint);
				else
					m_ConferenceManager.Dialers.RegisterDialingProvider(conferencePoint);
			}

			// Add the volume points to the conference manager
			foreach (IVolumePoint volumePoint in Originators.GetInstancesRecursive<IVolumePoint>())
			{
				if (volumePoint.Control == null)
					Logger.Log(eSeverity.Error, "Failed to register volume point with no control - {0}", volumePoint);
				else
					m_ConferenceManager.VolumePoints.RegisterVolumePoint(volumePoint);
			}
		}

		/// <summary>
		/// Generates occupancy points for the occupancy controls in the room.
		/// </summary>
		/// <param name="factory"></param>
		private void GenerateOccupancyPoints(IDeviceFactory factory)
		{
			foreach (IOccupancySensorControl control in this.GetControls<IOccupancySensorControl>())
			{
				int id = IdUtils.GetNewId(Core.Originators.GetChildrenIds().Concat(factory.GetOriginatorIds()), eSubsystem.OccupancyPoints);
				eCombineMode combineMode = Originators.GetCombineMode(control.Parent.Id);

				OccupancyPoint point = new OccupancyPoint
				{
					Id = id,
					Uuid = OriginatorUtils.GenerateUuid(Core, id),
					Name = control.Name
				};
				point.SetControl(control);

				Core.Originators.AddChild(point);
				Originators.Add(id, combineMode);
			}
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

			CommercialRoomConsole.BuildConsoleStatus(this, addRow);
		}

		/// <summary>
		/// Gets the child console node groups.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase node in GetBaseConsoleNodes())
				yield return node;

			foreach (IConsoleNodeBase node in CommercialRoomConsole.GetConsoleNodes(this))
				yield return node;
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			foreach (IConsoleCommand command in CommercialRoomConsole.GetConsoleCommands(this))
				yield return command;
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
