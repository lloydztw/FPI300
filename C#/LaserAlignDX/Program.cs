using System;
using System.Diagnostics;
using System.Windows.Forms;
using Traveller106;

namespace LaserAlignDX
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(params string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LtAoiFactory.Migrate();

            // 只跑單元測試
            if (run_unit_tests(args))
                return;

            if (AppInstance())
            {
                MessageBox.Show("程序已启动，请勿多开！！！", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Form frm = new Traveller106.FormMainDX();

            //frm.Load += (s, e) => GaMvcConfig.OpenCalibrationTool();
            //frm.Load += (s, e) => GaMvcConfig.OpenTamplateEditor(Model.Coords.CarrierEnum.C2);

            Application.Run(frm);
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

        static bool run_unit_tests(params string[] args)
        {
            if (Array.IndexOf(args, "TEST") >= 0)
            {
                if (Array.IndexOf(args, "OMRON") >= 0)
                {
                    run_unit_test_omron();
                    return true;
                }
                else
                {
                    return run_unit_test_others();
                }
            }
            return false;
        }
        static void run_unit_test_omron()
        {
            var test = new UnitTest_FP130.Test_OmronPlc();
            test.Run();
        }
        static bool run_unit_test_others()
        {
            return false;
            using (var dlg = new LaserAlignDX.Mvc.Gui.FormMotors_CarierSuckerXY())
            {
                dlg.ShowDialog();
                return true;
            }
            new UnitTest_FP130.Test_ChipMatcher().Run();
            return true;
        }
    }
}
