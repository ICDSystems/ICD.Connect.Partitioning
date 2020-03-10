using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Scheduler;
using ICD.Common.Utils.Xml;

namespace ICD.Connect.Partitioning.Commercial
{
	public sealed class WakeSchedule : AbstractScheduledAction
	{
		/// <summary>
		/// Raised when the wake action occurs.
		/// </summary>
		public event EventHandler OnWakeActionRequested;

		/// <summary>
		/// Raised when the sleep action occurs.
		/// </summary>
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

		#region Properties

		/// <summary>
		/// Gets/sets the Weekday Wake Time.
		/// </summary>
		public TimeSpan? WeekdayWakeTime
		{
			get { return m_WeekdayWakeTime; }
			set
			{
				value = Wrap24Hours(value);
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
				value = Wrap24Hours(value);
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
				value = Wrap24Hours(value);
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
				value = Wrap24Hours(value);
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

		/// <summary>
		/// Returns if the wake schedule is enabled for operation today.
		/// </summary>
		public bool IsEnabledToday
		{
			get
			{
				return IcdEnvironment.GetLocalTime().DayOfWeek.IsWeekday()
					? WeekdayEnable
					: WeekendEnable;
			}
		}

		/// <summary>
		/// Returns true if the system should be awake at the current time.
		/// </summary>
		public bool IsWakeTime
		{
			get
			{
				DateTime now = IcdEnvironment.GetLocalTime();
				return GetIsWakeTime(now);
			}
		}

		/// <summary>
		/// Returns true if the system should be asleep at the current time.
		/// </summary>
		public bool IsSleepTime
		{
			get
			{
				DateTime now = IcdEnvironment.GetLocalTime();
				return GetIsSleepTime(now);
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Runs when the action has hit its scheduled time
		/// </summary>
		public override void RunFinal()
		{
			if (IsSleepTime)
				OnSleepActionRequested.Raise(this);
			else if (IsWakeTime)
				OnWakeActionRequested.Raise(this);
		}

		/// <summary>
		/// Runs after RunFinal in order to determine the next run time of this action
		/// </summary>
		public override DateTime? GetNextRunTimeUtc()
		{
			var now = IcdEnvironment.GetUtcTime();

			bool? unused;
			return GetNextRunTimeUtc(now, out unused);
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

		#endregion

		#region Private Methods

		/// <summary>
		/// Wraps the given time to a 24 hour span.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static TimeSpan? Wrap24Hours(TimeSpan? value)
		{
			if (value == null)
				return null;

			int hour = MathUtils.Modulus(value.Value.Hours, 24);
			return new TimeSpan(hour, value.Value.Minutes, value.Value.Seconds);
		}

		/// <summary>
		/// Gets the run time following the given time.
		/// </summary>
		/// <param name="nowUtc"></param>
		/// <param name="wake">Outputs true if the result is a wake time, false if the result is a sleep time.</param>
		/// <returns></returns>
		private DateTime? GetNextRunTimeUtc(DateTime nowUtc, out bool? wake)
		{
			DateTime nowLocal = nowUtc.ToLocalTime();
			DateTime currentDayLocal = nowLocal.Date;

			// If no action found for a week, means all 4 times are null
			while (currentDayLocal < nowLocal.AddDays(7))
			{
				List<DateTime> times = new List<DateTime>();

				DateTime? sleepTime = GetSleepTimeForDay(currentDayLocal);
				if (sleepTime.HasValue)
					times.Add(sleepTime.Value);

				DateTime? wakeTime = GetWakeTimeForDay(currentDayLocal);
				if (wakeTime.HasValue)
					times.Add(wakeTime.Value);

				DateTime? time = nowLocal.NextEarliestTime(false, times);
				if (time.HasValue)
				{
					wake = time == wakeTime;
					return time.Value.ToUniversalTime();
				}

				// No actions scheduled for today, check next day
				currentDayLocal = currentDayLocal.AddDays(1);
			}

			wake = null;
			return null;
		}

		/// <summary>
		/// Gets the run time preceding the given time.
		/// </summary>
		/// <param name="now"></param>
		/// <param name="wake">Outputs true if the result is a wake time.</param>
		/// <returns></returns>
		private DateTime? GetLastRunTimeInclusive(DateTime now, out bool? wake)
		{
			DateTime currentDay = now.Date;

			// If no action found for a week, means all 4 times are null
			while (currentDay > now.AddDays(-7))
			{
				List<DateTime> times = new List<DateTime>();

				DateTime? sleepTime = GetSleepTimeForDay(currentDay);
				if (sleepTime.HasValue)
					times.Add(sleepTime.Value);

				DateTime? wakeTime = GetWakeTimeForDay(currentDay);
				if (wakeTime.HasValue)
					times.Add(wakeTime.Value);

				DateTime? time = now.PreviousLatestTime(true, times);
				if (time != null)
				{
					wake = time == wakeTime;
					return time;
				}

				// No actions scheduled for today, check next day
				currentDay = currentDay.AddDays(-1);
			}

			wake = null;
			return null;
		}

		/// <summary>
		/// Returns true if the system should be awake at the given time.
		/// </summary>
		private bool GetIsWakeTime(DateTime now)
		{
			bool? wake;
			return GetLastRunTimeInclusive(now, out wake).HasValue && wake == true;
		}

		/// <summary>
		/// Returns true if the system should be asleep at the given time.
		/// </summary>
		private bool GetIsSleepTime(DateTime now)
		{
			bool? wake;
			return GetLastRunTimeInclusive(now, out wake).HasValue && wake == false;
		}

		/// <summary>
		/// Returns the sleep time for the given day.
		/// </summary>
		/// <param name="day"></param>
		/// <returns></returns>
		private DateTime? GetSleepTimeForDay(DateTime day)
		{
			// Remove any time info
			day = day.Date;

			if (day.DayOfWeek.IsWeekday() && WeekdayEnable && WeekdaySleepTime != null)
				return day.Add(WeekdaySleepTime.Value);
			if (day.DayOfWeek.IsWeekend() && WeekendEnable && WeekendSleepTime != null)
				return day.Add(WeekendSleepTime.Value);

			// Should not sleep today
			return null;
		}

		/// <summary>
		/// Returns the wake time for the given day.
		/// </summary>
		/// <param name="day"></param>
		/// <returns></returns>
		private DateTime? GetWakeTimeForDay(DateTime day)
		{
			// Remove any time info
			day = day.Date;

			if (day.DayOfWeek.IsWeekday() && WeekdayEnable && WeekdayWakeTime != null)
				return day.Add(WeekdayWakeTime.Value);
			if (day.DayOfWeek.IsWeekend() && WeekendEnable && WeekendWakeTime != null)
				return day.Add(WeekendWakeTime.Value);

			// Should not wake today
			return null;
		}

		#endregion

		#region Serialization

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
		/// Updates the weekday settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		private void ParseWeekday(string xml)
		{
			WeekdayWakeTime = XmlUtils.TryReadChildElementContentAsTimeSpan(xml, WAKE_ELEMENT);
			WeekdaySleepTime = XmlUtils.TryReadChildElementContentAsTimeSpan(xml, SLEEP_ELEMENT);
			WeekdayEnable = XmlUtils.TryReadChildElementContentAsBoolean(xml, ENABLE_ELEMENT) ?? false;
		}

		/// <summary>
		/// Updates the weekend settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		private void ParseWeekend(string xml)
		{
			WeekendWakeTime = XmlUtils.TryReadChildElementContentAsTimeSpan(xml, WAKE_ELEMENT);
			WeekendSleepTime = XmlUtils.TryReadChildElementContentAsTimeSpan(xml, SLEEP_ELEMENT);
			WeekendEnable = XmlUtils.TryReadChildElementContentAsBoolean(xml, ENABLE_ELEMENT) ?? false;
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
					writer.WriteElementString(WAKE_ELEMENT, IcdXmlConvert.ToString(WeekdayWakeTime));
					writer.WriteElementString(SLEEP_ELEMENT, IcdXmlConvert.ToString(WeekdaySleepTime));
					writer.WriteElementString(ENABLE_ELEMENT, IcdXmlConvert.ToString(WeekdayEnable));
				}
				writer.WriteEndElement();

				writer.WriteStartElement(WEEKEND_ELEMENT);
				{
					writer.WriteElementString(WAKE_ELEMENT, IcdXmlConvert.ToString(WeekendWakeTime));
					writer.WriteElementString(SLEEP_ELEMENT, IcdXmlConvert.ToString(WeekendSleepTime));
					writer.WriteElementString(ENABLE_ELEMENT, IcdXmlConvert.ToString(WeekendEnable));
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		#endregion
	}
}
