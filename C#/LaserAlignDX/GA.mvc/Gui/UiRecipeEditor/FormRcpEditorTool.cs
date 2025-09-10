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

using JetEazy.BasicSpace;
using JetEazy.QMath;
using LaserAlignDX.Mvc.Ctrl;
using System;
using System.Windows.Forms;


namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormRcpEditorTool : Form, IvRecipeEditorUI
    {
        public FormRcpEditorTool()
        {
            InitializeComponent();

            rdoCarriers = new RadioButton[] { rdoCarrier1, rdoCarrier2 };
            //rdoSuckerRows = new RadioButton[] { rdoSucker1, rdoSucker2 };

            rdoCarrier1.CheckedChanged += RdoCarrier1_CheckedChanged;
            SizeChanged += (s, e) => autoLayout();
            Load += Form_Load;

            if (!DesignMode)
            {
                var ctrl = new GaRecipeEditCtrl();
                ctrl.Attach(this);

                // 設定 Form 屬性
                this.Text = "参数设定窗口";
                this.WindowState = FormWindowState.Maximized;
                LanguageExClass.Instance.EnumControls(this);
            }
        }

        #region GUI_LINKS
        public RadioButton[] rdoCarriers
        {
            get; private set;
        }
        Control IvRecipeEditorUI.Window => this;

        JezTransImageViewPanel IvRecipeEditorUI.ImgViewer => jezTransImageViewPanel1;
        Control IvRecipeEditorUI.wndVisionSettingsPanel => gwPanePropsViewer1;

        Button IvRecipeEditorUI.btnLoadImage => btnLoadImage;
        Button IvRecipeEditorUI.btnGrabImage => btnGrabImage;
        Button IvRecipeEditorUI.btnSaveImage => btnSaveImage;

        Button IvRecipeEditorUI.btnPickGoldenEmptyRegion => btnPickGoldenEmptyRegion;
        Button IvRecipeEditorUI.btnRunEmptyTrayInspect => btnRunEmptyTrayInspect;

        Button IvRecipeEditorUI.btnPickGoldenChipRegion => btnPickGoldenChipRegion;
        Button IvRecipeEditorUI.btnCreateCellRegions => btnOpenTemplateMatch;

        Button IvRecipeEditorUI.btnOpenTemplateMatchWindow => btnOpenTemplateMatch;
        Button IvRecipeEditorUI.btnOpenFlyCamRcpWindow => btnOpenFlyCamRcpEditor;
        Button IvRecipeEditorUI.btnOpenLightCtrlWindow => btnOpenLightCtrl;
        Button IvRecipeEditorUI.btnOpenEmptyTrayWindow => null;

        Button IvRecipeEditorUI.btnCancel => btnCancel;
        Button IvRecipeEditorUI.btnOK => btnOK;
        #endregion

        public void UpdateCoordsRef(QVector camPt, QVector motorPt)
        {
            var dgv = gvCalibPointsDataGridView1.DataGridView;
            int ci = 1;
            dgv.Rows[0].Cells[ci++].Value = camPt != null ? camPt.X : 0.0;
            dgv.Rows[0].Cells[ci++].Value = camPt != null ? camPt.Y : 0.0;
            dgv.Rows[0].Cells[ci++].Value = motorPt != null ? motorPt.X : 0.0;
            dgv.Rows[0].Cells[ci++].Value = motorPt != null ? motorPt.Y : 0.0;
        }

        #region EVENT_HANDLERS
        private void Form_Load(object sender, System.EventArgs e)
        {
            initDgv();
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

        #region PRIVATE_FUNCTION
        void initDgv()
        {
            var dgv = gvCalibPointsDataGridView1.DataGridView;
            while (dgv.Rows.Count > 1)
                dgv.Rows.RemoveAt(dgv.Rows.Count - 1);
            
            dgv.Columns[0].HeaderCell.Value = "點位";
            int cols = dgv.Columns.Count;
            for (int c = 0; c < cols; c++)
            {
                dgv.Columns[c].ReadOnly = true;
            }

            gvCalibPointsDataGridView1.SelectedIndex = -1;
            gvCalibPointsDataGridView1.Height = 2;
        }
        #endregion
    }
}
