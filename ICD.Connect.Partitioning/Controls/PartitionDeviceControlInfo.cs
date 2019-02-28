using System;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Partitioning.Controls
{
	public struct PartitionDeviceControlInfo : IEquatable<PartitionDeviceControlInfo>, IComparable<PartitionDeviceControlInfo>
	{
		private const string MODE_ELEMENT = "Mode";

		private readonly ePartitionFeedback m_Mode;
		private readonly DeviceControlInfo m_Control;

		/// <summary>
		/// When the mask has "set" the program will open/close the partition when rooms are combined/uncombined.
		/// When the mask has "get" the program will combine/uncombine rooms when the partition opens/closes.
		/// </summary>
		public ePartitionFeedback Mode { get { return m_Mode; } }

		/// <summary>
		/// Gets the control info.
		/// </summary>
		public DeviceControlInfo Control { get { return m_Control; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="control"></param>
		public PartitionDeviceControlInfo(ePartitionFeedback mode, DeviceControlInfo control)
		{
			m_Mode = mode;
			m_Control = control;
		}

		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("Mode", Mode);
			builder.AppendProperty("DeviceId", Control.DeviceId);
			builder.AppendProperty("ControlId", Control.ControlId);

			return builder.ToString();
		}

		/// <summary>
		/// Writes the partition device control info to the xml writer.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="element"></param>
		public void WriteToXml(IcdXmlTextWriter writer, string element)
		{
			writer.WriteStartElement(element);
			{
				writer.WriteElementString(MODE_ELEMENT, IcdXmlConvert.ToString(Mode));
				writer.WriteElementString(DeviceControlInfo.DEVICE_ELEMENT, IcdXmlConvert.ToString(Control.DeviceId));
				writer.WriteElementString(DeviceControlInfo.CONTROL_ELEMENT, IcdXmlConvert.ToString(Control.ControlId));
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Deserializes the partition device control info from an xml element.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public static PartitionDeviceControlInfo ReadFromXml(string xml)
		{
			ePartitionFeedback mode =
				XmlUtils.TryReadChildElementContentAsEnum<ePartitionFeedback>(xml, MODE_ELEMENT, true) ?? ePartitionFeedback.None;

			// Hack - we have the same elements as a DeviceControlInfo
			DeviceControlInfo control = DeviceControlInfo.ReadFromXml(xml);

			return new PartitionDeviceControlInfo(mode, control);
		}

		#region Equality

		/// <summary>
		/// Implementing default equality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator ==(PartitionDeviceControlInfo a1, PartitionDeviceControlInfo a2)
		{
			return a1.Equals(a2);
		}

		/// <summary>
		/// Implementing default inequality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator !=(PartitionDeviceControlInfo a1, PartitionDeviceControlInfo a2)
		{
			return !a1.Equals(a2);
		}

		public int CompareTo(PartitionDeviceControlInfo other)
		{
			int result = m_Control.CompareTo(other.m_Control);
			if (result != 0)
				return result;

// ReSharper disable ImpureMethodCallOnReadonlyValueField
			return m_Mode.CompareTo(other.m_Mode);
// ReSharper restore ImpureMethodCallOnReadonlyValueField
		}

		public bool Equals(PartitionDeviceControlInfo other)
		{
			return m_Mode == other.m_Mode &&
				   m_Control == other.m_Control;
		}

		/// <summary>
		/// Returns true if this instance is equal to the given object.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			return other is PartitionDeviceControlInfo && Equals((PartitionDeviceControlInfo)other);
		}

		/// <summary>
		/// Gets the hashcode for this instance.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + (int)m_Mode;
				hash = hash * 23 + m_Control.GetHashCode();
				return hash;
			}
		}

		#endregion
	}
}
