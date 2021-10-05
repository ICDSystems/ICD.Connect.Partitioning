using ICD.Connect.Devices.Points;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;

namespace ICD.Connect.Partitioning.Commercial.OccupancyPoints
{
	public interface IOccupancyPointSettings : IPointSettings
	{
		eOccupancyFeatures SupportedFeaturesMask { get; set; }
	}
}