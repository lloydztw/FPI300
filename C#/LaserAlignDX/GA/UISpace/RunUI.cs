#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-06-29 在沒有更改邏輯的狀況下, 使用 #region #endregion
 *                 重新收納整理 萬子 散亂放置的代碼 
 *                 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.DBSpace;
using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.Utils;
using LaserAlignDX;
using NeedleX.ProcessSpace;
using System;
using System.Drawing;
using System.Windows.Forms;
using TravellerMINIX6.ProcessSpace;
using VsCommon.ControlSpace.IOSpace;

namespace PhotoMachine.UISpace
{
    public partial class RunUI : UserControl
    {
        #region Constants
        const int ShinningDuriation = 50;
        const int ShiningTimes = 2;
        #endregion

        #region GUI_LINKS
        Label lblBigPass => label4;
        Label lblDuriation => label2;

        TextBox txtOPBarcode => textBox1;
        TextBox txtProductBarcode => textBox3;
        TextBox txtResult => textBox2;

        CheckBox chkIsSaveRaw => checkBox1;
        CheckBox chkIsSaveNGRaw => checkBox2;
        CheckBox chkIsSaveDebug => checkBox3;

        TextBox txtLotID => txtLotData1;
        TextBox txtStripID => txtLotData2;
        Button btnSoftwareReady => button5;
        Button btnSingleSnap => button6;

        Button btnSingleTest => button1;
        Button btnSaveImage => button2;
        Button btnSingleOfflineTest => button3;
        Button btnClearDataZero => button4;

        //Button btnAutoManual => button7;
        #endregion

        #region HARDWARE_DEVICE
        IxLineScanCam IScanCam
        {
            get { return Traveller106.Universal.IxLineScan; }
        }
        IPlcIoFPIX3 _plcIO
        {
            //get => GaMvcConfig.InstancePLC("RUNUI");
            get => null;
        }
        #endregion

        #region DB
        RCPDBClass RCPDB
        {
            get { return Traveller106.Universal.RCPDB; }
        }
        #endregion

        #region Processes & Threads
        BaseProcess m_SingleProcess
        {
            get { return LineScanSingleProcess.Instance; }
        }

        ProcessClass _shinningProcess = new ProcessClass();
        #endregion

        #region State & Run Data Variables
        bool IsResultPass = false;
        int m_Start = -1;
        Timer m_CalTimer;
        int ShinigCount = 0;
        DateTime m_dtStart = DateTime.Now;
        #endregion

        #region Environment, Config & Language Settings
        string UIPath = "";
        int LanguageIndex = 0;
        VersionEnum VER = VersionEnum.STEROPES;
        OptionEnum OPT = OptionEnum.MAIN;

        JzToolsClass JzTools = new JzToolsClass();
        JzLanguageClass myLanguage = new JzLanguageClass();
        #endregion

        #region Properties
        public bool IsSaveRaw
        {
            get { return chkIsSaveRaw.Checked; }
        }
        public bool IsSaveNGRaw
        {
            get { return chkIsSaveNGRaw.Checked; }
        }
        public bool IsSaveDebug
        {
            get { return chkIsSaveDebug.Checked; }
        }

        public bool SetBarcodeEnable
        {
            set
            {
                txtProductBarcode.Enabled = value;
                txtOPBarcode.Enabled = value;

                if (value)
                {
                    txtOPBarcode.Text = "";
                    txtOPBarcode.Focus();
                }
            }
        }
        public string SetBarcodeString
        {
            set
            {
                txtOPBarcode.Text = value;
                txtOPBarcode.Focus();
            }
        }
        public bool IsShinning
        {
            get { return _shinningProcess.IsOn; }
        }
        #endregion

        public RunUI()
        {
            InitializeComponent();

            if (!DesignMode)
                initGui();
        }

        void initGui()
        {
            txtLotID.ReadOnly = true;
            txtStripID.ReadOnly = true;

            //var injection = GaMvcConfig.UxInjection;
            //if (injection != null)
            //{
            //    injection.AttachOpUI(btnSoftwareReady, txtLotID, txtStripID);
            //}
            //else
            {
                btnSoftwareReady.Click += BtnSoftwareReady_Click;
            }

            chkIsSaveRaw.Visible = false;
            chkIsSaveNGRaw.Visible = false;
            chkIsSaveDebug.Visible = false;

            // 事件繫結保持不變
            btnSingleSnap.Click += BtnSingleSnap_Click;
            btnSingleTest.Click += BtnSingleTest_Click;
            btnSaveImage.Click += BtnSaveImage_Click;
            btnSingleOfflineTest.Click += BtnSingleOfflineTest_Click;
            btnClearDataZero.Click += BtnClearDataZero_Click;
            //btnAutoManual.Click += BtnAutoManual_Click;

            txtProductBarcode.KeyDown += new KeyEventHandler(txtProductBarcode_KeyDown);
            txtOPBarcode.KeyDown += new KeyEventHandler(txtBarcode_KeyDown);
            SizeChanged += RunUI_SizeChanged;

            InitializeDataGridView();
        }

        public void Initial(string uipath, int langindex, VersionEnum ver, OptionEnum opt)
        {
            UIPath = uipath;
            LanguageIndex = langindex;
            VER = ver;
            OPT = opt;

            m_CalTimer = new Timer();
            m_CalTimer.Interval = 1000;
            m_CalTimer.Enabled = true;
            m_CalTimer.Tick += M_CalTimer_Tick;

            SetDuriation("0 s");

            if (!DesignMode)
            {
                var model = GaMvcConfig.SysModel?.AoiModel;
                if (model != null)
                    model.OnLotDataChanged += AoiModel_OnLotDataChanged;
            }
        }

        #region Control Event Handlers
        private void AoiModel_OnLotDataChanged(object sender, EventArgs e)
        {
            UpdateLotData();
        }

        private void BtnAutoManual_Click(object sender, EventArgs e)
        {

        }

        private void BtnSoftwareReady_Click(object sender, EventArgs e)
        {
            //if (GaMvcConfig.UxInjection != null)
            //    return;

            var plcIO = _plcIO; // MACHINE?.PLCIO;
            if (plcIO == null) return;

            bool bReady = !plcIO.bSoftwareReady;    // Toggle bSoftwareReady

            plcIO.bSoftwareReady = bReady;
            plcIO.bFlyReady = bReady;

            if (bReady)
            {
                plcIO.ResetPlc(resetRecipeNum: true);
                OnTrigger(RunStatusEnum.CHANGERECIPE);
            }
        }

        private void BtnClearDataZero_Click(object sender, EventArgs e)
        {
            SetDuriation("0 s");
        }

        private void BtnSingleOfflineTest_Click(object sender, EventArgs e)
        {
#if (OPT_ABANDONED)
            if (!m_SingleProcess.IsOn)
                m_SingleProcess.Start("OfflineTest");
            else
                m_SingleProcess.Stop();
#endif
        }

        private void BtnSaveImage_Click(object sender, EventArgs e)
        {
            Bitmap srcBmp = null;
            bool isOffLine = false;

            if (Traveller106.Universal.IsNoUseCCD)
            {
                var offlineBmp = GaMvcConfig.SysModel.LineScanImageHolder.PeekBitmap();
                srcBmp = (Bitmap)offlineBmp?.Clone();
                isOffLine = true;
            }
            else
            {
                var cameraFreeBmp = IScanCam.GetFreeImageBitmap();
                srcBmp = cameraFreeBmp?.ToBitmap();
                isOffLine = false;
            }

            if (srcBmp != null)
            {
                string dstFilename = SaveFilePicker("JPG Files (*.jpg)|*.jpg|BMP Files (*.bmp)|*.bmp|PNG Files (*.png)|*.png", "");
                if (!string.IsNullOrEmpty(dstFilename))
                {
                    var oldCur = GaUtil.SetCursor(this, Cursors.AppStarting);
                    this.FindForm().Refresh();

                    GaImageUtil.SaveBigImage(dstFilename, srcBmp);
                    string msg = isOffLine ? "離線圖檔 已經另存至:" : "相機圖檔 已經保存至:";
                    VsMessageBox.Info(msg + Environment.NewLine + dstFilename);

                    GaUtil.SetCursor(this, oldCur);
                }
                srcBmp.Dispose();
            }
        }

        private void BtnSingleSnap_Click(object sender, EventArgs e)
        {
#if(OPT_ABANDONED)
            if (!m_SingleProcess.IsOn)
                m_SingleProcess.Start("Snap");
            else
                m_SingleProcess.Stop();
#endif
        }

        private void BtnSingleTest_Click(object sender, EventArgs e)
        {
            if (!m_SingleProcess.IsOn)
                m_SingleProcess.Start("Test");
            else
                m_SingleProcess.Stop();
        }

        private void M_CalTimer_Tick(object sender, EventArgs e)
        {
            if (m_Start > 0)
            {
                SetDuriation(ConvertToString(DateTime.Now.Subtract(m_dtStart)));
            }
        }

        void txtProductBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                txtOPBarcode.Focus();
                txtOPBarcode.SelectAll();
            }
        }

        void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                OnTrigger(RunStatusEnum.STARTRUN);
            }
        }
        #endregion

        #region DataGridView Operations
        private void InitializeDataGridView()
        {
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;

            dgv.Columns["col1"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["col2"].SortMode = DataGridViewColumnSortMode.NotSortable;

            dgv.Rows.Add("PASS数", 0);
            dgv.Rows.Add("NG数", 0);
            dgv.Rows.Add("良率", 0);
        }

        void _updateDgvData()
        {
            // 預留邏輯
        }

        void dgvDataReset()
        {
            int i = 0;
            while (i < dgv.RowCount)
            {
                dgv.Rows[i].Cells[1].Value = 0;
                i++;
            }
        }
        #endregion

        #region Business Logic Methods (Barcode, Lot, Timer)
        public string GetProductBarcode()
        {
            return txtProductBarcode.Text.Trim();
        }
        public string GetOPBarcode()
        {
            return txtOPBarcode.Text.Trim();
        }
        public void SetEnable(bool isendable)
        {
            panel1.Enabled = isendable;
        }
        public void SetDuriation(string inputstr)
        {
            lblDuriation.Text = inputstr;
        }
        public void StartTime()
        {
            m_dtStart = DateTime.Now;
            m_Start = 1;
            SetProductBarcode("");
        }
        public void StopTime()
        {
            m_Start = -1;
        }
        public void SetProductBarcode(string eBarcode)
        {
            txtProductBarcode.Text = eBarcode;
            txtProductBarcode.Focus();
        }
        void UpdateLotData()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)UpdateLotData);
            }
            else
            {
                var plcIO = _plcIO; //  MACHINE?.PLCIO;
                if (plcIO != null)
                {
                    txtStripID.Text = plcIO.sStripID;
                    txtLotID.Text = plcIO.sLotID;
                }
            }
        }
        public string ConvertToString(TimeSpan tp)
        {
            string Str = "";
            Str += tp.Hours.ToString("00") + ":";
            Str += tp.Minutes.ToString("00") + ":";
            Str += tp.Seconds.ToString("00");
            return Str;
        }
        #endregion

        #region UI Shinnig Engine
        public void StartShinnig(bool ispass)
        {
            IsResultPass = ispass;
            _shinningProcess.Start();
        }
        void ShinningTick()
        {
            ProcessClass Process = _shinningProcess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:
                        if (ShinigCount == 0)
                        {
                            Process.TimeUnit = TimeUnitEnum.ms;
                            lblBigPass.Text = (IsResultPass ? "PASS" : "NG");
                        }

                        if (ShinigCount == 0 || Process.IsTimeup)
                        {
                            lblBigPass.ForeColor = (IsResultPass ? Color.Lime : Color.Red);
                            lblBigPass.Refresh();

                            Process.ID = 10;
                            Process.NextDuriation = 100;
                        }
                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            lblBigPass.ForeColor = (IsResultPass ? Color.Green : Color.DarkRed);
                            lblBigPass.Refresh();

                            ShinigCount++;

                            if (ShinigCount > ShiningTimes)
                            {
                                ShinigCount = 0;
                                OnTrigger(RunStatusEnum.SHINNIGEND);
                                Process.Stop();
                            }
                            else
                                Process.ID = 5;
                        }
                        break;
                }
            }
        }
        #endregion

        #region Main Form Tick Loop
        public void Tick()
        {
            ShinningTick();

            btnSingleSnap.BackColor = (m_SingleProcess.IsOn && m_SingleProcess.RelateString == "Snap" ? Color.Red : Color.FromArgb(192, 255, 192));
            btnSingleTest.BackColor = (m_SingleProcess.IsOn && m_SingleProcess.RelateString == "Test" ? Color.Red : Color.FromArgb(192, 255, 192));
            btnSingleOfflineTest.BackColor = (m_SingleProcess.IsOn && m_SingleProcess.RelateString == "OfflineTest" ? Color.Red : Color.FromArgb(192, 255, 192));

            //if (GaMvcConfig.UxInjection == null)
            {
                var plcIO = _plcIO; // MACHINE?.PLCIO;
                if (plcIO != null)
                {
                    bool bScanStart = plcIO.bScanStart;
                    bool bSoftwareReady = plcIO.bSoftwareReady;

                    btnSoftwareReady.BackColor = bSoftwareReady ? Color.Green : Color.FromArgb(192, 255, 192);
                    btnSoftwareReady.ForeColor = bSoftwareReady ? Color.Yellow : Color.Black;

                    if (bSoftwareReady)
                    {
                        if (plcIO.sRecipeName != RCPDB.RCPItemNow.Name)
                        {
                            plcIO.iRecipeNum = 0;
                            OnTrigger(RunStatusEnum.CHANGERECIPE);
                        }
                    }

                    if (bScanStart)
                    {
                        if (txtStripID.Tag == null)
                        {
                            txtStripID.Tag = "bScanStart";
                            UpdateLotData();
                        }
                    }
                    else
                    {
                        txtStripID.Tag = null;
                    }
                }
            }
        }
        #endregion

        #region Result Output & File Operations
        public void ClearResult()
        {
            IsResultPass = false;
            txtResult.Clear();
        }
        public void SetResultLine(string str)
        {
            txtResult.AppendText(str + Environment.NewLine);
        }
        public void SetResultText(string str)
        {
            txtResult.Text = str;
            txtResult.Refresh();
        }
        public void SaveResultLog(string filepath)
        {
            JzTools.SaveData(txtResult.Text, filepath);
        }
        public string SaveFilePicker(string DefaultPath, string DefaultName)
        {
            string retStr = "";
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = DefaultPath;
            dlg.FileName = DefaultName;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                retStr = dlg.FileName;
            }
            return retStr;
        }
        #endregion

        #region Custom Events & Triggers
        public delegate void TriggerHandler(RunStatusEnum Status);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(RunStatusEnum Status)
        {
            if (TriggerAction != null)
            {
                TriggerAction(Status);
            }
        }

        public delegate void BarcodeHandler(string barcode);
        public event BarcodeHandler BarcodeAction;
        public void OnBarcode(string barcode)
        {
            if (BarcodeAction != null)
            {
                BarcodeAction(barcode);
            }
        }

        public delegate void RunHandler(RunStatusEnum Status, string opstring);
        public event RunHandler RunAction;
        public void OnTrigger(RunStatusEnum Status, string opstring)
        {
            if (RunAction != null)
            {
                RunAction(Status, opstring);
            }
        }
        #endregion

        #region AUTO_LAYOUT
        void RunUI_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                _auto_layout();
            }
            catch
            {
            }
        }
        void _auto_layout()
        {
#if OPT_LETIAN_AUTO_LAYOUT
            foreach (var c in new Control[] { groupBox2, textBox1, textBox2, textBox3, label4, button1, button2, button6 })
            {
                var rcc = c.Parent.ClientRectangle;
                c.Width = rcc.Width - c.Left * 2;
            }
            label2.Width = label4.Right - label2.Left;
#endif
        }
        #endregion

        #region Localization
        //private string ToChangeLanguage(string eText)
        //{
        //    string retStr = eText;
        //    retStr = LanguageExClass.Instance.GetLanguageText(eText);
        //    return retStr;
        //}
        #endregion
    }
}
