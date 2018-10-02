using System;
using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Partitioning.PartitionManagers;
using NUnit.Framework;

namespace ICD.Connect.Partitioning.Tests.PartitionManagers
{
	[TestFixture]
	public sealed class RoomLayoutTest
	{
		#region Methods

		[Test]
		public void ClearTest()
		{
			RoomLayout layout = new RoomLayout(null);

			layout.SetRooms(new[] { new RoomLayoutInfo(1, 2, 3) });
			layout.Clear();

			Assert.AreEqual(0, layout.GetRooms().Count());
		}

		[Test]
		public void GetRoomsTest()
		{
			RoomLayout layout = new RoomLayout(null);

			RoomLayoutInfo[] info =
			{
				new RoomLayoutInfo(1, 2, 3),
				new RoomLayoutInfo(4, 5, 6),
				new RoomLayoutInfo(7, 8, 9),
				new RoomLayoutInfo(10, 11, 12)
			};

			layout.SetRooms(info);

			Assert.IsTrue(layout.GetRooms().ScrambledEquals(info));
		}

		[Test]
		public void SetRoomsTest()
		{
			RoomLayout layout = new RoomLayout(null);

			RoomLayoutInfo[] duplicates =
			{
				new RoomLayoutInfo(1, 2, 3),
				new RoomLayoutInfo(1, 2, 3),
				new RoomLayoutInfo(1, 2, 3)
			};

			Assert.Throws<ArgumentException>(() => layout.SetRooms(duplicates));

			RoomLayoutInfo[] unique =
			{
				new RoomLayoutInfo(1, 2, 3),
				new RoomLayoutInfo(4, 5, 6),
				new RoomLayoutInfo(7, 8, 9)
			};

			Assert.DoesNotThrow(() => layout.SetRooms(unique));
		}

		[Test]
		public void GetRoomTest()
		{
			RoomLayout layout = new RoomLayout(null);

			RoomLayoutInfo[] rooms =
			{
				new RoomLayoutInfo(1, 1, 1),
				new RoomLayoutInfo(2, 1, 2),

				new RoomLayoutInfo(1, 2, 3),
				new RoomLayoutInfo(2, 2, 4),

				new RoomLayoutInfo(1, 3, 5),
				new RoomLayoutInfo(2, 3, 6)
			};

			layout.SetRooms(rooms);

			Assert.AreEqual(3, layout.GetRoom(new RoomPositionInfo(1, 2)));
		}

		[Test]
		public void TryGetRoomTest()
		{
			RoomLayout layout = new RoomLayout(null);

			RoomLayoutInfo[] rooms =
			{
				new RoomLayoutInfo(1, 1, 1),
				new RoomLayoutInfo(2, 1, 2),

				new RoomLayoutInfo(1, 2, 3),
				new RoomLayoutInfo(2, 2, 4),

				new RoomLayoutInfo(1, 3, 5),
				new RoomLayoutInfo(2, 3, 6)
			};

			layout.SetRooms(rooms);

			int room;

			Assert.IsFalse(layout.TryGetRoom(new RoomPositionInfo(8, 9), out room));
			Assert.AreEqual(default(int), room);

			Assert.IsTrue(layout.TryGetRoom(new RoomPositionInfo(1, 2), out room));
			Assert.AreEqual(3, room);
		}

		[Test]
		public void GetRoomPositionTest()
		{
			RoomLayout layout = new RoomLayout(null);

			RoomLayoutInfo[] rooms =
			{
				new RoomLayoutInfo(1, 1, 1),
				new RoomLayoutInfo(2, 1, 2),

				new RoomLayoutInfo(1, 2, 3),
				new RoomLayoutInfo(2, 2, 4),

				new RoomLayoutInfo(1, 3, 5),
				new RoomLayoutInfo(2, 3, 6)
			};

			layout.SetRooms(rooms);

			Assert.AreEqual(new RoomPositionInfo(1, 2), layout.GetRoomPosition(3));
		}

		[Test]
		public void TryGetRoomPositionTest()
		{
			RoomLayout layout = new RoomLayout(null);

			RoomLayoutInfo[] rooms =
			{
				new RoomLayoutInfo(1, 1, 1),
				new RoomLayoutInfo(2, 1, 2),

				new RoomLayoutInfo(1, 2, 3),
				new RoomLayoutInfo(2, 2, 4),

				new RoomLayoutInfo(1, 3, 5),
				new RoomLayoutInfo(2, 3, 6)
			};

			layout.SetRooms(rooms);

			RoomPositionInfo position;

			Assert.IsFalse(layout.TryGetRoomPosition(7, out position));
			Assert.AreEqual(default(RoomPositionInfo), position);

			Assert.IsTrue(layout.TryGetRoomPosition(3, out position));
			Assert.AreEqual(new RoomPositionInfo(1, 2), position);
		}

		#endregion
	}
}
