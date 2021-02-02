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
		/// Triggered when the occupancy state changes
		/// True = occupied
		/// False = unoccupied/vacant
		/// </summary>
		[EventTelemetry(OccupancyTelemetryNames.OCCUPANCY_STATE_CHANGED,
			DebounceMode = eDebounceMode.RisingEdge,
			DebounceInterval = 10 * 1000,
			DebounceLowValue = eOccupancyState.Unoccupied)]
		event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupancyStateChanged;

		/// <summary>
		/// State of the occupancy sensor
		/// True = occupied
		/// False = unoccupied/vacant
		/// </summary>
		[PropertyTelemetry(OccupancyTelemetryNames.OCCUPANCY_STATE, null, OccupancyTelemetryNames.OCCUPANCY_STATE_CHANGED)]
		eOccupancyState OccupancyState { get; }
	}
}
