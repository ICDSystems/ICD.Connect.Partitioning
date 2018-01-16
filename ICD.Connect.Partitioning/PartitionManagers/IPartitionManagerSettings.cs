using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public interface IPartitionManagerSettings : ISettings
	{
		SettingsCollection PartitionSettings { get; }
	}
}
