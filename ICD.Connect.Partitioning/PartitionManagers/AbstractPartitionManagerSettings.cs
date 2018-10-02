using System.Collections.Generic;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public abstract class AbstractPartitionManagerSettings : AbstractSettings, IPartitionManagerSettings
	{
		private const string PARTITIONS_ELEMENT = "Partitions";
		private const string PARTITION_ELEMENT = "Partition";

		private const string LAYOUT_ELEMENT = "Layout";

		private readonly SettingsCollection m_PartitionSettings;
		private readonly RoomLayoutSettings m_RoomLayoutSettings;

		#region Properties

		/// <summary>
		/// Gets the collection of individual partition settings instances.
		/// </summary>
		public SettingsCollection PartitionSettings { get { return m_PartitionSettings; } }

		/// <summary>
		/// Gets the room layout settings.
		/// </summary>
		public RoomLayoutSettings RoomLayoutSettings { get { return m_RoomLayoutSettings; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractPartitionManagerSettings()
		{
			m_PartitionSettings = new SettingsCollection();
			m_RoomLayoutSettings = new RoomLayoutSettings();
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
			m_RoomLayoutSettings.ToXml(writer, LAYOUT_ELEMENT);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			string layoutXml;
			if (XmlUtils.TryGetChildElementAsString(xml, LAYOUT_ELEMENT, out layoutXml))
				m_RoomLayoutSettings.ParseXml(layoutXml);
			else
				m_RoomLayoutSettings.Clear();

			IEnumerable<ISettings> partitions = PluginFactory.GetSettingsFromXml(xml, PARTITIONS_ELEMENT);

			foreach (ISettings item in partitions)
			{
				if (PartitionSettings.Add(item))
					continue;

				Logger.AddEntry(eSeverity.Error, "{0} failed to add duplicate {1}", GetType().Name, item);
			}
		}

		#endregion
	}
}
