using System;
using System.Windows.Forms;


namespace EzAoiEmptyTrayInspector
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AoiMigration.Check();

            var frmMain = AoiFactory.OpenEmptyTrayInspectorTool();
            postAdjustSize(frmMain, 3000);

            Application.Run(frmMain);
        }
        static void postAdjustSize(Form frmMain, int delay)
        {
            var a = new Action<Form, int>((frm, d) =>
            {
                System.Threading.Thread.Sleep(d);
                frm.BeginInvoke(new Action(() =>
                {
                    frm.Size = new System.Drawing.Size(1280, 1024);
                    frm.WindowState = FormWindowState.Normal;
                }));
            });
            a.BeginInvoke(frmMain, delay, null, null);
        }
    }
}
