using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
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

			IParticipant lastAudioCall = sources.Where(s => s.CallType.HasFlag(eCallType.Audio))
			                                    .OrderByDescending(s => s.Start)
			                                    .FirstOrDefault();
			IParticipant lastVideoCall = sources.Where(s => s.CallType.HasFlag(eCallType.Video))
			                                    .OrderByDescending(s => s.Start)
			                                    .FirstOrDefault();

			eVolumeType type = eVolumeType.Room;

			bool inAudioCall = lastAudioCall != null;
			bool inVideoCall = lastVideoCall != null;

			if (inAudioCall)
				type |= eVolumeType.Atc;

			if (inVideoCall)
				type |= eVolumeType.Vtc;

			// Otherwise return Room and Program volume points
			IVolumePoint[] points = extends.Originators
			                               .GetInstancesRecursive<IVolumePoint>(p => EnumUtils.HasAnyFlags(p.VolumeType,
			                                                                                               type))
			                               .ToArray();

			return points.OrderBy(p => p, new VolumeContextComparer(type));
		}

		/// <summary>
		/// Comparer which determines the greater volume point based on contextual priority using flags.
		/// </summary>
		private sealed class VolumeContextComparer : IComparer<IVolumePoint>
		{
			private readonly eVolumeType m_Context;

			public VolumeContextComparer(eVolumeType context)
			{
				m_Context = context;
			}

			public int Compare(IVolumePoint x, IVolumePoint y)
			{
				foreach (eVolumeType flag in EnumUtils.GetFlagsExceptNone(m_Context))
				{
					if (x.VolumeType.HasFlag(flag))
						return 1;

					if (y.VolumeType.HasFlag(flag))
						return -1;
				}

				return 0;
			}
		}
	}
}
