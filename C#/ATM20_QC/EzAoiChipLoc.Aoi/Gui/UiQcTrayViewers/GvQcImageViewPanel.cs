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

using AwFramework;
using AwFramework.Gui.JX.Gui;
using EzAoiChipLocQC.Ctrl;
using EzAoiChipLocQC.Gui.Util;
using JetEazy.ImageViewerEx;
using JetEazy.OpenCV;
using JetEazy.OpenCV.Viewer;
using System;
using System.Drawing;
using System.Linq.Expressions;
using System.Windows.Forms;
using CvzQuickImageViewPanel = JetEazy.OpenCV.Viewer.Develop.CvzQuickImageViewPanel;

namespace EzAoiChipLocQC.Gui.Panels
{
    public partial class GvQcImageViewPanel : UserControl, IvQcImageView
    {
        #region PRIVATE_DATA
        string _srcName = "";
        bool _isLiveCameraMode = false;
        #endregion

        public GvQcImageViewPanel()
        {
            InitializeComponent();
            initMenu();

            wndStillImageView.btnOpen.Visible = false;
            wndStillImageView.Dock = DockStyle.Fill;
            wndLiveCameraView.Dock = DockStyle.Fill;
            wndStillImageView.Visible = true;
            wndStillImageView.ImageBits = 8;

            HandleCreated += (s, e) => postInit();
        }

        #region GUI_LINKS
        Control IView.Window => this;
        JXImageViewer wndLiveCameraView => jxImageViewer1;
        CvzQuickImageViewPanel wndStillImageView => cvzQuickImageViewPanel1;
        CvMatViewer cvMatViewer => wndStillImageView.MatViewer;
        IvImageViewer IvQcImageView.ImageViewer => cvMatViewer;
        #endregion

        void initMenu()
        {
            var menuStrip = contextMenuStrip1;
            wndStillImageView.AttachPopupMenu(contextMenuStrip1);
            menuLoadImage.Click += (s, e) => browseStillImage();
            menuSnapshot.Click += (s, e) => snapshotOneImage();
            menuLiveMode.Click += (s, e) => switchToLiveCameraView(true);
        }
        void popupMenu()
        {
            // 取得滑鼠在螢幕上的當前位置
            var screenPos = Cursor.Position;
            // 顯示 ContextMenuStrip
            // Show() 方法的第一個參數是關聯的控制項 (這裡就是 picBox)
            // 第二個參數是相對於螢幕的座標
            contextMenuStrip1.Show(screenPos);
        }
        void postInit()
        {
            new Action(() =>
            {
                // 讓相機備妥
                var camera = wndLiveCameraView.Camera;
                if (camera == null)
                {
                    System.Threading.Thread.Sleep(3000);
                    BeginInvoke((Action<bool>)switchToLiveCameraView, true);
                    BeginInvoke((Action)switchToStillImageView);
                }

                // 攔截 wndLiveCameraView 的按鈕
                //wndLiveCameraView.picIcon.Click += PicIcon_Click;
                ControlExtensions.CopyMouseClickEvent(
                    wndStillImageView.picIcon,
                    wndLiveCameraView.picIcon
                );
            }).BeginInvoke(null, null);
        }

        #region EVENT_HANDLERS
        private void PicIcon_Click(object sender, EventArgs e)
        {
            popupMenu();
        }
        #endregion

        void IvQcImageView.UpdateImageSrcName(string srcName)
        {
            if (srcName == null)
                srcName = "";
            updateTitleText(_srcName = srcName, Color.White);
            switchToStillImageView();
        }
        void IvQcImageView.UpdateStatusInfo(string msg)
        {
            updateTitleText(msg, Color.White);
        }
        void IvQcImageView.UpdateRunState(object state)
        {
            if (state != null)
            {
                string txt = (string)state;
                if (string.IsNullOrEmpty(txt))
                {
                    updateTitleText(_srcName, Color.White);
                }
                else if (txt == "Ready")
                {
                    //txt = "[Ready] " + _srcName;
                    updateTitleText(_srcName, Color.White);
                }
                else
                {
                    updateTitleText(txt, txt.Contains("Error") ? Color.Red : Color.White);
                }
            }
        }
        void updateTitleText(string txt, Color color)
        {
            if (InvokeRequired)
            {
                Invoke((Action<string, Color>)updateTitleText, txt, color);
            }
            else
            {
                wndStillImageView.UpdateTitleBarText(txt, color);
                //jxImageViewer.lblTitle.Text = txt;
                //jxImageViewer.lblTitle.ForeColor = color;
            }
        }

        void switchToLiveCameraView(bool startLive = true)
        {
            _isLiveCameraMode = true;
            wndLiveCameraView.Visible = true;
            wndStillImageView.Visible = false;
            
            if (startLive) wndLiveCameraView.Camera?.StartLiveMode();
            BeginInvoke(new Action(() =>
            {
                wndLiveCameraView.btnOpen.Visible = false;
            }));
        }
        void switchToStillImageView()
        {
            _isLiveCameraMode = false;
            wndLiveCameraView.Visible = false;
            wndStillImageView.Visible = true;
            wndLiveCameraView.Camera?.StopLiveMode();
        }

        async void browseStillImage()
        {
            switchToStillImageView();
            await wndStillImageView.BrowseFile();
        }
        void snapshotOneImage()
        {
            var camera = wndLiveCameraView.Camera;
            if (camera == null)
            {
                MessageBox.Show("相機尚未備妥!", FindForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //new Action(() =>
                //{
                //    BeginInvoke((Action<bool>)switchToLiveCameraView, true);
                //    System.Threading.Thread.Sleep(3000);
                //    BeginInvoke((Action)snapshotOneImage);
                //}).BeginInvoke(null, null);
                return;
            }

            switchToStillImageView();
            using (var bmp = wndLiveCameraView.Camera.Snapshot())
            using (var bridge = new QxImageBridge(bmp))
            {
                //NOTE: wndStillImageView (cvMatView) 會接管 img 的生命週期 !!!
                var img = bridge.Image.Clone();
                wndStillImageView.Attach(img);
            }
        }
    }
}
