using System.Collections.Generic;
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

			XmlUtils.WriteListToXml(writer, Devices.Order(), DEVICES_ELEMENT, DEVICE_ELEMENT);
			XmlUtils.WriteListToXml(writer, Panels.Order(), PANELS_ELEMENT, PANEL_ELEMENT);
			XmlUtils.WriteListToXml(writer, Ports.Order(), PORTS_ELEMENT, PORT_ELEMENT);
			XmlUtils.WriteListToXml(writer, Sources.Order(), SOURCES_ELEMENT, SOURCE_ELEMENT);
			XmlUtils.WriteListToXml(writer, Destinations.Order(), DESTINATIONS_ELEMENT, DESTINATION_ELEMENT);
			XmlUtils.WriteListToXml(writer, DestinationGroups.Order(), DESTINATION_GROUPS_ELEMENT, DESTINATION_GROUP_ELEMENT);
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

			IEnumerable<int> panels = XmlUtils.ReadListFromXml(xml, PANELS_ELEMENT, PANEL_ELEMENT, content => XmlUtils.ReadElementContentAsInt(content));
			IEnumerable<int> ports = XmlUtils.ReadListFromXml(xml, PORTS_ELEMENT, PORT_ELEMENT, content => XmlUtils.ReadElementContentAsInt(content));
			IEnumerable<int> devices = XmlUtils.ReadListFromXml(xml, DEVICES_ELEMENT, DEVICE_ELEMENT, content => XmlUtils.ReadElementContentAsInt(content));
			IEnumerable<int> sources = XmlUtils.ReadListFromXml(xml, SOURCES_ELEMENT, SOURCE_ELEMENT, content => XmlUtils.ReadElementContentAsInt(content));
			IEnumerable<int> destinations = XmlUtils.ReadListFromXml(xml, DESTINATIONS_ELEMENT, DESTINATION_ELEMENT, content => XmlUtils.ReadElementContentAsInt(content));
			IEnumerable<int> destinationGroups = XmlUtils.ReadListFromXml(xml, DESTINATION_GROUPS_ELEMENT, DESTINATION_GROUP_ELEMENT, content => XmlUtils.ReadElementContentAsInt(content));

			instance.Panels.AddRange(panels);
			instance.Ports.AddRange(ports);
			instance.Devices.AddRange(devices);
			instance.Sources.AddRange(sources);
			instance.Destinations.AddRange(destinations);
			instance.DestinationGroups.AddRange(destinationGroups);
		}

		#endregion
	}
}
