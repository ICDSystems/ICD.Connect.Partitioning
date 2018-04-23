﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Panels;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Groups;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;
using ICD.Connect.Partitioning.VolumePoints;

namespace ICD.Connect.Partitioning.Rooms
{
	/// <summary>
	/// Base class for rooms.
	/// </summary>
	/// <typeparam name="TSettings"></typeparam>
	public abstract class AbstractRoom<TSettings> : AbstractOriginator<TSettings>, IRoom, IConsoleNode
		where TSettings : IRoomSettings, new()
	{
		public event EventHandler<BoolEventArgs> OnCombineStateChanged;

		private readonly RoomOriginatorIdCollection m_AudioDestinations;
		private readonly RoomOriginatorIdCollection m_OriginatorIds;

		private bool m_CombineState;

		#region Properties

		public ICore Core { get { return ServiceProvider.GetService<ICore>(); } }

		/// <summary>
		/// Returns true if the room is currently behaving as part of a combined room.
		/// </summary>
		public bool CombineState
		{
			get { return m_CombineState; }
			private set
			{
				if (value == m_CombineState)
					return;

				m_CombineState = value;

				Logger.AddEntry(eSeverity.Informational, "{0} combine state changed to {1}", this, m_CombineState);

				OnCombineStateChanged.Raise(this, new BoolEventArgs(m_CombineState));
			}
		}

		/// <summary>
		/// Returns the priority order for combining rooms. Lower is better.
		/// </summary>
		public int CombinePriority { get; set; }

		public RoomOriginatorIdCollection Originators { get { return m_OriginatorIds; } }

		public RoomOriginatorIdCollection AudioDestinations { get { return m_AudioDestinations; } }

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
			m_OriginatorIds = new RoomOriginatorIdCollection(this);
			m_AudioDestinations = new RoomOriginatorIdCollection(this);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnCombineStateChanged = null;

			base.DisposeFinal(disposing);

			m_OriginatorIds.Clear();
		}

		/// <summary>
		/// Informs the room it is part of a combined room.
		/// </summary>
		public void EnterCombineState()
		{
			CombineState = true;
		}

		/// <summary>
		/// Informs the room it is no longer part of a combined room.
		/// </summary>
		public void LeaveCombineState()
		{
			CombineState = false;
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
			settings.Panels.Clear();
			settings.Sources.Clear();
			settings.Destinations.Clear();
			settings.DestinationGroups.Clear();
			settings.Partitions.Clear();
			settings.VolumePoints.Clear();

			settings.Ports.AddRange(GetChildren<IPort>());
			settings.Devices.AddRange(GetChildren<IDevice>());
			settings.Panels.AddRange(GetChildren<IPanelDevice>());
			settings.Sources.AddRange(GetChildren<ISource>());
			settings.Destinations.AddRange(GetChildren<IDestination>());
			settings.AudioDestinations.AddRange(GetChildren<IDestination>().Where(kvp => m_AudioDestinations.Contains(kvp.Key)));
			settings.DestinationGroups.AddRange(GetChildren<IDestinationGroup>());
			settings.Partitions.AddRange(GetChildren<IPartition>());
			settings.VolumePoints.AddRange(GetChildren<IVolumePoint>());
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			CombinePriority = 0;

			m_AudioDestinations.Clear();
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
			AddOriginatorsSkipExceptions<IPanelDevice>(settings.Panels, factory);
			AddOriginatorsSkipExceptions<ISource>(settings.Sources, factory);
			AddOriginatorsSkipExceptions<IDestination>(settings.AudioDestinations, factory);
			AddOriginatorsSkipExceptions<IDestination>(settings.Destinations, factory);
			AddOriginatorsSkipExceptions<IDestinationGroup>(settings.DestinationGroups, factory);
			AddOriginatorsSkipExceptions<IPartition>(settings.Partitions, factory);
			AddOriginatorsSkipExceptions<IVolumePoint>(settings.VolumePoints, factory);

			// Audio destinations are loaded like normal destinations and then we keep a seperate list to differentiate them.
			AddOriginatorsSkipExceptions<IDestination>(settings.AudioDestinations, factory, m_AudioDestinations);
		}

		private IEnumerable<KeyValuePair<int, eCombineMode>> GetChildren<TInstance>()
			where TInstance : IOriginator
		{
			return m_OriginatorIds.GetInstances<TInstance>()
			                      .Select(p => new KeyValuePair<int, eCombineMode>(p.Id, m_OriginatorIds.GetCombineMode(p.Id)));
		}

		private void AddOriginatorsSkipExceptions<T>(IEnumerable<KeyValuePair<int, eCombineMode>> originatorIds, IDeviceFactory factory)
			where T : class, IOriginator
		{
			AddOriginatorsSkipExceptions<T>(originatorIds, factory, Originators);
		}

		private void AddOriginatorsSkipExceptions<T>(IEnumerable<KeyValuePair<int, eCombineMode>> originatorIds, IDeviceFactory factory, RoomOriginatorIdCollection originators)
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
					Logger.AddEntry(eSeverity.Error, "{0} failed to add {1} with id {2} - {3}", this, typeof(T).Name, kvp.Key, e.Message);
					continue;
				}

				originators.Add(kvp.Key, kvp.Value);
			}
		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public virtual void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			addRow("Combine Priority", CombinePriority);
		}

		/// <summary>
		/// Gets the child console node groups.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			yield return ConsoleNodeGroup.KeyNodeMap("Panels", Originators.GetInstances<IPanelDevice>().OfType<IConsoleNode>(), p => (uint)((IPanelDevice)p).Id);
			yield return ConsoleNodeGroup.KeyNodeMap("Devices", Originators.GetInstances<IDevice>().OfType<IConsoleNode>(), p => (uint)((IDevice)p).Id);
			yield return ConsoleNodeGroup.KeyNodeMap("Ports", Originators.GetInstances<IPort>().OfType<IConsoleNode>(), p => (uint)((IPort)p).Id);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			yield return
				new GenericConsoleCommand<int>("SetCombinePriority", "SetCombinePriority <PRIORITY>", i => CombinePriority = i);
		}

		#endregion
	}
}
