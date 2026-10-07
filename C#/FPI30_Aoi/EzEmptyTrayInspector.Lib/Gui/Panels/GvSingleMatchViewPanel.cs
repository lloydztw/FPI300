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
using EzAoiEmptyTrayInspector.Model;
using JetEazy.ImageViewerEx;
using JetEazy.OpenCV.Viewer;
using OpenCvSharp;
using System;
using System.Drawing;
using System.Windows.Forms;


namespace EzAoiEmptyTrayInspector.Gui.Panels
{
    public partial class GvSingleMatchViewPanel : UserControl, IvSingleMatchView
    {
        #region PRIVATE_DATA
        string _srcName = "";
        #endregion

        public GvSingleMatchViewPanel()
        {
            InitializeComponent();
            cvMatViewer1.Attach(lblCoordInfo, lblBlinker);
        }
        
        public int ViewID
        {
            get;
            internal set;
        }

        Control IView.Window => this;
        CvMatViewer MatViewer => cvMatViewer1;
        IvImageViewer IvSingleMatchView.ImageViewer => cvMatViewer1;
        
        public void UpdateImage(Mat srcImg, string srcName, bool disposeSrc)
        {
            if (InvokeRequired)
            {
                Invoke((Action<Mat, string, bool>)UpdateImage, srcImg, srcName, disposeSrc);
            }
            else
            {
                lblTitle.Text = srcName;

                if (srcImg != null)
                {
                    MatViewer.CopyFrom(srcImg);
                    if (disposeSrc)
                        srcImg.Dispose();
                }
            }
        }
        void IvSingleMatchView.UpdateMatchState(object state)
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
        void IvSingleMatchView.UpdateImageSrcName(string srcName)
        {
            //_srcName = srcName != null ? System.IO.Path.GetFileName(srcName) : "";
            //lblTitle.Text = _srcName;
            //lblTitle.ForeColor = Color.White;
            //lblTitle.Refresh();
            if (srcName == null)
                srcName = "";
            updateTitleText(_srcName = srcName, Color.White);
        }
        void IvSingleMatchView.UpdateStatusInfo(string msg)
        {
            updateTitleText(msg, Color.White);
        }
        void updateTitleText(string txt, Color color)
        {
            if (InvokeRequired)
            {
                Invoke((Action<string, Color>)updateTitleText, txt, color);
            }
            else
            {
                lblTitle.Text = txt;
                lblTitle.ForeColor = color;
                lblTitle.Refresh();
            }
        }
    }
}
