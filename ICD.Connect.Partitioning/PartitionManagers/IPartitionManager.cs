﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.Partitioning.Cells;
using ICD.Connect.Partitioning.Controls;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public delegate void PartitionControlOpenStateCallback(IPartitionDeviceControl control, bool open);

	public interface IPartitionManager : IOriginator
	{
		/// <summary>
		/// Raised when a partition control opens/closes.
		/// </summary>
		event PartitionControlOpenStateCallback OnPartitionControlOpenStateChange;

		/// <summary>
		/// Gets the partitions in the system.
		/// </summary>
		IPartitionsCollection Partitions { get; }

		/// <summary>
		/// Gets the cells in the system.
		/// </summary>
		ICellsCollection Cells { get; }

		/// <summary>
		/// Gets the controls for the given partition.
		/// </summary>
		/// <param name="partition"></param>
		/// <param name="mask"></param>
		/// <returns></returns>
		IEnumerable<IPartitionDeviceControl> GetControls(IPartition partition, ePartitionFeedback mask);

		/// <summary>
		/// Returns true if the given partition is currently part of a combine room.
		/// </summary>
		/// <param name="partition"></param>
		/// <returns></returns>
		bool CombinesRoom(IPartition partition);

		/// <summary>
		/// Returns true if the given partition is currently part of a combine room.
		/// </summary>
		/// <param name="partitionId"></param>
		/// <returns></returns>
		bool CombinesRoom(int partitionId);

		/// <summary>
		/// Gets the combine room containing the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		[CanBeNull]
		IRoom GetCombineRoom(IRoom room);

		/// <summary>
		/// Gets the combine room containing the given partition.
		/// </summary>
		/// <param name="partition"></param>
		/// <returns></returns>
		[CanBeNull]
		IRoom GetCombineRoom(IPartition partition);

		/// <summary>
		/// Gets the combine rooms containing any of the given partitions.
		/// </summary>
		/// <param name="partitions"></param>
		/// <returns></returns>
		IEnumerable<IRoom> GetCombineRooms(IEnumerable<IPartition> partitions);

		/// <summary>
		/// Returns combine rooms and any individual rooms that are not part of a combined space.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IRoom> GetTopLevelRooms();

		/// <summary>
		/// Toggles the given partition to create a new combine room or uncombine an existing room.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partition"></param>
		/// <param name="constructor"></param>
		void ToggleCombineRooms<TRoom>(IPartition partition, Func<TRoom> constructor) where TRoom : IRoom;

		/// <summary>
		/// Performs a pass of the partitions, creating combine rooms where partitions are open and
		/// destroying combine rooms where partitions are closed.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="constructor"></param>
		void InitializeCombineRooms<TRoom>(Func<TRoom> constructor) where TRoom : IRoom;

		/// <summary>
		/// Combines/uncombines rooms in a single pass by opening/closing the given partitions.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="open"></param>
		/// <param name="close"></param>
		/// <param name="constructor"></param>
		void CombineRooms<TRoom>(IEnumerable<IPartition> open, IEnumerable<IPartition> close, Func<TRoom> constructor) where TRoom : IRoom;

		/// <summary>
		/// Creates a new room instance to contain the given partitions.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partitions"></param>
		/// <param name="constructor"></param>
		void CombineRooms<TRoom>(IEnumerable<IPartition> partitions, Func<TRoom> constructor) where TRoom : IRoom;

		/// <summary>
		/// Creates a new room instance to contain the given partition.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partition"></param>
		/// <param name="constructor"></param>
		void CombineRooms<TRoom>(IPartition partition, Func<TRoom> constructor) where TRoom : IRoom;

		/// <summary>
		/// Removes the partitions from existing rooms.
		/// </summary>
		/// <param name="partitions"></param>
		/// <param name="constructor"></param>
		void UncombineRooms<TRoom>(IEnumerable<IPartition> partitions, Func<TRoom> constructor) where TRoom : IRoom;

		/// <summary>
		/// Removes the partition from existing rooms.
		/// </summary>
		/// <param name="partition"></param>
		/// <param name="constructor"></param>
		void UncombineRooms<TRoom>(IPartition partition, Func<TRoom> constructor) where TRoom : IRoom;
	}

	public static class PartitionManagerExtensions
	{
		/// <summary>
		/// Gets the partition for the given cell coordinates and direction.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="column"></param>
		/// <param name="row"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public static IPartition GetPartition([NotNull] this IPartitionManager extends, int column, int row, eCellDirection direction)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			// Get original cell if it exists
			ICell cell = extends.Cells.GetCell(column, row);
			if (cell == null)
				return null;

			ICell neighborCell = extends.Cells.GetNeighboringCell(column, row, direction);
			if (neighborCell == null)
				return null;

			// Find partition that's in between both cells
			return extends.Partitions.FirstOrDefault(p => p.CellA == cell && p.CellB == neighborCell || p.CellA == neighborCell && p.CellB == cell);
		}

		public static void SetPartition<TRoom>([NotNull] this IPartitionManager extends, [NotNull] IPartition partition, bool open)
			where TRoom : IRoom, new()
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (partition == null)
				throw new ArgumentNullException("partition");

			if (open)
				extends.CombineRooms(partition, () => ReflectionUtils.CreateInstance<TRoom>());
			else
				extends.UncombineRooms(partition, () => ReflectionUtils.CreateInstance<TRoom>());
		}

		public static void SetPartition<TRoom>([NotNull] this IPartitionManager extends, [NotNull] IPartitionDeviceControl partitionControl, bool open)
			where TRoom : IRoom, new()
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (partitionControl == null)
				throw new ArgumentNullException("partitionControl");

			IEnumerable<IPartition> partitions = extends.Partitions.GetPartitions(partitionControl);
			foreach (IPartition partition in partitions)
				extends.SetPartition<TRoom>(partition, open);
		}
	}
}
