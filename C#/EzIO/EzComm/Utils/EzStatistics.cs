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


namespace EzComm.Utils
{
    public class EzStatistics
    {
        #region PRIVATE_DATA
        SpinLock m_spinlock = new SpinLock();
        Dictionary<int, uint> m_errHist = new Dictionary<int, uint>();
        uint m_totalCount = 0;
        #endregion

        public void Reset()
        {
            bool gotLock = false;
            try
            {
                m_spinlock.Enter(ref gotLock);
                reset();
            }
            finally
            {
                if (gotLock)
                    m_spinlock.Exit();
            }
        }
        public uint TotalCount
        {
            get
            {
                return m_totalCount;
            }
        }
        public uint GetCount(int err)
        {
            bool gotLock = false;
            try
            {
                m_spinlock.Enter(ref gotLock);
                return getCount(err);
            }
            finally
            {
                if (gotLock)
                    m_spinlock.Exit();
            }
        }
        public uint Count(int err)
        {
            bool gotLock = false;
            try
            {
                m_spinlock.Enter(ref gotLock);
                return count(err);
            }
            finally
            {
                if (gotLock)
                    m_spinlock.Exit();
            }
        }

        #region PRIVATE_FUNCTIONS
        private void reset()
        {
            m_errHist = new Dictionary<int, uint>();
            m_totalCount = 0;
        }
        private uint count(int err)
        {
            uint n;
            if (m_errHist.ContainsKey(err))
            {
                n = (++m_errHist[err]);
            }
            else
            {
                m_errHist.Add((int)err, n = 1);
            }
            m_totalCount++;
            return n;
        }
        private uint getCount(int err)
        {
            if (m_errHist.ContainsKey((int)err))
                return m_errHist[(int)err];
            return 0;
        }
        #endregion
    }
}