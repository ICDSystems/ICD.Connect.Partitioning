using ICD.Connect.Partitioning.Rooms;

namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	public interface ICommercialRoomSettings : IRoomSettings
	{
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
