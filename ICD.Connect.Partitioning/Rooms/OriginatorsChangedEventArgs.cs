using System;

namespace ICD.Connect.Partitioning.Rooms
{
	public sealed class OriginatorsChangedEventArgs : EventArgs
	{
		public int OriginatorId { get; private set; }

		public eAddRemoveType AddRemoveType { get; private set; }

		public OriginatorsChangedEventArgs(int id, eAddRemoveType type)
		{
			OriginatorId = id;
			AddRemoveType = type;
		}

	}
}