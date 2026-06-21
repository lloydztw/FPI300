using EzCamera.Interface;
using System;
using System.Windows.Forms;

namespace JetEazy.GUI
{
    public partial class FormSelectDeviceInfo : Form
    {
        #region PRIVATE_DATA
        IEzDeviceInfo[] _infos;
        #endregion

        #region GUI_MEMBERS
        ListBox lstDeviceInfos => listBox1;
        Button btnSelect => button1;
        #endregion

        public FormSelectDeviceInfo(IEzCameraFactory factory)
        {
            InitializeComponent();

            _infos = factory.GetAvailableCameraInfos();
            updateCameraDeviceInfos();

            lstDeviceInfos.SelectedIndexChanged += LstDeviceInfos_SelectedIndexChanged;
            btnSelect.Click += BtnSelect_Click;
        }
        public IEzDeviceInfo DeviceInfo
        {
            get;
            private set;
        }

        #region EVENT_HANDLERS
        private void LstDeviceInfos_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSelect.Enabled = true;
        }
        private void BtnSelect_Click(object sender, EventArgs e)
        {
            int index = lstDeviceInfos.SelectedIndex;
            if (index < 0)
                return;

            try
            {
                DeviceInfo = _infos[index];
                DialogResult = DialogResult.OK;
            }
            catch(Exception ex)
            {
                DeviceInfo = null;
                MessageBox.Show(ex.Message);
                DialogResult = DialogResult.Cancel;
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void updateCameraDeviceInfos()
        {
            int idx = 0;
            lstDeviceInfos.Items.Clear();
            foreach(var info in _infos)
            {
                lstDeviceInfos.Items.Add($"[{idx++}] {info}");
            }
        }
        #endregion
    }
}
