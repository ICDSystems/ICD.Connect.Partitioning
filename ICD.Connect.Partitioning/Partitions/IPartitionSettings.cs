using System.Collections.Generic;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.Partitions
{
	public interface IPartitionSettings : ISettings
	{
		/// <summary>
		/// Gets/sets the optional device for the partition.
		/// </summary>
		int Device { get; set; }

		/// <summary>
		/// Gets/sets the optional device control for the partition.
		/// </summary>
		int Control { get; set; }

		/// <summary>
		/// Sets the rooms that are adjacent to the partition.
		/// </summary>
		/// <param name="roomIds"></param>
		void SetRooms(IEnumerable<int> roomIds);

		/// <summary>
		/// Returns the rooms that are added as adjacent to the partition.
		/// </summary>
		/// <returns></returns>
		IEnumerable<int> GetRooms();
	}
}
