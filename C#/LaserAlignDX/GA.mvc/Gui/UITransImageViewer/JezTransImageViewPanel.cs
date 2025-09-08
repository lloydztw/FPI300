#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.ImageViewerEx;
using JetEazy.OpenCV.Viewer;
using JetEazy.Transform;
using System;
using System.Windows.Forms;


namespace LaserAlignDX.Mvc.Gui
{
    public partial class JezTransImageViewPanel : UserControl
    {
        #region PRIVATE_DATA
        CviTransCoordInfo _cviCoordInfo = new CviTransCoordInfo();
        ContextMenuStrip _contextMenuStrip = null;
        #endregion

        public JezTransImageViewPanel()
        {
            InitializeComponent();
            cvMatViewer.Attach(lblShadowInfo, lblBlinker);
            cvMatViewer.AddInteractor(_cviCoordInfo);
            _cviCoordInfo.Attach(lblCoordInfo, lblShadowInfo);
            _cviCoordInfo.Enabled = true;
            _cviCoordInfo.Visible = true;
            picIcon.Click += PicIcon_Click;
        }

        private void PicIcon_Click(object sender, EventArgs e)
        {
            if (_contextMenuStrip != null)
            {
                // 取得滑鼠在螢幕上的當前位置
                var screenPos = Cursor.Position;

                // 顯示 ContextMenuStrip
                // Show() 方法的第一個參數是關聯的控制項 (這裡就是 picBox)
                // 第二個參數是相對於螢幕的座標
                _contextMenuStrip.Show(screenPos);
            }
        }

        public IvImageViewer ImgViewer
        {
            get => cvMatViewer;
        }

        public CvMatViewer MatViewer
        {
            get => cvMatViewer;
        }

        public void SetTransform(ITransform trf)
        {
            _cviCoordInfo.Transform = trf;
        }

        public void AttachPopupMenu(ContextMenuStrip contextMenuStrip)
        {
            _contextMenuStrip = contextMenuStrip;
        }
    }
}
