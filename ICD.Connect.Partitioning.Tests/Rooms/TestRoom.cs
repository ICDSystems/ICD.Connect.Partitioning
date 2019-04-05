using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings.Core;
using ICD.Connect.Settings.Tests;

namespace ICD.Connect.Partitioning.Tests.Rooms
{
	public sealed class TestRoom : AbstractTestOriginator<TestRoomSettings>, IRoom
	{
		public event EventHandler<BoolEventArgs> OnCombineStateChanged;

		public ICore Core { get; set; }

		public bool CombineState { get { throw new NotImplementedException(); } }

		public int CombinePriority { get; set; }

		public RoomOriginatorIdCollection Originators { get; set; }

		/// <summary>
		/// Informs the room it is part of a combined room.
		/// </summary>
		/// <param name="combine"></param>
		public void EnterCombineState(bool combine)
		{
			throw new NotImplementedException();
		}

		public RoomOriginatorIdCollection AudioDestinations { get; set; }
	}

	public sealed class TestRoomSettings: AbstractTestSettings
	{
	}
}