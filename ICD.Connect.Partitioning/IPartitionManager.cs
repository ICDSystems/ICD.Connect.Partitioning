using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning
{
	public interface IPartitionManager : IOriginator
	{
		IOriginatorCollection<IPartition> Partitions { get; }
	}
}
