using System;
using ICD.Common.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Partitions.Controls
{
	public abstract class AbstractPartitionDeviceControl<TParent> : AbstractDeviceControl<TParent>, IPartitionDeviceControl
		where TParent : IDevice
	{
		/// <summary>
		/// Raised when the partition is detected as open or closed.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnOpenStatusChanged;

		private bool m_IsOpen;

		/// <summary>
		/// Returns the current open state of the partition.
		/// </summary>
		public bool IsOpen
		{
			get { return m_IsOpen; }
			protected set
			{
				if (value == m_IsOpen)
					return;

				m_IsOpen = value;

				OnOpenStatusChanged.Raise(this, new BoolEventArgs(m_IsOpen));
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractPartitionDeviceControl(TParent parent, int id)
			: base(parent, id)
		{
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnOpenStatusChanged = null;

			base.DisposeFinal(disposing);
		}

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("IsOpen", IsOpen);
		}

		#endregion
	}
}
