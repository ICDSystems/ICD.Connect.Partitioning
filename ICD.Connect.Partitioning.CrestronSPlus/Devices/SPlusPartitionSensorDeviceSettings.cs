using ICD.Connect.Devices.CrestronSPlus.Devices.SPlus;
using ICD.Connect.Partitioning.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Partitioning.CrestronSPlus.Devices
{
	[KrangSettings("SPlusPartitionSensorDevice", typeof(SPlusPartitionSensorDevice))]
	public sealed class SPlusPartitionSensorDeviceSettings : AbstractSPlusDeviceSettings, IPartitionDeviceSettings
	{
	}
}