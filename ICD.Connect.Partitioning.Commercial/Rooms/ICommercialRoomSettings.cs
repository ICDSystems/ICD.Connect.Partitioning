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
		/// Gets the WakeSchedule settings.
		/// </summary>
		WakeSchedule WakeSchedule { get; }

		/// <summary>
		/// Gets/sets the dialing plan path.
		/// </summary>
		string DialingPlan { get; set; }

		/// <summary>
		/// Gets/sets the seat count.
		/// </summary>
		int SeatCount { get; set; }
	}
}
