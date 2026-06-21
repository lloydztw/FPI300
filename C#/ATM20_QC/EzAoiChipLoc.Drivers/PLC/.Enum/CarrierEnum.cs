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
    public enum CarrierEnum : int
    {
        [Description("載台1")]
        C1 = 0,
        [Description("載台2")]
        C2,
    }

    public enum SuckerRowEnum : int
    {
        [Description("吸嘴排1")]
        S1 = 0,
        [Description("吸嘴排2")]
        S2,
    }
}
