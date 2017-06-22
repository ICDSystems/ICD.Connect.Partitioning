using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Rooms
{
	public sealed class RoomSettings : AbstractRoomSettings
	{
		private const string FACTORY_NAME = "Room";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Creates a new originator instance from the settings.
		/// </summary>
		/// <param name="factory"></param>
		/// <returns></returns>
		public override IOriginator ToOriginator(IDeviceFactory factory)
		{
			Room output = new Room();
			output.ApplySettings(this, factory);
			return output;
		}

		/// <summary>
		/// Instantiates room settings from an xml element.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[XmlRoomSettingsFactoryMethod(FACTORY_NAME)]
		public static RoomSettings FromXml(string xml)
		{
			RoomSettings output = new RoomSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
