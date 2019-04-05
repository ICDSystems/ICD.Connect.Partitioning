﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.Partitioning.Controls;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public abstract class AbstractPartitionManager<TSettings> : AbstractOriginator<TSettings>, IPartitionManager
		where TSettings : IPartitionManagerSettings, new()
	{
		public abstract event PartitionControlOpenStateCallback OnPartitionOpenStateChange;

		#region Properties

		/// <summary>
		/// Gets the partitions in the system.
		/// </summary>
		public abstract IPartitionsCollection Partitions { get; }

		/// <summary>
		/// Gets the layout of rooms in the system.
		/// </summary>
		public abstract IRoomLayout RoomLayout { get; }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public override string ConsoleHelp { get { return "Tracks the opening and closing of partition walls."; } }

		#endregion

		#region Methods

		/// <summary>
		/// Gets the control for the given partition.
		/// Returns null if the partition has no control specified.
		/// </summary>
		/// <param name="partition"></param>
		/// <returns></returns>
		public abstract IEnumerable<IPartitionDeviceControl> GetControls(IPartition partition);

		/// <summary>
		/// Returns true if the given partition is currently part of a combine room.
		/// </summary>
		/// <param name="partition"></param>
		/// <returns></returns>
		public abstract bool CombinesRoom(IPartition partition);

		/// <summary>
		/// Returns true if the given partition is currently part of a combine room.
		/// </summary>
		/// <param name="partitionId"></param>
		/// <returns></returns>
		public abstract bool CombinesRoom(int partitionId);

		/// <summary>
		/// Gets the combine room containing the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public abstract IRoom GetCombineRoom(IRoom room);

		/// <summary>
		/// Gets the combine room containing the given partition.
		/// </summary>
		/// <param name="partition"></param>
		/// <returns></returns>
		public abstract IRoom GetCombineRoom(IPartition partition);

		/// <summary>
		/// Gets the combine rooms containing any of the given partitions.
		/// </summary>
		/// <param name="partitions"></param>
		/// <returns></returns>
		public abstract IEnumerable<IRoom> GetCombineRooms(IEnumerable<IPartition> partitions);

		/// <summary>
		/// Returns combine rooms and any individual rooms that are not part of a combined space.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<IRoom> GetTopLevelRooms();

		/// <summary>
		/// Toggles the given partition to create a new combine room or uncombine an existing room.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partition"></param>
		/// <param name="func"></param>
		public abstract void ToggleCombineRooms<TRoom>(IPartition partition, Func<TRoom> func) where TRoom : IRoom;

		/// <summary>
		/// Performs a pass of the partitions, creating combine rooms where partitions are open and
		/// destroying combine rooms where partitions are closed.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="constructor"></param>
		public abstract void InitializeCombineRooms<TRoom>(Func<TRoom> constructor) where TRoom : IRoom;

		/// <summary>
		/// Creates a new room instance to contain the given partitions.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partitions"></param>
		/// <param name="constructor"></param>
		public abstract void CombineRooms<TRoom>(IEnumerable<IPartition> partitions, Func<TRoom> constructor)
			where TRoom : IRoom;

		/// <summary>
		/// Creates a new room instance to contain the given partition controls.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="controls"></param>
		/// <param name="constructor"></param>
		public abstract void CombineRooms<TRoom>(IEnumerable<IPartitionDeviceControl> controls, Func<TRoom> constructor)
			where TRoom : IRoom;

		/// <summary>
		/// Creates a new room instance to contain the given partition.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partition"></param>
		/// <param name="constructor"></param>
		public abstract void CombineRooms<TRoom>(IPartition partition, Func<TRoom> constructor) where TRoom : IRoom;

		/// <summary>
		/// Creates a new room instance to contain the partitions tied to the control.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partitionControl"></param>
		/// <param name="constructor"></param>
		public abstract void CombineRooms<TRoom>(IPartitionDeviceControl partitionControl, Func<TRoom> constructor)
			where TRoom : IRoom;

		/// <summary>
		/// Removes the partitions from existing rooms.
		/// </summary>
		/// <param name="partitions"></param>
		/// <param name="constructor"></param>
		public abstract void UncombineRooms<TRoom>(IEnumerable<IPartition> partitions, Func<TRoom> constructor)
			where TRoom : IRoom;

		/// <summary>
		/// Removes the partition from existing rooms.
		/// </summary>
		/// <param name="partition"></param>
		/// <param name="constructor"></param>
		public abstract void UncombineRooms<TRoom>(IPartition partition, Func<TRoom> constructor) where TRoom : IRoom;

		/// <summary>
		/// Removes the partitions tied to the given control from existing rooms.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partitionControl"></param>
		/// <param name="constructor"></param>
		public abstract void UncombineRooms<TRoom>(IPartitionDeviceControl partitionControl, Func<TRoom> constructor)
			where TRoom : IRoom;

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("PrintPartitions", "Prints the list of all partitions.", () => PrintPartitions());
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		private string PrintPartitions()
		{
			TableBuilder builder = new TableBuilder("Id", "Partition", "Controls", "Rooms");

			foreach (IPartition partition in Partitions.OrderBy(c => c.Id))
			{
				int id = partition.Id;
				string controls = StringUtils.ArrayFormat(partition.GetPartitionControls().Order());
				string rooms = StringUtils.ArrayFormat(partition.GetRooms().Order());

				builder.AddRow(id, partition, controls, rooms);
			}

			return builder.ToString();
		}

		#endregion
	}
}
