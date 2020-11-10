using System;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.CrestronSPlus.SPlusShims;
using ICD.Connect.Partitioning.CrestronSPlus.Devices;

namespace ICD.Connect.Partitioning.CrestronSPlus.Shims
{
	public sealed class SPlusPartitionSensorShim : AbstractSPlusDeviceShim<SPlusPartitionSensorDevice>
	{

		#region SPlus

		[PublicAPI("S+")]
		public event EventHandler OnOpenStatusChanged;

		[PublicAPI("S+")]
		public ushort IsOpen
		{
			get { return (ushort)(Originator != null && Originator.IsOpen ? 1 : 0); }
		}
		
		[PublicAPI("S+")]
		public void SetPartitionState(ushort state)
		{
			if (Originator != null)
				Originator.IsOpen = state != 0;
		}

		#endregion

		#region Originator

		/// <summary>
		/// Subscribes to the originator events.
		/// </summary>
		/// <param name="originator"></param>
		protected override void Subscribe(SPlusPartitionSensorDevice originator)
		{
			base.Subscribe(originator);

			if (originator == null)
				return;

			originator.OnOpenStatusChanged += OriginatorOnOpenStatusChanged;
		}

		/// <summary>
		/// Unsubscribes from the originator events.
		/// </summary>
		/// <param name="originator"></param>
		protected override void Unsubscribe(SPlusPartitionSensorDevice originator)
		{
			base.Unsubscribe(originator);

			if (originator == null)
				return;

			originator.OnOpenStatusChanged -= OriginatorOnOpenStatusChanged;
		}

		private void OriginatorOnOpenStatusChanged(object sender, BoolEventArgs args)
		{
			OnOpenStatusChanged.Raise(this);
		}

		/// <summary>
		/// Called when the originator is attached.
		/// Do any actions needed to syncronize
		/// </summary>
		protected override void InitializeOriginator()
		{
			base.InitializeOriginator();

			OnOpenStatusChanged.Raise(this);
		}

		#endregion
	}
}