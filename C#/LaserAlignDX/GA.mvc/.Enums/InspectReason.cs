using System.ComponentModel;


namespace LaserAlignDX.OPSpace
{
    public enum InspectReason : int
    {
        [Description("PASS")]
        PASS = 0,
        /// <summary>
        /// 空料
        /// </summary>
        [Description("空料")]
        //[Description("印字错误")]
        INS_ALIGNERR = 1,
        [Description("切割NG")]
        INS_CUTTINGERR = 2,
        /// <summary>
        /// 疑似有料及外观NG
        /// </summary>
        [Description("疑似有料")]
        //[Description("印字缺失")]
        INS_DEFECTERR = 3,
        /// <summary>
        /// 2D读取错误
        /// </summary>
        [Description("2D读取错误")]
        INS_2DERR = 4,
        /// <summary>
        /// 2D比对错误
        /// </summary>
        [Description("2D比对错误")]
        INS_2DMAPNG = 5,
        [Description("2D码重复")]
        INS_2DREPEATERR = 6,
        [Description("不检测")]
        INS_NOOPEN = 7,
        /// <summary>
        /// 切割偏移NG
        /// </summary>
        [Description("切割偏移NG")]
        INS_PADEDGEGAPERR = 8,
    }
}
