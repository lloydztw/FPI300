using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace LaserAlignDX
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 只跑單元測試
            if (run_unit_tests())
                return;

            if (AppInstance())
            {
                MessageBox.Show("程序已启动，请勿多开！！！", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //Application.Run(new frmMain());
            Application.Run(new Traveller106.frmMainDX());
        }

        public static bool AppInstance()
        {
            Process[] MyProcesses = Process.GetProcesses();
            int i = 0;
            foreach (Process MyProcess in MyProcesses)
            {
                if (MyProcess.ProcessName == Process.GetCurrentProcess().ProcessName)
                {
                    i++;
                }
            }
            return (i > 1) ? true : false;
        }

        static bool run_unit_tests()
        {
            //LeTian.Match.Test.Run();
            //return true;
            return false;
        }
    }
}
