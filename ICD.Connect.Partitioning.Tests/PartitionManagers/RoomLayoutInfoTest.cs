using ICD.Connect.Partitioning.PartitionManagers;
using NUnit.Framework;

namespace ICD.Connect.Partitioning.Tests.PartitionManagers
{
	[TestFixture]
	public sealed class RoomPositionInfoTest
	{
		[TestCase(1)]
		public void ColumnTest(int column)
		{
			RoomPositionInfo info = new RoomPositionInfo(column, 0);
			Assert.AreEqual(column, info.Column);
		}

		[TestCase(1)]
		public void RowTest(int row)
		{
			RoomPositionInfo info = new RoomPositionInfo(0, row);
			Assert.AreEqual(row, info.Row);
		}
	}

	[TestFixture]
	public sealed class RoomLayoutInfoTest
	{
		[TestCase(1, 3)]
		public void PositionTest(int column, int row)
		{
			RoomPositionInfo position = new RoomPositionInfo(column, row);
			RoomLayoutInfo info = new RoomLayoutInfo(position, 0);

			Assert.AreEqual(position, info.Position);
		}

		[TestCase(1)]
		public void RoomIdTest(int roomId)
		{
			RoomLayoutInfo info = new RoomLayoutInfo(0, 0, roomId);
			Assert.AreEqual(roomId, info.RoomId);
		}
	}
}
