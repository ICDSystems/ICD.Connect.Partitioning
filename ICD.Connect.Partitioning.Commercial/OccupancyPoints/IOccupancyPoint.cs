using System;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.Devices.Points;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;

namespace ICD.Connect.Partitioning.Commercial.OccupancyPoints
{
	public interface IOccupancyPoint : IPoint<IOccupancySensorControl>
	{
		/// <summary>
		/// Masks supported features on the occupancy control
		/// Implementers should take the intersection of SupportedFeatures on the occupancy control and this mask
		/// </summary>
		eOccupancyFeatures SupportedFeaturesMask { get; }
	}

	public static class OccupancyPointExtensions
	{
		public static eOccupancyFeatures GetMaskedOccupancyFeatures([NotNull] this IOccupancyPoint extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (extends.Control == null)
				return eOccupancyFeatures.None;

			return EnumUtils.GetFlagsIntersection(extends.Control.SupportedFeatures, extends.SupportedFeaturesMask);
		}
	}
}