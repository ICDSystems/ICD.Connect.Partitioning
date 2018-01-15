using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning
{
	public interface IPartitionManagerSettings : ISettings
	{
		SettingsCollection PartitionSettings { get; }
	}
}
