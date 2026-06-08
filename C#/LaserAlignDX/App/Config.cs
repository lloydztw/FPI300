
namespace LaserAlignDX
{
    public class GlobalConfig
    {
        public static bool IsSim => false;

        public static string VersionDate => "2026-06-08";

        public static readonly bool N_THREADS_ENABLED = true;
        public static readonly int N_THREADS = 16;

        /// <summary>
        /// 是否採用 平均邊隙 (從8個獨立數值 變成 4個有效數值) 
        /// </summary>
        public static readonly bool OPT_USING_GAPS_4 = true;
        public static readonly int TOTAL_MOTORS_NUMBER = 16;
    }
}
