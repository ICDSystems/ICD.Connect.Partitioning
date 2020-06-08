using ICD.Common.Properties;
using ICD.Connect.Devices.Points;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;

namespace ICD.Connect.Partitioning.Commercial.OccupancyPoints
{
	public interface IOccupancyPoint : IPoint
	{
		/// <summary>
		/// Gets the control for this point.
		/// </summary>
		[CanBeNull]
		new IOccupancySensorControl Control { get; }
	}
}