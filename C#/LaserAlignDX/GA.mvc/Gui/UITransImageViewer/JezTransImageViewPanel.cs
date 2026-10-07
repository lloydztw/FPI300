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
using JetEazy.Lang;
using JetEazy.OpenCV;
using JetEazy.OpenCV.Viewer;
using JetEazy.Transform;
using OpenCvSharp;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class JezTransImageViewPanel : UserControl
    {
        #region PRIVATE_DATA
        /// <summary>
        /// ContextMenuStrip 的語系轉換, 放在 PicIcon_Click 處理.
        /// 利用 JetEazy.Lang.EzContextMenuTranslator.Translate(_contextMenuStrip);
        /// </summary>
        ContextMenuStrip _contextMenuStrip = null;
        CviTransCoordInfo _cviCoordInfo = new CviTransCoordInfo();
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
            //HandleCreated += (s, e) => PostInitLaguage();
        }
        void cleanUp()
        {
            try
            {
                var old = cvMatViewer.Image;
                cvMatViewer.Image = null;
                old?.Dispose();
            }
            catch(Exception ex) 
            { 
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        #region EVENT_HANDLERS
        private void PicIcon_Click(object sender, EventArgs e)
        {
            if (_contextMenuStrip != null)
            {
                // 取得滑鼠在螢幕上的當前位置
                var screenPos = Cursor.Position;

                // 多語系轉換
                EzContextMenuTranslator.Translate(_contextMenuStrip);

                // 顯示 ContextMenuStrip
                // Show() 方法的第一個參數是關聯的控制項 (這裡就是 picBox)
                // 第二個參數是相對於螢幕的座標
                _contextMenuStrip.Show(screenPos);
            }
        }
        #endregion

        public IvImageViewer ImgViewer
        {
            get => cvMatViewer;
        }
        public CvMatViewer MatViewer
        {
            get => cvMatViewer;
        }

        public Control TitleBar => panel1;
        public Control StatusBar => panel2;

        public Mat Image
        {
            get => cvMatViewer.Image;
        }
        public void UpdateImage(Bitmap srcBmp, string srcName, bool disposeSrc)
        {
            if (InvokeRequired)
            {
                Invoke((Action<Bitmap, string, bool>)UpdateImage, srcBmp, srcName, disposeSrc);
            }
            else
            {
                lblTitle.Text = srcName;
                if (srcBmp != null)
                {
                    var old = cvMatViewer.Image;
                    using (var bridge = new QxImageBridge(srcBmp))
                    {
                        cvMatViewer.Image = bridge.Image.Clone();
                    }
                    old?.Dispose();
                    if (disposeSrc)
                    {
                        srcBmp.Dispose();
                    }
                }
            }
        }

        public void SetTransform(ITransform trf)
        {
            _cviCoordInfo.Transform = trf;
        }
        public void AttachPopupMenu(ContextMenuStrip contextMenuStrip)
        {
            _contextMenuStrip = contextMenuStrip;
            if (_contextMenuStrip == null)
                return;

            // ContextMenuStrip 的語系轉換, 放在 PicIcon_Click 處理.
            // 利用 JetEazy.Lang.EzContextMenuTranslator.Translate(_contextMenuStrip);

#if (OPT_RESERVED)
            if (IsHandleCreated)
            {
                BeginInvoke((Action)TranslateContextMenu);
            }
            else
            {
                HandleCreated += (s, e) => TranslateContextMenu();
            }
#endif
        }

#if (OPT_RESERVED)
        void PostInitLaguage()
        {
            //QxLang.Instance("gui").LanguageChanged += (s, e) => TranslateContextMenu();
        }
        void TranslateContextMenu()
        {
            //if (_contextMenuStrip != null)
            //    EzContextMenuTranslator.Translate(_contextMenuStrip);
        }
#endif
    }
}
