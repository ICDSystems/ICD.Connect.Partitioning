using ICD.Common.Utils.EventArguments;
using ICD.Connect.Partitioning.Devices;

namespace ICD.Connect.Partitioning.Controls
{
	/// <summary>
	/// Simple partition control for a partition device.
	/// </summary>
	public sealed class PartitionDeviceControl : AbstractPartitionDeviceControl<IPartitionDevice>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public PartitionDeviceControl(IPartitionDevice parent, int id)
			: base(parent, id)
		{
			parent.OnOpenStatusChanged += ParentOnOpenStatusChanged;

			IsOpen = parent.IsOpen;
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
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
			IsOpen = Parent.IsOpen;
		}
	}
}
