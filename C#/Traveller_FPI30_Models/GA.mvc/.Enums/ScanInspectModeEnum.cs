using System.ComponentModel;


namespace LaserAlignDX.AoiModel
{
    public enum ScanInspectMode : int
    {
        [Description("外观及尺寸检测")]
        MEASUREAOI = 0,
        [Description("QR检测")]
        QRCODE = 1,
        [Description("空载台检测")]
        NOTRAY = 2,
    }
}
