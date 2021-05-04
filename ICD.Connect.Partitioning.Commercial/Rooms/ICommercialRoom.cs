using System;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Calendaring.CalendarManagers;
using ICD.Connect.Conferencing.ConferenceManagers;
using ICD.Connect.Partitioning.Commercial.CalendarOccupancyManagers;
using ICD.Connect.Partitioning.Commercial.CallRatings;
using ICD.Connect.Partitioning.Commercial.OccupancyManagers;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Telemetry.Attributes;

namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	[ExternalTelemetry("Commercial Room Telemetry", typeof(CommercialRoomExternalTelemetryProvider))]
	public interface ICommercialRoom : IRoom
	{
		#region Events

		/// <summary>
		/// Raised when the conference manager changes.
		/// </summary>
		event EventHandler<GenericEventArgs<IConferenceManager>> OnConferenceManagerChanged;

		/// <summary>
		/// Raised when the calendar manager changes.
		/// </summary>
		event EventHandler<GenericEventArgs<ICalendarManager>> OnCalendarManagerChanged;

		/// <summary>
		/// Raised when the occupancy manager changes
		/// </summary>
		event EventHandler<GenericEventArgs<IOccupancyManager>> OnOccupancyManagerChanged;

		/// <summary>
		/// Raised when the wake schedule changes.
		/// </summary>
		event EventHandler<GenericEventArgs<WakeSchedule>> OnWakeScheduleChanged;

		/// <summary>
		/// Raised when the Touch Free changes.
		/// </summary>
		event EventHandler<GenericEventArgs<TouchFree>> OnTouchFreeChanged;

		/// <summary>
		/// Raised when the room Call Rating Manager changes.
		/// </summary>
		event EventHandler<GenericEventArgs<CallRatingManager>> OnCallRatingManagerChanged;

		/// <summary>
		/// Raised when Touch Free becomes enabled/disabled.
		/// </summary>
		event EventHandler<BoolEventArgs> OnTouchFreeEnabledChanged;

		/// <summary>
		/// Raised when the room wakes or goes to sleep.
		/// </summary>
		[EventTelemetry(CommercialRoomTelemetryNames.IS_AWAKE_CHANGED)]
		event EventHandler<BoolEventArgs> OnIsAwakeStateChanged;

		/// <summary>
		/// Raised when the room type changes.
		/// </summary>
		[EventTelemetry(CommercialRoomTelemetryNames.ROOM_TYPE_CHANGED)]
		event EventHandler<StringEventArgs> OnRoomTypeChanged;

		/// <summary>
		/// Raised when the room starts/stops a meeting.
		/// </summary>
		[EventTelemetry(CommercialRoomTelemetryNames.IS_IN_MEETING_CHANGED)]
		event EventHandler<BoolEventArgs> OnIsInMeetingChanged;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the wake/sleep schedule.
		/// </summary>
		[CanBeNull]
		WakeSchedule WakeSchedule { get; }

		/// <summary>
		/// Gets the Touch Free.
		/// </summary>
		[CanBeNull]
		TouchFree TouchFree { get; }

		/// <summary>
		/// Gets the Call Rating Manager.
		/// </summary>
		[CanBeNull]
		[NodeTelemetry("CallRatingManager")]
		CallRatingManager CallRatingManager { get; }

		/// <summary>
		/// Returns true if TouchFree is not null and enabled.
		/// </summary>
		bool TouchFreeEnabled { get; }

		/// <summary>
		/// Gets the Operational Hours of the Room
		/// </summary>
		[NotNull]
		[NodeTelemetry("OperationalHours")]
		OperationalHours OperationalHours { get; }

		/// <summary>
		/// Gets the path to the loaded dialing plan xml file. Used by fusion :(
		/// </summary>
		string DialingPlan { get; }

		/// <summary>
		/// Gets the conference manager
		/// </summary>
		[CanBeNull]
		IConferenceManager ConferenceManager { get; }

		/// <summary>
		/// Gets the calendar manager
		/// </summary>
		[CanBeNull]
		ICalendarManager CalendarManager { get; }

		/// <summary>
		/// Gets the occupancy manager
		/// </summary>
		[CanBeNull]
		IOccupancyManager OccupancyManager { get; }

		/// <summary>
		/// Gets the calendar occupancy manager
		/// </summary>
		[NotNull]
		ICalendarOccupancyManager CalendarOccupancyManager { get; }

		/// <summary>
		/// Gets the awake state.
		/// </summary>
		[PropertyTelemetry(CommercialRoomTelemetryNames.IS_AWAKE, null, CommercialRoomTelemetryNames.IS_AWAKE_CHANGED)]
		bool IsAwake { get; }

		/// <summary>
		/// Gets the number of seats for this room.
		/// </summary>
		[PropertyTelemetry(CommercialRoomTelemetryNames.SEAT_COUNT, null, null)]
		int SeatCount { get; }

		/// <summary>
		/// Gets the type of room (eg: Huddle, Presentation, etc)
		/// This can be varying values for different implementations/customers
		/// </summary>
		[PropertyTelemetry(CommercialRoomTelemetryNames.ROOM_TYPE, null, CommercialRoomTelemetryNames.ROOM_TYPE_CHANGED)]
		string RoomType { get; }

		/// <summary>
		/// Gets/sets the current meeting status.
		/// </summary>
		[PropertyTelemetry(CommercialRoomTelemetryNames.IS_IN_MEETING, null, CommercialRoomTelemetryNames.IS_IN_MEETING_CHANGED)]
		bool IsInMeeting { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Shuts down the room.
		/// </summary>
		[MethodTelemetry(CommercialRoomTelemetryNames.SLEEP_COMMAND)]
		void Sleep();

		/// <summary>
		/// Wakes up the room.
		/// </summary>
		void Wake();

		#endregion
	}
}
