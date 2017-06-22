using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Rooms.Extensions
{
	public sealed class CoreRoomCollection : AbstractOriginatorCollection<IRoom>
	{
		public CoreRoomCollection()
		{	
		}

		public CoreRoomCollection(IEnumerable<IRoom> children) : base(children)
		{
		}
	}

	public static class CoreExtensions
	{
		public static CoreRoomCollection GetRooms(this ICore core)
		{
			return new CoreRoomCollection(core.Originators.OfType<IRoom>());
		}
	}
}