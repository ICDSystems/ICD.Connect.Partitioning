using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;

namespace ICD.Connect.Settings
{
	/// <summary>
	/// Base class for room settings.
	/// </summary>
	[PublicAPI]
	public abstract class AbstractRoomSettings : AbstractSettings, IRoomSettings
	{
		public const string ROOM_ELEMENT = "Room";

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

		private readonly IcdHashSet<int> m_Devices;
		private readonly IcdHashSet<int> m_Ports;
		private readonly IcdHashSet<int> m_Panels;
		private readonly IcdHashSet<int> m_Sources;
		private readonly IcdHashSet<int> m_Destinations;
		private readonly IcdHashSet<int> m_DestinationGroups;

		#region Properties

		public IcdHashSet<int> Devices { get { return m_Devices; } }
		public IcdHashSet<int> Ports { get { return m_Ports; } }
		public IcdHashSet<int> Panels { get { return m_Panels; } }
		public IcdHashSet<int> Sources { get { return m_Sources; } }
		public IcdHashSet<int> Destinations { get { return m_Destinations; } }
		public IcdHashSet<int> DestinationGroups { get { return m_DestinationGroups; } }

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
			m_Devices = new IcdHashSet<int>();
			m_Ports = new IcdHashSet<int>();
			m_Panels = new IcdHashSet<int>();
			m_Sources = new IcdHashSet<int>();
			m_Destinations = new IcdHashSet<int>();
			m_DestinationGroups = new IcdHashSet<int>();
		}

		#region Methods

		/// <summary>
		/// Returns the collection of ids that the settings will depend on.
		/// For example, to instantiate an IR Port from settings, the device the physical port
		/// belongs to will need to be instantiated first.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<int> GetDeviceDependencies()
		{
			return m_Devices;
		}

		#endregion

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			ListToXml(writer, DEVICES_ELEMENT, DEVICE_ELEMENT, Devices.Order());
			ListToXml(writer, PANELS_ELEMENT, PANEL_ELEMENT, Panels.Order());
			ListToXml(writer, PORTS_ELEMENT, PORT_ELEMENT, Ports.Order());
			ListToXml(writer, SOURCES_ELEMENT, SOURCE_ELEMENT, Sources.Order());
			ListToXml(writer, DESTINATIONS_ELEMENT, DESTINATION_ELEMENT, Destinations.Order());
			ListToXml(writer, DESTINATION_GROUPS_ELEMENT, DESTINATION_GROUP_ELEMENT, DestinationGroups.Order());
		}

		private static void ListToXml(IcdXmlTextWriter writer, string parentElement, string childElement,
		                              IEnumerable<int> list)
		{
			writer.WriteStartElement(parentElement);
			{
				foreach (int item in list)
					writer.WriteElementString(childElement, item.ToString());
			}
			writer.WriteEndElement();
		}

		#region Protected Methods

		/// <summary>
		/// Parses the xml and applies the properties to the instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="xml"></param>
		protected static void ParseXml(AbstractRoomSettings instance, string xml)
		{
			AbstractSettings.ParseXml(instance, xml);

			IEnumerable<string> panels = GetListElementsFromXml(xml, PANELS_ELEMENT, PANEL_ELEMENT);
			IEnumerable<string> ports = GetListElementsFromXml(xml, PORTS_ELEMENT, PORT_ELEMENT);
			IEnumerable<string> devices = GetListElementsFromXml(xml, DEVICES_ELEMENT, DEVICE_ELEMENT);
			IEnumerable<string> sources = GetListElementsFromXml(xml, SOURCES_ELEMENT, SOURCE_ELEMENT);
			IEnumerable<string> destinations = GetListElementsFromXml(xml, DESTINATIONS_ELEMENT, DESTINATION_ELEMENT);
			IEnumerable<string> destinationGroups = GetListElementsFromXml(xml, DESTINATION_GROUPS_ELEMENT,
			                                                               DESTINATION_GROUP_ELEMENT);

			instance.Panels.AddRange(panels.Select(p => XmlUtils.ReadElementContentAsInt(p)));
			instance.Ports.AddRange(ports.Select(p => XmlUtils.ReadElementContentAsInt(p)));
			instance.Devices.AddRange(devices.Select(d => XmlUtils.ReadElementContentAsInt(d)));
			instance.Sources.AddRange(sources.Select(s => XmlUtils.ReadElementContentAsInt(s)));
			instance.Destinations.AddRange(destinations.Select(d => XmlUtils.ReadElementContentAsInt(d)));
			instance.DestinationGroups.AddRange(destinationGroups.Select(d => XmlUtils.ReadElementContentAsInt(d)));
		}

		private static IEnumerable<string> GetListElementsFromXml(string xml, string listElement, string itemElement)
		{
			string child;
			return XmlUtils.TryGetChildElementAsString(xml, listElement, out child)
				       ? XmlUtils.GetChildElementsAsString(child, itemElement)
				       : Enumerable.Empty<string>();
		}

		#endregion
	}
}
