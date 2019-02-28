using System.Collections.Generic;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Partitioning.Cells;
using ICD.Connect.Partitioning.Controls;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Partitioning.Partitions
{
	public interface IPartitionSettings : ISettings
	{
		/// <summary>
		/// Gets/sets the id for the first cell adjacent to this partition.
		/// </summary>
		[OriginatorIdSettingsProperty(typeof(ICell))]
		int? CellAId { get; set; }

		/// <summary>
		/// Gets/sets the id for the second cell adjacent to this partition.
		/// </summary>
		[OriginatorIdSettingsProperty(typeof(ICell))]
		int? CellBId { get; set; }

		/// <summary>
		/// Sets the controls associated with this partition.
		/// </summary>
		/// <param name="partitionControls"></param>
		void SetPartitionControls(IEnumerable<PartitionDeviceControlInfo> partitionControls);

		/// <summary>
		/// Returns the controls that are associated with thr
		/// </summary>
		/// <returns></returns>
		IEnumerable<PartitionDeviceControlInfo> GetPartitionControls();
	}
}
