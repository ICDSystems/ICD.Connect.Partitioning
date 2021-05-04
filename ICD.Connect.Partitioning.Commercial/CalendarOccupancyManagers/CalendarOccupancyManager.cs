using System;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Timers;
using ICD.Connect.Calendaring.Bookings;
using ICD.Connect.Calendaring.CalendarManagers;
using ICD.Connect.Calendaring.Comparers;
using ICD.Connect.Partitioning.Commercial.CalendarOccupancyManagers.History;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;
using ICD.Connect.Partitioning.Commercial.OccupancyManagers;
using ICD.Connect.Partitioning.Commercial.Rooms;

namespace ICD.Connect.Partitioning.Commercial.CalendarOccupancyManagers
{
	public sealed class CalendarOccupancyManager : ICalendarOccupancyManager
	{

		/// <summary>
		/// Fractional time into the booking that the occupancy snapshot gets taken
		/// </summary>
		private const float SNAPSHOT_FRACTIONAL_TIME = .5f;

		/// <summary>
		/// Maximum time into the booking that the occupancy snapshot gets taken, in seconds
		/// </summary>
		private const long SNAPSHOT_MAX_TIME = 20 * 60;

		#region Events

		public event EventHandler<GenericEventArgs<ICalendarManager>> OnCalendarManagerChanged;

		public event EventHandler<GenericEventArgs<IOccupancyManager>> OnOccupancyManagerChanged;

		#endregion

		#region Fields

		[NotNull]
		private readonly ICommercialRoom m_CommercialRoom;
		[NotNull]
		private readonly IBookingHistoryManager m_BookingHistory;

		[CanBeNull]
		private ICalendarManager m_CalendarManager;
		
		[CanBeNull]
		private IOccupancyManager m_OccupancyManager;
		
		[CanBeNull]
		private IBooking m_CurrentBooking;

		[CanBeNull]
		private BookingPeopleCounter m_CurrentBookingPeopleCounter;

		private readonly SafeTimer m_OccupancySnapshotTimer;

		#endregion

		#region Properties

		public IBookingHistoryManager BookingHistory { get { return m_BookingHistory; } }

		[CanBeNull]
		private IBooking CurrentBooking { get { return m_CurrentBooking;}
			set
			{
				if(BookingEqualityComparer.Instance.Equals(m_CurrentBooking, value))
					return;

				ProcessBookingEnd(m_CurrentBooking, m_CurrentBookingPeopleCounter);
				
				m_CurrentBooking = value;

				ScheduleOccupancySnapshot();
				m_CurrentBookingPeopleCounter = m_CurrentBooking == null ? null : new BookingPeopleCounter();
				UpdatePeopleCounter();
				
			}
		}

		public ICommercialRoom CommercialRoom {get {return m_CommercialRoom; } }

		public ICalendarManager CalendarManager
		{
			get{ return m_CalendarManager; }
			private set
			{
				if (m_CalendarManager == value)
					return;

				Unsubscribe(m_CalendarManager);
				m_CalendarManager = value;
				Subscribe(m_CalendarManager);
				CurrentBooking = m_CalendarManager == null ? null : m_CalendarManager.CurrentBooking;

				OnCalendarManagerChanged.Raise(this, m_CalendarManager);
			}
		}

		public IOccupancyManager OccupancyManager
		{
			get { return m_OccupancyManager; }
			private set
			{
				if (m_OccupancyManager == value)
					return;

				Unsubscribe(m_OccupancyManager);
				m_OccupancyManager = value;
				Subscribe(m_OccupancyManager);

				OnOccupancyManagerChanged.Raise(this, m_OccupancyManager);
			}
		}

		#endregion

		#region Constructor

		public CalendarOccupancyManager([NotNull] ICommercialRoom parent)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");

			m_CommercialRoom = parent;

			m_BookingHistory = new BookingHistoryManager(this);

			m_OccupancySnapshotTimer = SafeTimer.Stopped(TakeOccupancySnapshot);

			Subscribe(m_CommercialRoom);
			OccupancyManager = m_CommercialRoom.OccupancyManager;
			CalendarManager = m_CommercialRoom.CalendarManager;


		}

		#endregion

		#region Methods

		/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
		public void Dispose()
		{
			Unsubscribe(m_CommercialRoom);
			CalendarManager = null;
			OccupancyManager = null;
			m_CurrentBooking = null;
			m_OccupancySnapshotTimer.Stop();
			m_OccupancySnapshotTimer.Dispose();
		}

		#region Occupancy

		private void ScheduleOccupancySnapshot()
		{
			var currentBooking = CurrentBooking;

			if (currentBooking == null)
			{
				m_OccupancySnapshotTimer.Stop();
				return;
			}

			int totalDuration = (int)((currentBooking.EndTime - currentBooking.StartTime).TotalSeconds);

			int fractionalDuration = (int)(totalDuration * SNAPSHOT_FRACTIONAL_TIME);

			long snapshotDuration = Math.Min(fractionalDuration, SNAPSHOT_MAX_TIME);

			DateTime snapshotTime = currentBooking.StartTime.AddSeconds(snapshotDuration);

			DateTime utcNow = DateTime.UtcNow;

			// Edge Case - snapshot has already elapsed
			if (snapshotTime <= utcNow)
			{
				m_OccupancySnapshotTimer.Stop();

				// Super Edge Case - make sure meeting has not ended
				if (currentBooking.EndTime > utcNow)
					TakeOccupancySnapshot();

				return;
			}
			
			TimeSpan timeUntilSnapshot = snapshotTime - utcNow;

			m_OccupancySnapshotTimer.Reset((long)timeUntilSnapshot.TotalMilliseconds);
		}

		private void TakeOccupancySnapshot()
		{
			if (CurrentBooking == null)
				return;

			eOccupancyState occupancyState = OccupancyManager == null ? eOccupancyState.Unknown : OccupancyManager.OccupancyState;

			BookingHistory.TrySetOccupancyHistoryForBooking(CurrentBooking, occupancyState);
		}

		#endregion

		#region People Count

		private void UpdatePeopleCounter()
		{
			if (m_CurrentBookingPeopleCounter == null || OccupancyManager == null)
				return;

			m_CurrentBookingPeopleCounter.CurrentPeopleCount = OccupancyManager.PeopleCount;
			m_CurrentBookingPeopleCounter.PeopleCountEnabled =
				OccupancyManager.SupportedFeatures.HasFlag(eOccupancyFeatures.PeopleCounting);
		}

		private void ProcessBookingEnd([CanBeNull] IBooking booking, [CanBeNull] BookingPeopleCounter peopleCounter)
		{
			if (booking == null || peopleCounter == null)
				return;

			float? peopleCount = peopleCounter.EndPeopleCounting();
			
			BookingHistory.TrySetPeopleCountHistoryForBooking(booking, peopleCount);
		}

		#endregion

		#endregion

		#region Room Callbacks

		private void Subscribe([NotNull] ICommercialRoom room)
		{
			room.OnCalendarManagerChanged += RoomOnCalendarManagerChanged;
			room.OnOccupancyManagerChanged += RoomOnOccupancyManagerChanged;
		}

		private void Unsubscribe([NotNull] ICommercialRoom room)
		{
			room.OnCalendarManagerChanged -= RoomOnCalendarManagerChanged;
			room.OnOccupancyManagerChanged -= RoomOnOccupancyManagerChanged;
		}

		private void RoomOnCalendarManagerChanged(object sender, GenericEventArgs<ICalendarManager> args)
		{
			CalendarManager = args.Data;
		}

		private void RoomOnOccupancyManagerChanged(object sender, GenericEventArgs<IOccupancyManager> args)
		{
			OccupancyManager = args.Data;
		}

		#endregion

		#region Calendar Manager Callbacks

		private void Subscribe(ICalendarManager calendarManager)
		{
			if (calendarManager == null)
				return;

			calendarManager.OnCurrentBookingChanged += CalendarManagerOnCurrentBookingChanged;
		}

		private void Unsubscribe(ICalendarManager calendarManager)
		{
			if (calendarManager == null)
				return;

			calendarManager.OnCurrentBookingChanged -= CalendarManagerOnCurrentBookingChanged;
		}

		private void CalendarManagerOnCurrentBookingChanged(object sender, GenericEventArgs<IBooking> args)
		{
			CurrentBooking = args.Data;
		}

		#endregion

		

		#region Occupancy Manager Callbacks

		private void Subscribe(IOccupancyManager occupancyManager)
		{
			if (occupancyManager == null)
				return;

			occupancyManager.OnPeopleCountChanged += OccupancyManagerOnPeopleCountChanged;
			occupancyManager.OnSupportedFeaturesChanged += OccupancyManagerOnSupportedFeaturesChanged;
		}

		
		private void Unsubscribe(IOccupancyManager occupancyManager)
		{
			if (occupancyManager == null)
				return;

			occupancyManager.OnPeopleCountChanged -= OccupancyManagerOnPeopleCountChanged;
			occupancyManager.OnSupportedFeaturesChanged -= OccupancyManagerOnSupportedFeaturesChanged;
		}

		private void OccupancyManagerOnPeopleCountChanged(object sender, IntEventArgs args)
		{
			if (m_CurrentBookingPeopleCounter == null)
				return;

			m_CurrentBookingPeopleCounter.CurrentPeopleCount = args.Data;
		}

		private void OccupancyManagerOnSupportedFeaturesChanged(object sender, GenericEventArgs<eOccupancyFeatures> args)
		{
			if (m_CurrentBookingPeopleCounter != null)
				m_CurrentBookingPeopleCounter.PeopleCountEnabled = args.Data.HasFlag(eOccupancyFeatures.PeopleCounting);
		}


		#endregion
	}
}
