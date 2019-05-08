using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public interface IPartitionManagerSettings : ISettings
	{
		/// <summary>
		/// Gets the collection of individual cell settings instances.
		/// </summary>
		SettingsCollection CellSettings { get; }

		/// <summary>
		/// Gets the collection of individual partition settings instances.
		/// </summary>
		SettingsCollection PartitionSettings { get; }
	}
}
