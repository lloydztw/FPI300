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
        public event EventHandler OnSelectedChanged;

        public FormRecipeEditor()
        {
            InitializeComponent();
            rdoCarriers = new[] { wndBtnsPanel.rdoCarrier1, wndBtnsPanel.rdoCarrier2 };

            var ctrl = new GaRecipeEditCtrl();
            ctrl.Attach(this);

            initLocalEventHandlers();
        }

        #region PRIVATE_MEMBERS
        void initLocalEventHandlers()
        {
            wndBtnsPanel.OnActiveButtonIndexChanged += (s, e) => OnSelectedChanged?.Invoke(this, e);
        }
        #endregion

        #region GUI_LINKS
        GvRecipeBtnsPanel wndBtnsPanel => gvRecipeBtnsPanel1;

        Control IvRecipeEditorUI.Window => this;
        Control IvRecipeEditorUI.wndVisionSettingsPanel => propertyGrid1;

        int IvRecipeEditorUI.SelectedIndex 
        {
            get => wndBtnsPanel.ActiveButtonIndex;
            set => wndBtnsPanel.ActiveButtonIndex = value;
        }
        JezTransImageViewPanel IvRecipeEditorUI.ImgViewer
        {
            get => jezTransImageViewPanel1;
        }

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