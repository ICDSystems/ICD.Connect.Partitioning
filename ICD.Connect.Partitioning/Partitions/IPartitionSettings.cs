using System.Collections.Generic;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.Partitions
{
	public interface IPartitionSettings : ISettings
	{
		/// <summary>
		/// Sets the controls associated with this partition.
		/// </summary>
		/// <param name="partitionControls"></param>
		void SetPartitionControls(IEnumerable<DeviceControlInfo> partitionControls);

		/// <summary>
		/// Returns the controls that are associated with thr
		/// </summary>
		/// <returns></returns>
		IEnumerable<DeviceControlInfo> GetPartitionControls();

		/// <summary>
		/// Sets the rooms that are adjacent to the partition.
		/// </summary>
		/// <param name="roomIds"></param>
		void SetRooms(IEnumerable<int> roomIds);

		/// <summary>
		/// Returns the rooms that are added as adjacent to the partition.
		/// </summary>
		/// <returns></returns>
		IEnumerable<int> GetRooms();
	}
}
