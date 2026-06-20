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
    public class FatekIoPointBit : IoDevPointBit
    {
        internal FatekIoPointBit(IoPointReg host, int bitOffset, FatekAddr originalAddr)
                    : base(originalAddr, host, bitOffset)
        {
        }
    }
}