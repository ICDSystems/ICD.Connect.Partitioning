using System;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Conferencing.ConferenceManagers;
using ICD.Connect.Partitioning.Rooms;

namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	public interface ICommercialRoom : IRoom
	{
		event EventHandler<GenericEventArgs<IConferenceManager>> OnConferenceManagerChanged;

		/// <summary>
		/// Gets the wake/sleep schedule.
		/// </summary>
		WakeSchedule WakeSchedule { get; }

		/// <summary>
		/// Gets the path to the loaded dialing plan xml file. Used by fusion :(
		/// </summary>
		string DialingPlan { get; }

		/// <summary>
		/// Gets the conference manager
		/// </summary>
		[CanBeNull]
		IConferenceManager ConferenceManager { get; }
	}
}