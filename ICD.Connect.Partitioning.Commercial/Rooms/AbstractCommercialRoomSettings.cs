using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	public abstract class AbstractCommercialRoomSettings : AbstractRoomSettings, ICommercialRoomSettings
	{
		private const string CONFERENCE_POINTS_ELEMENT = "ConferencePoints";
		private const string CONFERENCE_POINT_ELEMENT = "ConferencePoint";
		private const string CALENDAR_POINTS_ELEMENT = "CalendarPoints";
		private const string CALENDAR_POINT_ELEMENT = "CalendarPoint";
		private const string OCCUPANCY_POINTS_ELEMENT = "OccupancyPoints";
		private const string OCCUPANCY_POINT_ELEMENT = "OccupancyPoint";

		private const string SEAT_COUNT_ELEMENT = "SeatCount";
		private const string WAKE_SCHEDULE_ELEMENT = "WakeSchedule";
		private const string DIALING_PLAN_ELEMENT = "DialingPlan";
		private const string TOUCH_FREE_ELEMENT = "TouchFree";

		private readonly WakeSchedule m_WakeScheduleSettings;
		private readonly TouchFreeSettings m_TouchFreeSettings;
		private readonly Dictionary<int, eCombineMode> m_ConferencePoints;
		private readonly Dictionary<int, eCombineMode> m_CalendarPoints;
		private readonly Dictionary<int, eCombineMode> m_OccupancyPoints;

		#region Properties

		public Dictionary<int, eCombineMode> ConferencePoints { get { return m_ConferencePoints; } }
		public Dictionary<int, eCombineMode> CalendarPoints { get { return m_CalendarPoints; } }
		public Dictionary<int, eCombineMode> OccupancyPoints {get { return m_OccupancyPoints; }}
		[HiddenSettingsProperty]
		public WakeSchedule WakeSchedule { get { return m_WakeScheduleSettings; } }
		[HiddenSettingsProperty]
		public TouchFreeSettings TouchFree { get { return m_TouchFreeSettings; } }

		[UsedImplicitly]
		public bool WeekdayEnableWake
		{
			get { return m_WakeScheduleSettings.WeekdayEnableWake; }
			set { m_WakeScheduleSettings.WeekdayEnableWake = value; }
		}

		[UsedImplicitly]
		public bool WeekdayEnableSleep
		{
			get { return m_WakeScheduleSettings.WeekdayEnableSleep; }
			set { m_WakeScheduleSettings.WeekdayEnableSleep = value; }
		}

		[UsedImplicitly]
		public TimeSpan? WeekdayWakeTime
		{
			get { return m_WakeScheduleSettings.WeekdayWakeTime; }
			set { m_WakeScheduleSettings.WeekdayWakeTime = value; }
		}

		[UsedImplicitly]
		public TimeSpan? WeekdaySleepTime
		{
			get { return m_WakeScheduleSettings.WeekdaySleepTime; }
			set { m_WakeScheduleSettings.WeekdaySleepTime = value; }
		}

		[UsedImplicitly]
		public bool WeekendEnableWake
		{
			get { return m_WakeScheduleSettings.WeekendEnableWake; }
			set { m_WakeScheduleSettings.WeekendEnableWake = value; }
		}

		[UsedImplicitly]
		public bool WeekendEnable
		{
			get { return m_WakeScheduleSettings.WeekendEnableSleep; }
			set { m_WakeScheduleSettings.WeekendEnableSleep = value; }
		}

		[UsedImplicitly]
		public TimeSpan? WeekendWakeTime
		{
			get { return m_WakeScheduleSettings.WeekendWakeTime; }
			set { m_WakeScheduleSettings.WeekendWakeTime = value; }
		}

		[UsedImplicitly]
		public TimeSpan? WeekendSleepTime
		{
			get { return m_WakeScheduleSettings.WeekendSleepTime; }
			set { m_WakeScheduleSettings.WeekendSleepTime = value; }
		}

		[UsedImplicitly]
		public int TouchFreeCountdown
		{
			get { return m_TouchFreeSettings.CountdownSeconds; } 
			set { m_TouchFreeSettings.CountdownSeconds = value; }
		}

		[UsedImplicitly]
		public bool TouchFreeEnable
		{
			get { return m_TouchFreeSettings.Enabled; }
			set { m_TouchFreeSettings.Enabled = value;}
		}

		[UsedImplicitly]
		[OriginatorIdSettingsProperty(typeof(ISource))]
		public int? TouchFreeSource
		{
			get { return m_TouchFreeSettings.SourceId; }
			set { m_TouchFreeSettings.SourceId = value; }
		}

		[PathSettingsProperty("DialingPlans", ".xml")]
		public string DialingPlan { get; set; }

		[HiddenSettingsProperty]
		public int SeatCount { get; set; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractCommercialRoomSettings()
		{
			m_WakeScheduleSettings = new WakeSchedule();
			m_TouchFreeSettings = new TouchFreeSettings();

			m_ConferencePoints = new Dictionary<int, eCombineMode>();
			m_CalendarPoints = new Dictionary<int, eCombineMode>();
			m_OccupancyPoints = new Dictionary<int, eCombineMode>();
		}
		
		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			WriteChildrenToXml(writer, m_ConferencePoints, CONFERENCE_POINTS_ELEMENT, CONFERENCE_POINT_ELEMENT);
			WriteChildrenToXml(writer, m_CalendarPoints, CALENDAR_POINTS_ELEMENT, CALENDAR_POINT_ELEMENT);
			WriteChildrenToXml(writer, m_OccupancyPoints, OCCUPANCY_POINTS_ELEMENT, OCCUPANCY_POINT_ELEMENT);

			writer.WriteElementString(SEAT_COUNT_ELEMENT, IcdXmlConvert.ToString(SeatCount));
			writer.WriteElementString(DIALING_PLAN_ELEMENT, DialingPlan);

			m_WakeScheduleSettings.WriteElements(writer, WAKE_SCHEDULE_ELEMENT);
			m_TouchFreeSettings.WriteElements(writer, TOUCH_FREE_ELEMENT);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			string wakeScheduleXml;
			if (XmlUtils.TryGetChildElementAsString(xml, WAKE_SCHEDULE_ELEMENT, out wakeScheduleXml))
				m_WakeScheduleSettings.ParseXml(wakeScheduleXml);

			string touchFreeXml;
			if (XmlUtils.TryGetChildElementAsString(xml, TOUCH_FREE_ELEMENT, out touchFreeXml))
				m_TouchFreeSettings.ParseXml(touchFreeXml);

			DialingPlan = XmlUtils.TryReadChildElementContentAsString(xml, DIALING_PLAN_ELEMENT);
			SeatCount = XmlUtils.TryReadChildElementContentAsInt(xml, SEAT_COUNT_ELEMENT) ?? 0;

			IEnumerable<KeyValuePair<int, eCombineMode>> conferencePoints = ReadListFromXml(xml, CONFERENCE_POINTS_ELEMENT, CONFERENCE_POINT_ELEMENT);
			IEnumerable<KeyValuePair<int, eCombineMode>> calendarPoints = ReadListFromXml(xml, CALENDAR_POINTS_ELEMENT, CALENDAR_POINT_ELEMENT);
			IEnumerable<KeyValuePair<int, eCombineMode>> occupancyPoints = ReadListFromXml(xml, OCCUPANCY_POINTS_ELEMENT,OCCUPANCY_POINT_ELEMENT);

			ConferencePoints.Clear();
			CalendarPoints.Clear();
			OccupancyPoints.Clear();

			ConferencePoints.Update(conferencePoints);
			CalendarPoints.Update(calendarPoints);
			OccupancyPoints.Update(occupancyPoints);
		}
	}
}