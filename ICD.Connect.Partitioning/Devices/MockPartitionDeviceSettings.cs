using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Partitioning.Devices
{
	public sealed class MockPartitionDeviceSettings : AbstractPartitionDeviceSettings
	{
		private const string FACTORY_NAME = "MockPartition";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(MockPartitionDevice); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static MockPartitionDeviceSettings FromXml(string xml)
		{
			MockPartitionDeviceSettings output = new MockPartitionDeviceSettings();
			output.ParseXml(xml);
			return output;
		}
	}
}
