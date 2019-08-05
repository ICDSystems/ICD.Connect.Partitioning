using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.Extensions;
using ICD.Connect.Partitioning.Controls;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Cores;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public sealed class PartitionManager : AbstractPartitionManager<PartitionManagerSettings>
	{
		/// <summary>
		/// Raised when a parition control opens/closes.
		/// </summary>
		public override event PartitionControlOpenStateCallback OnPartitionOpenStateChange;

		private readonly PartitionsCollection m_Partitions;
		private readonly RoomLayout m_RoomLayout;
		private readonly IcdHashSet<IPartitionDeviceControl> m_SubscribedPartitions;

		#region Properties

		private static ICore Core { get { return ServiceProvider.GetService<ICore>(); } }

		public override IPartitionsCollection Partitions { get { return m_Partitions; } }

		/// <summary>
		/// Gets the layout of rooms in the system.
		/// </summary>
		public override IRoomLayout RoomLayout { get { return m_RoomLayout; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public PartitionManager()
		{
			m_Partitions = new PartitionsCollection(this);
			m_RoomLayout = new RoomLayout(this);
			m_SubscribedPartitions = new IcdHashSet<IPartitionDeviceControl>();

			ServiceProvider.AddService<IPartitionManager>(this);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnPartitionOpenStateChange = null;

			base.DisposeFinal(disposing);

			ServiceProvider.RemoveService<IPartitionManager>(this);
		}

		#region Controls

		/// <summary>
		/// Gets the controls for the given partition.
		/// </summary>
		/// <param name="partition"></param>
		/// <returns></returns>
		public override IEnumerable<IPartitionDeviceControl> GetControls(IPartition partition)
		{
			if (partition == null)
				throw new ArgumentNullException("partition");

			foreach (DeviceControlInfo info in partition.GetPartitionControls())
			{
				IPartitionDeviceControl control;

				try
				{
					control = Core.GetControl<IPartitionDeviceControl>(info);
				}
				catch (Exception e)
				{
					Log(eSeverity.Error, "Unable to get partition control for {0} - {1}", info, e.Message);
					continue;
				}

				yield return control;
			}
		}

		#endregion

		#region Combine

		/// <summary>
		/// Returns true if the given partition is currently part of a combine room.
		/// </summary>
		/// <param name="partition"></param>
		/// <returns></returns>
		public override bool CombinesRoom(IPartition partition)
		{
			if (partition == null)
				throw new ArgumentNullException("partition");

			return CombinesRoom(partition.Id);
		}

		/// <summary>
		/// Returns true if the given partition is currently part of a combine room.
		/// </summary>
		/// <param name="partitionId"></param>
		/// <returns></returns>
		public override bool CombinesRoom(int partitionId)
		{
			return GetRooms().Any(r => r.Originators.ContainsRecursive(partitionId));
		}

		/// <summary>
		/// Gets the combine room containing the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IRoom GetCombineRoom(IRoom room)
		{
			if (room == null)
				throw new ArgumentNullException("room");

			return GetRooms().FirstOrDefault(r => r.ContainsRoom(room));
		}

		/// <summary>
		/// Gets the combine room containing the given partition.
		/// </summary>
		/// <param name="partition"></param>
		/// <returns></returns>
		public override IRoom GetCombineRoom(IPartition partition)
		{
			if (partition == null)
				throw new ArgumentNullException("partition");

			return GetRooms().FirstOrDefault(r => r.Originators.ContainsRecursive(partition.Id));
		}

		/// <summary>
		/// Gets the combine rooms containing any of the given partitions.
		/// </summary>
		/// <param name="partitions"></param>
		/// <returns></returns>
		public override IEnumerable<IRoom> GetCombineRooms(IEnumerable<IPartition> partitions)
		{
			if (partitions == null)
				throw new ArgumentNullException("partitions");

			return partitions.Select(p => GetCombineRoom(p))
			                 .Except((IRoom)null)
			                 .Distinct();
		}

		/// <summary>
		/// Returns combine rooms and any individual rooms that are not part of a combined space.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IRoom> GetTopLevelRooms()
		{
			IRoom[] rooms = GetRooms().OrderByDescending(r => r.Originators.GetInstances<IPartition>().Count()).ToArray();
			IcdHashSet<IRoom> visited = new IcdHashSet<IRoom>();

			foreach (IRoom room in rooms)
			{
				if (visited.Contains(room))
					continue;

				visited.Add(room);
				visited.AddRange(room.GetRoomsRecursive());

				yield return room;
			}
		}

		/// <summary>
		/// Toggles the given partition to create a new combine room or uncombine an existing room.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partition"></param>
		/// <param name="func"></param>
		public override void ToggleCombineRooms<TRoom>(IPartition partition, Func<TRoom> func)
		{
			IRoom room = GetCombineRoom(partition);

			if (room == null)
				CombineRooms(partition, func);
			else
				UncombineRooms(partition, func);
		}

		/// <summary>
		/// Performs a pass of the partitions, creating combine rooms where partitions are open and
		/// destroying combine rooms where partitions are closed.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="constructor"></param>
		public override void InitializeCombineRooms<TRoom>(Func<TRoom> constructor)
		{
			if (constructor == null)
				throw new ArgumentNullException("constructor");

			IcdHashSet<IPartition> open = Partitions.Where(InitializeOpen).ToIcdHashSet();
			IcdHashSet<IPartition> closed = Partitions.Except(open).ToIcdHashSet();

			UncombineRooms(closed, constructor);
			CombineRooms(open, constructor);
		}

		/// <summary>
		/// Returns true if the given partition should be considered open for the InitializeCombineRooms pass.
		/// </summary>
		/// <param name="partition"></param>
		/// <returns></returns>
		private bool InitializeOpen(IPartition partition)
		{
			if (partition == null)
				throw new ArgumentNullException("partition");

			IcdHashSet<IPartitionDeviceControl> controls = GetControls(partition).ToIcdHashSet();
			if (controls.Count == 0)
				return false;

			// Simple case
			bool result;
			if (controls.Select(c => c.IsOpen).Unanimous(out result))
				return result;

			// During uncombining when a partition has more than 1 control we hit the following situation:
			//	- Control A is closed, Control B is still open
			//	- Event is fired, external consumer tells partition manager to initialize combine rooms
			//	- We need to determine that the partition should be closed even though one control is still open
			return CombinesRoom(partition);
		}

		/// <summary>
		/// Creates a new room instance to contain the given partitions.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partitions"></param>
		/// <param name="constructor"></param>
		public override void CombineRooms<TRoom>(IEnumerable<IPartition> partitions, Func<TRoom> constructor)
		{
			if (partitions == null)
				throw new ArgumentNullException("partitions");

			if (constructor == null)
				throw new ArgumentNullException("constructor");

			CombineRooms(partitions, constructor, true);
		}

		/// <summary>
		/// Creates a new room instance to contain the given partition controls.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="controls"></param>
		/// <param name="constructor"></param>
		public override void CombineRooms<TRoom>(IEnumerable<IPartitionDeviceControl> controls, Func<TRoom> constructor)
		{
			if (controls == null)
				throw new ArgumentNullException("controls");

			if (constructor == null)
				throw new ArgumentNullException("constructor");

			IEnumerable<IPartition> partitions = Partitions.GetPartitions(controls);
			CombineRooms(partitions, constructor);
		}

		/// <summary>
		/// Creates a new room instance to contain the given partition.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partition"></param>
		/// <param name="constructor"></param>
		public override void CombineRooms<TRoom>(IPartition partition, Func<TRoom> constructor)
		{
			if (partition == null)
				throw new ArgumentNullException("partition");

			if (constructor == null)
				throw new ArgumentNullException("constructor");

			CombineRooms(new[] {partition}, constructor);
		}

		/// <summary>
		/// Creates a new room instance to contain the partitions tied to the control.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partitionControl"></param>
		/// <param name="constructor"></param>
		public override void CombineRooms<TRoom>(IPartitionDeviceControl partitionControl, Func<TRoom> constructor)
		{
			if (partitionControl == null)
				throw new ArgumentNullException("partitionControl");

			if (constructor == null)
				throw new ArgumentNullException("constructor");

			IEnumerable<IPartition> partitions = Partitions.GetPartitions(partitionControl);
			CombineRooms(partitions, constructor);
		}

		/// <summary>
		/// Removes the partitions from existing rooms.
		/// </summary>
		/// <param name="partitions"></param>
		/// <param name="constructor"></param>
		public override void UncombineRooms<TRoom>(IEnumerable<IPartition> partitions, Func<TRoom> constructor)
		{
			if (partitions == null)
				throw new ArgumentNullException("partitions");

			if (constructor == null)
				throw new ArgumentNullException("constructor");

			UncombineRooms(partitions, constructor, true);
		}

		/// <summary>
		/// Removes the partition from existing rooms.
		/// </summary>
		/// <param name="partition"></param>
		/// <param name="constructor"></param>
		public override void UncombineRooms<TRoom>(IPartition partition, Func<TRoom> constructor)
		{
			if (partition == null)
				throw new ArgumentNullException("partition");

			if (constructor == null)
				throw new ArgumentNullException("constructor");

			UncombineRooms(new[] {partition}, constructor);
		}

		/// <summary>
		/// Removes the partitions tied to the given control from existing rooms.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partitionControl"></param>
		/// <param name="constructor"></param>
		public override void UncombineRooms<TRoom>(IPartitionDeviceControl partitionControl, Func<TRoom> constructor)
		{
			if (partitionControl == null)
				throw new ArgumentNullException("partitionControl");

			if (constructor == null)
				throw new ArgumentNullException("constructor");

			IEnumerable<IPartition> partitions = Partitions.GetPartitions(partitionControl);
			UncombineRooms(partitions, constructor);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Creates a new room instance to contain the given partitions.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partitions"></param>
		/// <param name="constructor"></param>
		/// <param name="update"></param>
		private void CombineRooms<TRoom>(IEnumerable<IPartition> partitions, Func<TRoom> constructor, bool update)
			where TRoom : IRoom
		{
			if (partitions == null)
				throw new ArgumentNullException("partitions");

			if (constructor == null)
				throw new ArgumentNullException("constructor");

			// Take the partitions that are not already combining rooms
			IcdHashSet<IPartition> partitionsSet = partitions.Where(p => !CombinesRoom(p)).ToIcdHashSet();
			if (partitionsSet.Count == 0)
				return;

			// Add the partitions from any adjacent combine spaces
			foreach (IPartition partition in partitionsSet.ToArray())
			{
				foreach (IRoom room in GetAdjacentCombineRooms(partition))
					partitionsSet.AddRange(room.Originators.GetInstancesRecursive<IPartition>());
			}

			// Clear out any existing combine spaces
			UncombineRooms(partitionsSet, constructor, false);

			// Build a sequence of continguous partitions
			IEnumerable<IEnumerable<IPartition>> groups = GetContiguous(partitionsSet);
			CreateCombineRooms(groups, constructor);

			if (!update)
				return;

			// Update the partitions and the rooms
			// TODO - Extremely lazy, should only update anything we touched
			UpdateRoomCombineState();
			UpdatePartitions(Partitions);
		}

		/// <summary>
		/// Removes the partitions from existing rooms.
		/// </summary>
		/// <param name="partitions"></param>
		/// <param name="constructor"></param>
		/// <param name="update"></param>
		private void UncombineRooms<TRoom>(IEnumerable<IPartition> partitions, Func<TRoom> constructor, bool update)
			where TRoom : IRoom
		{
			if (partitions == null)
				throw new ArgumentNullException("partitions");

			if (constructor == null)
				throw new ArgumentNullException("constructor");

			// We only need to close partitions that are currently combining rooms
			IcdHashSet<IPartition> closePartitions = partitions.Where(CombinesRoom).ToIcdHashSet();
			if (closePartitions.Count == 0)
				return;

			// Get the partitions that need to be re-opened after the close
			IcdHashSet<IPartition> openPartitions =
				GetCombineRooms(closePartitions).SelectMany(r => r.Originators
				                                                  .GetInstances<IPartition>())
				                                .Except(closePartitions)
				                                .ToIcdHashSet();

			// Destroy the combine rooms
			IEnumerable<IRoom> combineRooms = GetCombineRooms(closePartitions);
			DestroyCombineRooms(combineRooms);

			// Rebuild combine rooms around the open partitions
			if (openPartitions.Count > 0)
				CombineRooms(openPartitions, constructor, false);

			if (!update)
				return;

			// Update the partitions and the rooms
			// TODO - Extremely lazy, should only update anything we touched
			UpdateRoomCombineState();
			UpdatePartitions(Partitions);
		}

		/// <summary>
		/// Removes the rooms from the core and disposes them.
		/// </summary>
		/// <param name="rooms"></param>
		private void DestroyCombineRooms(IEnumerable<IRoom> rooms)
		{
			if (rooms == null)
				throw new ArgumentNullException("rooms");

			foreach (IRoom room in rooms)
				DestroyCombineRoom(room);
		}

		/// <summary>
		/// Removes the room from the core and disposes it.
		/// </summary>
		/// <param name="room"></param>
		private void DestroyCombineRoom(IRoom room)
		{
			if (room == null)
				throw new ArgumentNullException("room");

			Log(eSeverity.Informational, "Destroying combined room {0}", room);

			// Remove the partitions from the room.
			IPartition[] partitions = room.Originators.GetInstances<IPartition>().ToArray();
			foreach (IPartition partition in partitions)
				room.Originators.Remove(partition.Id);

			// Remove the room from the core
			Core.Originators.RemoveChild(room);
			IDisposable disposable = room as IDisposable;
			if (disposable != null)
				disposable.Dispose();
		}

		private void CreateCombineRooms<TRoom>(IEnumerable<IEnumerable<IPartition>> groups, Func<TRoom> constructor)
			where TRoom : IRoom
		{
			if (groups == null)
				throw new ArgumentNullException("groups");

			foreach (IEnumerable<IPartition> group in groups)
				CreateCombineRoom(group, constructor);
		}

		/// <summary>
		/// Creates a new room of the given type containing the given partitions.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="partitions"></param>
		/// <param name="constructor"></param>
		private void CreateCombineRoom<TRoom>(IEnumerable<IPartition> partitions, Func<TRoom> constructor)
			where TRoom : IRoom
		{
			if (partitions == null)
				throw new ArgumentNullException("partitions");

			if (constructor == null)
				throw new ArgumentNullException("constructor");

			IcdHashSet<IPartition> partitionsSet = partitions.ToIcdHashSet();
			if (partitionsSet.Count == 0)
				return;

			// Build the room.
			TRoom room = constructor();
			room.Id = IdUtils.GetNewRoomId(Core.Originators.GetChildren<IRoom>().Select(r => r.Id));
			room.Originators.AddRange(partitionsSet.Select(p => new KeyValuePair<int, eCombineMode>(p.Id, eCombineMode.Always)));

			// Add to the core
			Core.Originators.AddChild(room);

			Log(eSeverity.Informational, "Created new combine room {0}", room);
		}

		/// <summary>
		/// Loops through the rooms to find combine rooms that are adjacent to the given partition.
		/// </summary>
		/// <param name="partition"></param>
		/// <returns></returns>
		private static IEnumerable<IRoom> GetAdjacentCombineRooms(IPartition partition)
		{
			if (partition == null)
				throw new ArgumentNullException("partition");

			return GetRooms().Where(room => room.IsCombineRoom() && IsCombineRoomAdjacent(room, partition));
		}

		/// <summary>
		/// Returns true if the given combined space is adjacent to the given partition.
		/// Returns false if the given combined space contains the given partition.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="partition"></param>
		/// <returns></returns>
		private static bool IsCombineRoomAdjacent(IRoom room, IPartition partition)
		{
			if (room == null)
				throw new ArgumentNullException("room");

			if (partition == null)
				throw new ArgumentNullException("partition");

			// Only interested in the edge of the combined space
			if (room.Originators.ContainsRecursive(partition.Id))
				return false;

			// Returns true if any of the rooms in the combined space overlap with the partition rooms.
			return room.Originators
			           .GetInstancesRecursive<IPartition>()
			           .Any(p => partition.GetRooms()
			                              .Any(p.ContainsRoom));
		}

		/// <summary>
		/// Gets the rooms available to the core.
		/// </summary>
		/// <returns></returns>
		private static IEnumerable<IRoom> GetRooms()
		{
			return Core.Originators.GetChildren<IRoom>();
		}

		/// <summary>
		/// Returns a sequence of contiguous partition groups.
		/// </summary>
		/// <param name="partitions"></param>
		/// <returns></returns>
		private IEnumerable<IEnumerable<IPartition>> GetContiguous(IEnumerable<IPartition> partitions)
		{
			if (partitions == null)
				throw new ArgumentNullException("partitions");

			return RecursionUtils.GetCliques(partitions, p => Partitions.GetAdjacentPartitions(p));
		}

		/// <summary>
		/// Sets the open/close state for the given partitions based on the combined rooms.
		/// </summary>
		/// <param name="partitions"></param>
		private void UpdatePartitions(IEnumerable<IPartition> partitions)
		{
			if (partitions == null)
				throw new ArgumentNullException("partitions");

			foreach (IPartition partition in partitions)
			{
				bool combinesRoom = CombinesRoom(partition);

				if (combinesRoom)
					OpenPartition(partition);
				else
					ClosePartition(partition);
			}
		}

		/// <summary>
		/// Sets the combined state for all of the rooms based on the partitions.
		/// </summary>
		private static void UpdateRoomCombineState()
		{
			IRoom[] rooms = GetRooms().ToArray();

			// Build a set of all of the rooms that are children to combine spaces
			IcdHashSet<IRoom> roomsInCombineState =
				rooms.SelectMany(r => r.GetMasterAndSlaveRooms())
				     .ToIcdHashSet();

			// Update each room's combine state
			foreach (IRoom room in rooms)
			{
				bool isCombineState = roomsInCombineState.Contains(room);
				room.EnterCombineState(isCombineState);
			}
		}

		/// <summary>
		/// Gets the control for the partition and sets it closed.
		/// </summary>
		/// <param name="partition"></param>
		private void ClosePartition(IPartition partition)
		{
			if (partition == null)
				throw new ArgumentNullException("partition");

			foreach (IPartitionDeviceControl control in GetControls(partition).Where(c => c.IsOpen))
				control.Close();
		}

		/// <summary>
		/// Gets the control for the partition and sets it open.
		/// </summary>
		/// <param name="partition"></param>
		private void OpenPartition(IPartition partition)
		{
			if (partition == null)
				throw new ArgumentNullException("partition");

			foreach (IPartitionDeviceControl control in GetControls(partition).Where(c => !c.IsOpen))
				control.Open();
		}

		#endregion

		#region Partition Callbacks

		/// <summary>
		/// Called when the partitions collection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void PartitionsOnChildrenChanged(object sender, EventArgs eventArgs)
		{
			SubscribePartitions();
		}

		/// <summary>
		/// Subscribes to the partition controls.
		/// </summary>
		private void SubscribePartitions()
		{
			UnsubscribePartitions();

			m_SubscribedPartitions.AddRange(Partitions.SelectMany(p => GetControls(p)));

			foreach (IPartitionDeviceControl partition in m_SubscribedPartitions)
				Subscribe(partition);
		}

		/// <summary>
		/// Unsubscribes from the previously subscribed partitions.
		/// </summary>
		private void UnsubscribePartitions()
		{
			foreach (IPartitionDeviceControl partition in m_SubscribedPartitions)
				Unsubscribe(partition);
			m_SubscribedPartitions.Clear();
		}

		/// <summary>
		/// Subscribe to the partition events.
		/// </summary>
		/// <param name="partition"></param>
		private void Subscribe(IPartitionDeviceControl partition)
		{
			if (partition == null)
				return;

			partition.OnOpenStatusChanged += PartitionOnOpenStatusChanged;
		}

		/// <summary>
		/// Unsubscribe from the partition events.
		/// </summary>
		/// <param name="partition"></param>
		private void Unsubscribe(IPartitionDeviceControl partition)
		{
			if (partition == null)
				return;

			partition.OnOpenStatusChanged -= PartitionOnOpenStatusChanged;
		}

		/// <summary>
		/// Called when a partitions open state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void PartitionOnOpenStatusChanged(object sender, BoolEventArgs args)
		{
			PartitionControlOpenStateCallback handler = OnPartitionOpenStateChange;
			if (handler != null)
				handler(sender as IPartitionDeviceControl, args.Data);
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			UnsubscribePartitions();

			Partitions.Clear();
			RoomLayout.Clear();
		}

		protected override void CopySettingsFinal(PartitionManagerSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.PartitionSettings.SetRange(Partitions.Where(c => c.Serialize).Select(r => r.CopySettings()));
			settings.RoomLayoutSettings.SetRooms(RoomLayout.GetRooms());
		}

		protected override void ApplySettingsFinal(PartitionManagerSettings settings, IDeviceFactory factory)
		{
			m_Partitions.OnChildrenChanged -= PartitionsOnChildrenChanged;

			base.ApplySettingsFinal(settings, factory);

			IEnumerable<IPartition> partitions = GetPartitions(settings, factory);
			Partitions.SetChildren(partitions);

			RoomLayout.SetRooms(settings.RoomLayoutSettings.GetRooms());

			SubscribePartitions();

			m_Partitions.OnChildrenChanged += PartitionsOnChildrenChanged;
		}

		private IEnumerable<IPartition> GetPartitions(PartitionManagerSettings settings, IDeviceFactory factory)
		{
			return GetOriginatorsSkipExceptions<IPartition>(settings.PartitionSettings, factory);
		}

		private IEnumerable<T> GetOriginatorsSkipExceptions<T>(IEnumerable<ISettings> originatorSettings,
		                                                       IDeviceFactory factory)
			where T : class, IOriginator
		{
			foreach (ISettings settings in originatorSettings)
			{
				T output;

				try
				{
					output = factory.GetOriginatorById<T>(settings.Id);
				}
				catch (Exception e)
				{
					Logger.AddEntry(eSeverity.Error, e, "Failed to instantiate {0} with id {1}", typeof(T).Name, settings.Id);
					continue;
				}

				yield return output;
			}
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

			yield return new ConsoleCommand("PrintRooms", "Prints the list of rooms and their children.", () => PrintRooms());
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		private string PrintRooms()
		{
			TableBuilder builder = new TableBuilder("Id", "Room", "Children", "Combine Pritority", "Combine State");

			foreach (IRoom room in GetRooms().OrderBy(r => r.Id))
			{
				int id = room.Id;
				string children = StringUtils.ArrayFormat(room.GetRooms().Select(r => r.Id).Order());

				builder.AddRow(id, room, children, room.CombinePriority, room.CombineState);
			}

			return builder.ToString();
		}

		#endregion
	}
}
