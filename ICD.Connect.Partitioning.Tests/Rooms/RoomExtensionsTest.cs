using System.Linq;
using ICD.Common.Utils.Services;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings.Cores;
using NUnit.Framework;

namespace ICD.Connect.Partitioning.Tests.Rooms
{
	[TestFixture]
	public sealed class RoomExtensionsTest
	{
		[TearDown]
		public void TearDown()
		{
			ServiceProvider.RemoveAllServices();
		}

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

			Assert.IsFalse(a.IsCombineRoom());
			Assert.IsFalse(b.IsCombineRoom());
			Assert.IsTrue(c.IsCombineRoom());
		}

		[Test]
		public void GetMasterRoomTest()
		{
			Core core = new Core
			{
				Id = 1
			};

			Room parent = new Room
			{
				Id = 10
			};

			Room a = new Room
			{
				Id = 2,
				CombinePriority = 5
			};

			Room b = new Room
			{
				Id = 3,
				CombinePriority = 1
			};

			Room c = new Room
			{
				Id = 4,
				CombinePriority = 1
			};

			Partition partition = new Partition
			{
				Id = 5
			};

			core.Originators.AddChild(parent);
			core.Originators.AddChild(a);
			core.Originators.AddChild(b);
			core.Originators.AddChild(c);
			core.Originators.AddChild(partition);

			partition.AddRoom(a.Id);
			partition.AddRoom(b.Id);
			partition.AddRoom(c.Id);

			parent.Originators.Add(partition.Id, eCombineMode.Always);

			Assert.IsNull(a.GetMasterRoom());
			Assert.IsNull(b.GetMasterRoom());
			Assert.IsNull(c.GetMasterRoom());
			Assert.AreEqual(b, parent.GetMasterRoom());
		}

		[Test]
		public void GetSlaveRoomsTest()
		{
			Core core = new Core
			{
				Id = 1
			};

			Room parent = new Room
			{
				Id = 10
			};

			Room a = new Room
			{
				Id = 2,
				CombinePriority = 5
			};

			Room b = new Room
			{
				Id = 3,
				CombinePriority = 1
			};

			Room c = new Room
			{
				Id = 4,
				CombinePriority = 1
			};

			Partition partition = new Partition
			{
				Id = 5
			};

			core.Originators.AddChild(parent);
			core.Originators.AddChild(a);
			core.Originators.AddChild(b);
			core.Originators.AddChild(c);
			core.Originators.AddChild(partition);

			partition.AddRoom(a.Id);
			partition.AddRoom(b.Id);
			partition.AddRoom(c.Id);

			parent.Originators.Add(partition.Id, eCombineMode.Always);

			Assert.IsEmpty(a.GetSlaveRooms());
			Assert.IsEmpty(b.GetSlaveRooms());
			Assert.IsEmpty(c.GetSlaveRooms());

			IRoom[] slaves = parent.GetSlaveRooms().ToArray();
			
			Assert.AreEqual(2, slaves.Length);
			Assert.IsTrue(slaves.Contains(a));
			Assert.IsTrue(slaves.Contains(c));
		}

		[Test]
		public void GetMasterAndSlaveRoomsTest()
		{
			Core core = new Core
			{
				Id = 1
			};

			Room parent = new Room
			{
				Id = 10
			};

			Room a = new Room
			{
				Id = 2,
				CombinePriority = 5
			};

			Room b = new Room
			{
				Id = 3,
				CombinePriority = 1
			};

			Room c = new Room
			{
				Id = 4,
				CombinePriority = 1
			};

			Partition partition = new Partition
			{
				Id = 5
			};

			core.Originators.AddChild(parent);
			core.Originators.AddChild(a);
			core.Originators.AddChild(b);
			core.Originators.AddChild(c);
			core.Originators.AddChild(partition);

			partition.AddRoom(a.Id);
			partition.AddRoom(b.Id);
			partition.AddRoom(c.Id);

			parent.Originators.Add(partition.Id, eCombineMode.Always);

			Assert.IsEmpty(a.GetSlaveRooms());
			Assert.IsEmpty(b.GetSlaveRooms());
			Assert.IsEmpty(c.GetSlaveRooms());

			IRoom[] masterAndSlaves = parent.GetMasterAndSlaveRooms().ToArray();

			Assert.AreEqual(3, masterAndSlaves.Length);
			Assert.AreEqual(b, masterAndSlaves[0]);
			Assert.IsTrue(masterAndSlaves.Contains(a));
			Assert.IsTrue(masterAndSlaves.Contains(c));
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

			IRoom[] aRooms = a.GetRoomsRecursive().ToArray();
			IRoom[] bRooms = b.GetRoomsRecursive().ToArray();
			IRoom[] cRooms = c.GetRoomsRecursive().ToArray();

			Assert.AreEqual(1, aRooms.Length);
			Assert.AreEqual(1, bRooms.Length);
			Assert.AreEqual(3, cRooms.Length);

			Assert.IsTrue(aRooms.Contains(a));

			Assert.IsTrue(bRooms.Contains(b));

			Assert.IsTrue(cRooms.Contains(a));
			Assert.IsTrue(cRooms.Contains(b));
			Assert.IsTrue(cRooms.Contains(c));
		}

		#endregion

		[Test]
		public void HasDestinationOfTypeTest()
		{
			Assert.Inconclusive();
		}
	}
}
