using System;

namespace ICD.Connect.Partitioning.Rooms
{
	public sealed class ChildChangedEventArgs : EventArgs
	{
		public int OriginatorId { get; private set; }

		public eCombineMode CombineMode { get; private set; }

		public ChildChangedEventArgs(int id, eCombineMode type)
		{
			OriginatorId = id;
			CombineMode = type;
		}
	}
}