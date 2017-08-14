using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.Partitions
{
	/// <summary>
	/// IPartition describes a division between multiple adjacent rooms.
	/// </summary>
	public interface IPartition : IOriginator
	{
		/// <summary>
		/// Gets/sets the optional control for this partition.
		/// </summary>
		[PublicAPI]
		DeviceControlInfo PartitionControl { get; set; }

		/// <summary>
		/// Adds a room as adjacent to this partition.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		bool AddRoom(int roomId);

		/// <summary>
		/// Removes the room as adjacent to this partition.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		bool RemoveRoom(int roomId);

		/// <summary>
		/// Returns true if the given room has been added as adjacent to this partition.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		bool ContainsRoom(int roomId);

		/// <summary>
		/// Returns the rooms that are added as adjacent to this partition.
		/// </summary>
		/// <returns></returns>
		IEnumerable<int> GetRooms();
	}
}
