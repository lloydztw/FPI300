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


namespace EzComm.Utils
{
    public class EzSpinlockU32
    {
        private SpinLock m_spinlock = new SpinLock();
        private uint m_cacheData = 0;

        public void Write(uint data)
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
        public uint Read()
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
        public uint Value
        {
            get { return Read(); }
            set { Write(value); }
        }
    }
}