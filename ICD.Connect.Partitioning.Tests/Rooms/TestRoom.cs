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

		public void EnterCombineState()
		{
			throw new NotImplementedException();
		}

		public void LeaveCombineState()
		{
			throw new NotImplementedException();
		}
	}

	public sealed class TestRoomSettings: AbstractTestSettings
	{
	}
}