﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Services.Logging;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Services;
using ICD.Common.Utils.Extensions;
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

			settings.Ports.AddRange(Originators.GetInstances<IPort>().Select(p => p.Id));
			settings.Devices.AddRange(Originators.GetInstances<IDevice>().Select(p => p.Id));
			settings.Panels.AddRange(Originators.GetInstances<IPanelDevice>().Select(p => p.Id));
			settings.Sources.AddRange(Originators.GetInstances<ISource>().Select(p => p.Id));
			settings.Destinations.AddRange(Originators.GetInstances<IDestination>().Select(p => p.Id));
			settings.DestinationGroups.AddRange(Originators.GetInstances<IDestinationGroup>().Select(p => p.Id));
			settings.Partitions.AddRange(Originators.GetInstances<IPartition>().Select(p => p.Id));
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			CombinePriority = 0;

			m_OriginatorIds.Clear();
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			// Ensure all dependencies are loaded first.
			factory.LoadOriginators(settings.Devices);
			factory.LoadOriginators(settings.Ports);
			factory.LoadOriginators(settings.Panels);
			factory.LoadOriginators(settings.Sources);
			factory.LoadOriginators(settings.Destinations);
			factory.LoadOriginators(settings.DestinationGroups);
			factory.LoadOriginators(settings.Partitions);

			base.ApplySettingsFinal(settings, factory);

			CombinePriority = settings.CombinePriority;

			Originators.AddRange(settings.Devices);
			Originators.AddRange(settings.Ports);
			Originators.AddRange(settings.Panels);
			Originators.AddRange(settings.Sources);
			Originators.AddRange(settings.Destinations);
			Originators.AddRange(settings.DestinationGroups);
			Originators.AddRange(settings.Partitions);
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
