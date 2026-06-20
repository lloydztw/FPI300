#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-14 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

namespace LeTian.DebugUtil
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

