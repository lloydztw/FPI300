#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-29 重整 Gaara 原來的 MvdFindLineClass (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.ComponentModel;

namespace LaserAlignDX.BasicSpace
{
    public enum EdgeBorder
    {
        [Description("左 邊線")]
        Left,
        [Description("上 邊線")]
        Top,
        [Description("右 邊線")]
        Right,
        [Description("下 邊線")]
        Bottom
    }

    public enum EdgeBackGroundType : int
    {
        [Description("黑色 載台背景")]
        Dark,
        [Description("白色 載台背景")]
        White
    }

    public enum EdgeSearchPolarity : int
    {
        [Description("黑 到 白")]
        BlackToWhite,
        [Description("白 到 黑")]
        WhiteToBlack,
        [Description("雙極")]
        Both,
    }
}
