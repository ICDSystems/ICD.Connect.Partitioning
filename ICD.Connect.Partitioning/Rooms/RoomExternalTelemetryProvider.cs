using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Audio.Utils;
using ICD.Connect.Audio.VolumePoints;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers.External;

namespace ICD.Connect.Partitioning.Rooms
{
	public sealed class RoomExternalTelemetryProvider : AbstractExternalTelemetryProvider<IRoom>
	{
		[PublicAPI("DAV-PRO")]
		[EventTelemetry(RoomTelemetryNames.ORIGINATOR_IDS_CHANGED)]
		public event EventHandler OnOriginatorIdsChanged;

		[PublicAPI("DAV-PRO")]
		[EventTelemetry(RoomTelemetryNames.VOLUME_PERCENT_CHANGED)]
		public event EventHandler<FloatEventArgs> OnVolumePercentChanged;

		[EventTelemetry(RoomTelemetryNames.MUTED_CHANGED)]
		public event EventHandler<BoolEventArgs> OnMutedChanged; 

		private readonly IcdHashSet<Guid> m_OriginatorIds;
		private readonly SafeCriticalSection m_OriginatorIdsSection;
		private readonly VolumePointHelper m_VolumePointHelper;

		private float m_VolumePercent;
		private bool m_Muted;

		#region Properties

		[PublicAPI("DAV-PRO")]
		[PropertyTelemetry(RoomTelemetryNames.ORIGINATOR_IDS, null, RoomTelemetryNames.ORIGINATOR_IDS_CHANGED)]
		public IEnumerable<Guid> OriginatorIds { get { return m_OriginatorIdsSection.Execute(() => m_OriginatorIds.ToArray()); } }

		[PublicAPI("DAV-PRO")]
		[PropertyTelemetry(RoomTelemetryNames.VOLUME_PERCENT, null, RoomTelemetryNames.VOLUME_PERCENT_CHANGED)]
		public float VolumePercent
		{
			get { return m_VolumePercent; }
			private set
			{
				if (Math.Abs(value - m_VolumePercent) < 0.001f)
					return;

				m_VolumePercent = value;

				OnVolumePercentChanged.Raise(this, new FloatEventArgs(m_VolumePercent));
			}
		}

		[PropertyTelemetry(RoomTelemetryNames.MUTED, null, RoomTelemetryNames.MUTED_CHANGED)]
		public bool Muted
		{
			get { return m_Muted; }
			private set
			{
				if (value == m_Muted)
					return;

				m_Muted = value;

				OnMutedChanged.Raise(this, new BoolEventArgs(m_Muted));
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomExternalTelemetryProvider()
		{
			m_OriginatorIds = new IcdHashSet<Guid>();
			m_OriginatorIdsSection = new SafeCriticalSection();
			m_VolumePointHelper = new VolumePointHelper();

			m_VolumePointHelper.OnVolumeControlVolumeChanged += VolumePointHelperOnVolumeControlVolumeChanged;
			m_VolumePointHelper.OnVolumeControlIsMutedChanged += VolumePointHelperOnVolumeControlIsMutedChanged;
		}

		#region Methods

		/// <summary>
		/// Sets the parent telemetry provider that this instance extends.
		/// </summary>
		/// <param name="parent"></param>
		protected override void SetParent(IRoom parent)
		{
			base.SetParent(parent);

			UpdateOriginatorIds();
			UpdateVolumePoint();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Updates the OriginatorIds collection and raises the OnOriginatorIdsChanged event
		/// if the collection has changed.
		/// </summary>
		private void UpdateOriginatorIds()
		{
			IcdHashSet<Guid> originatorIds =
				Parent.Originators
				      .GetInstancesRecursive(eCombineMode.Always)
				      .Select(d => d.Uuid)
				      .ToIcdHashSet();

			m_OriginatorIdsSection.Enter();

			try
			{
				if (originatorIds.SetEquals(m_OriginatorIds))
					return;

				m_OriginatorIds.Clear();
				m_OriginatorIds.AddRange(originatorIds);
			}
			finally
			{
				m_OriginatorIdsSection.Leave();
			}

			OnOriginatorIdsChanged.Raise(this);
		}

		/// <summary>
		/// Called when the current room volume changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="floatEventArgs"></param>
		private void VolumePointHelperOnVolumeControlVolumeChanged(object sender, FloatEventArgs floatEventArgs)
		{
			VolumePercent = m_VolumePointHelper.GetVolumePercent();
		}

		/// <summary>
		/// Called when the current room mute state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="boolEventArgs"></param>
		private void VolumePointHelperOnVolumeControlIsMutedChanged(object sender, BoolEventArgs boolEventArgs)
		{
			Muted = m_VolumePointHelper.IsMuted;
		}

		/// <summary>
		/// Updates the volume point helper to wrap the current room volume control.
		/// </summary>
		private void UpdateVolumePoint()
		{
			m_VolumePointHelper.VolumePoint =
				Parent == null
					? null
					: Parent.GetContextualVolumePoints().FirstOrDefault();
		}

		#endregion

		#region Provider Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Subscribe(IRoom parent)
		{
			base.Subscribe(parent);

			if (parent == null)
				return;

			parent.Originators.OnChildrenChanged += OriginatorsOnChildrenChanged;
			parent.OnVolumeContextChanged += ParentOnVolumeContextChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Unsubscribe(IRoom parent)
		{
			base.Unsubscribe(parent);

			if (parent == null)
				return;

			parent.Originators.OnChildrenChanged -= OriginatorsOnChildrenChanged;
			parent.OnVolumeContextChanged -= ParentOnVolumeContextChanged;
		}

		/// <summary>
		/// Called when originators are added/removed to/from the room.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void OriginatorsOnChildrenChanged(object sender, EventArgs args)
		{
			UpdateOriginatorIds();
		}

		/// <summary>
		/// Called when the room volume context changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void ParentOnVolumeContextChanged(object sender, GenericEventArgs<eVolumePointContext> eventArgs)
		{
			UpdateVolumePoint();
		}

		#endregion
	}
}
