using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Partitioning.Devices
{
	[KrangSettings("MockPartition", typeof(MockPartitionDevice))]
	public sealed class MockPartitionDeviceSettings : AbstractPartitionDeviceSettings
	{
	}
}
