using System;

namespace ICD.Connect.Partitioning.Controls
{
	[Flags]
	public enum ePartitionFeedback
	{
		None = 0,

		/// <summary>
		/// The partition control provides feedback for the current state of the partition (e.g. a sensor)
		/// </summary>
		Get = 1,

		/// <summary>
		/// The partition control can be opened/closed to change the state of the partition (e.g. a DSP mixer)
		/// </summary>
		Set = 2,

		GetSet = Get | Set
	}
}
