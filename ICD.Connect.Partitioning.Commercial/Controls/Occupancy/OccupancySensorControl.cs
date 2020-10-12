using ICD.Common.Utils.EventArguments;
using ICD.Connect.Partitioning.Commercial.Devices.Occupancy;

namespace ICD.Connect.Partitioning.Commercial.Controls.Occupancy
{
	public sealed class OccupancySensorControl : AbstractOccupancySensorControl<IOccupancySensorDevice>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public OccupancySensorControl(IOccupancySensorDevice parent, int id)
			: base(parent, id)
		{
			parent.OnOccupancyStateChanged += ParentOnOccupancyStateChanged;
		}

		private void ParentOnOccupancyStateChanged(object sender, GenericEventArgs<eOccupancyState> args)
		{
			OccupancyState = args.Data;
		}
	}
}