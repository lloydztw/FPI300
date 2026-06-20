#region AUTHOR
/****************************************************************************
 *                                                                          
 * Copyright (c) 2012 Jet Eazy Corp. All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$
 *          20120622 LeTian Chang: Revised for the more robust connection.
 *	        20080701 LeTian Chang: Creation         
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/
#endregion

using System;
using System.Threading;


namespace EzComm.Utils
{
    public class QxPtr<T> : IDisposable where T : class, IDisposable
    {
        public event EventHandler OnFinalDisposing;

        #region PRIVATE_DATA
        private int m_refCount = 1;
        private readonly object m_lock = new object();
        #endregion

        #region PROTECTED_DATA
        protected T m_obj;
        #endregion

        public QxPtr(T obj = null)
        {
            m_obj = obj;
        }

        /// <summary>
        /// 增加引用計數。使用原子操作確保執行緒安全。
        /// </summary>
        public bool AddRef()
        {
            while (true)
            {
                int current = m_refCount;

                // 如果物件已經在銷毀過程中 (負數)，則不允許增加引用
                if (current <= 0) 
                    return false;

                if (Interlocked.CompareExchange(ref m_refCount, current + 1, current) == current)
                    return true;
            }
        }

        /// <summary>
        /// 實作標準的 IDisposable 介面
        /// </summary>
        public void Dispose()
        {
            // 使用 CAS Loop (Compare-And-Swap) 確保遞減與銷毀邏輯是原子的
            while (true)
            {
                int current = m_refCount;
                if (current <= 0) return; // 已經銷毀中或已銷毀

                int next = current - 1;
                if (Interlocked.CompareExchange(ref m_refCount, next, current) == current)
                {
                    if (next == 0)
                    {
                        // 確保只有一個執行緒能將狀態轉為銷毀中 (int.MinValue)
                        if (Interlocked.CompareExchange(ref m_refCount, int.MinValue, 0) == 0)
                        {
                            PerformCleanup();
                        }
                    }
                    break;
                }
            }
        }

        private void PerformCleanup()
        {
            // 雙重檢查，確保 m_obj 釋放的原子性
            T objToDispose = null;
            lock (m_lock)
            {
                if (m_obj != null)
                {
                    objToDispose = m_obj;
                    m_obj = null; // 立即設為 null 防止懸空指標
                }
            }

            if (objToDispose != null)
            {
                // 避免在鎖定內執行 Dispose，防止潛在死鎖
                if (!ReferenceEquals(objToDispose, this))
                {
                    objToDispose.Dispose();
                }
                OnFinalDisposing?.Invoke(this, null);
                OnDisposing();
            }
        }

        protected virtual void OnDisposing() { }

        public static implicit operator T(QxPtr<T> ptr)
        {
            // 檢查 ptr 是否為 null 及其內部物件狀態
            if (ptr == null) return null;

            // 如果計數已無效，不應回傳物件
            if (Thread.VolatileRead(ref ptr.m_refCount) <= 0) return null;

            return ptr.m_obj;
        }

        // 終結器：防止開發者忘記呼叫 Dispose
        ~QxPtr()
        {
            Dispose();
        }
    }

    public class QxPtr : QxPtr<IDisposable>
    {
        public QxPtr(IDisposable obj = null) : base(obj) { }
    }
}


namespace EzComm.Utils.OLD
{
    class QxPtr<T> where T : IDisposable
    {
        #region PRIVATE_DATA
        private int m_refCount = 1;
        protected IDisposable m_obj;
        #endregion

        public QxPtr(IDisposable obj = null)
        {
            m_obj = obj;
        }
        public bool AddRef()
        {
            return Interlocked.Increment(ref m_refCount) > 0;
        }
        public void Dispose()
        {
            if (Interlocked.Decrement(ref m_refCount) == 0 &&
                Interlocked.CompareExchange(ref m_refCount, int.MinValue, 0) == 0)
            {
                if (m_obj != this && m_obj != null)
                {
                    m_obj.Dispose();
                    m_obj = null;
                }
                else
                {
                    OnDisposing();
                }
            }
        }
        protected virtual void OnDisposing()
        {
        }

        public static implicit operator T(QxPtr<T> ptr)
        {
            return (ptr != null) ? (T)ptr.m_obj : default;
        }
    }

    class QxPtr : QxPtr<IDisposable>
    {
    }
}

