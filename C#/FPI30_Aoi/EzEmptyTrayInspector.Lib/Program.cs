using System;
using System.Windows.Forms;

namespace EzDualMatch
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var app = EzDualMatchApp.Instance;
            Form frmMain = app.Build();
            Application.Run(frmMain);
        }
    }
}
