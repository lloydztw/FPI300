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

        [Description("缺少 '全域校正' 數據")]
        NO_CALIB_TRANSFORM,

        [Description("缺少 跑線時期的 PLC 格點\n\r\n\r請設定 '空盤檢測'")]
        NO_RUNTIME_PLC_GRID,

        [Description("參數 格點陣列 (Camera Grid) 沒有建置\n\r\n\r請執行 '生成陣列'")]
        NO_CAMERA_GRID,

        [Description("參數 格點陣列 (Camera Grid) 行列數太小")]
        LOW_GRID_ROWS_COLS,

        [Description("無法抓到格點")]
        CAN_NOT_FETCH_CAMERA_GRID,

        [Description("空盤檢測 參數沒建立")]
        NO_EMPTY_TRAY_RECIPE,

        [Description("線掃AOI 運行異常 (可能沒有加密狗)")]
        EXCEPTION_AT_AOI_RUN,
    }
}
