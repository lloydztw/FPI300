#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-19 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using LaserAlignDX.Mvc.Ctrl;
using System.Windows.Forms;
using DispUI = JzDisplay.UISpace.DispUI;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormTemplateEditor : Form, IvTemplateEditorUI
    {
        public FormTemplateEditor(CarrierEnum carrierID)
        {
            InitializeComponent();

            DispViewers = new[] { DS1, DS2, DS3 };
            rdoBoxSelectors = new[] { radioButtonG, radioButtonLn, radioButtonQr };
            SizeChanged += (s, e) => autoLayout();
            autoLayout();

            var ctrl = new GaTemplateEditCtrl();
            ctrl.Attach(this, carrierID);
        }

        Control IvTemplateEditorUI.Window => this;
        Control IvTemplateEditorUI.lblActiveCarrierID => lblActiveCarrierID;

        public DispUI[] DispViewers
        {
            get; private set;
        }
        public RadioButton[] rdoBoxSelectors
        {
            get; private set;
        }

        Button IvTemplateEditorUI.btnPickGolden => btnPickGolden;
        Button IvTemplateEditorUI.btnAutoLineBorders => btnAutoLineBorders;
        NumericUpDown IvTemplateEditorUI.numBorderIndent => numBorderIndent;
        NumericUpDown IvTemplateEditorUI.numBorderExtend => numBorderSize;
        NumericUpDown IvTemplateEditorUI.numLineSpanPercentage => numSpanRatio;

        Button IvTemplateEditorUI.btnTryScanQrCode => btnTryQrCode;
        Control IvTemplateEditorUI.wndQrCodeResult => rtbCodeContent;
        Control IvTemplateEditorUI.wndVisionSettingsPanel => propertyGrid1;

        Button IvTemplateEditorUI.btnDefectRegionAdd => btnDefectAdd;
        Button IvTemplateEditorUI.btnDefectRegionDelete => btnDefectDelete;
        Button IvTemplateEditorUI.btnDefectRegionClearAll => btnDefectClear;
        Button IvTemplateEditorUI.btnTrainTemplate => btnTrain;
        Button IvTemplateEditorUI.btnSaveAllParams => btnSave;
        Button IvTemplateEditorUI.btnCancel => btnCancel;

        #region PRIVATE_FUNCTIONS
        void autoLayout()
        {
            if (WindowState == FormWindowState.Minimized)
                return;
            var ccSize = ClientSize;
            tbLayoutA.Dock = DockStyle.Top;
            tbLayoutB.Dock = DockStyle.Bottom;
            tbLayoutA.Height = ccSize.Height - tbLayoutB.Height;
        }
        #endregion
    }
}
