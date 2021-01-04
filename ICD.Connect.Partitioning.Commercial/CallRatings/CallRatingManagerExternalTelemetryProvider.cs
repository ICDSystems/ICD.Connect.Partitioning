using System;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers.External;

namespace ICD.Connect.Partitioning.Commercial.CallRatings
{
	public sealed class CallRatingManagerExternalTelemetryProvider : AbstractExternalTelemetryProvider<CallRatingManager>
	{
		[EventTelemetry(CallRatingTelemetryNames.CALL_RATING_AVERAGE_CHANGED)]
		public event EventHandler<StringEventArgs> OnCallRatingAverageChanged; 

		private string m_CallRatingAverage;

		[PropertyTelemetry(CallRatingTelemetryNames.CALL_RATING_AVERAGE, null, CallRatingTelemetryNames.CALL_RATING_AVERAGE_CHANGED)]
		public string CallRatingAverage
		{
			get { return m_CallRatingAverage; }
			private set
			{
				if (value == m_CallRatingAverage)
					return;

				m_CallRatingAverage = value;

				OnCallRatingAverageChanged.Raise(this, m_CallRatingAverage);
			}
		}

		#region Methods

		protected override void SetParent(CallRatingManager parent)
		{
			base.SetParent(parent);

			UpdateCallRatingAverage();
		}

		private void UpdateCallRatingAverage()
		{
			CallRatingAverage =
				Parent == null
					? null
					: CallRating.AverageRating(Parent.Room.Id, 
					                           IcdEnvironment.GetUtcTime() - TimeSpan.FromDays(14), 
					                           IcdEnvironment.GetUtcTime())
					            .ToString("0.00");
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
			UpdateCallRatingAverage();
		}

		#endregion
	}
}
