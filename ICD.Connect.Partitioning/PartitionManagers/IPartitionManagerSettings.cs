using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public interface IPartitionManagerSettings : ISettings
	{
		/// <summary>
		/// Gets the collection of individual partition settings instances.
		/// </summary>
		SettingsCollection PartitionSettings { get; }

		/// <summary>
		/// Gets the room layout settings.
		/// </summary>
		RoomLayoutSettings RoomLayoutSettings { get; }
	}
}
