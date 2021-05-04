using System.Collections.Generic;
using ICD.Common.Logging.Activities;
using ICD.Common.Utils.Services.Logging;

namespace ICD.Connect.Partitioning.Commercial.Controls.Occupancy
{
	public static class OccupancySensorControlActivities
	{
		private static readonly Dictionary<eOccupancyState, Activity> s_OccupancytateActivities
			= new Dictionary<eOccupancyState, Activity>
			{
				{eOccupancyState.Unknown, new Activity(Activity.ePriority.Low, "OccupancyState", "Unknown Occupancy", eSeverity.Informational) },
				{eOccupancyState.Unoccupied, new Activity(Activity.ePriority.Medium, "OccupancyState", "Unoccupied", eSeverity.Informational) },
				{eOccupancyState.Occupied, new Activity(Activity.ePriority.High, "OccupancyState", "Occupied", eSeverity.Informational) },
			};

		public static Activity GetOccupancyStateActivity(eOccupancyState occupancyState)
		{
			return s_OccupancytateActivities[occupancyState];
		}

		public static Activity GetPeopleCountActivity(int peopleCount)
		{
			return new Activity(Activity.ePriority.Low, "PeopleCount", string.Format("People Count Set to {0}", peopleCount), eSeverity.Informational);
		}
	}
}