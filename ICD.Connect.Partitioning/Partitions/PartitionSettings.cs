using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.Connect.Partitions
{
	public sealed class PartitionSettings : AbstractPartitionSettings
	{
		private const string FACTORY_NAME = "Partition";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(Partition); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlPartitionSettingsFactoryMethod(FACTORY_NAME)]
		public static PartitionSettings FromXml(string xml)
		{
			PartitionSettings output = new PartitionSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
