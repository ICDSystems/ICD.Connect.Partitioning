﻿using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Services;
using ICD.Connect.Partitioning.Rooms;
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
		public void GetInstanceIdTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetInstanceIdGenericTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetInstanceSelectorTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetInstanceGenericTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetInstancesTest()
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
			Assert.Inconclusive();
		}

		[Test]
		public void GetIdsRecursiveTest()
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
			Assert.Inconclusive();
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
