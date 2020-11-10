using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Audio.VolumePoints;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Partitioning.Extensions;
using ICD.Connect.Partitioning.PartitionManagers;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Settings.Originators;
using ICD.Connect.Telemetry.Attributes;

namespace ICD.Connect.Partitioning.Rooms
{
	/// <summary>
	/// Represents a room of devices.
	/// </summary>
	[ExternalTelemetry("Room Telemetry", typeof(RoomExternalTelemetryProvider))]
	public interface IRoom : IOriginator
	{
		/// <summary>
		/// Raised when the room combine state changes.
		/// </summary>
		event EventHandler<BoolEventArgs> OnCombineStateChanged;

		/// <summary>
		/// Raised when the current volume context changes.
		/// </summary>
		event EventHandler<GenericEventArgs<eVolumePointContext>> OnVolumeContextChanged;

		#region Properties

		/// <summary>
		/// Returns true if the room is currently behaving as part of a combined room.
		/// </summary>
		bool CombineState { get; }

		/// <summary>
		/// Returns the priority order for combining rooms. Lower is better.
		/// </summary>
		int CombinePriority { get; }

		/// <summary>
		/// Gets the originators that are contained within this room.
		/// </summary>
		[NotNull]
		RoomOriginatorIdCollection Originators { get; }

		/// <summary>
		/// Gets the current volume context.
		/// </summary>
		eVolumePointContext VolumeContext { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Informs the room it is part of a combined room.
		/// </summary>
		/// <param name="combine"></param>
		void EnterCombineState(bool combine);

		/// <summary>
		/// Called before this combine space is destroyed as part of an uncombine operation.
		/// </summary>
		void HandlePreUncombine();

		#endregion
	}

	/// <summary>
	/// Extensions methods for IRooms.
	/// </summary>
	public static class RoomExtensions
	{
		#region Controls

		/// <summary>
		/// Returns the first available control of the given type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[CanBeNull]
		public static T GetControl<T>([NotNull] this IRoom extends)
			where T : class, IDeviceControl
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetControls<T>().FirstOrDefault();
		}

		/// <summary>
		/// Returns the controls of the given type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IEnumerable<T> GetControls<T>([NotNull] this IRoom extends)
			where T : class, IDeviceControl
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.Originators.GetInstances<IDevice>().SelectMany(o => o.Controls.GetControls<T>());
		}

		/// <summary>
		/// Returns the controls of the given type recursively, as defined by partitions.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IEnumerable<T> GetControlsRecursive<T>([NotNull] this IRoom extends)
			where T : class, IDeviceControl
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetRoomsRecursive()
			              .SelectMany(r => r.GetControls<T>())
			              .Distinct();
		}

		#endregion

		#region Rooms

		/// <summary>
		/// Returns true if this room contains the given room.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static bool ContainsRoom([NotNull] this IRoom extends, IRoom other)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (other == null)
				throw new ArgumentNullException("other");

			return extends.Originators
			              .GetInstancesRecursive<IPartition>()
			              .Any(p => p.ContainsRoom(other.Id));
		}

		/// <summary>
		/// Returns true if the room is made up of child rooms.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		public static bool IsCombineRoom([NotNull] this IRoom extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.Originators.HasInstances<IPartition>();
		}

		/// <summary>
		/// returns true if the room is a child to a combine space and is the master room.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		public static bool IsMasterRoom([NotNull] this IRoom extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			IPartitionManager manager;
			if (!extends.Core.TryGetPartitionManager(out manager))
				return false;

			IRoom parent = manager.GetCombineRoom(extends);
			if (parent == null)
				return false;

			return extends == parent.GetMasterRoom();
		}

		/// <summary>
		/// Returns the child room with the lowest combine priority.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		[CanBeNull]
		public static IRoom GetMasterRoom([NotNull] this IRoom extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetMasterAndSlaveRooms().FirstOrDefault();
		}

		/// <summary>
		/// Returns all distinct child rooms recursively, prepended by the master room.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		public static IEnumerable<IRoom> GetMasterAndSlaveRooms([NotNull] this IRoom extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetRoomsRecursive().Except(extends);
		}

		/// <summary>
		/// Gets the child rooms as defined by the partitions.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		public static IEnumerable<IRoom> GetRooms([NotNull] this IRoom extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (!extends.IsCombineRoom())
				return Enumerable.Empty<IRoom>();

			return extends.Originators
			              .GetInstances<IPartition>()
			              .SelectMany(p => p.GetRooms())
			              .Distinct()
						  .OrderBy(r => r.CombinePriority)
						  .ThenBy(r => r.Id);
		}

		/// <summary>
		/// Returns this room, and all child rooms recursively as defined by the partitions.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		public static IEnumerable<IRoom> GetRoomsRecursive([NotNull] this IRoom extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return RecursionUtils.BreadthFirstSearch(extends, r => r.GetRooms())
			                     .OrderBy(r => r.CombinePriority)
			                     .ThenBy(r => r.Id);
		}

		#endregion

		#region Volume

		/// <summary>
		/// Gets the ordered volume points for the current context.
		/// </summary>
		[NotNull]
		public static IEnumerable<IVolumePoint> GetContextualVolumePoints([NotNull] this IRoom extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			eVolumePointContext type = extends.VolumeContext;

			return extends.Originators
			              .GetInstancesRecursive<IVolumePoint>()
			              .Where(v => EnumUtils.HasAnyFlags(v.Context, type))
			              .OrderBy(v => v, new VolumeContextComparer(type));
		}

		#endregion

		#region CriicalDevices

		public static IEnumerable<IDevice> GetOfflineCriticalDevicesRecursive([NotNull] this IRoom extends)
		{
			return GetCriticalDevicesRecursive(extends).Where(device => !device.IsOnline);
		}

		[NotNull]
		public static IEnumerable<IDevice> GetCriticalDevicesRecursive([NotNull] this IRoom extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.Originators
				.GetInstancesRecursive<IDevice>()
				.Where(device => device.RoomCritical);
		}

		#endregion CriticalDevices

	}
}
