using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Partitioning.Commercial.Comparers;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers.External;

namespace ICD.Connect.Partitioning.Commercial.CallRatings
{
	public sealed class CallRatingManagerExternalTelemetryProvider : AbstractExternalTelemetryProvider<CallRatingManager>
	{
		#region Events

		[EventTelemetry(CallRatingTelemetryNames.CALL_RATING_AVERAGE_CHANGED)]
		public event EventHandler<FloatEventArgs> OnCallRatingAverageChanged;

		[EventTelemetry(CallRatingTelemetryNames.RECENT_CALL_RATINGS_CHANGED)]
		public event EventHandler<GenericEventArgs<IEnumerable<CallRating>>> OnRecentCallRatingsChanged;

		#endregion

		private float m_CallRatingAverage;

		private List<CallRating> m_RecentCallRatings;

		#region Properties

		[PropertyTelemetry(CallRatingTelemetryNames.RECENT_CALL_RATINGS, null,
		                   CallRatingTelemetryNames.RECENT_CALL_RATINGS_CHANGED)]
		public IEnumerable<CallRating> RecentCallRatings
		{
			get { return m_RecentCallRatings; }
			private set
			{
				if (value.SequenceEqual(m_RecentCallRatings, CallRatingComparer.Instance))
					return;

				m_RecentCallRatings = value.ToList();

				OnRecentCallRatingsChanged.Raise(this, m_RecentCallRatings);
			}
		}

		private float CallRatingAverage
		{
			get { return m_CallRatingAverage; }
			set
			{
				if (Math.Abs(value - m_CallRatingAverage) < 0.01f)
					return;

				m_CallRatingAverage = value;

				OnCallRatingAverageChanged.Raise(this, m_CallRatingAverage);
			}
		}

		[PropertyTelemetry(CallRatingTelemetryNames.CALL_RATING_AVERAGE_STRING, null,
		                   CallRatingTelemetryNames.CALL_RATING_AVERAGE_CHANGED)]
		public string CallRatingAverageString { get { return CallRatingAverage.ToString("0.00"); } }

		#endregion

		public CallRatingManagerExternalTelemetryProvider()
		{
			m_RecentCallRatings = new List<CallRating>();
		}

		#region Methods

		protected override void SetParent(CallRatingManager parent)
		{
			base.SetParent(parent);

			UpdateRecentCallRatings();
			UpdateCallRatingAverage();
		}

		private void UpdateCallRatingAverage()
		{
			CallRatingAverage =
				Parent == null
					? 0
					: CallRating.AverageRating(Parent.Room.Id, 
					                           IcdEnvironment.GetUtcTime() - TimeSpan.FromDays(14), 
					                           IcdEnvironment.GetUtcTime());
		}

		private void UpdateRecentCallRatings()
		{
			RecentCallRatings =
				Parent == null
					? Enumerable.Empty<CallRating>()
					: CallRating.GetCallRatingsInDateRange(Parent.Room.Id,
					                                IcdEnvironment.GetUtcTime() - TimeSpan.FromHours(24),
					                                IcdEnvironment.GetUtcTime()).ToList();
		}

		#endregion

		#region Parent Callbacks

		protected override void Subscribe(CallRatingManager parent)
		{
			base.Subscribe(parent);

			if (parent == null)
				return;

			parent.OnCallRating += ParentOnCallRating;
		}

		protected override void Unsubscribe(CallRatingManager parent)
		{
			base.Unsubscribe(parent);

			if (parent == null)
				return;

			parent.OnCallRating -= ParentOnCallRating;
		}

		private void ParentOnCallRating(object sender, GenericEventArgs<eCallRating> e)
		{
			UpdateRecentCallRatings();
			UpdateCallRatingAverage();
		}

		#endregion
	}
}
