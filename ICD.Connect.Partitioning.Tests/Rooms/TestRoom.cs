using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Audio.VolumePoints;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings.Cores;
using ICD.Connect.Settings.Tests;

namespace ICD.Connect.Partitioning.Tests.Rooms
{
	public sealed class TestRoom : AbstractTestOriginator<TestRoomSettings>, IRoom
	{
		public event EventHandler<BoolEventArgs> OnCombineStateChanged;

		public override string Category { get { return "Room"; } }

		public ICore Core { get; set; }

		public bool CombineState { get; set; }

		public int CombinePriority { get; set; }

		public RoomOriginatorIdCollection Originators { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public TestRoom(ICore core)
		{
			Core = core;
			Originators = new RoomOriginatorIdCollection(this);
		}

		/// <summary>
		/// Informs the room it is part of a combined room.
		/// </summary>
		/// <param name="combine"></param>
		public void EnterCombineState(bool combine)
		{
			CombineState = combine;
		}

		/// <summary>
		/// Called before this combine space is destroyed as part of an uncombine operation.
		/// </summary>
		public void HandlePreUncombine()
		{
		}

		/// <summary>
		/// Gets the current volume context.
		/// </summary>
		public eVolumePointContext GetVolumeContext()
		{
			return default(eVolumePointContext);
		}
	}

	public sealed class TestRoomSettings : AbstractTestSettings
	{
	}
}