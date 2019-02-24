using System;
using System.Collections.Generic;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Audio.VolumePoints;
using ICD.Connect.Conferencing.ConferencePoints;
using ICD.Connect.Devices;
using ICD.Connect.Panels.Devices;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Sources;

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

			yield return ConsoleNodeGroup.KeyNodeMap("Panels", instance.Originators.GetInstances<IPanelDevice>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("Ports", instance.Originators.GetInstances<IPort>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("Devices", instance.Originators.GetInstances<IDevice>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("Sources", instance.Originators.GetInstances<ISourceBase>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("Destinations", instance.Originators.GetInstances<IDestination>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("Partitions", instance.Originators.GetInstances<IPartition>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("VolumePoints", instance.Originators.GetInstances<IVolumePoint>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("ConferencePoints", instance.Originators.GetInstances<IConferencePoint>(), p => (uint)p.Id);
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
		}
	}
}
