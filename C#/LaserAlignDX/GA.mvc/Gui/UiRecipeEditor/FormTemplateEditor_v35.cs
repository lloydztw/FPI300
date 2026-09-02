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

        Button IvTemplateEditorUI.btnAutoLineBorders => gwRcpLineBorderBtnsPanel1.btnAutoLineBorders;
        Button IvTemplateEditorUI.btnBuildMircoTransform => gwRcpLineBorderBtnsPanel1.btnBuildMircoTrf;
        NumericUpDown IvTemplateEditorUI.numBorderIndent => gwRcpLineBorderBtnsPanel1.numBorderIndent;
        NumericUpDown IvTemplateEditorUI.numBorderExtend => gwRcpLineBorderBtnsPanel1.numBorderSize;
        NumericUpDown IvTemplateEditorUI.numLineSpanPercentage => gwRcpLineBorderBtnsPanel1.numSpanRatio;

        NumericUpDown IvTemplateEditorUI.numMeasureDistXs => gwRcpLineBorderBtnsPanel1.numMeasureXs;
        NumericUpDown IvTemplateEditorUI.numMeasureDistYs => gwRcpLineBorderBtnsPanel1.numMeasureYs;
        NumericUpDown IvTemplateEditorUI.numMeasureMasks => gwRcpLineBorderBtnsPanel1.numMeasureMasks;

        CheckBox IvTemplateEditorUI.chkShowFilterResult => gwRcpLineBorderBtnsPanel1.chkShowFilterResult;
        NumericUpDown IvTemplateEditorUI.numGrayLimitHi => gwRcpLineBorderBtnsPanel1.numLbFilter1;
        NumericUpDown IvTemplateEditorUI.numGrayLimitLo => gwRcpLineBorderBtnsPanel1.numLbFilter2;

        Button IvTemplateEditorUI.btnTryScanQrCode => gwRcpQrCodeBtnsPanel1.btnTryQrCode;
        Control IvTemplateEditorUI.wndQrCodeResult => gwRcpQrCodeBtnsPanel1.rtbCodeContent;

        Button IvTemplateEditorUI.btnDefectRegionAdd => gwRcpDefectBtnsPanel1.btnDefectAdd;
        Button IvTemplateEditorUI.btnDefectRegionDelete => gwRcpDefectBtnsPanel1.btnDefectDelete;
        Button IvTemplateEditorUI.btnDefectRegionClearAll => gwRcpDefectBtnsPanel1.btnDefectClear;

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
            rdoBoxSelectors = new[] { radioButtonG, radioButtonLn, radioButtonQr, radioButtonDe };
            radioButtonG.Tag = gwRcpTemplateBtnsPanel1;
            radioButtonLn.Tag = gwRcpLineBorderBtnsPanel1;
            radioButtonDe.Tag = gwRcpDefectBtnsPanel1;
            radioButtonQr.Tag = gwRcpQrCodeBtnsPanel1;
            foreach (var rdo in rdoBoxSelectors)
            {
                rdo.CheckedChanged += Rdo_CheckedChanged;
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
            bool isDefectsMode = false;

            #region 1_切換_BUTTON_PANEL
            int index = 0;
            foreach (var rdo in rdoBoxSelectors)
            {
                if (rdo.Tag is Control panel)
                {
                    bool visible = index == activeIndex;
                    panel.Visible = visible;
                    if (visible)
                    {
                        panel.Dock = DockStyle.Fill;
                        isDefectsMode = panel == gwRcpDefectBtnsPanel1;
                    }
                }
                index++;
            }
            #endregion

            #region 2_切換_IMAGE_VIEWERS
            if (isDefectsMode)
            {
                var w1 = tbLayoutImgViews.ColumnStyles[1].Width;
                tbLayoutImgViews.ColumnStyles[0].Width = 0f;
                tbLayoutImgViews.ColumnStyles[2].Width = w1;
            }
            else
            {
                var w1 = tbLayoutImgViews.ColumnStyles[1].Width;
                tbLayoutImgViews.ColumnStyles[0].Width = w1;
                tbLayoutImgViews.ColumnStyles[2].Width = 0f;
            }
            #endregion
        }
        #endregion
    }
}