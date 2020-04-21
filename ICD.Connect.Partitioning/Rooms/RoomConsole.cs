using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Audio.VolumePoints;
using ICD.Connect.Conferencing.ConferencePoints;
using ICD.Connect.Devices;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Routing.Groups.Endpoints.Destinations;
using ICD.Connect.Routing.Groups.Endpoints.Sources;

namespace ICD.Connect.Partitioning.Rooms
{
	public static class RoomConsole
	{
		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleNodeBase> GetConsoleNodes(IRoom instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			yield return ConsoleNodeGroup.KeyNodeMap("Ports", instance.Originators.GetInstancesRecursive<IPort>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("Devices", instance.Originators.GetInstancesRecursive<IDevice>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("Sources", instance.Originators.GetInstancesRecursive<ISource>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("Destinations", instance.Originators.GetInstancesRecursive<IDestination>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("SourceGroups", instance.Originators.GetInstancesRecursive<ISourceGroup>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("DestinationGroups", instance.Originators.GetInstancesRecursive<IDestinationGroup>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("Partitions", instance.Originators.GetInstancesRecursive<IPartition>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("VolumePoints", instance.Originators.GetInstancesRecursive<IVolumePoint>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("ConferencePoints", instance.Originators.GetInstancesRecursive<IConferencePoint>(), p => (uint)p.Id);
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="addRow"></param>
		public static void BuildConsoleStatus(IRoom instance, AddStatusRowDelegate addRow)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			addRow("Combine Priority", instance.CombinePriority);
			addRow("Combine State", instance.CombineState);
			addRow("Is Combine Room", instance.IsCombineRoom());
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleCommand> GetConsoleCommands(IRoom instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

#if DEBUG
			yield return
				new GenericConsoleCommand<int>("SetCombinePriority", "SetCombinePriority <PRIORITY>",
				                               i => instance.CombinePriority = i);
#else
			yield break;
#endif

			yield return new ConsoleCommand("ListChildRooms", "Lists the child rooms, and whether they are a slave or master room.", ()=> ListChildRooms(instance));
		}

		private static string ListChildRooms(IRoom instance)
		{
			TableBuilder builder = new TableBuilder("Room Id", "Master/Slave", "Combine Priority");

			foreach (IRoom room in instance.GetRoomsRecursive().Except(instance))
			{
				builder.AddRow(room.Id, room.IsMasterRoom() ? "Master" : "Slave", room.CombinePriority);
			}

			return builder.ToString();
		}
	}
}
