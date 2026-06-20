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
using System;
using System.Collections.Generic;
using System.Linq;

namespace EzIO.Modbus.AutoScan
{
    public class EzModbusAutoScanner : IoAutoScanner
    {
        #region PRIVATE_DATA
        IoDevice _ioDevice;
        IoMemoryBank[] _scanBanks;
        #endregion

        public EzModbusAutoScanner(IoDevice device, IEnumerable<IoMemoryBank> scanBanks)
                : base(device)
        {
            _ioDevice = device;
            _scanBanks = scanBanks.ToArray();
        }
        
        public override string ToString()
        {
            var banks = _scanBanks;
            if (banks == null || banks.Length == 0)
                return "No AutoScan Banks Defined.";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"{GetType().Name} @ {_device.KeyName}");
            for (int i = 0; i < banks.Length; i++)
            {
                sb.AppendLine($"  [{i:D2}] {banks[i]}");
            }

            return sb.ToString();
        }
        public override void Dispose()
        {
            _scanBanks = null;
            base.Dispose();
        }

        protected override void DoScan()
        {
            var banks = _scanBanks;
            var ioDev = _ioDevice;

            if (ioDev != null && banks != null && banks.Length > 0)
            {
                foreach (var bank in banks)
                {
                    if (bank == null) continue;

                    var addr = bank.AddressBase;
                    var cate = addr.GetModbusCategory();
                    if (cate == ModbusCateEnum.DISCRETE_INPUT || cate == ModbusCateEnum.COIL)
                    {
                        ioDev.ReadBits(addr, bank.Span, (idx, on, b) =>
                        {
                            ((IoMemoryBank)b).UpdateCacheByIndex(idx, on);
                        },
                        bank);
                    }
                    else
                    {
                        ioDev.ReadRegs(addr, bank.Span, (idx, value, b) =>
                        {
                            ((IoMemoryBank)b).UpdateCacheByIndex(idx, value);
                        },
                        bank);
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
