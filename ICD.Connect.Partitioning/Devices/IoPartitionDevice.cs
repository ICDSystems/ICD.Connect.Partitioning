using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices.EventArguments;
using ICD.Connect.Protocol.Extensions;
using ICD.Connect.Protocol.Ports.DigitalInput;
using ICD.Connect.Protocol.Ports.IoPort;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.Devices
{
	public sealed class IoPartitionDevice : AbstractPartitionDevice<IoPartitionDeviceSettings>
	{
		private IDigitalInputPort m_Port;
		private bool m_InvertInput;

		/// <summary>
		/// By default we detect a "true" signal from the port as the partition being open.
		/// If InvertInput is true we use "false" for open.
		/// </summary>
		public bool InvertInput
		{
			get { return m_InvertInput; }
			set
			{
				if (value == m_InvertInput)
					return;

				m_InvertInput = value;

				UpdateIsOpen();
			}
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			SetPort(null);
		}

		/// <summary>
		/// Opens the partition.
		/// </summary>
		public override void Open()
		{
			// Currently not supporting opening/closing partitions
			Logger.AddEntry(eSeverity.Error, "{0} does not support opening/closing", GetType().Name);
		}

		/// <summary>
		/// Closes the partition.
		/// </summary>
		public override void Close()
		{
			// Currently not supporting opening/closing partitions
			Logger.AddEntry(eSeverity.Error, "{0} does not support opening/closing", GetType().Name);
		}

		#region Methods

		/// <summary>
		/// Sets the input port for detecting partition changes.
		/// </summary>
		/// <param name="port"></param>
		[PublicAPI]
		public void SetPort(IIoPort port)
		{
			if (port == m_Port)
				return;

			Unsubscribe(port);
			m_Port = port;
			Subscribe(port);

			UpdatePortConfiguration();
			UpdateIsOpen();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_Port != null && m_Port.IsOnline;
		}

		/// <summary>
		/// Forces the port to have the correct configuration to communicate properly with the partition.
		/// </summary>
		private void UpdatePortConfiguration()
		{
			IIoPort ioPort = m_Port as IIoPort;
			if (ioPort == null)
				return;

			ioPort.SetConfiguration(eIoPortConfiguration.DigitalIn);
		}

		#endregion

		#region Port Callbacks

		/// <summary>
		/// Subscribe to the port events.
		/// </summary>
		/// <param name="port"></param>
		private void Subscribe(IIoPort port)
		{
			if (port == null)
				return;

			port.OnIsOnlineStateChanged += PortOnIsOnlineStateChanged;
			port.OnDigitalInChanged += PortOnDigitalInChanged;
			port.OnConfigurationChanged += PortOnConfigurationChanged;
		}

		/// <summary>
		/// Unsubscribe from the port events.
		/// </summary>
		/// <param name="port"></param>
		private void Unsubscribe(IIoPort port)
		{
			if (port == null)
				return;

			port.OnIsOnlineStateChanged -= PortOnIsOnlineStateChanged;
			port.OnDigitalInChanged -= PortOnDigitalInChanged;
			port.OnConfigurationChanged -= PortOnConfigurationChanged;
		}

		/// <summary>
		/// Called when we get a configuration change event from one of the ports.
		/// </summary>
		/// <param name="port"></param>
		/// <param name="configuration"></param>
		private void PortOnConfigurationChanged(IIoPort port, eIoPortConfiguration configuration)
		{
			UpdatePortConfiguration();
		}

		/// <summary>
		/// Called when we get an online state change from one of the ports.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void PortOnIsOnlineStateChanged(object sender, DeviceBaseOnlineStateApiEventArgs args)
		{
			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Called when we get a digital input signal from one of the ports.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void PortOnDigitalInChanged(object sender, BoolEventArgs args)
		{
			UpdateIsOpen();
		}

		/// <summary>
		/// Updates the IsOpen property to match the current state of the port.
		/// </summary>
		private void UpdateIsOpen()
		{
			IsOpen = m_Port != null && (m_Port.State ^ InvertInput);
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(IoPartitionDeviceSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.IoPort = m_Port == null ? (int?)null : m_Port.Id;
			settings.InvertInput = InvertInput;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			SetPort(null);
			InvertInput = false;
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(IoPartitionDeviceSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			IIoPort port = GetPortFromSettings(factory, settings.IoPort);
			SetPort(port);
		}

		private IIoPort GetPortFromSettings(IDeviceFactory factory, int? portId)
		{
			if (portId == null)
				return null;

			IIoPort port = factory.GetPortById((int)portId) as IIoPort;
			if (port == null)
				Logger.AddEntry(eSeverity.Error, "No Serial Port with id {0}", portId);

			return port;
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

			addRow("Invert Output", InvertInput);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new GenericConsoleCommand<bool>("SetInvertInput", "SetInvertInput <true/false>", v => ConsoleSetInvertInput(v));
		}

		/// <summary>
		/// Sets the invert input value via the console.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private string ConsoleSetInvertInput(bool value)
		{
			InvertInput = value;
			return string.Format("InvertInput {0}", value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
