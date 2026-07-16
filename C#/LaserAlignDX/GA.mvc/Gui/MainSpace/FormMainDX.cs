using Eazy_Project_III;
using Eazy_Project_III.FormSpace;
using Eazy_Project_III.UISpace;
using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.DBSpace;
using JetEazy.FormSpace;
using JetEazy.Lang;
using JetEazy.UISpace;
using JetEazy.Utils;
using LaserAlignDX;
//using JzDisplay;
using LaserAlignDX.OPSpace.RecipeSpace;
using LaserAlignDX.UISpace;
using LeTian.AoiLib;
using NeedleX.ProcessSpace;
using PhotoMachine.UISpace;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TravellerMINIX6.ProcessSpace;
using VsCommon.ControlSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace Traveller106
{
    public partial class FormMainDX : Form
    {
        JetEazy.VersionEnum VERSION
        {
            get
            {
                return Universal.VERSION;
            }
        }
        JetEazy.OptionEnum OPTION
        {
            get
            {
                return Universal.OPTION;
            }
        }

        #region NLOG
        NLog.Logger _NLOG => LtDebug.LOG;
        #endregion

        #region GUI_LINKS
        EssUI ESSUI => essUI1;
        RunUI RUNUI => runUI1;
        RcpUI RCPUI => rcpUI1;
        IniUI SETUPUI => iniUI1;
        CtrlUI CTRLUI => ctrlUI1;
        MainControlUI MAINUI => mainControlUI1;
        #endregion

        #region TIMER
        Timer mMainTick;
        //JzTimes mImageTime = new JzTimes();
        //string MoveString = "";
        //bool IsLiveCapturing = true;
        #endregion

        #region DB
        AccDBClass ACCDB
        {
            get
            {
                return Universal.ACCDB;
            }
        }
        EsssDBClass ESSDB
        {
            get
            {
                return Universal.ESSDB;
            }
        }
        RCPDBClass RCPDB
        {
            get
            {
                return Universal.RCPDB;
            }
        }
        RUNDBClass RUNDB
        {
            get
            {
                return Universal.RUNDB;
            }
        }
        RCPItemClass RCPItemNow
        {
            get
            {
                return RCPDB.RCPItemNow;
            }
        }
        #endregion

        #region MACHINE
        MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }
        protected MainFPIX3MachineClass MACHINE
        {
            get { return (MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection.MACHINE; }
        }
        #endregion

        #region RECIPE
        protected RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        #endregion

        public FormMainDX()
        {
            InitializeComponent();

            #region RESERVED_CODE
            //if (!_getMxComponent())
            //{
            //    //LogClass.Instance.Log("Mx加载错误");
            //    MessageBox.Show("初始化错误", "Initial MxComponent", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    //this.Close();
            //    Application.Exit();
            //    return;
            //}
            #endregion

            //this.StartPosition = FormStartPosition.CenterScreen;
            //this.Load += MainForm_Load;
            //this.FormClosed += MainForm_FormClosed;
            //this.SizeChanged += MainForm_SizeChanged;

            if (!DesignMode)
            {
                this.Size = new Size(100, 100);
                this.StartPosition = FormStartPosition.CenterScreen;
                this.Load += MainForm_Load;
                this.FormClosed += MainForm_FormClosed;
                this.SizeChanged += (s, e) => auto_layout();
                auto_layout();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //(0) Banner
            BannerForm.ShowBanner();
            GaUtil.SetCursor(this, Cursors.AppStarting);

#if (false)
            //(0.1) MYDECODE
            JetEazy.Universal.MYDECODE = Universal.MAINPATH + @"\WORK\";

            //(1) 初始化 JzDisplay
            //---------------------------------------------------------------------
            // 注意:
            //  使用 dispUI = new DispUI() 動態生成
            //  必須將其加入 ower form 的 Controls 內,
            //  ower form closed 的時候,
            //  才會自動調用 dispUI.Dispose() 
            //---------------------------------------------------------------------
            JzDisplay.UISpace.DispUI dispUI = new JzDisplay.UISpace.DispUI();
            this.Controls.Add(dispUI); // Gaara 原來的代碼, 少寫此行 !!!!!
            bool bOK = dispUI.DispUIload(this);
            _TRACE("[初始化] JzDisplay");
#endif

            //(2) 初始化 本專案
            bool bOK = Init();

            //(3) 初始化 異常
            if (!bOK)
            {
                BannerForm.CloseBanner();
                //>>> LogClass.Instance.Log("Mx加载错误");
                //MessageBox.Show("初始化错误", "Initial Lic", MessageBoxButtons.OK, MessageBoxIcon.Error);
                VsMessageBox.Warning($"Error @ {GetType().Name}.Init() !");
                Application.Exit();
                return;
            }

            #region NOT_USED_CODE
            //>>> 沒有用到 Universal.MainFormLocation
            //>>> Universal.MainFormLocation = new Point(this.Location.X, this.Location.Y);
            #endregion

            //(4) 設定視窗標題
            this.Text = $"{GlobalConfig.TITLE} (Ver {Application.ProductVersion}) " + Universal.VersionDate;

            //(5) 輸出 LOG
            show_simulation_info_to_log();

            //(6) 語系
            _post_translate();
        }
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            switch (VERSION)
            {
                case VersionEnum.PROJECT:
                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSING:
                            //@LETIAN:
                            //  最後補漏:
                            //  有時候程式退出時
                            //      ESSStatusEnum.EXIT 不會被觸發!
                            //GdxCore.Dispose();
                            break;
                    }
                    break;
            }
            //mMainTick.Enabled = false;
            mMainTick?.Dispose();
            mMainTick = null;
            Universal.Dispose();
        }

        bool Init()
        {
            _TRACE($"[初始化] {GetType().Name}.Init");

            switch (VERSION)
            {
                case VersionEnum.PROJECT:

                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSING:
                            this.Text = "第三站点胶 Ver:" + Application.ProductVersion;
                            break;
                        case OptionEnum.DISPENSINGX1:
                            this.Text = "第一站点胶 Ver:" + Application.ProductVersion;
                            break;
                        case OptionEnum.DISPENSINGX2:
                            this.Text = "第二站点胶 Ver:" + Application.ProductVersion;
                            break;
                        case OptionEnum.DISPENSINGX4:
                            this.Text = "第四站点胶 Ver:" + Application.ProductVersion;
                            break;
                        default:
                            this.Text = "宇宙无敌XXX Ver:" + Application.ProductVersion;
                            break;
                    }

                    break;
            }

            _TRACE("[初始化] INI.Instance");
            CommonLogClass.Instance.LogPath = Universal.LOG_TXT_PATH;
            INI.Instance.Initial();

            _TRACE("[初始化] Universal.Initial");
            bool bOK = Universal.Initial(0);
            if (!bOK)
                return false;


#if (false)
            ESSUI = essUI1;
            RUNUI = runUI1;
            RCPUI = rcpUI1;
            SETUPUI = iniUI1;
            CTRLUI = ctrlUI1;
            MAINUI = mainControlUI1;
#endif

            RUNUI.Location = new Point(1212, 237);
            RCPUI.Location = RUNUI.Location;
            SETUPUI.Location = RUNUI.Location;
            //USERLOTUI.Location = CTRLUI.Location;
            //CTRLALLREGIONUI.Location = CTRLUI.Location;

            _TRACE("[初始化] InitialESSUI");
            InitialESSUI();
            _TRACE("[初始化] InitialRCPUI");
            InitialRCPUI();
            _TRACE("[初始化] InitialSETUPUI");
            InitialSETUPUI();
            _TRACE("[初始化] InitialCTRLUI");
            InitialCTRLUI();
            _TRACE("[初始化] InitialMAINUI");
            InitialMAINUI();
            //InitialRESULT();
            _TRACE("[初始化] InitialRUNUI");
            InitialRUNUI();

            _TRACE("[初始化] mMainTick");
            mMainTick = new Timer();
            mMainTick.Interval = 20;
            mMainTick.Tick += MMainTick_Tick;
            BeginInvoke((Action)mMainTick.Start);

            _TRACE("[初始化] CTRLUI.SetEnable");
            CTRLUI.SetEnable(false);
            _TRACE("[初始化] MAINUI.SetEnable");
            MAINUI.SetEnable(false);
            _TRACE("[初始化] RUNUI.SetEnable");
            RUNUI.SetEnable(false);

            if (Universal.IsAutoLogin)
            {
                ESSUI.AutoLogin();
            }

            switch (Universal.VERSION)
            {
                case VersionEnum.AOI:

                    switch (Universal.OPTION)
                    {
                        case OptionEnum.MAIN_X2:

                            //ESSUI.RunWatchTime = INI.Instance.AutoLogoutTime;

                            string _viewer_path = "AJZReportViewer.exe";
                            if (System.IO.File.Exists(_viewer_path))
                            {
                                IntPtr hwnd = FindWindow(null, "JetEazy Viewer");
                                if (hwnd == IntPtr.Zero)
                                {
                                    System.Diagnostics.Process.Start(_viewer_path);
                                }
                            }

                            break;
                    }

                    break;
            }

            if (X6_LASER_CLIENT != null)
            {
                _TRACE("[初始化] X6_LASER_CLIENT.TriggerAction");
                X6_LASER_CLIENT.TriggerAction += X6_LASER_CLIENT_TriggerAction;
            }

            if (X6_HANDLE_CLIENT != null)
            {
                _TRACE("[初始化] X6_HANDLE_CLIENT.TriggerAction");
                X6_HANDLE_CLIENT.TriggerAction += X6_HANDLE_CLIENT_TriggerAction;
            }

            return true;
        }
        void PostCloseBanner()
        {
            // To Maximize the window size.
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Maximized;
            QMSG.Translate(this);
            BeginInvoke((Action)UpdateCurrentLanguageName);

            // Close Banner
            BannerForm.CloseBanner();
            GaUtil.SetCursor(this, Cursors.Default);
        }
        void OpenLanguageSelector()
        {
            using (var dlg = new JetEazy.Lang.Gui.FormLanguageSelector(QMSG.Lang()))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    QMSG.Translate(this);
                    BeginInvoke((Action)UpdateCurrentLanguageName);
                }
            }
        }
        void UpdateCurrentLanguageName()
        {
            RUNUI.lblLanguage.Text = QxLang.Instance("gui").CurrentLanguageName;
        }

        #region MAIN_X1_LASER_TCP

        BaseProcess m_LineScanProcess
        {
            get { return LineScanProcess.Instance; }
        }
        BaseProcess m_SingleProcess
        {
            get { return LineScanSingleProcess.Instance; }
        }

        ClientSocket X6_LASER_CLIENT
        {
            get { return Traveller106.Universal.X6_LASER_CLIENT; }
        }
        ClientSocket X6_HANDLE_CLIENT
        {
            get { return Universal.X6_HANDLE_CLIENT; }
        }
        private void X6_LASER_CLIENT_TriggerAction(tcpItemData opstr)
        {
            if (m_tcpAction)
                return;

            tcpdata = opstr;
            m_tcpAction = true;
        }

        private void X6_HANDLE_CLIENT_TriggerAction(tcpItemData opstr)
        {
            if (m_tcpHandleAction)
                return;

            tcpHandledata = opstr;
            m_tcpHandleAction = true;
        }

        #region MAIN_X6

        bool m_tcpAction = false;
        //string _recipename = string.Empty;
        //tcpCmd _cmd = opstr.Cmd;
        tcpItemData tcpdata = null;
        string m_tcp_dataCheck = string.Empty;

        /// <summary>
        /// 多线程的测试
        /// </summary>
        private void tcp_TriggerAct()
        {
#if(OPT_X2)
            switch (VERSION)
            {
                case VersionEnum.AOI:

                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X2:

                            if (m_tcpAction)
                            {
                                m_tcpAction = false;
                                switch (tcpdata.Cmd)
                                {
                                    case tcpCmd.CMD_CHANGE:

                                        ESSStatusEnum _currentStatu = ESSUI.GetMainStatus();
                                        if (_currentStatu == ESSStatusEnum.RUN)
                                        {
                                            if (!m_LineScanProcess.IsOn && !m_SingleProcess.IsOn)
                                            {
                                                bool bOK = RCPDB.IsDifferentName(tcpdata.RecipeName) || RCPDB.RCPItemNow.Name == tcpdata.RecipeName;//切换参数
                                                if (bOK)
                                                {
                                                    //MACHINE.PLCIO.LineScanRecipe = RCPDB.RCPItemNow.Name;
                                                    int i = 0;
                                                    int selectedindex = -1;
                                                    foreach (string str in RCPDB.GetRecipeStringList2())
                                                    {
                                                        string[] strs = str.Split('?');
                                                        if (strs[0] == tcpdata.RecipeName)
                                                            selectedindex = i;
                                                        i++;
                                                    }

                                                    X6_LASER_CLIENT.Log.Log2("tcpdata.RecipeName:" + tcpdata.RecipeName);
                                                    X6_LASER_CLIENT.Log.Log2("tcpdata.LotName:" + tcpdata.LotName);
                                                    //INI.Instance.CurrentLotName = tcpdata.LotName;
                                                    //INI.Instance.SaveLotName();
                                                    xRecipe.xLotNoStr = tcpdata.LotName;
                                                    xRecipe.SaveLotNo();

                                                    //this.Invoke(new Action(() => RUNUI.SetLotID(xRecipe.xLotNoStr)));

                                                    if (selectedindex >= 0)
                                                    {
                                                        X6_LASER_CLIENT.Log.Log2("tcpCmd.CMD_CHANGE Start");
                                                        ESSUI.ChangeRecipe(selectedindex);
                                                        X6_LASER_CLIENT.Log.Log2("tcpCmd.CMD_CHANGE End");
                                                        //MACHINE.PLCIO.LineScanRecipe = tcpdata.RecipeName;
                                                        X6_LASER_CLIENT.Log.Log2($"tcpCmd.CMD_CHANGE SendToPlc:{tcpdata.RecipeName}");
                                                    }
                                                    else
                                                    {
                                                        X6_LASER_CLIENT.Log.Log2("tcpCmd.CMD_CHANGE NoRecipe");
                                                        X6_LASER_CLIENT.Send(tcpdata.CmdStr + "0003");//0003 切换失败
                                                    }

                                                }

                                                X6_LASER_CLIENT.Log.Log2("tcpCmd.CMD_CHANGE" + (bOK ? "成功" : "失败"));
                                                X6_LASER_CLIENT.Send(tcpdata.CmdStr + (bOK ? "0001" : "0003"));// 0001 切换成功 0003 切换失败
                                            }
                                            else
                                            {
                                                X6_LASER_CLIENT.Send(tcpdata.CmdStr + "0004");//测试中
                                            }
                                        }
                                        else
                                        {
                                            X6_LASER_CLIENT.Send(tcpdata.CmdStr + "0005");//不在跑线状态
                                        }

                                        break;
                                    case tcpCmd.CMD_QCBYPASS:

                                        _currentStatu = ESSUI.GetMainStatus();
                                        if (_currentStatu == ESSStatusEnum.RUN)
                                        {
                                            if (!m_LineScanProcess.IsOn && !m_SingleProcess.IsOn)
                                            {
                                                if (RUNUI != null)
                                                {
                                                    int iret = RecipeMainX2Class.Instance.SetByPass(tcpdata.QcByPass);
                                                    if (iret == 0)
                                                    {
                                                        X6_LASER_CLIENT.Log.Log2("tcpdata.QcByPass OK");// + tcpHandledata.Qc2ddata);
                                                    }
                                                    X6_LASER_CLIENT.Send(tcpdata.CmdStr + (iret == 0 ? "0001" : "0003"));// 0001 切换成功 0003 切换失败
                                                }
                                                else
                                                {
                                                    X6_LASER_CLIENT.Send(tcpdata.CmdStr + "0003");// 切换失败
                                                }
                                            }
                                            else
                                            {
                                                X6_LASER_CLIENT.Send(tcpdata.CmdStr + "0004");//测试中
                                            }
                                        }
                                        else
                                        {
                                            X6_LASER_CLIENT.Send(tcpdata.CmdStr + "0005");//不在跑线状态
                                        }

                                        break;
                                    case tcpCmd.CMD_QC2DBARCODE:
                                        _currentStatu = ESSUI.GetMainStatus();
                                        if (_currentStatu == ESSStatusEnum.RUN)
                                        {
                                            if (!m_LineScanProcess.IsOn && !m_SingleProcess.IsOn)
                                            {
                                                if (RUNUI != null)
                                                {
                                                    //int iret = 0;
                                                    ////switch (Universal.jetMappingType)
                                                    ////{
                                                    ////    case JetMappingType.MAPPING_A:
                                                    ////        iret = AlbumNow.ENVList[0].MappingA_GridSetMapping2d(tcpdata.QC2dbarcode, ref m_tcp_dataCheck);
                                                    ////        break;
                                                    ////    default:
                                                    //iret = RUNUI.SetCheckBarcode(tcpdata.QC2dbarcode, ref m_tcp_dataCheck);
                                                    ////        break;
                                                    ////}

                                                    int iret = RecipeMainX2Class.Instance.SetBarcode(tcpdata.QC2dbarcode);
                                                    if (iret == 0)
                                                    {
                                                        X6_LASER_CLIENT.Log.Log2("tcpdata.QC2dbarcode" + tcpdata.Qc2ddata);

                                                    }
                                                    X6_LASER_CLIENT.Log.Log2("tcpCmd.CMD_QC2DBARCODE" + " return=" + iret.ToString() + " " + m_tcp_dataCheck);
                                                   
                                                    X6_LASER_CLIENT.Send(tcpdata.CmdStr + (iret == 0 ? "0001" : "0003"));// 0001 切换成功 0003 切换失败
                                                }
                                                else
                                                {
                                                    X6_LASER_CLIENT.Send(tcpdata.CmdStr + "0003");// 切换失败
                                                }
                                            }
                                            else
                                            {
                                                X6_LASER_CLIENT.Send(tcpdata.CmdStr + "0004");//测试中
                                            }
                                        }
                                        else
                                        {
                                            X6_LASER_CLIENT.Send(tcpdata.CmdStr + "0005");//不在跑线状态
                                        }
                                        break;
                                    default:

                                        X6_LASER_CLIENT.Log.Log2("tcpCmd.NONE" + " 无效指令。");
                                        X6_LASER_CLIENT.Send(tcpdata.CmdStr + "0002");//0002 无法识别的指令

                                        break;
                                }
                            }

                            break;
                    }

                    break;
            }
#endif
        }

        bool m_tcpHandleAction = false;

        //string _recipename = string.Empty;
        //tcpCmd _cmd = opstr.Cmd;
        tcpItemData tcpHandledata = null;


        /// <summary>
        /// 多线程的测试
        /// </summary>
        private void tcp_HandleTriggerAct()
        {
#if(OPT_X2)
            switch (VERSION)
            {
                case VersionEnum.AOI:

                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X2:

                            if (m_tcpHandleAction)
                            {
                                m_tcpHandleAction = false;
                                switch (tcpHandledata.Cmd)
                                {
                                    case tcpCmd.CMD_QC2DBARCODE:
                                        ESSStatusEnum _currentStatu = ESSUI.GetMainStatus();
                                        if (_currentStatu == ESSStatusEnum.RUN)
                                        {
                                            if (!m_LineScanProcess.IsOn && !m_SingleProcess.IsOn)
                                            {
                                                if (RUNUI != null)
                                                {
                                                    //int iret = 0;
                                                    //switch (Universal.jetMappingType)
                                                    //{
                                                    //    case JetMappingType.MAPPING_A:
                                                    //        iret = AlbumNow.ENVList[0].MappingA_GridSetMapping2d(tcpHandledata.QC2dbarcode, ref m_tcp_dataCheck);
                                                    //        break;
                                                    //    default:
                                                    //        iret = RUNUI.SetCheckBarcode(tcpHandledata.QC2dbarcode, ref m_tcp_dataCheck);
                                                    //        break;
                                                    //}
                                                    int iret = RecipeMainX2Class.Instance.SetBarcode(tcpHandledata.QC2dbarcode);
                                                    if (iret == 0)
                                                    {
                                                        X6_HANDLE_CLIENT.Log.Log2("tcpHandledata.QC2dbarcode" + tcpHandledata.Qc2ddata);

                                                    }
                                                    X6_HANDLE_CLIENT.Log.Log2("tcpCmd.CMD_QC2DBARCODE" + " return=" + iret.ToString() + " " + m_tcp_dataCheck);
                                                    byte[] bytedata = new byte[36];
                                                    bytedata[0] = 27;
                                                    bytedata[4] = 4;
                                                    bytedata[8] = 0;
                                                    bytedata[32] = (iret == 0 ? (byte)1 : (byte)3);
                                                    X6_HANDLE_CLIENT.Send(bytedata);
                                                    //X6_HANDLE_CLIENT.Send(tcpHandledata.CmdStr + (iret == 0 ? "0001" : "0003"));// 0001 切换成功 0003 切换失败
                                                }
                                                else
                                                {
                                                    byte[] bytedata = new byte[36];
                                                    bytedata[0] = 27;
                                                    bytedata[4] = 4;
                                                    bytedata[8] = 0;
                                                    bytedata[32] = (byte)3;
                                                    X6_HANDLE_CLIENT.Send(bytedata);
                                                    //X6_HANDLE_CLIENT.Send(tcpHandledata.CmdStr + "0003");// 切换失败
                                                }
                                            }
                                            else
                                            {
                                                byte[] bytedata = new byte[36];
                                                bytedata[0] = 27;
                                                bytedata[4] = 4;
                                                bytedata[8] = 0;
                                                bytedata[32] = (byte)4;
                                                X6_HANDLE_CLIENT.Send(bytedata);
                                                //X6_HANDLE_CLIENT.Send(tcpHandledata.CmdStr + "0004");//测试中
                                            }
                                        }
                                        else
                                        {
                                            byte[] bytedata = new byte[36];
                                            bytedata[0] = 27;
                                            bytedata[4] = 4;
                                            bytedata[8] = 0;
                                            bytedata[32] = (byte)5;
                                            X6_HANDLE_CLIENT.Send(bytedata);
                                            //X6_HANDLE_CLIENT.Send(tcpHandledata.CmdStr + "0005");//不在跑线状态
                                        }
                                        break;
                                    case tcpCmd.CMD_QC2DDATA:
                                        _currentStatu = ESSUI.GetMainStatus();
                                        if (_currentStatu == ESSStatusEnum.RUN)
                                        {
                                            if (!m_LineScanProcess.IsOn && !m_SingleProcess.IsOn)
                                            {
                                                if (RUNUI != null)
                                                {
                                                    //int iret = 0;
                                                    //switch (Universal.jetMappingType)
                                                    //{
                                                    //    case JetMappingType.MAPPING_A:
                                                    //        iret = AlbumNow.ENVList[0].MappingA_GridSetMappingBypass(tcpHandledata.QcByPass, ref m_tcp_dataCheck);
                                                    //        break;
                                                    //    default:
                                                    //        iret = RUNUI.SetByPass(tcpHandledata.QcByPass, ref m_tcp_dataCheck);
                                                    //        break;
                                                    //}

                                                    int iret = RecipeMainX2Class.Instance.SetByPass(tcpHandledata.QcByPass);
                                                    if (iret == 0)
                                                    {
                                                        X6_HANDLE_CLIENT.Log.Log2("tcpHandledata.Qc2ddata" + tcpHandledata.Qc2ddata);

                                                    }
                                                    X6_HANDLE_CLIENT.Log.Log2("tcpCmd.CMD_QC2DDATA" + " return=" + iret.ToString() + " " + m_tcp_dataCheck);
                                                    byte[] bytedata = new byte[36];
                                                    bytedata[0] = 24;
                                                    bytedata[4] = 4;
                                                    bytedata[8] = 0;
                                                    bytedata[32] = (iret == 0 ? (byte)1 : (byte)3);
                                                    X6_HANDLE_CLIENT.Send(bytedata);
                                                    //X6_HANDLE_CLIENT.Send(tcpHandledata.CmdStr + (iret == 0 ? "0001" : "0003"));// 0001 切换成功 0003 切换失败
                                                }
                                                else
                                                {
                                                    byte[] bytedata = new byte[36];
                                                    bytedata[0] = 24;
                                                    bytedata[4] = 4;
                                                    bytedata[8] = 0;
                                                    bytedata[32] = (byte)3;
                                                    X6_HANDLE_CLIENT.Send(bytedata);
                                                    //X6_HANDLE_CLIENT.Send(tcpHandledata.CmdStr + "0003");// 切换失败
                                                }
                                            }
                                            else
                                            {
                                                byte[] bytedata = new byte[36];
                                                bytedata[0] = 24;
                                                bytedata[4] = 4;
                                                bytedata[8] = 0;
                                                bytedata[32] = (byte)4;
                                                X6_HANDLE_CLIENT.Send(bytedata);
                                                //X6_HANDLE_CLIENT.Send(tcpHandledata.CmdStr + "0004");//测试中
                                            }
                                        }
                                        else
                                        {
                                            byte[] bytedata = new byte[36];
                                            bytedata[0] = 24;
                                            bytedata[4] = 4;
                                            bytedata[8] = 0;
                                            bytedata[32] = (byte)5;
                                            X6_HANDLE_CLIENT.Send(bytedata);
                                            //X6_HANDLE_CLIENT.Send(tcpHandledata.CmdStr + "0005");//不在跑线状态
                                        }
                                        break;
                                    case tcpCmd.CMD_QCCHANGE_MODEL:
                                        _currentStatu = ESSUI.GetMainStatus();
                                        if (_currentStatu == ESSStatusEnum.RUN)
                                        {
                                            if (!m_LineScanProcess.IsOn && !m_SingleProcess.IsOn)
                                            {
                                                int iret = ProcessRunClass.Instance.ChangeModelBackgroudImage();
                                                if (iret == 0)
                                                {
                                                    this.Invoke(new Action(() =>
                                                    {
                                                        MAINUI.ChangeRecipe();
                                                    }));
                                                }
                                                X6_HANDLE_CLIENT.Log.Log2("tcpCmd.CMD_QCCHANGE_MODEL" + " return=" + iret.ToString());
                                                byte[] bytedata = new byte[36];
                                                bytedata[0] = 25;
                                                bytedata[4] = 4;
                                                bytedata[8] = 0;
                                                bytedata[32] = (iret == 0 ? (byte)1 : (byte)3);
                                                X6_HANDLE_CLIENT.Send(bytedata);
                                            }
                                            else
                                            {
                                                byte[] bytedata = new byte[36];
                                                bytedata[0] = 25;
                                                bytedata[4] = 4;
                                                bytedata[8] = 0;
                                                bytedata[32] = (byte)4;
                                                X6_HANDLE_CLIENT.Send(bytedata);
                                                X6_HANDLE_CLIENT.Log.Log2($"tcpCmd.CMD_QCCHANGE_MODEL 4-测试中");
                                                //X6_HANDLE_CLIENT.Send(tcpHandledata.CmdStr + "0004");//测试中
                                            }
                                        }
                                        else
                                        {
                                            byte[] bytedata = new byte[36];
                                            bytedata[0] = 25;
                                            bytedata[4] = 4;
                                            bytedata[8] = 0;
                                            bytedata[32] = (byte)5;
                                            X6_HANDLE_CLIENT.Send(bytedata);
                                            X6_HANDLE_CLIENT.Log.Log2($"tcpCmd.CMD_QCCHANGE_MODEL 5-不在跑线状态");
                                            //X6_HANDLE_CLIENT.Send(tcpHandledata.CmdStr + "0005");//不在跑线状态
                                        }
                                        break;
                                    default:
                                        byte[] bytedata1 = new byte[36];
                                        bytedata1[0] = byte.Parse(tcpdata.CmdStr);
                                        bytedata1[4] = 4;
                                        bytedata1[8] = 0;
                                        bytedata1[32] = (byte)2;
                                        X6_HANDLE_CLIENT.Send(bytedata1);
                                        X6_HANDLE_CLIENT.Log.Log2("tcpCmd.NONE" + " 无效指令。");
                                        //X6_HANDLE_CLIENT.Send(tcpdata.CmdStr + "0002");//0002 无法识别的指令
                                        break;
                                }
                            }

                            break;
                    }

                    break;
            }
#endif
        }

        #endregion

        #endregion

        void InitialESSUI()
        {
            ESSUI.Initial(ESSDB, ACCDB, Universal.UIPATH, INI.Instance.LANGUAGE, Universal.VERSION, Universal.OPTION, 200);
            //ESSUI.Set111(Universal.WORKPATH + "\\111.BMP");
            ESSUI.SetRecipeCombo(RCPDB.GetRecipeStringList());
            ESSUI.TriggerAction += new EssUI.TriggerHandler(ESSUI_TriggerAction);

            ESSUI.SetMainStatus(ESSStatusEnum.RUN);
        }
        void InitialRUNUI()
        {
            RUNUI.Initial(Universal.UIPATH, INI.Instance.LANGUAGE, Universal.VERSION, Universal.OPTION);
            RUNUI.TriggerAction += new RunUI.TriggerHandler(RUNUI_TriggerAction);
            RUNUI.btnLanguage.Click += (s, e) => OpenLanguageSelector();
        }
        void InitialRCPUI()
        {
            RCPUI.Initial(Universal.UIPATH, INI.Instance.LANGUAGE, Universal.VERSION, Universal.OPTION, RCPDB);
            RCPUI.TriggerAction += new RcpUI.TriggerHandler(RCPUI_TriggerAction);
            RCPUI.TriggerActionForSetupDetail += new RcpUI.TriggerHandlerForSetupDetail(RCPUI_TriggerActionForSetupDetail);
        }
        void InitialSETUPUI()
        {
            SETUPUI.Initial(Universal.UIPATH, INI.Instance.LANGUAGE, Universal.VERSION, Universal.OPTION);
            SETUPUI.TriggerAction += new IniUI.TriggerHandler(SETUPUI_TriggerAction);
            SETUPUI.TriggerStringAction += SETUPUI_TriggerStringAction;
        }
        void SETUPUI_TriggerStringAction(string statusstr)
        {
            //MoveString = statusstr;
        }

        void InitialCTRLUI()
        {
            CTRLUI.BackColor = SystemColors.Control;
            CTRLUI.Initial(VERSION, OPTION, MACHINECollection.MACHINE);
        }
        void InitialMAINUI()
        {
            MAINUI.BackColor = SystemColors.Control;
            MAINUI.Initial(VERSION, OPTION, MACHINECollection.MACHINE);
            MAINUI.OnStateChanged += MAINUI_OnChangeState;
        }

        #region EVENT_HANDLERS
        void MAINUI_OnChangeState(object sender, MainUiStateEventArgs e)
        {
            if (e == null)
                return;

            var status = e.Status;
            var tag = e.Tag;

            switch (status)
            {
                case MainS1State.S1_READY:
                    ESSUI.Enabled = true;
                    RUNUI.Enabled = true;
                    CTRLUI.Enabled = true;
                    mainControlUI1.SetEnableState(true);
                    break;

                case MainS1State.S1_RUNNING:
                case MainS1State.S1_RESETING:
                    ESSUI.Enabled = false;
                    RUNUI.Enabled = false;
                    CTRLUI.Enabled = false;
                    mainControlUI1.SetEnableState(false);
                    break;

                case MainS1State.LS_START:
                    RUNUI.StartTime();
                    break;

                case MainS1State.LS_STOP:
                    RUNUI.StopTime();
                    break;

                case MainS1State.M_PASS:
                    RUNUI.StartShinnig(true);
                    break;

                case MainS1State.M_NG:
                    RUNUI.StartShinnig(false);
                    break;

                case MainS1State.M_SHOWCODE:
                    if (tag != null)
                        RUNUI.SetProductBarcode((string)tag);
                    break;

                case MainS1State.M_SHOWRESULT:
                    if (tag != null)
                        RUNUI.SetDuriation((string)tag);
                    break;
            }
        }
        void ESSUI_TriggerAction(ESSStatusEnum status)
        {
            switch (status)
            {
                case ESSStatusEnum.EXIT:
                    if (CTRLUI != null)
                    {
                        CTRLUI.MyDispose();
                    }

                    if (X6_HANDLE_CLIENT != null)
                        X6_HANDLE_CLIENT.DisConnectServer();

                    if (X6_LASER_CLIENT != null)
                        X6_LASER_CLIENT.DisConnectServer();

                    MAINUI.Close();
                    //LETIAN: 原代碼有誤: 此處不會被調用到 !!!
                    //Universal.Close();
                    this.Close();
                    break;

                case ESSStatusEnum.RUN:
                case ESSStatusEnum.RECIPE:
                case ESSStatusEnum.SETUP:
                    RUNUI.Visible = status == ESSStatusEnum.RUN;
                    RCPUI.Visible = status == ESSStatusEnum.RECIPE;
                    SETUPUI.Visible = status == ESSStatusEnum.SETUP;
                    //USERLOTUI.Visible = status == ESSStatusEnum.RUN;
                    //CTRLUI.Visible = (status == ESSStatusEnum.SETUP);
                    //CTRLALLREGIONUI.Visible = (status == ESSStatusEnum.RECIPE && Universal.OPT == OptionEnum.AUTO && INI.MistDebugging);
                    break;

                case ESSStatusEnum.LOGIN:
                    //picResult.Visible = false;
                    //btnOK.Visible = false;
                    if (ACCDB.AccNow.IsAllowSetupINI)
                    {
                        //MAINUI.Enabled = true;
                        CTRLUI.SetEnable(true);
                        MAINUI.SetEnable(true);
                        RUNUI.SetEnable(true);
                    }
                    break;

                case ESSStatusEnum.LOGOUT:
                    //picResult.Visible = false;
                    //btnOK.Visible = false;
                    //MAINUI.Enabled = false;
                    CTRLUI.SetEnable(false);
                    MAINUI.SetEnable(false);
                    RUNUI.SetEnable(false);
                    break;

                case ESSStatusEnum.RESET:
                    //if (MessageBox.Show("是否要將所有馬達歸位?", "SYS", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    //    RESULT.StartResetMotorProcess();

                    //if (USEIO.IsProjectorOnsite)
                    //{
                    //    MessageBox.Show("請移開光機. ");
                    //    return;
                    //}

                    //if (RESULT.IsResetProcessOn)
                    //{
                    //    if (MessageBox.Show("是否要停止重置流程?", "SYS", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                    //        return;
                    //}
                    //else
                    //    if (MessageBox.Show("請將所有光機清空才可繼續，是否要開始重置流程?", "SYS", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                    //        return;

                    //RESULT.StartResetProcess();
                    break;

                case ESSStatusEnum.RECIPESELECTED:
                    using (var vsMessageBox = VsMessageBox.InfoForm(QMSG.Text(Prompts.Info_Recipe_Switching)))
                    {
                        vsMessageBox.Show();
                        vsMessageBox.Refresh();

                        //RCPDB.GetRCPItem(ESSDB.LastRecipeIndex);
                        RCPDB.Indicator = ESSDB.LastRecipeIndex;

                        //RecipeCHClass.Instance.ChangeIndex(ESSDB.LastRecipeIndex);
                        //RecipeTrayClass.Instance.ChangeIndex(ESSDB.LastRecipeIndex);
                        //VisionTrayClass.Instance.ChangeIndex(ESSDB.LastRecipeIndex);
                        //RecipeNeedleClass.Instance.ChangeIndex(ESSDB.LastRecipeIndex);
                        xRecipe.ChangeIndex(ESSDB.LastRecipeIndex);
                        xRecipe.Load();
                        //ViewPreload();
                        RCPUI.ChangeRecipe(true);
                        //ESSDB.RecipeChange(RCPItemNow.Index);
                        //RESULT.InitialBMPResult();
                        //DisplayStatus = DisplayStatusEnum.LIVE;
                        RUNUI.SetDuriation("");
                        MAINUI.ChangeRecipe();

                        //int ialone = (int)RecipeCHClass.Instance.PMode;// myUserSelectFormX1.Mode;

                        //if (ialone == 1)
                        //{
                        //    VsMSG.Instance.Tishi("切換到 重工模式 完成");
                        //}

                        vsMessageBox.Close();
                    }
                    break;

                case ESSStatusEnum.FASTCAL:
                    //if (!PhotoMainProcess.IsOn)
                    //    PhotoMainProcess.Start();

                    //TestMethod = TestMethodEnum.BUTTON;

                    //RUNUI.SetBarcodeEnable = false;

                    //RESULT.StartCalProcess(VIEW,
                    //    TestMethod,
                    //    RUNUI.IsSaveRaw,
                    //    RUNUI.IsSaveNGRaw,
                    //    RUNUI.IsSaveDebug,
                    //    RUNUI.GetOPBarcode(),
                    //    Universal.IsDebug,
                    //    RUNUI.GetProductBarcode());
                    break;
            }
        }
        void SETUPUI_TriggerAction(INIStatusEnum status)
        {
            switch (status)
            {
                case INIStatusEnum.CHANGELANGUAGE:

                    INI.Instance.LoadIniSetup();
                    //RecipeMiniX6Class.Instance.LoadIniSetup();
                    //RecipeMiniX6Class.Instance.LoadFourSetup();
                    //INI.Instance.LoadLanguage();
                    LanguageExClass.Instance.EnumControls(this);

                    //ESSUI.SetLanguage(INI.Instance.LANGUAGE);
                    //SETUPUI.SetLanguage(INI.Instance.LANGUAGE);

                    break;
                case INIStatusEnum.EDIT:
                    ESSUI.Disable = true;

                    //MoveString = "";
                    //m_DispUI.SetDisplayType(DisplayTypeEnum.ADJUST);

                    break;
                case INIStatusEnum.EXIT:
                    ESSUI.Disable = false;

                    //m_DispUI.SetDisplayType(DisplayTypeEnum.SHOW);
                    switch (Universal.VERSION)
                    {
                        case VersionEnum.LASER:

                            switch (Universal.OPTION)
                            {
                                case OptionEnum.MAIN_X1:

                                    ESSUI.RunWatchTime = INI.Instance.AutoLogoutTime;

                                    break;
                            }

                            break;
                    }

                    break;

            }
        }
        void RCPUI_TriggerAction(RCPStatusEnum status)
        {
            switch (status)
            {
                case RCPStatusEnum.EDIT:
                    ESSUI.Disable = true;
                    break;
                case RCPStatusEnum.MODIFYCOMPLETE:
                    ESSUI.Disable = false;
                    ESSDB.RecipeChange(RCPItemNow.Index);
                    //RESULT.InitialBMPResult();
                    ESSUI.SetRecipeCombo(RCPDB.GetRecipeStringList());

                    ESSUI.FillDisplay();

                    //For Test Only
                    //RUNUI.InitialRun(VIEW);

                    //LanguageExClass.Instance.EnumControls(this);
                    MAINUI.ChangeRecipe();
                    break;
                case RCPStatusEnum.MODIFYCANCEL:
                    ESSUI.Disable = false;

                    //LanguageExClass.Instance.EnumControls(this);
                    MAINUI.ChangeRecipe();
                    break;
                case RCPStatusEnum.DELETE:

                    RCPDB.Save();

                    break;
            }
        }
        void RCPUI_TriggerActionForSetupDetail(RCPStatusEnum status, int setupindex)
        {
            switch (status)
            {
                case RCPStatusEnum.SHOWDETAIL:
                    break;
            }
        }
        void RUNUI_TriggerAction(RunStatusEnum Status)
        {
            switch (Status)
            {
                case RunStatusEnum.STARTRUN:
                    break;
                case RunStatusEnum.SHINNIGEND:
                    break;
                case RunStatusEnum.CHANGERECIPE:

                    #region 切换参数

                    ESSStatusEnum _currentStatu = ESSUI.GetMainStatus();
                    if (_currentStatu == ESSStatusEnum.RUN)
                    {
                        if (!m_LineScanProcess.IsOn && !m_SingleProcess.IsOn)
                        {
                            string plcRecipeName = MACHINE.PLCIO.sRecipeName;
                            bool bOK = RCPDB.IsDifferentName(plcRecipeName) || RCPDB.RCPItemNow.Name == plcRecipeName;//切换参数
                            _LOG($"plcRecipeName:{plcRecipeName}");
                            if (bOK)
                            {
                                //MACHINE.PLCIO.LineScanRecipe = RCPDB.RCPItemNow.Name;
                                int i = 0;
                                int selectedindex = -1;
                                foreach (string str in RCPDB.GetRecipeStringList2())
                                {
                                    string[] strs = str.Split('?');
                                    if (strs[0] == plcRecipeName)
                                        selectedindex = i;
                                    i++;
                                }

                                if (selectedindex >= 0)
                                {
                                    _LOG($"CMD_CHANGE Start");
                                    ESSUI.ChangeRecipe(selectedindex);
                                    _LOG($"CMD_CHANGE End");
                                }
                                else
                                {
                                    _LOG($"CMD_CHANGE NoRecipe");
                                }
                            }
                            MACHINE.PLCIO.iRecipeNum = (bOK ? 1 : 2);
                            _LOG($"CMD_CHANGE {(bOK ? "成功" : "失败")}");
                        }
                        else
                        {
                            MACHINE.PLCIO.iRecipeNum = 2;
                            _LOG($"CMD_CHANGE 测试中");
                        }
                    }
                    else
                    {
                        MACHINE.PLCIO.iRecipeNum = 2;
                        _LOG($"CMD_CHANGE 不在跑线状态");
                    }

                    #endregion

                    break;
            }
        }
        #endregion

        #region SCAN_TIME_AND_TIME_TICK
        //主程序扫描时间
        JzTimes JzMainScanTime = new JzTimes();
        int JzScanTimeMS = 0;
        private void MMainTick_Tick(object sender, EventArgs e)
        {
            if (BannerForm.IsShowing())
                PostCloseBanner();

            JzScanTimeMS = JzMainScanTime.msDuriation;
            JzMainScanTime.Cut();

            //MACHINECollection.Tick();

            tcp_TriggerAct();
            tcp_HandleTriggerAct();

            ESSUI.Tick();
            CTRLUI.Tick();
            RUNUI.Tick();
            MAINUI.Tick();

            if (!Traveller106.Universal.IsNoUseIO)
            {
                //// 每隔 20 ms 不斷的 向 PLC 通訊 !!!
                //// 重複詢問 LotID 與 StripID
                //// 不優 !!!
                
                //>>> 於 2025-11-06 廢除 !!!
                //RUNUI.SetLotID(MACHINE.PLCIO.sLotID);
                //RUNUI.SetStripID(MACHINE.PLCIO.sStripID);
            }

#if (OPT_STATION_S2)
             ESSUI.ShowPLC_RxTime(Universal.VersionDate + "_" +
                                 Universal.OPTION.ToString() + "B " +
                                 JzScanTimeMS.ToString() + "ms " +
                                 MACHINECollection.PLCFps());
#else
            ESSUI.ShowPLC_RxTime(Universal.VersionDate + "_" +
                                 Universal.OPTION.ToString() + " " +
                                 JzScanTimeMS.ToString() + "ms " +
                                 MACHINECollection.PLCFps());
#endif

        }
        #endregion

        #region PRIVATE_FUNCTIONS
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
#if(false)
        bool _getMxComponent()
        {

            bool ret = false;

            if (Universal.IsNoUseIO)
                return true;

            string _path = Universal.WORKPATH + "\\mx\\MxComponent.exe";
            if (System.IO.File.Exists(_path))
            {
                string _pathname1 = Universal.WORKPATH + "\\mx\\OK.TXT";
                string _pathname2 = Universal.WORKPATH + "\\mx\\NG.TXT";

                if (System.IO.File.Exists(_pathname1))
                    System.IO.File.Delete(_pathname1);

                if (System.IO.File.Exists(_pathname2))
                    System.IO.File.Delete(_pathname2);

                IntPtr hwnd = FindWindow(null, "MxComponent");
                if (hwnd == IntPtr.Zero)
                {
                    System.Diagnostics.Process.Start(_path);
                }

                System.Threading.Thread.Sleep(2500);
                while (true)
                {
                    hwnd = FindWindow(null, "MxComponent");
                    if (hwnd == IntPtr.Zero)
                    {
                        break;
                    }
                }


                if (System.IO.File.Exists(_pathname1))
                {
                    ret = true;
                    System.IO.File.Delete(_pathname1);
                }
                else
                {
                    ret = false;
                }

                if (System.IO.File.Exists(_pathname2))
                    System.IO.File.Delete(_pathname2);

            }

            return ret;
        }
#endif
        private void show_simulation_info_to_log()
        {
            if (Universal.IsNoUseIO)
                CommonLogClass.Instance.LogMessage("模擬 PLC", Color.OrangeRed);
            if (Universal.IsNoUseMotor)
                CommonLogClass.Instance.LogMessage("模擬 Motor", Color.OrangeRed);
            //for (int i = 0, N = Universal.CAMERAS.Length; i < N; i++)
            //{
            //    if (Universal.CAMERAS[i].IsSim())
            //        CommonLogClass.Instance.LogMessage("模擬 Cam" + i, Color.OrangeRed);
            //}
        }
        private void auto_layout()
        {
            if (WindowState == FormWindowState.Minimized)
                return;

            SuspendLayout();

            //@LETIAN: 自動調整 layout
            int panelWidth = 235;
            var rcc = ClientRectangle;
            mainControlUI1.Width = rcc.Width - panelWidth - 2;
            mainControlUI1.Height = rcc.Height;
            var panels = new Control[]
            {
                essUI1,
                runUI1,
                rcpUI1,
                iniUI1,
                ctrlUI1,
            };
            foreach (var panel in panels)
            {
                panel.Width = panelWidth;
                panel.Left = rcc.Width - panel.Width;
                panel.Padding = new Padding(5, 5, 5, 5);
            }
            runUI1.Top = essUI1.Bottom;
            rcpUI1.Top = runUI1.Top;
            iniUI1.Top = runUI1.Top;
            ctrlUI1.Top = runUI1.Bottom;
            ctrlUI1.Height = rcc.Bottom - ctrlUI1.Top;

            ResumeLayout(true);
        }
        private void _post_translate()
        {
            QMSG.Translate(this, 3000);
            QMSG.Lang("gui").LanguageChanged += (s, e) =>
            {
                QMSG.Translate(SETUPUI, 100);
            };

            //new Action(() => {
            //    System.Threading.Thread.Sleep(3000);
            //    BeginInvoke(new Action(() => QMSG.Translate(this)));
            //    BeginInvoke(new Action(() => QMSG.Translate(SETUPUI)));
            //}).BeginInvoke(null, null);
        }
        #endregion

        protected void _LOG(string msg, params object[] args)
        {
#if (true)
            Color color = Color.Black;

            int N = args.Length;
            if (N > 0 && args[N - 1] is Color)
            {
                color = (Color)args[N - 1];
                N -= 1;
            }

            var sb = new System.Text.StringBuilder();
            //sb.Append(Name);
            //sb.Append(", ");
            sb.Append(msg);

            for (int i = 0; i < N; i++)
            {
                sb.Append(", ");
                sb.Append(args[i]);
            }

            msg = sb.ToString();
            CommonLogClass.Instance.LogMessage(msg, color);
            //if (color == Color.Red)
            //    GdxGlobal.LOG.Warn(msg);
            //else
            //    GdxGlobal.LOG.Debug(msg);
#endif
            //msg = Name + ", " + msg;
            //GdxGlobal.LOG.Log(msg, args);
        }

        private void _TRACE(string msg)
        {
            _NLOG.Info(msg);
        }
    }
}
