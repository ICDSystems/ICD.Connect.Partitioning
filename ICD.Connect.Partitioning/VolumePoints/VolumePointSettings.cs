using ICD.Connect.Settings.Attributes;
using System;

namespace ICD.Connect.Partitioning.VolumePoints
{
    [KrangSettings(FACTORY_NAME)]
    public class VolumePointSettings : AbstractVolumePointSettings
    {
        private const string FACTORY_NAME = "VolumePoint";

        public override string FactoryName { get { return FACTORY_NAME; } }

        public override Type OriginatorType { get { return typeof(VolumePoint); } }
    }
}
