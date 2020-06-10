using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Calendaring.CalendarPoints;
using ICD.Connect.Calendaring.Controls;
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
		/// Raised when the wake schedule changes.
		/// </summary>
		event EventHandler<GenericEventArgs<WakeSchedule>> OnWakeScheduleChanged;

		/// <summary>
		/// Raised when the room wakes or goes to sleep.
		/// </summary>
		event EventHandler<BoolEventArgs> OnIsAwakeStateChanged;

		/// <summary>
		/// Raised when the room becomed occupied or vacated.
		/// </summary>
		[EventTelemetry(CommercialRoomTelemetryNames.OCCUPIED_CHANGED)]
		event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupiedChanged;

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
		/// Gets the path to the loaded dialing plan xml file. Used by fusion :(
		/// </summary>
		string DialingPlan { get; }

		/// <summary>
		/// Gets the conference manager
		/// </summary>
		[CanBeNull]
		IConferenceManager ConferenceManager { get; }

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

		#region Methods

		/// <summary>
		/// Shuts down the room.
		/// </summary>
		void Sleep();

		/// <summary>
		/// Wakes up the room.
		/// </summary>
		void Wake();

		#endregion
	}

	public static class CommercialRoomExtensions
	{
		/// <summary>
		/// Gets the calendar controls from the calendar points.
		/// </summary>
		/// <param name="commercialRoom"></param>
		/// <returns></returns>
		[NotNull]
		public static IEnumerable<ICalendarControl> GetCalendarControls([NotNull] this ICommercialRoom commercialRoom)
		{
			if (commercialRoom == null)
				throw new ArgumentNullException("commercialRoom");

			return commercialRoom.Originators
			                     .GetInstancesRecursive<ICalendarPoint>()
			                     .Select(p => p.Control)
			                     .Where(c => c != null);
		}
	}
}
