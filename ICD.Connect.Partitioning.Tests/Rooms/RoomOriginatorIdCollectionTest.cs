﻿using System.Linq;
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
			Assert.Inconclusive();
		}

		#region Methods

		[Test]
		public void ClearTest()
		{
			Assert.Inconclusive();
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
			Assert.Inconclusive();
		}

		[Test]
		public void AddTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void AddRangeTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void RemoveTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void ContainsTest()
		{
			Assert.Inconclusive();
		}

		#endregion

		#region Combine

		[Test]
		public void GetCombineModeTest()
		{
			Assert.Inconclusive();
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
