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
			get { return DateTime.Today.DayOfWeek.IsWeekday() ? WeekdayEnable : WeekendEnable; }
		}

		/// <summary>
		/// Returns true if the system should be awake at the current time.
		/// </summary>
		public bool IsWakeTime
		{
			get
			{
				DateTime now = IcdEnvironment.GetLocalTime();
				DateTime? wakeTime = GetWakeTimeForDay(now.Date);
				DateTime? sleepTime = GetSleepTimeForDay(now.Date);

				if (sleepTime == null || wakeTime == null)
					return wakeTime != null && now >= wakeTime;

				DateTime? time = now.PreviousLatestTime(true, sleepTime.Value, wakeTime.Value);
				if (time == null)
					return false;

				return time == wakeTime.Value;
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
				DateTime? wakeTime = GetWakeTimeForDay(now.Date);
				DateTime? sleepTime = GetSleepTimeForDay(now.Date);

				if (sleepTime == null || wakeTime == null)
					return sleepTime != null && now >= sleepTime;

				DateTime? time = now.PreviousLatestTime(true, sleepTime.Value, wakeTime.Value);
				if (time == null)
					return false;

				return time == sleepTime.Value;
			}
		}

		#endregion

		#region Methods

		public override void RunFinal()
		{
			if (IsSleepTime)
				Sleep();
			else if (IsWakeTime)
				Wake();
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

		#region Private Methods

		private TimeSpan? Wrap24Hours(TimeSpan? value)
		{
			if (value == null)
				return null;

			int hour = value.Value.Hours % 24;
			return new TimeSpan(hour, value.Value.Minutes, value.Value.Seconds);
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

		private void Wake()
		{
			OnWakeActionRequested.Raise(this);
		}

		private void Sleep()
		{
			OnSleepActionRequested.Raise(this);
		}

		#endregion
	}
}
