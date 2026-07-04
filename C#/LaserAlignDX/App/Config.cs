
namespace LaserAlignDX
{
    public class GlobalConfig
    {
        public static string TITLE => "Traveller FPI30";

        public static string APP_ROOT_PATH => "D:\\AUTOMATION\\Eazy FPI30";

        /// <summary>
        /// 默認值是 false, 由主程式的 args 引數 來決定是否啟用 SIM 模式
        /// </summary>
        public static bool IsSim { get; set; } = false;

        public static string VersionDate => "2026-07-04";

        public static readonly bool N_THREADS_ENABLED = true;
        public static readonly int N_THREADS = 16;

        /// <summary>
        /// 是否採用 平均邊隙 (從8個獨立數值 變成 4個有效數值) 
        /// </summary>
        public static readonly bool OPT_USING_GAPS_4 = true;
        public static readonly int TOTAL_MOTORS_NUMBER = 16;
    }
}
