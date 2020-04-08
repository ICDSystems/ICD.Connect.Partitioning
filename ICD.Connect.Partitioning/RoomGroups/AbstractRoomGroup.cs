using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Partitioning.RoomGroups
{
	public abstract class AbstractRoomGroup<T> : AbstractOriginator<T>, IRoomGroup where T : IRoomGroupSettings, new()
	{
		private readonly List<IRoom> m_Rooms;

		protected AbstractRoomGroup()
		{
			m_Rooms = new List<IRoom>();
		}

		public IEnumerable<IRoom> GetRooms()
		{
			return m_Rooms.ToArray();
		}

		#region Settings

		protected override void ApplySettingsFinal(T settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_Rooms.Clear();

			foreach (int id in settings.RoomIds)
			{
				try
				{
					m_Rooms.Add(factory.GetOriginatorById<IRoom>(id));
				}
				catch (Exception e)
				{
					Logger.Log(eSeverity.Error, "Failed to add {0} with id {1} - {2}", typeof(T).Name, id, e.Message);
				}
			}
		}

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_Rooms.Clear();
		}

		protected override void CopySettingsFinal(T settings)
		{
			base.CopySettingsFinal(settings);

			settings.RoomIds.Clear();
			settings.RoomIds.AddRange(GetRooms().Select(r => r.Id));
		}

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("ListRooms", "Lists all the rooms in this group", () => ConsoleListRooms());
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		private string ConsoleListRooms()
		{
			TableBuilder builder = new TableBuilder("ID", "Name");
			foreach (IRoom room in GetRooms())
				builder.AddRow(room.Id, room.Name);

			return builder.ToString();
		}

		#endregion
	}
}