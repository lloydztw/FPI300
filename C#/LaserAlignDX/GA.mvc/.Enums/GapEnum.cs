#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 初稿 (by LeTian Chang)
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
    public enum GapEnum : int
    {
        [Description("左上X")]
        LUX,
        [Description("左上Y")]
        LUY,

        [Description("右上X")]
        RUX,
        [Description("右上Y")]
        RUY,

        [Description("右下X")]
        RDX,
        [Description("右下Y")]
        RDY,

        [Description("左下X")]
        LDX,
        [Description("左下Y")]
        LDY,
    }
}
