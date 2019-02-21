using System;
using ICD.Common.Properties;

namespace ICD.Connect.Partitioning.Cells
{
	public struct CellColumnRowInfo : IEquatable<CellColumnRowInfo>, IComparable<CellColumnRowInfo>
	{
		private readonly int m_Column;
		private readonly int m_Row;

		/// <summary>
		/// Gets the column number.
		/// </summary>
		public int Column { get { return m_Column; }}

		/// <summary>
		/// Gets the row number.
		/// </summary>
		public int Row { get { return m_Row; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="row"></param>
		public CellColumnRowInfo(int column, int row)
		{
			m_Column = column;
			m_Row = row;
		}

		#region Equality

		/// <summary>
		/// Implementing default equality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator ==(CellColumnRowInfo a1, CellColumnRowInfo a2)
		{
			return a1.Equals(a2);
		}

		/// <summary>
		/// Implementing default inequality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator !=(CellColumnRowInfo a1, CellColumnRowInfo a2)
		{
			return !a1.Equals(a2);
		}

		/// <summary>
		/// Returns true if this instance is equal to the given object.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			return other is CellColumnRowInfo && Equals((CellColumnRowInfo)other);
		}

		/// <summary>
		/// Returns true if this instance is equal to the given endpoint.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		[Pure]
		public bool Equals(CellColumnRowInfo other)
		{
			return m_Column == other.m_Column &&
				   m_Row == other.m_Row;
		}

		/// <summary>
		/// Gets the hashcode for this instance.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + m_Column;
				hash = hash * 23 + m_Row;
				return hash;
			}
		}

		public int CompareTo(CellColumnRowInfo other)
		{
			int result = other.m_Row.CompareTo(other.m_Row);
			return result != 0 ? result : m_Column.CompareTo(other.m_Column);
		}

		#endregion
	}
}
