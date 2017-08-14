using ICD.Connect.Devices;
using ICD.Connect.Panels;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Groups;
using ICD.Connect.Routing.Endpoints.Sources;

namespace ICD.Connect.Partitioning.Rooms
{
	public sealed class RoomPortIdCollection : AbstractRoomChildIdCollection<IPort>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomPortIdCollection(IRoom room)
			: base(room)
		{
		}
	}

	public sealed class RoomDeviceIdCollection : AbstractRoomChildIdCollection<IDevice>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomDeviceIdCollection(IRoom room)
			: base(room)
		{
		}
	}

	public sealed class RoomPanelIdCollection : AbstractRoomChildIdCollection<IPanelDevice>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomPanelIdCollection(IRoom room)
			: base(room)
		{
		}
	}

	public sealed class RoomSourceIdCollection : AbstractRoomChildIdCollection<ISource>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomSourceIdCollection(IRoom room)
			: base(room)
		{
		}
	}

	public sealed class RoomDestinationIdCollection : AbstractRoomChildIdCollection<IDestination>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomDestinationIdCollection(IRoom room)
			: base(room)
		{
		}
	}

	public sealed class RoomDestinationGroupIdCollection : AbstractRoomChildIdCollection<IDestinationGroup>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomDestinationGroupIdCollection(IRoom room)
			: base(room)
		{
		}
	}

	public sealed class RoomPartitionIdCollection : AbstractRoomChildIdCollection<IPartition>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomPartitionIdCollection(IRoom room)
			: base(room)
		{
		}
	}
}
