#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-09 重新設計校正架構 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.ComponentModel;

namespace LaserAlignDX
{
    //PC->PLC 单颗结果:
    //  1: Ok
    //  2: 外观NG
    //  3: 空
    //  4: 读码NG
    //  9: 切割NG

    public enum PlcResultCode : int
    {
        [Description("OK")]
        OK = 1,
        [Description("NG")]
        NG = 2,
        [Description("外觀 NG")]
        NG_APPEARANCE = 2,
        [Description("空缺")]
        NG_EMPTY = 3,
        [Description("讀碼 NG")]
        NG_QRCODE = 4,
        [Description("切割 NG")]
        NG_CUT = 9,
    };
}
