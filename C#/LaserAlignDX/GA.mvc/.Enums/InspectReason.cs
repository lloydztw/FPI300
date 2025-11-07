using System.ComponentModel;


namespace LaserAlignDX.OPSpace
{
    public enum InspectReason : int
    {
        [Description("PASS")]
        PASS = 0,

        [Description("空料")]
        NG_EMPTY,
        INS_ALIGNERR = NG_EMPTY,

        [Description("切割 NG")]
        NG_CUT,
        INS_CUTTINGERR = NG_CUT,

        [Description("外觀 NG)")]
        NG_APPEARANCE,
        INS_DEFECTERR = NG_APPEARANCE,

        [Description("二維碼 讀取錯誤")]
        NG_QRCODE_ERR,
        INS_2DERR = NG_QRCODE_ERR,

        [Description("二維碼 比對錯誤")]
        NG_QRCODE_COMPARE,
        INS_2DMAPNG = NG_QRCODE_ERR,

        [Description("二維碼 重複")]             // 沒用到
        NG_QRCODE_REPEATED,
        INS_2DREPEATERR = NG_QRCODE_REPEATED,

        [Description("不檢測")]                  // 沒用到
        NG_BYPASS,
        INS_NOOPEN = NG_BYPASS,

        [Description("切割偏移 NG (邊隙 NG)")]
        NG_EDGE_GAP,
        INS_PADEDGEGAPERR = NG_EDGE_GAP,
    }
}
