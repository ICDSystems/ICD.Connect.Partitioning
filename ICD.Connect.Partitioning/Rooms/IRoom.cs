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
using ICD.Connect.Devices.Extensions;
using ICD.Connect.Partitioning.Extensions;
using ICD.Connect.Partitioning.PartitionManagers;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Settings.Cores;
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

		#region Properties

		/// <summary>
		/// Gets the parent core instance.
		/// </summary>
		[NotNull]
		ICore Core { get; }

		/// <summary>
		/// Returns true if the room is currently behaving as part of a combined room.
		/// </summary>
		bool CombineState { get; }

		/// <summary>
		/// Returns the priority order for combining rooms. Lower is better.
		/// </summary>
		int CombinePriority { get; set; }

		/// <summary>
		/// Gets the originators that are contained within this room.
		/// </summary>
		[NotNull]
		RoomOriginatorIdCollection Originators { get; }

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

		/// <summary>
		/// Gets the current volume context.
		/// </summary>
		eVolumePointContext GetVolumeContext();

		#endregion
	}

	/// <summary>
	/// Extensions methods for IRooms.
	/// </summary>
	public static class RoomExtensions
	{
		#region Controls

		/// <summary>
		/// Returns true if the room contains the given control.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="controlInfo"></param>
		/// <returns></returns>
		public static bool ContainsControl([NotNull] this IRoom extends, DeviceControlInfo controlInfo)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.Originators.Contains(controlInfo.DeviceId);
		}

		/// <summary>
		/// Returns the first available control of the given type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[CanBeNull]
		[PublicAPI]
		public static T GetControl<T>([NotNull] this IRoom extends)
			where T : class, IDeviceControl
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetControls<T>().FirstOrDefault();
		}

		/// <summary>
		/// Gets the control matching the given control info.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="controlInfo"></param>
		/// <returns></returns>
		[PublicAPI]
		[NotNull]
		public static IDeviceControl GetControl([NotNull] this IRoom extends, DeviceControlInfo controlInfo)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			IDeviceControl output;
			if (extends.TryGetControl(controlInfo, out output))
				return output;

			string message = string.Format("{0} does not contain an {1} with id {2}", extends, typeof(IDeviceBase),
			                               controlInfo.DeviceId);
			throw new KeyNotFoundException(message);
		}

		/// <summary>
		/// Gets the control matching the given control info.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="controlInfo"></param>
		/// <returns></returns>
		[PublicAPI]
		[NotNull]
		public static T GetControl<T>([NotNull] this IRoom extends, DeviceControlInfo controlInfo)
			where T : IDeviceControl
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			IDeviceControl control = extends.GetControl(controlInfo);
			if (control is T)
				return (T)control;

			throw new InvalidOperationException(string.Format("{0} is not of type {1}", control, typeof(T).Name));
		}

		/// <summary>
		/// Returns the controls of the given type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[PublicAPI]
		public static IEnumerable<T> GetControls<T>([NotNull] this IRoom extends)
			where T : class, IDeviceControl
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.Originators.GetInstances<IDevice>().SelectMany(o => o.Controls.GetControls<T>());
		}

		/// <summary>
		/// Gets the control matching the given control info.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="controlInfo"></param>
		/// <param name="control"></param>
		/// <returns></returns>
		[PublicAPI]
		public static bool TryGetControl([NotNull] this IRoom extends, DeviceControlInfo controlInfo, out IDeviceControl control)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			control = null;
			if (!extends.ContainsControl(controlInfo))
				return false;

			control = extends.Core.GetControl(controlInfo);
			return true;
		}

		/// <summary>
		/// Gets the control matching the given control info.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="controlInfo"></param>
		/// <param name="control"></param>
		/// <returns></returns>
		[PublicAPI]
		public static bool TryGetControl<T>([NotNull] this IRoom extends, DeviceControlInfo controlInfo, out T control)
			where T : class, IDeviceControl
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			control = null;

			IDeviceControl output;
			bool found = extends.TryGetControl(controlInfo, out output);
			if (!found)
				return false;

			control = output as T;
			if (control == null)
				throw new InvalidOperationException(string.Format("{0} is not of type {1}", output, typeof(T).Name));

			return true;
		}

		#endregion

		#region Control Recursion

		/// <summary>
		/// Returns the first available control of the given type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[CanBeNull]
		[PublicAPI]
		public static T GetControlRecursive<T>([NotNull] this IRoom extends)
			where T : class, IDeviceControl
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetControlsRecursive<T>().FirstOrDefault();
		}

		/// <summary>
		/// Returns the control matching the given type and control info recursively, as defined by partitions.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		[NotNull]
		public static IDeviceControl GetControlRecursive([NotNull] this IRoom extends, DeviceControlInfo controlInfo)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			IDeviceControl output;
			if (extends.TryGetControlRecursive(controlInfo, out output))
				return output;

			string message = string.Format("{0} does not recursively contain an {1} with id {2}", extends, typeof(IDeviceBase),
			                               controlInfo.DeviceId);
			throw new KeyNotFoundException(message);
		}

		/// <summary>
		/// Returns the control matching the given type and control info recursively, as defined by partitions.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[PublicAPI]
		[NotNull]
		public static T GetControlRecursive<T>([NotNull] this IRoom extends, DeviceControlInfo controlInfo)
			where T : class, IDeviceControl
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			IDeviceControl control = extends.GetControlRecursive(controlInfo);
			T cast = control as T;
			if (cast != null)
				return cast;

			throw new InvalidOperationException(string.Format("{0} is not of type {1}", control, typeof(T).Name));
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

		/// <summary>
		/// Returns the control matching the given type and control info recursively, as defined by partitions.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public static bool TryGetControlRecursive([NotNull] this IRoom extends, DeviceControlInfo controlInfo,
		                                          out IDeviceControl control)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetRoomsRecursive()
						  .Where(room => room.ContainsControl(controlInfo))
						  .Select(r => r.GetControl(controlInfo))
						  .TryFirst(out control);
		}

		/// <summary>
		/// Returns the control matching the given type and control info recursively, as defined by partitions.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[PublicAPI]
		public static bool TryGetControlRecursive<T>([NotNull] this IRoom extends, DeviceControlInfo controlInfo, out T control)
			where T : IDeviceControl
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			control = default(T);

			IDeviceControl output;
			bool found = extends.TryGetControlRecursive(controlInfo, out output);
			if (!found)
				return false;

			if (!(output is T))
				throw new InvalidOperationException(string.Format("{0} is not of type {1}", output, typeof(T).Name));

			control = (T)output;
			return true;
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
		/// Returns all child rooms except the master room.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		public static IEnumerable<IRoom> GetSlaveRooms([NotNull] this IRoom extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetMasterAndSlaveRooms().Skip(1);
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

			return extends.GetRoomsRecursive()
			              .Except(extends)
			              .Distinct()
			              .OrderBy(r => r.CombinePriority)
			              .ThenBy(r => r.Id);
		}

		/// <summary>
		/// Gets the child rooms as defined by the partitions.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		[PublicAPI]
		public static IEnumerable<IRoom> GetRooms([NotNull] this IRoom extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (!extends.IsCombineRoom())
				return Enumerable.Empty<IRoom>();

			IEnumerable<int> ids = extends.Originators
			                              .GetInstances<IPartition>()
			                              .SelectMany(p => p.GetRooms())
			                              .Distinct();

			return extends.Core.Originators.GetChildren<IRoom>(ids);
		}

		/// <summary>
		/// Returns this room, and all child rooms recursively as defined by the partitions.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		[PublicAPI]
		public static IEnumerable<IRoom> GetRoomsRecursive([NotNull] this IRoom extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return RecursionUtils.BreadthFirstSearch(extends, r => r.GetRooms());
		}

		#endregion

		#region Destinations

		/// <summary>
		/// Returns true if the room has a destination with the given connection type.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool HasDestinationOfType([NotNull] this IRoom extends, eConnectionType type)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.Originators
			              .GetInstancesRecursive<IDestination>()
			              .Any(d => d.ConnectionType.HasFlags(type));
		}

		#endregion
	}
}
