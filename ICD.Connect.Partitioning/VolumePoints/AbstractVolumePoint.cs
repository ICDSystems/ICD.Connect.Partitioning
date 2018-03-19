using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Partitioning.VolumePoints
{
    /// <summary>
    /// Used by the metlife room to better manage volume controls.
    /// </summary>
    public abstract class AbstractVolumePoint<TSettings> : AbstractOriginator<TSettings>, IVolumePoint
        where TSettings : IVolumePointSettings, new()
    {
        private int m_DeviceId;
        private int? m_ControlId;

        #region Properties

        /// <summary>
        /// Device id
        /// </summary>
        public int DeviceId { get { return m_DeviceId; } }

        /// <summary>
        /// Control id.
        /// </summary>
        public int? ControlId { get { return m_ControlId; } }

        #endregion

        #region Settings

        protected override void CopySettingsFinal(TSettings settings)
        {
            base.CopySettingsFinal(settings);

            settings.DeviceId = m_DeviceId;
            settings.ControlId = m_ControlId;
        }

        protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
        {
            base.ApplySettingsFinal(settings, factory);

            m_DeviceId = settings.DeviceId;
            m_ControlId = settings.ControlId;
        }

        protected override void ClearSettingsFinal()
        {
            base.ClearSettingsFinal();

            m_DeviceId = 0;
            m_ControlId = null;
        }

        #endregion
    }
}
