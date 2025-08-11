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

using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace Traveller106
{
    /// <summary>
    /// 效能追蹤計時
    /// </summary>
    public class EzTiming
    {
        private bool _bypass = false;

        #region PRIVATE_DATA
        private NLog.Logger _logger;
        private DateTime _lastTime = DateTime.Now;
        private Dictionary<string, TimeSpan> _markers = new Dictionary<string, TimeSpan>();
        #endregion

        public EzTiming(NLog.Logger logger = null)
        {
            _logger = logger;
            Reset();
        }
        public void Reset()
        {
            if(_bypass) return;
            _lastTime = DateTime.Now;
            _markers.Clear();
        }
        public void Trace(string marker)
        {
            if (_bypass) return;
            var ts = DateTime.Now - _lastTime;
            if(_logger!=null)
                _logger.Trace("[{0}] @ {1:0.0} s", marker, ts.TotalSeconds);
            else
                _markers.Add(marker, ts);
            _lastTime = DateTime.Now;
        }
        public void Dump()
        {
            if (_bypass) return;
            var ts = DateTime.Now - _lastTime;
            if (_logger != null)
            {
                _logger.Trace("[{0}] @ {1:0.0} s", "End", ts.TotalSeconds);
                return;
            }
            else
            {
                var printFunc = new Action<string>((msg) =>
                {
                    System.Diagnostics.Trace.WriteLine(msg);
                });
                Dump(printFunc);
            }
            _lastTime = DateTime.Now;
        }
        public void Dump(Control wnd)
        {
            if (_bypass) return;
            if (_logger != null)
                return;

            string lines = "";
            var printFunc = new Action<string>((msg) =>
            {
                lines += msg + "\n\r";
            });
            wnd.Text = lines;
            wnd.Refresh();
            Dump(printFunc);
        }
        public void Dump(Action<string> printFunc)
        {
            if (_bypass) return;
            if (_logger != null)
                return;

            string msg;
            double totalSconds = 0;
            foreach (var marker in _markers.Keys)
            {
                double secs = _markers[marker].TotalSeconds;
                totalSconds += secs;
                msg = $"[{marker}] = {secs:0.0} s";
                printFunc(msg);
            }
            msg = $"[Total] = {totalSconds:0.0} s";
            printFunc(msg);
            printFunc("");
        }
    }
}

