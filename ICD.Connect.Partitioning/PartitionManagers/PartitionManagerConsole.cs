using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Partitioning.Cells;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Partitioning.Rooms;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public static class PartitionManagerConsole
	{
		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleNodeBase> GetConsoleNodes(IPartitionManager instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			yield break;
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="addRow"></param>
		public static void BuildConsoleStatus(IPartitionManager instance, AddStatusRowDelegate addRow)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleCommand> GetConsoleCommands(IPartitionManager instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			yield return new ConsoleCommand("PrintCells", "Prints the list of all cells.", () => PrintCells(instance));
			yield return new ConsoleCommand("PrintPartitions", "Prints the list of all partitions.", () => PrintPartitions(instance));
			yield return new ConsoleCommand("PrintRooms", "Prints the list of rooms and their children.", () => PrintRooms(instance));
		}

		private static string PrintCells(IPartitionManager instance)
		{
			TableBuilder builder = new TableBuilder("Id", "Cell", "Column", "Row", "Room");

			foreach (ICell cell in instance.Cells.OrderBy(c => c.Id))
				builder.AddRow(cell.Id, cell, cell.Column, cell.Row, cell.Room);

			return builder.ToString();
		}

		private static string PrintPartitions(IPartitionManager instance)
		{
			TableBuilder builder = new TableBuilder("Id", "Partition", "Controls", "Rooms");

			foreach (IPartition partition in instance.Partitions.OrderBy(c => c.Id))
			{
				int id = partition.Id;
				string controls = StringUtils.ArrayFormat(partition.GetPartitionControlInfos().Order());
				string rooms = StringUtils.ArrayFormat(partition.GetRooms().OrderBy(r => r.Id));

				builder.AddRow(id, partition, controls, rooms);
			}

			return builder.ToString();
		}

		private static string PrintRooms(IPartitionManager instance)
		{
			TableBuilder builder = new TableBuilder("Id", "Room", "Children", "Combine Pritority", "Combine State");

			IEnumerable<IRoom> rooms = instance.GetTopLevelRooms().OrderBy(r => r.Id);

			foreach (IRoom room in rooms)
			{
				int id = room.Id;
				string children = StringUtils.ArrayFormat(room.GetRooms().Select(r => r.Id).Order());

				builder.AddRow(id, room, children, room.CombinePriority, room.CombineState);
			}

			return builder.ToString();
		}
	}
}
