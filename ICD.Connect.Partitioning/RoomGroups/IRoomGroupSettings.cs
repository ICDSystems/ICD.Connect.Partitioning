using System.Collections.Generic;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.RoomGroups
{
	public interface IRoomGroupSettings : ISettings
	{
		List<int> RoomIds { get; }
	}
}