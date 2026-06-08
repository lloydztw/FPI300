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
    public enum ErrorCodes : int
    {
        [Description("OK")]
        OK = 0,

        [Description("沒有 Aoi Model")]
        NO_AOI_MODEL,

        [Description("沒有線掃圖檔")]
        NO_LINE_SCAN_IMAGE,

        [Description("Machine.PLCIO 還沒配置")]
        NO_PLC_IO,

        [Description("空盤檢測 參數沒建立")]
        NO_EMPTY_TRAY_RECIPE,

        [Description("缺少 PLC 格點\n\r\n\r請設定 '空盤檢測'")]
        NO_RUNTIME_PLC_GRID,

        [Description("參數 格點陣列 (Camera Grid) 沒有建置\n\r\n\r請執行 '生成陣列'")]
        NO_CAMERA_GRID,

        [Description("格點陣列 (Camera Grid) 行列數太小")]
        LOW_GRID_ROWS_COLS,

        [Description("參數 模板訓練失敗, 無法建立 GoldenQuad2D!")]
        CANNOT_FETCH_GOLDEN_QUAD_2D,

        [Description("像測 無法抓到 格位點\n\r請確認 滿盤 行列數 (rows, cols) 是否設定正確!")]
        AoiErr_can_not_fetch_camera_grid,

        [Description("像測 抓到的 格位點 不一致!")]
        AoiErr_camera_grid_not_consistent,

        [Description("沒有 共用校正參數")]
        CalibErr_No_Recipe,

        [Description("沒有 座標轉換 模型")]
        CalibErr_No_Transform_Model,

        [Description("節距 (Pitch) 不一致!")]
        CalibErr_pitch_not_consistent,

        [Description("點墨 標定 不完整!")]
        CalibErr_Ink_Marks_Not_Completed,

        [Description("馬達座標 標定 不完整!")]
        CalibErr_Motor_Coords_Not_Completed,

        [Description("馬達座標轉換系統 建置不良!")]
        CalibErr_Motor_Coords_Transform_Build_NG,


        [Description("線掃AOI 運行異常 (可能沒有加密狗)")]
        EXCEPTION_AT_AOI_RUN,

        [Description("無法定位, 請建立晶粒匹配模板!")]
        ERR_NO_CHIP_LOCATION,

        [Description("無法抓到 PADS, 請建立晶粒匹配模板!")]
        ERR_NO_CHIP_PADS,

        [Description("晶粒 PAD 格點缺角!")]
        ERR_LACK_CHIP_PAD_CORNER,

        [Description("邊線框 條件不佳, 請再調整 邊線框!")]
        ERR_WEAK_LINE_CONDITION,

        [Description("晶粒格點 條件不佳, 請再抓取圖像!")]
        ERR_WEAK_PADS_CONDITION,

        [Description("晶粒格點 尺寸 計算異常!")]
        ERR_EDGE_DIM_CALCULATION,

        [Description("晶粒格點 邊隙 計算異常!")]
        ERR_EDGE_GAP_CALCULATION,

        [Description("盤面參數設定不一致!")]
        ERR_TRAY_CONFIG_CONFLICTS,

        [Description("請先設定 晶粒匹配樣本!")]
        WARN_NO_GOLDN_TEMPLATE_SETUP,

        [Description("無法找到 GoldenQuad2D!")]
        WARN_CAN_NOT_FETCH_QUAD_2D,

        [Description("無法自動抓到 INK 定位點!\n\r請確認 參數 是否適配?")]
        WARN_CAN_NOT_FETCH_INKS,

        [Description("異常: 定位結果為【全空盤】!\n\r請檢查設備狀態與參數是否適配.")]
        ERR_CHIP_LOC_ALL_EMPTY,
    }
}
