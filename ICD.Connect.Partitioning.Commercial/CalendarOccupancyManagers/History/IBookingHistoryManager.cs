using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.Calendaring.Bookings;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;

namespace ICD.Connect.Partitioning.Commercial.CalendarOccupancyManagers.History
{
	public interface IBookingHistoryManager : IDisposable
	{
		event EventHandler OnBookingsUpdated;

		/// <summary>
		/// Tries to set the occupancy history for the given booking
		/// </summary>
		/// <param name="booking"></param>
		/// <param name="state"></param>
		/// <returns>True if success, false if failed</returns>
		bool TrySetOccupancyHistoryForBooking(IBooking booking, eOccupancyState state);

		/// <summary>
		/// Tries to set the people count history for the given booking
		/// </summary>
		/// <param name="booking"></param>
		/// <param name="peopleCount"></param>
		/// <returns>True if success, false if failed</returns>
		bool TrySetPeopleCountHistoryForBooking(IBooking booking, float? peopleCount);

		[NotNull]
		IEnumerable<IHistoricalBooking> GetBookings();
	}
}
