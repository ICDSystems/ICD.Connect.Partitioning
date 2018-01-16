using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Partitioning.PartitionManagers;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Partitioning.Extensions
{
	public static class CoreExtensions
	{
		/// <summary>
		/// Gets the partition manager instance from the core.
		/// </summary>
		/// <param name="core"></param>
		/// <returns></returns>
		[NotNull]
		public static IPartitionManager GetPartitionManager(this ICore core)
		{
			if (core == null)
				throw new ArgumentNullException("core");

			return core.Originators.GetChild<IPartitionManager>();
		}

		/// <summary>
		/// Gets the partition manager instance from the core.
		/// </summary>
		/// <param name="core"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		public static bool TryGetPartitionManager(this ICore core, out IPartitionManager output)
		{
			if (core == null)
				throw new ArgumentNullException("core");

			return core.Originators.GetChildren<IPartitionManager>().TryFirst(out output);
		}
	}
}
