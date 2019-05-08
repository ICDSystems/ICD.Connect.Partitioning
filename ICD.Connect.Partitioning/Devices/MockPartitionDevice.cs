using ICD.Common.Properties;
using ICD.Connect.Partitioning.Controls;

namespace ICD.Connect.Partitioning.Devices
{
	public sealed class MockPartitionDevice : AbstractPartitionDevice<MockPartitionDeviceSettings>
	{
		/// <summary>
		/// Returns the mask for the type of feedback that is supported,
		/// I.e. if we can set the open state of the partition, and if the partition
		/// gives us feedback for the current open state.
		/// </summary>
		public override ePartitionFeedback SupportsFeedback { get { return ePartitionFeedback.GetSet; } }

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
