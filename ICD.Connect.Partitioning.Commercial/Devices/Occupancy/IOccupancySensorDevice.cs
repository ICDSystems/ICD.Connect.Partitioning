using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;

namespace ICD.Connect.Partitioning.Commercial.Devices.Occupancy
{
	public interface IOccupancySensorDevice: IDevice
	{
		event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupancyStateChanged;

		eOccupancyState OccupancyState { get; }
	}
}
