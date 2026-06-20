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

using EzComm.Uart;
using EzIO.Device;
using EzPlc.Fatek.Comm;
using System;

namespace EzPlc.Fatek.AutoScan.Try001
{
    public class FatekAutoScanner : IAutoScan, IDisposable
    {
        public event EventHandler OnScanned;

        #region PRIVATE_DATA
        FatekIoDevice _device;
        FkCmd[] _autoScanCmds;
        bool _isRunning = false;
        #endregion

        public FatekAutoScanner(FatekIoDevice device, FkCmd[] cmds)
        {
            _device = device;
            _autoScanCmds = cmds;
        }
        public override string ToString()
        {
            var cmds = _autoScanCmds;
            if (cmds == null || cmds.Length == 0)
                return "No AutoScan Commands Defined.";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"{GetType().Name} @ {_device.KeyName}");
            for (int i = 0; i < cmds.Length; i++)
            {
                sb.AppendLine($"  [{i:D2}] {cmds[i]}");
            }

            return sb.ToString();
        }
        public void Dispose()
        {
            ((IAutoScan)this).Stop();
        }

        void postRoundRobinCmds()
        {
            var comm = _device.FatekComm;
            var p = CmdPriority.RoundRobin;
            for (int i = 0, N = _autoScanCmds.Length; i < N; i++)
            {
                var cmd = _autoScanCmds[i];
                cmd.Priority = (byte)p;
                if (i != N - 1)
                    comm.PostCmd(cmd, 0, (int)p, null);
                else
                    comm.PostCmd(cmd, 0, (int)p, OnCmdCompleted);
            }
        }
        void removeRoundRobinCmds()
        {
            foreach (var cmd in _autoScanCmds)
                cmd.Priority = (int)CmdPriority.Normal;
        }
        void OnCmdCompleted(CUartCmdResult r)
        {
            OnScanned?.Invoke(this, null);
        }

        int IAutoScan.ScanInterval
        {
            get;
            set;
        }
        bool IAutoScan.IsRunning()
        {
            return _isRunning;
        }
        void IAutoScan.Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            postRoundRobinCmds();
        }
        void IAutoScan.Stop()
        {
            if (!_isRunning) return;
            _isRunning = false;
            removeRoundRobinCmds();
        }
    }
}
