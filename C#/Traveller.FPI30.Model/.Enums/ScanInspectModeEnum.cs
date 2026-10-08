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


namespace LaserAlignDX.AoiModel
{
    public enum ScanInspectMode : int
    {
        [Description("外观及尺寸检测")]
        MEASUREAOI = 0,

        [Description("QR检测")]
        QRCODE = 1,

        [Description("空载台检测")]
        NOTRAY = 2,
    }
}
