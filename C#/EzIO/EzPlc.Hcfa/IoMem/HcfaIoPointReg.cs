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
    public class HcfaIoPointReg : IoDevPointReg
    {
        public HcfaIoPointReg(HcfaAddress address) : base(address)
        {
        }

        protected override IoPointBit CreatePointBit(int bit)
        {
            // 請直接使用 new HcfaIoPointBit !!!
            throw new System.Exception("請直接使用 new HcfaIoPointBit() !");
            return null;
        }
    }
}