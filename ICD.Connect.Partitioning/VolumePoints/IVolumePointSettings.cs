using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.VolumePoints
{
	public interface IVolumePointSettings : ISettings
	{
		int DeviceId { get; set; }

		int ControlId { get; set; }
	}
}
