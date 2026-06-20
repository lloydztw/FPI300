#region AUTHOR
/*
 * EzPlc.Fatek
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-05 LeTian Chang: Integration with EzIO
 * 2009-11-30 LeTian Chang: Creation.
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Threading;


namespace EzIO.Mem.Utils
{
    public class EzThreadRunner : IDisposable
    {
        #region NLOG
        private static NLog.Logger s_nlog = null;
        private static NLog.Logger _LOG
        {
            get
            {
                if (s_nlog == null)
                {
                    s_nlog = NLog.LogManager.GetCurrentClassLogger();
                }
                return s_nlog;
            }
        }
        #endregion

        #region PRIVATE_DATA
        private readonly object _lock = new object();
        private readonly Action _scanFunc;
        private readonly object _owner;
        private Thread _thread = null;
        private volatile bool _runFlag = false; // 使用 volatile 確保可見性
        #endregion

        public EzThreadRunner(object owner, Action scanFunc)
        {
            _owner = owner;
            _scanFunc = scanFunc;
            Name = $"{owner} ThreadRunner";
        }
        public string Name
        {
            get;
            set;
        }
        public int IntervalMs
        {
            get;
            set;
        }

        public bool IsRunning()
        {
            // 同時檢查 Flag 與 Thread 物件狀態
            // 只有當 _runFlag 為 true 且執行緒物件存在且還在存活時，才算真正運行中
            var t = _thread;
            return _runFlag && t != null && t.IsAlive;
        }
        public void Start()
        {
            lock (_lock)
            {
                if (_runFlag || _thread != null) return;

                _runFlag = true;
                _thread = new Thread(ThreadEntry)
                {
                    Name = this.Name,
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal
                };
                _thread.Start();
            }
        }
        public void Stop()
        {
            lock (_lock)
            {
                if (!_runFlag) return;

                _runFlag = false;
                if (_thread != null)
                {
                    // 同步等待執行緒結束 (最多 2 秒)
                    if (!_thread.Join(2000))
                    {
                        _LOG.Warn($"{Name} 停止超時，可能存在阻塞操作。");
                        // 除非萬不得已，否則不建議 Abort
                    }
                    _thread = null;
                }
            }
        }
        public void Dispose()
        {
            Stop();
        }

        #region PRIVATE_FUNCTIONS
        private void ThreadEntry()
        {
            try
            {
                while (_runFlag)
                {
                    if (IntervalMs > 0)
                        System.Threading.Thread.Sleep(IntervalMs);
                    if (_scanFunc != null)
                        _scanFunc();
                    else
                        Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                if (_runFlag) 
                    _LOG.Error(ex, $"{Name} 異常終止");
            }
            finally
            {
                _runFlag = false;
            }
        }
        #endregion
    }
}
