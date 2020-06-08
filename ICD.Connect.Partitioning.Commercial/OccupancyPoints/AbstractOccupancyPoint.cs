using ICD.Connect.Devices.Points;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;

namespace ICD.Connect.Partitioning.Commercial.OccupancyPoints
{
	public abstract class AbstractOccupancyPoint<TSettings> : AbstractPoint<TSettings, IOccupancySensorControl>, IOccupancyPoint
		where TSettings : IOccupancyPointSettings, new()
	{
		/// <summary>
		/// Gets the category for this originator type (e.g. Device, Port, etc)
		/// </summary>
		public override string Category { get { return "OccupancyPoint"; } }
	}
}
