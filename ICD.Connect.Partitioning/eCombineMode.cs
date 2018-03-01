using System;

namespace ICD.Connect.Partitioning
{
	/// <summary>
	/// eCombineMode describes the avilability of room contents based on the current combine state.
	/// </summary>
	[Flags]
	public enum eCombineMode
	{
		/// <summary>
		/// Never available.
		/// </summary>
		None = 0,

		/// <summary>
		/// Available in a single room.
		/// </summary>
		Single = 1,

		/// <summary>
		/// Available in a slaved room.
		/// </summary>
		Slave = 2,

		/// <summary>
		/// Available in a master room.
		/// </summary>
		Master = 4,

		/// <summary>
		/// Available in a combined room.
		/// </summary>
		Combine = Slave | Master,

		/// <summary>
		/// Always available.
		/// </summary>
		Always = Single | Combine,
	}
}
