using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.Simpl;
using ICD.Connect.Partitioning.Controls;
using ICD.Connect.Partitioning.Devices;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.CrestronSPlus.Devices
{
	public sealed class SPlusPartitionSensorDevice : AbstractSimplDevice<SPlusPartitionSensorDeviceSettings>, IPartitionDevice
	{
		public event EventHandler<BoolEventArgs> OnOpenStatusChanged;

		private bool m_IsOpen;

		/// <summary>
		/// Returns the mask for the type of feedback that is supported,
		/// I.e. if we can set the open state of the partition, and if the partition
		/// gives us feedback for the current open state.
		/// </summary>
		public ePartitionFeedback SupportsFeedback { get { return ePartitionFeedback.Get; } }

		/// <summary>
		/// Returns the current open state of the partition.
		/// </summary>
		public bool IsOpen {
			get { return m_IsOpen; }
			internal set
			{
				if (m_IsOpen == value)
					return;

				m_IsOpen = value;

				OnOpenStatusChanged.Raise(this, value);
			}
		}

		/// <summary>
		/// Opens the partition.
		/// </summary>
		public void Open()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Closes the partition.
		/// </summary>
		public void Close()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Toggles the open state of the partition.
		/// </summary>
		public void Toggle()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(SPlusPartitionSensorDeviceSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new PartitionDeviceControl(this, 1));
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("IsOpen", IsOpen);
		}
	}
}