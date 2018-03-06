using System;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Partitioning.Devices;

namespace ICD.Connect.Partitioning.Controls
{
	/// <summary>
	/// Simple partition control for a partition device.
	/// </summary>
	public sealed class PartitionDeviceControl : AbstractPartitionDeviceControl<IPartitionDevice>
	{
		/// <summary>
		/// Raised when the partition is detected as open or closed.
		/// </summary>
		public override event EventHandler<BoolEventArgs> OnOpenStatusChanged;

		/// <summary>
		/// Returns the current open state of the partition.
		/// </summary>
		public override bool IsOpen { get { return Parent.IsOpen; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public PartitionDeviceControl(IPartitionDevice parent, int id)
			: base(parent, id)
		{
			parent.OnOpenStatusChanged += ParentOnOpenStatusChanged;
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnOpenStatusChanged = null;

			Parent.OnOpenStatusChanged -= ParentOnOpenStatusChanged;

			base.DisposeFinal(disposing);
		}

		/// <summary>
		/// Opens the partition.
		/// </summary>
		public override void Open()
		{
			Parent.Open();
		}

		/// <summary>
		/// Closes the partition.
		/// </summary>
		public override void Close()
		{
			Parent.Close();
		}

		/// <summary>
		/// Called when the parent IsOpen status changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ParentOnOpenStatusChanged(object sender, BoolEventArgs args)
		{
			OnOpenStatusChanged.Raise(this, new BoolEventArgs(args.Data));
		}
	}
}
