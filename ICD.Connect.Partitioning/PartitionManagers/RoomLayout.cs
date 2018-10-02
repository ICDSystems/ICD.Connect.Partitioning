using System.Collections.Generic;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public sealed class RoomLayout : IRoomLayout
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		public RoomLayout(PartitionManager parent)
		{
		}

		/// <summary>
		/// Clears the arranged rooms.
		/// </summary>
		public void Clear()
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Gets the room layout info.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<RoomLayoutInfo> GetRooms()
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Sets the room layout info.
		/// </summary>
		/// <param name="layoutInfo"></param>
		public void SetRooms(IEnumerable<RoomLayoutInfo> layoutInfo)
		{
			throw new System.NotImplementedException();
		}
	}
}
