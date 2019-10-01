using ICD.Connect.Partitioning.Rooms;

namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	public interface ICommercialRoomSettings : IRoomSettings
	{
		 WakeSchedule WakeSchedule { get; }

		 /// <summary>
		 /// Gets the dialing plan.
		 /// </summary>
		 string DialingPlan { get; set; }
	}
}