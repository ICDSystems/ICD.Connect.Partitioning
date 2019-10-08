﻿using System;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Conferencing.ConferenceManagers;
using ICD.Connect.Partitioning.Rooms;

namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	public interface ICommercialRoom : IRoom
	{
		/// <summary>
		/// Raised when the conference manager changes.
		/// </summary>
		event EventHandler<GenericEventArgs<IConferenceManager>> OnConferenceManagerChanged;

		/// <summary>
		/// Raised when the wake schedule changes.
		/// </summary>
		event EventHandler<GenericEventArgs<WakeSchedule>> OnWakeScheduleChanged;

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