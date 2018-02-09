using System;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class PartitionManagerSettings : AbstractPartitionManagerSettings
	{
		private const string FACTORY_NAME = "PartitionManager";

		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(PartitionManager); } }
	}
}
