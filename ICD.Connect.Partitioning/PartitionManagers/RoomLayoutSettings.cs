using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Xml;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public sealed class RoomLayoutSettings
	{
		private const string ROWS_ELEMENT = "Rows";
		private const string ROW_ELEMENT = "Row";
		private const string ROOM_ELEMENT = "Room";

		private const string INDEX_ATTRIBUTE = "index";

		private readonly IcdOrderedDictionary<int, IcdOrderedDictionary<int, int>> m_Layout;

		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomLayoutSettings()
		{
			m_Layout = new IcdOrderedDictionary<int, IcdOrderedDictionary<int, int>>();
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
			return m_Layout.SelectMany(row => row.Value.Select(column => new RoomLayoutInfo(column.Key, row.Key, column.Value)))
			               .ToArray();
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

			foreach (RoomLayoutInfo info in layoutInfo)
				AddRoom(info);
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
				writer.WriteStartElement(ROWS_ELEMENT);
				{
					foreach (KeyValuePair<int, IcdOrderedDictionary<int, int>> row in m_Layout)
					{
						writer.WriteStartElement(ROW_ELEMENT);
						writer.WriteAttributeString(INDEX_ATTRIBUTE, IcdXmlConvert.ToString(row.Key));
						{
							foreach (KeyValuePair<int, int> column in row.Value)
							{
								writer.WriteStartElement(ROOM_ELEMENT);
								writer.WriteAttributeString(INDEX_ATTRIBUTE, IcdXmlConvert.ToString(column.Key));
								writer.WriteString(IcdXmlConvert.ToString(column.Value));
								writer.WriteEndElement();
							}
						}
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

			string rowsXml;
			if (!XmlUtils.TryGetChildElementAsString(xml, ROWS_ELEMENT, out rowsXml))
				return;

			foreach (string rowXml in XmlUtils.GetChildElementsAsString(rowsXml, ROW_ELEMENT))
			{
				int rowIndex = XmlUtils.GetAttributeAsInt(rowXml, INDEX_ATTRIBUTE);

				foreach (string roomXml in XmlUtils.GetChildElementsAsString(rowXml, ROOM_ELEMENT))
				{
					int columnIndex = XmlUtils.GetAttributeAsInt(roomXml, INDEX_ATTRIBUTE);
					int roomId = XmlUtils.ReadElementContentAsInt(roomXml);

					RoomLayoutInfo info = new RoomLayoutInfo(columnIndex, rowIndex, roomId);
					AddRoom(info);
				}
			}
		}

		#endregion

		private void AddRoom(RoomLayoutInfo info)
		{
			IcdOrderedDictionary<int, int> columns;
			if (!m_Layout.TryGetValue(info.Position.Row, out columns))
			{
				columns = new IcdOrderedDictionary<int, int>();
				m_Layout[info.Position.Row] = columns;
			}

			columns[info.Position.Column] = info.RoomId;
		}
	}
}