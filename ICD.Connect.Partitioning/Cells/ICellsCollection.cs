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
}