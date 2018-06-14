﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Settings;

namespace ICD.Connect.Partitioning.Rooms
{
	public sealed class RoomOriginatorIdCollection
	{
		public event EventHandler OnChildrenChanged;

		private readonly List<int> m_OrderedIds;
		private readonly Dictionary<int, eCombineMode> m_Ids;
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
		public RoomOriginatorIdCollection(IRoom room)
		{
			m_OrderedIds = new List<int>();
			m_Ids = new Dictionary<int, eCombineMode>();
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
				return m_OrderedIds.Count == 0
					       ? Enumerable.Empty<int>()
					       : m_OrderedIds.ToArray(m_OrderedIds.Count);
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
				m_OrderedIds.Clear();

				m_Ids.AddRange(newIds);
				m_OrderedIds.AddRange(m_Ids.Keys.Order());
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
			bool output;
			
			m_Section.Enter();

			try
			{
				output = ids.Aggregate(false, (current, kvp) => current || AddInternal(kvp.Key, kvp.Value));
			}
			finally
			{
				m_Section.Leave();
			}
			
			if (output)
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
				if (m_Ids.ContainsKey(id))
				{
					if (combine == m_Ids[id])
						return false;
				}
				else
				{
					m_OrderedIds.AddSorted(id);
				}

				m_Ids[id] = combine;
			}
			finally
			{
				m_Section.Leave();
			}

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
				if (!m_Ids.Remove(id))
					return false;

				m_OrderedIds.Remove(id);
			}
			finally
			{
				m_Section.Leave();
			}

			OnChildrenChanged.Raise(this);
			return true;
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
				if (m_Ids.ContainsKey(id))
					return m_Ids[id];

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
		/// Gets the originator instance with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[NotNull]
		public IOriginator GetInstance(int id)
		{
			m_Section.Enter();

			try
			{
				if (m_Ids.ContainsKey(id))
					return Originators.GetChild<IOriginator>(id);

				string message = string.Format("{0} does not contain a {1} with id {2}", GetType().Name, typeof(IOriginator).Name, id);
				throw new KeyNotFoundException(message);
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Gets the first originator instance with the given type.
		/// </summary>
		/// <typeparam name="TInstance"></typeparam>
		/// <returns></returns>
		[CanBeNull]
		public TInstance GetInstance<TInstance>(Func<TInstance, bool> selector)
			where TInstance : IOriginator
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
			where TInstance : IOriginator
		{
			if (selector == null)
				throw new ArgumentNullException("selector");

			if (mask == eCombineMode.None)
				return default(TInstance);

			m_Section.Enter();

			try
			{
				if (m_Ids.Count == 0)
					return default(TInstance);

				IEnumerable<int> ids =
					m_OrderedIds.Where(id => EnumUtils.GetFlagsIntersection(m_Ids[id], mask) != eCombineMode.None);

				return Originators.GetChild(ids, selector);
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Gets the first originator instance with the given type.
		/// </summary>
		/// <typeparam name="TInstance"></typeparam>
		/// <returns></returns>
		[CanBeNull]
		public TInstance GetInstance<TInstance>()
			where TInstance : IOriginator
		{
			return GetInstance<TInstance>(i => true);
		}

		/// <summary>
		/// Gets all of the originator instances of the given type from the core.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TInstance> GetInstances<TInstance>()
			where TInstance : IOriginator
		{
			return GetInstances<TInstance>(i => true);
		}

		/// <summary>
		/// Gets all of the originator instances of the given type from the core.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TInstance> GetInstances<TInstance>(Func<TInstance, bool> selector)
			where TInstance : IOriginator
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
			where TInstance : IOriginator
		{
			if (selector == null)
				throw new ArgumentNullException("selector");

			m_Section.Enter();

			try
			{
				if (m_Ids.Count == 0)
					return Enumerable.Empty<TInstance>();

				IEnumerable<int> ids =
					m_OrderedIds.Where(id => EnumUtils.GetFlagsIntersection(m_Ids[id], mask) != eCombineMode.None);
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
			where TInstance : IOriginator
		{
			return m_Section.Execute(() => Originators.ContainsChildAny<TInstance>(m_OrderedIds));
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
			where TInstance : IOriginator
		{
			return GetInstanceRecursive<TInstance>(eCombineMode.Always);
		}

		/// <summary>
		/// Gets the first instance of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		[CanBeNull]
		public TInstance GetInstanceRecursive<TInstance>(eCombineMode mask)
			where TInstance : IOriginator
		{
			return GetInstanceRecursive<TInstance>(mask, i => true);
		}

		/// <summary>
		/// Gets the first instance of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		[CanBeNull]
		public TInstance GetInstanceRecursive<TInstance>(Func<TInstance, bool> selector)
			where TInstance : IOriginator
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
			where TInstance : IOriginator
		{
			if (selector == null)
				throw new ArgumentNullException("selector");

			if (mask == eCombineMode.None)
				return default(TInstance);

			// Combine room
			TInstance instance = GetInstance(mask, selector);
// ReSharper disable once CompareNonConstrainedGenericWithNull
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

			return default(TInstance);
		}

		/// <summary>
		/// Gets all instances recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IOriginator> GetInstancesRecursive()
		{
			return GetInstancesRecursive(eCombineMode.Always);
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
			where TInstance : IOriginator
		{
			return GetInstancesRecursive<TInstance>(eCombineMode.Always);
		}

		/// <summary>
		/// Gets all instances of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TInstance> GetInstancesRecursive<TInstance>(eCombineMode mask)
			where TInstance : IOriginator
		{
			return GetInstancesRecursive<TInstance>(mask, i => true);
		}

		/// <summary>
		/// Gets all instances of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TInstance> GetInstancesRecursive<TInstance>(Func<TInstance, bool> selector)
			where TInstance : IOriginator
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
			where TInstance : IOriginator
		{
			if (selector == null)
				throw new ArgumentNullException("selector");

			return GetInstancesRecursiveIterator(mask, selector);
		}

		/// <summary>
		/// Gets all instances of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<TInstance> GetInstancesRecursiveIterator<TInstance>(eCombineMode mask, Func<TInstance, bool> selector)
			where TInstance : IOriginator
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
