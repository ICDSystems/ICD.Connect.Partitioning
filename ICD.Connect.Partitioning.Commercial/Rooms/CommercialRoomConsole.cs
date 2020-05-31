using System;
using System.Collections.Generic;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Calendaring.CalendarPoints;
using ICD.Connect.Conferencing.ConferencePoints;

namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	public static class CommercialRoomConsole
	{
		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleNodeBase> GetConsoleNodes(ICommercialRoom instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			yield return ConsoleNodeGroup.KeyNodeMap("ConferencePoints", instance.Originators.GetInstancesRecursive<IConferencePoint>(), p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("CalendarPoints", instance.Originators.GetInstancesRecursive<ICalendarPoint>(), p => (uint)p.Id);
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="addRow"></param>
		public static void BuildConsoleStatus(ICommercialRoom instance, AddStatusRowDelegate addRow)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			addRow("IsAwake", instance.IsAwake);
			addRow("Seat Count", instance.SeatCount);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleCommand> GetConsoleCommands(ICommercialRoom instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			yield return new ConsoleCommand("Wake", "Wakes the room", () => instance.Wake());
			yield return new ConsoleCommand("Sleep", "Puts the room to sleep", () => instance.Sleep());
		}
	}
}
