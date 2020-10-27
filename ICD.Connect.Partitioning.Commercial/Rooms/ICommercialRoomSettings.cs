using System.Collections.Generic;
using ICD.Connect.Partitioning.Rooms;

namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	public interface ICommercialRoomSettings : IRoomSettings
	{
		/// <summary>
		/// Gets the conference point settings.
		/// </summary>
		Dictionary<int, eCombineMode> ConferencePoints { get; }

		/// <summary>
		/// Gets the Calendar point settings.
		/// </summary>
		Dictionary<int, eCombineMode> CalendarPoints { get; }

		/// <summary>
		/// Gets the Occupancy point settings.
		/// </summary>
		Dictionary<int, eCombineMode> OccupancyPoints { get; }

		/// <summary>
		/// Gets the WakeSchedule settings.
		/// </summary>
		WakeSchedule WakeSchedule { get; }

		/// <summary>
		/// Gets the Touch Free settings.
		/// </summary>
		TouchFreeSettings TouchFree{ get; }

		/// <summary>
		/// Gets the operational hours settings.
		/// </summary>
		OperationalHoursSettings OperationalHours { get; }

		/// <summary>
		/// Gets/sets the dialing plan path.
		/// </summary>
		string DialingPlan { get; set; }

		/// <summary>
		/// Gets/sets the seat count.
		/// </summary>
		int SeatCount { get; set; }

		/// <summary>
		/// Gets/sets the room type.
		/// </summary>
		string RoomType { get; set; }
	}
}
