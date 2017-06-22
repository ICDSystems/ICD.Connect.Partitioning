using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Rooms
{
	/// <summary>
	/// Represents a room of devices.
	/// </summary>
	public interface IRoom : IOriginator
	{
		/// <summary>
		/// Gets the parent core instance.
		/// </summary>
		ICore Core { get; }

		RoomDeviceIdCollection Devices { get; }
		RoomPortIdCollection Ports { get; }
		RoomPanelIdCollection Panels { get; }
		RoomSourceIdCollection Sources { get; }
		RoomDestinationIdCollection Destinations { get; }
		RoomDestinationGroupIdCollection DestinationGroups { get; }
	}

	/// <summary>
	/// Extensions methods for IRooms.
	/// </summary>
	public static class RoomExtensions
	{
		/// <summary>
		/// Gets the first device of the given type.
		/// Returns null if no device of the given type.
		/// </summary>
		/// <typeparam name="TDevice"></typeparam>
		/// <param name="extends"></param>
		/// <returns></returns>
		[CanBeNull]
		[PublicAPI]
		public static TDevice GetDevice<TDevice>(this IRoom extends)
			where TDevice : IDevice
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetDevices<TDevice>().FirstOrDefault();
		}

		/// <summary>
		/// Gets the devices of the given type.
		/// </summary>
		/// <typeparam name="TDevice"></typeparam>
		/// <param name="extends"></param>
		/// <returns></returns>
		[PublicAPI]
		public static IEnumerable<TDevice> GetDevices<TDevice>(this IRoom extends)
			where TDevice : IDevice
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.Devices.OfType<TDevice>();
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
		public static IDeviceControl GetControl(this IRoom extends, DeviceControlInfo controlInfo)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.Devices[controlInfo.DeviceId].Controls[controlInfo.ControlId];
		}

		/// <summary>
		/// Gets the control matching the given control info.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="controlInfo"></param>
		/// <returns></returns>
		[PublicAPI]
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
		/// Returns the first available control of the given type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[PublicAPI]
		public static IEnumerable<T> GetControls<T>(this IRoom extends)
			where T : IDeviceControl
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.Devices.SelectMany(d => d.Controls.GetControls<T>());
		}

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

			return extends.Destinations.Any(d => d.ConnectionType.HasFlags(type));
		}
	}
}
