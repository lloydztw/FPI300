#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-20 配合 新校正板 (陣列圓點 + INK區塊 二合一) (by LeTian Chang)
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
using GaCalibCtrl = LaserAlignDX.Mvc.Ctrl.Galib.V5.GaCalibCtrl;

namespace LaserAlignDX.Mvc.Gui.Calib.V5
{
    public partial class FormCalibrationTool : Form, IvCalibToolUI
    {
        public event EventHandler OnActiveViewChanged;

        public FormCalibrationTool()
        {
            InitializeComponent();

            if (DesignMode)
                return;

            // 載台 選項
            rdoCarriers = new RadioButton[] { rdoCarrier1, rdoCarrier2 };
            // 吸嘴排 選項
            rdoSuckerRows = new RadioButton[] { rdoSucker1, rdoSucker2 };
            // ImgViewers (暫時設定兩組相同 以維持舊碼相容性)
            ImgViewers = new[] { jezTransImageViewPanel1, jezTransImageViewPanel1 };

            #region LOCAL_EVENT_HANDLERS
            rdoCarrier1.CheckedChanged += RdoCarrier1_CheckedChanged;
            rdoSucker1.CheckedChanged += RdoSucker1_CheckedChanged;
            SizeChanged += (s, e) => autoLayout();
            #endregion

            if (!DesignMode)
            {
                Load += Form_Load;
                // CONTROL
                var ctrl = new GaCalibCtrl();
                ctrl.Attach(this);
            }
        }

        #region GUI_LINKS
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

        int IvCalibToolUI.ActiveViewID => 0;
        Control IvCalibToolUI.Window => this;
        GvCalibPointsDataGridView IvCalibToolUI.dgvCalibPointsListView => gvCalibPointsDataGridView1;
        Control IvCalibToolUI.wndVisionSettingsPanel => gwPanePropsViewer1;
        Button IvCalibToolUI.btnGrabImage => btnGrabImage;
        Button IvCalibToolUI.btnLoadImage => btnLoadImage;
        Button IvCalibToolUI.btnAutoFetchGrid => btnAutoFetchAll;
        Button IvCalibToolUI.btnAutoFetchInkMarks => null;
        Button IvCalibToolUI.btnBuildCalib => btnBuildCalib;
        Button IvCalibToolUI.btnOpenMotorXY => btnOpenMotorXY;
        GwMotorSimpleGoPanel IvCalibToolUI.wndFocusMotorGoPanel => gvFocusSettingPanel1;
        Button IvCalibToolUI.btnCancel => btnCancel;
        Button IvCalibToolUI.btnOK => btnOK;
        #endregion

        #region EVENT_HANDLERS
        private void Form_Load(object sender, System.EventArgs e)
        {
            gvCalibPointsDataGridView1.SelectedIndex = -1;

            //QMSG.Dump(this);
            QMSG.Translate(this);
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
        }
        #endregion
    }
}
