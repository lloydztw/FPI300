#region AUTHOR
/*
 * EzIO.Mem
 * Copyright (C) 2023
 * 2013-07-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using EzIO.Mem;

namespace EzPlc.Fatek.Comm
{
    /// <summary>
    /// FATEK 系統特殊點位: M1920 ~ M1926
    /// </summary>
    public enum FatekSysPointEnum : int
    {
        /// <summary>
        /// 0.01 秒週期脈波
        /// </summary>
        [EzPointEnumAttribute("0.01 秒週期脈波", "M1920", autoScan: true)]
        M1920 = 1920,
        /// <summary>
        /// 0.1 秒週期脈波
        /// </summary>
        [EzPointEnumAttribute("0.1 秒週期脈波", "M1921", autoScan: true)]
        M1921,
        /// <summary>
        /// 1 秒週期脈波
        /// </summary>
        [EzPointEnumAttribute("1 秒週期脈波", "M1922", autoScan: true)]
        M1922,
        /// <summary>
        /// 60 秒週期脈波
        /// </summary>
        [EzPointEnumAttribute("60 秒週期脈波", "M1923", autoScan: true)]
        M1923,
        /// <summary>
        /// 啟始（第一次掃描）脈波
        /// </summary>
        [EzPointEnumAttribute("啟始脈波", "M1924", autoScan: true)]
        M1924,
        /// <summary>
        /// 掃描週期脈波
        /// </summary>
        [EzPointEnumAttribute("掃描週期脈波", "M1925", autoScan: true)]
        M1925,
        /// <summary>
        /// PLC 工作模式: (1=RUN / 0=STOP) 
        /// </summary>
        [EzPointEnumAttribute("PLC 工作模式", "M1926", autoScan: true)]
        M1926,
    }
}
