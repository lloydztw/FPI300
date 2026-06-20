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

using System.ComponentModel;

namespace EzPlc.Hcfa
{
    /// <summary>
    /// 目前 Gaara 只用到 IX, QX, QB, MW 四種
    /// </summary>
    public enum HcfaCateEnum : int
    {
        [Description("輸入繼電器 (X) (1-bit) (8進制)")]
        IX,

        [Description("輸出繼電器 (Y) (1-bit) (8進制)")]
        QX,

        //[Description("輔助繼電器 (M) (1-bit) (10進制)")]
        //MX,

        [Description("資料暫存器 (D) (16-bit) (10進制)")]
        D,

        //[Description("I群 (8-bit 打包, 8進制) 輸入繼電器群 (X)")]
        //IB,

        [Description("Q群 (8-bit 打包, 8進制) 輸出繼電器群 (Y)")]
        QB,

        //[Description("M群 (8-bit 打包, 10進制) 輔助繼電器群 (M)")]
        //MB,

        //[Description("I群 (16-bit 打包, 8進制) 輸入繼電器群 (X)")]
        //IW,

        //[Description("Q群 (16-bit 打包, 8進制) 輸出繼電器群 (Y)")]
        //QW,

        [Description("M群 (16-bit 打包, 10進制) 輔助繼電器群 (M)")]
        MW,

#if (OPT_RESERVED_FOR_32_BIT)
        [Description("I群 (32-bit 打包, 8進制) 輔助繼電器群 (X)")]
        ID,
        [Description("Q群 (32-bit 打包, 8進制) 輔助繼電器群 (Y)")]
        QD,
        [Description("M群 (32-bit 打包, 10進制) 輔助繼電器群 (M)")]
        MD,
        [Description("資料暫存器 (DD) (32-bit) (10進制)")]
        DD,
#endif
    }
}
