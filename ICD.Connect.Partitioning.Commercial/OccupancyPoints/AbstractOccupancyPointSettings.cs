using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices.Points;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;

namespace ICD.Connect.Partitioning.Commercial.OccupancyPoints
{
	public abstract class AbstractOccupancyPointSettings : AbstractPointSettings, IOccupancyPointSettings
	{
		private const string SUPPORTED_FEATURES_MASK_ELEMENT = "SupportedFeaturesMask";

		public eOccupancyFeatures SupportedFeaturesMask { get; set; }

		/// <summary>
		/// Instantiate volume point settings from an xml element
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			SupportedFeaturesMask = XmlUtils.TryReadChildElementContentAsEnum<eOccupancyFeatures>(xml, SUPPORTED_FEATURES_MASK_ELEMENT, true) 
			                        ?? EnumUtils.GetFlagsAllValue<eOccupancyFeatures>();
		}

		/// <summary>
		/// Write property elements to xml
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(SUPPORTED_FEATURES_MASK_ELEMENT, IcdXmlConvert.ToString(SupportedFeaturesMask));
		}
	}
}