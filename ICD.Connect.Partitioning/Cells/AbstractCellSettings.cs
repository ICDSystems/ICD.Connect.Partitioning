using ICD.Common.Utils.Xml;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Partitioning.Cells
{
	public abstract class AbstractCellSettings : AbstractSettings, ICellSettings
	{
		private const string ELEMENT_ROOM = "Room";
		private const string ELEMENT_COLUMN = "Column";
		private const string ELEMENT_ROW = "Row";

		/// <summary>
		/// Gets/sets the id for the room occupying this cell.
		/// </summary>
		[OriginatorIdSettingsProperty(typeof(IRoom))]
		public int? Room { get; set; }

		/// <summary>
		/// Gets the horizontal position of the cell in the grid.
		/// </summary>
		public int Column { get; set; }

		/// <summary>
		/// Gets the vertical position of the cell in the grid.
		/// </summary>
		public int Row { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(ELEMENT_ROOM, IcdXmlConvert.ToString(Room));
			writer.WriteElementString(ELEMENT_COLUMN, IcdXmlConvert.ToString(Column));
			writer.WriteElementString(ELEMENT_ROW, IcdXmlConvert.ToString(Row));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Room = XmlUtils.TryReadChildElementContentAsInt(xml, ELEMENT_ROOM);
			Column = XmlUtils.TryReadChildElementContentAsInt(xml, ELEMENT_COLUMN) ?? 0;
			Row = XmlUtils.TryReadChildElementContentAsInt(xml, ELEMENT_ROW) ?? 0;
		}
	}
}
