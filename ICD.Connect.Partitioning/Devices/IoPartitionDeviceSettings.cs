using ICD.Common.Utils.Xml;
using ICD.Connect.Protocol.Ports.DigitalInput;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Partitioning.Devices
{
	[KrangSettings("IoPartition", typeof(IoPartitionDevice))]
	public sealed class IoPartitionDeviceSettings : AbstractPartitionDeviceSettings
	{
		private const string IO_PORT_ELEMENT = "IoPort";
		private const string INVERT_INPUT_ELEMENT = "InvertInput";

		[OriginatorIdSettingsProperty(typeof(IDigitalInputPort))]
		public int? IoPort { get; set; }

		public bool InvertInput { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(IO_PORT_ELEMENT, IcdXmlConvert.ToString(IoPort));
			writer.WriteElementString(INVERT_INPUT_ELEMENT, IcdXmlConvert.ToString(InvertInput));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			IoPort = XmlUtils.TryReadChildElementContentAsInt(xml, IO_PORT_ELEMENT);
			InvertInput = XmlUtils.TryReadChildElementContentAsBoolean(xml, INVERT_INPUT_ELEMENT) ?? false;
		}
	}
}
