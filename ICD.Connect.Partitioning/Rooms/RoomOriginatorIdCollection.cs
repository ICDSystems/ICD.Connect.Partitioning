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
	public sealed class RoomOriginatorIdCollection
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
		public RoomOriginatorIdCollection(IRoom room)
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
			SetIds(Enumerable.Empty<int>());
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
					       : m_Ids.ToArray();
			}
			finally
			{
				m_Section.Leave();
			}
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
				IcdHashSet<int> idsSet = ids.ToIcdHashSet();
				if (m_Ids.NonIntersection(idsSet).Count == 0)
					return;

				m_Ids.Clear();
				m_Ids.AddRange(idsSet);
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
		public IOriginator GetInstance(int id)
		{
			m_Section.Enter();

			try
			{
				if (m_Ids.Contains(id))
					return Originators.GetChild<IOriginator>(id);

				string message = string.Format("{0} does not contain a {1} with id {2}", GetType().Name, typeof(IOriginator).Name, id);
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
			where TInstance : IOriginator
		{
			IOriginator child = GetInstance(id);

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
			where TInstance : IOriginator
		{
			m_Section.Enter();

			try
			{
				return m_Ids.Count == 0 ? default(TInstance) : Originators.GetChild(m_Ids, selector);
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
		/// Gets all of the orignator instances from the core.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IOriginator> GetInstances()
		{
			m_Section.Enter();

			try
			{
				return m_Ids.Count == 0
					       ? Enumerable.Empty<IOriginator>()
					       : Originators.GetChildren(m_Ids);
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
			where TInstance : IOriginator
		{
			m_Section.Enter();

			try
			{
				if (m_Ids.Count == 0)
					return Enumerable.Empty<TInstance>();

				IEnumerable<TInstance> output = Originators.GetChildren<TInstance>(m_Ids);
				return output as TInstance[] ?? output.ToArray();
			}
			finally
			{
				m_Section.Leave();
			}
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
			             .Select(r => r.Originators)
			             .Any(c => c.Contains(id));
		}

		/// <summary>
		/// Gets all of the ids recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<int> GetIdsRecursive()
		{
			return m_Room.GetRoomsRecursive()
			             .SelectMany(r => r.Originators.GetIds())
			             .Distinct();
		}

		/// <summary>
		/// Gets the instance for the given id recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		[NotNull]
		public IOriginator GetInstanceRecursive(int id)
		{
			RoomOriginatorIdCollection collection = m_Room.GetRoomsRecursive()
			                                              .Select(r => r.Originators)
			                                              .FirstOrDefault(c => c.Contains(id));

			if (collection != null)
				return collection.GetInstance(id);

			string message = string.Format("{0} does not recursively contain a {1} with id {2}", GetType().Name,
			                               typeof(IOriginator).Name, id);
			throw new InvalidOperationException(message);
		}

		/// <summary>
		/// Gets the instance of the given type and id recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		[NotNull]
		public TInstance GetInstanceRecursive<TInstance>(int id)
			where TInstance : IOriginator
		{
			IOriginator child = GetInstanceRecursive(id);

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
			where TInstance : IOriginator
		{
			return GetInstanceRecursive<TInstance>(i => true);
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

			return m_Room.GetRoomsRecursive()
			             .Select(r => r.Originators)
			             .Select(c => c.GetInstance(selector))
				// ReSharper disable once CompareNonConstrainedGenericWithNull
			             .FirstOrDefault(i => i != null);
		}

		/// <summary>
		/// Gets all instances of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IOriginator> GetInstancesRecursive()
		{
			return m_Room.GetRoomsRecursive()
			             .Select(r => r.Originators)
			             .SelectMany(c => c.GetInstances())
			             .Distinct();
		}

		/// <summary>
		/// Gets all instances of the given type recursively as defined by partitions.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TInstance> GetInstancesRecursive<TInstance>()
			where TInstance : IOriginator
		{
			return m_Room.GetRoomsRecursive()
			             .Select(r => r.Originators)
			             .SelectMany(c => c.GetInstances<TInstance>())
			             .Distinct();
		}

		#endregion

		#endregion
	}
}
