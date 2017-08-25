using System.Collections.Generic;
using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Partitioning.Partitions
{
	public interface IPartitionsCollection : IEnumerable<IPartition>
	{
		/// <summary>
		/// Gets all of the partitions.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IPartition> GetPartitions();

		/// <summary>
		/// Clears and sets the partitions.
		/// </summary>
		/// <param name="partitions"></param>
		void SetPartitions(IEnumerable<IPartition> partitions);

		/// <summary>
		/// Gets the partitions related to the given control.
		/// </summary>
		/// <param name="deviceControlInfo"></param>
		IEnumerable<IPartition> GetPartitions(DeviceControlInfo deviceControlInfo);
	}
}
