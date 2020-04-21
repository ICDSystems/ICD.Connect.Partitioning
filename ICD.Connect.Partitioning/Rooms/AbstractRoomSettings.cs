using System;
using System.Collections.Generic;
using System.Linq;
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
		private const string COMBINE_PRIORITY_ELEMENT = "CombinePriority";

		[Obsolete] private const string PANELS_ELEMENT = "Panels";
		[Obsolete] private const string PANEL_ELEMENT = "Panel";
		private const string PORTS_ELEMENT = "Ports";
		private const string PORT_ELEMENT = "Port";
		private const string DEVICES_ELEMENT = "Devices";
		private const string DEVICE_ELEMENT = "Device";
		private const string SOURCES_ELEMENT = "Sources";
		private const string SOURCE_ELEMENT = "Source";
		private const string DESTINATIONS_ELEMENT = "Destinations";
		private const string DESTINATION_ELEMENT = "Destination";
		private const string SOURCE_GROUPS_ELEMENT = "SourceGroups";
		private const string SOURCE_GROUP_ELEMENT = "SourceGroup";
		private const string DESTINATION_GROUPS_ELEMENT = "DestinationGroups";
		private const string DESTINATION_GROUP_ELEMENT = "DestinationGroup";
		private const string VOLUME_POINTS_ELEMENT = "VolumePoints";
		private const string VOLUME_POINT_ELEMENT = "VolumePoint";
		private const string CONFERENCE_POINTS_ELEMENT = "ConferencePoints";
		private const string CONFERENCE_POINT_ELEMENT = "ConferencePoint";

		private const string COMBINE_ATTRIBUTE = "combine";

		private readonly Dictionary<int, eCombineMode> m_Devices;
		private readonly Dictionary<int, eCombineMode> m_Ports;
		private readonly Dictionary<int, eCombineMode> m_Sources;
		private readonly Dictionary<int, eCombineMode> m_Destinations;
		private readonly Dictionary<int, eCombineMode> m_SourceGroups;
		private readonly Dictionary<int, eCombineMode> m_DestinationGroups;
		private readonly Dictionary<int, eCombineMode> m_VolumePoints;
		private readonly Dictionary<int, eCombineMode> m_ConferencePoints;

		#region Properties

		public int CombinePriority { get; set; }

		public Dictionary<int, eCombineMode> Devices { get { return m_Devices; } }
		public Dictionary<int, eCombineMode> Ports { get { return m_Ports; } }
		public Dictionary<int, eCombineMode> Sources { get { return m_Sources; } }
		public Dictionary<int, eCombineMode> Destinations { get { return m_Destinations; } }
		public Dictionary<int, eCombineMode> SourceGroups { get { return m_SourceGroups; } }
		public Dictionary<int, eCombineMode> DestinationGroups { get { return m_DestinationGroups; } }
		public Dictionary<int, eCombineMode> VolumePoints { get { return m_VolumePoints; } }
		public Dictionary<int, eCombineMode> ConferencePoints { get { return m_ConferencePoints; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractRoomSettings()
		{
			m_Devices = new Dictionary<int, eCombineMode>();
			m_Ports = new Dictionary<int, eCombineMode>();
			m_Sources = new Dictionary<int, eCombineMode>();
			m_Destinations = new Dictionary<int, eCombineMode>();
			m_SourceGroups = new Dictionary<int, eCombineMode>();
			m_DestinationGroups = new Dictionary<int, eCombineMode>();
			m_VolumePoints = new Dictionary<int, eCombineMode>();
			m_ConferencePoints = new Dictionary<int, eCombineMode>();
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(COMBINE_PRIORITY_ELEMENT, IcdXmlConvert.ToString(CombinePriority));

			WriteChildrenToXml(writer, m_Ports, PORTS_ELEMENT, PORT_ELEMENT);
			WriteChildrenToXml(writer, m_Devices, DEVICES_ELEMENT, DEVICE_ELEMENT);
			WriteChildrenToXml(writer, m_Sources, SOURCES_ELEMENT, SOURCE_ELEMENT);
			WriteChildrenToXml(writer, m_Destinations, DESTINATIONS_ELEMENT, DESTINATION_ELEMENT);
			WriteChildrenToXml(writer, m_SourceGroups, SOURCE_GROUPS_ELEMENT, SOURCE_GROUP_ELEMENT);
			WriteChildrenToXml(writer, m_DestinationGroups, DESTINATION_GROUPS_ELEMENT, DESTINATION_GROUP_ELEMENT);
			WriteChildrenToXml(writer, m_VolumePoints, VOLUME_POINTS_ELEMENT, VOLUME_POINT_ELEMENT);
			WriteChildrenToXml(writer, m_ConferencePoints, CONFERENCE_POINTS_ELEMENT, CONFERENCE_POINT_ELEMENT);
        }

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			CombinePriority = XmlUtils.TryReadChildElementContentAsInt(xml, COMBINE_PRIORITY_ELEMENT) ?? 0;

// ReSharper disable CSharpWarnings::CS0612
			// Backwards compatibility
			IEnumerable<KeyValuePair<int, eCombineMode>> panels = ReadListFromXml(xml, PANELS_ELEMENT, PANEL_ELEMENT);
// ReSharper restore CSharpWarnings::CS0612
			IEnumerable<KeyValuePair<int, eCombineMode>> ports = ReadListFromXml(xml, PORTS_ELEMENT, PORT_ELEMENT);
			IEnumerable<KeyValuePair<int, eCombineMode>> devices = ReadListFromXml(xml, DEVICES_ELEMENT, DEVICE_ELEMENT);
			IEnumerable<KeyValuePair<int, eCombineMode>> sources = ReadListFromXml(xml, SOURCES_ELEMENT, SOURCE_ELEMENT);
			IEnumerable<KeyValuePair<int, eCombineMode>> destinations = ReadListFromXml(xml, DESTINATIONS_ELEMENT, DESTINATION_ELEMENT);
			IEnumerable<KeyValuePair<int, eCombineMode>> sourceGroups = ReadListFromXml(xml, SOURCE_GROUPS_ELEMENT, SOURCE_GROUP_ELEMENT);
			IEnumerable<KeyValuePair<int, eCombineMode>> destinationGroups = ReadListFromXml(xml, DESTINATION_GROUPS_ELEMENT, DESTINATION_GROUP_ELEMENT);
			IEnumerable<KeyValuePair<int, eCombineMode>> volumePoints = ReadListFromXml(xml, VOLUME_POINTS_ELEMENT, VOLUME_POINT_ELEMENT);
			IEnumerable<KeyValuePair<int, eCombineMode>> conferencePoints = ReadListFromXml(xml, CONFERENCE_POINTS_ELEMENT, CONFERENCE_POINT_ELEMENT);

			devices = devices.Concat(panels);

			Ports.Clear();
			Devices.Clear();
			Sources.Clear();
			Destinations.Clear();
			SourceGroups.Clear();
			DestinationGroups.Clear();
			VolumePoints.Clear();
			ConferencePoints.Clear();

			Ports.Update(ports);
			Devices.Update(devices);
			Sources.Update(sources);
			Destinations.Update(destinations);
			SourceGroups.Update(sourceGroups);
			DestinationGroups.Update(destinationGroups);
			VolumePoints.Update(volumePoints);
			ConferencePoints.Update(conferencePoints);
		}

		private void WriteChildrenToXml(IcdXmlTextWriter writer, Dictionary<int, eCombineMode> children, string listElement, string childElement)
		{
			XmlUtils.WriteListToXml(writer, children.OrderByKey(), listElement, (w, kvp) => WriteChildToXml(w, kvp, childElement));
		}

		private static void WriteChildToXml(IcdXmlTextWriter writer, KeyValuePair<int, eCombineMode> kvp, string childElement)
		{
			writer.WriteStartElement(childElement);
			{
				writer.WriteAttributeString(COMBINE_ATTRIBUTE, kvp.Value.ToString());
				writer.WriteString(IcdXmlConvert.ToString(kvp.Key));
			}
			writer.WriteEndElement();
		}

		private static IEnumerable<KeyValuePair<int, eCombineMode>> ReadListFromXml(string xml, string listElement, string childElement)
		{
			return XmlUtils.ReadListFromXml<KeyValuePair<int, eCombineMode>>(xml, listElement, childElement, ReadChildFromXml);
		}

		private static KeyValuePair<int, eCombineMode> ReadChildFromXml(string xml)
		{
			string attribute =
				XmlUtils.HasAttribute(xml, COMBINE_ATTRIBUTE)
					? XmlUtils.GetAttribute(xml, COMBINE_ATTRIBUTE)
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
