using System;
using ICD.Common.EventArguments;
using ICD.Common.Properties;
using ICD.Connect.Devices;
using ICD.Connect.Partitioning.Controls;

namespace ICD.Connect.Partitioning.Devices
{
	/// <summary>
	/// IPartitionDevice simply notifies if a partition has been opened.
	/// </summary>
	public interface IPartitionDevice : IDevice
	{
		/// <summary>
		/// Raised when the partition is detected as open or closed.
		/// </summary>
		event EventHandler<BoolEventArgs> OnOpenStatusChanged;

		/// <summary>
		/// Gets the partition control for this device.
		/// </summary>
		[PublicAPI]
		IPartitionDeviceControl PartitionControl { get; }

		/// <summary>
		/// Returns the current open state of the partition.
		/// </summary>
		[PublicAPI]
		bool IsOpen { get; }
	}
}
