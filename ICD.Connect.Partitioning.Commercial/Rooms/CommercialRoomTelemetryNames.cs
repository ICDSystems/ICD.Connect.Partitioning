namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	public static class CommercialRoomTelemetryNames
	{
		// Awake
		public const string IS_AWAKE = "IsAwake";
		public const string IS_AWAKE_CHANGED = "IsAwakeChanged";
		public const string SLEEP_COMMAND = "SleepCommand";

		// Seat Count
		public const string SEAT_COUNT = "SeatCount";

		// Occupied
		public const string OCCUPIED_CHANGED = "OccupiedChanged";
		public const string OCCUPIED = "Occupied";

		// Privacy Mute
		public const string MUTE_PRIVACY_CHANGED = "PrivacyMuteChanged";
		public const string MUTE_PRIVACY = "PrivacyMute";
		public const string MUTE_PRIVACY_COMMAND = "RoomPrivacyMuteCommand";

		// Active conference device
		public const string ACTIVE_CONFERENCE_DEVICE = "ActiveConferenceDevice";
		public const string ACTIVE_CONFERENCE_DEVICE_CHANGED = "ActiveConferenceDeviceChanged";

		// Call-in Info
		public const string CALL_IN_INFO = "CallInInfo";
		public const string CALL_IN_INFO_CHANGED = "CallInInfoChanged";

		// Bookings
		public const string BOOKINGS = "Bookings";
		public const string BOOKINGS_CHANGED = "BookingsChanged";

		// RoomType
		public const string ROOM_TYPE = "RoomType";
		public const string ROOM_TYPE_CHANGED = "RoomTypeChanged";

		// IsInMeeting
		public const string IS_IN_MEETING = "IsInMeeting";
		public const string IS_IN_MEETING_CHANGED = "IsInMeetingChanged";
	}
}
