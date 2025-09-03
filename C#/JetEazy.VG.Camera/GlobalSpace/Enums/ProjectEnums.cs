namespace JetEazy
{
    public enum VersionEnum : int
    {
        BRONTES = 0,
        STEROPES = 1,
        ARGES = 2,

        KBAOI = 3,
        CNAOI = 4,
        HEIGHTMEASURE = 5,
        MEASURE2D = 6,

        ALLINONE = 7,

        AUDIX = 8,

        PROJECT = 9,
        TRAVELLER = 10,
        /// <summary>
        /// 德龙镭射
        /// </summary>
        LASER = 11,
        /// <summary>
        /// 线扫AOI  Create 20250516
        /// </summary>
        AOI = 12,
    }

    public enum OptionEnum : int
    {
        MAIN = 0,
        DISPENSING = 1,
        DISPENSINGX1 = 2,
        DISPENSINGX2 = 3,
        DISPENSINGX3 = 4,
        DISPENSINGX4 = 5,
        MAIN_X6 = 6,
        MAIN_LS = 7,
        MAIN_NEEDLE = 8,
        MAIN_MINIX6 = 9,
        /// <summary>
        /// 镭雕引导定位
        /// </summary>
        MAIN_X1 = 10,
        /// <summary>
        /// 线扫AOI
        /// </summary>
        MAIN_X2 = 11,
        /// <summary>
        /// FPI 线扫+飞拍
        /// </summary>
        MAIN_FPIX3 = 12,
    }
}
