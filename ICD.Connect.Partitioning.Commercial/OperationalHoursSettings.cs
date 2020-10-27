using System;
using ICD.Common.Utils;
using ICD.Common.Utils.Xml;

namespace ICD.Connect.Partitioning.Commercial
{
	public sealed class OperationalHoursSettings
	{
		private const string START_TIME_ELEMENT = "StartTime";
		private const string END_TIME_ELEMENT = "EndTime";
		private const string DAYS_OF_WEEK_ELEMENT = "DaysOfWeek";

		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }
		public eDaysOfWeek Days { get; set; }

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public void ParseXml(string xml)
		{
			StartTime = XmlUtils.TryReadChildElementContentAsTimeSpan(xml, START_TIME_ELEMENT) ?? TimeSpan.Zero;
			EndTime = XmlUtils.TryReadChildElementContentAsTimeSpan(xml, END_TIME_ELEMENT) ?? TimeSpan.Zero;
			Days = XmlUtils.TryReadChildElementContentAsEnum<eDaysOfWeek>(xml, DAYS_OF_WEEK_ELEMENT, true) ?? eDaysOfWeek.None;
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
				writer.WriteElementString(START_TIME_ELEMENT, IcdXmlConvert.ToString(StartTime));
				writer.WriteElementString(END_TIME_ELEMENT, IcdXmlConvert.ToString(EndTime));
				writer.WriteElementString(DAYS_OF_WEEK_ELEMENT, IcdXmlConvert.ToString(Days));
			}
			writer.WriteEndElement();
		}

	}
}