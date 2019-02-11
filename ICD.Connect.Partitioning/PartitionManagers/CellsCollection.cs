using ICD.Connect.Partitioning.Cells;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Partitioning.PartitionManagers
{
	public sealed class CellsCollection : AbstractOriginatorCollection<ICell>, ICellsCollection
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="partitionManager"></param>
		public CellsCollection(PartitionManager partitionManager)
		{
		}
	}
}
