using System.Collections.Generic;
using System.Linq;
using ICD.Common.Services;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Rooms
{
	/// <summary>
	/// Base class for rooms.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class AbstractRoom<T> : AbstractOriginator<T>, IRoom, IConsoleNode
		where T : AbstractRoomSettings, new()
	{
		private readonly RoomDeviceIdCollection m_DeviceIds;
		private readonly RoomPortIdCollection m_PortIds;
		private readonly RoomPanelIdCollection m_PanelIds;
		private readonly RoomSourceIdCollection m_SourceIds;
		private readonly RoomDestinationIdCollection m_DestinationIds;
		private readonly RoomDestinationGroupIdCollection m_DestinationGroupIds;

		#region Properties

		public ICore Core { get; private set; }

		public RoomDeviceIdCollection Devices { get { return m_DeviceIds; } }
		public RoomPortIdCollection Ports { get { return m_PortIds; } }
		public RoomPanelIdCollection Panels { get { return m_PanelIds; } }
		public RoomSourceIdCollection Sources { get { return m_SourceIds; } }
		public RoomDestinationIdCollection Destinations { get { return m_DestinationIds; } }
		public RoomDestinationGroupIdCollection DestinationGroups { get { return m_DestinationGroupIds; } }

		/// <summary>
		/// Gets the name of the node in the console.
		/// </summary>
		public virtual string ConsoleName { get { return string.IsNullOrEmpty(Name) ? GetType().Name : Name; } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public virtual string ConsoleHelp { get { return string.Empty; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractRoom()
		{
			m_DeviceIds = new RoomDeviceIdCollection(this);
			m_PortIds = new RoomPortIdCollection(this);
			m_PanelIds = new RoomPanelIdCollection(this);
			m_SourceIds = new RoomSourceIdCollection(this);
			m_DestinationIds = new RoomDestinationIdCollection(this);
			m_DestinationGroupIds = new RoomDestinationGroupIdCollection(this);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			m_DeviceIds.Clear();
			m_PortIds.Clear();
			m_PanelIds.Clear();
			m_SourceIds.Clear();
			m_DestinationIds.Clear();
			m_DestinationGroupIds.Clear();
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(T settings)
		{
			base.CopySettingsFinal(settings);

			settings.Ports.AddRange(m_PortIds.GetIds());
			settings.Devices.AddRange(m_DeviceIds.GetIds());
			settings.Panels.AddRange(m_PanelIds.GetIds());
			settings.Sources.AddRange(m_SourceIds.GetIds());
			settings.Destinations.AddRange(m_DestinationIds.GetIds());
			settings.DestinationGroups.AddRange(m_DestinationGroupIds.GetIds());
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			//Core = null;

			m_DeviceIds.Clear();
			m_PortIds.Clear();
			m_PanelIds.Clear();
			m_SourceIds.Clear();
			m_DestinationIds.Clear();
			m_DestinationGroupIds.Clear();
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(T settings, IDeviceFactory factory)
		{
			// Ensure all dependencies are loaded first.
			factory.LoadOriginators(settings.Devices);
			factory.LoadOriginators(settings.Ports);
			factory.LoadOriginators(settings.Panels);
			factory.LoadOriginators(settings.Sources);
			factory.LoadOriginators(settings.Destinations);
			factory.LoadOriginators(settings.DestinationGroups);

			base.ApplySettingsFinal(settings, factory);

			Core = ServiceProvider.GetService<ICore>();

			m_DeviceIds.SetIds(settings.Devices);
			m_PortIds.SetIds(settings.Ports);
			m_PanelIds.SetIds(settings.Panels);
			m_SourceIds.SetIds(settings.Sources);
			m_DestinationIds.SetIds(settings.Destinations);
			m_DestinationGroupIds.SetIds(settings.DestinationGroups);
		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public virtual void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			addRow("Panel count", m_PanelIds.Count);
			addRow("Device count", m_DeviceIds.Count);
			addRow("Port count", m_PortIds.Count);
			addRow("Source count", m_SourceIds.Count);
			addRow("Destination count", m_DestinationIds.Count);
			addRow("Destination Group count", m_DestinationGroupIds.Count);
		}

		/// <summary>
		/// Gets the child console node groups.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			yield return ConsoleNodeGroup.IndexNodeMap("Panels", m_PanelIds.GetInstances().OfType<IConsoleNode>());
			yield return ConsoleNodeGroup.IndexNodeMap("Devices", m_DeviceIds.GetInstances().OfType<IConsoleNode>());
			yield return ConsoleNodeGroup.IndexNodeMap("Ports", m_PortIds.GetInstances().OfType<IConsoleNode>());
			//yield return ConsoleNodeGroup.IndexNodeMap("Sources", m_SourceIds.GetInstances().OfType<IConsoleNode>());
			//yield return ConsoleNodeGroup.IndexNodeMap("Destinations", m_DestinationIds.GetInstances().OfType<IConsoleNode>());
			//yield return ConsoleNodeGroup.IndexNodeMap("DestinationGroups", m_DestinationGroupIds.GetInstances().OfType<IConsoleNode>());
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			yield break;
		}

		#endregion
	}
}
