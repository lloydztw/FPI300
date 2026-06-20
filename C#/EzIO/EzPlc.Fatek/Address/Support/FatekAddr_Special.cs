#region AUTHOR
/*
 * EzPlc.Fatek
 * Copyright (C) 2026
 * 2026-04-05 revised by LeTian Chang
 * 2013-07-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


namespace EzPlc.Fatek
{
    using ADDR = FatekAddr;

    public partial class FatekAddr
    {
        /// <summary>
        /// 0.01 秒週期脈波
        /// </summary>
        public static ADDR M1920 { get { return M(1920, "0.01 秒週期脈波"); } }
        /// <summary>
        /// 0.1 秒週期脈波
        /// </summary>
        public static ADDR M1921 { get { return M(1921, "0.1 秒週期脈波"); } }
        /// <summary>
        /// 1 秒週期脈波
        /// </summary>
        public static ADDR M1922 { get { return M(1922, "1 秒週期脈波"); } }
        /// <summary>
        /// 60 秒週期脈波
        /// </summary>
        public static ADDR M1923 { get { return M(1923, "60 秒週期脈波"); } }
        /// <summary>
        /// 啟始（第一次掃描）脈波
        /// </summary>
        public static ADDR M1924 { get { return M(1924, "啟始（第一次掃描）脈波"); } }
        /// <summary>
        /// 掃描週期脈波
        /// </summary>
        public static ADDR M1925 { get { return M(1925, "掃描週期脈波"); } }
        /// <summary>
        /// PLC 工作模式: (1=RUN / 0=STOP) 
        /// </summary>
        public static ADDR M1926 { get { return M(1926, "工作模式 (1=RUN / 0=STOP)"); } }

        public static ADDR M(int address, string description = null)
        {
            return new ADDR($"M{address}", description);
        }
        public static ADDR R(int address, string description = null)
        {
            return new ADDR($"R{address}", description);
        }
        public static ADDR D(int address, string description = null)
        {
            return new ADDR($"D{address}", description);
        }
        public static ADDR X(int address, string description = null)
        {
            return new ADDR($"X{address}", description);
        }
        public static ADDR Y(int address, string description = null)
        {
            return new ADDR($"Y{address}", description);
        }
    }
}
