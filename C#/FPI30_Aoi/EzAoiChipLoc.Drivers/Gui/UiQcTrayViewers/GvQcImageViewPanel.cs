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
using CvzQuickImageViewPanel = JetEazy.OpenCV.Viewer.Develop.CvzQuickImageViewPanel;

namespace EzAoiChipLocQC.Gui.Panels
{
    public partial class GvQcImageViewPanel : UserControl, IvQcImageView
    {
        #region PRIVATE_DATA
        string _srcName = "";
        #endregion

        public GvQcImageViewPanel()
        {
            InitializeComponent();
            //if (!DesignMode)
            //    cvMatViewer.Attach(lblCoordInfo, lblBlinker);
        }

        #region GUI_LINKS
        Control IView.Window => this;
        CvzQuickImageViewPanel quickImageViewPanel => cvzQuickImageViewPanel1;
        CvMatViewer cvMatViewer => quickImageViewPanel.MatViewer;
        IvImageViewer IvQcImageView.ImageViewer => cvMatViewer;
        #endregion

        void IvQcImageView.UpdateImageSrcName(string srcName)
        {
            if (srcName == null)
                srcName = "";
            updateTitleText(_srcName = srcName, Color.White);
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
                quickImageViewPanel.UpdateTitleBarText(txt, color);
            }
        }
    }
}
