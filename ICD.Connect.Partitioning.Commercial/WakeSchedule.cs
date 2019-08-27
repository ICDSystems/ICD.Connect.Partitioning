using System;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Scheduler;
using ICD.Common.Utils.Xml;

namespace ICD.Connect.Partitioning.Commercial
{
	public sealed class WakeSchedule : AbstractScheduledAction
	{
		public event EventHandler OnWakeActionRequested;
		public event EventHandler OnSleepActionRequested;

		private const string WEEKDAY_ELEMENT = "Weekday";
		private const string WEEKEND_ELEMENT = "Weekend";

		private const string WAKE_ELEMENT = "Wake";
		private const string SLEEP_ELEMENT = "Sleep";
		private const string ENABLE_ELEMENT = "Enable";

		private TimeSpan? m_WeekdayWakeTime;
		private TimeSpan? m_WeekdaySleepTime;
		private TimeSpan? m_WeekendWakeTime;
		private TimeSpan? m_WeekendSleepTime;

		private bool m_WeekdayEnable;
		private bool m_WeekendEnable;

		private bool? m_IsAwake;
		
		#region Properties

		/// <summary>
		/// Gets/sets the Weekday Wake Time.
		/// </summary>
		public TimeSpan? WeekdayWakeTime
		{
			get { return m_WeekdayWakeTime; }
			set
			{
				if (value != null)
				{
					int hour = value.Value.Hours % 24;
					value = new TimeSpan(hour, value.Value.Minutes, value.Value.Seconds);
				}
				
				if (m_WeekdayWakeTime == value)
					return;

				m_WeekdayWakeTime = value;
				UpdateNextRunTime();
			}
		}

		/// <summary>
		/// Gets/sets the Weekday Sleep Time.
		/// </summary>
		public TimeSpan? WeekdaySleepTime
		{
			get { return m_WeekdaySleepTime; }
			set
			{
				if (value != null)
				{
					int hour = value.Value.Hours % 24;
					value = new TimeSpan(hour, value.Value.Minutes, value.Value.Seconds);
				}

				if (m_WeekdaySleepTime == value)
					return;

				m_WeekdaySleepTime = value;
				UpdateNextRunTime();
			}
		}

		/// <summary>
		/// Gets/sets the Weekend Wake Time.
		/// </summary>
		public TimeSpan? WeekendWakeTime
		{
			get { return m_WeekendWakeTime; }
			set
			{
				if (value != null)
				{
					int hour = value.Value.Hours % 24;
					value = new TimeSpan(hour, value.Value.Minutes, value.Value.Seconds);
				}

				if (m_WeekendWakeTime == value)
					return;

				m_WeekendWakeTime = value;
				UpdateNextRunTime();
			}
		}

		/// <summary>
		/// Gets/sets the Weekend Sleep Time.
		/// </summary>
		public TimeSpan? WeekendSleepTime
		{
			get { return m_WeekendSleepTime; }
			set
			{
				if (value != null)
				{
					int hour = value.Value.Hours % 24;
					value = new TimeSpan(hour, value.Value.Minutes, value.Value.Seconds);
				}

				if (m_WeekendSleepTime == value)
					return;

				m_WeekendSleepTime = value;
				UpdateNextRunTime();
			}
		}

		/// <summary>
		/// Enables/disables the weekday wake/sleep schedule.
		/// </summary>
		public bool WeekdayEnable
		{
			get { return m_WeekdayEnable; }
			set
			{
				if (m_WeekdayEnable == value)
					return;

				m_WeekdayEnable = value;
				UpdateNextRunTime();
			}
		}

		/// <summary>
		/// Enables/disables the weekend wake/sleep schedule.
		/// </summary>
		public bool WeekendEnable
		{
			get { return m_WeekendEnable; }
			set
			{
				if (m_WeekendEnable == value)
					return;

				m_WeekendEnable = value;
				UpdateNextRunTime();
			}
		}

		public bool IsAwake
		{
			get
			{
				if (m_IsAwake == null)
				{
					var recentWake = GetMostRecentWakeTime() ?? DateTime.MinValue;
					var recentSleep = GetMostRecentSleepTime() ?? DateTime.MinValue;
					// If recent wake/sleep are the same, they are both unconfigured or the config is invalid, default to awake.
					m_IsAwake = recentWake == recentSleep || recentWake > recentSleep;
				}

				return m_IsAwake.Value;
			}
			private set { m_IsAwake = value; }
		}

		#endregion

		#region Methods

		public override void RunFinal()
		{
			var now = IcdEnvironment.GetLocalTime();
			var wakeTime = GetWakeTimeForDay(now.Date);
			var sleepTime = GetSleepTimeForDay(now.Date);

			if (sleepTime != null && wakeTime != null)
			{
				var time = now.PreviousLatestTime(true, sleepTime.Value, wakeTime.Value);
				if (time != null && time == sleepTime.Value)
					Sleep();
				else if (time != null && time == wakeTime.Value)
					Wake();
			}
			else if (wakeTime != null && now >= wakeTime)
				Wake();
			else if (sleepTime != null && now >= sleepTime)
				Sleep();
		}

		public override DateTime? GetNextRunTime()
		{
			var now = IcdEnvironment.GetLocalTime();
			var currentDay = now.Date;
			while (currentDay < now.AddDays(7)) // if no action found for a week, means all 4 times are null
			{
				var sleepTime = GetSleepTimeForDay(currentDay);
				var wakeTime = GetWakeTimeForDay(currentDay);

				if (sleepTime != null && wakeTime != null)
				{
					var time = now.NextEarliestTime(false, sleepTime.Value, wakeTime.Value);
					if (time != null)
						return time;
				}
				else if (wakeTime != null && now < wakeTime)
					return wakeTime.Value;
				else if (sleepTime != null && now < sleepTime)
					return sleepTime.Value;

				currentDay = currentDay.AddDays(1); // no actions scheduled for today, check next day
			}

			return null;
		}

		/// <summary>
		/// Clears the stored times.
		/// </summary>
		public void Clear()
		{
			WeekdayWakeTime = null;
			WeekdaySleepTime = null;
			WeekendWakeTime = null;
			WeekendSleepTime = null;

			WeekdayEnable = false;
			WeekendEnable = false;
		}

		/// <summary>
		/// Copies the properties from the other instance.
		/// </summary>
		/// <param name="other"></param>
		public void Copy(WakeSchedule other)
		{
			if (other == null)
				throw new ArgumentNullException("other");

			WeekdayWakeTime = other.WeekdayWakeTime;
			WeekdaySleepTime = other.WeekdaySleepTime;
			WeekendWakeTime = other.WeekendWakeTime;
			WeekendSleepTime = other.WeekendSleepTime;
			
			WeekdayEnable = other.WeekdayEnable;
			WeekendEnable = other.WeekendEnable;
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public void ParseXml(string xml)
		{
			Clear();

			string weekdayXml;
			if (XmlUtils.TryGetChildElementAsString(xml, WEEKDAY_ELEMENT, out weekdayXml))
				ParseWeekday(weekdayXml);

			string weekendXml;
			if (XmlUtils.TryGetChildElementAsString(xml, WEEKEND_ELEMENT, out weekendXml))
				ParseWeekend(weekendXml);
		}

		/// <summary>
		/// Writes the settings to xml.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="element"></param>
		public void WriteElements(IcdXmlTextWriter writer, string element)
		{
			writer.WriteStartElement(element);
			{
				writer.WriteStartElement(WEEKDAY_ELEMENT);
				{
					writer.WriteElementString(WAKE_ELEMENT, WeekdayWakeTime == null ? null : WeekdayWakeTime.ToString());
					writer.WriteElementString(SLEEP_ELEMENT, WeekdaySleepTime == null ? null : WeekdaySleepTime.ToString());
					writer.WriteElementString(ENABLE_ELEMENT, IcdXmlConvert.ToString(WeekdayEnable));
				}
				writer.WriteEndElement();

				writer.WriteStartElement(WEEKEND_ELEMENT);
				{
					writer.WriteElementString(WAKE_ELEMENT, WeekendWakeTime == null ? null : WeekendWakeTime.ToString());
					writer.WriteElementString(SLEEP_ELEMENT, WeekendSleepTime == null ? null : WeekendSleepTime.ToString());
					writer.WriteElementString(ENABLE_ELEMENT, IcdXmlConvert.ToString(WeekendEnable));
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Updates the weekday settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		private void ParseWeekday(string xml)
		{
			string wakeXml;
			if (XmlUtils.TryGetChildElementAsString(xml, WAKE_ELEMENT, out wakeXml))
				WeekdayWakeTime = ParseTimeSpan(wakeXml);

			string sleepXml;
			if (XmlUtils.TryGetChildElementAsString(xml, SLEEP_ELEMENT, out sleepXml))
				WeekdaySleepTime = ParseTimeSpan(sleepXml);

			WeekdayEnable = XmlUtils.TryReadChildElementContentAsBoolean(xml, ENABLE_ELEMENT) ?? false;
		}

		/// <summary>
		/// Updates the weekend settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		private void ParseWeekend(string xml)
		{
			string wakeXml;
			if (XmlUtils.TryGetChildElementAsString(xml, WAKE_ELEMENT, out wakeXml))
				WeekendWakeTime = ParseTimeSpan(wakeXml);

			string sleepXml;
			if (XmlUtils.TryGetChildElementAsString(xml, SLEEP_ELEMENT, out sleepXml))
				WeekendSleepTime = ParseTimeSpan(sleepXml);

			WeekendEnable = XmlUtils.TryReadChildElementContentAsBoolean(xml, ENABLE_ELEMENT) ?? false;
		}

		/// <summary>
		/// Parses the xml content as a TimeSpan.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		private static TimeSpan? ParseTimeSpan(string xml)
		{
			string content = XmlUtils.ReadElementContent(xml);
			if (string.IsNullOrEmpty(content))
				return null;

			return TimeSpan.Parse(content);
		}

		private DateTime? GetSleepTimeForDay(DateTime day)
		{
			day = day.Date; // remove any time info
			if (day.DayOfWeek.IsWeekday() && WeekdayEnable && WeekdaySleepTime != null)
				return day.Add(WeekdaySleepTime.Value);
			if (day.DayOfWeek.IsWeekend() && WeekendEnable && WeekendSleepTime != null)
				return day.Add(WeekendSleepTime.Value);

			return null; // should not sleep today
		}

		private DateTime? GetWakeTimeForDay(DateTime day)
		{
			day = day.Date; // remove any time info
			if (day.DayOfWeek.IsWeekday() && WeekdayEnable && WeekdayWakeTime != null)
				return day.Add(WeekdayWakeTime.Value);
			if (day.DayOfWeek.IsWeekend() && WeekendEnable && WeekendWakeTime != null)
				return day.Add(WeekendWakeTime.Value);

			return null; // should not wake today
		}

		private DateTime? GetMostRecentWakeTime()
		{
			var day = DateTime.Today;
			var now = DateTime.Now;

			// if no wake time is found in the past 7 days, the system never auto-wakes
			for (int i = 0; i < 7; i++)
			{
				var recentWakeTime = GetWakeTimeForDay(day);
				if (recentWakeTime < now)
					return recentWakeTime;
			}
			return null;
		}

		private DateTime? GetMostRecentSleepTime()
		{
			var day = DateTime.Today;
			var now = DateTime.Now;

			// if no sleep time is found in the past 7 days, the system never auto-sleeps
			for (int i = 0; i < 7; i++)
			{
				var recentSleepTime = GetSleepTimeForDay(day);
				if (recentSleepTime < now)
					return recentSleepTime;
			}
			return null;
		}

		private void Wake()
		{
			IsAwake = true;
			OnWakeActionRequested.Raise(this);
		}

		private void Sleep()
		{
			IsAwake = false;
			OnSleepActionRequested.Raise(this);
		}

		#endregion
	}
}
