namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	public static class CommercialRoomTelemetryNames
	{
		public const string SEAT_COUNT = "SeatCount";

		// Privacy Mute
		public const string MUTE_PRIVACY_CHANGED = "PrivacyMuteChanged";
		public const string MUTE_PRIVACY = "PrivacyMute";
		public const string MUTE_PRIVACY_COMMAND = "RoomPrivacyMuteCommand";

		// Active conference device
		public const string ACTIVE_CONFERENCE_DEVICE = "ActiveConferenceDevice";
		public const string ACTIVE_CONFERENCE_DEVICE_CHANGED = "ActiveConferenceDeviceChanged";
	}
}