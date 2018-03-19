﻿using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.Rooms
{
	/// <summary>
	/// Base class for room settings.
	/// </summary>
	[PublicAPI]
	public abstract class AbstractRoomSettings : AbstractSettings, IRoomSettings
	{
		private const string ROOM_ELEMENT = "Room";

		private const string COMBINE_PRIORITY_ELEMENT = "CombinePriority";

		private const string PANELS_ELEMENT = "Panels";
		private const string PANEL_ELEMENT = "Panel";
		private const string PORTS_ELEMENT = "Ports";
		private const string PORT_ELEMENT = "Port";
		private const string DEVICES_ELEMENT = "Devices";
		private const string DEVICE_ELEMENT = "Device";
		private const string SOURCES_ELEMENT = "Sources";
		private const string SOURCE_ELEMENT = "Source";
		private const string DESTINATIONS_ELEMENT = "Destinations";
		private const string DESTINATION_ELEMENT = "Destination";
		private const string DESTINATION_GROUPS_ELEMENT = "DestinationGroups";
		private const string DESTINATION_GROUP_ELEMENT = "DestinationGroup";
		private const string PARTITION_ELEMENT = "Partition";
		private const string PARTITIONS_ELEMENT = "Partitions";
        private const string VOLUME_POINT_ELEMENT = "VolumePoint";
        private const string VOLUME_POINTS_ELEMENT = "VolumePoints";

		private const string COMBINE_ATTRIBUTE = "combine";

		private readonly Dictionary<int, eCombineMode> m_Devices;
		private readonly Dictionary<int, eCombineMode> m_Ports;
		private readonly Dictionary<int, eCombineMode> m_Panels;
		private readonly Dictionary<int, eCombineMode> m_Sources;
		private readonly Dictionary<int, eCombineMode> m_Destinations;
		private readonly Dictionary<int, eCombineMode> m_DestinationGroups;
		private readonly Dictionary<int, eCombineMode> m_Partitions;
        private readonly Dictionary<int, eCombineMode> m_VolumePoints;

		#region Properties

		public int CombinePriority { get; set; }

		public Dictionary<int, eCombineMode> Devices { get { return m_Devices; } }
		public Dictionary<int, eCombineMode> Ports { get { return m_Ports; } }
		public Dictionary<int, eCombineMode> Panels { get { return m_Panels; } }
		public Dictionary<int, eCombineMode> Sources { get { return m_Sources; } }
		public Dictionary<int, eCombineMode> Destinations { get { return m_Destinations; } }
		public Dictionary<int, eCombineMode> DestinationGroups { get { return m_DestinationGroups; } }
		public Dictionary<int, eCombineMode> Partitions { get { return m_Partitions; } }
        public Dictionary<int, eCombineMode> VolumePoints { get { return m_VolumePoints; } }

		/// <summary>
		/// Gets the xml element.
		/// </summary>
		protected override string Element { get { return ROOM_ELEMENT; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractRoomSettings()
		{
			m_Devices = new Dictionary<int, eCombineMode>();
			m_Ports = new Dictionary<int, eCombineMode>();
			m_Panels = new Dictionary<int, eCombineMode>();
			m_Sources = new Dictionary<int, eCombineMode>();
			m_Destinations = new Dictionary<int, eCombineMode>();
			m_DestinationGroups = new Dictionary<int, eCombineMode>();
			m_Partitions = new Dictionary<int, eCombineMode>();
            m_VolumePoints = new Dictionary<int, eCombineMode>();
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(COMBINE_PRIORITY_ELEMENT, IcdXmlConvert.ToString(CombinePriority));

			WriteChildrenToXml(writer, m_Devices, DEVICES_ELEMENT, DEVICE_ELEMENT);
			WriteChildrenToXml(writer, m_Panels, PANELS_ELEMENT, PANEL_ELEMENT);
			WriteChildrenToXml(writer, m_Ports, PORTS_ELEMENT, PORT_ELEMENT);
			WriteChildrenToXml(writer, m_Sources, SOURCES_ELEMENT, SOURCE_ELEMENT);
			WriteChildrenToXml(writer, m_Destinations, DESTINATIONS_ELEMENT, DESTINATION_ELEMENT);
			WriteChildrenToXml(writer, m_DestinationGroups, DESTINATION_GROUPS_ELEMENT, DESTINATION_GROUP_ELEMENT);
			WriteChildrenToXml(writer, m_Partitions, PARTITIONS_ELEMENT, PARTITION_ELEMENT);
            WriteChildrenToXml(writer, m_VolumePoints, VOLUME_POINTS_ELEMENT, VOLUME_POINT_ELEMENT);
        }

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			CombinePriority = XmlUtils.TryReadChildElementContentAsInt(xml, COMBINE_PRIORITY_ELEMENT) ?? 0;

			IEnumerable<KeyValuePair<int, eCombineMode>> panels = ReadListFromXml(xml, PANELS_ELEMENT, PANEL_ELEMENT);
			IEnumerable<KeyValuePair<int, eCombineMode>> ports = ReadListFromXml(xml, PORTS_ELEMENT, PORT_ELEMENT);
			IEnumerable<KeyValuePair<int, eCombineMode>> devices = ReadListFromXml(xml, DEVICES_ELEMENT, DEVICE_ELEMENT);
			IEnumerable<KeyValuePair<int, eCombineMode>> sources = ReadListFromXml(xml, SOURCES_ELEMENT, SOURCE_ELEMENT);
			IEnumerable<KeyValuePair<int, eCombineMode>> destinations = ReadListFromXml(xml, DESTINATIONS_ELEMENT, DESTINATION_ELEMENT);
			IEnumerable<KeyValuePair<int, eCombineMode>> destinationGroups = ReadListFromXml(xml, DESTINATION_GROUPS_ELEMENT, DESTINATION_GROUP_ELEMENT);
			IEnumerable<KeyValuePair<int, eCombineMode>> partitions = ReadListFromXml(xml, PARTITIONS_ELEMENT, PARTITION_ELEMENT);
            IEnumerable<KeyValuePair<int, eCombineMode>> volumePoints = ReadListFromXml(xml, VOLUME_POINTS_ELEMENT, VOLUME_POINT_ELEMENT);

            Panels.Update(panels);
			Ports.Update(ports);
			Devices.Update(devices);
			Sources.Update(sources);
			Destinations.Update(destinations);
			DestinationGroups.Update(destinationGroups);
			Partitions.Update(partitions);
            VolumePoints.Update(volumePoints);
		}

		private void WriteChildrenToXml(IcdXmlTextWriter writer, Dictionary<int, eCombineMode> children, string listElement, string childElement)
		{
			XmlUtils.WriteListToXml(writer, children.OrderByKey(), listElement, (w, kvp) => WriteChildToXml(w, kvp, childElement));
		}

		private void WriteChildToXml(IcdXmlTextWriter writer, KeyValuePair<int, eCombineMode> kvp, string childElement)
		{
			writer.WriteStartElement(childElement);
			{
				writer.WriteAttributeString(COMBINE_ATTRIBUTE, kvp.Value.ToString());
				writer.WriteString(IcdXmlConvert.ToString(kvp.Key));
			}
			writer.WriteEndElement();
		}

		private IEnumerable<KeyValuePair<int, eCombineMode>> ReadListFromXml(string xml, string listElement, string childElement)
		{
			return XmlUtils.ReadListFromXml<KeyValuePair<int, eCombineMode>>(xml, listElement, childElement, ReadChildFromXml);
		}

		private KeyValuePair<int, eCombineMode> ReadChildFromXml(string xml)
		{
			string attribute =
				XmlUtils.HasAttribute(xml, COMBINE_ATTRIBUTE)
					? XmlUtils.GetAttribute(xml, COMBINE_ATTRIBUTE).Value
					: null;

			eCombineMode combine =
				attribute == null
					? eCombineMode.Always
					: EnumUtils.Parse<eCombineMode>(attribute, false);

			int id = XmlUtils.ReadElementContentAsInt(xml);

			return new KeyValuePair<int, eCombineMode>(id, combine);
		}
	}
}
