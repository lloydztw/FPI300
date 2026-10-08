#region AUTHOR
/*
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 */
#endregion

using System.ComponentModel;
namespace Eazy_Project_III
{
    public enum BlobMode : int
    {
        /// <summary>
        /// 找白色 白字黑底
        /// </summary>
        [Description("找亮斑")]
        White = 0,
        /// <summary>
        /// 找黑色 黑字白底
        /// </summary>
        [Description("找黑斑")]
        Black = 1,
    }
    public enum MeasureFindLineType : int
    {
        [Description("V1-找直线")]
        FindLineType_v1 = 0,
        [Description("V2-找平行线")]
        FindLineType_v2 = 1,
    }
    public enum StageNumber : int
    {
        [Description("载台一")]
        N0 = 0,
        [Description("载台二")]
        N1 = 1,
    }
}
