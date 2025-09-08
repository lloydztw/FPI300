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
using System.Windows.Forms;


namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormRcpEditorTool : Form, IvCalibToolUI
    {
        public FormRcpEditorTool()
        {
            InitializeComponent();

            rdoCarriers = new RadioButton[] { rdoCarrier1, rdoCarrier2 };
            //rdoSuckerRows = new RadioButton[] { rdoSucker1, rdoSucker2 };

            rdoCarrier1.CheckedChanged += RdoCarrier1_CheckedChanged;
            //rdoSucker1.CheckedChanged += RdoSucker1_CheckedChanged;
            SizeChanged += (s, e) => autoLayout();
            Load += FormCalibrationTool_Load;

            var ctrl = new GaCalibCtrl();
            ctrl.Attach(this);
            this.Height = Screen.PrimaryScreen.Bounds.Height;
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
        Control IvCalibToolUI.Window => this;
        JezTransImageViewPanel IvCalibToolUI.ImgViewer => jezTransImageViewPanel1;
        GvCalibPointsDataGridView IvCalibToolUI.dgvCalibPointsListView => gvCalibPointsDataGridView1;
        Control IvCalibToolUI.wndVisionSettingsPanel => gwPanePropsViewer1;
        Button IvCalibToolUI.btnGrabImage => btnGrabImage;
        Button IvCalibToolUI.btnLoadImage => btnLoadImage;
        Button IvCalibToolUI.btnPickupGolden => btnPickGolden;
        Button IvCalibToolUI.btnAutoFindCalibPoints => btnAutoFindCalibPoints;
        Button IvCalibToolUI.btnBuildCalib => btnBuildCalib;
        Button IvCalibToolUI.btnCancel => btnCancel;
        Button IvCalibToolUI.btnOK => btnOK;
        #endregion

        #region EVENT_HANDLERS
        private void FormCalibrationTool_Load(object sender, System.EventArgs e)
        {
            gvCalibPointsDataGridView1.SelectedIndex = -1;
        }
        private void RdoSucker1_CheckedChanged(object sender, System.EventArgs e)
        {
            //if (sender == rdoSucker1)
            //{
            //    swapForeColors(rdoSucker1, rdoSucker2);
            //}
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
