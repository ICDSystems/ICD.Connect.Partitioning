using ICD.Common.Properties;
using ICD.Connect.Devices.SPlusShims;
using ICD.Connect.Partitioning.CrestronSPlus.Devices;

namespace ICD.Connect.Partitioning.CrestronSPlus.Shims
{
	public sealed class SPlusPartitionSensorShim : AbstractSPlusDeviceShim<SPlusPartitionSensorDevice>
	{

		#region SPlus

		[PublicAPI("S+")]
		public void SetPartitionState(ushort state)
		{
			if (Originator != null)
				Originator.IsOpen = state != 0;
		}

		#endregion

	}
}