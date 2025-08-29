/****************************************************************************
 *                                                                          
 * Copyright (c) 2009 Jet Eazy Corp. All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$    
 *	    2008/07/01 The class is created by LeTian Chang
  * DESCRIPTION
 *      
 *
 ***************************************************************************/

namespace LeTian.AoiLib
{
    public class LtDebug
    {
        #region PRIVATE_DATA
        static NLog.Logger _NLOG = NLog.LogManager.GetCurrentClassLogger();
        static EzTiming _TM = new EzTiming(_NLOG);
        #endregion
        
        public static NLog.Logger LOG
        {
            get => _NLOG;
        }

        public static void Reset()
        {
            _TM?.Reset();
        }
        public static void Trace(string remark)
        {
            _TM?.Trace(remark);
        }
        public static void DumpSummary()
        {
            _TM?.Dump();
        }

        public static void RESET_ACCUM()
        {
            _TM?.RESET_ACCUM();
        }
        public static void BEGIN(string remark)
        {
            _TM?.BEGIN(remark);
        }
        public static void END(string remark)
        {
            _TM?.END(remark);
        }
        public static void DUMP_ACCUM()
        {
            _TM?.DUMP_ACCUM();
        }
    }
}

