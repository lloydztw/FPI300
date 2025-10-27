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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Traveller106;
using VsCommon.ControlSpace.MachineSpace;
using LaserAlignDX.ControlSpace.MachineSpace;
//using System.Reflection.Emit;
using System.Runtime.InteropServices;
using LaserAlignDX.ControlSpace.IOSpace;
using System.Threading;

namespace LaserAlignDX.UISpace.CtrlSpace
{
    enum TagEnum
    {
        TOPLIGHT,
        FRONTLIGHT,
        BACKLIGHT,
        READY,
        BUSY,
        PASS,
        FAIL,
        RECONNECTSERVER,
        TCPCOMPLETE,

        GETIMAGEOK,
        GETIMAGEINDEX,
        RECONNECT_HANDLE_SERVER,
        CIPMAPPING,
    }

    public partial class MainX2Ctrl : UserControl, IoPanelUI
    {
        VersionEnum VERSION;
        OptionEnum OPTION;
        JzTransparentPanel tpnlCover;
        JzTimes myTime;
        MainX2MachineClass MACHINE;
        //Label lblCalibration;
        JzTimes myTimeForHeart;

        System.Threading.Thread m_ThreadPlc = null;
        bool m_ThRunning = false;

        MainX2IOClass PLCIO
        {
            get
            {
                return MACHINE.PLCIO;
            }
        }

        ClientSocket X6_LASER_CLIENT
        {
            get { return Traveller106.Universal.X6_LASER_CLIENT; }
        }
        ClientSocket X6_HANDLE_CLIENT
        {
            get { return Traveller106.Universal.X6_HANDLE_CLIENT; }
        }

        Label lblIsStart;
        Label lblIsGetImage;
        Label lblIsGetImageReset;

        Label lblTopLight;
        Label lblFrontLight;
        Label lblBackLight;
        Label lblReady;
        Label lblBusy;
        Label lblPass;
        Label lblFail;
        Label lblGetImageOK;
        Label lblGetImageIndex;
        Label lblHandlerOK;
        Label lblTcpComplete;

        Label lblReConnectServer;
        Label lblReConnectHandleServer;

        public MainX2Ctrl()
        {
            InitializeComponent();
            InitUI();

            if (!DesignMode)
            {
                HandleDestroyed += (s, e) => MyDispose();
            }
        }
        void InitUI()
        {
            lblIsStart = label1;

            lblTopLight = label2;
            lblFrontLight = label3;
            lblBackLight = label6;
            lblReady = label7;
            lblBusy = label8;
            lblPass = label9;
            lblFail = label10;
            lblReConnectServer = label4;
            lblIsGetImage = label5;
            lblGetImageOK = label11;
            //lblGetImageIndex = label12;
            lblIsGetImageReset = label13;
            lblReConnectHandleServer = label14;
            lblHandlerOK = label15;
            lblTcpComplete = label16;
            //lblCipMapping = label17;

            lblTopLight.Tag = TagEnum.TOPLIGHT;
            lblFrontLight.Tag = TagEnum.FRONTLIGHT;
            lblBackLight.Tag = TagEnum.BACKLIGHT;
            lblReady.Tag = TagEnum.READY;
            lblBusy.Tag = TagEnum.BUSY;
            lblPass.Tag = TagEnum.PASS;
            lblFail.Tag = TagEnum.FAIL;
            lblReConnectServer.Tag = TagEnum.RECONNECTSERVER;
            lblGetImageOK.Tag = TagEnum.GETIMAGEOK;
            //lblGetImageIndex.Tag = TagEnum.GETIMAGEINDEX;
            lblReConnectHandleServer.Tag = TagEnum.RECONNECT_HANDLE_SERVER;
            lblTcpComplete.Tag = TagEnum.TCPCOMPLETE;
            //lblCipMapping.Tag = TagEnum.CIPMAPPING;

            lblTopLight.DoubleClick += lbl_DoubleClick;
            lblFrontLight.DoubleClick += lbl_DoubleClick;
            lblBackLight.DoubleClick += lbl_DoubleClick;
            lblReady.DoubleClick += lbl_DoubleClick;
            lblBusy.DoubleClick += lbl_DoubleClick;
            lblPass.DoubleClick += lbl_DoubleClick;
            lblFail.DoubleClick += lbl_DoubleClick;
            lblReConnectServer.DoubleClick += lbl_DoubleClick;
            lblGetImageOK.DoubleClick += lbl_DoubleClick;
            //lblGetImageIndex.DoubleClick += lbl_DoubleClick;
            lblTcpComplete.DoubleClick += lbl_DoubleClick;
            lblTcpComplete.BackColor = Color.Black;
        }

        Control IoPanelUI.Window => this;
        void IoPanelUI.Initial(VersionEnum version, OptionEnum option, object machine)
        {
            Initial(version, option, (MainX2MachineClass)machine);
        }

        private void lbl_DoubleClick(object sender, EventArgs e)
        {
            TagEnum KEYS = (TagEnum)((Label)sender).Tag;

            switch (KEYS)
            {
                case TagEnum.TOPLIGHT:
                    PLCIO.TopLight = !PLCIO.TopLight;
                    break;
                case TagEnum.FRONTLIGHT:
                    PLCIO.FrontLight = !PLCIO.FrontLight;
                    break;
                case TagEnum.BACKLIGHT:
                    PLCIO.BackLight = !PLCIO.BackLight;
                    break;
                case TagEnum.READY:
                    PLCIO.Ready = !PLCIO.Ready;
                    break;
                case TagEnum.BUSY:
                    PLCIO.Busy = !PLCIO.Busy;
                    break;
                case TagEnum.PASS:
                    PLCIO.Pass = !PLCIO.Pass;
                    break;
                case TagEnum.FAIL:
                    PLCIO.Fail = !PLCIO.Fail;
                    break;
                case TagEnum.GETIMAGEOK:
                    PLCIO.GetImageOK = !PLCIO.GetImageOK;
                    break;
                case TagEnum.GETIMAGEINDEX:
                    //JetEazy.PlugSpace.CamActClass.Instance.ResetStepCurrent();
                    break;
                case TagEnum.RECONNECTSERVER:


                    //if (DialogResult.OK != MessageBox.Show("是否重新连接Server ？", "重连Server", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
                    //{
                    //    return;
                    //}
                    //m_ReConnectIndex = 0;
                    //m_ReConnecting = true;
                    //ClientSocket.Instance.Host = INI.tcp_ip;
                    //ClientSocket.Instance.Port = INI.tcp_port;
                    //int iret = ClientSocket.Instance.ReConnectServer();
                    //if (iret != 0)
                    //{
                    //    m_ReConnectIndex = 10;
                    //    MessageBox.Show("重新连接服务器错误，请检查。", "重连Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //}
                    //m_ReConnecting = false;


                    break;
                case TagEnum.TCPCOMPLETE:

                    //int _currentStep = CamActClass.Instance.StepCurrent;
                    //_tcpSendCompleteOKSign(1, _currentStep, -1);

                    break;
                case TagEnum.CIPMAPPING:

                    //OnTrigger(ActionEnum.ACT_CIPMAPPING, "M");

                    break;
            }

        }

        public void Initial(VersionEnum version, OptionEnum option, MainX2MachineClass machine)
        {
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

            myTime = new JzTimes();
            myTime.Cut();

            myTimeForHeart = new JzTimes();
            myTimeForHeart.Cut();

            SetEnable(false);

            lblReConnectServer.BackColor = Color.Green;
            lblReConnectServer.Text = "Server打标连接成功";

            lblReConnectHandleServer.BackColor = Color.Green;
            lblReConnectHandleServer.Text = "ServerHandle连接成功";

            //lblReConnectServer.BackColor = (ClientSocket.Instance.IsConnecting ? Color.Green : Color.Red);
            //lblReConnectServer.BackColor = (ClientSocket.Instance.IsConnecting ? Color.Green : Color.Red);
            //if (Universal.m_UseCommToDLHandle)
            {
                updateReConnectServerUI(X6_LASER_CLIENT.IsConnecting);
                X6_LASER_CLIENT.TriggerStringAction += X6_LASER_CLIENT_TriggerStringAction;

                updateReConnectHandleServerUI(X6_HANDLE_CLIENT.IsConnecting);
                X6_HANDLE_CLIENT.TriggerStringAction += X6_HANDLE_CLIENT_TriggerStringAction;
            }

            if (m_ThreadPlc == null)
            {
                m_ThRunning = true;
                m_ThreadPlc = new System.Threading.Thread(new System.Threading.ThreadStart(PlcTick));
                m_ThreadPlc.IsBackground = true;
                m_ThreadPlc.Start();
            }
        }

        public void SetEnable(bool isendable)
        {
            tpnlCover.Visible = !isendable;

            Color fillcolor = SystemColors.Control;

            if (!isendable)
                fillcolor = Color.Silver;
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

        private void X6_HANDLE_CLIENT_TriggerStringAction(string opstr)
        {
            string[] str = opstr.Split(',');
            switch (str[0])
            {
                case "S"://状态
                    //lblReConnectServer.BackColor = (str[1] == "OK" ? Color.Green : Color.Red);
                    updateReConnectHandleServerUI(str[1] == "OK");
                    break;
            }
        }

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
        private void updateReConnectHandleServerUI(bool bOK)
        {

            try
            {
                this.Invoke(new Action(() =>
                {
                    lblReConnectHandleServer.BackColor = (bOK ? Color.Green : Color.Red);
                    lblReConnectHandleServer.Text = (bOK ? "ServerHandle连接成功" : "ServerHandle连接失败");
                }));
            }
            catch
            {

            }
        }

        int m_ReConnectIndex = 0;
        int m_ReConnectCount = 10;
        bool m_ReConnecting = false;
        JzTimes m_ReConnectTime = new JzTimes();

        bool m_ReHandleConnecting = false;
        JzTimes m_ReHandleConnectTime = new JzTimes();

        #endregion

        public void Tick()
        {
            //if (myTimeForHeart.msDuriation >= 2000)
            //{
            //    myTimeForHeart.Cut();

            //    MACHINE.PLCIO.Heart = !MACHINE.PLCIO.Heart;
            //}

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

                lblReConnectHandleServer.Visible = INI.Instance.tcp_handle_open;
                #region handle服务器重连
                if (X6_HANDLE_CLIENT.IsConnecting)
                {
                    m_ReHandleConnectTime.Cut();
                }
                else
                {
                    if (m_ReHandleConnectTime.msDuriation > 3 * 1000)
                    {
                        m_ReHandleConnectTime.Cut();
                        if (!m_ReHandleConnecting)
                        {
                            m_ReHandleConnecting = true;
                            //m_ReConnectIndex++;

                            lblReConnectHandleServer.BackColor = Color.Red;
                            lblReConnectHandleServer.Text = "ServerHandle重连中";

                            System.Threading.Thread thread_DL_ReConnectServer = new System.Threading.Thread(_reConnectHandleServer);
                            thread_DL_ReConnectServer.Start();
                        }
                    }
                }
                #endregion

                myTime.Cut();

                //lblHeart.BackColor = (MACHINE.PLCIO.Heart ? Color.Green : Color.Black);
                //lblSoftwareReady.BackColor = (MACHINE.PLCIO.SoftwareReady ? Color.Green : Color.Black);
                //lblLineScanStart.BackColor = (MACHINE.PLCIO.LineScanStart ? Color.Green : Color.Black);
                //lblLineScanReady.BackColor = (MACHINE.PLCIO.LineScanReady ? Color.Green : Color.Black);
                //lblLineScanDone.BackColor = (MACHINE.PLCIO.LineScanDone ? Color.Green : Color.Black);
                //lblLineScanAutoCali.BackColor = (MACHINE.PLCIO.ADR_LineScannWhatFor == 1 ? Color.Green : Color.Black);
                //lblLineScanResult.BackColor = (MACHINE.PLCIO.LineScanResult ? Color.Green : Color.Black);
                //lblLinescanBarcode.Text = MACHINE.PLCIO.ADR_LineScanBarcode;

                lblIsGetImage.BackColor = (PLCIO.IsGetImage ? Color.Green : Color.Black);
                lblIsStart.BackColor = (PLCIO.IsStart ? Color.Green : Color.Black);
                lblIsGetImageReset.BackColor = (PLCIO.IsGetImageReset ? Color.Green : Color.Black);
                lblTopLight.BackColor = (PLCIO.TopLight ? Color.Green : Color.Black);
                lblFrontLight.BackColor = (PLCIO.FrontLight ? Color.Green : Color.Black);
                lblBackLight.BackColor = (PLCIO.BackLight ? Color.Green : Color.Black);
                lblReady.BackColor = (PLCIO.Ready ? Color.Green : Color.Black);
                lblBusy.BackColor = (PLCIO.Busy ? Color.Green : Color.Black);
                lblPass.BackColor = (PLCIO.Pass ? Color.Green : Color.Black);
                lblFail.BackColor = (PLCIO.Fail ? Color.Red : Color.Black);
                lblGetImageOK.BackColor = (PLCIO.GetImageOK ? Color.Green : Color.Black);
                //lblGetImageIndex.BackColor = Color.Black;
                //lblGetImageIndex.Text = JetEazy.PlugSpace.CamActClass.Instance.StepCurrent.ToString() + " Total [" +
                //    JetEazy.PlugSpace.CamActClass.Instance.StepCount.ToString() + "]";

                //if (INI.IsReadHandlerOKSign && !INI.IsNoUseHandlerOKSign)
                //    lblHandlerOK.BackColor = (PLCIO.IsHandlerOK ? Color.Green : Color.Black);

            }
        }

        public void PlcTick()
        {
            while (m_ThRunning)
            {
                MACHINE.Tick();
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// 释放资源并关闭线程
        /// </summary>
        public void MyDispose()
        {
            MACHINE.PLCIO.Ready = false;
            MACHINE.PLCIO.Busy = false;
            PLCIO.GetImageOK = false;

            MACHINE.PLCIO.Pass = false;
            MACHINE.PLCIO.Fail = false;

            MACHINE.PLCIO.TopLight = false;
            MACHINE.PLCIO.FrontLight = false;
            MACHINE.PLCIO.BackLight = false;

            m_ThRunning = false;
            if (m_ThreadPlc != null)
            {
                m_ThreadPlc.Abort();
                m_ThreadPlc = null;
            }
        }

        private void _reConnectServer()
        {
            X6_LASER_CLIENT.Host = INI.Instance.tcp_ip;
            X6_LASER_CLIENT.Port = INI.Instance.tcp_port;
            int iret = X6_LASER_CLIENT.ReConnectServer();
            m_ReConnecting = false;
        }
        private void _reConnectHandleServer()
        {
            X6_HANDLE_CLIENT.Host = INI.Instance.tcp_handle_ip;
            X6_HANDLE_CLIENT.Port = INI.Instance.tcp_handle_port;
            int iret = X6_HANDLE_CLIENT.ReConnectServer();
            m_ReHandleConnecting = false;
        }
        private string ToChangeLanguage(string eText)
        {
            string retStr = eText;
            //retStr = LanguageExClass.Instance.GetLanguageText(eText);
            return retStr;
        }
    }
}
