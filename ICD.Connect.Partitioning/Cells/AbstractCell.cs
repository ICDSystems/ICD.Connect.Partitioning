using System.Collections.Generic;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Partitioning.Cells
{
	public abstract class AbstractCell<TSettings> : AbstractOriginator<TSettings>, ICell
		where TSettings : ICellSettings, new()
	{
		#region Properties

		/// <summary>
		/// Gets the category for this originator type (e.g. Device, Port, etc)
		/// </summary>
		public override string Category { get { return "Cell"; } }

		/// <summary>
		/// Gets/sets the room occupying this cell.
		/// </summary>
		public IRoom Room { get; set; }

		/// <summary>
		/// Gets the horizontal position of the cell in the grid.
		/// </summary>
		public int Column { get; set; }

		/// <summary>
		/// Gets the vertical position of the cell in the grid.
		/// </summary>
		public int Row { get; set; }

		#endregion

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			Room = null;
			Column = 0;
			Row = 0;
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Room = Room == null ? 0 : Room.Id;
			settings.Column = Column;
			settings.Row = Row;
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			Column = settings.Column;
			Row = settings.Row;

			if (!settings.Room.HasValue)
				return;

			try
			{
				Room = factory.GetOriginatorById<IRoom>(settings.Room.Value);
			}
			catch (KeyNotFoundException)
			{
				Logger.Log(eSeverity.Error, "No room with id {0}", settings.Room);
			}
		}

		#endregion
	}
}
