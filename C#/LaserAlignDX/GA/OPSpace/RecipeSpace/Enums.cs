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

namespace LaserAlignDX.OPSpace.RecipeSpace
{
    public enum LightChannelEnum : int
    {
        [Description("紅光")]
        Red = 1,
        [Description("白光")]
        White = 2,
        [Description("紅白光")]
        Red_White = 3,
    };
}
