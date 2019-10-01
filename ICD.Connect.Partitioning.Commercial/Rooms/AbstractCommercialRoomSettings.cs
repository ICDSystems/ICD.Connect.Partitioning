using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Partitioning.Commercial.Rooms
{
	public abstract class AbstractCommercialRoomSettings : AbstractRoomSettings, ICommercialRoomSettings
	{
		private const string WAKE_SCHEDULE_ELEMENT = "WakeSchedule";
		private const string DIALING_PLAN_ELEMENT = "DialingPlan";

		private readonly WakeSchedule m_WakeScheduleSettings;

		#region Properties

		[HiddenSettingsProperty]
		public WakeSchedule WakeSchedule { get { return m_WakeScheduleSettings; } }

		[UsedImplicitly]
		public bool WeekdayEnable
		{
			get { return m_WakeScheduleSettings.WeekdayEnable; }
			set { m_WakeScheduleSettings.WeekdayEnable = value; }
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
		public bool WeekendEnable
		{
			get { return m_WakeScheduleSettings.WeekendEnable; }
			set { m_WakeScheduleSettings.WeekendEnable = value; }
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

		[PathSettingsProperty("DialingPlans", ".xml")]
		public string DialingPlan { get; set; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractCommercialRoomSettings()
		{
			m_WakeScheduleSettings = new WakeSchedule();
		}
		
		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DIALING_PLAN_ELEMENT, DialingPlan);

			m_WakeScheduleSettings.WriteElements(writer, WAKE_SCHEDULE_ELEMENT);
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

			DialingPlan = XmlUtils.TryReadChildElementContentAsString(xml, DIALING_PLAN_ELEMENT);
		}
	}
}