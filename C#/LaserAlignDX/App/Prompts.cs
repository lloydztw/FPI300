using System.ComponentModel;

namespace LaserAlignDX
{
    /// <summary>
    /// 此處管理所有的提示詞
    /// </summary>
    public enum Prompts : int
    {
        [Description("請先登入 擁有修改參數權限 的 帳號!")]
        No_Privilege,

        [Description("是否直接把 參考坐標點 (CoordRefs) 寫入PLC?")]
        Question_Write_CoordRefs_To_PLC,

        [Description("是否 同時進行 座標系統 線性遷移?")]
        Question_To_Migrate_Transforms_Models,

        [Description("座標成功寫入至 PLC.")]
        Info_Write_CoordRefs_To_PLC_OK,

        [Description("共用座標系統 建置完成.")]
        Info_CommonBase_Trf_Successed,

        [Description("不支援此功能.")]
        Warn_the_function_is_not_supported,

        //[Description("即將移動 線掃相機 馬達")]
        //Question_To_Move_Big_Linescan_Motor,

        [Description("是否要採用 目前馬達 XY 座標值")]
        Question_Update_Motor_Coord_To_Calib,

        [Description("是否確定要將 馬達 回 HOME")]
        Question_Motor_Home,

        [Description("是否確定要 移動馬達 至定位")]
        Question_Motor_GoTo_Pos,
    }
}
