using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.EventArguments;
using ICD.Common.Properties;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Partitioning.Rooms
{
	/// <summary>
	/// Represents a room of devices.
	/// </summary>
	public interface IRoom : IOriginator
	{
		/// <summary>
		/// Raised when the room combine state changes.
		/// </summary>
		event EventHandler<BoolEventArgs> OnCombineStateChanged;
		
		/// <summary>
		/// Gets the parent core instance.
		/// </summary>
		ICore Core { get; }

		/// <summary>
		/// Returns true if the room is currently behaving as part of a combined room.
		/// </summary>
		bool CombineState { get; }

		RoomDeviceIdCollection Devices { get; }
		RoomPortIdCollection Ports { get; }
		RoomPanelIdCollection Panels { get; }
		RoomSourceIdCollection Sources { get; }
		RoomDestinationIdCollection Destinations { get; }
		RoomDestinationGroupIdCollection DestinationGroups { get; }
		RoomPartitionIdCollection Partitions { get; }

		/// <summary>
		/// Informs the room it is part of a combined room.
		/// </summary>
		void EnterCombineState();

		/// <summary>
		/// Informs the room it is no longer part of a combined room.
		/// </summary>
		void LeaveCombineState();
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
		public static bool ContainsControl(this IRoom extends, DeviceControlInfo controlInfo)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			// TODO - Janky, controls can be on IDeviceBase, IDeviceBase lives in Ports, Devices and Panels
			return extends.Ports.Contains(controlInfo.DeviceId) ||
			       extends.Devices.Contains(controlInfo.DeviceId) ||
			       extends.Panels.Contains(controlInfo.DeviceId);
		}

		/// <summary>
		/// Returns the first available control of the given type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[CanBeNull]
		[PublicAPI]
		public static T GetControl<T>(this IRoom extends)
			where T : IDeviceControl
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
		public static IDeviceControl GetControl(this IRoom extends, DeviceControlInfo controlInfo)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (extends.ContainsControl(controlInfo))
				return extends.Core.GetControl(controlInfo);

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
		public static T GetControl<T>(this IRoom extends, DeviceControlInfo controlInfo)
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
		public static IEnumerable<T> GetControls<T>(this IRoom extends)
			where T : IDeviceControl
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			// TODO - Janky, controls can be on IDeviceBase, IDeviceBase lives in Ports, Devices and Panels
			IEnumerable<IDeviceBase> devices = extends.Devices.GetInstances().Cast<IDeviceBase>();
			IEnumerable<IDeviceBase> ports = extends.Ports.GetInstances().Cast<IDeviceBase>();
			IEnumerable<IDeviceBase> panels = extends.Panels.GetInstances().Cast<IDeviceBase>();

			return devices.Concat(ports)
			              .Concat(panels)
			              .SelectMany(d => d.Controls.GetControls<T>());
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
		public static T GetControlRecursive<T>(this IRoom extends)
			where T : IDeviceControl
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
		public static IDeviceControl GetControlRecursive(this IRoom extends, DeviceControlInfo controlInfo)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			foreach (IRoom room in extends.GetRoomsRecursive().Where(room => room.ContainsControl(controlInfo)))
				return room.GetControl(controlInfo);

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
		public static T GetControlRecursive<T>(this IRoom extends, DeviceControlInfo controlInfo)
			where T : IDeviceControl
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			IDeviceControl control = extends.GetControlRecursive(controlInfo);
			if (control is T)
				return (T)control;

			throw new InvalidOperationException(string.Format("{0} is not of type {1}", control, typeof(T).Name));
		}

		/// <summary>
		/// Returns the controls of the given type recursively, as defined by partitions.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IEnumerable<T> GetControlsRecursive<T>(this IRoom extends)
			where T : IDeviceControl
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
		/// Gets the child rooms as defined by the partitions.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		[PublicAPI]
		public static IEnumerable<IRoom> GetRooms(this IRoom extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.Partitions
			              .GetInstances()
			              .SelectMany(p => p.GetRooms())
						  .Distinct()
			              .Select(i => extends.Core.Originators.GetChild<IRoom>(i));
		}

		/// <summary>
		/// Returns this room, and all child rooms as defined by the partitions.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		[PublicAPI]
		public static IEnumerable<IRoom> GetRoomsRecursive(this IRoom extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetRoomsRecursive(new IcdHashSet<IRoom>());
		}

		/// <summary>
		/// Returns this room, and all child rooms as defined by the partitions.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="visited"></param>
		/// <returns></returns>
		[PublicAPI]
		private static IEnumerable<IRoom> GetRoomsRecursive(this IRoom extends, IcdHashSet<IRoom> visited)
		{
			if (!visited.Add(extends))
				yield break;

			yield return extends;

			foreach (IRoom child in extends.GetRooms().SelectMany(r => r.GetRoomsRecursive(visited)))
				yield return child;
		}

		#endregion

		/// <summary>
		/// Returns true if the room has a destination with the given connection type
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool HasDestinationOfType(this IRoom extends, eConnectionType type)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.Destinations
			              .GetInstancesRecursive()
			              .Any(d => d.ConnectionType.HasFlags(type));
		}
	}
}
