using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Partitioning.Partitions
{
	public abstract class AbstractPartition<TSettings> : AbstractOriginator<TSettings>, IPartition
		where TSettings : IPartitionSettings, new()
	{
		private readonly IcdHashSet<int> m_Rooms;
		private readonly SafeCriticalSection m_RoomsSection;

		/// <summary>
		/// Gets/sets the optional control for this partition.
		/// </summary>
		public DeviceControlInfo PartitionControl { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractPartition()
		{
			m_Rooms = new IcdHashSet<int>();
			m_RoomsSection = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Adds a room as adjacent to this partition.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public bool AddRoom(int roomId)
		{
			return m_RoomsSection.Execute(() => m_Rooms.Add(roomId));
		}

		/// <summary>
		/// Removes the room as adjacent to this partition.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public bool RemoveRoom(int roomId)
		{
			return m_RoomsSection.Execute(() => m_Rooms.Remove(roomId));
		}

		/// <summary>
		/// Returns true if the given room has been added as adjacent to this partition.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public bool ContainsRoom(int roomId)
		{
			return m_RoomsSection.Execute(() => m_Rooms.Contains(roomId));
		}

		/// <summary>
		/// Returns the rooms that are added as adjacent to this partition.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<int> GetRooms()
		{
			return m_RoomsSection.Execute(() => m_Rooms.ToArray());
		}

		/// <summary>
		/// Sets the rooms that are adjacent to this partition.
		/// </summary>
		/// <param name="roomIds"></param>
		public void SetRooms(IEnumerable<int> roomIds)
		{
			m_RoomsSection.Enter();

			try
			{
				m_Rooms.Clear();
				m_Rooms.AddRange(roomIds);
			}
			finally
			{
				m_RoomsSection.Leave();
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

			PartitionControl = default(DeviceControlInfo);
			m_RoomsSection.Execute(() => m_Rooms.Clear());
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Device = PartitionControl.DeviceId;
			settings.Control = PartitionControl.ControlId;
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

			base.ApplySettingsFinal(settings, factory);

			PartitionControl = new DeviceControlInfo(settings.Device, settings.Control);
			SetRooms(settings.GetRooms());
		}

		#endregion
	}
}
