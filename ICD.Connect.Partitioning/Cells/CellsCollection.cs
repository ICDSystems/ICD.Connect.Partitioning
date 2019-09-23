using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Partitioning.PartitionManagers;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Partitioning.Cells
{
	public sealed class CellsCollection : AbstractOriginatorCollection<ICell>, ICellsCollection
	{
		private readonly Dictionary<CellColumnRowInfo, List<ICell>> m_GridCells;
		private readonly SafeCriticalSection m_Section;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="partitionManager"></param>
		public CellsCollection(PartitionManager partitionManager)
		{
			m_GridCells = new Dictionary<CellColumnRowInfo, List<ICell>>();
			m_Section = new SafeCriticalSection();
		}

		/// <summary>
		/// Gets the first cell occupying the given row and column.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="row"></param>
		/// <returns></returns>
		[CanBeNull]
		public ICell GetCell(int column, int row)
		{
			CellColumnRowInfo info = new CellColumnRowInfo(column, row);

			m_Section.Enter();

			try
			{
				List<ICell> cells;
				return m_GridCells.TryGetValue(info, out cells) ? cells.FirstOrDefault() : null;
			}
			finally
			{
				m_Section.Leave();
			}
		}

		public void RebuildCache()
		{
			m_Section.Enter();

			try
			{
				m_GridCells.Clear();

				foreach (ICell cell in GetChildren())
				{
					CellColumnRowInfo info = new CellColumnRowInfo(cell.Column, cell.Row);

					List<ICell> cells;
					if (!m_GridCells.TryGetValue(info, out cells))
					{
						cells = new List<ICell>();
						m_GridCells.Add(info, cells);
					}

					cells.InsertSorted(cell, c => info.CompareTo(new CellColumnRowInfo(c.Column, c.Row)));
				}
			}
			finally
			{
				m_Section.Leave();
			}
		}
	}
}
