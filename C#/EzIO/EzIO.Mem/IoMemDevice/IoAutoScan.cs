#region AUTHOR
/*
 * EzPlc.Fatek
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-05 LeTian Chang: Integration with EzIO
 * 2022-09-16 LeTian Chang: 重整
 * 2009-11-30 LeTian Chang: Creation.
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;
using EzIO.Mem.Utils;
using System;

namespace EzIO.Device
{
    public interface IAutoScan
    {
        event EventHandler OnScanned;
        int ScanInterval
        {
            get; set;
        }
        bool IsRunning();
        void Start();
        void Stop();
    }


    public abstract class IoAutoScanner : IAutoScan, IDisposable
    {
        public event EventHandler OnScanned;

        #region NLOG
        static NLog.Logger _LOG => EzLog.LOG;
        #endregion

        #region PROTECTED_DATA
        protected readonly IoDevice _device;
        protected readonly EzThreadRunner _threadRunner;
        protected bool _disposed = false;
        #endregion

        protected IoAutoScanner(IoDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _threadRunner = new EzThreadRunner(_device, this.InternalScanLoop);
            _threadRunner.Name = $"{_device.KeyName} AutoScanner";
        }
        public virtual void Dispose()
        {
            if (_disposed)
                return;
            Stop();
            OnScanned = null;
            _disposed = true;
        }

        public int ScanInterval
        {
            get => _threadRunner.IntervalMs;
            set => _threadRunner.IntervalMs = value;
        }
        public bool IsRunning()
        {
            return _threadRunner != null && _threadRunner.IsRunning();
        }
        public void Start()
        {
            // 如果是模擬, ScanInterval == 0, 會吃掉很多 CPU 資源 !!!
            if (_device.IsSim && ScanInterval < 1)
                ScanInterval = 1;

            if (!IsRunning())
                _threadRunner?.Start();
        }
        public void Stop()
        {
            if (IsRunning())
                _threadRunner?.Stop();
        }

        #region PRIVATE_FUNCTIONS
        /// <summary>
        /// 核心掃描循環：處理異常隔離與事件觸發
        /// </summary>
        private void InternalScanLoop()
        {
            try
            {
                // 執行子類別具體的通訊邏輯
                DoScan();
                // 每次掃描完成 觸發通知
                OnScanned?.Invoke(_device, null);
            }
            catch (Exception ex)
            {
                HandleScanException(ex);
            }
        }
        #endregion

        #region PROTECTED_VIRTUAL_FUNCTIONS
        /// <summary>
        /// 子類別必須實作：具體的 PLC 通訊指令發送
        /// </summary>
        protected abstract void DoScan();
        protected virtual void HandleScanException(Exception ex)
        {
            // 預設錯誤處理，子類別可覆寫
            _LOG.Error(ex, $"{_device.KeyName} Scan Error");
        }
        #endregion
    }
}
