#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using JetEazy.Lang;
using JetEazy.Utils;
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

            //設定語系 json 檔 的資料夾
            QMSG.Path = System.IO.Path.Combine(GlobalConfig.APP_ROOT_PATH, "Ini", "language");
            //通用的 Title
            QMSG.Title = GlobalConfig.TITLE;

            using (var app = AppBootstrapper.Instance)
            {
                // 解析引數 (args)
                bool go = parse_args(args);
                if (!go)
                    return;

                if (AppInstance())
                {
                    var msg = GaUtil.GetEnumDescription(Prompts.ReEntry);
                    MessageBox.Show(msg, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                LtAoiFactory.Migrate();

                using (Form frm = app.CreateMainForm())
                {
                    #region DEBUG_CODE
                    _DEBUG(frm);
                    #endregion

                    Application.Run(frm);
                }
            }
        }

        static bool AppInstance()
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
        
        static bool parse_args(params string[] args)
        {
            bool go = true;

            GlobalConfig.ParseArgs(args);

            if (Array.IndexOf(args, "TEST") >= 0)
            {
                if (Array.IndexOf(args, "OMRON") >= 0)
                {
                    run_unit_test_omron();
                    go = false;
                }
                else
                {
                    if (run_unit_test_others())
                        go = false;
                }
            }

            return go;
        }
        static void run_unit_test_omron()
        {
            var test = new UnitTest_FP130.Test_OmronPlc();
            test.Run();
        }
        static bool run_unit_test_others()
        {
            return false;
            using (var frm = new LaserAlignDX.Mvc.Gui.V35.FormTemplateEditor())
            {
                frm.StartPosition = FormStartPosition.CenterScreen;
                frm.ShowDialog();
                return true;
            }
            using (var dlg = new LaserAlignDX.Mvc.Gui.FormMotors_CarierSuckerXY())
            {
                dlg.ShowDialog();
                return true;
            }
            new UnitTest_FP130.Test_ChipMatcher().Run();
            return true;
        }
        static void _DEBUG(Form frm)
        {
            return;

            frm.Load += (s, e) =>
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Thread.Sleep(3000);
                    frm.BeginInvoke(new Action(() =>
                    {
                        //GaMvcConfig.OpenTamplateEditor(Model.Coords.CarrierEnum.C2)
                        //GaMvcConfig.OpenCalibrationTool();
                        GaMvcConfig.OpenRecipeEditor();
                    }));
                });
            };
        }
    }
}
