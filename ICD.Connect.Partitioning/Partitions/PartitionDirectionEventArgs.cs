using System;

namespace ICD.Connect.Partitioning.Partitions
{
	public enum ePartitionDirection
	{
		Top,
		Bottom,
		Left,
		Right
	}

	public sealed class PartitionDirectionEventArgs : EventArgs
	{
		private readonly ePartitionDirection m_Direction;
		private readonly int m_Column;
		private readonly int m_Row;

		public ePartitionDirection Direction { get { return m_Direction; } }

		public int Column { get { return m_Column; } }

		public int Row { get { return m_Row; } }

		public PartitionDirectionEventArgs(ePartitionDirection direction, int column, int row)
		{
			m_Direction = direction;
			m_Column = column;
			m_Row = row;
		}
	}
}
