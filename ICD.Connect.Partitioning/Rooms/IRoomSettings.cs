using System.Collections.Generic;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.Rooms
{
	public interface IRoomSettings : ISettings
	{
		int CombinePriority { get; set; }

		/// <summary>
		/// Gets the device settings.
		/// </summary>
		Dictionary<int, eCombineMode> Devices { get; }

		/// <summary>
		/// Gets the port settings.
		/// </summary>
		Dictionary<int, eCombineMode> Ports { get; }

		/// <summary>
		/// Gets the panel settings.
		/// </summary>
		Dictionary<int, eCombineMode> Panels { get; }

		Dictionary<int, eCombineMode> Sources { get; }

		Dictionary<int, eCombineMode> Destinations { get; }

		Dictionary<int, eCombineMode> DestinationGroups { get; }

		Dictionary<int, eCombineMode> Partitions { get; }
	}
}
