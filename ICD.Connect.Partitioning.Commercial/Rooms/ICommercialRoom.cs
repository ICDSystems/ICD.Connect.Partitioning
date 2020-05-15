using System;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Calendaring.Controls;
using ICD.Connect.Conferencing.ConferenceManagers;
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
		/// Raised when the wake schedule changes.
		/// </summary>
		event EventHandler<GenericEventArgs<WakeSchedule>> OnWakeScheduleChanged;

		/// <summary>
		/// Raised when the room wakes or goes to sleep.
		/// </summary>
		event EventHandler<BoolEventArgs> OnIsAwakeStateChanged;

		/// <summary>
		/// Raised when the calendar control changes.
		/// </summary>
		event EventHandler<GenericEventArgs<ICalendarControl>> OnCalendarControlChanged; 

		#region Properties

		/// <summary>
		/// Gets the wake/sleep schedule.
		/// </summary>
		[CanBeNull]
		WakeSchedule WakeSchedule { get; }

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
		/// Gets the CalendarControl for the room.
		/// </summary>
		[CanBeNull]
		ICalendarControl CalendarControl { get; }

		/// <summary>
		/// Gets the awake state.
		/// </summary>
		bool IsAwake { get; }

		/// <summary>
		/// Gets the number of seats for this room.
		/// </summary>
		[PropertyTelemetry(CommercialRoomTelemetryNames.SEAT_COUNT, null, null)]
		int SeatCount { get; }

		#endregion
	}
}
