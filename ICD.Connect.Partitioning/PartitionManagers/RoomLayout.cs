using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public sealed class RoomLayout : IRoomLayout
	{
		private readonly BiDictionary<RoomPositionInfo, int> m_Rooms;
		private readonly SafeCriticalSection m_RoomsSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		public RoomLayout(PartitionManager parent)
		{
			m_Rooms = new BiDictionary<RoomPositionInfo, int>();
			m_RoomsSection = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Clears the arranged rooms.
		/// </summary>
		public void Clear()
		{
			m_RoomsSection.Execute(() => m_Rooms.Clear());
		}

		/// <summary>
		/// Gets the room layout info.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<RoomLayoutInfo> GetRooms()
		{
			m_RoomsSection.Enter();

			try
			{
				return m_Rooms.Select(kvp => new RoomLayoutInfo(kvp.Key, kvp.Value))
				              .ToArray(m_Rooms.Count);
			}
			finally
			{
				m_RoomsSection.Leave();
			}
		}

		/// <summary>
		/// Sets the room layout info.
		/// </summary>
		/// <param name="layoutInfo"></param>
		public void SetRooms(IEnumerable<RoomLayoutInfo> layoutInfo)
		{
			if (layoutInfo == null)
				throw new ArgumentNullException("layoutInfo");

			m_RoomsSection.Enter();

			try
			{
				Clear();

				foreach (RoomLayoutInfo info in layoutInfo)
					m_Rooms.Add(info.Position, info.RoomId);
			}
			catch (Exception)
			{
				Clear();
				throw;
			}
			finally
			{
				m_RoomsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the room id at the given position.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public int GetRoom(RoomPositionInfo position)
		{
			return m_RoomsSection.Execute(() => m_Rooms.GetValue(position));
		}

		/// <summary>
		/// Try to get the room id at the given position.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public bool TryGetRoom(RoomPositionInfo position, out int roomId)
		{
			m_RoomsSection.Enter();

			try
			{
				return m_Rooms.TryGetValue(position, out roomId);
			}
			finally
			{
				m_RoomsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the position for the room with the given id.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public RoomPositionInfo GetRoomPosition(int roomId)
		{
			return m_RoomsSection.Execute(() => m_Rooms.GetKey(roomId));
		}

		/// <summary>
		/// Try to get the position for the room with the given id.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public bool TryGetRoomPosition(int roomId, out RoomPositionInfo position)
		{
			m_RoomsSection.Enter();

			try
			{
				return m_Rooms.TryGetKey(roomId, out position);
			}
			finally
			{
				m_RoomsSection.Leave();
			}
		}

		#endregion
	}
}
