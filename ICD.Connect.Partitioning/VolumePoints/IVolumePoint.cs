using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.VolumePoints
{
	public interface IVolumePoint : IOriginator
	{
		/// <summary>
		/// Device id
		/// </summary>
		int DeviceId { get; set; }

		/// <summary>
		/// Control id.
		/// </summary>
		int ControlId { get; set; }
	}
}
