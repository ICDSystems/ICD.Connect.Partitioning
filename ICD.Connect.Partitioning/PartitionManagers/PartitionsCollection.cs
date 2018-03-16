using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Partitioning.Controls;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public sealed class PartitionsCollection : AbstractOriginatorCollection<IPartition>, IPartitionsCollection
	{
		private readonly Dictionary<DeviceControlInfo, IcdHashSet<IPartition>> m_ControlPartitions;
		private readonly Dictionary<int, IcdHashSet<IPartition>> m_RoomAdjacentPartitions;
		private readonly Dictionary<IPartition, IcdHashSet<IPartition>> m_PartitionAdjacentPartitions; 

		private readonly SafeCriticalSection m_PartitionsSection;

		private readonly PartitionManager m_Manager;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="manager"></param>
		public PartitionsCollection(PartitionManager manager)
		{
			m_Manager = manager;

			m_RoomAdjacentPartitions = new Dictionary<int, IcdHashSet<IPartition>>();
			m_ControlPartitions = new Dictionary<DeviceControlInfo, IcdHashSet<IPartition>>();
			m_PartitionAdjacentPartitions = new Dictionary<IPartition, IcdHashSet<IPartition>>();

			m_PartitionsSection = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Gets the partitions related to the given control.
		/// </summary>
		/// <param name="deviceControlInfo"></param>
		public IEnumerable<IPartition> GetPartitions(DeviceControlInfo deviceControlInfo)
		{
			m_PartitionsSection.Enter();

			try
			{
				return m_ControlPartitions.ContainsKey(deviceControlInfo)
					       ? m_ControlPartitions[deviceControlInfo]
					       : Enumerable.Empty<IPartition>();
			}
			finally
			{
				m_PartitionsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the partitions related to the given control.
		/// </summary>
		/// <param name="deviceControl"></param>
		/// <returns></returns>
		public IEnumerable<IPartition> GetPartitions(IPartitionDeviceControl deviceControl)
		{
			if (deviceControl == null)
				throw new ArgumentNullException("deviceControl");

			return GetPartitions(deviceControl.DeviceControlInfo);
		}

		/// <summary>
		/// Gets the partitions related to the given controls.
		/// </summary>
		/// <param name="deviceControls"></param>
		/// <returns></returns>
		public IEnumerable<IPartition> GetPartitions(IEnumerable<IPartitionDeviceControl> deviceControls)
		{
			if (deviceControls == null)
				throw new ArgumentNullException("deviceControls");

			return deviceControls.SelectMany(d => GetPartitions(d)).Distinct();
		}

		/// <summary>
		/// Gets the immediately adjacent partitions for the given partition.
		/// </summary>
		/// <param name="partition"></param>
		/// <returns></returns>
		public IEnumerable<IPartition> GetAdjacentPartitions(IPartition partition)
		{
			if (partition == null)
				throw new ArgumentNullException("partition");

			m_PartitionsSection.Enter();

			try
			{
				return m_PartitionAdjacentPartitions.ContainsKey(partition)
					? m_PartitionAdjacentPartitions[partition].ToArray()
					: Enumerable.Empty<IPartition>();
			}
			finally
			{
				m_PartitionsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the partitions adjacent to the given room.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IPartition> GetRoomAdjacentPartitions(IRoom room)
		{
			if (room == null)
				throw new ArgumentNullException("room");

			return GetRoomAdjacentPartitions(room.Id);
		}

		/// <summary>
		/// Gets the partitions adjacent to the given room.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IPartition> GetRoomAdjacentPartitions(int roomId)
		{
			m_PartitionsSection.Enter();

			try
			{
				return m_RoomAdjacentPartitions.ContainsKey(roomId)
					       ? m_RoomAdjacentPartitions[roomId].ToArray()
					       : Enumerable.Empty<IPartition>();
			}
			finally
			{
				m_PartitionsSection.Leave();
			}
		}

		/// <summary>
		/// Given a sequence of partitions and a partition to split on, returns the remaining congruous groups of adjacent partitions.
		/// </summary>
		/// <param name="partitions"></param>
		/// <param name="split"></param>
		/// <returns></returns>
		public IEnumerable<IEnumerable<IPartition>> SplitAdjacentPartitionsByPartition(IEnumerable<IPartition> partitions,
		                                                                    IPartition split)
		{
			return RecursionUtils.GetCliques(partitions.Except(split), GetAdjacentPartitions);
		}

		#endregion

		protected override void ChildAdded(IPartition child)
		{
			base.ChildAdded(child);

			m_PartitionsSection.Enter();

			try
			{
				// Build room adjacency lookup
				foreach (int room in child.GetRooms())
				{
					if (!m_RoomAdjacentPartitions.ContainsKey(room))
						m_RoomAdjacentPartitions.Add(room, new IcdHashSet<IPartition>());
					m_RoomAdjacentPartitions[room].Add(child);
				}

				// Build control to partition lookup
				foreach (DeviceControlInfo partitionControl in child.GetPartitionControls())
				{
					if (!m_ControlPartitions.ContainsKey(partitionControl))
						m_ControlPartitions.Add(partitionControl, new IcdHashSet<IPartition>());
					m_ControlPartitions[partitionControl].Add(child);
				}

				// Build partition adjacency lookup
				if (!m_PartitionAdjacentPartitions.ContainsKey(child))
					m_PartitionAdjacentPartitions.Add(child, new IcdHashSet<IPartition>());

				IEnumerable<IPartition> adjacent =
					GetChildren().Except(child)
					             .Where(partition =>
					                    partition.GetRooms()
					                             .Intersect(child.GetRooms())
					                             .Any());

				foreach (IPartition partition in adjacent)
				{
					m_PartitionAdjacentPartitions[child].Add(partition);
					m_PartitionAdjacentPartitions[partition].Add(child);
				}
			}
			finally
			{
				m_PartitionsSection.Leave();
			}
		}

		/// <summary>
		/// Called each time a child is removed from the collection before any events are raised.
		/// </summary>
		/// <param name="child"></param>
		protected override void ChildRemoved(IPartition child)
		{
			base.ChildRemoved(child);

			m_PartitionsSection.Enter();

			try
			{
				foreach (KeyValuePair<int, IcdHashSet<IPartition>> kvp in m_RoomAdjacentPartitions)
					kvp.Value.Remove(child);

				foreach (KeyValuePair<DeviceControlInfo, IcdHashSet<IPartition>> kvp in m_ControlPartitions)
					kvp.Value.Remove(child);

				m_PartitionAdjacentPartitions.Remove(child);
				foreach (KeyValuePair<IPartition, IcdHashSet<IPartition>> kvp in m_PartitionAdjacentPartitions)
					kvp.Value.Remove(child);
			}
			finally
			{
				m_PartitionsSection.Leave();
			}
		}
	}
}
