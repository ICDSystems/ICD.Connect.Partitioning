using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.Partitions
{
	public abstract class AbstractPartitionSettings : AbstractSettings, IPartitionSettings
	{
		private const string PARTITION_ELEMENT = "Partition";

		private const string PARTITION_CONTROLS_ELEMENT = "PartitionControls";
		private const string PARTITION_CONTROL_ELEMENT = "PartitionControl";
		private const string ROOMS_ELEMENT = "Rooms";
		private const string ROOM_ELEMENT = "Room";

		private IEnumerable<int> m_Dependencies;

		private readonly IcdHashSet<int> m_Rooms;
		private readonly IcdHashSet<DeviceControlInfo> m_Controls;
		private readonly SafeCriticalSection m_Section;

		/// <summary>
		/// Gets the xml element.
		/// </summary>
		protected override string Element { get { return PARTITION_ELEMENT; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractPartitionSettings()
		{
			m_Rooms = new IcdHashSet<int>();
			m_Controls = new IcdHashSet<DeviceControlInfo>();
			m_Section = new SafeCriticalSection();
		}

		/// <summary>
		/// Sets the rooms that are adjacent to the partition.
		/// </summary>
		/// <param name="roomIds"></param>
		public void SetRooms(IEnumerable<int> roomIds)
		{
			m_Section.Enter();

			try
			{
				m_Rooms.Clear();
				m_Rooms.AddRange(roomIds);
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Sets the controls associated with this partition.
		/// </summary>
		/// <param name="partitionControls"></param>
		public void SetPartitionControls(IEnumerable<DeviceControlInfo> partitionControls)
		{
			m_Section.Enter();

			try
			{
				m_Controls.Clear();
				m_Controls.AddRange(partitionControls);
				m_Dependencies = m_Section.Execute(() => m_Controls.Select(c => c.DeviceId).Distinct());
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Returns the rooms that are added as adjacent to the partition.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<int> GetRooms()
		{
			return m_Section.Execute(() => m_Rooms.ToArray());
		}

		/// <summary>
		/// Returns the controls that are associated with thr
		/// </summary>
		/// <returns></returns>
		public IEnumerable<DeviceControlInfo> GetPartitionControls()
		{
			return m_Section.Execute(() => m_Controls.ToArray());
		}

		/// <summary>
		/// Returns true if the settings depend on a device with the given ID.
		/// For example, to instantiate an IR Port from settings, the device the physical port
		/// belongs to will need to be instantiated first.
		/// </summary>
		/// <returns></returns>
		public override bool HasDeviceDependency(int id)
		{
			return m_Dependencies.Contains(id);
		}

		/// <summary>
		/// Returns the count from the collection of ids that the settings depends on.
		/// </summary>
		public override int DependencyCount { get { return m_Dependencies.Count(); } }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			XmlUtils.WriteListToXml(writer, GetPartitionControls(), PARTITION_CONTROLS_ELEMENT,
			                        (w, d) => d.WriteToXml(w, PARTITION_CONTROL_ELEMENT));
			XmlUtils.WriteListToXml(writer, GetRooms(), ROOMS_ELEMENT, ROOM_ELEMENT);
		}

		/// <summary>
		/// Parses the xml and configures the settings instance.
		/// </summary>c =
		/// <param name="instance"></param>
		/// <param name="xml"></param>
		protected static void ParseXml(AbstractPartitionSettings instance, string xml)
		{
			IEnumerable<int> roomIds = XmlUtils.ReadListFromXml(xml, ROOMS_ELEMENT, ROOM_ELEMENT,
			                                                    x => XmlUtils.ReadElementContentAsInt(x));
			IEnumerable<DeviceControlInfo> partitionControls =
				XmlUtils.ReadListFromXml(xml, PARTITION_CONTROLS_ELEMENT, PARTITION_CONTROL_ELEMENT,
				                         e => DeviceControlInfo.ReadFromXml(e));

			// Migration
			int? deviceId = XmlUtils.TryReadChildElementContentAsInt(xml, "Device");
			int? controlId = XmlUtils.TryReadChildElementContentAsInt(xml, "Control");

			if (deviceId.HasValue || controlId.HasValue)
			{
				DeviceControlInfo deviceControl = new DeviceControlInfo(deviceId ?? 0, controlId ?? 0);
				partitionControls = partitionControls.Append(deviceControl);
			}

			instance.SetPartitionControls(partitionControls);
			instance.SetRooms(roomIds);

			ParseXml((AbstractSettings)instance, xml);
		}
	}
}
