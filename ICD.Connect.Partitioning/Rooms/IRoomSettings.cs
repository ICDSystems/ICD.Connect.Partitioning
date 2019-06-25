using System.Collections.Generic;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.Rooms
{
	public interface IRoomSettings : ISettings
	{
		int CombinePriority { get; set; }

		/// <summary>
		/// Gets the dialing plan.
		/// </summary>
		string DialingPlan { get; set; }

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
		/// Gets the destination settings.
		/// </summary>
		Dictionary<int, eCombineMode> Destinations { get; }

		/// <summary>
		/// Gets the volume point settings.
		/// </summary>
		Dictionary<int, eCombineMode> VolumePoints { get; }

		/// <summary>
		/// Gets the conference point settings.
		/// </summary>
		Dictionary<int, eCombineMode> ConferencePoints { get; }
	}
}
