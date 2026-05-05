using System;
using System.Threading;
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
            frmMain.Load += (s,e) => PostInit(s as Form);

            Application.Run(frmMain);
        }

        static void PostInit(Form frm)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                System.Threading.Thread.Sleep(1000);
                frm.BeginInvoke(new Action(() =>
                {
                    AdjustFormSize(frm);
                }));
                LoadLastImage();
            });
        }
        static void AdjustFormSize(Form frm)
        {
            // 調整主視窗大小和位置
            frm.Size = new System.Drawing.Size(1280, 1024);
            frm.WindowState = FormWindowState.Normal;
        }
        static void LoadLastImage()
        {
            var app = EzAppForDll.Instance;
            var appSettings = app.appSettings as JxAppSettings;
            if (appSettings == null)
                return;

            var imgFile = appSettings?.VisionSrc0?.ImgFile?.Value;
            if (string.IsNullOrEmpty(imgFile) || !System.IO.File.Exists(imgFile))
                return;

            var image = ImageUtil.LoadLargeImage(imgFile, fmt: System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            var fname = System.IO.Path.GetFileName(imgFile);
            AoiFactory.PushImage(image, fname);
        }
    }
}
