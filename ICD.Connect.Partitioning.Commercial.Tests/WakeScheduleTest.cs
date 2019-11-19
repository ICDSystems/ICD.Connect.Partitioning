using System;
using NUnit.Framework;

namespace ICD.Connect.Partitioning.Commercial.Tests
{
	[TestFixture]
	public sealed class WakeScheduleTest
	{
		#region Events

		[Test]
		public void WakeActionRequestedFeedbackTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void SleepActionRequestedFeedbackTest()
		{
			Assert.Inconclusive();
		}

		#endregion

		#region Properties

		[Test]
		public void WeekdayWakeTimeTest()
		{
			WeekdayWakeTimeTest(null, null);
			WeekdayWakeTimeTest(new TimeSpan(1, 1, 1), new TimeSpan(1, 1, 1));
			WeekdayWakeTimeTest(new TimeSpan(24, 1, 1), new TimeSpan(0, 1, 1));
			WeekdayWakeTimeTest(new TimeSpan(-24, 1, 1), new TimeSpan(0, 1, 1));
		}

		private static void WeekdayWakeTimeTest(TimeSpan? value, TimeSpan? expected)
		{
			WakeSchedule schedule = new WakeSchedule {WeekdayWakeTime = value};
			Assert.AreEqual(expected, schedule.WeekdayWakeTime);
		}

		[Test]
		public void WeekdaySleepTimeTest()
		{
			WeekdaySleepTimeTest(null, null);
			WeekdaySleepTimeTest(new TimeSpan(1, 1, 1), new TimeSpan(1, 1, 1));
			WeekdaySleepTimeTest(new TimeSpan(24, 1, 1), new TimeSpan(0, 1, 1));
			WeekdaySleepTimeTest(new TimeSpan(-24, 1, 1), new TimeSpan(0, 1, 1));
		}

		private static void WeekdaySleepTimeTest(TimeSpan? value, TimeSpan? expected)
		{
			WakeSchedule schedule = new WakeSchedule { WeekdaySleepTime = value };
			Assert.AreEqual(expected, schedule.WeekdaySleepTime);
		}

		[Test]
		public void WeekendWakeTimeTest()
		{
			WeekendWakeTimeTest(null, null);
			WeekendWakeTimeTest(new TimeSpan(1, 1, 1), new TimeSpan(1, 1, 1));
			WeekendWakeTimeTest(new TimeSpan(24, 1, 1), new TimeSpan(0, 1, 1));
			WeekendWakeTimeTest(new TimeSpan(-24, 1, 1), new TimeSpan(0, 1, 1));
		}

		private static void WeekendWakeTimeTest(TimeSpan? value, TimeSpan? expected)
		{
			WakeSchedule schedule = new WakeSchedule { WeekendWakeTime = value };
			Assert.AreEqual(expected, schedule.WeekendWakeTime);
		}

		[Test]
		public void WeekendSleepTimeTest()
		{
			WeekendSleepTimeTest(null, null);
			WeekendSleepTimeTest(new TimeSpan(1, 1, 1), new TimeSpan(1, 1, 1));
			WeekendSleepTimeTest(new TimeSpan(24, 1, 1), new TimeSpan(0, 1, 1));
			WeekendSleepTimeTest(new TimeSpan(-24, 1, 1), new TimeSpan(0, 1, 1));
		}

		private static void WeekendSleepTimeTest(TimeSpan? value, TimeSpan? expected)
		{
			WakeSchedule schedule = new WakeSchedule { WeekendSleepTime = value };
			Assert.AreEqual(expected, schedule.WeekendSleepTime);
		}
		
		[TestCase(true, true)]
		[TestCase(false, false)]
		public void WeekdayEnableTest(bool value, bool expected)
		{
			WakeSchedule schedule = new WakeSchedule { WeekdayEnable = value };
			Assert.AreEqual(expected, schedule.WeekdayEnable);
		}

		[TestCase(true, true)]
		[TestCase(false, false)]
		public void WeekendEnableTest(bool value, bool expected)
		{
			WakeSchedule schedule = new WakeSchedule { WeekendEnable = value };
			Assert.AreEqual(expected, schedule.WeekendEnable);
		}

		[Test]
		public void IsEnabledTodayTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void IsWakeTimeTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void IsSleepTimeTest()
		{
			Assert.Inconclusive();
		}

		#endregion

		#region Methods

		[Test]
		public void RunFinalTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void ClearTest()
		{
			WakeSchedule schedule = new WakeSchedule
			{
				WeekdayWakeTime = new TimeSpan(1, 1, 1),
				WeekdaySleepTime = new TimeSpan(1, 1, 1),
				WeekendWakeTime = new TimeSpan(1, 1, 1),
				WeekendSleepTime = new TimeSpan(1, 1, 1),
				WeekdayEnable = true,
				WeekendEnable = true
			};

			schedule.Clear();

			Assert.AreEqual(null, schedule.WeekdayWakeTime);
			Assert.AreEqual(null, schedule.WeekdaySleepTime);
			Assert.AreEqual(null, schedule.WeekendWakeTime);
			Assert.AreEqual(null, schedule.WeekendSleepTime);
			Assert.AreEqual(false, schedule.WeekdayEnable);
			Assert.AreEqual(false, schedule.WeekendEnable);
		}

		[Test]
		public void CopyTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void ParseXmlTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void WriteElementsTest()
		{
			Assert.Inconclusive();
		}

		#endregion
	}
}
