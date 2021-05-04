using ICD.Common.Utils;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices.Points;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.Commercial.OccupancyPoints
{
	public abstract class AbstractOccupancyPoint<TSettings> : AbstractPoint<TSettings, IOccupancySensorControl>, IOccupancyPoint
		where TSettings : IOccupancyPointSettings, new()
	{
		/// <summary>
		/// Gets the category for this originator type (e.g. Device, Port, etc)
		/// </summary>
		public override string Category { get { return "OccupancyPoint"; } }

		/// <summary>
		/// Masks supported features on the occupancy control
		/// Implementers should take the intersection of SupportedFeatures on the occupancy control and this mask
		/// </summary>
		public eOccupancyFeatures SupportedFeaturesMask { get; private set; }

		protected AbstractOccupancyPoint()
		{
			SupportedFeaturesMask = EnumUtils.GetFlagsAllValue<eOccupancyFeatures>();
		}

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			SupportedFeaturesMask = settings.SupportedFeaturesMask;
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.SupportedFeaturesMask = SupportedFeaturesMask;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			SupportedFeaturesMask = eOccupancyFeatures.None;
		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Supported Features Mask", SupportedFeaturesMask);
		}

		#endregion
	}
}
