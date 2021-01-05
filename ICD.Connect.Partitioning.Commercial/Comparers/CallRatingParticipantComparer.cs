using System.Collections.Generic;
using ICD.Connect.Partitioning.Commercial.CallRatings;

namespace ICD.Connect.Partitioning.Commercial.Comparers
{
	public sealed class CallRatingParticipantComparer : IEqualityComparer<CallRatingParticipant>
	{
		private static CallRatingParticipantComparer s_Instance;

		public static CallRatingParticipantComparer Instance
		{
			get { return s_Instance = s_Instance ?? new CallRatingParticipantComparer(); }
		}

		private CallRatingParticipantComparer()
		{
		}

		public bool Equals(CallRatingParticipant x, CallRatingParticipant y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;

			return x.Name == y.Name &&
			       x.Id == y.Id &&
			       x.StartTime == y.StartTime &&
			       x.EndTime == y.EndTime &&
			       x.AnswerState == y.AnswerState &&
			       x.CallRatingId == y.CallRatingId &&
			       x.CallType == y.CallType &&
			       x.Direction == y.Direction &&
			       x.Number == y.Number;
		}

		public int GetHashCode(CallRatingParticipant obj)
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + (obj.Name == null ? 0 : obj.Name.GetHashCode());
				hash = hash * 23 + obj.Id.GetHashCode();
				hash = hash * 23 + obj.StartTime.GetHashCode();
				hash = hash * 23 + obj.EndTime.GetHashCode();
				hash = hash * 23 + obj.AnswerState.GetHashCode();
				hash = hash * 23 + obj.CallRatingId.GetHashCode();
				hash = hash * 23 + obj.CallType.GetHashCode();
				hash = hash * 23 + obj.Direction.GetHashCode();
				hash = hash * 23 + (obj.Number == null ? 0 : obj.Number.GetHashCode());
				return hash;
			}
		}
	}
}
