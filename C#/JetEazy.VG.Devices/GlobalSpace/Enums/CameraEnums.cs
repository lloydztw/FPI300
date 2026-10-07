using System.ComponentModel;

namespace JetEazy
{
    public enum CCDTYPEEnum
    {
        EPIX,
        TIS,
        TISUSB,
        IDS,
        PTG,
        FILE,
        AISYS,
        ICAM,
        IWIN,
        MVS,
        HIK,
        USBCAM,
    }

    /// <summary>
    /// CCD属性
    /// </summary>
    public enum CCDProcAmpProperty
    {
        /// <summary>
        /// 亮度
        /// </summary>
        Brightness = 0,
        /// <summary>
        /// 对比度
        /// </summary>
        Contrast = 1,
        /// <summary>
        /// 色调
        /// </summary>
        Hue = 2,
        /// <summary>
        /// 饱和度
        /// </summary>
        Saturation = 3,
        /// <summary>
        /// 清晰度
        /// </summary>
        Sharpness = 4,
        /// <summary>
        /// 锐度
        /// </summary>
        Gamma = 5,
        /// <summary>
        /// 色彩深度
        /// </summary>
        ColorEnable = 6,
        /// <summary>
        /// 白平衡
        /// </summary>
        WhiteBalance = 7,
        /// <summary>
        /// 弱光补偿
        /// </summary>
        BacklightCompensation = 8,
        /// <summary>
        /// Gain值
        /// </summary>
        Gain = 9
    }

    public enum LinescanTypeEnum : int
    {
        [Description("华睿线扫")]
        HUARUI = 0,
        [Description("度申线扫")]
        DVP2 = 1,
        [Description("埃科线扫")]
        ITK = 2,
        [Description("迈德威视")]
        MIND = 3,
    }
}