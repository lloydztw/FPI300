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
            Application.Run(frmMain);
        }
    }
}
