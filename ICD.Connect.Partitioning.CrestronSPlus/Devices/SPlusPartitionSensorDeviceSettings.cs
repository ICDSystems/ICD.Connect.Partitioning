using ICD.Connect.Devices.Simpl;
using ICD.Connect.Partitioning.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Partitioning.CrestronSPlus.Devices
{
	[KrangSettings("SPlusPartitionSensorDevice", typeof(SPlusPartitionSensorDevice))]
	public sealed class SPlusPartitionSensorDeviceSettings : AbstractSimplDeviceSettings, IPartitionDeviceSettings
	{
	}
}