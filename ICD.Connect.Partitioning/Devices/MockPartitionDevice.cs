using ICD.Common.Properties;

namespace ICD.Connect.Partitioning.Devices
{
	public sealed class MockPartitionDevice : AbstractPartitionDevice<MockPartitionDeviceSettings>
	{
		#region Methods

		[PublicAPI]
		public override void Open()
		{
			IsOpen = true;
		}

		[PublicAPI]
		public override void Close()
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
	}
}
