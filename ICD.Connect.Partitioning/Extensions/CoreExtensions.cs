using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Partitioning.PartitionManagers;
using ICD.Connect.Settings.Cores;

namespace ICD.Connect.Partitioning.Extensions
{
	public static class CoreExtensions
	{
		/// <summary>
		/// Gets the partition manager instance from the core.
		/// </summary>
		/// <param name="core"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		public static bool TryGetPartitionManager([NotNull] this ICore core, out IPartitionManager output)
		{
			if (core == null)
				throw new ArgumentNullException("core");

			return core.Originators.GetChildren<IPartitionManager>().TryFirst(out output);
		}
	}
}
