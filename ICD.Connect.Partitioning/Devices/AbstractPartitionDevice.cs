using System;
using System.Collections.Generic;
using ICD.Common.Logging.Activities;
using ICD.Common.Logging.LoggingContexts;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Partitioning.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.Devices
{
	public abstract class AbstractPartitionDevice<TSettings> : AbstractDevice<TSettings>, IPartitionDevice
		where TSettings : IPartitionDeviceSettings, new()
	{
		/// <summary>
		/// Raised when the partition is detected as open or closed.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnOpenStatusChanged;

		private bool m_IsOpen;

		#region Properties

		/// <summary>
		/// Returns the mask for the type of feedback that is supported,
		/// I.e. if we can set the open state of the partition, and if the partition
		/// gives us feedback for the current open state.
		/// </summary>
		public abstract ePartitionFeedback SupportsFeedback { get; }

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

				Logger.LogSetTo(eSeverity.Informational, "IsOpen", m_IsOpen);
				Activities.LogActivity(m_IsOpen
					                   ? new Activity(Activity.ePriority.Medium, "Is Open", "Open", eSeverity.Informational)
					                   : new Activity(Activity.ePriority.Medium, "Is Open", "Closed", eSeverity.Informational));

				OnOpenStatusChanged.Raise(this, new BoolEventArgs(m_IsOpen));
			}
		}

		#endregion

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnOpenStatusChanged = null;

			base.DisposeFinal(disposing);
		}

		#region Methods

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

		#endregion

		#region New region

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(TSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new PartitionDeviceControl(this, 0));
		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Supports Feedback", SupportsFeedback);
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
