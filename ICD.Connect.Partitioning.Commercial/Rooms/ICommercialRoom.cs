﻿using System;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Calendaring.CalendarManagers;
using ICD.Connect.Conferencing.ConferenceManagers;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Telemetry.Attributes;

namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	[ExternalTelemetry("Commercial Room Telemetry", typeof(CommercialRoomExternalTelemetryProvider))]
	public interface ICommercialRoom : IRoom
	{
		/// <summary>
		/// Raised when the conference manager changes.
		/// </summary>
		event EventHandler<GenericEventArgs<IConferenceManager>> OnConferenceManagerChanged;

		/// <summary>
		/// Raised when the calendar manager changes.
		/// </summary>
		event EventHandler<GenericEventArgs<ICalendarManager>> OnCalendarManagerChanged;

		/// <summary>
		/// Raised when the wake schedule changes.
		/// </summary>
		event EventHandler<GenericEventArgs<WakeSchedule>> OnWakeScheduleChanged;

		/// <summary>
		/// Raised when the Touch Free changes.
		/// </summary>
		event EventHandler<GenericEventArgs<TouchFree>> OnTouchFreeChanged;

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
		/// Raised when the room becomed occupied or vacated.
		/// </summary>
		[EventTelemetry(CommercialRoomTelemetryNames.OCCUPIED_CHANGED)]
		event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupiedChanged;

		/// <summary>
		/// Raised when the room type changes.
		/// </summary>
		[EventTelemetry(CommercialRoomTelemetryNames.ROOM_TYPE_CHANGED)]
		event EventHandler<StringEventArgs> OnRoomTypeChanged;

		#region Properties

		/// <summary>
		/// Gets the Occupancy state.
		/// </summary>
		[PropertyTelemetry(CommercialRoomTelemetryNames.OCCUPIED, null, CommercialRoomTelemetryNames.OCCUPIED_CHANGED)]
		eOccupancyState Occupied { get; }

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
