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
    /// <summary>
    /// 編碼: 0 <= IoPriority < 0xFF
    /// 0xFF 是特殊的 RoundRobin 請不要使用!
    /// 請參考 EzComm 的 CmdPriority
    /// </summary>
    public enum IoPriority : int
    {
        [Description("最高級別直接讀寫")]
        Directly = 0,

        [Description("正常級別")]
        Normal = 1,

        [Description("快取內部更新")]
        InternalUpdate = 0x1F,
    }
}
