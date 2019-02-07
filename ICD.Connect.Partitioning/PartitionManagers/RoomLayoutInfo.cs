using System;
using ICD.Common.Properties;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public struct RoomPositionInfo : IEquatable<RoomPositionInfo>, IComparable<RoomPositionInfo>
	{
		private readonly int m_Column;
		private readonly int m_Row;

		/// <summary>
		/// Gets the column for the room.
		/// </summary>
		public int Column { get { return m_Column; } }

		/// <summary>
		/// Gets the row for the room.
		/// </summary>
		public int Row { get { return m_Row; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="row"></param>
		public RoomPositionInfo(int column, int row)
		{
			m_Column = column;
			m_Row = row;
		}

		/// <summary>
		/// Implementing default equality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator ==(RoomPositionInfo a1, RoomPositionInfo a2)
		{
			return a1.Equals(a2);
		}

		/// <summary>
		/// Implementing default inequality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator !=(RoomPositionInfo a1, RoomPositionInfo a2)
		{
			return !a1.Equals(a2);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		[Pure]
		public bool Equals(RoomPositionInfo other)
		{
			return m_Column == other.m_Column &&
			       m_Row == other.m_Row;
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is RoomPositionInfo && Equals((RoomPositionInfo)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = m_Column;
				hashCode = (hashCode * 397) ^ m_Row;
				return hashCode;
			}
		}

		[Pure]
		public int CompareTo(RoomPositionInfo other)
		{
			int columnComparison = m_Column.CompareTo(other.m_Column);
			return columnComparison != 0 ? columnComparison : m_Row.CompareTo(other.m_Row);
		}
	}

	public struct RoomLayoutInfo : IEquatable<RoomLayoutInfo>, IComparable<RoomLayoutInfo>
	{
		private readonly RoomPositionInfo m_Position;
		private readonly int m_RoomId;

		/// <summary>
		/// Gets the position for the room.
		/// </summary>
		public RoomPositionInfo Position { get { return m_Position; } }

		/// <summary>
		/// Gets the room ID.
		/// </summary>
		public int RoomId { get { return m_RoomId; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="row"></param>
		/// <param name="roomId"></param>
		public RoomLayoutInfo(int column, int row, int roomId)
			: this(new RoomPositionInfo(column, row), roomId)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="roomId"></param>
		public RoomLayoutInfo(RoomPositionInfo position, int roomId)
		{
			m_Position = position;
			m_RoomId = roomId;
		}

		/// <summary>
		/// Implementing default equality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator ==(RoomLayoutInfo a1, RoomLayoutInfo a2)
		{
			return a1.Equals(a2);
		}

		/// <summary>
		/// Implementing default inequality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator !=(RoomLayoutInfo a1, RoomLayoutInfo a2)
		{
			return !a1.Equals(a2);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(RoomLayoutInfo other)
		{
			return m_Position == other.m_Position &&
			       m_RoomId == other.m_RoomId;
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is RoomLayoutInfo && Equals((RoomLayoutInfo)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = m_Position.GetHashCode();
				hashCode = (hashCode * 397) ^ m_RoomId;
				return hashCode;
			}
		}

		public int CompareTo(RoomLayoutInfo other)
		{
			int columnComparison = m_Position.CompareTo(other.m_Position);
			return columnComparison != 0 ? columnComparison : m_RoomId.CompareTo(other.m_RoomId);
		}
	}
}
