
using System;

namespace LaserAlignDX
{
    public class GlobalConfig
    {
        public static string TITLE => "Traveller FPI30";

        public static string APP_ROOT_PATH => "D:\\AUTOMATION\\Eazy FPI30";

        public static string VersionDate => "2026-09-09";

        /// <summary>
        /// 默認值是 false, 由主程式的 args 引數 來決定是否啟用 SIM 模式
        /// </summary>
        public static bool IsSim { get; private set; } = false;

        /// <summary>
        /// 默認值是 true, 由主程式的 args 引數 來決定 多線呈 模式
        /// </summary>
        public static bool N_THREADS_ENABLED { get; private set; } = true;

        public static readonly int N_THREADS = 16;

        /// <summary>
        /// 是否採用 平均邊隙 (從8個獨立數值 變成 4個有效數值) 
        /// </summary>
        public static readonly bool OPT_USING_GAPS_4 = true;

        public static readonly int TOTAL_MOTORS_NUMBER = 16;

        public static void ParseArgs(params string[] args)
        {
            if (Array.IndexOf(args, "SIM") >= 0)
                IsSim = true;

            if (Array.IndexOf(args, "SINGLE_THREAD") >= 0)
                N_THREADS_ENABLED = false;
        }
    }
}
