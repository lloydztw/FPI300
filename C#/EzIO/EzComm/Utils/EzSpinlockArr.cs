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

using System.Threading;
using System;


namespace EzComm.Utils
{
    public class EzSpinlockArr<T>
    {
        private SpinLock m_spinlock = new SpinLock();
        private T[] m_cacheData;

        public EzSpinlockArr(int len)
        {
            m_cacheData = new T[len];
        }
        public int Length
        {
            get { return m_cacheData.Length; }
        }
        public void Write(int idx, T data)
        {
            bool gotLock = false;
            try
            {
                m_spinlock.Enter(ref gotLock);
                m_cacheData[idx] = data;
            }
            finally
            {
                if (gotLock)
                    m_spinlock.Exit();
            }
        }
        public T Read(int idx)
        {
            bool gotLock = false;
            try
            {
                m_spinlock.Enter(ref gotLock);
                return m_cacheData[idx];
            }
            finally
            {
                if (gotLock)
                    m_spinlock.Exit();
            }
        }
        public T this[int idx]
        {
            get { return Read(idx); }
            set { Write(idx, value); }
        }

        public void Action(int idx, Action<T> action)
        {
            bool gotLock = false;
            try
            {
                m_spinlock.Enter(ref gotLock);
                action(m_cacheData[idx]);
            }
            finally
            {
                if (gotLock)
                    m_spinlock.Exit();
            }
        }
        public TOut Func<TOut>(int idx, Func<T, TOut> func)
        {
            bool gotLock = false;
            try
            {
                m_spinlock.Enter(ref gotLock);
                return func(m_cacheData[idx]);
            }
            finally
            {
                if (gotLock)
                    m_spinlock.Exit();
            }
        }
    }



    public class EzSpinlockObj<T>
    {
        private SpinLock m_spinlock = new SpinLock();
        private T m_cacheData;
        public void Write(T data)
        {
            bool gotLock = false;
            try
            {
                m_spinlock.Enter(ref gotLock);
                m_cacheData = data;
            }
            finally
            {
                if (gotLock)
                    m_spinlock.Exit();
            }
        }
        public T Read()
        {
            bool gotLock = false;
            try
            {
                m_spinlock.Enter(ref gotLock);
                return m_cacheData;
            }
            finally
            {
                if (gotLock)
                    m_spinlock.Exit();
            }
        }

        public void Action(Action<T> action)
        {
            bool gotLock = false;
            try
            {
                m_spinlock.Enter(ref gotLock);
                action(m_cacheData);
            }
            finally
            {
                if (gotLock)
                    m_spinlock.Exit();
            }
        }
        public TOut Func<TOut>(Func<T, TOut> func)
        {
            bool gotLock = false;
            try
            {
                m_spinlock.Enter(ref gotLock);
                return func(m_cacheData);
            }
            finally
            {
                if (gotLock)
                    m_spinlock.Exit();
            }
        }

    }
}