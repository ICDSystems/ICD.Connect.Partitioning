using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.Commercial
{
	public sealed class TouchFree
	{
		public event EventHandler<IntEventArgs> OnCountDownSecondsChanged;
		public event EventHandler<BoolEventArgs> OnEnabledChanged;
		public event EventHandler<SourceEventArgs> OnDefaultSourceChanged; 

		private int m_CountDownSeconds ;
		private bool m_Enabled;
		private ISource m_Source;

		#region Properties

		public int CountdownSeconds
		{
			get {return m_CountDownSeconds;}
			set
			{
				if (value == m_CountDownSeconds)
					return;

				m_CountDownSeconds = value;

				OnCountDownSecondsChanged.Raise(this, new IntEventArgs(m_CountDownSeconds));
			}
		}

		public bool Enabled
		{
			get {return m_Enabled;}
			set
			{
				if (value == m_Enabled)
					return;

				m_Enabled = value;

				OnEnabledChanged.Raise(this, new BoolEventArgs(m_Enabled));
			}
		}

		[CanBeNull]
		public ISource Source
		{
			get { return m_Source; }
			set
			{
				if (value == m_Source)
					return;

				m_Source = value;

				OnDefaultSourceChanged.Raise(this, new SourceEventArgs(m_Source));
			}
		}

		#endregion

		#region Settings

		public void ClearSettings()
		{
			CountdownSeconds = 0;
			Enabled = false;
			Source = null;
		}

		/// <summary>
		/// Copies the properties onto the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		public void CopySettings(TouchFreeSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			settings.CountdownSeconds = CountdownSeconds;
			settings.Enabled = Enabled;

			settings.SourceId = Source == null ? (int?)null : Source.Id;
		}

		public void ApplySettings(TouchFreeSettings settings, IDeviceFactory factory)
		{
			CountdownSeconds = settings.CountdownSeconds;
			Enabled = settings.Enabled;

			try
			{
				Source = settings.SourceId == null ? null : factory.GetOriginatorById<ISource>(settings.SourceId.Value);
			}
			catch (KeyNotFoundException)
			{
				ServiceProvider.GetService<ILoggerService>().AddEntry(eSeverity.Error, "{0} - No Source with ID {1}", GetType().Name, settings.SourceId);
			}
		}

		#endregion
	}
}