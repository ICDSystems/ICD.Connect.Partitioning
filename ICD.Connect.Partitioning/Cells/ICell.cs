using ICD.Connect.Partitioning.Partitions;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Partitioning.Cells
{
	public interface ICell : IOriginator
	{
		/// <summary>
		/// Gets/sets the room occupying this cell.
		/// </summary>
		IRoom Room { get; set; }

		/// <summary>
		/// Gets the horizontal position of the cell in the grid.
		/// </summary>
		int Column { get; set; }

		/// <summary>
		/// Gets the vertical position of the cell in the grid.
		/// </summary>
		int Row { get; set; }
	}
}
