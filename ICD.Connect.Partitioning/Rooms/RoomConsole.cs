using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Timers;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Panels;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Routing;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;

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

#if DEBUG
			yield return
				new GenericConsoleCommand<int>("SetCombinePriority", "SetCombinePriority <PRIORITY>",
				                               i => instance.CombinePriority = i);

			yield return new GenericConsoleCommand<int>("ProfileRouting", "ProfileRouting <ITERATIONS>", i => ProfileRouting(instance, i));
#endif
		}

		private static void ProfileRouting(IRoom instance, int iterations)
		{
			IRouteSwitcherControl[] switchers =
				instance.Originators
				        .GetInstancesRecursive<IDeviceBase>()
				        .Select(d => d.Controls.GetControl<IRouteSwitcherControl>())
				        .Where(c => c != null)
				        .Distinct()
				        .ToArray();

			IcdStopwatch.Profile(() => RouteRandom(switchers), iterations, "Profile Routing");
		}

		private static void RouteRandom(IEnumerable<IRouteSwitcherControl> switchers)
		{
			IRouteSwitcherControl switcher = switchers.Random();
			ConnectorInfo input = switcher.GetInputs().Random();
			ConnectorInfo output = switcher.GetOutputs().Random();

			eConnectionType type = EnumUtils.GetFlagsIntersection(input.ConnectionType, output.ConnectionType);

			switcher.Route(input.Address, output.Address, type);
		}
	}
}
