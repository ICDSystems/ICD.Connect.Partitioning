using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Logging.Activities;
using ICD.Common.Logging.LoggingContexts;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Audio.VolumePoints;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Routing.Groups.Endpoints.Destinations;
using ICD.Connect.Routing.Groups.Endpoints.Sources;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Partitioning.Rooms
{
	/// <summary>
	/// Base class for rooms.
	/// </summary>
	/// <typeparam name="TSettings"></typeparam>
	public abstract class AbstractRoom<TSettings> : AbstractOriginator<TSettings>, IRoom
		where TSettings : IRoomSettings, new()
	{
		/// <summary>
		/// Raised when the room combine state changes.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnCombineStateChanged;

		/// <summary>
		/// Raised when the current volume context changes.
		/// </summary>
		public event EventHandler<GenericEventArgs<eVolumePointContext>> OnVolumeContextChanged;

		private readonly RoomOriginatorIdCollection m_OriginatorIds;

		private bool m_CombineState;
		private eVolumePointContext m_VolumeContext;

		#region Properties

		/// <summary>
		/// Gets the category for this originator type (e.g. Device, Port, etc)
		/// </summary>
		public override string Category { get { return "Room"; } }

		/// <summary>
		/// Returns true if the room is currently behaving as part of a combined room.
		/// </summary>
		public bool CombineState
		{
			get { return m_CombineState; }
			private set
			{
				try
				{
					if (value == m_CombineState)
						return;

					m_CombineState = value;

					Logger.LogSetTo(eSeverity.Informational, "CombineState", m_CombineState);

					HandleCombineState();

					OnCombineStateChanged.Raise(this, new BoolEventArgs(m_CombineState));
				}
				finally
				{
					Activities.LogActivity(m_CombineState
						                       ? new Activity(Activity.ePriority.Medium, "Combined", "Combined", eSeverity.Informational)
						                       : new Activity(Activity.ePriority.Lowest, "Combined", "Uncombined", eSeverity.Informational));
				}
			}
		}

		/// <summary>
		/// Returns the priority order for combining rooms. Lower is better.
		/// </summary>
		public int CombinePriority { get; set; }

		/// <summary>
		/// Gets the originators that are contained within this room.
		/// </summary>
		public RoomOriginatorIdCollection Originators { get { return m_OriginatorIds; } }

		/// <summary>
		/// Gets the current volume context.
		/// </summary>
		public eVolumePointContext VolumeContext
		{
			get { return m_VolumeContext; }
			protected set
			{
				if (value == m_VolumeContext)
					return;

				m_VolumeContext = value;

				OnVolumeContextChanged.Raise(this, new GenericEventArgs<eVolumePointContext>(m_VolumeContext));
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractRoom()
		{
			m_OriginatorIds = new RoomOriginatorIdCollection(this);
			m_OriginatorIds.OnCollectionChanged += OriginatorsOnCollectionChanged;

			VolumeContext = eVolumePointContext.Room;

			// Initialize activities
			CombineState = false;
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnCombineStateChanged = null;
			OnVolumeContextChanged = null;

			base.DisposeFinal(disposing);

			m_OriginatorIds.Clear();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Informs the room it is part of a combined room.
		/// </summary>
		/// <param name="combine"></param>
		public void EnterCombineState(bool combine)
		{
			CombineState = combine;
		}

		/// <summary>
		/// Called before this combine space is destroyed as part of an uncombine operation.
		/// </summary>
		public virtual void HandlePreUncombine()
		{
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Called when the room combine state changes.
		/// </summary>
		protected virtual void HandleCombineState()
		{
		}

		/// <summary>
		/// Called when an originator is added to/removed from the room.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected virtual void OriginatorsOnCollectionChanged(object sender, EventArgs args)
		{
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.CombinePriority = CombinePriority;

			settings.Ports.Clear();
			settings.Devices.Clear();
			settings.Sources.Clear();
			settings.Destinations.Clear();
			settings.SourceGroups.Clear();
			settings.DestinationGroups.Clear();
			settings.VolumePoints.Clear();

			settings.Ports.AddRange(GetSerializableChildren<IPort>());
			settings.Devices.AddRange(GetSerializableChildren<IDevice>());
			settings.Sources.AddRange(GetSerializableChildren<ISource>());
			settings.Destinations.AddRange(GetSerializableChildren<IDestination>());
			settings.SourceGroups.AddRange(GetSerializableChildren<ISourceGroup>());
			settings.DestinationGroups.AddRange(GetSerializableChildren<IDestinationGroup>());
			settings.VolumePoints.AddRange(GetSerializableChildren<IVolumePoint>());
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			CombinePriority = 0;
			VolumeContext = eVolumePointContext.Room;

			m_OriginatorIds.Clear();
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			CombinePriority = settings.CombinePriority;

			AddOriginatorsSkipExceptions<IDevice>(settings.Devices, factory);
			AddOriginatorsSkipExceptions<IPort>(settings.Ports, factory);
			AddOriginatorsSkipExceptions<ISource>(settings.Sources, factory);
			AddOriginatorsSkipExceptions<IDestination>(settings.Destinations, factory);
			AddOriginatorsSkipExceptions<ISourceGroup>(settings.SourceGroups, factory);
			AddOriginatorsSkipExceptions<IDestinationGroup>(settings.DestinationGroups, factory);
			AddOriginatorsSkipExceptions<IVolumePoint>(settings.VolumePoints, factory);
		}

		protected IEnumerable<KeyValuePair<int, eCombineMode>> GetSerializableChildren<TInstance>()
			where TInstance : class, IOriginator
		{
			return m_OriginatorIds.GetInstances<TInstance>().Where(i => i.Serialize)
			                      .Select(p => new KeyValuePair<int, eCombineMode>(p.Id, m_OriginatorIds.GetCombineMode(p.Id)));
		}

		protected void AddOriginatorsSkipExceptions<T>(IEnumerable<KeyValuePair<int, eCombineMode>> originatorIds, IDeviceFactory factory)
			where T : class, IOriginator
		{
			foreach (KeyValuePair<int, eCombineMode> kvp in originatorIds)
			{
				try
				{
					factory.GetOriginatorById<T>(kvp.Key);
				}
				catch (Exception e)
				{
					Logger.Log(eSeverity.Error, "Failed to add {0} with id {1} - {2}", typeof(T).Name, kvp.Key, e.Message);
					continue;
				}

				Originators.Add(kvp.Key, kvp.Value);
			}
		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			RoomConsole.BuildConsoleStatus(this, addRow);
		}

		/// <summary>
		/// Gets the child console node groups.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase node in GetBaseConsoleNodes())
				yield return node;

			foreach (IConsoleNodeBase node in RoomConsole.GetConsoleNodes(this))
				yield return node;
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			foreach (IConsoleCommand command in RoomConsole.GetConsoleCommands(this))
				yield return command;
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
