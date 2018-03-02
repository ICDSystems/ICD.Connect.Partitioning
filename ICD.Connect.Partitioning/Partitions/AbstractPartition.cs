using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.Partitioning.Partitions
{
	public abstract class AbstractPartition<TSettings> : AbstractOriginator<TSettings>, IPartition
		where TSettings : IPartitionSettings, new()
	{
		private readonly IcdHashSet<DeviceControlInfo> m_Controls;
		private readonly List<int> m_RoomsOrdered;
		private readonly IcdHashSet<int> m_Rooms;
		private readonly SafeCriticalSection m_Section;

		/// <summary>
		/// Gets the number of rooms the partition is adjacent to.
		/// </summary>
		public int RoomsCount { get { return m_Section.Execute(() => m_Rooms.Count); } }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractPartition()
		{
			m_Controls = new IcdHashSet<DeviceControlInfo>();
			m_RoomsOrdered = new List<int>();
			m_Rooms = new IcdHashSet<int>();
			m_Section = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Adds a room as adjacent to this partition.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public bool AddRoom(int roomId)
		{
			m_Section.Enter();

			try
			{
				if (!m_Rooms.Add(roomId))
					return false;

				m_RoomsOrdered.AddSorted(roomId);
				return true;
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Removes the room as adjacent to this partition.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public bool RemoveRoom(int roomId)
		{
			m_Section.Enter();

			try
			{
				if (!m_Rooms.Remove(roomId))
					return false;

				m_RoomsOrdered.Remove(roomId);
				return true;
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Returns true if the given room has been added as adjacent to this partition.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public bool ContainsRoom(int roomId)
		{
			return m_Section.Execute(() => m_Rooms.Contains(roomId));
		}

		/// <summary>
		/// Gets the controls that are associated with this partition.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<DeviceControlInfo> GetPartitionControls()
		{
			return m_Section.Execute(() => m_Controls.ToArray(m_Controls.Count));
		}

		/// <summary>
		/// Returns the rooms that are added as adjacent to this partition.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<int> GetRooms()
		{
			return m_Section.Execute(() => m_RoomsOrdered.ToArray(m_RoomsOrdered.Count));
		}

		/// <summary>
		/// Sets the rooms that are adjacent to this partition.
		/// </summary>
		/// <param name="roomIds"></param>
		public void SetRooms(IEnumerable<int> roomIds)
		{
			m_Section.Enter();

			try
			{
				m_Rooms.Clear();
				m_RoomsOrdered.Clear();

				m_Rooms.AddRange(roomIds);
				m_RoomsOrdered.AddRange(m_Rooms.Order());
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Sets the controls that are associated with this partition.
		/// </summary>
		/// <param name="partitionControls"></param>
		public void SetPartitionControls(IEnumerable<DeviceControlInfo> partitionControls)
		{
			m_Section.Enter();

			try
			{
				m_Controls.Clear();
				m_Controls.AddRange(partitionControls);
			}
			finally
			{
				m_Section.Leave();
			}
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			SetRooms(Enumerable.Empty<int>());
			SetPartitionControls(Enumerable.Empty<DeviceControlInfo>());
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.SetPartitionControls(GetPartitionControls());
			settings.SetRooms(GetRooms());
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			// Load the rooms
			factory.LoadOriginators(settings.GetRooms());

			// Load the partition controls
			factory.LoadOriginators(settings.GetPartitionControls().Select(c => c.DeviceId));

			base.ApplySettingsFinal(settings, factory);

			SetPartitionControls(settings.GetPartitionControls());
			SetRooms(settings.GetRooms());
		}

		#endregion
	}
}
