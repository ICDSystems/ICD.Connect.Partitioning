using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.Rooms
{
	public abstract class AbstractRoomChildIdCollection<TCollection, TChild>
		where TCollection : AbstractRoomChildIdCollection<TCollection, TChild>
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

		private IOriginatorCollection<IOriginator> Originators { get { return m_Room.Core.Originators; } }

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

		#region Ids

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
			if (ids == null)
				throw new ArgumentNullException("ids");

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
		/// Adds the ids to the collection.
		/// </summary>
		/// <param name="ids"></param>
		public void AddRange(IEnumerable<int> ids)
		{
			m_Section.Enter();

			try
			{
				int count = m_Ids.Count;

				m_Ids.AddRange(ids);

				if (m_Ids.Count == count)
					return;
			}
			finally
			{
				m_Section.Leave();
			}

			OnChildrenChanged.Raise(this);
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

		/// <summary>
		/// Returns true if the 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool Contains(int id)
		{
			return m_Section.Execute(() => m_Ids.Contains(id));
		}

		#endregion

		#region Instances

		/// <summary>
		/// Gets the originator instance with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[NotNull]
		public TChild GetInstance(int id)
		{
			m_Section.Enter();

			try
			{
				if (m_Ids.Contains(id))
					return Originators.GetChild<TChild>(id);

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
		[NotNull]
		public TInstance GetInstance<TInstance>(int id)
			where TInstance : TChild
		{
			TChild child = GetInstance(id);

			if (!child.GetType().IsAssignableTo(typeof(TInstance)))
				throw new InvalidCastException(string.Format("{0} is not of type {1}", child.GetType().Name, typeof(TInstance).Name));

			return (TInstance)child;
		}

		/// <summary>
		/// Gets the first originator instance with the given type.
		/// </summary>
		/// <typeparam name="TInstance"></typeparam>
		/// <returns></returns>
		[CanBeNull]
		public TInstance GetInstance<TInstance>(Func<TInstance, bool> selector)
			where TInstance : TChild
		{
			return m_Section.Execute(() => Originators.GetChild<TInstance>(m_Ids, selector));
		}

		/// <summary>
		/// Gets the first originator instance with the given type.
		/// </summary>
		/// <typeparam name="TInstance"></typeparam>
		/// <returns></returns>
		[CanBeNull]
		public TInstance GetInstance<TInstance>()
			where TInstance : TChild
		{
			return GetInstance<TInstance>(i => true);
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
			return m_Section.Execute(() => Originators.GetChildren<TInstance>(m_Ids));
		}

		#endregion

		#region Recursion

		/// <summary>
		/// Returns true if the given id is contained in this collection or any child collection recursively.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool ContainsRecursive(int id)
		{
			return m_Room.GetRoomsRecursive()
			             .Select(r => GetCollection(r))
			             .Any(c => c.Contains(id));
		}

		/// <summary>
		/// Gets all of the ids recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<int> GetIdsRecursive()
		{
			return m_Room.GetRoomsRecursive()
			             .SelectMany(r => GetCollection(r).GetIds())
			             .Distinct();
		}

		/// <summary>
		/// Gets the instance for the given id recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		[NotNull]
		public TChild GetInstanceRecursive(int id)
		{
			TCollection collection = m_Room.GetRoomsRecursive()
			                               .Select(r => GetCollection(r))
			                               .FirstOrDefault(c => c.Contains(id));

			if (collection != null)
				return collection.GetInstance(id);

			string message = string.Format("{0} does not recursively contain a {1} with id {2}", GetType().Name,
			                               typeof(TChild).Name, id);
			throw new InvalidOperationException(message);
		}

		/// <summary>
		/// Gets the instance of the given type and id recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		[NotNull]
		public TInstance GetInstanceRecursive<TInstance>(int id)
			where TInstance : TChild
		{
			TChild child = GetInstanceRecursive(id);

			if (!child.GetType().IsAssignableTo(typeof(TInstance)))
				throw new InvalidCastException(string.Format("{0} is not of type {1}", child.GetType().Name, typeof(TInstance).Name));

			return (TInstance)child;
		}

		/// <summary>
		/// Gets the first instance of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		[CanBeNull]
		public TInstance GetInstanceRecursive<TInstance>()
			where TInstance : TChild
		{
			return GetInstanceRecursive<TInstance>(i => true);
		}

		/// <summary>
		/// Gets the first instance of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		[CanBeNull]
		public TInstance GetInstanceRecursive<TInstance>(Func<TInstance, bool> selector)
			where TInstance : TChild
		{
			if (selector == null)
				throw new ArgumentNullException("selector");

			return m_Room.GetRoomsRecursive()
						 .Select(r => GetCollection(r))
						 .Select(c => c.GetInstance<TInstance>(selector))
				// ReSharper disable once CompareNonConstrainedGenericWithNull
						 .FirstOrDefault(i => i != null);
		}

		/// <summary>
		/// Gets all instances of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TChild> GetInstancesRecursive()
		{
			return m_Room.GetRoomsRecursive()
			             .Select(r => GetCollection(r))
			             .SelectMany(c => c.GetInstances())
			             .Distinct();
		}

		/// <summary>
		/// Gets all instances of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TInstance> GetInstancesRecursive<TInstance>()
			where TInstance : TChild
		{
			return m_Room.GetRoomsRecursive()
						 .Select(r => GetCollection(r))
						 .SelectMany(c => c.GetInstances<TInstance>())
						 .Distinct();
		}

		/// <summary>
		/// Gets the equivalent collection from the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		protected abstract TCollection GetCollection(IRoom room);

		#endregion

		#endregion
	}
}
