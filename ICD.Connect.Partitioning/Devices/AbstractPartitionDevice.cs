using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Partitioning.Controls;

namespace ICD.Connect.Partitioning.Devices
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

				Log(eSeverity.Informational, "Open status changed to {0}", m_IsOpen);

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

		/// <summary>
		/// Opens the partition.
		/// </summary>
		public abstract void Open();

		/// <summary>
		/// Closes the partition.
		/// </summary>
		public abstract void Close();

		/// <summary>
		/// Toggles the open state of the partition.
		/// </summary>
		public virtual void Toggle()
		{
			if (IsOpen)
				Close();
			else
				Open();
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

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("Open", "Opens the partition", () => Open());
			yield return new ConsoleCommand("Close", "Closes the partition", () => Close());
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
