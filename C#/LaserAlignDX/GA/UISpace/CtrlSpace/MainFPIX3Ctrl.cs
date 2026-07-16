using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.FormSpace;
using JetEazy.Lang;
using JetEazy.Machine;
using LeTian.AoiLib;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Traveller106;
using VsCommon.ControlSpace.MachineSpace;

namespace LaserAlignDX.UISpace.CtrlSpace
{
    public partial class MainFPIX3Ctrl : UserControl, IoPanelUI
    {
        #region NLOG
        NLog.Logger _NLOG => LtDebug.LOG;
        #endregion

        #region KERNEL_DATA
        VersionEnum VERSION;
        OptionEnum OPTION;
        MainFPIX3MachineClass MACHINE;
        #endregion

        #region TIMERS
        JzTimes myTime;
        JzTimes myTimeForHeart;
        #endregion

        #region PLC_POLLING_THREAD
        System.Threading.Thread m_ThreadPlc = null;
        bool m_ThRunning = false;
        #endregion

#if (OPT_X6_LASER_CLIENT)
        ClientSocket X6_LASER_CLIENT
        {
            get { return Traveller106.Universal.X6_LASER_CLIENT; }
        }
#endif

        #region GUI
        JzTransparentPanel tpnlCover;
        Label lblCalibration;

        //VsLight m_Light
        //{
        //    get { return MACHINE.LightCollection[0]; }
        //}

        Label lblHeart;
        //Label lblLineScanRecipe;
        Label lblSoftwareReady;
        Label lblLineScanStart;
        Label lblLineScanReady;
        Label lblLineScanDone;
        Label lblLineScanResult;

        Label lblReConnectServer;
        Label lblLineScanAutoCali;
        Label lblLinescanBarcode;

        Label lblIsUsedBarcode;
        Label lblIsUsedJudgeBarcode;
        Label lblScanState;
        Label lblFlyStart;
        Label lblFlyReady;
        Label lblFlyDone;

        NumericUpDown numLightValue;
        Button btnOn;
        Button btnOff;

        Button btnSIMData;
        Button btnReady;
        Button btnCalib;
        #endregion

        public MainFPIX3Ctrl()
        {
            _TRACE($"{GetType().Name}.Ctor() ++");

            InitializeComponent();
            InitUI();

            if (!DesignMode)
            {
                HandleDestroyed += (s, e) => MyDispose();
            }

            _TRACE($"{GetType().Name}.Ctor() --");
        }
        void InitUI()
        {
            lblHeart = ioLabel2;
            //lblLineScanRecipe = label4;
            lblSoftwareReady = ioLabel1;
            lblLineScanStart = ioLabel3;
            lblLineScanReady = ioLabel4;
            lblLineScanDone = ioLabel5;
            lblLineScanResult = ioLabel9;
            lblReConnectServer = label9Ctr;
            lblLineScanAutoCali = ioLabel6;
            //lblLinescanBarcode = label2;

            btnReady = button1;
            btnSIMData = button6;

            lblIsUsedBarcode = ioLabel6;
            lblIsUsedJudgeBarcode = ioLabel7;
            lblScanState = ioLabel8;
            lblFlyStart = ioLabel10;
            lblFlyReady = ioLabel11;
            lblFlyDone = ioLabel12;

            btnCalib = btnGlobalCalib;

            //btnOn = button6Ctr;
            //btnOff = button1Ctr;
            //numLightValue = numericUpDown1Ctr;
        }

        Control IoPanelUI.Window => this;
        void IoPanelUI.Initial(VersionEnum version, OptionEnum option, object machine)
        {
            Initial(version, option, (MainFPIX3MachineClass)machine);
        }

        public void Initial(VersionEnum version, OptionEnum option, MainFPIX3MachineClass machine)
        {
            _TRACE($"{GetType().Name}.Initial( {machine} )");

            VERSION = version;
            OPTION = option;
            MACHINE = machine;

            //lblCalibration = label2Ctr;

            tpnlCover = new JzTransparentPanel();
            tpnlCover.BackColor = System.Drawing.Color.Transparent;
            tpnlCover.Location = new System.Drawing.Point(6, 30);
            tpnlCover.Name = "panel1";
            tpnlCover.Size = this.Size;
            tpnlCover.TabIndex = 0;
            this.Controls.Add(tpnlCover);
            tpnlCover.BringToFront();

            //lblCalibration.DoubleClick += LblCalibration_DoubleClick;

            lblHeart.DoubleClick += LblHeart_DoubleClick;
            lblSoftwareReady.DoubleClick += LblSoftwareReady_DoubleClick;
            lblLineScanReady.DoubleClick += LblLineScanReady_DoubleClick;
            lblLineScanDone.DoubleClick += LblLineScanDone_DoubleClick;
            lblLineScanResult.DoubleClick += LblLineScanResult_DoubleClick;

            //btnOn.Click += BtnOn_Click;
            //btnOff.Click += BtnOff_Click;
            //numLightValue.ValueChanged += NumLightValue_ValueChanged;

            btnReady.Click += BtnReady_Click;
            btnSIMData.Click += BtnSIMData_Click;
            btnCalib.Click += BtnCalib_Click;

            myTime = new JzTimes();
            myTime.Cut();

            myTimeForHeart = new JzTimes();
            myTimeForHeart.Cut();

            SetEnable(false);

            lblReConnectServer.BackColor = Color.Green;
            lblReConnectServer.Text = "Server打标连接成功";

            //numericUpDown1Ctr.Minimum = INI.Instance.LightMinValue;

            //lblReConnectHandleServer.BackColor = Color.Green;
            //lblReConnectHandleServer.Text = "ServerHandle连接成功";

            //lblReConnectServer.BackColor = (ClientSocket.Instance.IsConnecting ? Color.Green : Color.Red);
            //lblReConnectServer.BackColor = (ClientSocket.Instance.IsConnecting ? Color.Green : Color.Red);
            //if (Universal.m_UseCommToDLHandle)
            {
                //updateReConnectServerUI(X6_LASER_CLIENT.IsConnecting);
                //X6_LASER_CLIENT.TriggerStringAction += X6_LASER_CLIENT_TriggerStringAction;

                //updateReConnectHandleServerUI(X6_HANDLE_CLIENT.IsConnecting);
                //X6_HANDLE_CLIENT.TriggerStringAction += X6_HANDLE_CLIENT_TriggerStringAction;
            }

            if (m_ThreadPlc == null)
            {
                m_ThRunning = true;
                m_ThreadPlc = new System.Threading.Thread(new System.Threading.ThreadStart(PlcTick));
                m_ThreadPlc.IsBackground = true;
                m_ThreadPlc.Start();
                _TRACE($"{GetType().Name}: the PLC thread created !");
            }
        }

        private void BtnCalib_Click(object sender, EventArgs e)
        {
            //using (var calibTool = new FormCalibration())
            //{
            //    calibTool.ShowDialog();
            //}
            GaMvcConfig.OpenCalibrationTool();
        }

        private void BtnSIMData_Click(object sender, EventArgs e)
        {
            //if (DialogResult.OK == VsMSG.Instance.Question($"{ToChangeLanguage("是否发送模拟数据到plc?")}"))
            if (VsMessageBox.Question(QMSG.Text(Prompts.Question_Send_Sim_Signal_To_PLC)) == DialogResult.Yes)
            {
                int[] iflyresults = new int[4] { 1, 2, 3, 1 };
                MACHINE.PLCIO.iFlyResult(iflyresults);
                float[] iflyoffsets = new float[12] { 0.1f, 0.1f, 5f, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                MACHINE.PLCIO.rOffset(iflyoffsets);
            }
        }

        private void BtnReady_Click(object sender, EventArgs e)
        {
            //MACHINE.PLCIO.bSoftwareReady = !MACHINE.PLCIO.bSoftwareReady;
            //MACHINE.PLCIO.bFlyReady = !MACHINE.PLCIO.bFlyReady;
        }

        private void NumLightValue_ValueChanged(object sender, EventArgs e)
        {
            //m_Light.CstLightValue = (int)numLightValue.Value;
            LightValue((int)numLightValue.Value);
        }

        private void BtnOff_Click(object sender, EventArgs e)
        {
            //m_Light.LightONOFF(false);
            LightOnOff(false);
        }

        private void BtnOn_Click(object sender, EventArgs e)
        {
            //m_Light.CstLightValue = (int)numLightValue.Value;
            //m_Light.LightONOFF(true);
            LightValue((int)numLightValue.Value);
            LightOnOff(true);
        }

        private void LblLineScanResult_DoubleClick(object sender, EventArgs e)
        {
            //MACHINE.PLCIO.LineScanResult = !MACHINE.PLCIO.LineScanResult;
        }

        private void LblLineScanDone_DoubleClick(object sender, EventArgs e)
        {
            MACHINE.PLCIO.bScanDone = !MACHINE.PLCIO.bScanDone;
        }

        private void LblLineScanReady_DoubleClick(object sender, EventArgs e)
        {
            MACHINE.PLCIO.bScanReady = !MACHINE.PLCIO.bScanReady;
        }

        private void LblSoftwareReady_DoubleClick(object sender, EventArgs e)
        {
            MACHINE.PLCIO.bSoftwareReady = !MACHINE.PLCIO.bSoftwareReady;
        }

        private void LblHeart_DoubleClick(object sender, EventArgs e)
        {
            MACHINE.PLCIO.bSyncClock = !MACHINE.PLCIO.bSyncClock;
        }

        public void SetEnable(bool isendable)
        {
            _TRACE($"{GetType().Name}.SetEnable({isendable})");

            tpnlCover.Visible = !isendable;

            Color fillcolor = SystemColors.Control;

            if (!isendable)
                fillcolor = Color.Silver;
        }
        public void SetEnable()
        {
            bool isenable = !tpnlCover.Visible;
            SetEnable(isenable);
            this.Invalidate();
        }

        #region 服务器重新连接

        private void X6_LASER_CLIENT_TriggerStringAction(string opstr)
        {
            string[] str = opstr.Split(',');
            switch (str[0])
            {
                case "S"://状态
                    //lblReConnectServer.BackColor = (str[1] == "OK" ? Color.Green : Color.Red);
                    updateReConnectServerUI(str[1] == "OK");
                    break;
            }
        }

        //private void X6_HANDLE_CLIENT_TriggerStringAction(string opstr)
        //{
        //    string[] str = opstr.Split(',');
        //    switch (str[0])
        //    {
        //        case "S"://状态
        //            //lblReConnectServer.BackColor = (str[1] == "OK" ? Color.Green : Color.Red);
        //            updateReConnectHandleServerUI(str[1] == "OK");
        //            break;
        //    }
        //}

        private void updateReConnectServerUI(bool bOK)
        {

            try
            {
                this.Invoke(new Action(() =>
                {
                    lblReConnectServer.BackColor = (bOK ? Color.Green : Color.Red);
                    lblReConnectServer.Text = (bOK ? ToChangeLanguage("Server打标连接成功") : ToChangeLanguage("Server打标连接失败"));
                }));
            }
            catch
            {

            }
        }
        //private void updateReConnectHandleServerUI(bool bOK)
        //{

        //    try
        //    {
        //        this.Invoke(new Action(() =>
        //        {
        //            lblReConnectHandleServer.BackColor = (bOK ? Color.Green : Color.Red);
        //            lblReConnectHandleServer.Text = (bOK ? "ServerHandle连接成功" : "ServerHandle连接失败");
        //        }));
        //    }
        //    catch
        //    {

        //    }
        //}

        int m_ReConnectIndex = 0;
        int m_ReConnectCount = 10;
        bool m_ReConnecting = false;
        JzTimes m_ReConnectTime = new JzTimes();

        //bool m_ReHandleConnecting = false;
        //JzTimes m_ReHandleConnectTime = new JzTimes();

        #endregion
        public void Tick()
        {
            //if (myTimeForHeart.msDuriation >= 1000)
            //{
            //    myTimeForHeart.Cut();

            //    MACHINE.PLCIO.bSyncClock = !MACHINE.PLCIO.bSyncClock;
            //}

            if (myTime.msDuriation > 100)
            {
                #region 打标服务器重连
                //if (X6_LASER_CLIENT != null)
                //{
                //    if (X6_LASER_CLIENT.IsConnecting)
                //    {
                //        m_ReConnectTime.Cut();
                //    }
                //    else
                //    {
                //        if (m_ReConnectTime.msDuriation > 3 * 1000)
                //        {
                //            m_ReConnectTime.Cut();
                //            if (!m_ReConnecting)
                //            {
                //                m_ReConnecting = true;
                //                //m_ReConnectIndex++;

                //                lblReConnectServer.BackColor = Color.Red;
                //                lblReConnectServer.Text = ToChangeLanguage("Server打标重连中");

                //                System.Threading.Thread thread_DL_ReConnectServer = new System.Threading.Thread(_reConnectServer);
                //                thread_DL_ReConnectServer.Start();
                //            }
                //        }
                //    }
                //}
                #endregion

                //lblReConnectHandleServer.Visible = INI.tcp_handle_open;
                #region handle服务器重连
                //if (X6_HANDLE_CLIENT.IsConnecting)
                //{
                //    m_ReHandleConnectTime.Cut();
                //}
                //else
                //{
                //    if (m_ReHandleConnectTime.msDuriation > 3 * 1000)
                //    {
                //        m_ReHandleConnectTime.Cut();
                //        if (!m_ReHandleConnecting)
                //        {
                //            m_ReHandleConnecting = true;
                //            //m_ReConnectIndex++;

                //            lblReConnectHandleServer.BackColor = Color.Red;
                //            lblReConnectHandleServer.Text = "ServerHandle重连中";

                //            System.Threading.Thread thread_DL_ReConnectServer = new System.Threading.Thread(_reConnectHandleServer);
                //            thread_DL_ReConnectServer.Start();
                //        }
                //    }
                //}
                #endregion

                myTime.Cut();

                //lblHeart.BackColor = (MACHINE.PLCIO.bSyncClock ? Color.Green : Color.Black);
                //lblSoftwareReady.BackColor = (MACHINE.PLCIO.bSoftwareReady ? Color.Green : Color.Black);
                //lblLineScanStart.BackColor = (MACHINE.PLCIO.bScanStart ? Color.Green : Color.Black);
                //lblLineScanReady.BackColor = (MACHINE.PLCIO.bScanReady ? Color.Green : Color.Black);
                //lblLineScanDone.BackColor = (MACHINE.PLCIO.bScanDone ? Color.Green : Color.Black);
                ////lblLineScanAutoCali.BackColor = (MACHINE.PLCIO.ADR_LineScannWhatFor == 1 ? Color.Green : Color.Black);
                ////lblLineScanResult.BackColor = (MACHINE.PLCIO.LineScanResult ? Color.Green : Color.Black);
                ////lblLinescanBarcode.Text = MACHINE.PLCIO.ADR_LineScanBarcode;

                //btnReady.BackColor = (MACHINE.PLCIO.bScanReady ? Color.Green : Color.FromArgb(192, 255, 192));

                //lblIsUsedBarcode.BackColor = (MACHINE.PLCIO.bQRUsed ? Color.Green : Color.Black);
                //lblIsUsedJudgeBarcode.BackColor = (MACHINE.PLCIO.bQRJudgeUsed ? Color.Green : Color.Black);
                ////lblScanState.BackColor = (MACHINE.PLCIO.bScanDone ? Color.Green : Color.Black);
                ////lblFlyStart.BackColor = (MACHINE.PLCIO.bScanDone ? Color.Green : Color.Black);

                //lblScanState.Text = $"{ToChangeLanguage("线扫状态")}{MACHINE.PLCIO.iScanStatus}";
                //lblFlyStart.Text = $"{ToChangeLanguage("飞拍轴")}{MACHINE.PLCIO.iFlyStart}";

                //lblFlyReady.BackColor = (MACHINE.PLCIO.bFlyReady ? Color.Green : Color.Black);
                //lblFlyDone.BackColor = (MACHINE.PLCIO.bFlyDone ? Color.Green : Color.Black);

                //switch (MACHINE.PLCIO.iScanResult)
                //{
                //    case 1:
                //        lblLineScanResult.BackColor = Color.Green;
                //        break;
                //    case 2:
                //        lblLineScanResult.BackColor = Color.Red;
                //        break;
                //    default:
                //        lblLineScanResult.BackColor = Color.Black;
                //        break;
                //}

            }
        }
        private void _updateUI()
        {
            if (!this.IsHandleCreated)
                return;

            this.Invoke(new Action(() =>
            {
                if (myTimeForHeart.msDuriation >= 1000)
                {
                    myTimeForHeart.Cut();

                    MACHINE.PLCIO.bSyncClock = !MACHINE.PLCIO.bSyncClock;
                }

                lblHeart.BackColor = (MACHINE.PLCIO.bSyncClock ? Color.Green : Color.Black);
                lblSoftwareReady.BackColor = (MACHINE.PLCIO.bSoftwareReady ? Color.Green : Color.Black);
                lblLineScanStart.BackColor = (MACHINE.PLCIO.bScanStart ? Color.Green : Color.Black);
                lblLineScanReady.BackColor = (MACHINE.PLCIO.bScanReady ? Color.Green : Color.Black);
                lblLineScanDone.BackColor = (MACHINE.PLCIO.bScanDone ? Color.Green : Color.Black);
                //lblLineScanAutoCali.BackColor = (MACHINE.PLCIO.ADR_LineScannWhatFor == 1 ? Color.Green : Color.Black);
                //lblLineScanResult.BackColor = (MACHINE.PLCIO.LineScanResult ? Color.Green : Color.Black);
                //lblLinescanBarcode.Text = MACHINE.PLCIO.ADR_LineScanBarcode;

                btnReady.BackColor = (MACHINE.PLCIO.bSoftwareReady ? Color.Green : Color.FromArgb(192, 255, 192));

                lblIsUsedBarcode.BackColor = (MACHINE.PLCIO.bQRUsed ? Color.Green : Color.Black);
                lblIsUsedJudgeBarcode.BackColor = (MACHINE.PLCIO.bQRJudgeUsed ? Color.Green : Color.Black);
                //lblScanState.BackColor = (MACHINE.PLCIO.bScanDone ? Color.Green : Color.Black);
                //lblFlyStart.BackColor = (MACHINE.PLCIO.bScanDone ? Color.Green : Color.Black);

                //lblScanState.Text = $"{ToChangeLanguage("线扫状态")}{MACHINE.PLCIO.iScanStatus}";
                //lblFlyStart.Text = $"{ToChangeLanguage("飞拍轴")}{MACHINE.PLCIO.iFlyStart}";
                SetPostfix(lblScanState, MACHINE.PLCIO.iScanStatus);
                SetPostfix(lblFlyStart, MACHINE.PLCIO.iFlyStart);

                lblFlyReady.BackColor = (MACHINE.PLCIO.bFlyReady ? Color.Green : Color.Black);
                lblFlyDone.BackColor = (MACHINE.PLCIO.bFlyDone ? Color.Green : Color.Black);

                switch (MACHINE.PLCIO.iScanResult)
                {
                    case 1:
                        lblLineScanResult.BackColor = Color.Green;
                        break;
                    case 2:
                        lblLineScanResult.BackColor = Color.Red;
                        break;
                    default:
                        lblLineScanResult.BackColor = Color.Black;
                        break;
                }
            }));
        }

        public void PlcTick()
        {
            while (m_ThRunning)
            {
                _updateUI();
                MACHINE.Tick();
                Thread.Sleep(50);
            }
        }

        protected void LightValue(int eVal)
        {
            foreach (var machine in MACHINE.LightCollection)
            {
                machine.CstLightValue = eVal;
            }
        }
        protected void LightOnOff(bool eOn)
        {
            foreach (var machine in MACHINE.LightCollection)
            {
                machine.LightONOFF(eOn);
            }
        }

        /// <summary>
        /// 释放资源并关闭线程
        /// </summary>
        public void MyDispose()
        {
            m_ThRunning = false;
            if (m_ThreadPlc != null)
            {
                m_ThreadPlc.Abort();
                m_ThreadPlc = null;
                _TRACE($"{GetType().Name}.MyDispose()");
            }
        }

        private void _reConnectServer()
        {
#if (OPT_X6_LASER_CLIENT)
            X6_LASER_CLIENT.Host = INI.Instance.tcp_ip;
            X6_LASER_CLIENT.Port = INI.Instance.tcp_port;
            int iret = X6_LASER_CLIENT.ReConnectServer();
            m_ReConnecting = false;
#endif
        }

#if (OPT_OLD_CODE)
        frmMSR mFromMSR = null;
        private void LblCalibration_DoubleClick(object sender, EventArgs e)
        {
            mFromMSR = new frmMSR();
            mFromMSR.ShowDialog();
        }
#endif

        private void SetPostfix(Control c, int value)
        {
            if (c.Text.Contains(":"))
            {
                var strs = c.Text.Split(':');
                c.Text = $"{strs[0].Trim()}:{value}";
            }
            else
            {
                c.Text = $"{c.Text}:{value}";
            }
        }

        private string ToChangeLanguage(string eText)
        {
            string retStr = eText;
            retStr = LanguageExClass.Instance.GetLanguageText(eText);
            return retStr;
        }

        private void _TRACE(string msg)
        {
            _NLOG.Info(msg);
        }
    }
}
