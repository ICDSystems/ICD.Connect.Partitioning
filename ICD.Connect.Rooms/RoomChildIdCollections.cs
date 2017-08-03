using System;
using ICD.Connect.Devices;
using ICD.Connect.Panels;
using ICD.Connect.Partitions;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Groups;
using ICD.Connect.Routing.Endpoints.Sources;

namespace ICD.Connect.Rooms
{
	public sealed class RoomPortIdCollection : AbstractRoomChildIdCollection<RoomPortIdCollection, IPort>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomPortIdCollection(IRoom room)
			: base(room)
		{
		}

		/// <summary>
		/// Gets the equivalent collection from the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		protected override RoomPortIdCollection GetCollection(IRoom room)
		{
			if (room == null)
				throw new ArgumentNullException("room");

			return room.Ports;
		}
	}

	public sealed class RoomDeviceIdCollection : AbstractRoomChildIdCollection<RoomDeviceIdCollection, IDevice>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomDeviceIdCollection(IRoom room)
			: base(room)
		{
		}

		/// <summary>
		/// Gets the equivalent collection from the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		protected override RoomDeviceIdCollection GetCollection(IRoom room)
		{
			if (room == null)
				throw new ArgumentNullException("room");

			return room.Devices;
		}
	}

	public sealed class RoomPanelIdCollection : AbstractRoomChildIdCollection<RoomPanelIdCollection, IPanelDevice>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomPanelIdCollection(IRoom room)
			: base(room)
		{
		}

		/// <summary>
		/// Gets the equivalent collection from the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		protected override RoomPanelIdCollection GetCollection(IRoom room)
		{
			if (room == null)
				throw new ArgumentNullException("room");

			return room.Panels;
		}
	}

	public sealed class RoomSourceIdCollection : AbstractRoomChildIdCollection<RoomSourceIdCollection, ISource>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomSourceIdCollection(IRoom room)
			: base(room)
		{
		}

		/// <summary>
		/// Gets the equivalent collection from the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		protected override RoomSourceIdCollection GetCollection(IRoom room)
		{
			if (room == null)
				throw new ArgumentNullException("room");

			return room.Sources;
		}
	}

	public sealed class RoomDestinationIdCollection : AbstractRoomChildIdCollection<RoomDestinationIdCollection, IDestination>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomDestinationIdCollection(IRoom room)
			: base(room)
		{
		}

		/// <summary>
		/// Gets the equivalent collection from the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		protected override RoomDestinationIdCollection GetCollection(IRoom room)
		{
			if (room == null)
				throw new ArgumentNullException("room");

			return room.Destinations;
		}
	}

	public sealed class RoomDestinationGroupIdCollection : AbstractRoomChildIdCollection<RoomDestinationGroupIdCollection, IDestinationGroup>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomDestinationGroupIdCollection(IRoom room)
			: base(room)
		{
		}

		/// <summary>
		/// Gets the equivalent collection from the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		protected override RoomDestinationGroupIdCollection GetCollection(IRoom room)
		{
			if (room == null)
				throw new ArgumentNullException("room");

			return room.DestinationGroups;
		}
	}

	public sealed class RoomPartitionIdCollection : AbstractRoomChildIdCollection<RoomPartitionIdCollection, IPartition>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomPartitionIdCollection(IRoom room)
			: base(room)
		{
		}

		/// <summary>
		/// Gets the equivalent collection from the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		protected override RoomPartitionIdCollection GetCollection(IRoom room)
		{
			if (room == null)
				throw new ArgumentNullException("room");

			return room.Partitions;
		}
	}
}
