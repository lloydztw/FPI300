#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-12 增修 (by LeTian Chang)
 *      2008-07-01 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace LeTian.AoiLib
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


        #region ACCUMULATION
        class AccumItem
        {
            public DateTime Tm0;
            public double TotalSeconds;
        }
        static Dictionary<string, AccumItem> _accumDict = new Dictionary<string, AccumItem>();
        #endregion

        public void RESET_ACCUM()
        {
            _accumDict?.Clear();
        }
        public void BEGIN(string remark)
        {
            if (_bypass) return;
            if (!_accumDict.ContainsKey(remark))
            {
                var item = new AccumItem() { Tm0 = DateTime.Now, TotalSeconds = 0 };
                _accumDict.Add(remark, item);
            }
            else
            {
                var item = _accumDict[remark];
                item.Tm0 = DateTime.Now;
            }
        }
        public void END(string remark)
        {
            if (_bypass) return;
            if (_accumDict.ContainsKey(remark))
            {
                var item = _accumDict[remark];
                var ts = DateTime.Now - item.Tm0;
                item.TotalSeconds += ts.TotalSeconds;
            }
        }
        public void DUMP_ACCUM()
        {
            if (_bypass) return;

            if (_logger != null)
            {
                foreach (var kp in _accumDict)
                    _logger.Debug("[{0}] = {1:0.00} s", kp.Key, kp.Value.TotalSeconds);
            }
        }
    }
}

