#region AUTHOR
/*
 * EzIO.Mem
 * Copyright (C) 2026
 * 2026-04-05 revised by LeTian Chang
 * 2013-07-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.ComponentModel;

namespace EzIO.Mem
{
    public enum IoAttrib : byte
    {
        [Description("唯讀")]
        In = 0x01,

        [Description("唯寫")]
        Out = 0x02,

        [Description("可讀可寫")]
        InOut = 0x03,

        [Description("自動更新")]
        AutoScan = 0x10,

        [Description("常閉")]
        NormalClose = 0x80,
    }
}
