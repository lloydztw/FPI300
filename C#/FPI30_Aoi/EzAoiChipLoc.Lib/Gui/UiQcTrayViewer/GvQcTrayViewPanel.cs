#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-06-20 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AwFramework;
using EzAoiChipLocQC.Gui.QcTrayView;
using EzAoiChipLocQC.Model;
using OpenCvSharp;
using System;
using System.Windows.Forms;

using CvzQuickImageViewPanel = JetEazy.OpenCV.Viewer.Develop.CvzQuickImageViewPanel;
using QcTrayDrawConfig = EzAoiChipLocQC.Gui.QcTrayView.QcTrayDrawConfig;

namespace EzAoiChipLocQC.Gui.Panels
{
    public partial class GvQcTrayViewPanel : UserControl, IvQcTrayView
    {
        #region CONFIG
        static int MM_PER_PIXEL => QcTrayDrawConfig.MM_PER_PIXEL;
        #endregion

        #region PRIVATE_DATA
        Mat _canvas
        {
            get => quickImageViewPanel.Image;
            set
            {
                var old = quickImageViewPanel.Image;
                quickImageViewPanel.Image = value;
                if (old != value)
                    old?.Dispose();
            }
        }
        string _title = "QC Tray View";
        #endregion

        #region INTERACTORS
        CviQcTrayPlaceHoldsBox _cviQcTrayPlaceHoldsBox = new CviQcTrayPlaceHoldsBox();
        #endregion

        public GvQcTrayViewPanel()
        {
            InitializeComponent();
            quickImageViewPanel.btnOpen.Visible = false;
            quickImageViewPanel.Icon = Properties.Resources.grid_3x3;
            quickImageViewPanel.ImageViewer.AddInteractor(_cviQcTrayPlaceHoldsBox);
            HandleDestroyed += (s, e) => cleanUp();
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                    quickImageViewPanel?.UpdateTitleBarText(_title = value);
            }
        }

        #region GUI_LINKS
        Control IView.Window => this;
        CvzQuickImageViewPanel quickImageViewPanel => cvzQuickImageViewPanel1;
        #endregion

        public void UpdateSettings(JxTrayDimSettings traySettings)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action<JxTrayDimSettings>)UpdateSettings, traySettings);
            }
            else
            {
                updateCanvas(traySettings);
                updatePlaceHolds(traySettings);
                BeginInvoke((Action)Refresh);
            }
        }

        #region PRIVATE_FUNCTIONS
        void updateCanvas(JxTrayDimSettings traySettings)
        {
            var dim = traySettings.GetTrayDimension();
            var width = (int)(dim.Width * MM_PER_PIXEL);
            var height = (int)(dim.Height * MM_PER_PIXEL);
            
            var canvas = this._canvas;
            if (canvas == null || canvas.Width != width || canvas.Height != height)
            {
                canvas = new Mat(height, width, MatType.CV_8UC3);
                canvas.SetTo(QcTrayDrawConfig.TrayBackGroundColor);
                this._canvas = canvas;
                quickImageViewPanel.UpdateTitleBarText(_title);
            }
        }
        void updatePlaceHolds(JxTrayDimSettings traySettings)
        {
            _cviQcTrayPlaceHoldsBox.Visible = false;
            _cviQcTrayPlaceHoldsBox.UpdatePlaceHolds(traySettings);
            _cviQcTrayPlaceHoldsBox.Visible = true;
        }
        void cleanUp()
        {
            _canvas = null;
        }
        #endregion
    }
}
