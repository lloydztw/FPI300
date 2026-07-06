using System.ComponentModel;

namespace LaserAlignDX
{
    /// <summary>
    /// 此處管理所有的提示詞
    /// </summary>
    public enum Prompts : int
    {
        [Description("程序已經啟動, 請勿多開！")]
        ReEntry,

        [Description("沒有此權限!")]
        No_Privilege,

        [Description("是否要删除参数？")]
        Quection_To_Delete_Recipe,

        [Description("参数无法删除!")]
        Info_Can_NOT_Delete_Recipe,

        [Description("名称或版本已存在,请检查!")]
        Info_Recipe_Already_Existing,

        [Description("保存参数中请稍后...")]
        Info_Recipe_Saving,

        [Description("切换参数中请稍后...")]
        Info_Recipe_Switching,

        [Description("取消中请稍后...")]
        Info_Recipe_Rollback,

        [Description("是否 重新驅動 載台線掃相機?")]
        Question_ReTrigger_Carrier_LineScan_Camera,

        [Description("是否直接把 參考坐標點 (CoordRefs) 寫入PLC?")]
        Question_Write_CoordRefs_To_PLC,

        [Description("是否 同時進行 座標系統 線性遷移?")]
        Question_To_Migrate_Transforms_Models,

        [Description("是否 要重新設定 樣本尺寸?")]
        Question_To_Rebuild_Template_Dimension,

        [Description("座標成功寫入至 PLC.")]
        Info_Write_CoordRefs_To_PLC_OK,

        [Description("共用座標系統 建置完成.")]
        Info_CommonBase_Trf_Successed,

        [Description("不支援此功能!")]
        Warning_the_function_is_not_supported,

        [Description("找不到馬達!")]
        Warning_No_Motor,

        [Description("馬達忙碌中...")]
        Warning_Motor_Busy,

        [Description("AOI 執行中...")]
        Warning_AOI_Busy,

        [Description("請先 加載圖檔 或 取像")]
        Info_Please_Load_Or_Grab_Image,

        [Description("飛拍計算角度")]
        Info_FlyCam_Angle,

        [Description("是否要採用 目前馬達 XY 座標值?")]
        Question_Update_Motor_Coord_To_Calib,

        [Description("是否確定要將 馬達 回 HOME?")]
        Question_Motor_Home,

        [Description("是否確定要 移動馬達 至定位?")]
        Question_Motor_GoTo_Pos,

        [Description("即將 移動馬達 至定位.")]
        Info_Motor_GoTo_Pos,

        [Description("Inker 在下位時, 禁止移動 馬達 XY!")]
        Warning_MotorXY_Disabled_By_Inker_Down,

        [Description("已成功保存二值化圖檔")]
        Info_Save_Binary_Image_OK,

        [Description("圖檔 已成功保存")]
        Info_Save_Image_OK,

        [Description("圖檔 無法保存")]
        Info_Save_Image_Failed,

        [Description("參數 已成功保存")]
        Info_Save_Recipe_OK,

        [Description("參數 已自動保存")]
        Info_Auto_Save_Rcipe_OK,

        [Description("是否 繼續跑 模擬?")]
        Question_Continue_To_Run_Simulation,

        [Description("【軟件準備】 已經啟動連線, 無法進行 飛拍 離線測試!")]
        Waring_Software_Ready_Can_NOT_Simulate_Fly_Camera,

        [Description("驅動 PLC 重新線掃 : 尚未完成 !")]
        Waring_Trigger_PLC_To_Start_LineScan_Not_Completed,

        [Description("[離線版]")]
        Info_Simulation,

        [Description("即將進行 循環自測\n\r\n\r是否也要包含 飛拍 模擬?")]
        Question_To_Simulate_With_FlyCam,

        [Description("複製到剪貼簿失敗!")]
        Waring_Copy_To_ClipBoard_Error,

        [Description("請先停止實時畫面!")]
        Waring_Please_Stop_Live_Mode,

        [Description("是否發送模擬數據到PLC?")]
        Question_Send_Sim_Signal_To_PLC,
    }
}
