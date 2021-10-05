using ICD.Connect.Calendaring.Bookings;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;

namespace ICD.Connect.Partitioning.Commercial.CalendarOccupancyManagers.History
{
	public interface IHistoricalBooking : IBooking
	{
		/// <summary>
		/// Occupancy State of the Booking
		/// </summary>
		eOccupancyState Occupancy { get; }

		/// <summary>
		/// Average people count of the booking
		/// Null if unknown
		/// </summary>
		float? PeopleCount { get; }
	}
}
