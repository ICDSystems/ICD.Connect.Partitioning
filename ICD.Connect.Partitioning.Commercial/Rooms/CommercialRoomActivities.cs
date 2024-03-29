﻿using System;
using ICD.Common.Logging.Activities;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Conferencing.EventArguments;

namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	public static class CommercialRoomActivities
	{
		/// <summary>
		/// Gets the room call activity for the given call state.
		/// </summary>
		/// <param name="inCall"></param>
		/// <returns></returns>
		public static Activity GetCallActivity(eInCall inCall)
		{
			switch (inCall)
			{
				case eInCall.None:
					return new Activity(Activity.ePriority.Lowest, "In Call", "Not In Call", eSeverity.Informational);
				case eInCall.Audio:
					return new Activity(Activity.ePriority.High, "In Call", "Audio Call", eSeverity.Informational);
				case eInCall.Video:
					return new Activity(Activity.ePriority.High, "In Call", "Video Call", eSeverity.Informational);
				default:
					throw new ArgumentOutOfRangeException("inCall");
			}
		}

		/// <summary>
		/// Gets the room IsAwake activity for the given awake state.
		/// </summary>
		/// <param name="isAwake"></param>
		/// <returns></returns>
		public static Activity GetAwakeActivity(bool isAwake)
		{
			return isAwake
				       ? new Activity(Activity.ePriority.Lowest - 1, "Is Awake", "Idle", eSeverity.Informational)
				       : new Activity(Activity.ePriority.Medium, "Is Awake", "Standby", eSeverity.Informational);
		}
	}
}