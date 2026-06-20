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

using EzIO.Device;
using EzPlc.Fatek.Comm;
using System;

namespace EzPlc.Fatek.AutoScan
{
    public class FatekAutoScanner : IoAutoScanner
    {
        #region PRIVATE_DATA
        IxFatekComm _comm;
        FkCmd[] _autoScanCmds;
        #endregion

        public FatekAutoScanner(FatekIoDevice device, FkCmd[] cmds)
                : base(device)
        {
            _comm = device.FatekComm;
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
        public override void Dispose()
        {
            base.Dispose();
            _autoScanCmds = null;
        }

        protected override void DoScan()
        {
            var cmds = _autoScanCmds;
            var comm = _comm;

            if (comm != null && cmds != null && cmds.Length > 0)
            {
                foreach (var cmd in cmds)
                {
                    if (cmd == null) continue;

                    // 立即執行通訊
                    var result = comm.SendCmd(cmd);

                    // 異常處理
                    if (result.Error != 0)
                    {
                        // RESERVED
                    }
                }
            }
            else
            {
                // 如果通訊物件遺失，稍微等久一點再重試，避免 Busy Loop
                System.Threading.Thread.Sleep(100);
            }
        }
        protected override void HandleScanException(Exception ex)
        {
            base.HandleScanException(ex);
        }
    }
}
