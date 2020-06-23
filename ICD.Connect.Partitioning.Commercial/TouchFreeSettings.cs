using System;
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
		/// Copies the properties from the other instance.
		/// </summary>
		/// <param name="other"></param>
		public void Copy(TouchFreeSettings other)
		{
			if (other == null)
				throw new ArgumentNullException("other");

			CountdownSeconds = other.CountdownSeconds;
			Enabled = other.Enabled;
			SourceId = other.SourceId;
		}

		public void Clear()
		{
			CountdownSeconds = 0;
			Enabled = false;
		}

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

				writer.WriteElementString(ENABLED_ELEMENT, IcdXmlConvert.ToString(ENABLED_ELEMENT));

				writer.WriteElementString(SOURCE_ELEMENT, IcdXmlConvert.ToString(SOURCE_ELEMENT));
			}
			writer.WriteEndElement();
		}
	}
}
