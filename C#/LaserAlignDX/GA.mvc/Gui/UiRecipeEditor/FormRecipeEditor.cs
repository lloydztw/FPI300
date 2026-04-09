#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-09 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QMath;
using LaserAlignDX.Mvc.Ctrl;
using System;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormRecipeEditor : Form, IvRecipeEditorUI
    {
        public event EventHandler OnActiveViewChanged;

        public FormRecipeEditor()
        {
            InitializeComponent();
            selectImageViewer(0);
            rdoCarriers = new[] { wndBtnsPanel.rdoCarrier1, wndBtnsPanel.rdoCarrier2 };

            var ctrl = new GaRecipeEditCtrl();
            ctrl.Attach(this);

            initLocalEventHandlers();
        }

        #region PRIVATE_MEMBERS
        void initLocalEventHandlers()
        {
            btnSwitchToEmptyTray.Click += (s, e) => selectImageViewer(0);
            btnSwitchToChipTemplate.Click += (s, e) => selectImageViewer(1);
            wndBtnsPanel.OnActiveViewChanged += (s, e) => OnActiveViewChanged?.Invoke(this, e);
        }
        object selectImageViewer(int index)
        {
            ImgViewer = index == 0 ? jezTransImageViewPanel1 : jezTransImageViewPanel2;
            jezTransImageViewPanel1.Visible = index == 0;
            jezTransImageViewPanel2.Visible = index == 1;
            jezTransImageViewPanel1.Dock = index == 0 ? DockStyle.Fill : DockStyle.None;
            jezTransImageViewPanel2.Dock = index == 1 ? DockStyle.Fill : DockStyle.None;
            ((IvRecipeEditorUI)this).ActiveViewIndex = index;
            return ImgViewer;
        }
        #endregion

        #region GUI_LINKS
        GvRecipeBtnsPanel wndBtnsPanel => gvRecipeBtnsPanel1;
        Button btnSwitchToEmptyTray => wndBtnsPanel.btnSwitchToEmptyTray;
        Button btnSwitchToChipTemplate => wndBtnsPanel.btnSwitchToChipTemplate;

        Control IvRecipeEditorUI.Window => this;
        Control IvRecipeEditorUI.wndVisionSettingsPanel => propertyGrid1;

        int IvRecipeEditorUI.ActiveViewIndex 
        {
            get => wndBtnsPanel.ActiveViewIndex;
            set => wndBtnsPanel.ActiveViewIndex = value;
        }
        public JezTransImageViewPanel ImgViewer
        {
            get;
            private set;
        }
        JezTransImageViewPanel IvRecipeEditorUI.GetViewer(int index)
        {
            return index == 0 ? jezTransImageViewPanel1 : jezTransImageViewPanel2;
        }
        //JezTransImageViewPanel IvRecipeEditorUI.ImgViewerEmptyTray => jezTransImageViewPanel1;
        //JezTransImageViewPanel IvRecipeEditorUI.ImgViewerChipTemplate => jezTransImageViewPanel2;

        public RadioButton[] rdoCarriers
        {
            get;
            private set;
        }
        Button IvRecipeEditorUI.btnLoadImage => wndBtnsPanel.btnLoadImage;
        Button IvRecipeEditorUI.btnGrabImage => wndBtnsPanel.btnGrabImage;
        Button IvRecipeEditorUI.btnSaveImage => wndBtnsPanel.btnSaveImage;

        Button IvRecipeEditorUI.btnPickGoldenChipRegion => wndBtnsPanel.btnPickGoldenRegion;
        Button IvRecipeEditorUI.btnAutoCreateCellRegions => wndBtnsPanel.btnCreateCellRegions;

        Button IvRecipeEditorUI.btnOpenTemplateMatchWindow => wndBtnsPanel.btnOpenTemplateMatchWindow;
        Button IvRecipeEditorUI.btnOpenEmptyTrayWindow => wndBtnsPanel.btnOpenEmptyTrayWindow;
        Button IvRecipeEditorUI.btnOpenFlyCamRcpWindow => wndBtnsPanel.btnOpenFlyCamRcpWindow;
        Button IvRecipeEditorUI.btnOpenLightCtrlWindow => wndBtnsPanel.btnOpenLightCtrlWindow;
        Button IvRecipeEditorUI.btnWriteCoordsToPlc => wndBtnsPanel.btnWriteCoordsToPlc;
        
        Button IvRecipeEditorUI.btnFocusMotorSettings => wndBtnsPanel.btnFocusMotorSettings;
        Button IvRecipeEditorUI.btnFocusMotorGo => wndBtnsPanel.btnFocusMotorGo;
        Control IvRecipeEditorUI.lblFocusMotorZ => wndBtnsPanel.lblFocusMotorZ;

        Button IvRecipeEditorUI.btnCancel => btnCancel;
        Button IvRecipeEditorUI.btnOK => btnOK;
        #endregion

        void IvRecipeEditorUI.UpdateCoordsRef(QVector camPt, QVector worldPtSucker1, QVector worldPtSucker2)
        {
            DataGridView dgv = wndBtnsPanel.gvCalibPointsDataGridView1.DataGridView;
            int ri = 0;
            foreach (var worldPt in new[] { worldPtSucker1, worldPtSucker2 })
            {
                int ci = 1;
                dgv.Rows[ri].Cells[ci++].Value = camPt.X;
                dgv.Rows[ri].Cells[ci++].Value = camPt.Y;
                dgv.Rows[ri].Cells[ci++].Value = worldPt.X;
                dgv.Rows[ri].Cells[ci++].Value = worldPt.Y;
                ri++;
            }
        }
    }
}