using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Xml;
using ICD.Connect.Partitioning.Cells;
using ICD.Connect.Partitioning.Controls;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Partitioning.Partitions
{
	public abstract class AbstractPartitionSettings : AbstractSettings, IPartitionSettings
	{
		private const string ELEMENT_CELL_A = "CellA";
		private const string ELEMENT_CELL_B = "CellB";

		private const string ELEMENT_PARTITION_CONTROLS = "PartitionControls";
		private const string ELEMENT_PARTITION_CONTROL = "PartitionControl";

		private readonly IcdHashSet<PartitionControlInfo> m_Controls;
		private readonly SafeCriticalSection m_Section;

		/// <summary>
		/// Gets/sets the id for the first cell adjacent to this partition.
		/// </summary>
		[OriginatorIdSettingsProperty(typeof(ICell))]
		public int? CellAId { get; set; }

		/// <summary>
		/// Gets/sets the id for the second cell adjacent to this partition.
		/// </summary>
		[OriginatorIdSettingsProperty(typeof(ICell))]
		public int? CellBId { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractPartitionSettings()
		{
			m_Controls = new IcdHashSet<PartitionControlInfo>();
			m_Section = new SafeCriticalSection();
		}

		/// <summary>
		/// Sets the controls associated with this partition.
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

		/// <summary>
		/// Returns the controls that are associated with thr
		/// </summary>
		/// <returns></returns>
		public IEnumerable<PartitionControlInfo> GetPartitionControls()
		{
			return m_Section.Execute(() => m_Controls.ToArray());
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(ELEMENT_CELL_A, IcdXmlConvert.ToString(CellAId));
			writer.WriteElementString(ELEMENT_CELL_B, IcdXmlConvert.ToString(CellBId));

			XmlUtils.WriteListToXml(writer, GetPartitionControls(), ELEMENT_PARTITION_CONTROLS,
			                        (w, d) => d.WriteToXml(w, ELEMENT_PARTITION_CONTROL));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			CellAId = XmlUtils.TryReadChildElementContentAsInt(xml, ELEMENT_CELL_A);
			CellBId = XmlUtils.TryReadChildElementContentAsInt(xml, ELEMENT_CELL_B);

			IEnumerable<PartitionControlInfo> partitionControls =
				XmlUtils.ReadListFromXml(xml, ELEMENT_PARTITION_CONTROLS, ELEMENT_PARTITION_CONTROL,
										 e => PartitionControlInfo.ReadFromXml(e));

			SetPartitionControls(partitionControls);
		}
	}
}
