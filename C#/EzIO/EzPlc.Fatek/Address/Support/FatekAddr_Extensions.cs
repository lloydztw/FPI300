#region AUTHOR
/*
 * EzPlc.Fatek
 * Copyright (C) 2026
 * 2026-04-05 revised by LeTian Chang
 * 2013-07-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;
using System.Dynamic;


namespace EzPlc.Fatek
{
    public static class FatekAddrExtensions
    {
        /// <summary>
        /// 將 IAddress 轉換成 FatekAddr
        /// </summary>
        public static FatekAddr ToFatekAddr(this IAddress addr)
        {
            if (!(addr is FatekAddr fAddr))
                fAddr = (addr != null) ? new FatekAddr(addr) : null;
            return fAddr;
        }
    }
}
