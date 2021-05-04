using System;
using System.Collections.Generic;
using System.Linq;

namespace ICD.Connect.Partitioning.Commercial.CalendarOccupancyManagers
{
	public sealed class BookingPeopleCounter
	{
		private readonly List<PeopleCountTimeSpan> m_PeopleCounts;
		private int m_CurrentPeopleCount;
		private DateTime m_CurrentPeopleCountTime;
		private bool m_PeopleCountEnabled;

		public int CurrentPeopleCount
		{
			get { return m_CurrentPeopleCount; }
			set
			{
				if (m_CurrentPeopleCount == value)
					return;

				AddCurrentPeopleCountInfo();
				m_CurrentPeopleCount = value;
				m_CurrentPeopleCountTime = DateTime.UtcNow;

			}
		}

		public bool PeopleCountEnabled
		{
			get { return m_PeopleCountEnabled; }
			set
			{
				if (m_PeopleCountEnabled == value)
					return;

				// Add current value now if we're going from enabled to disabled
				if (m_PeopleCountEnabled)
					AddCurrentPeopleCountInfo();
				
				m_PeopleCountEnabled = value;
				
				//Update Count Time
				m_CurrentPeopleCountTime = m_PeopleCountEnabled ? DateTime.UtcNow : default(DateTime);

			}
		}

		public DateTime CurrentPeopleCountTime { get { return m_CurrentPeopleCountTime; } }

		public BookingPeopleCounter()
		{
			m_PeopleCounts = new List<PeopleCountTimeSpan>();
		}

		private void AddCurrentPeopleCountInfo()
		{
			if (!PeopleCountEnabled || CurrentPeopleCountTime == default(DateTime))
				return;

			m_PeopleCounts.Add(new PeopleCountTimeSpan(CurrentPeopleCount, DateTime.UtcNow - CurrentPeopleCountTime));
		}

		/// <summary>
		/// Ends the people counting and returns the average people count for the duration
		/// </summary>
		/// <returns>Average people count, or null if no people counts for more than one minute exist</returns>
		public float? EndPeopleCounting()
		{
			AddCurrentPeopleCountInfo();
			return GetAveragePeopleCount();
		}

		/// <summary>
		/// Gets the average people count for the duration, where the people count was >0 and time was at least 1 minute
		/// </summary>
		/// <returns>Average people count, or null if no people counts for more than one minute exist</returns>
		private float? GetAveragePeopleCount()
		{
			double weightedPeople = 0;
			double totalMinutes = 0;

			// Don't count entries of less than 1 minute, or of less than 1 person
			foreach (PeopleCountTimeSpan pc in m_PeopleCounts.Where(pc => pc.TimeSpan.TotalMinutes >= 1 && pc.PeopleCount > 0))
			{
				weightedPeople += pc.PeopleCount * pc.TimeSpan.TotalMinutes;
				totalMinutes += pc.TimeSpan.TotalMinutes;
			}

			// Head off "divide by 0" errors
			if (totalMinutes < 1)
				return null;

			return (float)(weightedPeople / totalMinutes);
		}
	}
}
