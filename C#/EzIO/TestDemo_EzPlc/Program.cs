using System;
using System.Windows.Forms;
using TestDemo_EzPLC.Gui;

namespace TestDemo_EzPLC
{
    internal static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                int demoOption = QueryDemoOptions(out int portNumber, out bool isSim);
                if (demoOption == 0)
                {
                    var app = new DemoApp0();
                    var frm = app.BuildFatekDemo(portNumber, isSim);
                    Application.Run(frm);
                }
                else if(demoOption == 1)
                {
                    var app = new DemoApp1();
                    var frm = app.BuildFatekDemo(portNumber, isSim);
                    Application.Run(frm);
                }
                else if(demoOption == 2)
                {
                    var app = new DemoApp2();
                    var frm = app.BuildHcfaDemo(portNumber, isSim);
                    Application.Run(frm);
                }
                else
                {
                    var app = new DemoApp3();
                    var frm = app.BuildOmronDemo(portNumber, isSim);
                    Application.Run(frm);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "異常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        static int QueryDemoOptions(out int portNumber, out bool isSim)
        {
            using (var dlg = new FormDemoOptions())
            {
                dlg.ShowDialog();
                isSim = dlg.IsSim;
                portNumber = dlg.PortNumber;
                return dlg.DemoOption;
            }
        }
    }
}
