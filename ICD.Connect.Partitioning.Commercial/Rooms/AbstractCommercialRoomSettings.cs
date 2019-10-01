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

		private DialingPlanInfo m_DialingPlan;

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

		[UsedImplicitly]
		public DialingPlanInfo DialingPlan
		{
			get { return m_DialingPlan; }
			set { m_DialingPlan = value; }
		}

		protected AbstractCommercialRoomSettings()
		{
			m_WakeScheduleSettings = new WakeSchedule();
			m_DialingPlan = new DialingPlanInfo();
		}
		
		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			m_DialingPlan.WriteToXml(writer, DIALING_PLAN_ELEMENT);

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

			string dialingPlan;
			XmlUtils.TryGetChildElementAsString(xml, DIALING_PLAN_ELEMENT, out dialingPlan);

			m_DialingPlan = string.IsNullOrEmpty(dialingPlan)
							  ? new DialingPlanInfo()
							  : DialingPlanInfo.FromXml(dialingPlan);
		}
	}
}