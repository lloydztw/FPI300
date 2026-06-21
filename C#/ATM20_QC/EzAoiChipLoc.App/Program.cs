using System;
using System.Threading;
using System.Windows.Forms;


namespace EzAoiChipLocQC
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var frmMain = AoiFactory.OpenChipLocQcTool();
            frmMain.Load += (s, e) => PostInit(s as Form);
            Application.Run(frmMain);

            //var app = new EzAppForMain();
            //Form frm = app.Build();
            //Application.Run(frm);
        }

        static void PostInit(Form frm)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                System.Threading.Thread.Sleep(1000);
                //frm.BeginInvoke(new Action(() =>
                //{
                //    AdjustFormSize(frm);
                //}));
                LoadLastImage();
            });
        }
        static void LoadLastImage()
        {
            var app = EzAppForDll.Instance;
            var appSettings = app.appSettings as JxAppSettings;
            if (appSettings == null)
                return;

            var imgFile = appSettings?.MiscSysSettings?.VisionSrc0?.ImgFile?.Value;
            if (string.IsNullOrEmpty(imgFile) || !System.IO.File.Exists(imgFile))
                return;

            var image = ImageUtil.LoadLargeImage(imgFile, fmt: System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            var fname = System.IO.Path.GetFileName(imgFile);
            AoiFactory.PushImage(image, fname);
        }
    }
}
