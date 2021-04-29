using System;
using ICD.Connect.Conferencing.EventArguments;
using ICD.Connect.Conferencing.Participants.Enums;
using ICD.Connect.Settings.ORM;

namespace ICD.Connect.Partitioning.Commercial.CallRatings
{
	public sealed class CallRatingParticipant
	{
		[PrimaryKey]
		[System.Reflection.Obfuscation(Exclude = true)]
		public int Id { get; set; }

		[ForeignKey(typeof(CallRating))]
		[System.Reflection.Obfuscation(Exclude = true)]
		public int CallRatingId { get; set; }

		[DataField]
		[System.Reflection.Obfuscation(Exclude = true)]
		public eCallType CallType { get; set; }

		[DataField]
		[System.Reflection.Obfuscation(Exclude = true)]
		public DateTime? StartTime { get; set; }

		[DataField]
		[System.Reflection.Obfuscation(Exclude = true)]
		public DateTime? EndTime { get; set; }

		[DataField]
		[System.Reflection.Obfuscation(Exclude = true)]
		public string Name { get; set; }

		[DataField]
		[System.Reflection.Obfuscation(Exclude = true)]
		public string Number { get; set; }

		[DataField]
		[System.Reflection.Obfuscation(Exclude = true)]
		public eCallDirection Direction { get; set; }

		[DataField]
		[System.Reflection.Obfuscation(Exclude = true)]
		public eCallAnswerState AnswerState { get; set; }
	}
}
