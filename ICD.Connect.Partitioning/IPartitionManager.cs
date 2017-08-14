using ICD.Connect.Settings;

namespace ICD.Connect.Partitions
{
	public interface IPartitionManager : IOriginator
	{
		IOriginatorCollection<IPartition> Partitions { get; }
	}
}
