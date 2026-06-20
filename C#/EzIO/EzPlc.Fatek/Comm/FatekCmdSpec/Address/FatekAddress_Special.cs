
namespace EzPlc.Fatek.Comm
{
    partial class FatekAddress
    {
        /// <summary>
        /// 0.01 秒週期脈波
        /// </summary>
        public static FatekAddress M1920 { get { return M(1920); } }
        /// <summary>
        /// 0.1 秒週期脈波
        /// </summary>
        public static FatekAddress M1921 { get { return M(1921); } }
        /// <summary>
        /// 1 秒週期脈波
        /// </summary>
        public static FatekAddress M1922 { get { return M(1922); } }
        /// <summary>
        /// 60 秒週期脈波
        /// </summary>
        public static FatekAddress M1923 { get { return M(1923); } }
        /// <summary>
        /// 啟始（第一次掃描）脈波
        /// </summary>
        public static FatekAddress M1924 { get { return M(1924); } }
        /// <summary>
        /// 掃描週期脈波
        /// </summary>
        public static FatekAddress M1925 { get { return M(1925); } }
        /// <summary>
        /// PLC 工作模式: (1=RUN / 0=STOP) 
        /// </summary>
        public static FatekAddress M1926 { get { return M(1926); } }


        /// <summary>
        /// TESTING
        /// </summary>
        static void selfTest()
        {
            FatekAddress addr;
            System.Console.WriteLine(addr = M(100));
            System.Console.WriteLine(addr = new FatekAddress("M", 100));
            System.Console.WriteLine(addr = new FatekAddress("WM", 128));
            System.Console.WriteLine(addr = new FatekAddress("DWM", 256));
            System.Console.WriteLine(addr = DWM(1000));
        }
    }
}
