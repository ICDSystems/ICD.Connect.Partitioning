using System;

namespace ICD.Connect.Partitioning.Cells
{
	public enum eCellDirection
	{
		Top,
		Bottom,
		Left,
		Right
	}

	public sealed class CellDirectionEventArgs : EventArgs
	{
		private readonly eCellDirection m_Direction;
		private readonly int m_Column;
		private readonly int m_Row;

		public eCellDirection Direction { get { return m_Direction; } }

		public int Column { get { return m_Column; } }

		public int Row { get { return m_Row; } }

		public CellDirectionEventArgs(eCellDirection direction, int column, int row)
		{
			m_Direction = direction;
			m_Column = column;
			m_Row = row;
		}
	}
}
