using System.Linq;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings.Core;
using NUnit.Framework;

namespace ICD.Connect.Partitioning.Tests.Rooms
{
	[TestFixture]
	public sealed class RoomExtensionsTest
	{
		#region Controls

		[Test]
		public void ContainsControlTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetControlGenericTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetControlTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetControlGenericByInfoTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetControlsTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void TryGetControlTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void TryGetControlGenericTest()
		{
			Assert.Inconclusive();
		}

		#endregion

		#region Control Recursion

		[Test]
		public void GetControlRecursiveGenericTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetControlRecursiveByInfoTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetControlRecursiveGenericByInfoTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetControlsRecursiveTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void TryGetControlRecursiveTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void TryGetControlRecursiveGenericTest()
		{
			Assert.Inconclusive();
		}

		#endregion

		#region Rooms

		[Test]
		public void IsCombineRoomTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetRoomsTest()
		{
			Core core = new Core
			{
				Id = 1
			};

			Room a = new Room
			{
				Id = 2
			};

			Room b = new Room
			{
				Id = 3
			};

			Room c = new Room
			{
				Id = 4
			};

			Partition partition = new Partition
			{
				Id = 5
			};

			core.Originators.AddChild(a);
			core.Originators.AddChild(b);
			core.Originators.AddChild(c);
			core.Originators.AddChild(partition);

			partition.AddRoom(a.Id);
			partition.AddRoom(b.Id);

			c.Originators.Add(partition.Id, eCombineMode.Always);

			IRoom[] aRooms = a.GetRooms().ToArray();
			IRoom[] bRooms = b.GetRooms().ToArray();
			IRoom[] cRooms = c.GetRooms().ToArray();

			Assert.AreEqual(0, aRooms.Length);
			Assert.AreEqual(0, bRooms.Length);
			Assert.AreEqual(2, cRooms.Length);

			Assert.IsTrue(cRooms.Contains(a));
			Assert.IsTrue(cRooms.Contains(b));
		}

		[Test]
		public void GetRoomsRecursiveTest()
		{
			Assert.Inconclusive();
		}

		#endregion

		[Test]
		public void HasDestinationOfTypeTest()
		{
			Assert.Inconclusive();
		}
	}
}
