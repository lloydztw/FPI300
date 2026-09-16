#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 開始重整優化 Gaara 原來的 MvdFindClass (by LeTian Chang)
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
    /// <summary>
    /// 模板比對演算法
    /// </summary>
    public enum MatchAlgorithmEnum
    {
        [Description("格點型 晶粒")]
        GridMatch,

        [Description("一般型 晶粒")]
        TemplateMatch,
    };
}
