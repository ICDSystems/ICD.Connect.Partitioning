using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Panels;
using ICD.Connect.Protocol.Ports;

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

			yield return
				ConsoleNodeGroup.KeyNodeMap("Panels", instance.Originators.GetInstances<IPanelDevice>().OfType<IConsoleNode>(),
				                            p => (uint)((IPanelDevice)p).Id);
			yield return
				ConsoleNodeGroup.KeyNodeMap("Devices", instance.Originators.GetInstances<IDevice>().OfType<IConsoleNode>(),
				                            p => (uint)((IDevice)p).Id);
			yield return
				ConsoleNodeGroup.KeyNodeMap("Ports", instance.Originators.GetInstances<IPort>().OfType<IConsoleNode>(),
				                            p => (uint)((IPort)p).Id);
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

			yield return
				new GenericConsoleCommand<int>("SetCombinePriority", "SetCombinePriority <PRIORITY>",
				                               i => instance.CombinePriority = i);
		}
	}
}
