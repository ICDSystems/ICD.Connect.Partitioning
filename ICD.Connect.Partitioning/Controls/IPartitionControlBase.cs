using System;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;

namespace ICD.Connect.Partitioning.Controls
{
	public interface IPartitionControlBase
	{
		/// <summary>
		/// Raised when the partition is detected as open or closed.
		/// </summary>
		[PublicAPI]
		event EventHandler<BoolEventArgs> OnOpenStatusChanged;

		/// <summary>
		/// Returns the mask for the type of feedback that is supported,
		/// I.e. if we can set the open state of the partition, and if the partition
		/// gives us feedback for the current open state.
		/// </summary>
		ePartitionFeedback SupportsFeedback { get; }

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
