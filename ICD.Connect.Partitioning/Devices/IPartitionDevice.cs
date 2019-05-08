using ICD.Connect.Devices;
using ICD.Connect.Partitioning.Controls;

namespace ICD.Connect.Partitioning.Devices
{
	/// <summary>
	/// IPartitionDevice simply notifies if a partition has been opened.
	/// </summary>
	public interface IPartitionDevice : IDevice, IPartitionControlBase
	{
	}
}
