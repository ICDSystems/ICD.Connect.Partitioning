using System;
using System.Collections.Generic;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public abstract class AbstractPartitionManagerSettings : AbstractSettings, IPartitionManagerSettings
	{
		private const string ELEMENT_NAME = "Partitioning";

		private const string PARTITIONS_ELEMENT = "Partitions";

		private readonly SettingsCollection m_PartitionSettings;

		#region Properties

		public SettingsCollection PartitionSettings { get { return m_PartitionSettings; } }

		protected override string Element { get { return ELEMENT_NAME; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractPartitionManagerSettings()
		{
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

			m_PartitionSettings.ToXml(writer, PARTITIONS_ELEMENT);
		}

		public static void ParseXml(AbstractPartitionManagerSettings instance, string xml)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			IEnumerable<ISettings> partitions = PluginFactory.GetSettingsFromXml(xml, PARTITIONS_ELEMENT);

			instance.PartitionSettings.SetRange(partitions);

			AbstractSettings.ParseXml(instance, xml);
		}

		#endregion
	}
}
