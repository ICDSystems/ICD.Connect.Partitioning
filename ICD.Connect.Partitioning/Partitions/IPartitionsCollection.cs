using System.Collections.Generic;

namespace ICD.Connect.Partitioning.Partitions
{
	public interface IPartitionsCollection : IEnumerable<IPartition>
	{
		/// <summary>
		/// Gets all of the partitions.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IPartition> GetPartitions();

		/// <summary>
		/// Clears and sets the partitions.
		/// </summary>
		/// <param name="partitions"></param>
		void SetPartitions(IEnumerable<IPartition> partitions);
	}
}
