using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Debounce;

namespace ICD.Connect.Partitioning.Commercial.Controls.Occupancy
{
	public interface IOccupancySensorControl : IDeviceControl
	{
		
		/// <summary>
		/// Raised when the supported features change
		/// </summary>
		[EventTelemetry(OccupancyTelemetryNames.SUPPORTED_FEATURES_CHANGED)]
		event EventHandler<GenericEventArgs<eOccupancyFeatures>> OnSupportedFeaturesChanged;

		/// <summary>
		/// Triggered when the occupancy state changes
		/// </summary>
		[EventTelemetry(OccupancyTelemetryNames.OCCUPANCY_STATE_CHANGED,
			DebounceMode = eDebounceMode.RisingEdge,
			DebounceInterval = 10 * 1000,
			DebounceLowValue = eOccupancyState.Unoccupied)]
		event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupancyStateChanged;

		/// <summary>
		/// Raised when the number of people counted by the sensor changes
		/// </summary>
		[EventTelemetry(OccupancyTelemetryNames.PEOPLE_COUNT_CHANGED,
		                DebounceMode = eDebounceMode.All,
		                DebounceInterval = 10 * 1000)]
		event EventHandler<IntEventArgs> OnPeopleCountChanged;


		/// <summary>
		/// Get what features are supported by this control
		/// </summary>
		[PropertyTelemetry(OccupancyTelemetryNames.SUPPORTED_FEATURES, null, OccupancyTelemetryNames.SUPPORTED_FEATURES_CHANGED)]
		eOccupancyFeatures SupportedFeatures { get; }

		/// <summary>
		/// State of the occupancy sensor
		/// </summary>
		[PropertyTelemetry(OccupancyTelemetryNames.OCCUPANCY_STATE, null, OccupancyTelemetryNames.OCCUPANCY_STATE_CHANGED)]
		eOccupancyState OccupancyState { get; }

		/// <summary>
		/// Number of people counted by this sensor
		/// </summary>
		[PropertyTelemetry(OccupancyTelemetryNames.PEOPLE_COUNT, null, OccupancyTelemetryNames.PEOPLE_COUNT_CHANGED)]
		int PeopleCount { get; }

		
	}
}
