using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Logging.LoggingContexts;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Devices.Extensions;
using ICD.Connect.Partitioning.Cells;
using ICD.Connect.Partitioning.Controls;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Originators;
using ICD.Connect.Settings.Utils;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public sealed class PartitionManager : AbstractPartitionManager<PartitionManagerSettings>
	{
		/// <summary>
		/// Raised when a partition control opens/closes.
		/// </summary>
		public override event PartitionControlOpenStateCallback OnPartitionControlOpenStateChange;

		private readonly CellsCollection m_Cells;
		private readonly PartitionsCollection m_Partitions;
		private readonly IcdHashSet<IPartitionDeviceControl> m_SubscribedPartitions;

		#region Properties

		/// <summary>
		/// Gets the cells in the system.
		/// </summary>
		public override ICellsCollection Cells { get { return m_Cells; } }

		/// <summary>
		/// Gets the partitions in the system.
		/// </summary>
		public override IPartitionsCollection Partitions { get { return m_Partitions; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public PartitionManager()
		{
			m_Cells = new CellsCollection(this);
			m_Partitions = new PartitionsCollection(this);
			
			m_SubscribedPartitions = new IcdHashSet<IPartitionDeviceControl>();

			ServiceProvider.AddService<IPartitionManager>(this);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnPartitionControlOpenStateChange = null;

			base.DisposeFinal(disposing);

			ServiceProvider.RemoveService<IPartitionManager>(this);
		}

		#region Controls

		/// <summary>
		/// Gets the controls for the given partition.
		/// </summary>
		/// <param name="partition"></param>
		/// <param name="mask"></param>
		/// <returns></returns>
		public override IEnumerable<IPartitionDeviceControl> GetControls(IPartition partition, ePartitionFeedback mask)
		{
			if (partition == null)
				throw new ArgumentNullException("partition");

			foreach (PartitionControlInfo info in partition.GetPartitionControlInfos(mask))
			{
				IPartitionDeviceControl control;

				try
				{
					control = Core.GetControl<IPartitionDeviceControl>(info.Control);
				}
				catch (Exception e)
				{
					Logger.Log(eSeverity.Error, "Unable to get partition control for {0} - {1}", info, e.Message);
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
			IEnumerable<IRoom> rooms = GetRooms().OrderByDescending(r => r.IsCombineRoom());
			IcdHashSet<IRoom> visited = new IcdHashSet<IRoom>();

			foreach (IRoom room in rooms.Where(room => !visited.Contains(room)))
			{
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

			CombineRooms(open, closed, constructor);
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

			IcdHashSet<IPartitionDeviceControl> controls = GetControls(partition, ePartitionFeedback.Get).ToIcdHashSet();
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
		/// <param name="open"></param>
		/// <param name="close"></param>
		/// <param name="constructor"></param>
		public override void CombineRooms<TRoom>(IEnumerable<IPartition> open, IEnumerable<IPartition> close, Func<TRoom> constructor)
		{
			if (open == null)
				throw new ArgumentNullException("open");

			if (close == null)
				throw new ArgumentNullException("close");

			if (constructor == null)
				throw new ArgumentNullException("constructor");

			CombineRooms(open, close, constructor, true);
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

			CombineRooms(partitions, Enumerable.Empty<IPartition>(), constructor);
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

			CombineRooms(partition.Yield(), constructor);
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

			CombineRooms(Enumerable.Empty<IPartition>(), partitions, constructor);
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

			UncombineRooms(partition.Yield(), constructor);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Combines/uncombines rooms to match the given set of partitions that are open/closed.
		/// </summary>
		/// <typeparam name="TRoom"></typeparam>
		/// <param name="openPartitions"></param>
		/// <param name="closedPartitions"></param>
		/// <param name="constructor"></param>
		/// <param name="update"></param>
		private void CombineRooms<TRoom>(IEnumerable<IPartition> openPartitions, IEnumerable<IPartition> closedPartitions,
										 Func<TRoom> constructor, bool update)
			where TRoom : IRoom
		{
			if (openPartitions == null)
				throw new ArgumentNullException("openPartitions");

			if (closedPartitions == null)
				throw new ArgumentNullException("closedPartitions");

			if (constructor == null)
				throw new ArgumentNullException("constructor");

			// Remove the partitions that are not changing
			IcdHashSet<IPartition> openPartitionsUpdateSet = openPartitions.Where(p => !CombinesRoom(p)).ToIcdHashSet();
			IcdHashSet<IPartition> closedPartitionsUpdateSet = closedPartitions.Where(CombinesRoom).ToIcdHashSet();

			// if no changes necessary, exit early
			if (!openPartitionsUpdateSet.Any() && !closedPartitionsUpdateSet.Any())
				return;

			// Add the partitions from any adjacent combine spaces that will be combined by an opening partition
			foreach (IPartition partition in openPartitionsUpdateSet.ToArray())
			{
				foreach (IRoom room in GetAdjacentCombineRooms(partition))
					openPartitionsUpdateSet.AddRange(room.Originators.GetInstancesRecursive<IPartition>().Except(closedPartitionsUpdateSet));
			}

			// Add the partitions from any existing combine rooms that have a partition closing
			foreach (IRoom room in GetCombineRooms(closedPartitionsUpdateSet))
				openPartitionsUpdateSet.AddRange(room.Originators.GetInstancesRecursive<IPartition>().Except(closedPartitionsUpdateSet));

			// Destroy all current combine rooms that exist with the accumulated sets of partitions
			IEnumerable<IRoom> combineRooms = GetCombineRooms(closedPartitionsUpdateSet.Concat(openPartitionsUpdateSet));
			DestroyCombineRooms(combineRooms);

			// Find the sequence of contiguous partitions in the open set and (re)build combine rooms
			IEnumerable<IcdHashSet<IPartition>> groups = GetContiguous(openPartitionsUpdateSet).Select(g => g.ToIcdHashSet()).ToArray();
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
			
			CombineRooms(Enumerable.Empty<IPartition>(), partitions, constructor, update);
		}

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

			CombineRooms(partitions, Enumerable.Empty<IPartition>(), constructor, update);
		}

		/// <summary>
		/// Removes the rooms from the core and disposes them.
		/// </summary>
		/// <param name="rooms"></param>
		private void DestroyCombineRooms(IEnumerable<IRoom> rooms)
		{
			if (rooms == null)
				throw new ArgumentNullException("rooms");

			IList<IRoom> roomsList = rooms as IList<IRoom> ?? rooms.ToArray();

			// Remove the partitions from the rooms.
			foreach (IRoom room in roomsList)
			{
				Logger.Log(eSeverity.Informational, "Destroying combined room {0}", room);

				room.HandlePreUncombine();

				int[] partitionIds = room.Originators.GetInstances<IPartition>().Select(p => p.Id).ToArray();
				room.Originators.RemoveRange(partitionIds);
			}

			// Remove the room from the core
			Core.Originators.RemoveChildren(roomsList);

			// Dispose the rooms
			foreach (IDisposable room in roomsList.OfType<IDisposable>())
				room.Dispose();
		}

		private void CreateCombineRooms<TRoom>(IEnumerable<IcdHashSet<IPartition>> groups, Func<TRoom> constructor)
			where TRoom : IRoom
		{
			if (groups == null)
				throw new ArgumentNullException("groups");

			if (constructor == null)
				throw new ArgumentNullException("constructor");

			List<IRoom> rooms = new List<IRoom>();

			foreach (IcdHashSet<IPartition> group in groups)
			{
				if (group.Count == 0)
					continue;

				// Build the room.
				TRoom room = constructor();
				room.Id = IdUtils.GetNewRoomId(Core.Originators.GetChildrenIds().Concat(rooms.Select(r => r.Id)));
				room.Uuid = GuidUtils.Combine(group.SelectMany(p => p.GetRooms().Select(r => r.Uuid).Order()));
				room.Originators.AddRange(group.Select(p => new KeyValuePair<int, eCombineMode>(p.Id, eCombineMode.Always)));

				Logger.Log(eSeverity.Informational, "Created new combine room {0}", room);

				rooms.Add(room);
			}

			// Add to the core
			Core.Originators.AddChildren(rooms);
		}

		/// <summary>
		/// Loops through the rooms to find combine rooms that are adjacent to the given partition.
		/// </summary>
		/// <param name="partition"></param>
		/// <returns></returns>
		private IEnumerable<IRoom> GetAdjacentCombineRooms(IPartition partition)
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
		private IEnumerable<IRoom> GetRooms()
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
		private void UpdateRoomCombineState()
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

			foreach (IPartitionDeviceControl control in GetControls(partition, ePartitionFeedback.Set))
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

			foreach (IPartitionDeviceControl control in GetControls(partition, ePartitionFeedback.Set))
				control.Open();
		}

		#endregion

		#region Partition Callbacks

		/// <summary>
		/// Called when the partitions collection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void PartitionsOnCollectionChanged(object sender, EventArgs eventArgs)
		{
			SubscribePartitions();
		}

		/// <summary>
		/// Subscribes to the partition controls.
		/// </summary>
		private void SubscribePartitions()
		{
			UnsubscribePartitions();

			m_SubscribedPartitions.AddRange(Partitions.SelectMany(p => GetControls(p, ePartitionFeedback.Get)));

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
		/// Subscribe to the partition control events.
		/// </summary>
		/// <param name="partitionControl"></param>
		private void Subscribe(IPartitionDeviceControl partitionControl)
		{
			if (partitionControl == null)
				return;

			partitionControl.OnOpenStatusChanged += PartitionControlOnOpenStatusChanged;
		}

		/// <summary>
		/// Unsubscribe from the partition control events.
		/// </summary>
		/// <param name="partition"></param>
		private void Unsubscribe(IPartitionDeviceControl partition)
		{
			if (partition == null)
				return;

			partition.OnOpenStatusChanged -= PartitionControlOnOpenStatusChanged;
		}

		/// <summary>
		/// Called when a partition control open state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void PartitionControlOnOpenStatusChanged(object sender, BoolEventArgs args)
		{
			PartitionControlOpenStateCallback handler = OnPartitionControlOpenStateChange;
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
			Cells.Clear();
		}

		protected override void CopySettingsFinal(PartitionManagerSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.CellSettings.SetRange(Cells.Where(c => c.Serialize).Select(r => r.CopySettings()));
			settings.PartitionSettings.SetRange(Partitions.Where(c => c.Serialize).Select(r => r.CopySettings()));
		}

		protected override void ApplySettingsFinal(PartitionManagerSettings settings, IDeviceFactory factory)
		{
			m_Partitions.OnCollectionChanged -= PartitionsOnCollectionChanged;

			base.ApplySettingsFinal(settings, factory);

			IEnumerable<IPartition> partitions = GetPartitions(settings, factory);
			m_Partitions.SetChildren(partitions);

			IEnumerable<ICell> cells = GetCells(settings, factory);
			m_Cells.SetChildren(cells);
			m_Cells.RebuildCache();

			SubscribePartitions();

			m_Partitions.OnCollectionChanged += PartitionsOnCollectionChanged;
		}

		private IEnumerable<ICell> GetCells(PartitionManagerSettings settings, IDeviceFactory factory)
		{
			return GetOriginatorsSkipExceptions<ICell>(settings.CellSettings, factory);
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
					Logger.Log(eSeverity.Error, e, "Failed to instantiate {0} with id {1}", typeof(T).Name, settings.Id);
					continue;
				}

				yield return output;
			}
		}

		#endregion
	}
}
