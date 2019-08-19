using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Partitioning.Rooms
{
	public sealed class RoomOriginatorIdCollection
	{
		/// <summary>
		/// Raised when items are added/removed to/from the collection.
		/// </summary>
		public event EventHandler OnChildrenChanged;

		private readonly IcdOrderedDictionary<int, eCombineMode> m_Ids;
		private readonly SafeCriticalSection m_Section;
		private readonly IRoom m_Room;

		/// <summary>
		/// Gets the number of items in the collection.
		/// </summary>
		public int Count { get { return m_Section.Execute(() => m_Ids.Count); } }

		/// <summary>
		/// Gets the core originators collection.
		/// </summary>
		private IOriginatorCollection<IOriginator> Originators { get { return m_Room.Core.Originators; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		public RoomOriginatorIdCollection(IRoom room)
		{
			m_Ids = new IcdOrderedDictionary<int, eCombineMode>();
			m_Section = new SafeCriticalSection();

			m_Room = room;
		}

		#region Methods

		/// <summary>
		/// Clears the ids from the collection.
		/// </summary>
		public void Clear()
		{
			SetIds(Enumerable.Empty<KeyValuePair<int, eCombineMode>>());
		}

		#region Ids

		/// <summary>
		/// Gets the child ids.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<int> GetIds()
		{
			m_Section.Enter();

			try
			{
				return m_Ids.Count == 0
					       ? Enumerable.Empty<int>()
					       : m_Ids.Keys.ToArray(m_Ids.Count);
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Clears and sets the child ids.
		/// </summary>
		public void SetIds(IEnumerable<KeyValuePair<int, eCombineMode>> ids)
		{
			if (ids == null)
				throw new ArgumentNullException("ids");

			m_Section.Enter();

			try
			{
				Dictionary<int, eCombineMode> newIds = ids.ToDictionary();
				if (newIds.DictionaryEqual(m_Ids))
					return;

				m_Ids.Clear();

				m_Ids.AddRange(newIds);
			}
			finally
			{
				m_Section.Leave();
			}

			OnChildrenChanged.Raise(this);
		}

		/// <summary>
		/// Adds the id to the collection.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="combine"></param>
		/// <returns>False if the collection already contains the given id and it has the given combine mode.</returns>
		public bool Add(int id, eCombineMode combine)
		{
			bool output = AddInternal(id, combine);
			if (output)
				OnChildrenChanged.Raise(this);

			return output;
		}

		/// <summary>
		/// Adds the ids to the collection.
		/// </summary>
		/// <param name="ids"></param>
		public void AddRange(IEnumerable<KeyValuePair<int, eCombineMode>> ids)
		{
            if (ids == null)
                throw new ArgumentNullException("ids");

			bool change = false;
			
			m_Section.Enter();

			try
			{
			    foreach (KeyValuePair<int, eCombineMode> kvp in ids)
			        change |= AddInternal(kvp.Key, kvp.Value);
			}
			finally
			{
				m_Section.Leave();
			}
			
			if (change)
				OnChildrenChanged.Raise(this);
		}

		/// <summary>
		/// Adds the id to the collection.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="combine"></param>
		/// <returns>False if the collection already contains the given id and it has the given combine mode.</returns>
		private bool AddInternal(int id, eCombineMode combine)
		{
			if (combine == eCombineMode.None)
				return false;

			m_Section.Enter();

			try
			{
				eCombineMode existing;
				if (m_Ids.TryGetValue(id, out existing) && combine == existing)
					return false;

				m_Ids[id] = combine;

				return true;
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Removes the id from the collection.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>False if the collection doesn't contain the given id.</returns>
		public bool Remove(int id)
		{
			bool output = RemoveInternal(id);
			if (output)
				OnChildrenChanged.Raise(this);

			return output;
		}

		/// <summary>
		/// Removes the ids from the collection.
		/// </summary>
		/// <param name="ids"></param>
		public void RemoveRange(IEnumerable<int> ids)
		{
			if (ids == null)
				throw new ArgumentNullException("ids");

			bool output = false;

			m_Section.Enter();

			try
			{
				foreach (int id in ids)
					output |= RemoveInternal(id);
			}
			finally
			{
				m_Section.Leave();
			}

			if (output)
				OnChildrenChanged.Raise(this);
		}

		/// <summary>
		/// Removes the id from the collection.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>True if the collection contains the given id.</returns>
		private bool RemoveInternal(int id)
		{
			return m_Section.Execute(() => m_Ids.Remove(id));
		}

		/// <summary>
		/// Returns true if the collection contains the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool Contains(int id)
		{
			return Contains(id, eCombineMode.Always);
		}

		/// <summary>
		/// Returns true if the collection contains the given id intersecting with the given mask.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="mask"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool Contains(int id, eCombineMode mask)
		{
			m_Section.Enter();

			try
			{
				eCombineMode mode = m_Ids.GetDefault(id, eCombineMode.None);
				return EnumUtils.GetFlagsIntersection(mode, mask) != eCombineMode.None;
			}
			finally
			{
				m_Section.Leave();
			}
		}

		#endregion

		#region Combine

		/// <summary>
		/// Gets the combine mode for the child with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public eCombineMode GetCombineMode(int id)
		{
			m_Section.Enter();

			try
			{
				eCombineMode mode;
				if (m_Ids.TryGetValue(id, out mode))
					return mode;

				string message = string.Format("{0} does not contain a {1} with id {2}", GetType().Name, typeof(IOriginator).Name, id);
				throw new KeyNotFoundException(message);
			}
			finally
			{
				m_Section.Leave();
			}
		}

		#endregion

		#region Instances

		/// <summary>
		/// Gets the first originator instance with the given type.
		/// </summary>
		/// <typeparam name="TInstance"></typeparam>
		/// <returns></returns>
		[CanBeNull]
		public TInstance GetInstance<TInstance>()
			where TInstance : class, IOriginator
		{
			return GetInstance<TInstance>(i => true);
		}

		/// <summary>
		/// Gets the first originator instance with the given type.
		/// </summary>
		/// <typeparam name="TInstance"></typeparam>
		/// <returns></returns>
		[CanBeNull]
		public TInstance GetInstance<TInstance>(Func<TInstance, bool> selector)
			where TInstance : class, IOriginator
		{
			if (selector == null)
				throw new ArgumentNullException("selector");

			return GetInstance(eCombineMode.Always, selector);
		}

		/// <summary>
		/// Gets the first originator instance with the given type.
		/// </summary>
		/// <typeparam name="TInstance"></typeparam>
		/// <returns></returns>
		[CanBeNull]
		public TInstance GetInstance<TInstance>(eCombineMode mask, Func<TInstance, bool> selector)
			where TInstance : class, IOriginator
		{
			if (selector == null)
				throw new ArgumentNullException("selector");

			if (mask == eCombineMode.None)
				return null;

			m_Section.Enter();

			try
			{
				if (m_Ids.Count == 0)
					return null;

				IEnumerable<int> ids =
					m_Ids.Where(kvp => EnumUtils.GetFlagsIntersection(kvp.Value, mask) != eCombineMode.None)
					     .Select(kvp => kvp.Key);

				return Originators.GetChild(ids, selector);
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Gets all of the originator instances of the given type from the core.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TInstance> GetInstances<TInstance>()
			where TInstance : class, IOriginator
		{
			return GetInstances<TInstance>(i => true);
		}

		/// <summary>
		/// Gets all of the originator instances of the given type from the core.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TInstance> GetInstances<TInstance>(Func<TInstance, bool> selector)
			where TInstance : class, IOriginator
		{
			if (selector == null)
				throw new ArgumentNullException("selector");

			return GetInstances(eCombineMode.Always, selector);
		}

		/// <summary>
		/// Gets all of the originator instances of the given type from the core.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TInstance> GetInstances<TInstance>(eCombineMode mask, Func<TInstance, bool> selector)
			where TInstance : class, IOriginator
		{
			if (selector == null)
				throw new ArgumentNullException("selector");

			m_Section.Enter();

			try
			{
				if (m_Ids.Count == 0)
					return Enumerable.Empty<TInstance>();

				IEnumerable<int> ids =
					m_Ids.Where(kvp => EnumUtils.GetFlagsIntersection(kvp.Value, mask) != eCombineMode.None)
					     .Select(kvp => kvp.Key);

                return Originators.GetChildren(ids, selector);
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Returns true if instances of the given type exist.
		/// </summary>
		/// <typeparam name="TInstance"></typeparam>
		/// <returns></returns>
		public bool HasInstances<TInstance>()
			where TInstance : class, IOriginator
		{
			return m_Section.Execute(() => Originators.ContainsChildAny<TInstance>(m_Ids.Keys));
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
			return ContainsRecursive(id, eCombineMode.Always);
		}

		/// <summary>
		/// Returns true if the given id is contained in this collection or any child collection recursively.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="mask"></param>
		/// <returns></returns>
		public bool ContainsRecursive(int id, eCombineMode mask)
		{
			if (mask == eCombineMode.None)
				return false;

			// Combine room
			if (Contains(id, mask))
				return true;

			// Master and slaves
			bool master = true;
			foreach (IRoom room in m_Room.GetMasterAndSlaveRooms())
			{
				eCombineMode childMask = mask & (master ? eCombineMode.Master : eCombineMode.Slave);
				if (room.Originators.Contains(id, childMask))
					return true;

				master = false;
			}

			return false;
		}

		/// <summary>
		/// Gets the first instance of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		[CanBeNull]
		public TInstance GetInstanceRecursive<TInstance>()
			where TInstance : class, IOriginator
		{
			return GetInstanceRecursive<TInstance>(eCombineMode.Always);
		}

		/// <summary>
		/// Gets the first instance of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		[CanBeNull]
		public TInstance GetInstanceRecursive<TInstance>(eCombineMode mask)
			where TInstance : class, IOriginator
		{
			return GetInstanceRecursive<TInstance>(mask, i => true);
		}

		/// <summary>
		/// Gets the first instance of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		[CanBeNull]
		public TInstance GetInstanceRecursive<TInstance>(Func<TInstance, bool> selector)
			where TInstance : class, IOriginator
		{
			if (selector == null)
				throw new ArgumentNullException("selector");

			return GetInstanceRecursive(eCombineMode.Always, selector);
		}

		/// <summary>
		/// Gets the first instance of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		[CanBeNull]
		public TInstance GetInstanceRecursive<TInstance>(eCombineMode mask, Func<TInstance, bool> selector)
			where TInstance : class, IOriginator
		{
			if (selector == null)
				throw new ArgumentNullException("selector");

			if (mask == eCombineMode.None)
				return null;

			// Combine room
			TInstance instance = GetInstance(mask, selector);
			if (instance != null)
				return instance;

			// Master and slaves
			bool master = true;
			foreach (IRoom room in m_Room.GetMasterAndSlaveRooms())
			{
				eCombineMode childMask = mask & (master ? eCombineMode.Master : eCombineMode.Slave);

				instance = room.Originators.GetInstance(childMask, selector);
				if (instance != null)
					return instance;

				master = false;
			}

			return null;
		}

		/// <summary>
		/// Gets all instances recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IOriginator> GetInstancesRecursive(eCombineMode mask)
		{
			return GetInstancesRecursive<IOriginator>(mask);
		}

		/// <summary>
		/// Gets all instances of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TInstance> GetInstancesRecursive<TInstance>()
			where TInstance : class, IOriginator
		{
			return GetInstancesRecursive<TInstance>(eCombineMode.Always);
		}

		/// <summary>
		/// Gets all instances of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TInstance> GetInstancesRecursive<TInstance>(eCombineMode mask)
			where TInstance : class, IOriginator
		{
			return GetInstancesRecursive<TInstance>(mask, i => true);
		}

		/// <summary>
		/// Gets all instances of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TInstance> GetInstancesRecursive<TInstance>(Func<TInstance, bool> selector)
			where TInstance : class, IOriginator
		{
			if (selector == null)
				throw new ArgumentNullException("selector");

			return GetInstancesRecursive(eCombineMode.Always, selector);
		}

		/// <summary>
		/// Gets all instances of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TInstance> GetInstancesRecursive<TInstance>(eCombineMode mask, Func<TInstance, bool> selector)
			where TInstance : class, IOriginator
		{
			if (selector == null)
				throw new ArgumentNullException("selector");

			return GetInstancesRecursiveIterator(mask, selector).Distinct();
		}

		/// <summary>
		/// Gets all instances of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<TInstance> GetInstancesRecursiveIterator<TInstance>(eCombineMode mask, Func<TInstance, bool> selector)
			where TInstance : class, IOriginator
		{
			if (mask == eCombineMode.None)
				yield break;

			// Combine room
			foreach (TInstance instance in GetInstances(mask, selector))
				yield return instance;

			// Master and slaves
			bool master = true;
			foreach (IRoom room in m_Room.GetMasterAndSlaveRooms())
			{
				eCombineMode childMask = mask & (master ? eCombineMode.Master : eCombineMode.Slave);

				foreach (TInstance instance in room.Originators.GetInstances(childMask, selector))
					yield return instance;

				master = false;
			}
		}

		#endregion

		#endregion
	}
}
