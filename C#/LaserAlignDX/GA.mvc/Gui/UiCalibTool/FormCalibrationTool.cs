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

using LaserAlignDX.Mvc.Ctrl;
using System;
using System.Windows.Forms;


namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormCalibrationTool : Form, IvCalibToolUI
    {
        public event EventHandler OnActiveViewChanged;

        public FormCalibrationTool()
        {
            InitializeComponent();

            if (DesignMode)
                return;

            rdoCarriers = new RadioButton[] { rdoCarrier1, rdoCarrier2 };
            rdoSuckerRows = new RadioButton[] { rdoSucker1, rdoSucker2 };
            ImgViewers = new[] { jezTransImageViewPanel1, jezTransImageViewPanel2 };
            
            rdoCarrier1.CheckedChanged += RdoCarrier1_CheckedChanged;
            rdoSucker1.CheckedChanged += RdoSucker1_CheckedChanged;
            tabControl1.SelectedIndexChanged += (s, e) => OnActiveViewChanged?.Invoke(s, e);

            SizeChanged += (s, e) => autoLayout();
            Load += Form_Load;

            if (!DesignMode)
            {
                var ctrl = new GaCalibCtrl();
                ctrl.Attach(this);
            }

            //>>> this.Height = Screen.PrimaryScreen.Bounds.Height;
        }

        public int ActiveViewID
        {
            get => tabControl1.SelectedIndex;

            //set
            //{
            //    if (value < tabControl1.TabPages.Count && value >= 0)
            //        tabControl1.SelectedIndex = value;
            //}
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

        Control IvCalibToolUI.Window => this;
        GvCalibPointsDataGridView IvCalibToolUI.dgvCalibPointsListView => gvCalibPointsDataGridView1;
        Control IvCalibToolUI.wndVisionSettingsPanel => gwPanePropsViewer1;
        Button IvCalibToolUI.btnGrabImage => btnGrabImage;
        Button IvCalibToolUI.btnLoadImage => btnLoadImage;
        Button IvCalibToolUI.btnPickupGolden => btnPickGolden;
        Button IvCalibToolUI.btnRunAutoFetch => btnAutoFindCalibPoints;
        Button IvCalibToolUI.btnBuildCalib => btnBuildCalib;
        Button IvCalibToolUI.btnAutoFetchInkPts => btnAutoFetchInkPts;
        Button IvCalibToolUI.btnBuildCalibInkAdj => btnBuildCalibInkAdj;
        Button IvCalibToolUI.btnCancel => btnCancel;
        Button IvCalibToolUI.btnOK => btnOK;
        #endregion

        #region EVENT_HANDLERS
        private void Form_Load(object sender, System.EventArgs e)
        {
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

            panelDockLeft.Width = ClientRectangle.Width - tbLayoutDockRight.Width;
        }
        #endregion
    }
}
