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
    public class TaskUtil
    {
        public static void LockFreeUpdate<T>(ref T field, Func<T, T> updateFunction) where T : class
        {
            var spinWait = new SpinWait();
            while (true)
            {
                T snapshot1 = field;
                T calc = updateFunction(snapshot1);
                T snapshot2 = Interlocked.CompareExchange(ref field, calc, snapshot1);
                if (snapshot1 == snapshot2) return;
                spinWait.SpinOnce();
            }
        }
        public static void Sleep(int ms)
        {
            //============================================================================
            //!!! 用 System.Threading.Thread.Sleep 最少耗費 15 ms !!!
            //============================================================================
            if (ms < 50)
            {
#if(!OPT_USING_NET45 && false)
            var tmRef = DateTime.Now;
            while (true)
            {
                var ts = DateTime.Now - tmRef;
                if (ts.TotalMilliseconds >= ms)
                {
                    tmRef = DateTime.Now;
                    break;
                }
            }
#else
                System.Threading.SpinWait.SpinUntil(() => false, ms);
#endif
            }
            else
            {
                System.Threading.Thread.Sleep(ms);
            }
        }
    }
}

