using JetEazy.BasicSpace;
using JetEazy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VsCommon.ControlSpace.MachineSpace;
using Traveller106.FormSpace;
using Traveller106;
using LaserAlignDX.FormSpace;
using JetEazy.ControlSpace.PLCSpace;

namespace LaserAlignDX.UISpace.CtrlSpace
{
    public partial class MainX1Ctrl : UserControl, IoPanelUI
    {
        VersionEnum VERSION;
        OptionEnum OPTION;
        JzTransparentPanel tpnlCover;
        JzTimes myTime;
        MainX1MachineClass MACHINE;
        Label lblCalibration;
        JzTimes myTimeForHeart;

        ClientSocket X6_LASER_CLIENT
        {
            get { return Traveller106.Universal.X6_LASER_CLIENT; }
        }

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

        NumericUpDown numLightValue;
        Button btnOn;
        Button btnOff;

        public MainX1Ctrl()
        {
            InitializeComponent();
            InitUI();
        }
        void InitUI()
        {
            lblHeart = label4Ctr;
            //lblLineScanRecipe = label4;
            lblSoftwareReady = label3Ctr;
            lblLineScanStart = label1Ctr;
            lblLineScanReady = label5Ctr;
            lblLineScanDone = label6Ctr;
            lblLineScanResult = label7Ctr;
            lblReConnectServer = label9Ctr;
            lblLineScanAutoCali = label1;
            lblLinescanBarcode = label2;

            btnOn = button6Ctr;
            btnOff = button1Ctr;
            numLightValue = numericUpDown1Ctr;
        }

        Control IoPanelUI.Window => this;
        void IoPanelUI.Initial(VersionEnum version, OptionEnum option, object machine)
        {
            Initial(version, option, (MainX1MachineClass)machine);
        }

        public void Initial(VersionEnum version, OptionEnum option, MainX1MachineClass machine)
        {
            VERSION = version;
            OPTION = option;
            MACHINE = machine;

            lblCalibration = label2Ctr;

            tpnlCover = new JzTransparentPanel();
            tpnlCover.BackColor = System.Drawing.Color.Transparent;
            tpnlCover.Location = new System.Drawing.Point(6, 30);
            tpnlCover.Name = "panel1";
            tpnlCover.Size = this.Size;
            tpnlCover.TabIndex = 0;
            this.Controls.Add(tpnlCover);
            tpnlCover.BringToFront();

            lblCalibration.DoubleClick += LblCalibration_DoubleClick;

            lblHeart.DoubleClick += LblHeart_DoubleClick;
            lblSoftwareReady.DoubleClick += LblSoftwareReady_DoubleClick;
            lblLineScanReady.DoubleClick += LblLineScanReady_DoubleClick;
            lblLineScanDone.DoubleClick += LblLineScanDone_DoubleClick;
            lblLineScanResult.DoubleClick += LblLineScanResult_DoubleClick;

            btnOn.Click += BtnOn_Click;
            btnOff.Click += BtnOff_Click;
            numLightValue.ValueChanged += NumLightValue_ValueChanged;

            myTime = new JzTimes();
            myTime.Cut();

            myTimeForHeart = new JzTimes();
            myTimeForHeart.Cut();

            SetEnable(false);

            lblReConnectServer.BackColor = Color.Green;
            lblReConnectServer.Text = "Server打标连接成功";

            numericUpDown1Ctr.Minimum = INI.Instance.LightMinValue;

            //lblReConnectHandleServer.BackColor = Color.Green;
            //lblReConnectHandleServer.Text = "ServerHandle连接成功";

            //lblReConnectServer.BackColor = (ClientSocket.Instance.IsConnecting ? Color.Green : Color.Red);
            //lblReConnectServer.BackColor = (ClientSocket.Instance.IsConnecting ? Color.Green : Color.Red);
            //if (Universal.m_UseCommToDLHandle)
            {
                updateReConnectServerUI(X6_LASER_CLIENT.IsConnecting);
                X6_LASER_CLIENT.TriggerStringAction += X6_LASER_CLIENT_TriggerStringAction;

                //updateReConnectHandleServerUI(X6_HANDLE_CLIENT.IsConnecting);
                //X6_HANDLE_CLIENT.TriggerStringAction += X6_HANDLE_CLIENT_TriggerStringAction;
            }
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
            MACHINE.PLCIO.LineScanDone = !MACHINE.PLCIO.LineScanDone;
        }

        private void LblLineScanReady_DoubleClick(object sender, EventArgs e)
        {
            MACHINE.PLCIO.LineScanReady = !MACHINE.PLCIO.LineScanReady;
        }

        private void LblSoftwareReady_DoubleClick(object sender, EventArgs e)
        {
            MACHINE.PLCIO.SoftwareReady = !MACHINE.PLCIO.SoftwareReady;
        }

        private void LblHeart_DoubleClick(object sender, EventArgs e)
        {
            MACHINE.PLCIO.Heart = !MACHINE.PLCIO.Heart;
        }

        public void SetEnable(bool isendable)
        {
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
            if (myTimeForHeart.msDuriation >= 2000)
            {
                myTimeForHeart.Cut();

                MACHINE.PLCIO.Heart = !MACHINE.PLCIO.Heart;
            }

            if (myTime.msDuriation > 100)
            {
                #region 打标服务器重连
                if (X6_LASER_CLIENT != null)
                {
                    if (X6_LASER_CLIENT.IsConnecting)
                    {
                        m_ReConnectTime.Cut();
                    }
                    else
                    {
                        if (m_ReConnectTime.msDuriation > 3 * 1000)
                        {
                            m_ReConnectTime.Cut();
                            if (!m_ReConnecting)
                            {
                                m_ReConnecting = true;
                                //m_ReConnectIndex++;

                                lblReConnectServer.BackColor = Color.Red;
                                lblReConnectServer.Text = ToChangeLanguage("Server打标重连中");

                                System.Threading.Thread thread_DL_ReConnectServer = new System.Threading.Thread(_reConnectServer);
                                thread_DL_ReConnectServer.Start();
                            }
                        }
                    }
                }
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

                lblHeart.BackColor = (MACHINE.PLCIO.Heart ? Color.Green : Color.Black);
                lblSoftwareReady.BackColor = (MACHINE.PLCIO.SoftwareReady ? Color.Green : Color.Black);
                lblLineScanStart.BackColor = (MACHINE.PLCIO.LineScanStart ? Color.Green : Color.Black);
                lblLineScanReady.BackColor = (MACHINE.PLCIO.LineScanReady ? Color.Green : Color.Black);
                lblLineScanDone.BackColor = (MACHINE.PLCIO.LineScanDone ? Color.Green : Color.Black);
                lblLineScanAutoCali.BackColor = (MACHINE.PLCIO.ADR_LineScannWhatFor == 1 ? Color.Green : Color.Black);
                //lblLineScanResult.BackColor = (MACHINE.PLCIO.LineScanResult ? Color.Green : Color.Black);
                //lblLinescanBarcode.Text = MACHINE.PLCIO.ADR_LineScanBarcode;
                switch (MACHINE.PLCIO.LineScanResult)
                {
                    case "1":
                        lblLineScanResult.BackColor = Color.Green;
                        break;
                    case "2":
                        lblLineScanResult.BackColor = Color.Red;
                        break;
                    default:
                        lblLineScanResult.BackColor = Color.Black;
                        break;
                }

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

        private void _reConnectServer()
        {
            X6_LASER_CLIENT.Host = INI.Instance.tcp_ip;
            X6_LASER_CLIENT.Port = INI.Instance.tcp_port;
            int iret = X6_LASER_CLIENT.ReConnectServer();
            m_ReConnecting = false;
        }
        frmMSR mFromMSR = null;
        private void LblCalibration_DoubleClick(object sender, EventArgs e)
        {
            mFromMSR = new frmMSR();
            mFromMSR.ShowDialog();
        }

        private string ToChangeLanguage(string eText)
        {
            string retStr = eText;
            retStr = LanguageExClass.Instance.GetLanguageText(eText);
            return retStr;
        }
    }
}
