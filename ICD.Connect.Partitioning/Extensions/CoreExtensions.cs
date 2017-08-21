using System;
using ICD.Common.Properties;
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
	}
}
