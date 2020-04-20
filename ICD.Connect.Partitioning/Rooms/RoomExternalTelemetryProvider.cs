using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry;

namespace ICD.Connect.Partitioning.Rooms
{
	public sealed class RoomExternalTelemetryProvider : AbstractExternalTelemetryProvider<IRoom>,
	                                                    IRoomExternalTelemetryProvider
	{
		public event EventHandler OnOriginatorIdsChanged;

		private readonly IcdHashSet<Guid> m_OriginatorIds;
		private readonly SafeCriticalSection m_OriginatorIdsSection;

		#region Properties

		public IEnumerable<Guid> OriginatorIds { get { return m_OriginatorIdsSection.Execute(() => m_OriginatorIds.ToArray()); } }
		
		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomExternalTelemetryProvider()
		{
			m_OriginatorIds = new IcdHashSet<Guid>();
			m_OriginatorIdsSection = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Sets the parent telemetry provider that this instance extends.
		/// </summary>
		/// <param name="parent"></param>
		public override void SetParent(IRoom parent)
		{
			base.SetParent(parent);

			UpdateOriginatorIds();
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
				      .Select(d => d.NewId)
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

		#endregion

		#region Parent Callbacks

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

		#endregion
	}
}
