using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public sealed class RoomLayoutSettings
	{
		private const string ROOMS_ELEMENT = "Rooms";
		private const string ROOM_ELEMENT = "Room";

		private const string ROW_ATTRIBUTE = "row";
		private const string COLUMN_ATTRIBUTE = "column";

		private readonly BiDictionary<RoomPositionInfo, int> m_Layout;

		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomLayoutSettings()
		{
			m_Layout = new BiDictionary<RoomPositionInfo, int>();
		}

		#region Methods

		/// <summary>
		/// Clears the stored layout info.
		/// </summary>
		public void Clear()
		{
			m_Layout.Clear();
		}

		/// <summary>
		/// Gets the room layout info.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<RoomLayoutInfo> GetRooms()
		{
			return m_Layout.Select(kvp => new RoomLayoutInfo(kvp.Key, kvp.Value)).ToArray(m_Layout.Count);
		}

		/// <summary>
		/// Stores the layout info.
		/// </summary>
		/// <param name="layoutInfo"></param>
		public void SetRooms(IEnumerable<RoomLayoutInfo> layoutInfo)
		{
			if (layoutInfo == null)
				throw new ArgumentNullException("layoutInfo");

			m_Layout.Clear();

			try
			{
				foreach (RoomLayoutInfo info in layoutInfo)
					m_Layout.Add(info.Position, info.RoomId);
			}
			catch (Exception)
			{
				m_Layout.Clear();
				throw;
			}
		}

		/// <summary>
		/// Writes the layout settings to xml.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="element"></param>
		public void ToXml(IcdXmlTextWriter writer, string element)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			writer.WriteStartElement(element);
			{
				writer.WriteStartElement(ROOMS_ELEMENT);
				{
					foreach (KeyValuePair<RoomPositionInfo, int> item in m_Layout.OrderByKey())
					{
						writer.WriteStartElement(ROOM_ELEMENT);
						writer.WriteAttributeString(ROW_ATTRIBUTE, IcdXmlConvert.ToString(item.Key.Row));
						writer.WriteAttributeString(COLUMN_ATTRIBUTE, IcdXmlConvert.ToString(item.Key.Column));
						writer.WriteString(IcdXmlConvert.ToString(item.Value));
						writer.WriteEndElement();
					}
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Reads the layout settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public void ParseXml(string xml)
		{
			Clear();

			string roomsXml;
			if (!XmlUtils.TryGetChildElementAsString(xml, ROOMS_ELEMENT, out roomsXml))
				return;

			foreach (string roomXml in XmlUtils.GetChildElementsAsString(roomsXml, ROOM_ELEMENT))
			{
				int columnIndex = XmlUtils.GetAttributeAsInt(roomXml, COLUMN_ATTRIBUTE);
				int rowIndex = XmlUtils.GetAttributeAsInt(roomXml, ROW_ATTRIBUTE);
				int roomId = XmlUtils.ReadElementContentAsInt(roomXml);

				RoomLayoutInfo info = new RoomLayoutInfo(columnIndex, rowIndex, roomId);
				m_Layout.Add(info.Position, info.RoomId);
			}
		}

		#endregion
	}
}