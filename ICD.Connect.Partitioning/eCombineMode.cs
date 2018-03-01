using System;

namespace ICD.Connect.Partitioning
{
	[Flags]
	public enum eCombineMode
	{
		None = 0,
		Single = 1,
		Slave = 2,
		Master = 4,
		Combine = Slave | Master,
		Always = Single | Combine,
	}
}
