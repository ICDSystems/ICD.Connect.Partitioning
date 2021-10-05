using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;

namespace ICD.Connect.Partitioning.Commercial.Devices.Occupancy
{
	public interface IOccupancySensorDevice: IDevice
	{

		event EventHandler<GenericEventArgs<eOccupancyFeatures>> OnSupportedFeaturesChanged;

		event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupancyStateChanged;

		event EventHandler<IntEventArgs> OnPeopleCountChanged;

		eOccupancyFeatures SupportedFeatures { get; }

		eOccupancyState OccupancyState { get; }

		int PeopleCount { get; }
	}
}
