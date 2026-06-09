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

        [Description("請先登入 擁有修改參數權限 的 帳號!")]
        No_Privilege,

        [Description("是否 重新驅動 載台線掃相機?")]
        Question_ReTrigger_Carrier_LineScan_Camera,

        [Description("是否直接把 參考坐標點 (CoordRefs) 寫入PLC?")]
        Question_Write_CoordRefs_To_PLC,

        [Description("是否 同時進行 座標系統 線性遷移?")]
        Question_To_Migrate_Transforms_Models,

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
    }
}
