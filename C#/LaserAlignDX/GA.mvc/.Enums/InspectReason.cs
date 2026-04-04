using System.ComponentModel;


namespace LaserAlignDX.OPSpace
{
    public enum InspectReason : int
    {
        [Description("PASS")]
        PASS = 0,

        [Description("空料")]
        NG_EMPTY,
        //[Description("空料")]
        //INS_ALIGNERR = NG_EMPTY,

        [Description("切割尺寸 NG")]
        NG_CUT,
        //[Description("切割尺寸 NG")]
        //INS_CUTTINGERR = NG_CUT,

        [Description("外觀 NG)")]
        NG_APPEARANCE,
        //[Description("外觀 NG)")]
        //INS_DEFECTERR = NG_APPEARANCE,

        [Description("二維碼 讀取錯誤")]
        NG_QRCODE_ERR,
        //[Description("二維碼 讀取錯誤")]
        //INS_2DERR = NG_QRCODE_ERR,

        [Description("二維碼 比對錯誤")]
        NG_QRCODE_COMPARE,
        //[Description("二維碼 比對錯誤")]
        //INS_2DMAPNG = NG_QRCODE_COMPARE,

        //[Description("二維碼 重複")]             // 沒用到
        //NG_QRCODE_REPEATED,
        //[Description("二維碼 重複")]
        //INS_2DREPEATERR = NG_QRCODE_REPEATED,

        //[Description("不檢測")]                  // 沒用到
        //NG_BYPASS,
        //[Description("不檢測")]
        //INS_NOOPEN = NG_BYPASS,

        [Description("邊隙 (切割偏移) NG")]
        NG_EDGE_GAP,
        //[Description("邊隙 (切割偏移) NG")]
        //INS_PADEDGEGAPERR = NG_EDGE_GAP,

        [Description("不明區塊")]
        NG_AMBIGUOUS_BLOC,
    }
}
