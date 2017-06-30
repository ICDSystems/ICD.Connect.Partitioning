using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Settings;

namespace ICD.Connect.Rooms
{
	public abstract class AbstractRoomChildIdCollection<TChild> : IEnumerable<TChild>
		where TChild : IOriginator
	{
		public event EventHandler OnChildrenChanged;

		private readonly IcdHashSet<int> m_Ids;
		private readonly SafeCriticalSection m_Section;
		private readonly IRoom m_Room;

		/// <summary>
		/// Gets the number of items in the collection.
		/// </summary>
		public int Count { get { return m_Section.Execute(() => m_Ids.Count); } }

		/// <summary>
		/// Gets the originator instance with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public TChild this[int id] { get { return GetInstance(id); } }

		public IOriginatorCollection<IOriginator> Collection { get { return m_Room.Core.Originators; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractRoomChildIdCollection(IRoom room)
		{
			m_Ids = new IcdHashSet<int>();
			m_Section = new SafeCriticalSection();
			m_Room = room;
		}

		#region Methods

		/// <summary>
		/// Clears the ids from the collection.
		/// </summary>
		public void Clear()
		{
			m_Section.Execute(() => m_Ids.Clear());
			OnChildrenChanged.Raise(this);
		}

		/// <summary>
		/// Gets the originator instance with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public TChild GetInstance(int id)
		{
			m_Section.Enter();

			try
			{
				if (m_Ids.Contains(id))
					return Collection.GetChild<TChild>(id);

				string message = string.Format("{0} does not contain a {1} with id {2}", GetType().Name, typeof(TChild).Name, id);
				throw new InvalidOperationException(message);
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Gets the originator instance with the given type and id.
		/// </summary>
		/// <typeparam name="TInstance"></typeparam>
		/// <param name="id"></param>
		/// <returns></returns>
		public TInstance GetInstance<TInstance>(int id)
			where TInstance : TChild
		{
			TChild child = GetInstance(id);
			TInstance output = (TInstance)child;

			if (output != null)
				return output;

			throw new InvalidCastException(string.Format("{0} is not of type {1}", typeof(TChild).Name,
			                                             typeof(TInstance).Name));
		}

		/// <summary>
		/// Gets all of the orignator instances from the core.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TChild> GetInstances()
		{
			return GetIds().Select(id => GetInstance(id));
		}

		/// <summary>
		/// Gets all of the originator instances of the given type from the core.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TInstance> GetInstances<TInstance>()
			where TInstance : TChild
		{
			return GetInstances().OfType<TInstance>();
		}

		/// <summary>
		/// Gets the child ids.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<int> GetIds()
		{
			return m_Section.Execute(() => m_Ids.Order().ToArray());
		}

		/// <summary>
		/// Clears and sets the child ids.
		/// </summary>
		public void SetIds(IEnumerable<int> ids)
		{
			m_Section.Enter();

			try
			{
				Clear();
				ids.ForEach(id => Add(id));
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Adds the id to the collection.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>False if the collection already contains the given id.</returns>
		public bool Add(int id)
		{
			m_Section.Enter();

			try
			{
				if (!m_Ids.Add(id))
					return false;
			}
			finally
			{
				m_Section.Leave();
			}

			OnChildrenChanged.Raise(this);
			return true;
		}

		/// <summary>
		/// Removes the id from the collection.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>False if the collection doesn't contain the given id.</returns>
		public bool Remove(int id)
		{
			m_Section.Enter();

			try
			{
				if (!m_Ids.Contains(id))
					return false;

				if (!m_Ids.Remove(id))
					return false;
			}
			finally
			{
				m_Section.Leave();
			}

			OnChildrenChanged.Raise(this);
			return true;
		}

		#endregion

		#region IEnumerable Methods

		public IEnumerator<TChild> GetEnumerator()
		{
			return GetInstances().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
