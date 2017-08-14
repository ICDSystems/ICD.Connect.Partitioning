using System;
using ICD.Common.EventArguments;
using ICD.Common.Properties;
using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Partitions.Controls
{
	public interface IPartitionDeviceControl : IDeviceControl
	{
		/// <summary>
		/// Raised when the partition is detected as open or closed.
		/// </summary>
		[PublicAPI]
		event EventHandler<BoolEventArgs> OnOpenStatusChanged;

		/// <summary>
		/// Returns the current open state of the partition.
		/// </summary>
		[PublicAPI]
		bool IsOpen { get; }
	}
}
