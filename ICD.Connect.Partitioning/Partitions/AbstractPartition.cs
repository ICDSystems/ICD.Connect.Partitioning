using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Connect.Partitioning.Cells;
using ICD.Connect.Partitioning.Controls;
using ICD.Connect.Settings;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Partitioning.Partitions
{
	public abstract class AbstractPartition<TSettings> : AbstractOriginator<TSettings>, IPartition
		where TSettings : IPartitionSettings, new()
	{
		private readonly IcdHashSet<PartitionControlInfo> m_Controls;
		private readonly SafeCriticalSection m_Section;

		#region Properties

		/// <summary>
		/// Gets the category for this originator type (e.g. Device, Port, etc)
		/// </summary>
		public override string Category { get { return "Partition"; } }

		/// <summary>
		/// Gets/sets the the first cell adjacent to this partition.
		/// </summary>
		public ICell CellA { get; set; }

		/// <summary>
		/// Gets/sets the the second cell adjacent to this partition.
		/// </summary>
		public ICell CellB { get; set; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractPartition()
		{
			m_Controls = new IcdHashSet<PartitionControlInfo>();
			m_Section = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Gets the controls that are associated with this partition.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<PartitionControlInfo> GetPartitionControlInfos()
		{
			return m_Section.Execute(() => m_Controls.ToArray(m_Controls.Count));
		}

		/// <summary>
		/// Gets the controls that are associated with this partition.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<PartitionControlInfo> GetPartitionControlInfos(ePartitionFeedback mask)
		{
			return m_Section.Execute(() => m_Controls.Where(c => c.Mode.HasFlags(mask)).ToArray());
		}

		/// <summary>
		/// Sets the controls that are associated with this partition.
		/// </summary>
		/// <param name="partitionControls"></param>
		public void SetPartitionControls(IEnumerable<PartitionControlInfo> partitionControls)
		{
			m_Section.Enter();

			try
			{
				m_Controls.Clear();
				m_Controls.AddRange(partitionControls);
			}
			finally
			{
				m_Section.Leave();
			}
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			CellA = null;
			CellB = null;

			SetPartitionControls(Enumerable.Empty<PartitionControlInfo>());
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.CellAId = CellA == null ? (int?)null : CellA.Id;
			settings.CellBId = CellB == null ? (int?)null : CellB.Id;

			settings.SetPartitionControls(GetPartitionControlInfos());
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			// Load the partition controls
			factory.LoadOriginators(settings.GetPartitionControls().Select(c => c.Control.DeviceId));

			base.ApplySettingsFinal(settings, factory);

			SetPartitionControls(settings.GetPartitionControls());
			
			CellA = settings.CellAId == null ? null : factory.GetOriginatorById<ICell>(settings.CellAId.Value);
			CellB = settings.CellBId == null ? null : factory.GetOriginatorById<ICell>(settings.CellBId.Value);
		}

		#endregion
	}
}
