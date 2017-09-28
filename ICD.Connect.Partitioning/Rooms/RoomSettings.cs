using System;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Partitioning.Rooms
{
	public sealed class RoomSettings : AbstractRoomSettings
	{
		private const string FACTORY_NAME = "Room";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(Room); } }

		/// <summary>
		/// Instantiates room settings from an xml element.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[XmlFactoryMethod(FACTORY_NAME)]
		public static RoomSettings FromXml(string xml)
		{
			RoomSettings output = new RoomSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
