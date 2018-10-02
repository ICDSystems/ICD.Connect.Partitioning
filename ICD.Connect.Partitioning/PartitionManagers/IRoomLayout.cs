using System.Collections.Generic;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public interface IRoomLayout
	{
		/// <summary>
		/// Clears the arranged rooms.
		/// </summary>
		void Clear();

		/// <summary>
		/// Gets the room layout info.
		/// </summary>
		/// <returns></returns>
		IEnumerable<RoomLayoutInfo> GetRooms();

		/// <summary>
		/// Sets the room layout info.
		/// </summary>
		/// <param name="layoutInfo"></param>
		void SetRooms(IEnumerable<RoomLayoutInfo> layoutInfo);
	}
}
