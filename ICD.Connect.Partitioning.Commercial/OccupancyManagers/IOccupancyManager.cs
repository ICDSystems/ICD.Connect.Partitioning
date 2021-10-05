using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;

namespace ICD.Connect.Partitioning.Commercial.OccupancyManagers
{
	public interface IOccupancyManager
	{
		/// <summary>
		/// Raised when the supported features change
		/// </summary>
		
		event EventHandler<GenericEventArgs<eOccupancyFeatures>> OnSupportedFeaturesChanged;

		/// <summary>
		/// Triggered when the rooms occupancy state changes
		/// </summary>
		event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupancyStateChanged;

		/// <summary>
		/// Raised when the number of people counted in the room changes
		/// </summary>
		event EventHandler<IntEventArgs> OnPeopleCountChanged;


		/// <summary>
		/// Get what features are supported by controls in the room
		/// </summary>
		eOccupancyFeatures SupportedFeatures { get; }

		/// <summary>
		/// Occupancy state of the room
		/// </summary>
		eOccupancyState OccupancyState { get; }

		/// <summary>
		/// Number of people counted in the room
		/// </summary>
		int PeopleCount { get; }

		/// <summary>
		/// The UTC time of the last occupancy state change
		/// </summary>
		DateTime OccupancyChangeTime { get; }
	}
}
