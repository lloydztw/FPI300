#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-05-06 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector.Gui.Panels;
using OpenCvSharp;
using System.Drawing;
using System.Windows.Forms;

namespace EzAoiEmptyTrayInspector.Gui
{
    public partial class FormImageViewer : Form
    {
        #region PRIVATE_GUI_LINKS
        JezImageViewPanel _imagePanel => jezImageViewPanel1;
        #endregion

        public FormImageViewer()
        {
            InitializeComponent();
        }

        public void UpdateImage(Bitmap srcBmp, string srcName, bool disposeSrc)
        {
            _imagePanel.UpdateImage(srcBmp, srcName, disposeSrc);
        }

        public void UpdateImage(Mat srcImg, string srcName, bool disposeSrc)
        {
            _imagePanel.UpdateImage(srcImg, srcName, disposeSrc);
        }
    }
}
