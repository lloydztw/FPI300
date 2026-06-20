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
using System.Threading;


namespace EzComm.Utils.V0
{
    public class EzPriorityQueue<T>
    {
        public static readonly T NULL = default(T);

        #region PRIVATE_MEMBERs
        SpinLock m_spinlock = new SpinLock();
        Queue<T>[] m_queues;
        T m_activeItem;
        #endregion

        public EzPriorityQueue(int priorities)
        {
            m_queues = new Queue<T>[priorities];
            for (int i = 0; i < priorities; i++)
                m_queues[i] = new Queue<T>();
            m_activeItem = NULL;
        }
        public int TotalPriorities
        {
            get { return m_queues.Length; }
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

                foreach (var q in m_queues)
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
                foreach (var q in m_queues)
                    cnt += q.Count;
                return cnt;
            }
        }
        private int count(int priority)
        {
            var q = priority < m_queues.Length ? m_queues[priority] : null;
            if (q != null)
                return q.Count;
            return 0;
        }
        private T peek(int priority)
        {
            var q = priority < m_queues.Length ? m_queues[priority] : null;
            if (q != null && q.Count > 0)
                return q.Peek();
            return NULL;
        }
        private T dequeue(int priority)
        {
            var q = priority < m_queues.Length ? m_queues[priority] : null;
            if (q != null && q.Count > 0)
                return q.Dequeue();
            return NULL;
        }
        private bool enqueue(T item, int priority)
        {
            var q = priority < m_queues.Length ? m_queues[priority] : null;
            if (q != null)
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