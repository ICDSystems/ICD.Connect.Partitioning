using System;

namespace ICD.Connect.Partitioning.Commercial.Controls.Occupancy
{
	[Flags]
	public enum eOccupancyFeatures
	{
		None = 0,
		Occupancy = 1,
		PeopleCounting = 2
	}
}
