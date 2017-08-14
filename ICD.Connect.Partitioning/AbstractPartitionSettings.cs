using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitions
{
	public abstract class AbstractPartitionSettings : AbstractSettings, IPartitionSettings
	{
		private const string PARTITION_ELEMENT = "Partition";

		private const string PARTITION_DEVICE_ELEMENT = "Device";
		private const string PARTITION_CONTROL_ELEMENT = "Control";
		private const string ROOMS_ELEMENT = "Rooms";
		private const string ROOM_ELEMENT = "Room";

		private readonly IcdHashSet<int> m_Rooms; 
		private readonly SafeCriticalSection m_RoomsSection;

		/// <summary>
		/// Gets the xml element.
		/// </summary>
		protected override string Element { get { return PARTITION_ELEMENT; } }

		/// <summary>
		/// Gets/sets the optional device for the partition.
		/// </summary>
		public int Device { get; set; }

		/// <summary>
		/// Gets/sets the optional device control for the partition.
		/// </summary>
		public int Control { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractPartitionSettings()
		{
			m_Rooms = new IcdHashSet<int>();
			m_RoomsSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Sets the rooms that are adjacent to the partition.
		/// </summary>
		/// <param name="roomIds"></param>
		public void SetRooms(IEnumerable<int> roomIds)
		{
			m_RoomsSection.Enter();

			try
			{
				m_Rooms.Clear();
				m_Rooms.AddRange(roomIds);
			}
			finally
			{
				m_RoomsSection.Leave();
			}
		}

		/// <summary>
		/// Returns the rooms that are added as adjacent to the partition.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<int> GetRooms()
		{
			return m_RoomsSection.Execute(() => m_Rooms.ToArray());
		}

		/// <summary>
		/// Returns the collection of ids that the settings will depend on.
		/// For example, to instantiate an IR Port from settings, the device the physical port
		/// belongs to will need to be instantiated first.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<int> GetDeviceDependencies()
		{
			if (Device != 0)
				yield return Device;
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(PARTITION_DEVICE_ELEMENT, IcdXmlConvert.ToString(Device));
			writer.WriteElementString(PARTITION_CONTROL_ELEMENT, IcdXmlConvert.ToString(Control));

			XmlUtils.WriteListToXml(writer, GetRooms(), ROOMS_ELEMENT, ROOM_ELEMENT);
		}

		/// <summary>
		/// Parses the xml and configures the settings instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="xml"></param>
		protected static void ParseXml(AbstractPartitionSettings instance, string xml)
		{
			IEnumerable<int> roomIds = XmlUtils.ReadListFromXml(xml, ROOMS_ELEMENT, ROOM_ELEMENT,
																x => XmlUtils.ReadElementContentAsInt(x));

			instance.Device = XmlUtils.TryReadChildElementContentAsInt(xml, PARTITION_DEVICE_ELEMENT) ?? 0;
			instance.Control = XmlUtils.TryReadChildElementContentAsInt(xml, PARTITION_CONTROL_ELEMENT) ?? 0;
			instance.SetRooms(roomIds);

			ParseXml((AbstractSettings)instance, xml);
		}
	}
}
