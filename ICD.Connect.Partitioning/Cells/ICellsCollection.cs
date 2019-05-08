using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Partitioning.Cells
{
	public interface ICellsCollection : IOriginatorCollection<ICell>
	{
		/// <summary>
		/// Gets the first cell occupying the given row and column.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="row"></param>
		/// <returns></returns>
		[CanBeNull]
		ICell GetCell(int column, int row);
	}

	public static class CellsCollectionExtensions
	{
		

		public static ICell GetNeighboringCell(this ICellsCollection extends, int column, int row, eCellDirection direction)
		{
			// get coordinates of neighbor cell based on direction
			int neighborColumn = column;
			int neighborRow = row;
			switch(direction)
			{
				case eCellDirection.Left:
					neighborColumn -= 1;
					break;
				case eCellDirection.Right:
					neighborColumn += 1;
					break;
				case eCellDirection.Top:
					neighborRow -= 1;
					break;
				case eCellDirection.Bottom:
					neighborRow += 1;
					break;
				default:
					throw new ArgumentOutOfRangeException("direction");
			}

			// get neighbor cell if it exists
			return extends.GetCell(neighborColumn, neighborRow);
		}
	}
}