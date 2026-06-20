#region AUTHOR
/****************************************************************************
 *                                                                          
 * Copyright (c) 2013 LETIAN CHANG All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$    
 *	        20130711 LeTian Chang : Creation         
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EzComm.Utils
{
    public class EzPriorityQueue<T>
    {
        public static readonly T NULL = default(T);

        #region PRIVATE_MEMBERs
        SpinLock m_spinlock = new SpinLock();
        Dictionary<int, Queue<T>> m_queuesDict;
        T m_activeItem;
        #endregion

        public EzPriorityQueue(IEnumerable<int> priorities)
        {
            m_queuesDict = new Dictionary<int, Queue<T>>();
            foreach (var p in priorities)
                m_queuesDict.Add((int)p, new Queue<T>());
            m_activeItem = NULL;
        }
        public EzPriorityQueue(int totalPrioritiesNumber)
        {
            m_queuesDict = new Dictionary<int, Queue<T>>();
            for (int i = 0; i < totalPrioritiesNumber; i++)
                m_queuesDict.Add(i, new Queue<T>());
            m_activeItem = NULL;
        }
        public int TotalPriorities
        {
            get { return m_queuesDict.Count; }
        }

        public int TotalCount
        {
            get
            {
                bool lockTaken = false;
                try
                {
                    m_spinlock.Enter(ref lockTaken);
                    return totalCount;
                }
                finally
                {
                    if (lockTaken) m_spinlock.Exit(false);
                }
            }
        }
        public int Count(int priority)
        {
            bool lockTaken = false;
            try
            {
                m_spinlock.Enter(ref lockTaken);
                return count(priority);
            }
            finally
            {
                if (lockTaken) m_spinlock.Exit(false);
            }
        }

        public T ActiveItem
        {
            get
            {
                bool lockTaken = false;
                try
                {
                    m_spinlock.Enter(ref lockTaken);
                    return m_activeItem;
                }
                finally
                {
                    if (lockTaken) m_spinlock.Exit(false);
                }
            }
        }
        public T PopOutNextActive(int priority)
        {
            bool lockTaken = false;
            try
            {
                m_spinlock.Enter(ref lockTaken);
                m_activeItem = dequeue(priority);
                return m_activeItem;
            }
            finally
            {
                if (lockTaken) m_spinlock.Exit(false);
            } 
        }
        public IEnumerable<T> PopOutNextActive()
        {
            var priorities = m_queuesDict.Keys.ToList();
            priorities.Sort();
            foreach(var p in priorities)
            {
                yield return PopOutNextActive(p);
            }
        }
        public void DismissActiveItem(T item)
        {
            bool lockTaken = false;
            try
            {
                m_spinlock.Enter(ref lockTaken);
                m_activeItem = NULL;
            }
            finally
            {
                if (lockTaken) m_spinlock.Exit(false);
            } 
        }

        public T Peek(int priority)
        {
            bool lockTaken = false;
            try
            {
                m_spinlock.Enter(ref lockTaken);
                return dequeue(priority);
            }
            finally
            {
                if (lockTaken) m_spinlock.Exit(false);
            } 
        }
        public T Dequeue(int priority)
        {
            bool lockTaken = false;
            try
            {
                m_spinlock.Enter(ref lockTaken);
                return dequeue(priority);
            }
            finally
            {
                if (lockTaken) m_spinlock.Exit(false);
            } 
        }
        public bool Enqueue(T item, int priority)
        {
            bool lockTaken = false;
            try
            {
                m_spinlock.Enter(ref lockTaken);
                return enqueue(item, priority);
            }
            finally
            {
                if (lockTaken) m_spinlock.Exit(false);
            }
        }

        public void Clear()
        {
            bool lockTaken = false;
            try
            {
                m_spinlock.Enter(ref lockTaken);

                foreach (var q in m_queuesDict.Values)
                    q.Clear();

                m_activeItem = NULL;
            }
            finally
            {
                if (lockTaken) m_spinlock.Exit(false);
            }
        }

        #region PRIVATE_FUNCTIONS
        private int totalCount
        {
            get
            {
                int cnt = 0;
                foreach (var q in m_queuesDict.Values)
                    cnt += q.Count;
                return cnt;
            }
        }
        private int count(int priority)
        {
            //var q = priority < m_queuesDict.Length ? m_queuesDict[priority] : null;
            //if (q != null)
            //    return q.Count;

            if (m_queuesDict.TryGetValue(priority, out var q) && q != null)
                return q.Count;
            return 0;
        }
        private T peek(int priority)
        {
            //var q = priority < m_queuesDict.Length ? m_queuesDict[priority] : null;
            //if (q != null && q.Count > 0)
            //    return q.Peek();

            if (m_queuesDict.TryGetValue(priority, out var q) && q != null)
                return q.Peek();
            return NULL;
        }
        private T dequeue(int priority)
        {
            //var q = priority < m_queuesDict.Length ? m_queuesDict[priority] : null;
            //if (q != null && q.Count > 0)
            //    return q.Dequeue();

            if (m_queuesDict.TryGetValue(priority, out var q) && q != null && q.Count > 0)
                return q.Dequeue();
            return NULL;
        }
        private bool enqueue(T item, int priority)
        {
            //var q = priority < m_queuesDict.Length ? m_queuesDict[priority] : null;
            //if (q != null)
            //{
            //    if (!q.Contains(item))
            //    {
            //        q.Enqueue(item);
            //        return true;
            //    }
            //}

            if (m_queuesDict.TryGetValue(priority, out var q) && q != null)
            {
                if (!q.Contains(item))
                {
                    q.Enqueue(item);
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}