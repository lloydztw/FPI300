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
using JetEazy.ImageViewerEx;
using JetEazy.OpenCV.Viewer;
using System;
using System.Drawing;
using System.Windows.Forms;


namespace EzAoiChipLocQC.Gui.Panels
{
    public partial class GvCommonImageViewPanel : UserControl, IvCommonImageView
    {
        #region PRIVATE_DATA
        string _srcName = "";
        #endregion

        public GvCommonImageViewPanel()
        {
            InitializeComponent();
            if (!DesignMode)
                cvMatViewer1.Attach(lblCoordInfo, lblBlinker);
        }
        
        public int ViewID
        {
            get;
            internal set;
        }

        //Form IView.frmOwner => FindForm();
        Control IView.Window => this;
        CvzQuickImageViewPanel IvCommonImageView.quickImageViewPanel => null;
        IvImageViewer IvCommonImageView.ImageViewer => cvMatViewer1;

        //Button IvSingleMatchView.btnOpenFile => btnOpen;
        //Button IvSingleMatchView.btnRunMatch => btnMatch;
        //Button IvSingleMatchView.btnResetClear => btnClear;
        //Button IvSingleMatchView.btnPickGolden => btnCatchGolden;
        //Button IvSingleMatchView.btnCombine => btnCombine;

        void IvCommonImageView.UpdateImageSrcName(string srcName)
        {
            //_srcName = srcName != null ? System.IO.Path.GetFileName(srcName) : "";
            //lblTitle.Text = _srcName;
            //lblTitle.ForeColor = Color.White;
            //lblTitle.Refresh();
            if (srcName == null)
                srcName = "";
            updateTitleText(_srcName = srcName, Color.White);
        }
        void IvCommonImageView.UpdateStatusInfo(string msg)
        {
            updateTitleText(msg, Color.White);
        }
        void IvCommonImageView.UpdateRunState(object state)
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
                lblTitle.Text = txt;
                lblTitle.ForeColor = color;
                lblTitle.Refresh();
            }
        }
    }
}
