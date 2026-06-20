#region AUTHOR
/*
 * EzPlc.Fatek
 * 
 * Copyright (C) 2026
 * 2026-04-05 revised by LeTian Chang
 * 2013-07-11 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;

namespace EzPlc.Fatek
{
    public class FatekIoMemory : IoMemory
    {
        /// <summary>
        /// 是否使用 DWM, DWX, DWY 來承載 M, X, Y
        /// </summary>
        public readonly bool OPT_USING_HOST_32BIT;

        public FatekIoMemory(IoDevice device = null, bool usingHost32Bit = false) : base(device)
        {
            OPT_USING_HOST_32BIT = usingHost32Bit;
        }
        public FatekIoMemory(bool usingHost32Bit) : this(null, usingHost32Bit)
        {
        }

        public override IoPoint GetPoint(string ezIoName)
        {
            //// 0. FatekAddr.TryParse 不會拋出異常
            //var fAddr = FatekAddr.TryParse(ezIoName);
            //if (fAddr == null)
            //    return null;

            // 1. 強制使用 new FatekAddr(ezIoName);
            //    如果 ezIoName 不合規範, 就會拋出異常.
            var fAddr = new FatekAddr(ezIoName);

            byte sid = fAddr.StationID;
            string cate = fAddr.Category;
            int addrNumber = fAddr.Address;
            bool isOneBit = fAddr.Bits == 1;

            // 2. 如果是 1-Bit  則取得 hostAddr 
            FatekAddr hostAddr;
            if (isOneBit)
            {
                if (OPT_USING_HOST_32BIT)
                {
                    // 執行 Bit 到 DWord 的映射 (例如 M33 -> DWM32)
                    int hostAddrNumber = (addrNumber / 32) * 32;
                    string regCate = "DW" + cate;
                    hostAddr = new FatekAddr($"{sid}:{regCate}{hostAddrNumber}");
                }
                else
                {
                    // 執行 Bit 到 Word 的映射 (例如 M17 -> WM16)
                    int hostAddrNumber = (addrNumber / 16) * 16;
                    string regCate = "W" + cate;
                    hostAddr = new FatekAddr($"{sid}:{regCate}{hostAddrNumber}");
                }
            }
            else
            {
                // 16-bit or 32-bit 暫存器 R, D, DR, DD, 自己直接作為 Host
                hostAddr = fAddr;
            }

            // 3. 直接利用 hostAddr.KeyName 作為唯一的快取 KEY
            string keyName = hostAddr.KeyName;

            // 4. 取得或建立 Register 實例
            IoPointReg ioReg = GetOrAddPoint(keyName, (k) => new FatekIoPointReg(hostAddr)) as IoPointReg;

            // 5. 自動綁定硬體設備
            var device = base.IoDevice;
            if (device != null)
            {
                if (ioReg is IoDevPointReg devReg)
                    devReg.BindDevice(device);
            }

            // 6. 回傳對應的點位物件
            if (isOneBit)
            {
                // 透過 IoPointReg 的索引子取得唯一的 IoPointBit 實例
                int bitOffset = addrNumber % hostAddr.Bits;
                return ioReg[bitOffset];
            }

            return ioReg;
        }
        public override IoMemoryBank GetBank(IAddress baseAddr, int span)
        {
            //FatekAddr.ParseCategory(baseAddr.Category, out var _, out var _, out var alignment, out var _);
            var fkCategory = (FatekCateEnum)baseAddr.CateID;
            FatekAddr.ParseCategory(fkCategory, out var _, out var _, out var _, out var alignment, out var _);
            var bank = new IoMemoryBank(this, baseAddr, span, alignment);
            return bank;
        }
    }
}

