using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Calendaring.Bookings;
using ICD.Connect.Calendaring.CalendarManagers;
using ICD.Connect.Calendaring.Comparers;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;

namespace ICD.Connect.Partitioning.Commercial.CalendarOccupancyManagers.History
{
	public sealed class BookingHistoryManager : IBookingHistoryManager
	{
		#region Events

		public event EventHandler OnBookingsUpdated;

		#endregion

		#region Fields

		private readonly ICalendarOccupancyManager m_Parent;

		private readonly Dictionary<IBooking, HistoricalBooking> m_HistoricalBookings;

		private readonly SafeCriticalSection m_HistoricalBookingsSection;

		private ICalendarManager m_CalendarManager;

		#endregion

		#region Properties

		[NotNull]
		public ICalendarOccupancyManager Parent { get { return m_Parent; } }

		[CanBeNull]
		public ICalendarManager CalendarManager
		{
			get { return m_CalendarManager; } 
			private set
			{
				if (m_CalendarManager == value)
					return;

				Unsubscribe(m_CalendarManager);
				m_CalendarManager = value;
				Subscribe(m_CalendarManager);
				Initialize(m_CalendarManager);
			}
		}

		#endregion

		#region Constructor

		public BookingHistoryManager([NotNull] ICalendarOccupancyManager parent)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");

			m_Parent = parent;
			m_HistoricalBookings = new Dictionary<IBooking, HistoricalBooking>(BookingEqualityComparer.Instance);
			m_HistoricalBookingsSection = new SafeCriticalSection();

			
			Subscribe(m_Parent);
			Initialize(m_Parent);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Unsubscribe(m_Parent);
			CalendarManager = null;
		}

		/// <summary>
		/// Tries to set the occupancy history for the given booking
		/// </summary>
		/// <param name="booking"></param>
		/// <param name="state"></param>
		/// <returns>True if success, false if failed</returns>
		public bool TrySetOccupancyHistoryForBooking(IBooking booking, eOccupancyState state)
		{
			m_HistoricalBookingsSection.Enter();

			try
			{
				HistoricalBooking info;
				if (!m_HistoricalBookings.TryGetValue(booking, out info))
					return false;

				info.Occupancy = state;
			}
			finally
			{
				m_HistoricalBookingsSection.Leave();
			}

			OnBookingsUpdated.Raise(this);
			return true;
		}

		/// <summary>
		/// Tries to set the people count history for the given booking
		/// </summary>
		/// <param name="booking"></param>
		/// <param name="peopleCount"></param>
		/// <returns>True if success, false if failed</returns>
		public bool TrySetPeopleCountHistoryForBooking(IBooking booking, float? peopleCount)
		{
			m_HistoricalBookingsSection.Enter();
			
			try
			{
				HistoricalBooking info;
				if (!m_HistoricalBookings.TryGetValue(booking, out info))
					return false;

				info.PeopleCount = peopleCount;
			}
			finally
			{
				m_HistoricalBookingsSection.Leave();
			}

			OnBookingsUpdated.Raise(this);

			return true;
		}

		public IEnumerable<IHistoricalBooking> GetBookings()
		{
			return m_HistoricalBookingsSection.Execute(() => m_HistoricalBookings.Values.ToArray(m_HistoricalBookings.Count));
		}

		private void UpdateHistoricalBookings(IEnumerable<IBooking> bookings)
		{
			var bookingsList = bookings.ToList();

			bool changed = false;

			m_HistoricalBookingsSection.Enter();

			try
			{
				IEnumerable<IBooking> newBookings = bookingsList.Except(m_HistoricalBookings.Keys, BookingEqualityComparer.Instance).ToList();
				IEnumerable<IBooking> staleBooking = m_HistoricalBookings.Keys.Except(bookingsList, BookingEqualityComparer.Instance).ToList();

				//Remove stale bookings, but only if they haven't ended yet (should be started?), or are more than one day old
				foreach (IBooking booking in staleBooking)
				{
					if (!booking.IsBookingEnded() ||
					    booking.EndTime.AddDays(1) < DateTime.UtcNow)
						changed |= m_HistoricalBookings.Remove(booking);
				}

				//Add new bookings with empty history
				foreach(IBooking booking in newBookings)
				{
					changed = true;
					m_HistoricalBookings.Add(booking, HistoricalBooking.Copy(booking));
				}
			}
			finally
			{
				m_HistoricalBookingsSection.Leave();
			}

			if (changed)
				OnBookingsUpdated.Raise(this);
		}

		/// <summary>
		/// Removes bookings that ended more than 24 hrs ago
		/// </summary>
		private void RemoveOldHistoricalBookings()
		{
			m_HistoricalBookingsSection.Enter();

			try
			{
				List<IBooking> bookingsToRemove = m_HistoricalBookings.Keys.Where(b => b.EndTime.AddDays(1) < DateTime.UtcNow).ToList();
				bookingsToRemove.ForEach(b => m_HistoricalBookings.Remove(b));
			}
			finally
			{
				m_HistoricalBookingsSection.Leave();
			}
		}

		#endregion

		#region Parent Callbacks

		private void Initialize(ICalendarOccupancyManager parent)
		{
			CalendarManager = parent.CalendarManager;
		}

		private void Subscribe([NotNull] ICalendarOccupancyManager parent)
		{
			parent.OnCalendarManagerChanged += ParentOnCalendarManagerChanged;
		}

		private void Unsubscribe([NotNull] ICalendarOccupancyManager parent)
		{
			parent.OnCalendarManagerChanged -= ParentOnCalendarManagerChanged;
		}

		private void ParentOnCalendarManagerChanged(object sender, GenericEventArgs<ICalendarManager> args)
		{
			CalendarManager = args.Data;
		}

		#endregion

		#region CalendarManager Callbacks

		private void Initialize([CanBeNull] ICalendarManager calendarManager)
		{
			if (calendarManager == null)
				return;

			// ReSharper disable once RedundantEnumerableCastCall
			UpdateHistoricalBookings(calendarManager.GetBookings().Cast<IBooking>());
		}

		private void Subscribe([CanBeNull] ICalendarManager calendarManager)
		{
			if (calendarManager == null)
				return;

			calendarManager.OnBookingsChanged += CalendarManagerOnBookingsChanged;
		}

		private void Unsubscribe([CanBeNull] ICalendarManager calendarManager)
		{
			if (calendarManager == null)
				return;

			calendarManager.OnBookingsChanged -= CalendarManagerOnBookingsChanged;
		}

		private void CalendarManagerOnBookingsChanged(object sender, EventArgs args)
		{
			if (CalendarManager != null)
				// ReSharper disable once RedundantEnumerableCastCall
				UpdateHistoricalBookings(CalendarManager.GetBookings().Cast<IBooking>());
		}

		#endregion
	}
}
