using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Partitioning.Cells
{
	public interface ICellSettings : ISettings
	{
		/// <summary>
		/// Gets/sets the id for the room occupying this cell.
		/// </summary>
		[OriginatorIdSettingsProperty(typeof(IRoom))]
		int? Room { get; set; }

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
