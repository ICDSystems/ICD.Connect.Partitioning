using System;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Calendaring.CalendarManagers;
using ICD.Connect.Partitioning.Commercial.CalendarOccupancyManagers.History;
using ICD.Connect.Partitioning.Commercial.OccupancyManagers;
using ICD.Connect.Partitioning.Commercial.Rooms;

namespace ICD.Connect.Partitioning.Commercial.CalendarOccupancyManagers
{
	public interface ICalendarOccupancyManager : IDisposable
	{

		event EventHandler<GenericEventArgs<ICalendarManager>> OnCalendarManagerChanged;

		event EventHandler<GenericEventArgs<IOccupancyManager>> OnOccupancyManagerChanged;

		[NotNull]
		ICommercialRoom CommercialRoom { get; }

		[CanBeNull]
		ICalendarManager CalendarManager { get; }

		[CanBeNull]
		IOccupancyManager OccupancyManager { get; }

		[NotNull]
		IBookingHistoryManager BookingHistory { get; }
	}
}
