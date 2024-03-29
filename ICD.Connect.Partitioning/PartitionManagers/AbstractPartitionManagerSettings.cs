﻿using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public abstract class AbstractPartitionManagerSettings : AbstractSettings, IPartitionManagerSettings
	{
		private const string CELLS_ELEMENT = "Cells";
		private const string CELL_ELEMENT = "Cell";

		private const string PARTITIONS_ELEMENT = "Partitions";
		private const string PARTITION_ELEMENT = "Partition";

		private readonly SettingsCollection m_CellSettings;
		private readonly SettingsCollection m_PartitionSettings;

		#region Properties

		/// <summary>
		/// Gets the collection of individual cell settings instances.
		/// </summary>
		public SettingsCollection CellSettings { get { return m_CellSettings; } }

		/// <summary>
		/// Gets the collection of individual partition settings instances.
		/// </summary>
		public SettingsCollection PartitionSettings { get { return m_PartitionSettings; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractPartitionManagerSettings()
		{
			m_CellSettings = new SettingsCollection();
			m_PartitionSettings = new SettingsCollection();
		}

		#region Methods

		/// <summary>
		/// Writes the routing settings to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			m_CellSettings.ToXml(writer, CELLS_ELEMENT, CELL_ELEMENT);
			m_PartitionSettings.ToXml(writer, PARTITIONS_ELEMENT, PARTITION_ELEMENT);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			IEnumerable<ISettings> cells = PluginFactory.GetSettingsFromXml(xml, CELLS_ELEMENT);
			IEnumerable<ISettings> partitions = PluginFactory.GetSettingsFromXml(xml, PARTITIONS_ELEMENT);

			AddSettingsLogDuplicates(CellSettings, cells);
			AddSettingsLogDuplicates(PartitionSettings, partitions);
		}

		private void AddSettingsLogDuplicates(SettingsCollection collection, IEnumerable<ISettings> settings)
		{
			foreach (ISettings item in settings.Where(item => !collection.Add(item)))
				Logger.AddEntry(eSeverity.Error, "{0} failed to add duplicate {1}", GetType().Name, item);
		}

		#endregion
	}
}
