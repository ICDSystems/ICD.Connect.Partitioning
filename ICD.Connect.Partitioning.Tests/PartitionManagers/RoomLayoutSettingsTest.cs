using System.Linq;
using System.Text;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Xml;
using ICD.Connect.Partitioning.PartitionManagers;
using NUnit.Framework;

namespace ICD.Connect.Partitioning.Tests.PartitionManagers
{
	[TestFixture]
	public sealed class RoomLayoutSettingsTest
	{
		[Test]
		public void ClearTest()
		{
			RoomLayoutSettings settings = new RoomLayoutSettings();

			settings.SetRooms(new[] {new RoomLayoutInfo(1, 2, 3)});
			settings.Clear();

			Assert.AreEqual(0, settings.GetRooms().Count());
		}

		[Test]
		public void GetRoomsTest()
		{
			RoomLayoutSettings settings = new RoomLayoutSettings();

			RoomLayoutInfo[] info =
			{
				new RoomLayoutInfo(1, 2, 3),
				new RoomLayoutInfo(4, 5, 6),
				new RoomLayoutInfo(7, 8, 9),
				new RoomLayoutInfo(10, 11, 12)
			};

			settings.SetRooms(info);

			Assert.IsTrue(settings.GetRooms().ScrambledEquals(info));
		}

		[Test]
		public void SetRoomsTest()
		{
			RoomLayoutSettings settings = new RoomLayoutSettings();

			RoomLayoutInfo[] info =
			{
				new RoomLayoutInfo(1, 2, 3),
				new RoomLayoutInfo(1, 2, 3),
				new RoomLayoutInfo(1, 2, 3)
			};

			settings.SetRooms(info);

			Assert.AreEqual(1, settings.GetRooms().Count());
		}

		[Test]
		public void ToXmlTest()
		{
			RoomLayoutInfo[] layout =
			{
				new RoomLayoutInfo(1, 1, 1),
				new RoomLayoutInfo(2, 1, 2),

				new RoomLayoutInfo(1, 2, 3),
				new RoomLayoutInfo(2, 2, 4),

				new RoomLayoutInfo(1, 3, 5),
				new RoomLayoutInfo(2, 3, 6)
			};

			RoomLayoutSettings settings = new RoomLayoutSettings();
			settings.SetRooms(layout);

			StringBuilder builder = new StringBuilder();

			using (IcdStringWriter stringWriter = new IcdStringWriter(builder))
				using (IcdXmlTextWriter writer = new IcdXmlTextWriter(stringWriter))
					settings.ToXml(writer, "Layout");

			string xml = builder.ToString();

			// Pulled from debug
			const string expected =
				"<Layout>\r\n  <Rows>\r\n    <Row index=\"1\">\r\n      <Room index=\"1\">1</Room>\r\n      <Room index=\"2\">2</Room>\r\n    </Row>\r\n    <Row index=\"2\">\r\n      <Room index=\"1\">3</Room>\r\n      <Room index=\"2\">4</Room>\r\n    </Row>\r\n    <Row index=\"3\">\r\n      <Room index=\"1\">5</Room>\r\n      <Room index=\"2\">6</Room>\r\n    </Row>\r\n  </Rows>\r\n</Layout>";

			Assert.AreEqual(expected, xml);
		}

		[Test]
		public void ParseXmlTest()
		{
			const string xml = @"<Layout>
  <Rows>
    <Row index=""1"">
      <Room index=""1"">1</Room>
      <Room index=""2"">2</Room>
    </Row>
    <Row index=""2"">
      <Room index=""1"">3</Room>
      <Room index=""2"">4</Room>
    </Row>
    <Row index=""3"">
      <Room index=""1"">5</Room>
      <Room index=""2"">6</Room>
    </Row>
  </Rows>
<Layout>";

			RoomLayoutInfo[] expected =
			{
				new RoomLayoutInfo(1, 1, 1),
				new RoomLayoutInfo(2, 1, 2),

				new RoomLayoutInfo(1, 2, 3),
				new RoomLayoutInfo(2, 2, 4),

				new RoomLayoutInfo(1, 3, 5),
				new RoomLayoutInfo(2, 3, 6)
			};

			RoomLayoutSettings settings = new RoomLayoutSettings();
			settings.ParseXml(xml);

			Assert.IsTrue(settings.GetRooms().ScrambledEquals(expected));
		}
	}
}
