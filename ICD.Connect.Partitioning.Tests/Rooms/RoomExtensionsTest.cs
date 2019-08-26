using System.Linq;
using ICD.Common.Utils.Services;
using ICD.Connect.Partitioning.Cells;
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

			TestRoom parent = new TestRoom(core)
			{
				Core = core,
				Id = 10
			};

			TestRoom a = new TestRoom(core)
			{
				Core = core,
				Id = 2,
				CombinePriority = 5
			};

			TestRoom b = new TestRoom(core)
			{
				Core = core,
				Id = 3,
				CombinePriority = 1
			};

			TestRoom c = new TestRoom(core)
			{
				Core = core,
				Id = 4,
				CombinePriority = 1
			};

			Cell cellA = new Cell
			{
				Id = 5,
				Room = a,
				Column = 1,
				Row = 1
			};

			Cell cellB = new Cell
			{
				Id = 6,
				Room = b,
				Column = 2,
				Row = 1
			};

			Cell cellC = new Cell
			{
				Id = 7,
				Room = c,
				Column = 3,
				Row = 1
			};

			Partition partitionAb = new Partition
			{
				Id = 8,
				CellA = cellA,
				CellB = cellB
			};

			Partition partitionBc = new Partition
			{
				Id = 9,
				CellA = cellB,
				CellB = cellC
			};

			core.Originators.AddChild(parent);
			core.Originators.AddChild(a);
			core.Originators.AddChild(b);
			core.Originators.AddChild(c);
			core.Originators.AddChild(cellA);
			core.Originators.AddChild(cellB);
			core.Originators.AddChild(cellC);
			core.Originators.AddChild(partitionAb);
			core.Originators.AddChild(partitionBc);

			parent.Originators.Add(partitionAb.Id, eCombineMode.Always);
			parent.Originators.Add(partitionBc.Id, eCombineMode.Always);

			Assert.IsFalse(a.IsCombineRoom());
			Assert.IsFalse(b.IsCombineRoom());
			Assert.IsFalse(c.IsCombineRoom());
			Assert.IsTrue(parent.IsCombineRoom());
		}

		[Test]
		public void GetMasterRoomTest()
		{
			Core core = new Core
			{
				Id = 1
			};

			TestRoom parent = new TestRoom(core)
			{
				Core = core,
				Id = 10
			};

			TestRoom a = new TestRoom(core)
			{
				Core = core,
				Id = 2,
				CombinePriority = 5
			};

			TestRoom b = new TestRoom(core)
			{
				Core = core,
				Id = 3,
				CombinePriority = 1
			};

			TestRoom c = new TestRoom(core)
			{
				Core = core,
				Id = 4,
				CombinePriority = 1
			};

			Cell cellA = new Cell
			{
				Id = 5,
				Room = a,
				Column = 1,
				Row = 1
			};

			Cell cellB = new Cell
			{
				Id = 6,
				Room = b,
				Column = 2,
				Row = 1
			};

			Cell cellC = new Cell
			{
				Id = 7,
				Room = c,
				Column = 3,
				Row = 1
			};

			Partition partitionAb = new Partition
			{
				Id = 8,
				CellA = cellA,
				CellB = cellB
			};

			Partition partitionBc = new Partition
			{
				Id = 9,
				CellA = cellB,
				CellB = cellC
			};

			core.Originators.AddChild(parent);
			core.Originators.AddChild(a);
			core.Originators.AddChild(b);
			core.Originators.AddChild(c);
			core.Originators.AddChild(cellA);
			core.Originators.AddChild(cellB);
			core.Originators.AddChild(cellC);
			core.Originators.AddChild(partitionAb);
			core.Originators.AddChild(partitionBc);

			parent.Originators.Add(partitionAb.Id, eCombineMode.Always);
			parent.Originators.Add(partitionBc.Id, eCombineMode.Always);

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

			TestRoom parent = new TestRoom(core)
			{
				Core = core,
				Id = 10
			};

			TestRoom a = new TestRoom(core)
			{
				Core = core,
				Id = 2,
				CombinePriority = 5
			};

			TestRoom b = new TestRoom(core)
			{
				Core = core,
				Id = 3,
				CombinePriority = 1
			};

			TestRoom c = new TestRoom(core)
			{
				Core = core,
				Id = 4,
				CombinePriority = 1
			};

			Cell cellA = new Cell
			{
				Id = 5,
				Room = a,
				Column = 1,
				Row = 1
			};

			Cell cellB = new Cell
			{
				Id = 6,
				Room = b,
				Column = 2,
				Row = 1
			};

			Cell cellC = new Cell
			{
				Id = 7,
				Room = c,
				Column = 3,
				Row = 1
			};

			Partition partitionAb = new Partition
			{
				Id = 8,
				CellA = cellA,
				CellB = cellB
			};

			Partition partitionBc = new Partition
			{
				Id = 9,
				CellA = cellB,
				CellB = cellC
			};

			core.Originators.AddChild(parent);
			core.Originators.AddChild(a);
			core.Originators.AddChild(b);
			core.Originators.AddChild(c);
			core.Originators.AddChild(cellA);
			core.Originators.AddChild(cellB);
			core.Originators.AddChild(cellC);
			core.Originators.AddChild(partitionAb);
			core.Originators.AddChild(partitionBc);

			parent.Originators.Add(partitionAb.Id, eCombineMode.Always);
			parent.Originators.Add(partitionBc.Id, eCombineMode.Always);

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

			TestRoom parent = new TestRoom(core)
			{
				Core = core,
				Id = 10
			};

			TestRoom a = new TestRoom(core)
			{
				Core = core,
				Id = 2,
				CombinePriority = 5
			};

			TestRoom b = new TestRoom(core)
			{
				Core = core,
				Id = 3,
				CombinePriority = 1
			};

			TestRoom c = new TestRoom(core)
			{
				Core = core,
				Id = 4,
				CombinePriority = 1
			};

			Cell cellA = new Cell
			{
				Id = 5,
				Room = a,
				Column = 1,
				Row = 1
			};

			Cell cellB = new Cell
			{
				Id = 6,
				Room = b,
				Column = 2,
				Row = 1
			};

			Cell cellC = new Cell
			{
				Id = 7,
				Room = c,
				Column = 3,
				Row = 1
			};

			Partition partitionAb = new Partition
			{
				Id = 8,
				CellA = cellA,
				CellB = cellB
			};

			Partition partitionBc = new Partition
			{
				Id = 9,
				CellA = cellB,
				CellB = cellC
			};

			core.Originators.AddChild(parent);
			core.Originators.AddChild(a);
			core.Originators.AddChild(b);
			core.Originators.AddChild(c);
			core.Originators.AddChild(cellA);
			core.Originators.AddChild(cellB);
			core.Originators.AddChild(cellC);
			core.Originators.AddChild(partitionAb);
			core.Originators.AddChild(partitionBc);

			parent.Originators.Add(partitionAb.Id, eCombineMode.Always);
			parent.Originators.Add(partitionBc.Id, eCombineMode.Always);

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

			TestRoom parent = new TestRoom(core)
			{
				Core = core,
				Id = 10
			};

			TestRoom a = new TestRoom(core)
			{
				Core = core,
				Id = 2,
				CombinePriority = 5
			};

			TestRoom b = new TestRoom(core)
			{
				Core = core,
				Id = 3,
				CombinePriority = 1
			};

			TestRoom c = new TestRoom(core)
			{
				Core = core,
				Id = 4,
				CombinePriority = 1
			};

			Cell cellA = new Cell
			{
				Id = 5,
				Room = a,
				Column = 1,
				Row = 1
			};

			Cell cellB = new Cell
			{
				Id = 6,
				Room = b,
				Column = 2,
				Row = 1
			};

			Cell cellC = new Cell
			{
				Id = 7,
				Room = c,
				Column = 3,
				Row = 1
			};

			Partition partitionAb = new Partition
			{
				Id = 8,
				CellA = cellA,
				CellB = cellB
			};

			Partition partitionBc = new Partition
			{
				Id = 9,
				CellA = cellB,
				CellB = cellC
			};

			core.Originators.AddChild(parent);
			core.Originators.AddChild(a);
			core.Originators.AddChild(b);
			core.Originators.AddChild(c);
			core.Originators.AddChild(cellA);
			core.Originators.AddChild(cellB);
			core.Originators.AddChild(cellC);
			core.Originators.AddChild(partitionAb);
			core.Originators.AddChild(partitionBc);

			parent.Originators.Add(partitionAb.Id, eCombineMode.Always);
			parent.Originators.Add(partitionBc.Id, eCombineMode.Always);

			IRoom[] aRooms = a.GetRooms().ToArray();
			IRoom[] bRooms = b.GetRooms().ToArray();
			IRoom[] cRooms = c.GetRooms().ToArray();
			IRoom[] parentRooms = parent.GetRooms().ToArray();

			Assert.AreEqual(0, aRooms.Length);
			Assert.AreEqual(0, bRooms.Length);
			Assert.AreEqual(0, cRooms.Length);
			Assert.AreEqual(3, parentRooms.Length);

			Assert.IsTrue(parentRooms.Contains(a));
			Assert.IsTrue(parentRooms.Contains(b));
			Assert.IsTrue(parentRooms.Contains(c));
		}

		[Test]
		public void GetRoomsRecursiveTest()
		{
			Core core = new Core
			{
				Id = 1
			};

			TestRoom parent = new TestRoom(core)
			{
				Core = core,
				Id = 10
			};

			TestRoom a = new TestRoom(core)
			{
				Core = core,
				Id = 2,
				CombinePriority = 5
			};

			TestRoom b = new TestRoom(core)
			{
				Core = core,
				Id = 3,
				CombinePriority = 1
			};

			TestRoom c = new TestRoom(core)
			{
				Core = core,
				Id = 4,
				CombinePriority = 1
			};

			Cell cellA = new Cell
			{
				Id = 5,
				Room = a,
				Column = 1,
				Row = 1
			};

			Cell cellB = new Cell
			{
				Id = 6,
				Room = b,
				Column = 2,
				Row = 1
			};

			Cell cellC = new Cell
			{
				Id = 7,
				Room = c,
				Column = 3,
				Row = 1
			};

			Partition partitionAb = new Partition
			{
				Id = 8,
				CellA = cellA,
				CellB = cellB
			};

			Partition partitionBc = new Partition
			{
				Id = 9,
				CellA = cellB,
				CellB = cellC
			};

			core.Originators.AddChild(parent);
			core.Originators.AddChild(a);
			core.Originators.AddChild(b);
			core.Originators.AddChild(c);
			core.Originators.AddChild(cellA);
			core.Originators.AddChild(cellB);
			core.Originators.AddChild(cellC);
			core.Originators.AddChild(partitionAb);
			core.Originators.AddChild(partitionBc);

			parent.Originators.Add(partitionAb.Id, eCombineMode.Always);
			parent.Originators.Add(partitionBc.Id, eCombineMode.Always);

			IRoom[] aRooms = a.GetRoomsRecursive().ToArray();
			IRoom[] bRooms = b.GetRoomsRecursive().ToArray();
			IRoom[] cRooms = c.GetRoomsRecursive().ToArray();
			IRoom[] parentRooms = parent.GetRoomsRecursive().ToArray();

			Assert.AreEqual(1, aRooms.Length);
			Assert.AreEqual(1, bRooms.Length);
			Assert.AreEqual(1, cRooms.Length);
			Assert.AreEqual(4, parentRooms.Length);

			Assert.IsTrue(aRooms.Contains(a));
			Assert.IsTrue(bRooms.Contains(b));
			Assert.IsTrue(cRooms.Contains(c));

			Assert.IsTrue(parentRooms.Contains(a));
			Assert.IsTrue(parentRooms.Contains(b));
			Assert.IsTrue(parentRooms.Contains(c));
			Assert.IsTrue(parentRooms.Contains(parent));
		}

		#endregion

		[Test]
		public void HasDestinationOfTypeTest()
		{
			Assert.Inconclusive();
		}
	}
}
