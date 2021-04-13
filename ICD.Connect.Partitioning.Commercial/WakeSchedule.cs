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
		/// Returns null if the action was performed without defferral, otherwise a period of time to wait before trying again.
		/// </summary>
		/// <returns></returns>
		public delegate TimeSpan? DeferRuntime();

		private const string WEEKDAY_ELEMENT = "Weekday";
		private const string WEEKEND_ELEMENT = "Weekend";

		private const string WAKE_ELEMENT = "Wake";
		private const string SLEEP_ELEMENT = "Sleep";
		private const string ENABLE_WAKE_ELEMENT = "EnableWake";
		private const string ENABLE_SLEEP_ELEMENT = "EnableSleep";

		[Obsolete("Use Enable Wake and Enable Sleep")]
		private const string ENABLE_ELEMENT = "Enable";

		private TimeSpan? m_WeekdayWakeTime;
		private TimeSpan? m_WeekdaySleepTime;
		private TimeSpan? m_WeekendWakeTime;
		private TimeSpan? m_WeekendSleepTime;

		private bool m_WeekdayEnableWake;
		private bool m_WeekendEnableWake;
		private bool m_WeekdayEnableSleep;
		private bool m_WeekendEnableSleep;

		#region Properties

		/// <summary>
		/// Gets/sets the wake action callback.
		/// </summary>
		public DeferRuntime WakeActionRequested { get; set; }

		/// <summary>
		/// Gets/sets the sleep action callback.
		/// </summary>
		public DeferRuntime SleepActionRequested { get; set; }

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
		/// Enables/disables the weekday wake schedule.
		/// </summary>
		public bool WeekdayEnableWake
		{
			get { return m_WeekdayEnableWake; }
			set
			{
				if (m_WeekdayEnableWake == value)
					return;
				m_WeekdayEnableWake = value;

				UpdateNextRunTime();
			}
		}

		/// <summary>
		/// Enables/disables the weekend wake schedule.
		/// </summary>
		public bool WeekendEnableWake
		{
			get { return m_WeekendEnableWake; }
			set
			{
				if (m_WeekendEnableWake == value)
					return;
				m_WeekendEnableWake = value;

				UpdateNextRunTime();
			}
		}

		/// <summary>
		/// Enables/disables the weekday sleep schedule.
		/// </summary>
		public bool WeekdayEnableSleep
		{
			get { return m_WeekdayEnableSleep; }
			set
			{
				if (m_WeekdayEnableSleep == value)
					return;
				m_WeekdayEnableSleep = value;

				UpdateNextRunTime();
			}
		}

		/// <summary>
		/// Enables/disables the weekend sleep schedule.
		/// </summary>
		public bool WeekendEnableSleep
		{
			get { return m_WeekendEnableSleep; }
			set
			{
				if (m_WeekendEnableSleep == value)
					return;
				m_WeekendEnableSleep = value;

				UpdateNextRunTime();
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
		protected override DateTime? RunFinal()
		{
			TimeSpan? deferral = null;

			if (IsSleepTime)
				deferral = SleepActionRequested();
			else if (IsWakeTime)
				deferral = WakeActionRequested();

			return deferral.HasValue
				? IcdEnvironment.GetUtcTime() + deferral.Value
				: GetNextRunTimeUtc();
		}

		/// <summary>
		/// Runs after RunFinal in order to determine the next run time of this action
		/// </summary>
		protected override DateTime? GetNextRunTimeUtc()
		{
			DateTime now = IcdEnvironment.GetUtcTime();

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

			WeekdayEnableWake = false;
			WeekendEnableWake = false;
			WeekdayEnableSleep = false;
			WeekendEnableSleep = false;
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
			value = value == null
				? (TimeSpan?)null
				: new TimeSpan(0,
				               value.Value.Hours,
				               value.Value.Minutes,
				               value.Value.Seconds,
				               value.Value.Milliseconds);

			return value == null ? (TimeSpan?)null : value.Value.AddHoursAndWrap(0);
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

			if (day.DayOfWeek.IsWeekday() && WeekdayEnableSleep && WeekdaySleepTime != null)
				return day.Add(WeekdaySleepTime.Value);
			if (day.DayOfWeek.IsWeekend() && WeekendEnableSleep && WeekendSleepTime != null)
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

			if (day.DayOfWeek.IsWeekday() && WeekdayEnableWake && WeekdayWakeTime != null)
				return day.Add(WeekdayWakeTime.Value);
			if (day.DayOfWeek.IsWeekend() && WeekendEnableWake && WeekendWakeTime != null)
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

			WeekdayEnableWake = other.WeekdayEnableWake;
			WeekdayEnableSleep = other.WeekdayEnableSleep;
			WeekendEnableWake = other.WeekendEnableWake;
			WeekendEnableSleep = other.WeekendEnableSleep;
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

			// Backwards compatibility
			bool enable = XmlUtils.TryReadChildElementContentAsBoolean(xml, ENABLE_ELEMENT) ?? false;

			WeekdayEnableWake = XmlUtils.TryReadChildElementContentAsBoolean(xml, ENABLE_WAKE_ELEMENT) ?? enable;
			WeekdayEnableSleep = XmlUtils.TryReadChildElementContentAsBoolean(xml, ENABLE_SLEEP_ELEMENT) ?? enable;
		}

		/// <summary>
		/// Updates the weekend settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		private void ParseWeekend(string xml)
		{
			WeekendWakeTime = XmlUtils.TryReadChildElementContentAsTimeSpan(xml, WAKE_ELEMENT);
			WeekendSleepTime = XmlUtils.TryReadChildElementContentAsTimeSpan(xml, SLEEP_ELEMENT);

			// Backwards compatibility
			bool enable = XmlUtils.TryReadChildElementContentAsBoolean(xml, ENABLE_ELEMENT) ?? false;

			WeekendEnableWake = XmlUtils.TryReadChildElementContentAsBoolean(xml, ENABLE_WAKE_ELEMENT) ?? enable;
			WeekendEnableSleep = XmlUtils.TryReadChildElementContentAsBoolean(xml, ENABLE_SLEEP_ELEMENT) ?? enable;
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
					writer.WriteElementString(ENABLE_WAKE_ELEMENT, IcdXmlConvert.ToString(WeekdayEnableWake)); 
					writer.WriteElementString(ENABLE_SLEEP_ELEMENT, IcdXmlConvert.ToString(WeekdayEnableSleep));
				}
				writer.WriteEndElement();

				writer.WriteStartElement(WEEKEND_ELEMENT);
				{
					writer.WriteElementString(WAKE_ELEMENT, IcdXmlConvert.ToString(WeekendWakeTime));
					writer.WriteElementString(SLEEP_ELEMENT, IcdXmlConvert.ToString(WeekendSleepTime));
					writer.WriteElementString(ENABLE_WAKE_ELEMENT, IcdXmlConvert.ToString(WeekendEnableWake));
					writer.WriteElementString(ENABLE_SLEEP_ELEMENT, IcdXmlConvert.ToString(WeekendEnableSleep));
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		#endregion
	}
}
