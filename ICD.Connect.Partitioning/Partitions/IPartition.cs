using System;
using System.Collections.Generic;
using System.Linq;
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
		/// Gets the number of rooms the partition is adjacent to.
		/// </summary>
		int RoomsCount { get; }

		/// <summary>
		/// Returns true if the given room has been added as adjacent to this partition.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		bool ContainsRoom(int roomId);

		/// <summary>
		/// Gets the controls that are associated with this partition.
		/// </summary>
		/// <returns></returns>
		IEnumerable<DeviceControlInfo> GetPartitionControls();
		
		/// <summary>
		/// Returns the rooms that are added as adjacent to this partition.
		/// </summary>
		/// <returns></returns>
		IEnumerable<int> GetRooms();
	}

	public static class PartitionExtensions
	{
		/// <summary>
		/// Returns true if the other partition shares a common room.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static bool IsAdjacent(this IPartition extends, IPartition other)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (other == null)
				throw new ArgumentNullException("other");

			return extends.GetRooms().Any(other.ContainsRoom);
		}
	}
}
