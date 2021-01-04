using System;
using ICD.Common.Utils.Xml;

namespace ICD.Connect.Partitioning.Commercial.CallRatings
{
	public struct CallRatingCauseOption : IEquatable<CallRatingCauseOption>
	{
		private readonly string m_Cause;
		private readonly int m_Option;

		#region Properties

		/// <summary>
		/// Get's the plain text for the cause of the call rating.
		/// </summary>
		public string Cause { get { return m_Cause ?? ""; } }

		/// <summary>
		/// Int key associated with the cause.
		/// </summary>
		public int Option { get { return m_Option; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="cause"></param>
		/// <param name="option"></param>
		public CallRatingCauseOption(string cause, int option)
		{
			m_Cause = cause;
			m_Option = option;
		}

		/// <summary>
		/// Instantiates a CallRatingCauseOption from xml.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public static CallRatingCauseOption FromXml(string xml)
		{
			string cause = XmlUtils.GetAttribute(xml, "Cause");
			int option = XmlUtils.ReadElementContentAsInt(xml);

			return new CallRatingCauseOption(cause, option);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Implementing default equality.
		/// </summary>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <returns></returns>
		public static bool operator ==(CallRatingCauseOption c1, CallRatingCauseOption c2)
		{
			return c1.Equals(c2);
		}

		/// <summary>
		/// Implementing default inequality.
		/// </summary>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <returns></returns>
		public static bool operator !=(CallRatingCauseOption c1, CallRatingCauseOption c2)
		{
			return !c1.Equals(c2);
		}

		/// <summary>
		/// Equality.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(CallRatingCauseOption other)
		{
			return m_Cause == other.m_Cause &&
			       m_Option == other.m_Option;
		}

		/// <summary>
		/// Object equality.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			return other is CallRatingCauseOption && Equals(other);
		}

		/// <summary>
		/// Gets the hashcode for this instance.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + (m_Cause == null ? 0 : m_Cause.GetHashCode());
				hash = hash * 23 + m_Option.GetHashCode();
				return hash;
			}
		}

		#endregion
	}
}
