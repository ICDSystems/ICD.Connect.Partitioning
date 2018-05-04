using System.Collections.Generic;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public abstract class AbstractPartitionManagerSettings : AbstractSettings, IPartitionManagerSettings
	{
		private const string PARTITIONS_ELEMENT = "Partitions";
		private const string PARTITION_ELEMENT = "Partition";

		private readonly SettingsCollection m_PartitionSettings;

		#region Properties

		public SettingsCollection PartitionSettings { get { return m_PartitionSettings; } }

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

			m_PartitionSettings.ToXml(writer, PARTITIONS_ELEMENT, PARTITION_ELEMENT);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			IEnumerable<ISettings> partitions = PluginFactory.GetSettingsFromXml(xml, PARTITIONS_ELEMENT);

			foreach (ISettings item in partitions)
			{
				if (PartitionSettings.Add(item))
					continue;

				ServiceProvider.GetService<ILoggerService>()
				               .AddEntry(eSeverity.Error, "{0} failed to add duplicate {1}", GetType().Name, item);
			}
		}

		#endregion
	}
}
