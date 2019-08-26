using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Services;
using ICD.Connect.Partitioning.Cells;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Settings.Cores;
using ICD.Connect.Settings.Originators;
using NUnit.Framework;

namespace ICD.Connect.Partitioning.Tests.Rooms
{
	[TestFixture]
	public sealed class RoomOriginatorIdCollectionTest
	{
		[TearDown]
		public void TearDown()
		{
			ServiceProvider.RemoveAllServices();
		}

		[Test]
		public void ChildrenChangedFeedbackTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void CountTest()
		{
			Core core = new Core
			{
				Id = 1
			};

			TestRoom room = new TestRoom(core)
			{
				Id = 2
			};

			Cell tempA = new Cell
			{
				Id = 3
			};

			Cell tempB = new Cell
			{
				Id = 4
			};

			Cell tempC = new Cell
			{
				Id = 5
			};

			core.Originators.AddChild(room);
			core.Originators.AddChild(tempA);
			core.Originators.AddChild(tempB);
			core.Originators.AddChild(tempC);

			room.Originators.Add(3, eCombineMode.Always);
			room.Originators.Add(4, eCombineMode.Always);
			room.Originators.Add(5, eCombineMode.Always);

			Assert.AreEqual(3, room.Originators.Count);
		}

		#region Methods

		[Test]
		public void ClearTest()
		{
			Core core = new Core
			{
				Id = 1
			};

			TestRoom room = new TestRoom(core)
			{
				Id = 2
			};

			Cell tempA = new Cell
			{
				Id = 3
			};

			Cell tempB = new Cell
			{
				Id = 4
			};

			Cell tempC = new Cell
			{
				Id = 5
			};

			core.Originators.AddChild(room);
			core.Originators.AddChild(tempA);
			core.Originators.AddChild(tempB);
			core.Originators.AddChild(tempC);

			room.Originators.Add(3, eCombineMode.Always);
			room.Originators.Add(4, eCombineMode.Always);
			room.Originators.Add(5, eCombineMode.Always);

			room.Originators.Clear();

			Assert.AreEqual(0, room.Originators.GetIds().Count());
		}

		#region Ids

		[Test]
		public void GetIdsTest()
		{
			Core core = new Core
			{
				Id = 1
			};

			TestRoom room = new TestRoom(core)
			{
				Id = 2
			};

			Cell tempA = new Cell
			{
				Id = 3
			};

			Cell tempB = new Cell
			{
				Id = 4
			};

			Cell tempC = new Cell
			{
				Id = 5
			};

			core.Originators.AddChild(room);
			core.Originators.AddChild(tempA);
			core.Originators.AddChild(tempB);
			core.Originators.AddChild(tempC);

			room.Originators.Add(3, eCombineMode.Always);
			room.Originators.Add(4, eCombineMode.Always);
			room.Originators.Add(5, eCombineMode.Always);

			Assert.AreEqual(3, room.Originators.GetIds().Count());
			Assert.IsTrue(room.Originators.GetIds().SequenceEqual(new[] {3, 4, 5}));
		}

		[Test]
		public void SetIdsTest()
		{
			Core core = new Core
			{
				Id = 1
			};

			TestRoom room = new TestRoom(core)
			{
				Id = 2
			};

			Cell tempA = new Cell
			{
				Id = 3
			};

			Cell tempB = new Cell
			{
				Id = 4
			};

			Cell tempC = new Cell
			{
				Id = 5
			};

			core.Originators.AddChild(room);
			core.Originators.AddChild(tempA);
			core.Originators.AddChild(tempB);
			core.Originators.AddChild(tempC);

			room.Originators.Add(3, eCombineMode.Always);

			room.Originators.SetIds(new []
			{
				new KeyValuePair<int, eCombineMode>(4, eCombineMode.Always),
				new KeyValuePair<int, eCombineMode>(5, eCombineMode.Always)
			});

			Assert.AreEqual(2, room.Originators.GetIds().Count());
			Assert.IsTrue(room.Originators.GetIds().SequenceEqual(new[] { 4, 5 }));
		}

		[Test]
		public void AddTest()
		{
			Core core = new Core
			{
				Id = 1
			};

			TestRoom room = new TestRoom(core)
			{
				Id = 2
			};

			Cell tempA = new Cell
			{
				Id = 3
			};

			Cell tempB = new Cell
			{
				Id = 4
			};

			Cell tempC = new Cell
			{
				Id = 5
			};

			core.Originators.AddChild(room);
			core.Originators.AddChild(tempA);
			core.Originators.AddChild(tempB);
			core.Originators.AddChild(tempC);

			Assert.IsTrue(room.Originators.Add(3, eCombineMode.Always));
			Assert.IsTrue(room.Originators.Add(3, eCombineMode.Combine));
			Assert.IsFalse(room.Originators.Add(3, eCombineMode.Combine));

			Assert.IsTrue(room.Originators.Contains(3));
		}

		[Test]
		public void AddRangeTest()
		{
			Core core = new Core
			{
				Id = 1
			};

			TestRoom room = new TestRoom(core)
			{
				Id = 2
			};

			Cell tempA = new Cell
			{
				Id = 3
			};

			Cell tempB = new Cell
			{
				Id = 4
			};

			Cell tempC = new Cell
			{
				Id = 5
			};

			core.Originators.AddChild(room);
			core.Originators.AddChild(tempA);
			core.Originators.AddChild(tempB);
			core.Originators.AddChild(tempC);

			room.Originators.Add(3, eCombineMode.Always);

			room.Originators.AddRange(new[]
			{
				new KeyValuePair<int, eCombineMode>(4, eCombineMode.Always),
				new KeyValuePair<int, eCombineMode>(5, eCombineMode.Always)
			});

			Assert.AreEqual(3, room.Originators.GetIds().Count());
			Assert.IsTrue(room.Originators.GetIds().SequenceEqual(new[] { 3, 4, 5 }));
		}

		[Test]
		public void RemoveTest()
		{
			Core core = new Core
			{
				Id = 1
			};

			TestRoom room = new TestRoom(core)
			{
				Id = 2
			};

			Cell tempA = new Cell
			{
				Id = 3
			};

			Cell tempB = new Cell
			{
				Id = 4
			};

			Cell tempC = new Cell
			{
				Id = 5
			};

			core.Originators.AddChild(room);
			core.Originators.AddChild(tempA);
			core.Originators.AddChild(tempB);
			core.Originators.AddChild(tempC);

			room.Originators.Add(3, eCombineMode.Always);
			room.Originators.Add(4, eCombineMode.Always);
			room.Originators.Add(5, eCombineMode.Always);

			Assert.IsTrue(room.Originators.Remove(3));
			Assert.IsFalse(room.Originators.Remove(3));

			Assert.IsFalse(room.Originators.Contains(3));
		}

		[Test]
		public void ContainsTest()
		{
			Core core = new Core()
			{
				Id = 1
			};

			TestRoom room = new TestRoom(core)
			{
				Id = 2
			};

			Cell tempA = new Cell
			{
				Id = 3
			};

			Cell tempB = new Cell
			{
				Id = 4
			};

			Cell tempC = new Cell
			{
				Id = 5
			};

			core.Originators.AddChild(room);
			core.Originators.AddChild(tempA);
			core.Originators.AddChild(tempB);
			core.Originators.AddChild(tempC);

			Assert.IsFalse(room.Originators.Contains(3));

			room.Originators.Add(3, eCombineMode.Always);
			room.Originators.Add(4, eCombineMode.Always);
			room.Originators.Add(5, eCombineMode.Always);

			Assert.IsTrue(room.Originators.Contains(3));
		}

		#endregion

		#region Combine

		[Test]
		public void GetCombineModeTest()
		{
			Core core = new Core
			{
				Id = 1
			};

			TestRoom room = new TestRoom(core)
			{
				Id = 2
			};

			Cell tempA = new Cell
			{
				Id = 3
			};

			Cell tempB = new Cell
			{
				Id = 4
			};

			Cell tempC = new Cell
			{
				Id = 5
			};

			core.Originators.AddChild(room);
			core.Originators.AddChild(tempA);
			core.Originators.AddChild(tempB);
			core.Originators.AddChild(tempC);

			Assert.Throws<KeyNotFoundException>(() => room.Originators.GetCombineMode(6));

			room.Originators.Add(3, eCombineMode.Always);
			room.Originators.Add(4, eCombineMode.Always);
			room.Originators.Add(5, eCombineMode.Always);

			Assert.AreEqual(eCombineMode.Always, room.Originators.GetCombineMode(3));
		}

		#endregion

		#region Instances

		[Test]
		public void GetInstanceSelectorTest()
		{
			ICore core = new Core
			{
				Id = 1
			};

			TestRoom roomA = new TestRoom(core)
			{
				Id = 2,
				Core = core,
				Name = "A"
			};

			TestRoom roomB = new TestRoom(core)
			{
				Id = 3,
				Core = core,
				Name = "B"
			};

			TestRoom roomC = new TestRoom(core)
			{
				Id = 4,
				Core = core,
				Name = "C"
			};

			core.Originators.AddChild(roomA);
			core.Originators.AddChild(roomB);
			core.Originators.AddChild(roomC);

			Assert.IsNull(roomA.Originators.GetInstance<TestRoom>(o => o.Name == "B"));

			roomA.Originators.Add(roomB.Id, eCombineMode.Always);
			roomA.Originators.Add(roomC.Id, eCombineMode.Always);

			Assert.AreEqual(roomB, roomA.Originators.GetInstance<TestRoom>(o => o.Name == "B"));
			Assert.AreEqual(roomC, roomA.Originators.GetInstance<TestRoom>(o => o.Name == "C"));
			Assert.IsNull(roomA.Originators.GetInstance<TestRoom>(o => o.Name == "D"));
		}

		[Test]
		public void GetInstanceMaskSelectorTest()
		{
			ICore core = new Core
			{
				Id = 1
			};

			TestRoom roomA = new TestRoom(core)
			{
				Id = 2,
				Core = core,
				Name = "A"
			};

			TestRoom roomB = new TestRoom(core)
			{
				Id = 3,
				Core = core,
				Name = "B"
			};

			TestRoom roomC = new TestRoom(core)
			{
				Id = 4,
				Core = core,
				Name = "C"
			};

			core.Originators.AddChild(roomA);
			core.Originators.AddChild(roomB);
			core.Originators.AddChild(roomC);

			Assert.IsNull(roomA.Originators.GetInstance<TestRoom>(o => o.Name == "B"));

			roomA.Originators.Add(roomB.Id, eCombineMode.Combine);
			roomA.Originators.Add(roomC.Id, eCombineMode.Single);

			Assert.AreEqual(roomB, roomA.Originators.GetInstance<TestRoom>(eCombineMode.Combine, o => o.Name == "B"));
			Assert.AreEqual(roomC, roomA.Originators.GetInstance<TestRoom>(eCombineMode.Single, o => o.Name == "C"));
			Assert.IsNull(roomA.Originators.GetInstance<TestRoom>(eCombineMode.Single, o => o.Name == "B"));
			Assert.IsNull(roomA.Originators.GetInstance<TestRoom>(eCombineMode.Combine, o => o.Name == "C"));
			Assert.IsNull(roomA.Originators.GetInstance<TestRoom>(eCombineMode.Always, o => o.Name == "D"));
		}

		[Test]
		public void GetInstanceGenericTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetInstancesGenericTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetInstancesSelectorTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void HasInstancesTest()
		{
			Assert.Inconclusive();
		}

		#endregion

		#region Recursion

		[Test]
		public void ContainsRecursiveTest()
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

			TestRoom roomA = new TestRoom(core)
			{
				Core = core,
				Id = 2,
				CombinePriority = 1
			};

			TestRoom roomB = new TestRoom(core)
			{
				Core = core,
				Id = 3,
				CombinePriority = 2
			};

			TestRoom roomC = new TestRoom(core)
			{
				Core = core,
				Id = 4,
				CombinePriority = 2
			};

			Cell cellA = new Cell
			{
				Id = 5,
				Room = roomA,
				Column = 1,
				Row = 1
			};

			Cell cellB = new Cell
			{
				Id = 6,
				Room = roomB,
				Column = 2,
				Row = 1
			};

			Cell cellC = new Cell
			{
				Id = 7,
				Room = roomC,
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
			core.Originators.AddChild(roomA);
			core.Originators.AddChild(roomB);
			core.Originators.AddChild(roomC);
			core.Originators.AddChild(cellA);
			core.Originators.AddChild(cellB);
			core.Originators.AddChild(cellC);
			core.Originators.AddChild(partitionAb);
			core.Originators.AddChild(partitionBc);

			parent.Originators.Add(partitionAb.Id, eCombineMode.Always);
			parent.Originators.Add(partitionBc.Id, eCombineMode.Always);

			// Add children to Room A
			Cell roomA1 = new Cell { Id = 11, Name = "A1" };
			Cell roomA2 = new Cell { Id = 12, Name = "A2" };
			Cell roomA3 = new Cell { Id = 13, Name = "A3" };
			Cell roomA4 = new Cell { Id = 14, Name = "A4" };
			Cell roomA5 = new Cell { Id = 15, Name = "A5" };
			Cell roomA6 = new Cell { Id = 16, Name = "A6" };

			core.Originators.AddChild(roomA1);
			core.Originators.AddChild(roomA2);
			core.Originators.AddChild(roomA3);
			core.Originators.AddChild(roomA4);
			core.Originators.AddChild(roomA5);
			core.Originators.AddChild(roomA6);

			roomA.Originators.Add(roomA1.Id, eCombineMode.None);
			roomA.Originators.Add(roomA2.Id, eCombineMode.Single);
			roomA.Originators.Add(roomA3.Id, eCombineMode.Slave);
			roomA.Originators.Add(roomA4.Id, eCombineMode.Master);
			roomA.Originators.Add(roomA5.Id, eCombineMode.Combine);
			roomA.Originators.Add(roomA6.Id, eCombineMode.Always);

			// Add children to Room B
			Cell roomB1 = new Cell { Id = 17, Name = "B1" };
			Cell roomB2 = new Cell { Id = 18, Name = "B2" };
			Cell roomB3 = new Cell { Id = 19, Name = "B3" };
			Cell roomB4 = new Cell { Id = 20, Name = "B4" };
			Cell roomB5 = new Cell { Id = 21, Name = "B5" };
			Cell roomB6 = new Cell { Id = 22, Name = "B6" };

			core.Originators.AddChild(roomB1);
			core.Originators.AddChild(roomB2);
			core.Originators.AddChild(roomB3);
			core.Originators.AddChild(roomB4);
			core.Originators.AddChild(roomB5);
			core.Originators.AddChild(roomB6);

			roomB.Originators.Add(roomB1.Id, eCombineMode.None);
			roomB.Originators.Add(roomB2.Id, eCombineMode.Single);
			roomB.Originators.Add(roomB3.Id, eCombineMode.Slave);
			roomB.Originators.Add(roomB4.Id, eCombineMode.Master);
			roomB.Originators.Add(roomB5.Id, eCombineMode.Combine);
			roomB.Originators.Add(roomB6.Id, eCombineMode.Always);

			// Add children to Room C
			Cell roomC1 = new Cell { Id = 23, Name = "C1" };
			Cell roomC2 = new Cell { Id = 24, Name = "C2" };
			Cell roomC3 = new Cell { Id = 25, Name = "C3" };
			Cell roomC4 = new Cell { Id = 26, Name = "C4" };
			Cell roomC5 = new Cell { Id = 27, Name = "C5" };
			Cell roomC6 = new Cell { Id = 28, Name = "C6" };

			core.Originators.AddChild(roomC1);
			core.Originators.AddChild(roomC2);
			core.Originators.AddChild(roomC3);
			core.Originators.AddChild(roomC4);
			core.Originators.AddChild(roomC5);
			core.Originators.AddChild(roomC6);

			roomC.Originators.Add(roomC1.Id, eCombineMode.None);
			roomC.Originators.Add(roomC2.Id, eCombineMode.Single);
			roomC.Originators.Add(roomC3.Id, eCombineMode.Slave);
			roomC.Originators.Add(roomC4.Id, eCombineMode.Master);
			roomC.Originators.Add(roomC5.Id, eCombineMode.Combine);
			roomC.Originators.Add(roomC6.Id, eCombineMode.Always);

			// Parent Room
			Assert.IsFalse(parent.Originators.ContainsRecursive(roomA1.Id));
			Assert.IsFalse(parent.Originators.ContainsRecursive(roomA2.Id));
			Assert.IsFalse(parent.Originators.ContainsRecursive(roomA3.Id));
			Assert.IsTrue(parent.Originators.ContainsRecursive(roomA4.Id));
			Assert.IsTrue(parent.Originators.ContainsRecursive(roomA5.Id));
			Assert.IsTrue(parent.Originators.ContainsRecursive(roomA6.Id));

			Assert.IsFalse(parent.Originators.ContainsRecursive(roomB1.Id));
			Assert.IsFalse(parent.Originators.ContainsRecursive(roomB2.Id));
			Assert.IsTrue(parent.Originators.ContainsRecursive(roomB3.Id));
			Assert.IsFalse(parent.Originators.ContainsRecursive(roomB4.Id));
			Assert.IsTrue(parent.Originators.ContainsRecursive(roomB5.Id));
			Assert.IsTrue(parent.Originators.ContainsRecursive(roomB6.Id));

			Assert.IsFalse(parent.Originators.ContainsRecursive(roomC1.Id));
			Assert.IsFalse(parent.Originators.ContainsRecursive(roomC2.Id));
			Assert.IsTrue(parent.Originators.ContainsRecursive(roomC3.Id));
			Assert.IsFalse(parent.Originators.ContainsRecursive(roomC4.Id));
			Assert.IsTrue(parent.Originators.ContainsRecursive(roomC5.Id));
			Assert.IsTrue(parent.Originators.ContainsRecursive(roomC6.Id));

			// Room A
			Assert.IsFalse(roomA.Originators.ContainsRecursive(roomA1.Id));
			Assert.IsTrue(roomA.Originators.ContainsRecursive(roomA2.Id));
			Assert.IsTrue(roomA.Originators.ContainsRecursive(roomA3.Id));
			Assert.IsTrue(roomA.Originators.ContainsRecursive(roomA4.Id));
			Assert.IsTrue(roomA.Originators.ContainsRecursive(roomA5.Id));
			Assert.IsTrue(roomA.Originators.ContainsRecursive(roomA6.Id));

			// Room B
			Assert.IsFalse(roomB.Originators.ContainsRecursive(roomB1.Id));
			Assert.IsTrue(roomB.Originators.ContainsRecursive(roomB2.Id));
			Assert.IsTrue(roomB.Originators.ContainsRecursive(roomB3.Id));
			Assert.IsTrue(roomB.Originators.ContainsRecursive(roomB4.Id));
			Assert.IsTrue(roomB.Originators.ContainsRecursive(roomB5.Id));
			Assert.IsTrue(roomB.Originators.ContainsRecursive(roomB6.Id));

			// Room C
			Assert.IsFalse(roomC.Originators.ContainsRecursive(roomC1.Id));
			Assert.IsTrue(roomC.Originators.ContainsRecursive(roomC2.Id));
			Assert.IsTrue(roomC.Originators.ContainsRecursive(roomC3.Id));
			Assert.IsTrue(roomC.Originators.ContainsRecursive(roomC4.Id));
			Assert.IsTrue(roomC.Originators.ContainsRecursive(roomC5.Id));
			Assert.IsTrue(roomC.Originators.ContainsRecursive(roomC6.Id));
		}

		[Test]
		public void ContainsRecursiveMaskTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetInstanceRecursiveIdTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetInstanceRecursiveGenericIdTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetInstanceRecursiveGenericTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetInstanceRecursiveSelectorTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetInstancesRecursiveTest()
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

			TestRoom roomA = new TestRoom(core)
			{
				Core = core,
				Id = 2,
				CombinePriority = 1
			};

			TestRoom roomB = new TestRoom(core)
			{
				Core = core,
				Id = 3,
				CombinePriority = 2
			};

			TestRoom roomC = new TestRoom(core)
			{
				Core = core,
				Id = 4,
				CombinePriority = 2
			};

			Cell cellA = new Cell
			{
				Id = 5,
				Room = roomA,
				Column = 1,
				Row = 1
			};

			Cell cellB = new Cell
			{
				Id = 6,
				Room = roomB,
				Column = 2,
				Row = 1
			};

			Cell cellC = new Cell
			{
				Id = 7,
				Room = roomC,
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
			core.Originators.AddChild(roomA);
			core.Originators.AddChild(roomB);
			core.Originators.AddChild(roomC);
			core.Originators.AddChild(cellA);
			core.Originators.AddChild(cellB);
			core.Originators.AddChild(cellC);
			core.Originators.AddChild(partitionAb);
			core.Originators.AddChild(partitionBc);

			parent.Originators.Add(partitionAb.Id, eCombineMode.Always);
			parent.Originators.Add(partitionBc.Id, eCombineMode.Always);

			// Add children to Room A
			Cell roomA1 = new Cell { Id = 11, Name = "A1" };
			Cell roomA2 = new Cell { Id = 12, Name = "A2" };
			Cell roomA3 = new Cell { Id = 13, Name = "A3" };
			Cell roomA4 = new Cell { Id = 14, Name = "A4" };
			Cell roomA5 = new Cell { Id = 15, Name = "A5" };
			Cell roomA6 = new Cell { Id = 16, Name = "A6" };

			core.Originators.AddChild(roomA1);
			core.Originators.AddChild(roomA2);
			core.Originators.AddChild(roomA3);
			core.Originators.AddChild(roomA4);
			core.Originators.AddChild(roomA5);
			core.Originators.AddChild(roomA6);

			roomA.Originators.Add(roomA1.Id, eCombineMode.None);
			roomA.Originators.Add(roomA2.Id, eCombineMode.Single);
			roomA.Originators.Add(roomA3.Id, eCombineMode.Slave);
			roomA.Originators.Add(roomA4.Id, eCombineMode.Master);
			roomA.Originators.Add(roomA5.Id, eCombineMode.Combine);
			roomA.Originators.Add(roomA6.Id, eCombineMode.Always);

			// Add children to Room B
			Cell roomB1 = new Cell { Id = 17, Name = "B1" };
			Cell roomB2 = new Cell { Id = 18, Name = "B2" };
			Cell roomB3 = new Cell { Id = 19, Name = "B3" };
			Cell roomB4 = new Cell { Id = 20, Name = "B4" };
			Cell roomB5 = new Cell { Id = 21, Name = "B5" };
			Cell roomB6 = new Cell { Id = 22, Name = "B6" };

			core.Originators.AddChild(roomB1);
			core.Originators.AddChild(roomB2);
			core.Originators.AddChild(roomB3);
			core.Originators.AddChild(roomB4);
			core.Originators.AddChild(roomB5);
			core.Originators.AddChild(roomB6);

			roomB.Originators.Add(roomB1.Id, eCombineMode.None);
			roomB.Originators.Add(roomB2.Id, eCombineMode.Single);
			roomB.Originators.Add(roomB3.Id, eCombineMode.Slave);
			roomB.Originators.Add(roomB4.Id, eCombineMode.Master);
			roomB.Originators.Add(roomB5.Id, eCombineMode.Combine);
			roomB.Originators.Add(roomB6.Id, eCombineMode.Always);

			// Add children to Room C
			Cell roomC1 = new Cell { Id = 23, Name = "C1" };
			Cell roomC2 = new Cell { Id = 24, Name = "C2" };
			Cell roomC3 = new Cell { Id = 25, Name = "C3" };
			Cell roomC4 = new Cell { Id = 26, Name = "C4" };
			Cell roomC5 = new Cell { Id = 27, Name = "C5" };
			Cell roomC6 = new Cell { Id = 28, Name = "C6" };

			core.Originators.AddChild(roomC1);
			core.Originators.AddChild(roomC2);
			core.Originators.AddChild(roomC3);
			core.Originators.AddChild(roomC4);
			core.Originators.AddChild(roomC5);
			core.Originators.AddChild(roomC6);

			roomC.Originators.Add(roomC1.Id, eCombineMode.None);
			roomC.Originators.Add(roomC2.Id, eCombineMode.Single);
			roomC.Originators.Add(roomC3.Id, eCombineMode.Slave);
			roomC.Originators.Add(roomC4.Id, eCombineMode.Master);
			roomC.Originators.Add(roomC5.Id, eCombineMode.Combine);
			roomC.Originators.Add(roomC6.Id, eCombineMode.Always);

			// Get recursive
			IOriginator[] parentContents = parent.Originators.GetInstancesRecursive(eCombineMode.Always).ToArray();
			IOriginator[] roomAContents = roomA.Originators.GetInstancesRecursive(eCombineMode.Always).ToArray();
			IOriginator[] roomBContents = roomB.Originators.GetInstancesRecursive(eCombineMode.Always).ToArray();
			IOriginator[] roomCContents = roomC.Originators.GetInstancesRecursive(eCombineMode.Always).ToArray();

			// Parent Room Contents
			Assert.IsFalse(parentContents.Contains(roomA1));
			Assert.IsFalse(parentContents.Contains(roomA2));
			Assert.IsFalse(parentContents.Contains(roomA3));
			Assert.IsTrue(parentContents.Contains(roomA4));
			Assert.IsTrue(parentContents.Contains(roomA5));
			Assert.IsTrue(parentContents.Contains(roomA6));

			Assert.IsFalse(parentContents.Contains(roomB1));
			Assert.IsFalse(parentContents.Contains(roomB2));
			Assert.IsTrue(parentContents.Contains(roomB3));
			Assert.IsFalse(parentContents.Contains(roomB4));
			Assert.IsTrue(parentContents.Contains(roomB5));
			Assert.IsTrue(parentContents.Contains(roomB6));

			Assert.IsFalse(parentContents.Contains(roomC1));
			Assert.IsFalse(parentContents.Contains(roomC2));
			Assert.IsTrue(parentContents.Contains(roomC3));
			Assert.IsFalse(parentContents.Contains(roomC4));
			Assert.IsTrue(parentContents.Contains(roomC5));
			Assert.IsTrue(parentContents.Contains(roomC6));

			// Room A Contents
			Assert.IsFalse(roomAContents.Contains(roomA1));
			Assert.IsTrue(roomAContents.Contains(roomA2));
			Assert.IsTrue(roomAContents.Contains(roomA3));
			Assert.IsTrue(roomAContents.Contains(roomA4));
			Assert.IsTrue(roomAContents.Contains(roomA5));
			Assert.IsTrue(roomAContents.Contains(roomA6));

			// Room B Contents
			Assert.IsFalse(roomBContents.Contains(roomB1));
			Assert.IsTrue(roomBContents.Contains(roomB2));
			Assert.IsTrue(roomBContents.Contains(roomB3));
			Assert.IsTrue(roomBContents.Contains(roomB4));
			Assert.IsTrue(roomBContents.Contains(roomB5));
			Assert.IsTrue(roomBContents.Contains(roomB6));

			// Room C Contents
			Assert.IsFalse(roomCContents.Contains(roomC1));
			Assert.IsTrue(roomCContents.Contains(roomC2));
			Assert.IsTrue(roomCContents.Contains(roomC3));
			Assert.IsTrue(roomCContents.Contains(roomC4));
			Assert.IsTrue(roomCContents.Contains(roomC5));
			Assert.IsTrue(roomCContents.Contains(roomC6));
		}

		[Test]
		public void GetInstancesRecursiveGenericTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetInstancesRecursiveSelectorTest()
		{
			Assert.Inconclusive();
		}

		#endregion

		#endregion
	}
}
