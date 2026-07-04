using System.ComponentModel;

namespace EzAoiEmptyTrayInspector.Model
{
    public enum Prompts : int
    {
        OK,

        [Description("系統忙碌中, 是否強制退出?")]
        Question_System_Busy_Force_To_Quit,

        [Description("請必須先關掉 Tool!")]
        Info_Please_Turn_Off_Tool_To_Continue,

        [Description("馬達座標欄位只能輸入數字!")]
        Waring_Motor_Coords_Input_Must_Be_Number,

        [Description("是否自動更新參數設定的 (rows, cols)\n\r再自動重新抓取?")]
        Question_Update_RowsCols_And_Fetch_Again,
    }
}
