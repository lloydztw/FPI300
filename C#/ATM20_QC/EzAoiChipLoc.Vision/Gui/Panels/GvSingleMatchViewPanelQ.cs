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
using EzDualMatch.Model;
using JetEazy.ImageViewerEx;
using JetEazy.OpenCV.Viewer;
using System.Drawing;
using System.Windows.Forms;


namespace EzDualMatch.Gui.Panels
{
    public partial class GvSingleMatchViewPanelQ : UserControl, IvSingleMatchView
    {
        public GvSingleMatchViewPanelQ()
        {
            InitializeComponent();
            quickImageViewPanel.OptAutoPersistLastFile = false;
            if (!DesignMode)
            {
                HandleCreated += (s, e) => buildCustomizedButtons();
            }
        }
        public int ViewID
        {
            get => quickImageViewPanel.ViewID;
            internal set { quickImageViewPanel.ViewID = value; }
        }

        #region PRIVATE_PROPERTIES
        string _srcName => quickImageViewPanel.SrcName;
        #endregion

        #region GUI_MEMBERS
        internal PictureBox picIcon => quickImageViewPanel.picIcon;
        Button btnOpen;
        Button btnMatch;
        Button btnClear;
        Button btnGolden;
        Button btnCombine;
        void buildCustomizedButtons()
        {
            btnOpen = null; // quickImageViewPanel.btnOpen;
            btnMatch = quickImageViewPanel.AddTitleWidgetButton("Match");
            btnClear = quickImageViewPanel.AddTitleWidgetButton("Clear");
            btnGolden = quickImageViewPanel.AddTitleWidgetButton("Golden");
            btnGolden.BackColor = Color.Gold;
            if (ViewID == 0)
            {
                btnCombine = quickImageViewPanel.AddTitleWidgetButton("合併", 0);
                btnCombine.BackColor = Color.Cyan;
                //btnCombine.Visible = false;
            }
            else
            {
                btnCombine = null;
            }
        }
        #endregion

        #region GUI_LINKS
        Form IView.frmOwner => FindForm();
        Control IView.Window => this;

        public CvzQuickImageViewPanel quickImageViewPanel => cvzQuickImageViewPanel1;
        IvImageViewer IvSingleMatchView.ImageViewer => quickImageViewPanel.ImageViewer;

        Button IvSingleMatchView.btnOpen => btnOpen;
        Button IvSingleMatchView.btnRunMatch => btnMatch;
        Button IvSingleMatchView.btnResetClear => btnClear;
        Button IvSingleMatchView.btnCatchGolden => btnGolden;
        Button IvSingleMatchView.btnCombine => btnCombine;
        #endregion

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
            if (srcName == null)
                srcName = "";
            updateTitleText(srcName, Color.White);
        }
        void IvSingleMatchView.UpdateMatchResult(MatchResultEventArgs e)
        {
            var result = e?.Result;
            if (e == null || e.IsResetting() || result == null)
            {
                updateTitleText(_srcName, Color.White);
            }
            else
            {
                updateTitleText(result.ToString(), Color.White);
            }
        }
        void updateTitleText(string txt, Color color)
        {
            quickImageViewPanel.UpdateTitleBarText(txt, color);
        }
    }
}
