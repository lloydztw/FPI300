#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-06 重新設計校正架構 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AX.Gui;
using System;
using System.Windows.Forms;
using GaCalibCtrl = LaserAlignDX.Mvc.Ctrl.Calib.V25.GaCalibCtrl;

namespace LaserAlignDX.Mvc.Gui.Calib.V25
{
    public partial class FormCalibrationTool : Form, IvCalibToolUI
    {
        public event EventHandler OnActiveViewChanged;

        public FormCalibrationTool()
        {
            InitializeComponent();

            if (DesignMode)
                return;

            rdoViewModes = new RadioButton[] { rdoCalibGridView, rdoCalibInkView }; 
            rdoCarriers = new RadioButton[] { rdoCarrier1, rdoCarrier2 };
            rdoSuckerRows = new RadioButton[] { rdoSucker1, rdoSucker2 };
            ImgViewers = new[] { jezTransImageViewPanel1, jezTransImageViewPanel2 };
            
            rdoCarrier1.CheckedChanged += RdoCarrier1_CheckedChanged;
            rdoSucker1.CheckedChanged += RdoSucker1_CheckedChanged;
            rdoCalibGridView.CheckedChanged += RdoCalibGridView_CheckedChanged;
            rdoCalibInkView.CheckedChanged += RdoCalibInkView_CheckedChanged;

            SizeChanged += (s, e) => autoLayout();
            Load += Form_Load;

            if (!DesignMode)
            {
                var ctrl = new GaCalibCtrl();
                ctrl.Attach(this);
            }
        }

        public int ActiveViewID
        {
            get
            {
                if (rdoCalibGridView.Checked)
                    return 0;
                else if (rdoCalibInkView.Checked)
                    return 1;
                else
                    return -1;
            }
        }

        #region GUI_LINKS
        public RadioButton[] rdoViewModes
        {
            get; private set;
        }
        public RadioButton[] rdoCarriers
        {
            get; private set;
        }
        public RadioButton[] rdoSuckerRows
        {
            get; private set;
        }
        public JezTransImageViewPanel[] ImgViewers
        {
            get;
            private set;
        }

        Control IvCalibToolUI.Window => this;
        GvCalibPointsDataGridView IvCalibToolUI.dgvCalibPointsListView => gvCalibPointsDataGridView1;
        Control IvCalibToolUI.wndVisionSettingsPanel => gwPanePropsViewer1;
        Button IvCalibToolUI.btnGrabImage => btnGrabImage;
        Button IvCalibToolUI.btnLoadImage => btnLoadImage;
        Button IvCalibToolUI.btnAutoFetchGrid => btnAutoFetchGrid;
        Button IvCalibToolUI.btnAutoFetchInkMarks => btnAutoFetchInkMarks;
        Button IvCalibToolUI.btnBuildCalib => btnBuildCalib;
        GwMotorSimpleGoPanel IvCalibToolUI.wndFocusMotorGoPanel => null;
        Button IvCalibToolUI.btnOpenMotorXY => null;
        Button IvCalibToolUI.btnCancel => btnCancel;
        Button IvCalibToolUI.btnOK => btnOK;
        #endregion

        #region EVENT_HANDLERS
        private void Form_Load(object sender, System.EventArgs e)
        {
            showActiveView();
            gvCalibPointsDataGridView1.SelectedIndex = -1;
        }
        private void RdoSucker1_CheckedChanged(object sender, System.EventArgs e)
        {
            if (sender == rdoSucker1)
            {
                swapForeColors(rdoSucker1, rdoSucker2);
            }
        }
        private void RdoCarrier1_CheckedChanged(object sender, System.EventArgs e)
        {
            if (sender == rdoCarrier1)
            {
                swapForeColors(rdoCarrier1, rdoCarrier2);
            }
        }
        private void RdoCalibGridView_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == rdoCalibGridView)
            {
                swapForeColors(rdoCalibGridView, rdoCalibInkView);
                if (rdoCalibGridView.Checked)
                {
                    showActiveView();
                    OnActiveViewChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        private void RdoCalibInkView_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == rdoCalibInkView)
            {
                swapForeColors(rdoCalibGridView, rdoCalibInkView);
                if (rdoCalibInkView.Checked)
                {
                    showActiveView();
                    OnActiveViewChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        void showActiveView()
        {
            int activeID = ActiveViewID;
            int id = 0;
            
            foreach(var viewer in ImgViewers)
            {
                viewer.Visible = id == activeID;
                viewer.Dock = id==activeID ? DockStyle.Fill : DockStyle.None;
                id++;
            }

            rdoSucker1.Visible = activeID != 0;
            rdoSucker2.Visible = activeID != 0;
        }
        void swapForeColors(Control c1, Control c2)
        {
            var swap = c1.ForeColor;
            c1.ForeColor = c2.ForeColor;
            c2.ForeColor = swap;
        }
        void autoLayout()
        {
            if (WindowState == FormWindowState.Minimized)
                return;

            panelViewers.Width = ClientRectangle.Width - tbLayoutDockRight.Width;
        }
        #endregion
    }
}
