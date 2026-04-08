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

        #region PRIVATE_FUNCTIONS
        void initLocalEventHandlers()
        {
            var switchButtons = new[]
{
                btnSwitchToEmptyTray,
                btnSwitchToChipTemplate,
            };

            foreach(var btn in switchButtons)
            {
                btn.Click += (s, e) =>
                {
                    int index = Array.IndexOf(switchButtons, btn);
                    var activewViewer = selectImageViewer(index);
                    OnActiveViewChanged?.Invoke(activewViewer, null);
                };
            }
        }
        object selectImageViewer(int index)
        {
            ImgViewerActive = index == 0 ? jezTransImageViewPanel1 : jezTransImageViewPanel2;
            jezTransImageViewPanel1.Visible = index == 0;
            jezTransImageViewPanel2.Visible = index == 1;
            jezTransImageViewPanel1.Dock = index == 0 ? DockStyle.Fill : DockStyle.None;
            jezTransImageViewPanel2.Dock = index == 1 ? DockStyle.Fill : DockStyle.None;
            return ImgViewerActive;
        }
        #endregion

        #region GUI_LINKS
        GvRecipeBtnsPanel wndBtnsPanel => gvRecipeBtnsPanel1;
        Button btnSwitchToEmptyTray => wndBtnsPanel.btnSwitchToEmptyTray;
        Button btnSwitchToChipTemplate => wndBtnsPanel.btnSwitchToChipTemplate;

        Control IvRecipeEditorUI.Window => this;
        Control IvRecipeEditorUI.wndVisionSettingsPanel => propertyGrid1;

        public JezTransImageViewPanel ImgViewerActive
        {
            get;
            private set;
        }
        JezTransImageViewPanel IvRecipeEditorUI.ImgViewerEmptyTray => jezTransImageViewPanel1;
        JezTransImageViewPanel IvRecipeEditorUI.ImgViewerChipTemplate => jezTransImageViewPanel2;

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