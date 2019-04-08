using System;
using System.Collections.Generic;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Partitioning.Cells;
using ICD.Connect.Partitioning.Controls;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings.Originators;

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
		/// Gets the cells in the system.
		/// </summary>
		public abstract ICellsCollection Cells { get; }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public override string ConsoleHelp { get { return "Tracks the opening and closing of partition walls."; } }

		#endregion

		#region Methods

		/// <summary>
		/// Gets the controls for the given partition.
		/// </summary>
		/// <param name="partition"></param>
		/// <param name="mask"></param>
		/// <returns></returns>
		public abstract IEnumerable<IPartitionDeviceControl> GetControls(IPartition partition, ePartitionFeedback mask);

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
		/// Combines/uncombines rooms in a single pass by opening/closing the given partitions.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="open"></param>
		/// <param name="close"></param>
		/// <param name="constructor"></param>
		public abstract void CombineRooms<TRoom>(IEnumerable<IPartition> open, IEnumerable<IPartition> close, Func<TRoom> constructor) where TRoom : IRoom;

		/// <summary>
		/// Creates a new room instance to contain the given partitions.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partitions"></param>
		/// <param name="constructor"></param>
		public abstract void CombineRooms<TRoom>(IEnumerable<IPartition> partitions, Func<TRoom> constructor)
			where TRoom : IRoom;

		/// <summary>
		/// Creates a new room instance to contain the given partition.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partition"></param>
		/// <param name="constructor"></param>
		public abstract void CombineRooms<TRoom>(IPartition partition, Func<TRoom> constructor) where TRoom : IRoom;

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

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase node in GetBaseConsoleNodes())
				yield return node;

			foreach (IConsoleNodeBase node in PartitionManagerConsole.GetConsoleNodes(this))
				yield return node;
		}

		/// <summary>
		/// Wrokaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			PartitionManagerConsole.BuildConsoleStatus(this, addRow);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			foreach (IConsoleCommand command in PartitionManagerConsole.GetConsoleCommands(this))
				yield return command;
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
