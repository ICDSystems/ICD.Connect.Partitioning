using System;
using System.Collections.Generic;
using ICD.Common.Logging.LoggingContexts;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Partitioning.Commercial.Controls.Occupancy
{
	public abstract class AbstractOccupancySensorControl<T> : AbstractDeviceControl<T>, IOccupancySensorControl
		where T : IDevice
	{
		private eOccupancyFeatures m_SupportedFeatures;
		private eOccupancyState m_OccupancyState;
		private int m_PeopleCount;

		#region events

		/// <summary>
		/// Raised when the supported features change
		/// </summary>
		public event EventHandler<GenericEventArgs<eOccupancyFeatures>> OnSupportedFeaturesChanged;

		/// <summary>
		/// Triggered when the occupancy state changes
		/// True = occupied
		/// False = unoccupied/vacant
		/// </summary>
		public event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupancyStateChanged;

		/// <summary>
		/// Raised when the number of people counted by the sensor changes
		/// </summary>
		public event EventHandler<IntEventArgs> OnPeopleCountChanged;

		#endregion

		#region properties

		/// <summary>
		/// Get what features are supported by this control
		/// </summary>
		public eOccupancyFeatures SupportedFeatures
		{
			get { return m_SupportedFeatures; } 
			protected set
			{
				if (m_SupportedFeatures == value)
					return;

				m_SupportedFeatures = value;

				OnSupportedFeaturesChanged.Raise(this, value);
			}
		}

		/// <summary>
		/// State of the occupancy sensor
		/// True = occupied
		/// False = unoccupied/vacant
		/// </summary>
		public eOccupancyState OccupancyState
		{
			get { return m_OccupancyState; }
			protected set
			{
				if (m_OccupancyState == value)
					return;

				m_OccupancyState = value;

				Logger.LogSetTo(eSeverity.Informational, "OccupancyState", m_OccupancyState);
				Activities.LogActivity(OccupancySensorControlActivities.GetOccupancyStateActivity(m_OccupancyState));

				OnOccupancyStateChanged.Raise(this, value);
			}
		}

		/// <summary>
		/// Number of people counted by this sensor
		/// </summary>
		public int PeopleCount
		{
			get { return m_PeopleCount; }
			protected set
			{
				if (m_PeopleCount == value)
					return;

				m_PeopleCount = value;

				Logger.LogSetTo(eSeverity.Informational, "PeopleCount", value);
				Activities.LogActivity(OccupancySensorControlActivities.GetPeopleCountActivity(value));

				OnPeopleCountChanged.Raise(this, value);
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractOccupancySensorControl(T parent, int id)
			: base(parent, id)
		{
		}

		protected void SetOccupancySupported(bool supported)
		{
			if (supported)
				SupportedFeatures = EnumUtils.IncludeFlags(SupportedFeatures, eOccupancyFeatures.Occupancy);
			else
				SupportedFeatures = EnumUtils.ExcludeFlags(SupportedFeatures, eOccupancyFeatures.Occupancy);
		}

		protected void SetPeopleCountSupported(bool supported)
		{
			if (supported)
				SupportedFeatures = EnumUtils.IncludeFlags(SupportedFeatures, eOccupancyFeatures.PeopleCounting);
			else
				SupportedFeatures = EnumUtils.ExcludeFlags(SupportedFeatures, eOccupancyFeatures.PeopleCounting);
		}

		#region Console

		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			foreach (IConsoleCommand command in OccupancySensorControlConsole.GetConsoleCommands(this))
				yield return command;
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase node in GetBaseConsoleNodes())
				yield return node;

			foreach (IConsoleNodeBase node in OccupancySensorControlConsole.GetConsoleNodes(this))
				yield return node;
		}

		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			OccupancySensorControlConsole.BuildConsoleStatus(this, addRow);
		}

		#endregion
	}
}
