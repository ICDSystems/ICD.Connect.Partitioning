using System;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Rooms.Extensions
{
	public static class SourceDestinationBaseExtensions
	{
		/// <summary>
		/// Gets the name of the source. If no name specified, returns the name of the device
		/// with the specified id.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="room"></param>
		/// <returns></returns>
		public static string GetNameOrDeviceName(this ISourceDestinationBase extends, IRoom room)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");
			if (room == null)
				throw new ArgumentNullException("room");

			return string.IsNullOrEmpty(extends.Name)
					   ? room.Devices.GetInstanceRecursive(extends.Endpoint.Device).Name
					   : extends.Name;
		}
	}
}