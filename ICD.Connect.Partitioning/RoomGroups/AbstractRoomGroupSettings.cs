using System.Collections.Generic;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.RoomGroups
{
	public abstract class AbstractRoomGroupSettings : AbstractSettings, IRoomGroupSettings
	{
		private const string ROOMS_ELEMENT = "Rooms";
		private const string ROOM_ELEMENT = "Room";

		public List<int> RoomIds { get; private set; }

		protected AbstractRoomGroupSettings()
		{
			RoomIds = new List<int>();
		}

		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			IEnumerable<int> roomIds = XmlUtils.ReadListFromXml<int>(xml, ROOMS_ELEMENT, ROOM_ELEMENT);

			RoomIds.Clear();

			RoomIds.AddRange(roomIds);
		}

		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			XmlUtils.WriteListToXml(writer, RoomIds, ROOMS_ELEMENT, ROOM_ELEMENT);
		}
	}
}