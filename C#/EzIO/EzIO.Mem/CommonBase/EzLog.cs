#region AUTHOR
/*
 * EzIO.Mem
 * Copyright (C) 2026
 * 2026-04-05 revised by LeTian Chang
 * 2013-07-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

namespace EzIO.Mem
{
    public class EzLog
    {
        #region PRIVATE_NLOG_DATA
        private static NLog.Logger _nlog = null;
        #endregion

        public static NLog.Logger LOG
        {
            get
            {
                if (_nlog == null)
                    _nlog = NLog.LogManager.GetCurrentClassLogger();
                return _nlog;
            }
        }
    }
}
