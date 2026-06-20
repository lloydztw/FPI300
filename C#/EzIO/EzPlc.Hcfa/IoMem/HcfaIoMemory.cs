#region AUTHOR
/*
 * EzPlc.Hcfa
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-05 LeTian Chang : Integration with EzIO
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;

namespace EzPlc.Hcfa
{
    public class HcfaIoMemory : IoMemory
    {
        public HcfaIoMemory()
        {
        }

        public override IoPoint GetPoint(string ezIoName)
        {
            // 無論是 1-bit or 16-bit, 都全註冊.
            var addr = new HcfaAddress(ezIoName);

            var ioPoint = GetOrAddPoint(addr.KeyName, (k) =>
            {
                if (addr.Bits >= 16)
                    return new HcfaIoPointReg(addr);
                else
                    return new HcfaIoPointBit(addr);
            });

            // 自動綁定硬體設備
            var device = base.IoDevice;
            if (device != null)
                ioPoint.BindDevice(device);

            return ioPoint;
        }

        public override IoMemoryBank GetBank(IAddress baseAddr, int span)
        {
            return base.GetBank(baseAddr, span);
        }
    }
}

