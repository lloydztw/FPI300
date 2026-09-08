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

using JetEazy.Lang;
using JzDisplay.UISpace;
using System;
using System.Windows.Forms;
using GaTemplateEditCtrl = LaserAlignDX.Mvc.Ctrl.V35.GaTemplateEditCtrl;

namespace LaserAlignDX.Mvc.Gui.V35
{
    public partial class FormTemplateEditor : Form, IvTemplateEditorUI
    {
        public FormTemplateEditor(params object[] args)
        {
            InitializeComponent();

            if (DesignMode)
                return;

            initGui();

            var ctrl = new GaTemplateEditCtrl();
            ctrl.Attach(this);
        }

        Control IvTemplateEditorUI.Window => this;
        Control IvTemplateEditorUI.lblActiveCarrierID => lblActiveCarrierID;

        public DispUI[] DispViewers
        {
            get; private set;
        }
        public Control[] ImgViewers
        {
            get;
            private set;
        }
        public RadioButton[] rdoBoxSelectors
        {
            get; private set;
        }

        Button IvTemplateEditorUI.btnRotateGolden => gwRcpTemplateBtnsPanel1.btnRotateGolden;
        Button IvTemplateEditorUI.btnPickGolden => gwRcpTemplateBtnsPanel1.btnPickGolden;

        Button IvTemplLinebordersEditorUI.btnAutoLineBorders => gwRcpLineBorderBtnsPanel1.btnAutoLineBorders;
        Button IvTemplLinebordersEditorUI.btnBuildMircoTransform => gwRcpLineBorderBtnsPanel1.btnBuildMircoTrf;
        NumericUpDown IvTemplLinebordersEditorUI.numBorderIndent => gwRcpLineBorderBtnsPanel1.numBorderIndent;
        NumericUpDown IvTemplLinebordersEditorUI.numBorderExtend => gwRcpLineBorderBtnsPanel1.numBorderSize;
        NumericUpDown IvTemplLinebordersEditorUI.numLineSpanPercentage => gwRcpLineBorderBtnsPanel1.numSpanRatio;

        NumericUpDown IvTemplLinebordersEditorUI.numMeasureDistXs => gwRcpLineBorderBtnsPanel1.numMeasureXs;
        NumericUpDown IvTemplLinebordersEditorUI.numMeasureDistYs => gwRcpLineBorderBtnsPanel1.numMeasureYs;
        NumericUpDown IvTemplLinebordersEditorUI.numMeasureMasks => gwRcpLineBorderBtnsPanel1.numMeasureMasks;

        CheckBox IvTemplLinebordersEditorUI.chkShowFilterResult => gwRcpLineBorderBtnsPanel1.chkShowFilterResult;
        NumericUpDown IvTemplLinebordersEditorUI.numGrayLimitHi => gwRcpLineBorderBtnsPanel1.numLbFilter1;
        NumericUpDown IvTemplLinebordersEditorUI.numGrayLimitLo => gwRcpLineBorderBtnsPanel1.numLbFilter2;

        Button IvTemplQrCodeEditorUI.btnTryScanQrCode => gwRcpQrCodeBtnsPanel1.btnTryQrCode;
        Control IvTemplQrCodeEditorUI.wndQrCodeResult => gwRcpQrCodeBtnsPanel1.rtbCodeContent;

        Button IvTemplDefectsEditorUI.btnDefectRegionAdd => gwRcpDefectBtnsPanel1.btnAdd;
        Button IvTemplDefectsEditorUI.btnDefectRegionDelete => gwRcpDefectBtnsPanel1.btnDelete;
        Button IvTemplDefectsEditorUI.btnDefectRegionClearAll => gwRcpDefectBtnsPanel1.btnClearAll;

        Button IvTemplBadConnsEditorUI.btnAddRegion => gwRcpConnBlocBtnsPanel1.btnAdd;
        Button IvTemplBadConnsEditorUI.btnDeleteRegion => gwRcpConnBlocBtnsPanel1.btnDelete;
        Button IvTemplBadConnsEditorUI.btnClearAllRegions => gwRcpConnBlocBtnsPanel1.btnClearAll;

        Control IvTemplateEditorUI.wndVisionSettingsPanel => propertyGrid1;
        Button IvTemplateEditorUI.btnTrainTemplate => btnTrain;
        Button IvTemplateEditorUI.btnSaveAllParams => btnSave;
        Button IvTemplateEditorUI.btnCancel => btnCancel;

        #region INIT_FUNCTIONS
        void initGui()
        {
            //(1) ImgViewers
            ImgViewers = new[]
            {
                jezTransImageViewPanel1, 
                jezTransImageViewPanel2, 
                jezTransImageViewPanel3,
            }; 
            jezTransImageViewPanel1.lblTitle.Text = "Region View";
            jezTransImageViewPanel2.lblTitle.Text = "Template View";
            jezTransImageViewPanel3.lblTitle.Text = "Defects View";

            try
            {
                jezTransImageViewPanel1.picIcon.BackgroundImage = Properties.Resources.recipe_edit;
                jezTransImageViewPanel2.picIcon.BackgroundImage = Properties.Resources.golden_star;
                jezTransImageViewPanel3.picIcon.BackgroundImage = Properties.Resources.debug_purple;
            }
            catch
            {

            }

            //(2) rdoBoxSelectors (順序: Golden, Line Border, QR Code, Defects)
            rdoBoxSelectors = new[] { 
                radioButtonG, 
                radioButtonLn, 
                radioButtonQr, 
                radioButtonDe, 
                radioButtonCn 
            };

            radioButtonG.Tag = gwRcpTemplateBtnsPanel1;
            radioButtonLn.Tag = gwRcpLineBorderBtnsPanel1;
            radioButtonDe.Tag = gwRcpDefectBtnsPanel1;
            radioButtonCn.Tag = gwRcpConnBlocBtnsPanel1;
            radioButtonQr.Tag = gwRcpQrCodeBtnsPanel1;

            foreach (var rdo in rdoBoxSelectors)
            {
                rdo.CheckedChanged += Rdo_CheckedChanged;
                (rdo.Tag as Control).Dock = DockStyle.Fill;
            }

            //(3) Load Event
            //>>> Load += (s, e) => QMSG.Dump(this, 2000);
            Load += (s, e) => QMSG.Translate(this);

            //(*) Update Layout
            updateLayout(0);
        }
        #endregion

        #region EVENT_HANDLERS
        private void Rdo_CheckedChanged(object sender, System.EventArgs e)
        {
            if(sender is RadioButton rdo)
            {
                if (rdo.Checked)
                {
                    int index = Array.IndexOf(rdoBoxSelectors, rdo);
                    if (index >= 0)
                    {
                        updateLayout(index);
                    }
                }
            }
        }
        #endregion

        #region PRIVATE_GUI_FUNCTIONS
        void updateLayout(int activeIndex)
        {
            Control activePanel = null;

            #region 1_切換_BUTTON_PANEL
            int index = 0;
            foreach (var rdo in rdoBoxSelectors)
            {
                if (rdo.Tag is Control panel)
                {
                    if (index != activeIndex)
                        panel.Visible = false;
                }
                index++;
            }
            if(activeIndex < rdoBoxSelectors.Length)
            {
                activePanel = rdoBoxSelectors[activeIndex].Tag as Control;
                if(activePanel!=null)
                {
                    activePanel.Dock = DockStyle.Fill;
                    activePanel.Visible = true;
                }
            }
            #endregion

            #region 2_切換_IMAGE_VIEWERS
            float w50 = 50f;
            // 瑕疵檢測
            if (activePanel == gwRcpDefectBtnsPanel1)
            {
                tbLayoutImgViews.ColumnStyles[0].Width = 0f;
                tbLayoutImgViews.ColumnStyles[1].Width = w50;
                tbLayoutImgViews.ColumnStyles[2].Width = w50;
            }
            // 連筋檢測
            else if(activePanel == gwRcpConnBlocBtnsPanel1)
            {
                tbLayoutImgViews.ColumnStyles[0].Width = w50;
                tbLayoutImgViews.ColumnStyles[1].Width = 0f;
                tbLayoutImgViews.ColumnStyles[2].Width = w50;
            }
            else
            {
                tbLayoutImgViews.ColumnStyles[0].Width = w50;
                tbLayoutImgViews.ColumnStyles[1].Width = w50;
                tbLayoutImgViews.ColumnStyles[2].Width = 0f;
            }
            #endregion
        }
        #endregion
    }
}