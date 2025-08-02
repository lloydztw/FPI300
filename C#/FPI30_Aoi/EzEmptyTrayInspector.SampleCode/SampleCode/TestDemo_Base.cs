using JetEazy.OpenCV;
using JetEazy.OpenCV.Viewer;
using OpenCvSharp;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using JetEazy.EzImage;


namespace EzDualMatch.Test
{
    class TestDemoBase
    {
        protected string _recipeFileName = Path.Combine(Global.AppPath("Recipes"), "v_003.json");
        protected string _imgFileA = "D:\\Lloydz\\ML\\Data\\LargeImages\\v003\\001.jpg";
        protected string _imgFileB = "D:\\Lloydz\\ML\\Data\\LargeImages\\v003\\002.jpg";
        protected string _dumpPath = Global.AppPath("Work", "Dump");
        protected bool _isDump = true;

        protected void _ASSERT_PATH_FILES()
        {
            System.Diagnostics.Trace.Assert(System.IO.File.Exists(_recipeFileName), $"參數檔不存在: {_recipeFileName}");
            System.Diagnostics.Trace.Assert(System.IO.File.Exists(_imgFileA), $"影像檔不存在: {_imgFileA}");
            System.Diagnostics.Trace.Assert(System.IO.File.Exists(_imgFileB), $"影像檔不存在: {_imgFileB}");
        }
        protected void _TRACE(string msg)
        {
            Console.WriteLine(msg);
        }
        
        protected void _SHOW_IMAGE(IEzImage img, string title)
        {
            _SHOW_IMAGE(img?.Image as Mat, title);
        }
        protected void _SHOW_IMAGE(Bitmap bmp, string title)
        {
            if (bmp == null)
            {
                _TRACE("_SHOW_IMAGE(bmp == null) !");
                return;
            }
            using(var bridge = new QxImageBridge(bmp))
            {
                _SHOW_IMAGE(bridge.Image, title);
            }
        }
        protected void _SHOW_IMAGE(Mat img, string title)
        {
            if (img == null)
            {
                _TRACE("_SHOW_IMAGE(img == null) !");
                return;
            }

            using (var frm = new Form())
            {
                frm.Text = title;
                var panel = new CvzQuickImageViewPanel();
                panel.OptAutoPersistLastFile = false;
                panel.OptTitleBarVisible = false;
                panel.OptCoordInfoVisible = true;
                frm.Controls.Add(panel);
                panel.Dock = DockStyle.Fill;
                panel.ImageViewer.Image = img;
                panel.Visible = true;
                frm.Size = new System.Drawing.Size(640, 480);
                frm.ShowDialog();
            }
        }
    }
}
