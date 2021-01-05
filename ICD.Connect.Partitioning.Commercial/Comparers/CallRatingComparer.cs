using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Partitioning.Commercial.CallRatings;

namespace ICD.Connect.Partitioning.Commercial.Comparers
{
	public sealed class CallRatingComparer : IEqualityComparer<CallRating>
	{
		private static CallRatingComparer s_Instance;

		public static CallRatingComparer Instance
		{
			get { return s_Instance = s_Instance ?? new CallRatingComparer(); }
		}

		private CallRatingComparer()
		{
		}

		public bool Equals(CallRating x, CallRating y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;

			return x.RoomId == y.RoomId &&
			       x.Id == y.Id &&
			       x.Rating == y.Rating &&
			       x.RoomName == y.RoomName &&
			       x.Date == y.Date &&
			       x.Participants.SequenceEqual(y.Participants, CallRatingParticipantComparer.Instance);
		}

		public int GetHashCode(CallRating obj)
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + obj.RoomId.GetHashCode();
				hash = hash * 23 + obj.Id.GetHashCode();
				hash = hash * 23 + obj.Rating.GetHashCode();
				hash = hash * 23 + (obj.RoomName == null ? 0 : obj.RoomName.GetHashCode());
				hash = hash * 23 + obj.Date.GetHashCode();

				foreach (var participant in obj.Participants ?? Enumerable.Empty<CallRatingParticipant>())
					hash = hash * 23 + CallRatingParticipantComparer.Instance.GetHashCode(participant);

				return hash;
			}
		}
	}
}
