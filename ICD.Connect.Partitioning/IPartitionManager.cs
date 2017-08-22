using ICD.Common.Properties;
using ICD.Connect.Partitioning.Controls;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning
{
	public interface IPartitionManager : IOriginator
	{
		IPartitionsCollection Partitions { get; }

		/// <summary>
		/// Gets the control for the given partition.
		/// Returns null if the partition has no control specified.
		/// </summary>
		/// <param name="partition"></param>
		/// <returns></returns>
		[CanBeNull]
		IPartitionDeviceControl GetControl(IPartition partition);
	}
}
