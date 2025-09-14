#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-28 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.ComponentModel;

namespace LaserAlignDX.Mvc.Model
{
    public enum ErrCodes : int
    {
        [Description("OK")]
        OK = 0,

        [Description("Machine.PLCIO 還沒配置")]
        NO_PLC_IO,

        [Description("缺少完整的 \"全域校正\" 數據")]
        NO_CALIB_TRANSFORM,

        [Description("格點或陣列沒有建置")]
        NO_CAMERA_GRID,

        [Description("陣列太小")]
        LOW_GRID_ROWS_COLS,

        [Description("無法抓到格點")]
        CAN_NOT_FETCH_CAMERA_GRID
    }
}
