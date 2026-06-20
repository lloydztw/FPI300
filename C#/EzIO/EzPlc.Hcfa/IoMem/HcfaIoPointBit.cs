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
    public class HcfaIoPointBit : IoDevPointBit
    {
        internal HcfaIoPointBit(HcfaAddress addr) : base(addr, null, addr.BitOffset)
        {
        }
    }
}