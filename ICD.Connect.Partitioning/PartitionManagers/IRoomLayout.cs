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

		/// <summary>
		/// Gets the room id at the given position.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		int GetRoom(RoomPositionInfo position);

		/// <summary>
		/// Try to get the room id at the given position.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		bool TryGetRoom(RoomPositionInfo position, out int roomId);

		/// <summary>
		/// Gets the position for the room with the given id.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		RoomPositionInfo GetRoomPosition(int roomId);

		/// <summary>
		/// Try to get the position for the room with the given id.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		bool TryGetRoomPosition(int roomId, out RoomPositionInfo position);
	}
}
