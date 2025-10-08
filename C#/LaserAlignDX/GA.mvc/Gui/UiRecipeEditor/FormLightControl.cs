#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-13 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Utils;
using System;
using System.Windows.Forms;
using Traveller106;
using VsCommon.ControlSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormLightControl : Form
    {
        #region GLOBAL_MESS
        protected MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }
        protected MainFPIX3MachineClass MACHINE
        {
            get { return (MainFPIX3MachineClass)Universal.MACHINECollection.MACHINE; }
        }
        #endregion

        public FormLightControl()
        {
            InitializeComponent();

            DialogResult = DialogResult.Cancel;
            btnOK.Click += BtnOK_Click;
            FormClosed += FormLightControl_FormClosed;
            Load += FormLightControl_Load;
        }

        public LightChannelEnum LightChannel
        {
            get;
            set;
        }
        public int LightValue
        {
            get;
            set;
        }

        #region EVENT_HANDLERS
        private void FormLightControl_FormClosed(object sender, FormClosedEventArgs e)
        {
            applyLightsToMachine(-1, 0); //離開時, 暫時關閉所有燈源.
        }
        private void FormLightControl_Load(object sender, EventArgs e)
        {
            updateAvailableChannels();
            updateSettings(false);
            applyLightsToMachine((int)LightChannel, LightValue);

            numLightValue.ValueChanged += NumLightValue_ValueChanged;
            cboLightChannels.SelectedIndexChanged += NumLightValue_ValueChanged;
        }
        private void NumLightValue_ValueChanged(object sender, EventArgs e)
        {
            updateSettings(true);
            applyLightsToMachine((int)LightChannel, LightValue);
        }
        private void BtnOK_Click(object sender, EventArgs e)
        {
            updateSettings(true);
            this.DialogResult = DialogResult.OK;
            Close();
        }
        #endregion

        void updateAvailableChannels()
        {
            cboLightChannels.Items.Clear();
            var channels = Enum.GetValues(typeof(LightChannelEnum));
            foreach (LightChannelEnum ch in channels)
                cboLightChannels.Items.Add("  " + GaUtil.GetEnumDescription(ch));
        }
        void updateSettings(bool toModel)
        {
            if (toModel)
            {
                LightChannel = (LightChannelEnum)(cboLightChannels.SelectedIndex + 1);
                LightValue = (int)numLightValue.Value;
            }
            else
            {
                cboLightChannels.SelectedIndex = (int)LightChannel - 1;
                numLightValue.Value = LightValue;
            }
        }

        void applyLightsToMachine(int ch, int value)
        {
            if (ch < 0 || ch > 2)
            {
                applyLightsToMachine(1, value);
                applyLightsToMachine(2, value);
                return;
            }

            foreach (var machine in MACHINE.LightCollection)
            {
                machine.ChNum = ch;
                machine.CstLightValue = value;
                machine.LightONOFF(value > 0);
            }
        }
    }
}
