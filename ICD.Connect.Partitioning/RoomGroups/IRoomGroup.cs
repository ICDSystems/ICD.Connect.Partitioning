using System.Collections.Generic;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Partitioning.RoomGroups
{
	public interface IRoomGroup : IOriginator
	{
		IEnumerable<IRoom> GetRooms();
	}
}