using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.API.Commands;

namespace ICD.Connect.Partitions.Devices
{
	public sealed class MockPartitionDevice : AbstractPartitionDevice<MockPartitionDeviceSettings>
	{
		#region Methods

		[PublicAPI]
		public void Open()
		{
			IsOpen = true;
		}

		[PublicAPI]
		public void Close()
		{
			IsOpen = false;
		}

		#endregion

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return true;
		}

		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("Open", "Sets the partition as open", () => Open());
			yield return new ConsoleCommand("Close", "Sets the partition as closed", () => Close());
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
