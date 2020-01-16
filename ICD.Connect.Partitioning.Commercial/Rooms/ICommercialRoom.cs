using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Audio.VolumePoints;
using ICD.Connect.Conferencing.ConferenceManagers;
using ICD.Connect.Conferencing.Conferences;
using ICD.Connect.Conferencing.EventArguments;
using ICD.Connect.Conferencing.Participants;
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
		/// Raised when the room wakes or goes to sleep.
		/// </summary>
		event EventHandler<BoolEventArgs> OnIsAwakeStateChanged;

		/// <summary>
		/// Gets the wake/sleep schedule.
		/// </summary>
		[CanBeNull]
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

		/// <summary>
		/// Gets the awake state.
		/// </summary>
		bool IsAwake { get; }
	}

	public static class CommercialRoomExtensions
	{
		/// <summary>
		/// Gets the volume points for the current context.
		/// </summary>
		/// <returns></returns>
		[NotNull]
		public static IEnumerable<IVolumePoint> GetContextualVolumePoints([NotNull] this ICommercialRoom extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			// Return ATC/VTC volume points if we are in a call
			IConferenceManager conferenceManager = extends.ConferenceManager;
			IParticipant[] sources =
				conferenceManager == null
					? new IParticipant[0]
					: conferenceManager.ActiveConferences
					                   .SelectMany(c => c.GetOnlineParticipants())
					                   .ToArray();

			IParticipant lastAudioCall = sources.Where(s => s.CallType == eCallType.Audio)
			                                    .OrderByDescending(s => s.Start)
			                                    .FirstOrDefault();
			IParticipant lastVideoCall = sources.Where(s => s.CallType == eCallType.Video)
			                                    .OrderByDescending(s => s.Start)
			                                    .FirstOrDefault();

			bool inAudioCall = lastAudioCall != null;
			bool inVideoCall = lastVideoCall != null;

			if (inAudioCall || inVideoCall)
			{
				eVolumeType type;

				if (inVideoCall && inAudioCall)
					type = lastVideoCall.Start > lastAudioCall.Start ? eVolumeType.Vtc : eVolumeType.Atc;
				else if (inAudioCall)
					type = eVolumeType.Atc;
				else
					type = eVolumeType.Vtc;

				eVolumeType typeCopy = type;
				return extends.Originators.GetInstancesRecursive<IVolumePoint>(p => p.VolumeType == typeCopy);
			}

			// Otherwise return Room and Program volume points
			return extends.Originators
			              .GetInstancesRecursive<IVolumePoint>(p =>
			                                                   {
				                                                   switch (p.VolumeType)
				                                                   {
					                                                   case eVolumeType.Room:
					                                                   case eVolumeType.Program:
						                                                   return true;
				                                                   }

				                                                   return false;
			                                                   });
		}
	}
}
