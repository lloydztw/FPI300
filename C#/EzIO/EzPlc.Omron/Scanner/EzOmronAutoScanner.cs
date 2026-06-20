#region AUTHOR
/*
 * EzIO.Modbus.AutoScan
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-21 LeTian Chang: Integration with EzIO
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Device;
using EzIO.Mem;
using EzPlc.Omron;
using System;
using System.Collections.Generic;

namespace EzIO.Omron.AutoScan
{
    public class EzOmronAutoScanner : IoAutoScanner
    {
        #region PRIVATE_DATA
        IoDevice _ioDevice;
        List<OmronIoVar> _scanPoints;
        #endregion

        public EzOmronAutoScanner(IoDevice device, IEnumerable<IoPoint> scanPoints)
                : base(device)
        {
            _ioDevice = device;

            _scanPoints = new List<OmronIoVar>();
            
            foreach (var p in scanPoints)
            {
                if (p is OmronIoVar op)
                {
                    op.OptAlwaysDirectlyRead = false;
                    _scanPoints.Add(op);
                }
            }
        }
        
        public override string ToString()
        {
            var points = _scanPoints;
            if (points == null || points.Count == 0)
                return "No AutoScan Banks Defined.";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"{GetType().Name} @ {_device.KeyName}");
            for (int i = 0; i < points.Count; i++)
            {
                sb.AppendLine($"  [{i:D2}] {points[i]}");
            }

            return sb.ToString();
        }
        public override void Dispose()
        {
            _scanPoints = null;
            base.Dispose();
        }

        protected override void DoScan()
        {
            var points = _scanPoints;
            var ioDev = _ioDevice;

            if (ioDev != null && points != null)
            {
                foreach (var point in points)
                {
                    point?.ReadVar(point.Address, IoPriority.Directly);
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
