using ICD.Common.Utils.Collections;

namespace ICD.Connect.Settings
{
	public interface IRoomSettings : ISettings
	{
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
	}
}
