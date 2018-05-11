using System.Collections.Generic;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Partitioning.Controls;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Partitioning.Partitions
{
	public interface IPartitionsCollection : IOriginatorCollection<IPartition>
	{
		/// <summary>
		/// Gets the partitions related to the given control.
		/// </summary>
		/// <param name="deviceControlInfo"></param>
		IEnumerable<IPartition> GetPartitions(DeviceControlInfo deviceControlInfo);

		/// <summary>
		/// Gets the partitions related to the given control.
		/// </summary>
		/// <param name="deviceControl"></param>
		/// <returns></returns>
		IEnumerable<IPartition> GetPartitions(IPartitionDeviceControl deviceControl);

		/// <summary>
		/// Gets the partitions related to the given controls.
		/// </summary>
		/// <param name="deviceControls"></param>
		/// <returns></returns>
		IEnumerable<IPartition> GetPartitions(IEnumerable<IPartitionDeviceControl> deviceControls);

		/// <summary>
		/// Gets the partitions that are adjacent to the given partition.
		/// </summary>
		/// <param name="partition"></param>
		/// <returns></returns>
		IEnumerable<IPartition> GetAdjacentPartitions(IPartition partition);
	}
}
