using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Xml;

namespace ICD.Connect.Partitioning.Commercial.CallRatings
{
	public sealed class XmlCallRatingCauseOptions : ICallRatingCauseOptions
	{
		private readonly List<CallRatingCauseOption> m_Options;
		private readonly SafeCriticalSection m_OptionsSection;

		#region Properties

		public int Count { get { return m_Options.Count; } }
		public bool IsReadOnly { get { return false; } }
		public CallRatingCauseOption this[int index]
		{
			get { return m_OptionsSection.Execute(() => m_Options[index]); }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public XmlCallRatingCauseOptions()
		{
			m_Options = new List<CallRatingCauseOption>();
			m_OptionsSection = new SafeCriticalSection();
		}

		public static XmlCallRatingCauseOptions FromXml(string xml)
		{
			XmlCallRatingCauseOptions output = new XmlCallRatingCauseOptions();
			output.Parse(xml);
			return output;
		}

		#endregion

		#region Methods

		public void Parse(string xml)
		{
			Clear();

			IEnumerable<CallRatingCauseOption> options = ParseOptions(xml);
			m_OptionsSection.Execute(() => m_Options.AddRange(options));
		}

		public static IEnumerable<CallRatingCauseOption> ParseOptions(string xml)
		{
			string options = XmlUtils.GetChildElementAsString(xml, "Options");

			return XmlUtils.GetChildElementsAsString(options).Select(s => CallRatingCauseOption.FromXml(s));
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<CallRatingCauseOption> GetEnumerator()
		{
			List<CallRatingCauseOption> output = m_OptionsSection.Execute(() => m_Options.ToList());
			return output.GetEnumerator();
		}

		public void Add(CallRatingCauseOption item)
		{
			m_OptionsSection.Execute(() => m_Options.Add(item));
		}

		public void Clear()
		{
			m_OptionsSection.Execute(() => m_Options.Clear());
		}

		public bool Contains(CallRatingCauseOption item)
		{
			return m_OptionsSection.Execute(() => m_Options.Contains(item));
		}

		public void CopyTo(CallRatingCauseOption[] array, int arrayIndex)
		{
			m_OptionsSection.Execute(() => m_Options.CopyTo(array, arrayIndex));
		}

		public bool Remove(CallRatingCauseOption item)
		{
			return m_OptionsSection.Execute(() => m_Options.Remove(item));
		}

		#endregion
	}
}
