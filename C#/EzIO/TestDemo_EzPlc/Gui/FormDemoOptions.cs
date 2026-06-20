#region AUTHOR
/*
 * EzIO GUI
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-11 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Windows.Forms;

namespace TestDemo_EzPLC.Gui
{
    public partial class FormDemoOptions : Form
    {
        public FormDemoOptions()
        {
            InitializeComponent();
            btnOK.Click += (s, e) => Confirm();
            btnCancel.Click += (s, e) => Cancel();
            Load += FormDemoOptions_Load;

            RdoOptions = new[]
            {
                radioButton1,
                radioButton2,
                radioButton3,
                radioButton4,
            };

            foreach (var rdo in RdoOptions)
                rdo.CheckedChanged += (s, e) => updateDemoOption(true);
        }

        #region PRIVATE_GUI
        RadioButton[] RdoOptions
        {
            get;
            set;
        }
        #endregion

        #region EVENT_HANLDERS
        private void FormDemoOptions_Load(object sender, System.EventArgs e)
        {
            //DemoOption = Properties.Settings.Default.DemoOption;
            //ComPort = Properties.Settings.Default.ComPort;
            //IpPort = Properties.Settings.Default.IpPort;
            //IsSim = Properties.Settings.Default.IsSim;
            LoadDefault();
            updateDemoOption(false);
        }
        #endregion

        public bool IsSim
        {
            get => chkSim.Checked;
            private set => chkSim.Checked = value;
        }
        public int DemoOption
        {
            get;
            private set;
        }
        public int PortNumber
        {
            get
            {
                if (isUsingComPort())
                    return ComPort;
                else
                    return IpPort;
            }
        }

        #region PRIVATE_FUNCTIONS
        bool isUsingComPort()
        {
            return DemoOption == 0 || DemoOption == 1;
        }
        int ComPort
        {
            get => (int)numComPort.Value;
            set => numComPort.Value = value;
        }
        int IpPort
        {
            get => (int)numIpPort.Value;
            set => numIpPort.Value = value;
        }

        void Confirm()
        {
            SaveDefault();
            DialogResult = DialogResult.OK;
            Close();
        }
        void Cancel()
        {
            DialogResult = DialogResult.Cancel; Close();
        }

        void LoadDefault()
        {
            var df = Properties.Settings.Default;
            if (df == null)
                return;

            DemoOption = df.DemoOption;
            ComPort = df.ComPort;
            IpPort = df.IpPort;
            IsSim = df.IsSim;
        }
        void SaveDefault()
        {
            var df = Properties.Settings.Default;
            if (df == null)
                return;

            if (df.IsSim != IsSim || 
                df.IpPort != IpPort ||
                df.ComPort != ComPort ||
                df.DemoOption != DemoOption)
            {
                df.DemoOption = DemoOption;
                df.ComPort = ComPort;
                df.IpPort = IpPort;
                df.IsSim = IsSim;
                df.Save();
            }
        }

        void updateDemoOption(bool toModel)
        {
            if (toModel)
            {
                for (int i = 0; i < RdoOptions.Length; i++)
                {
                    if (RdoOptions[i].Checked)
                    {
                        DemoOption = i;
                        break;
                    }
                }
            }
            else
            {
                RdoOptions[DemoOption].Checked = true;
            }

            numComPort.Visible = isUsingComPort();
            numIpPort.Visible = !numComPort.Visible;
        }
        #endregion
    }
}
