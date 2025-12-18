using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.DBSpace;
using JetEazy.EzImage;
using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.Utils;
using LaserAlignDX;
using LaserAlignDX.OPSpace.RecipeSpace;
using NeedleX.ProcessSpace;
using System;
using System.Drawing;
using System.Windows.Forms;
using TravellerMINIX6.ProcessSpace;
using VsCommon.ControlSpace.MachineSpace;

//using Mist.OPSpace;
//using Mist.DBSpace;
//using PhotoMachine.ControlSpace;

namespace PhotoMachine.UISpace
{
    public partial class RunUI : UserControl
    {
        const int ShinningDuriation = 50;
        const int ShiningTimes = 2;

        bool IsResultPass = false;

        //RESULTClass RESULT;
        //UseIOClass USEIO;

        public bool IsSaveRaw
        {
            get
            {
                return chkIsSaveRaw.Checked;
            }
        }
        public bool IsSaveNGRaw
        {
            get
            {
                return chkIsSaveNGRaw.Checked;
            }
        }
        public bool IsSaveDebug
        {
            get
            {
                return chkIsSaveDebug.Checked;
            }
        }

        int m_Start = -1;
        Timer m_CalTimer;
        Label lblBigPass;

        protected RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }

        RCPDBClass RCPDB
        {
            get
            {
                return Traveller106.Universal.RCPDB;
            }
        }

        protected MainFPIX3MachineClass MACHINE
        {
            get { return (MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection.MACHINE; }
        }

        TextBox txtProductBarcode;
        TextBox txtOPBarcode;
        Label lblDuriation;
        TextBox txtResult;

        CheckBox chkIsSaveDebug;
        CheckBox chkIsSaveRaw;
        CheckBox chkIsSaveNGRaw;

        Button btnSingleSnap;
        Button btnSingleTest;
        Button btnSaveImage;
        Button btnSingleOfflineTest;
        Button btnClearDataZero;
        Button btnSoftwareReady;
        Button btnAutoManual;

        JzToolsClass JzTools = new JzToolsClass();

        BaseProcess m_SingleProcess
        {
            get { return LineScanSingleProcess.Instance; }
        }

        //Language Setup
        JzLanguageClass myLanguage = new JzLanguageClass();

        protected IxLineScanCam IScanCam
        {
            get { return Traveller106.Universal.IxLineScan; }
        }

        string UIPath = "";
        int LanguageIndex = 0;

        VersionEnum VER = VersionEnum.STEROPES;
        OptionEnum OPT = OptionEnum.MAIN;

        public RunUI()
        {
            InitializeComponent();
            Initial();
        }
        void Initial()
        {
            lblBigPass = label4;
            lblDuriation = label2;

            txtOPBarcode = textBox1;
            txtProductBarcode = textBox3;
            txtResult = textBox2;

            chkIsSaveRaw = checkBox1;
            chkIsSaveNGRaw = checkBox2;
            chkIsSaveDebug = checkBox3;

            chkIsSaveRaw.Visible = false;
            chkIsSaveNGRaw.Visible = false;
            chkIsSaveDebug.Visible = false;

            btnSingleSnap = button6;
            btnSingleTest = button1;
            btnSaveImage = button2;
            btnSingleOfflineTest = button3;
            btnClearDataZero = button4;
            btnSoftwareReady = button5;
            //btnAutoManual = button7;

            btnSingleSnap.Click += BtnSingleSnap_Click;
            btnSingleTest.Click += BtnSingleTest_Click;
            btnSaveImage.Click += BtnSaveImage_Click;
            btnSingleOfflineTest.Click += BtnSingleOfflineTest_Click;
            btnClearDataZero.Click += BtnClearDataZero_Click;
            btnSoftwareReady.Click += BtnSoftwareReady_Click;
            //btnAutoManual.Click += BtnAutoManual_Click;

            txtProductBarcode.KeyDown += new KeyEventHandler(txtProductBarcode_KeyDown);
            txtOPBarcode.KeyDown += new KeyEventHandler(txtBarcode_KeyDown);
            SizeChanged += RunUI_SizeChanged;

            InitializeDataGridView();

            txtLotNo.ReadOnly = true;
            txtStripID.ReadOnly = true;

            if (!DesignMode)
            {
                HandleCreated += (s, e) =>
                {
                    GaMvcConfig.SysModel.AoiModel.OnLotDataChanged += AoiModel_OnLotDataChanged;
                };
            }
        }

        private void AoiModel_OnLotDataChanged(object sender, EventArgs e)
        {
            UpdateLotData();
        }

        private void BtnAutoManual_Click(object sender, EventArgs e)
        {
            
        }

        private void BtnSoftwareReady_Click(object sender, EventArgs e)
        {
            var plcIO = MACHINE?.PLCIO;
            if (plcIO == null) return;

            bool bReady = !plcIO.bSoftwareReady;    // Toggle bSoftwareReady

            //>>> plcIO.bScanStart = false;
            plcIO.bSoftwareReady = bReady;
            plcIO.bFlyReady = bReady;

            if (bReady)                             
            {
                //plcIO.bScanStart = false;
                //plcIO.bScanReady = false;
                //plcIO.bScanDone = false;
                //plcIO.iRecipeNum = 0;
                plcIO.ResetPlc(resetRecipeNum: true);
                OnTrigger(RunStatusEnum.CHANGERECIPE);
            }
        }

        private void BtnClearDataZero_Click(object sender, EventArgs e)
        {
            //dgvDataReset();
            //xRecipe.ResetZero();
            SetDuriation("0 s");
        }

        private void InitializeDataGridView()
        {
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;

            dgv.Columns["col1"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["col2"].SortMode = DataGridViewColumnSortMode.NotSortable;

            // 初始化数据
            dgv.Rows.Add("PASS数", 0);
            dgv.Rows.Add("NG数", 0);
            //dgv.Rows.Add("印字偏移", 0);
            //dgv.Rows.Add("印字缺失", 0);
            //dgv.Rows.Add("2D读取错误", 0);
            //dgv.Rows.Add("2D比对错误", 0);
            //dgv.Rows.Add("2D重复", 0);
            //dgv.Rows.Add("芯片数", 0);
            dgv.Rows.Add("良率", 0);

            //int i = 0;
            //foreach (DataGridViewRow row in dgv.Rows)
            //{
            //    row.DefaultCellStyle.BackColor = categoryColors[i];
            //    i++;
            //}
        }

        //// 定义不同错误类别的颜色
        //Color[] categoryColors =
        //{
        //    Color.Lime,     // PASS
        //    Color.Red,      // 印字错误
        //    Color.Violet,     // 印字偏移
        //    Color.Red, // 印字缺失
        //    Color.Fuchsia,    // 2D读取错误
        //    Color.Orange,   // 2D比对错误
        //    Color.LightPink,       // 2D重复
        //    Color.Gray,       // 芯片数
        //    Color.LightBlue,       // 良率
        //};
        void _updateDgvData()
        {
            //int i = 0;
            //while (i < xRecipe.AnalyzeDatas.Length - 1)
            //{
            //    dgv.Rows[i].Cells[1].Value = xRecipe.AnalyzeDatas[i];
            //    i++;
            //}
            //dgv.Rows[i].Cells[1].Value = $"{xRecipe.AnalyzeDatas[i].ToString("0.00")} %";
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

        private void BtnSingleOfflineTest_Click(object sender, EventArgs e)
        {
            if (!m_SingleProcess.IsOn)
                m_SingleProcess.Start("OfflineTest");
            else
                m_SingleProcess.Stop();
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
                // 注意: IScanCam.GetFreeImageBitmap() 所取得的 FreeBitmap 由 Camera Driver 自行維持其生命周期 
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
                    VsMessageBox.Info(ToChangeLanguage(msg) + Environment.NewLine + dstFilename);

                    GaUtil.SetCursor(this, oldCur);
                }
                srcBmp.Dispose();
            }
        }

        private void BtnSingleSnap_Click(object sender, EventArgs e)
        {
            if (!m_SingleProcess.IsOn)
                m_SingleProcess.Start("Snap");
            else
                m_SingleProcess.Stop();
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
        public string ConvertToString(TimeSpan tp)
        {
            string Str = "";

            Str += tp.Hours.ToString("00") + ":";
            Str += tp.Minutes.ToString("00") + ":";
            Str += tp.Seconds.ToString("00");

            return Str;
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

        //public void Initial(string uipath,
        //    int langindex,
        //    VersionEnum ver,
        //    OptionEnum opt,
        //    RESULTClass result,
        //    UseIOClass useio)
        //{
        //    UIPath = uipath;
        //    LanguageIndex = langindex;
        //    VER = ver;
        //    OPT = opt;
        //    RESULT = result;

        //    USEIO = useio;

        //}

        public void Initial(string uipath,
            int langindex,
            VersionEnum ver,
            OptionEnum opt)
            //RESULTClass result,
            //UseIOClass useio)
        {
            UIPath = uipath;
            LanguageIndex = langindex;
            VER = ver;
            OPT = opt;
            //RESULT = result;

            //USEIO = useio;

            m_CalTimer = new Timer();
            m_CalTimer.Interval = 1000;
            m_CalTimer.Enabled = true;
            m_CalTimer.Tick += M_CalTimer_Tick;

            //SetLotID(xRecipe.xLotNoStr);
            SetDuriation("0 s");
        }

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

        public void StartShinnig(bool ispass)
        {
            IsResultPass = ispass;
            ShinningProcess.Start();
        }
        public void SetDuriation(string inputstr)
        {
            lblDuriation.Text = inputstr;

            //dgv.Rows[0].Cells[1].Value = xRecipe.PassCount;
            //dgv.Rows[1].Cells[1].Value = xRecipe.NGCount;

            //if (xRecipe.PassCount + xRecipe.NGCount > 0)
            //    dgv.Rows[2].Cells[1].Value = (xRecipe.PassCount * 1.0 / (xRecipe.PassCount + xRecipe.NGCount) * 100).ToString("0.00") + " %";
            //else
            //    dgv.Rows[2].Cells[1].Value = 0;
        }
        DateTime m_dtStart = DateTime.Now;
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

        //public void SetLotID(string eLot)
        //{
        //    //txtLotNo.Text = eLot;
        //}
        //public void SetStripID(string eStrip)
        //{
        //    //txtStripID.Text = eStrip;
        //}
        void UpdateLotData()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)UpdateLotData);
            }
            else
            {
                var plcIO = MACHINE?.PLCIO;
                if (plcIO != null)
                {
                    txtStripID.Text = plcIO.sStripID;
                    txtLotNo.Text = plcIO.sLotID;
                }
            }
        }

        #region SHINING_MESS
        public bool IsShinning
        {
            get
            {
                return ShinningProcess.IsOn;
            }
        }
        int ShinigCount = 0;
        ProcessClass ShinningProcess = new ProcessClass();
        public void ShinningTick()
        {
            ProcessClass Process = ShinningProcess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        //lblBigPass.Visible = IsPass;
                        if (ShinigCount == 0)
                        {
                            Process.TimeUnit = TimeUnitEnum.ms;
                            lblBigPass.Text = (IsResultPass ? "PASS" : "NG");
                        }

                        if (ShinigCount == 0 || Process.IsTimeup)
                        {
                            lblBigPass.ForeColor = (IsResultPass ? Color.Lime : Color.Red);

                            //if (IsResultPass)
                            //    ShineGreen();
                            //else
                            //    ShineRed();

                            lblBigPass.Refresh();

                            Process.ID = 10;
                            Process.NextDuriation = 100;
                        }
                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            lblBigPass.ForeColor = (IsResultPass ? Color.Green : Color.DarkRed);

                            //ShineNothing();

                            lblBigPass.Refresh();

                            ShinigCount++;

                            if (ShinigCount > ShiningTimes)
                            {

                                ShinigCount = 0;
                                //OnTrigger((IsPass ? StatusEnum.CALPASS : StatusEnum.CALNG));

                                //OnTrigger(StatusEnum.CALEND);
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
        //void ShineGreen()
        //{
        //    USEIO.LEDGreen = true;
        //    USEIO.LEDRed = false;
        //    USEIO.LEDYellow = false;
        //}
        //void ShineRed()
        //{
        //    USEIO.LEDGreen = false;
        //    USEIO.LEDRed = true;
        //    USEIO.LEDYellow = false;
        //}
        //void ShineNothing()
        //{
        //    USEIO.LEDGreen = false;
        //    USEIO.LEDRed = false;
        //    USEIO.LEDYellow = false;
        //}
        #endregion

        public void Tick()
        {
            ShinningTick();

            btnSingleSnap.BackColor = (m_SingleProcess.IsOn && m_SingleProcess.RelateString == "Snap" ? Color.Red : Color.FromArgb(192, 255, 192));
            btnSingleTest.BackColor = (m_SingleProcess.IsOn && m_SingleProcess.RelateString == "Test" ? Color.Red : Color.FromArgb(192, 255, 192));
            btnSingleOfflineTest.BackColor = (m_SingleProcess.IsOn && m_SingleProcess.RelateString == "OfflineTest" ? Color.Red : Color.FromArgb(192, 255, 192));

            // 在此又 Polling PLC 了 !!!
            // polling PLC 必須要集中管理.
            var plcIO = MACHINE?.PLCIO;
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
                        //txtStripID.Text = plcIO.sStripID;
                        //txtLotNo.Text = plcIO.sLotID;
                        UpdateLotData();
                    }
                }
                else
                {
                    txtStripID.Tag = null;
                }
            }
        }

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

            //dlg.Filter = "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*";
            dlg.Filter = DefaultPath;
            dlg.FileName = DefaultName;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                retStr = dlg.FileName;
            }
            return retStr;
        }

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

        private string ToChangeLanguage(string eText)
        {
            string retStr = eText;
            retStr = LanguageExClass.Instance.GetLanguageText(eText);
            return retStr;
        }
    }
}