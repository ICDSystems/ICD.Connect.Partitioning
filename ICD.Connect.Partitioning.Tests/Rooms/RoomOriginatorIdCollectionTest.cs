using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Services;
using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Partitioning.Rooms;
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
			TestRoom room = new TestRoom();
			room.Originators = new RoomOriginatorIdCollection(room);

			room.Originators.Add(1, eCombineMode.Always);
			room.Originators.Add(3, eCombineMode.Always);
			room.Originators.Add(2, eCombineMode.Always);

			Assert.AreEqual(3, room.Originators.Count);
		}

		#region Methods

		[Test]
		public void ClearTest()
		{
			TestRoom room = new TestRoom();
			room.Originators = new RoomOriginatorIdCollection(room);

			room.Originators.Add(1, eCombineMode.Always);
			room.Originators.Add(3, eCombineMode.Always);
			room.Originators.Add(2, eCombineMode.Always);

			room.Originators.Clear();

			Assert.AreEqual(0, room.Originators.GetIds().Count());
		}

		#region Ids

		[Test]
		public void GetIdsTest()
		{
			TestRoom room = new TestRoom();
			room.Originators = new RoomOriginatorIdCollection(room);

			room.Originators.Add(1, eCombineMode.Always);
			room.Originators.Add(3, eCombineMode.Always);
			room.Originators.Add(2, eCombineMode.Always);

			Assert.AreEqual(3, room.Originators.GetIds().Count());
			Assert.IsTrue(room.Originators.GetIds().SequenceEqual(new[] {1, 2, 3}));
		}

		[Test]
		public void SetIdsTest()
		{
			TestRoom room = new TestRoom();
			room.Originators = new RoomOriginatorIdCollection(room);

			room.Originators.Add(1, eCombineMode.Always);
			room.Originators.Add(3, eCombineMode.Always);
			room.Originators.Add(2, eCombineMode.Always);

			room.Originators.SetIds(new []
			{
				new KeyValuePair<int, eCombineMode>(10, eCombineMode.Always),
				new KeyValuePair<int, eCombineMode>(12, eCombineMode.Always),
				new KeyValuePair<int, eCombineMode>(11, eCombineMode.Always),
			});

			Assert.AreEqual(3, room.Originators.GetIds().Count());
			Assert.IsTrue(room.Originators.GetIds().SequenceEqual(new[] { 10, 11, 12 }));
		}

		[Test]
		public void AddTest()
		{
			TestRoom room = new TestRoom();
			room.Originators = new RoomOriginatorIdCollection(room);

			Assert.IsTrue(room.Originators.Add(1, eCombineMode.Always));
			Assert.IsTrue(room.Originators.Add(1, eCombineMode.Combine));
			Assert.IsFalse(room.Originators.Add(1, eCombineMode.Combine));

			Assert.IsTrue(room.Originators.Contains(1));
		}

		[Test]
		public void AddRangeTest()
		{
			TestRoom room = new TestRoom();
			room.Originators = new RoomOriginatorIdCollection(room);

			room.Originators.AddRange(new[]
			{
				new KeyValuePair<int, eCombineMode>(10, eCombineMode.Always),
				new KeyValuePair<int, eCombineMode>(12, eCombineMode.Always),
				new KeyValuePair<int, eCombineMode>(11, eCombineMode.Always),
			});

			Assert.AreEqual(3, room.Originators.GetIds().Count());
			Assert.IsTrue(room.Originators.GetIds().SequenceEqual(new[] { 10, 11, 12 }));
		}

		[TestCase(1, eCombineMode.Always)]
		public void RemoveTest(int id, eCombineMode combineMode)
		{
			TestRoom room = new TestRoom();
			room.Originators = new RoomOriginatorIdCollection(room);

			room.Originators.Add(id, combineMode);

			Assert.IsTrue(room.Originators.Remove(id));
			Assert.IsFalse(room.Originators.Remove(id));

			Assert.IsFalse(room.Originators.Contains(id));
		}

		[TestCase(1, eCombineMode.Always)]
		public void ContainsTest(int id, eCombineMode combineMode)
		{
			TestRoom room = new TestRoom();
			room.Originators = new RoomOriginatorIdCollection(room);

			Assert.IsFalse(room.Originators.Contains(id));

			room.Originators.Add(id, combineMode);

			Assert.IsTrue(room.Originators.Contains(id));
		}

		#endregion

		#region Combine

		[TestCase(1, eCombineMode.Always)]
		public void GetCombineModeTest(int id, eCombineMode combineMode)
		{
			TestRoom room = new TestRoom();
			room.Originators = new RoomOriginatorIdCollection(room);

			Assert.Throws<KeyNotFoundException>(() => room.Originators.GetCombineMode(id));

			room.Originators.Add(id, combineMode);

			Assert.AreEqual(combineMode, room.Originators.GetCombineMode(id));
		}

		#endregion

		#region Instances

		[Test]
		public void GetInstanceSelectorTest()
		{
			ICore core = new Core();

			TestRoom roomA = new TestRoom { Id = 1, Core = core, Name = "A" };
			TestRoom roomB = new TestRoom { Id = 2, Core = core, Name = "B" };
			TestRoom roomC = new TestRoom { Id = 3, Core = core, Name = "C" };

			core.Originators.AddChild(roomA);
			core.Originators.AddChild(roomB);
			core.Originators.AddChild(roomC);

			roomA.Originators = new RoomOriginatorIdCollection(roomA);

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
			ICore core = new Core();

			TestRoom roomA = new TestRoom { Id = 1, Core = core, Name = "A" };
			TestRoom roomB = new TestRoom { Id = 2, Core = core, Name = "B" };
			TestRoom roomC = new TestRoom { Id = 3, Core = core, Name = "C" };

			core.Originators.AddChild(roomA);
			core.Originators.AddChild(roomB);
			core.Originators.AddChild(roomC);

			roomA.Originators = new RoomOriginatorIdCollection(roomA);

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

			Room parent = new Room
			{
				Id = 2
			};

			Room a = new Room
			{
				Id = 3,
				CombinePriority = 5
			};

			Room b = new Room
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
			core.Originators.AddChild(partition);

			partition.AddRoom(a.Id);
			partition.AddRoom(b.Id);

			parent.Originators.Add(partition.Id, eCombineMode.Always);

			
			Assert.Inconclusive();
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
			ICore core = new Core();

			// Add the rooms
			TestRoom roomA = new TestRoom { Id = 1, Core = core, Name = "A" };
			roomA.Originators = new RoomOriginatorIdCollection(roomA);

			TestRoom roomB = new TestRoom { Id = 2, Core = core, Name = "B" };
			roomB.Originators = new RoomOriginatorIdCollection(roomB);

			TestRoom roomC = new TestRoom { Id = 3, Core = core, Name = "C" };
			roomC.Originators = new RoomOriginatorIdCollection(roomC);

			core.Originators.AddChild(roomA);
			core.Originators.AddChild(roomB);
			core.Originators.AddChild(roomC);

			// Add the partition
			Partition partition = new Partition { Id = 4 };
			partition.AddRoom(roomB.Id);
			partition.AddRoom(roomC.Id);
			core.Originators.AddChild(partition);

			// Room A contains B and C
			roomA.Originators.Add(partition.Id, eCombineMode.Always);

			// Add children to Room A
			TestRoom roomA1 = new TestRoom { Id = 5, Core = core, Name = "A1" };
			TestRoom roomA2 = new TestRoom { Id = 6, Core = core, Name = "A2" };
			TestRoom roomA3 = new TestRoom { Id = 7, Core = core, Name = "A3" };
			TestRoom roomA4 = new TestRoom { Id = 8, Core = core, Name = "A4" };
			TestRoom roomA5 = new TestRoom { Id = 9, Core = core, Name = "A5" };
			TestRoom roomA6 = new TestRoom { Id = 10, Core = core, Name = "A6" };

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
			TestRoom roomB1 = new TestRoom { Id = 11, Core = core, Name = "B1" };
			TestRoom roomB2 = new TestRoom { Id = 12, Core = core, Name = "B2" };
			TestRoom roomB3 = new TestRoom { Id = 13, Core = core, Name = "B3" };
			TestRoom roomB4 = new TestRoom { Id = 14, Core = core, Name = "B4" };
			TestRoom roomB5 = new TestRoom { Id = 15, Core = core, Name = "B5" };
			TestRoom roomB6 = new TestRoom { Id = 16, Core = core, Name = "B6" };

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
			TestRoom roomC1 = new TestRoom { Id = 17, Core = core, Name = "C1" };
			TestRoom roomC2 = new TestRoom { Id = 18, Core = core, Name = "C2" };
			TestRoom roomC3 = new TestRoom { Id = 19, Core = core, Name = "C3" };
			TestRoom roomC4 = new TestRoom { Id = 20, Core = core, Name = "C4" };
			TestRoom roomC5 = new TestRoom { Id = 21, Core = core, Name = "C5" };
			TestRoom roomC6 = new TestRoom { Id = 22, Core = core, Name = "C6" };

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
			IOriginator[] roomAContents = roomA.Originators.GetInstancesRecursive(eCombineMode.Always).ToArray();
			IOriginator[] roomBContents = roomB.Originators.GetInstancesRecursive(eCombineMode.Always).ToArray();
			IOriginator[] roomCContents = roomC.Originators.GetInstancesRecursive(eCombineMode.Always).ToArray();

			// Room A Contents
			Assert.IsFalse(roomAContents.Contains(roomA1));
			Assert.IsTrue(roomAContents.Contains(roomA2));
			Assert.IsTrue(roomAContents.Contains(roomA3));
			Assert.IsTrue(roomAContents.Contains(roomA4));
			Assert.IsTrue(roomAContents.Contains(roomA5));
			Assert.IsTrue(roomAContents.Contains(roomA6));

			Assert.IsFalse(roomAContents.Contains(roomB1));
			Assert.IsFalse(roomAContents.Contains(roomB2));
			Assert.IsFalse(roomAContents.Contains(roomB3));
			Assert.IsTrue(roomAContents.Contains(roomB4));
			Assert.IsTrue(roomAContents.Contains(roomB5));
			Assert.IsTrue(roomAContents.Contains(roomB6));

			Assert.IsFalse(roomAContents.Contains(roomC1));
			Assert.IsFalse(roomAContents.Contains(roomC2));
			Assert.IsTrue(roomAContents.Contains(roomC3));
			Assert.IsFalse(roomAContents.Contains(roomC4));
			Assert.IsTrue(roomAContents.Contains(roomC5));
			Assert.IsTrue(roomAContents.Contains(roomC6));

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
