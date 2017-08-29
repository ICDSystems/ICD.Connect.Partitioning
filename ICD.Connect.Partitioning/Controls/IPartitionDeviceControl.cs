using System;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Properties;
using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Partitioning.Controls
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

		/// <summary>
		/// Opens the partition.
		/// </summary>
		[PublicAPI]
		void Open();

		/// <summary>
		/// Closes the partition.
		/// </summary>
		[PublicAPI]
		void Close();

		/// <summary>
		/// Toggles the open state of the partition.
		/// </summary>
		[PublicAPI]
		void Toggle();
	}
}
