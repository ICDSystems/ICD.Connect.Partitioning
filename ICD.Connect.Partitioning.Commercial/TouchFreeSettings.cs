using ICD.Common.Utils.Xml;

namespace ICD.Connect.Partitioning.Commercial
{
	public sealed class TouchFreeSettings
	{
		private const string COUNTDOWN_SECONDS_ELEMENT = "Countdown";
		private const string ENABLED_ELEMENT = "Enabled";
		private const string SOURCE_ELEMENT = "Source";

		public int CountdownSeconds { get; set; }

		public bool Enabled { get; set; }

		public int? SourceId { get; set; }

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public void ParseXml(string xml)
		{
			CountdownSeconds = XmlUtils.TryReadChildElementContentAsInt(xml, COUNTDOWN_SECONDS_ELEMENT) ?? 0;
			Enabled = XmlUtils.TryReadChildElementContentAsBoolean(xml, ENABLED_ELEMENT) ?? false;
			SourceId = XmlUtils.TryReadChildElementContentAsInt(xml, SOURCE_ELEMENT);
		}

		/// <summary>
		/// Writes the settings to xml.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="element"></param>
		public void WriteElements(IcdXmlTextWriter writer, string element)
		{
			writer.WriteStartElement(element);
			{
				writer.WriteElementString(COUNTDOWN_SECONDS_ELEMENT, IcdXmlConvert.ToString(CountdownSeconds));
				writer.WriteElementString(ENABLED_ELEMENT, IcdXmlConvert.ToString(Enabled));
				writer.WriteElementString(SOURCE_ELEMENT, IcdXmlConvert.ToString(SourceId));
			}
			writer.WriteEndElement();
		}
	}
}
