using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;
using ICD.Connect.Partitioning.Commercial.OccupancyPoints;
using ICD.Connect.Partitioning.Commercial.Rooms;

namespace ICD.Connect.Partitioning.Commercial.OccupancyManagers
{
	public sealed class OccupancyManager : IOccupancyManager, IDisposable
	{
		#region Events

		/// <summary>
		/// Raised when the supported features change
		/// </summary>
		public event EventHandler<GenericEventArgs<eOccupancyFeatures>> OnSupportedFeaturesChanged;

		/// <summary>
		/// Triggered when the rooms occupancy state changes
		/// </summary>
		public event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupancyStateChanged;

		/// <summary>
		/// Raised when the number of people counted in the room changes
		/// </summary>
		public event EventHandler<IntEventArgs> OnPeopleCountChanged;

		#endregion

		#region Fields

		private readonly ICommercialRoom m_Room;
		private readonly IcdHashSet<IOccupancySensorControl> m_OccupancyControls;
		private readonly SafeCriticalSection m_OccupancyControlsSection;

		private eOccupancyFeatures m_SupportedFeatures;
		private eOccupancyState m_OccupancyState;
		private int m_PeopleCount;

		#endregion

		#region Properties

		public ICommercialRoom Room { get { return m_Room; } }

		/// <summary>
		/// Get what features are supported by controls in the room
		/// </summary>
		public eOccupancyFeatures SupportedFeatures {
			get { return m_SupportedFeatures; }
			private set
			{
				if (m_SupportedFeatures == value)
					return;

				m_SupportedFeatures = value;

				OnSupportedFeaturesChanged.Raise(this, value);
			}
		}

		/// <summary>
		/// Occupancy state of the room
		/// </summary>
		public eOccupancyState OccupancyState
		{
			get { return m_OccupancyState; }
			private set
			{
				if (m_OccupancyState == value)
					return;

				m_OccupancyState = value;

				OccupancyChangeTime = DateTime.UtcNow;

				OnOccupancyStateChanged.Raise(this, value);
			}
		}

		/// <summary>
		/// Number of people counted in the room
		/// </summary>
		public int PeopleCount
		{
			get { return m_PeopleCount; }
			private set
			{
				if (m_PeopleCount == value)
					return;

				m_PeopleCount = value;

				OnPeopleCountChanged.Raise(this, value);
			}
		}

		/// <summary>
		/// The UTC time of the last occupancy state change
		/// </summary>
		public DateTime OccupancyChangeTime { get; private set; }

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="room"></param>
		public OccupancyManager([NotNull] ICommercialRoom room)
		{
			if (room == null)
				throw new ArgumentNullException("room");
			m_OccupancyControls = new IcdHashSet<IOccupancySensorControl>();
			m_OccupancyControlsSection = new SafeCriticalSection();

			m_Room = room;
			Subscribe(room);

		}

		#region Methods

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			ClearOccupancyControls();
			Unsubscribe(m_Room);
		}

		private void Update()
		{
			UpdateSupportedFeatures();
			UpdateOccupancy();
			UpdatePeopleCount();
		}

		private void UpdateSupportedFeatures()
		{
			eOccupancyFeatures features = eOccupancyFeatures.None;
			foreach (var point in Room.Originators.GetInstancesRecursive<IOccupancyPoint>())
				features |= point.GetMaskedOccupancyFeatures();

			SupportedFeatures = features;
		}

		private void UpdateOccupancy()
		{
			OccupancyState = Room.Originators.GetInstancesRecursive<IOccupancyPoint>()
			                     .Where(p => p.GetMaskedOccupancyFeatures().HasFlag(eOccupancyFeatures.Occupancy)) 
			                     .Select(p => p.Control)
			                     .Where(c => c != null)
			                     .Select(c => c.OccupancyState)
			                     .MaxOrDefault();
		}

		private void UpdatePeopleCount()
		{
			PeopleCount = Room.Originators.GetInstancesRecursive<IOccupancyPoint>()
			                  .Where (p => p.GetMaskedOccupancyFeatures().HasFlag(eOccupancyFeatures.PeopleCounting))
			                  .Select(p => p.Control)
			                  .Where(c => c != null)
			                  .Select(c => c.PeopleCount)
			                  .MaxOrDefault();
		}

		#endregion

		#region Occupancy Control Callbacks

		private void SubscribeOccupancyControls()
		{
			m_OccupancyControlsSection.Enter();

			try
			{
				ClearOccupancyControls();

				IEnumerable<IOccupancySensorControl> controls =
					Room.Originators.GetInstancesRecursive<IOccupancyPoint>()
					           .Select(p => p.Control)
					           .Where(c => c != null);

				m_OccupancyControls.AddRange(controls);

				foreach (IOccupancySensorControl control in m_OccupancyControls)
					Subscribe(control);
			}
			finally
			{
				m_OccupancyControlsSection.Leave();
			}

			Update();
		}

		private void ClearOccupancyControls()
		{
			m_OccupancyControlsSection.Enter();

			try
			{
				foreach (IOccupancySensorControl control in m_OccupancyControls)
					Unsubscribe(control);

				m_OccupancyControls.Clear();
			}
			finally
			{
				m_OccupancyControlsSection.Leave();
			}
		}

		private void Subscribe(IOccupancySensorControl control)
		{
			if (control == null)
				return;

			control.OnSupportedFeaturesChanged += ControlOnOnSupportedFeaturesChanged;
			control.OnOccupancyStateChanged += ControlOnOccupancyStateChanged;
			control.OnPeopleCountChanged += ControlOnOnPeopleCountChanged;
		}

		private void Unsubscribe(IOccupancySensorControl control)
		{
			if (control == null)
				return;

			control.OnSupportedFeaturesChanged -= ControlOnOnSupportedFeaturesChanged;
			control.OnOccupancyStateChanged -= ControlOnOccupancyStateChanged;
			control.OnPeopleCountChanged -= ControlOnOnPeopleCountChanged;
		}

		private void ControlOnOnSupportedFeaturesChanged(object sender, GenericEventArgs<eOccupancyFeatures> args)
		{
			UpdateSupportedFeatures();
		}

		private void ControlOnOccupancyStateChanged(object sender, GenericEventArgs<eOccupancyState> args)
		{
			UpdateOccupancy();
		}

		private void ControlOnOnPeopleCountChanged(object sender, IntEventArgs args)
		{
			UpdatePeopleCount();
		}

		#endregion

		#region Room Callbacks

		private void Subscribe([NotNull] ICommercialRoom room)
		{
			room.Originators.OnCollectionChanged += OriginatorsOnOnCollectionChanged;
		}

		private void Unsubscribe([NotNull] ICommercialRoom room)
		{
			room.Originators.OnCollectionChanged -= OriginatorsOnOnCollectionChanged;
		}

		private void OriginatorsOnOnCollectionChanged(object sender, EventArgs e)
		{
			SubscribeOccupancyControls();
		}

		#endregion
	}
}
