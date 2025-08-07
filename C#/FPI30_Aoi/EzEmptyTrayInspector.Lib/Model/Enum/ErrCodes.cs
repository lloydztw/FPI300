using System.ComponentModel;

namespace EzAoiEmptyTrayInspector.Model
{
    public enum ErrCodes : int
    {
        OK,

        [Description("沒有設定參數!")]
        NO_RECIPE = 100,

        [Description("圖像 沒有載入!")]
        NO_IMAGE = 200,
        //[Description("圖像 A 沒有載入!")]
        //NO_IMAGE_A,
        //[Description("圖像 B 沒有載入!")]
        //NO_IMAGE_B,

        //[Description("圖像 A B 長寬不一致!")]
        //IMAGES_SIZE_NOT_EQUAL,
        //[Description("沒有跑過 Match!")]
        //NOT_MATCHED_YET,
        //[Description("圖像A 沒有跑過 Match!")]
        //NOT_MATCHED_YET_A,
        //[Description("圖像B 沒有跑過 Match!")]
        //NOT_MATCHED_YET_B,

        [Description("Match 找不到格點!")]
        NO_MATCH_GRID = 300,
        //[Description("圖像A Match 找不到格點!")]
        //NO_MATCH_GRID_A,
        //[Description("圖像B Match 找不到格點!")]
        //NO_MATCH_GRID_B,

        [Description("Match 異常!")]
        MATCH_ERROR = 400,
        //[Description("Match 逾時!")]
        //MATCH_TIMEOUT,
        //[Description("圖像A Match 逾時!")]
        //MATCH_TIMEOUT_A,
        //[Description("圖像B Match 逾時!")]
        //MATCH_TIMEOUT_B,

        //[Description("圖像A 已經被合併過!")]
        //HAS_BEEN_COMBINED = 500,
        //[Description("建立 DualTransform 異常")]
        //BUILD_DUAL_TRANSFORM_ERROR,
        //[Description("儲存合併圖檔異常!")]
        //SAVE_COMBINED_FILE_ERROR,

        [Description("取樣出來的 格點數 與 FullRows, FullCols 設定值 不同!")]
        GRID_ROWS_COLS_ARE_NOT_THE_SAME_AS_USER_INPUT = 500,

        [Description("格點樣板比對 異常!")]
        ON_GRID_TEMPLATE_MATCH_ERROR = 600,

        [Description("格點外圍偵測 異常!")]
        OUT_GRID_NG_BLOCS_DETECT_ERROR = 700,

        [Description("一鍵執行異常!")]
        RUN_ALL_EXCEPTION = 800,
    }
}
