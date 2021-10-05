#if NETFRAMEWORK
extern alias RealNewtonsoft;
using RealNewtonsoft.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Calendaring.Bookings;
using ICD.Connect.Conferencing.DialContexts;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;

namespace ICD.Connect.Partitioning.Commercial.CalendarOccupancyManagers.History
{
	[JsonConverter(typeof(HistoricalBookingJsonConverter))]
	public sealed class HistoricalBooking : AbstractBasicBooking, IHistoricalBooking
	{
		/// <summary>
		/// Occupancy State of the Booking
		/// </summary>
		public eOccupancyState Occupancy { get; set; }

		/// <summary>
		/// Average people count of the booking
		/// Null if unknown
		/// </summary>
		public float? PeopleCount { get; set; }

		public static HistoricalBooking Copy([NotNull] IHistoricalBooking other)
		{
			if (other == null)
				throw new ArgumentNullException("other");

			HistoricalBooking output =
				new HistoricalBooking
				{
					MeetingName = other.MeetingName,
					OrganizerName = other.OrganizerName,
					OrganizerEmail = other.OrganizerEmail,
					StartTime = other.StartTime,
					EndTime = other.EndTime,
					IsPrivate = other.IsPrivate,
					CheckedIn = other.CheckedIn,
					CheckedOut = other.CheckedOut,
					Occupancy = other.Occupancy,
					PeopleCount =  other.PeopleCount
				};

			IEnumerable<IDialContext> bookingNumbers = other.GetBookingNumbers().Select(b => (IDialContext)DialContext.Copy(b));
			output.SetBookingNumbers(bookingNumbers);

			return output;
		}

		/// <summary>
		/// Creates a copy of the given booking.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		[NotNull]
		public static HistoricalBooking Copy([NotNull] IBooking other)
		{
			if (other == null)
				throw new ArgumentNullException("other");

			HistoricalBooking output =
				new HistoricalBooking
				{
					MeetingName = other.MeetingName,
					OrganizerName = other.OrganizerName,
					OrganizerEmail = other.OrganizerEmail,
					StartTime = other.StartTime,
					EndTime = other.EndTime,
					IsPrivate = other.IsPrivate,
					CheckedIn = other.CheckedIn,
					CheckedOut = other.CheckedOut
				};

			IEnumerable<IDialContext> bookingNumbers = other.GetBookingNumbers().Select(b => (IDialContext)DialContext.Copy(b));
			output.SetBookingNumbers(bookingNumbers);

			return output;
		}
	}

	public sealed class HistoricalBookingJsonConverter : AbstractBasicBookingJsonConverter<HistoricalBooking>
	{
		private const string TOKEN_OCCUPANCY = "occupancy";
		private const string TOKEN_PEOPLE_COUNT = "peopleCount";

		/// <summary>
		/// Override to write properties to the writer.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, HistoricalBooking value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			if (value.Occupancy != default(eOccupancyState))
				writer.WriteProperty(TOKEN_OCCUPANCY, value.Occupancy);

			if (value.PeopleCount.HasValue)
				writer.WriteProperty(TOKEN_PEOPLE_COUNT, value.PeopleCount);

		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, HistoricalBooking instance,
		                                     JsonSerializer serializer)
		{
			switch (property)
			{
				case TOKEN_OCCUPANCY:
					instance.Occupancy = reader.GetValueAsEnum<eOccupancyState>();
					break;

				case TOKEN_PEOPLE_COUNT:
					instance.PeopleCount = reader.GetValueAsFloat();
					break;

				default:
					base.ReadProperty(property, reader, instance, serializer);
					return;
			}
		}
	}
}
