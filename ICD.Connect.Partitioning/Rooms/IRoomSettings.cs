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

		/// <summary>
		/// Gets the source settings.
		/// </summary>
		Dictionary<int, eCombineMode> Sources { get; }

		/// <summary>
		/// Gets the audio destination settings.
		/// </summary>
		Dictionary<int, eCombineMode> AudioDestinations { get; }

		/// <summary>
		/// Gets the destination settings.
		/// </summary>
		Dictionary<int, eCombineMode> Destinations { get; }

		/// <summary>
		/// Gets the destination group settings.
		/// </summary>
		Dictionary<int, eCombineMode> DestinationGroups { get; }

		/// <summary>
		/// Gets the partition settings.
		/// </summary>
		Dictionary<int, eCombineMode> Partitions { get; }

		/// <summary>
		/// Gets the volume point settings.
		/// </summary>
		Dictionary<int, eCombineMode> VolumePoints { get; }
	}
}
