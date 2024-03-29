﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Connect.Partitioning.Cells;
using ICD.Connect.Partitioning.Controls;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Partitioning.Partitions
{
	/// <summary>
	/// IPartition describes a division between multiple adjacent rooms.
	/// </summary>
	public interface IPartition : IOriginator
	{
		/// <summary>
		/// Gets/sets the the first cell adjacent to this partition.
		/// </summary>
		ICell CellA { get; set; }

		/// <summary>
		/// Gets/sets the the second cell adjacent to this partition.
		/// </summary>
		ICell CellB { get; set; }

        /// <summary>
		/// Gets the controls that are associated with this partition.
		/// </summary>
		/// <returns></returns>
		IEnumerable<PartitionControlInfo> GetPartitionControlInfos();

		/// <summary>
		/// Gets the controls that are associated with this partition.
		/// </summary>
		/// <returns></returns>
		IEnumerable<PartitionControlInfo> GetPartitionControlInfos(ePartitionFeedback mask);
	}

	public static class PartitionExtensions
	{
		/// <summary>
		/// Returns the rooms that are added as adjacent to this partition.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<IRoom> GetRooms([NotNull] this IPartition extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			IRoom roomA = extends.CellA == null ? null : extends.CellA.Room;
			IRoom roomB = extends.CellB == null ? null : extends.CellB.Room;

			if (roomA != null)
				yield return roomA;

			if (roomB != null)
				yield return roomB;
		}

		/// <summary>
		/// Returns the room ids that are added as adjacent to this partition.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<int> GetRoomIds([NotNull] this IPartition extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetRooms().Select(r => r.Id);
		}

		/// <summary>
		/// Returns true if the other partition shares a common room.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static bool IsAdjacent([NotNull] this IPartition extends, [NotNull] IPartition other)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (other == null)
				throw new ArgumentNullException("other");

			return extends.GetRooms().Any(other.ContainsRoom);
		}

		/// <summary>
		/// Returns true if the given room has been added as adjacent to this partition.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="room"></param>
		/// <returns></returns>
		public static bool ContainsRoom([NotNull] this IPartition extends, [NotNull] IRoom room)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (room == null)
				throw new ArgumentNullException("room");

			return extends.GetRooms().Contains(room);
		}

		/// <summary>
		/// Returns true if the given room has been added as adjacent to this partition.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public static bool ContainsRoom(this IPartition extends, int roomId)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetRoomIds().Contains(roomId);
		}
	}
}
