using ICD.Common.Utils.Collections;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.Rooms
{
	public interface IRoomSettings : ISettings
	{
		int CombinePriority { get; set; }

		/// <summary>
		/// Gets the device settings.
		/// </summary>
		IcdHashSet<int> Devices { get; }

		/// <summary>
		/// Gets the port settings.
		/// </summary>
		IcdHashSet<int> Ports { get; }

		/// <summary>
		/// Gets the panel settings.
		/// </summary>
		IcdHashSet<int> Panels { get; }

		IcdHashSet<int> Sources { get; }

		IcdHashSet<int> Destinations { get; }

		IcdHashSet<int> DestinationGroups { get; }

		IcdHashSet<int> Partitions { get; }
	}
}
