using ICD.Connect.Devices;
using ICD.Connect.Devices.Extensions;
using ICD.Connect.Panels;
using ICD.Connect.Panels.Extensions;
using ICD.Connect.Protocol.Extensions;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Groups;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Routing.Extensions;

namespace ICD.Connect.Rooms
{
	public sealed class RoomPortIdCollection : AbstractRoomChildIdCollection<IPort>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomPortIdCollection(IRoom room)
			: base(() => room.Core.GetPorts())
		{
		}
	}

	public sealed class RoomDeviceIdCollection : AbstractRoomChildIdCollection<IDevice>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomDeviceIdCollection(IRoom room)
			: base(() => room.Core.GetDevices())
		{
		}
	}

	public sealed class RoomPanelIdCollection : AbstractRoomChildIdCollection<IPanelDevice>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomPanelIdCollection(IRoom room)
			: base(() => room.Core.GetPanels())
		{
		}
	}

	public sealed class RoomSourceIdCollection : AbstractRoomChildIdCollection<ISource>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomSourceIdCollection(IRoom room)
			: base(() => room.Core.GetRoutingGraph().Sources)
		{
		}
	}

	public sealed class RoomDestinationIdCollection : AbstractRoomChildIdCollection<IDestination>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomDestinationIdCollection(IRoom room)
			: base(() => room.Core.GetRoutingGraph().Destinations)
		{
		}
	}

	public sealed class RoomDestinationGroupIdCollection : AbstractRoomChildIdCollection<IDestinationGroup>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomDestinationGroupIdCollection(IRoom room)
			: base(() => room.Core.GetRoutingGraph().DestinationGroups)
		{
		}
	}
}
