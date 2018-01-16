using ICD.Common.Properties;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Partitioning.Extensions
{
	public static class DeviceFactoryExtensions
	{
		/// <summary>
		/// Lazy-loads the Connection with the given id.
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		[PublicAPI]
		public static IRoom GetRoomById(this IDeviceFactory factory, int id)
		{
			return factory.GetOriginatorById<IRoom>(id);
		}
	}
}
