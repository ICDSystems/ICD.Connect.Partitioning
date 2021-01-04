using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Settings.ORM;

namespace ICD.Connect.Partitioning.Commercial.CallRatings
{
	public sealed class CallRating
	{
		#region Properties

		[PrimaryKey]
		[System.Reflection.Obfuscation(Exclude = true)]
		public int Id { get; set; }

		[DataField]
		[System.Reflection.Obfuscation(Exclude = true)]
		public DateTime Date { get; set; }

		// TODO - Currently using int instead of eCallRating because we want to store int in the database
		[DataField]
		[System.Reflection.Obfuscation(Exclude = true)]
		public int Rating { get; set; }

		[DataField]
		[System.Reflection.Obfuscation(Exclude = true)]
		public string RoomName { get; set; }

		[DataField]
		[System.Reflection.Obfuscation(Exclude = true)]
		public int RoomId { get; set; }

		[ForeignKey]
		[System.Reflection.Obfuscation(Exclude = true)]
		public IEnumerable<CallRatingParticipant> Participants { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public CallRating()
		{
			Participants = new CallRatingParticipant[0];
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the participants associated with the call rating.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<CallRatingParticipant> GetCallRatingParticipants()
		{
			return Participants ?? Enumerable.Empty<CallRatingParticipant>();
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Adds the rating to the database.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="rating"></param>
		public static void Insert(int roomId, [NotNull] CallRating rating)
		{
			Persistent.Db(eDb.RoomData, roomId.ToString()).Insert<CallRating>(rating);
		}

		/// <summary>
		/// Gets the average rating for the given time period.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public static float AverageRating(int roomId, DateTime start, DateTime end)
		{
			string tableName = TypeModel.Get(typeof(CallRating)).TableName;

			string sql =
				string.Format(@"SELECT
									* FROM {0}
								WHERE
									Date >= '{1:yyyy-MM-dd HH:mm:ss.FFFFFF}'
								AND
									Date <= '{2:YYYY-MM-dd HH:mm:ss.FFFFFF}'
								AND
									Rating > 0",
				              tableName, start.ToUniversalTime(), end.ToUniversalTime());

			int[] ratings =
				Persistent.Db(eDb.RoomData, roomId.ToString())
				          .Query<CallRating>(sql)
				          .Select(r => (int)r.Rating)
				          .ToArray();

			return ratings.Length == 0 ? 0 : (float)ratings.Average();
		}

		#endregion
	}
}
