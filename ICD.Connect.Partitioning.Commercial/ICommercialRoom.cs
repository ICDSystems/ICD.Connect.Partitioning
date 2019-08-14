using ICD.Connect.Partitioning.Rooms;

namespace ICD.Connect.Partitioning.Commercial
{
	public interface ICommercialRoom : IRoom
	{
		/// <summary>
		/// Gets the wake/sleep schedule.
		/// </summary>
		WakeSchedule WakeSchedule { get; }

		/// <summary>
		/// Gets the path to the loaded dialing plan xml file. Used by fusion :(
		/// </summary>
		DialingPlanInfo DialingPlan { get; }
	}
}