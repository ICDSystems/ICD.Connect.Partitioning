using System;
using ICD.Common.EventArguments;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Partitions.Controls;

namespace ICD.Connect.Partitions.Devices
{
	public abstract class AbstractPartitionDevice<TSettings> : AbstractDevice<TSettings>, IPartitionDevice
		where TSettings : IPartitionDeviceSettings, new()
	{
		/// <summary>
		/// Raised when the partition is detected as open or closed.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnOpenStatusChanged;

		private readonly PartitionDeviceControl m_PartitionControl;
		private bool m_IsOpen;

		#region Properties

		/// <summary>
		/// Gets the partition control for this device.
		/// </summary>
		[PublicAPI]
		public PartitionDeviceControl PartitionControl { get { return m_PartitionControl; } }

		/// <summary>
		/// Gets the partition control for this device.
		/// </summary>
		IPartitionDeviceControl IPartitionDevice.PartitionControl { get { return PartitionControl; } }

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

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractPartitionDevice()
		{
			m_PartitionControl = new PartitionDeviceControl(this, 0);
			Controls.Add(m_PartitionControl);
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
