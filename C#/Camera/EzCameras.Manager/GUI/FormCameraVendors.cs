#region AUTHOR
/*
 * LeTian.JxProps
 * Copyright (C) 2025
 * 2025-03-10 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzCamera.Manager;
using System;
using System.Windows.Forms;


namespace EzCamera.GUI
{
    internal partial class FormCameraVendors : Form
    {
        string[] VENDOR_NAMES => AllSupportedCameraVendors.VendorIDs;

        #region GUI_MEMBERS
        ListBox lstVendors => listBox1;
        Label lblSimNumber => label2;
        NumericUpDown numSimNumber => numericUpDown1;
        Button btnSelect => button1;
        static int _simNumber = 1;
        #endregion

        public FormCameraVendors()
        {
            InitializeComponent();
            updateCameraVendors();
            lstVendors.SelectedIndexChanged += (s, e) => updateButtonsStatus();
            btnSelect.Click += (s, e) => comfirmSelection();
            btnCancel.Click += (s, e) => cancel();
            numSimNumber.Value = _simNumber;
            updateButtonsStatus();
        }
        public string VendorName
        {
            get;
            private set;
        }
        public int SimNumber
        {
            get => _simNumber;
            private set => _simNumber = value;
        }

        #region EVENT_HANDLERS
        #endregion

        #region PRIVATE_FUNCTIONS
        private void updateCameraVendors()
        {
            int idx = 0;
            lstVendors.Items.Clear();
            foreach(var vendor in VENDOR_NAMES)
            {
                lstVendors.Items.Add($"[{idx++}] {vendor}");
            }
        }
        private void updateButtonsStatus()
        {
            int index = lstVendors.SelectedIndex;
            btnSelect.Enabled = index >= 0;
            //bool isSim = index >= 0 && lstVendors.Items[index].ToString().ToUpper().Contains("SIM");
            //lblSimNumber.Visible = isSim;
            //numSimNumber.Visible = isSim;
        }
        private void comfirmSelection()
        {
            int index = lstVendors.SelectedIndex;
            if (index < 0)
                return;
            try
            {
                VendorName = VENDOR_NAMES[index];
                SimNumber = (int)numSimNumber.Value;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                VendorName = null;
                MessageBox.Show(ex.Message, GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                DialogResult = DialogResult.Cancel;
            }
        }
        private void cancel()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        #endregion
    }
}
