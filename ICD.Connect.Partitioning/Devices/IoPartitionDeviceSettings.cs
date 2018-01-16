using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;
using ICD.Connect.Protocol.Ports.DigitalInput;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Partitioning.Devices
{
	public sealed class IoPartitionDeviceSettings : AbstractPartitionDeviceSettings
	{
		private const string FACTORY_NAME = "IoPartition";

		private const string IO_PORT_ELEMENT = "IoPort";
		private const string INVERT_INPUT_ELEMENT = "InvertInput";

		[OriginatorIdSettingsProperty(typeof(IDigitalInputPort))]
		public int? IoPort { get; set; }

		public bool InvertInput { get; set; }

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(IoPartitionDevice); } }

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
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static IoPartitionDeviceSettings FromXml(string xml)
		{
			IoPartitionDeviceSettings output = new IoPartitionDeviceSettings
			{
				IoPort = XmlUtils.TryReadChildElementContentAsInt(xml, IO_PORT_ELEMENT),
				InvertInput = XmlUtils.TryReadChildElementContentAsBoolean(xml, INVERT_INPUT_ELEMENT) ?? false
			};

			ParseXml(output, xml);
			return output;
		}
	}
}
