using System;

namespace ICD.Connect.Partitioning.Commercial.CalendarOccupancyManagers
{
	public sealed class PeopleCountTimeSpan
	{
		private readonly int m_PeopleCount;
		private readonly TimeSpan m_TimeSpan;
		

		public int PeopleCount { get { return m_PeopleCount; } }
		public TimeSpan TimeSpan { get { return m_TimeSpan; } }

		public PeopleCountTimeSpan(int peopleCount, TimeSpan timeSpan)
		{
			m_PeopleCount = peopleCount;
			m_TimeSpan = timeSpan;
		}
	}
}
