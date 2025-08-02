using Eazy_Project_III.FormSpace;
using Eazy_Project_III.UISpace;
using Eazy_Project_III;
using JetEazy.DBSpace;
using JetEazy.UISpace;
using PhotoMachine.UISpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VsCommon.ControlSpace;
using JetEazy.BasicSpace;
using JetEazy;
using Common.RecipeSpace;
using System.Runtime.InteropServices;
//using Traveller106.OPSpace;
//using JetEazy;

namespace Traveller106
{
    public partial class MainForm : Form
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

        BannerForm BANNERFORM;

        EssUI ESSUI;
        RunUI RUNUI;
        RcpUI RCPUI;
        IniUI SETUPUI;
        CtrlUI CTRLUI;

        MainControlUI MAINUI;

        Timer mMainTick;
        //JzTimes mImageTime = new JzTimes();

        //string MoveString = "";
        //bool IsLiveCapturing = true;

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

        MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }

        public MainForm()
        {
            InitializeComponent();

            if (!_getMxComponent())
            {
                //LogClass.Instance.Log("Mx加载错误");

                MessageBox.Show("初始化错误", "Initial MxComponent", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //this.Close();
                Application.Exit();
                return;
            }

            this.Load += MainForm_Load;
            this.FormClosed += MainForm_FormClosed;
            this.SizeChanged += MainForm_SizeChanged;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            BANNERFORM = new BannerForm();
            BANNERFORM.Show();
            BANNERFORM.Refresh();

            Init();

            BANNERFORM.Close();
            BANNERFORM.Dispose();

            Universal.MainFormLocation = new Point(this.Location.X, this.Location.Y);

            this.Text = "镭雕定位打标系统 Ver" + Application.ProductVersion;

#if OPT_LETIAN_AUTO_LAYOUT
            // To fit into my screen for debug.
#if DEBUG
            this.FormBorderStyle = FormBorderStyle.Sizable;
#endif
            this.WindowState = FormWindowState.Maximized;
#endif

            _show_simulation_info_to_log();
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
        }
        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            //@LETIAN
            _auto_layout();
        }

        void Init()
        {
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

            CommonLogClass.Instance.LogPath = Universal.LOG_TXT_PATH;
            INI.Instance.Initial();
            bool bOK = Universal.Initial(0);
            if (!bOK)
            {
                Environment.Exit(0);
            }

            ESSUI = essUI1;
            RUNUI = runUI1;
            RCPUI = rcpUI1;
            SETUPUI = iniUI1;
            CTRLUI = ctrlUI1;
            MAINUI = mainControlUI1;

            RUNUI.Location = new Point(1212, 237);
            RCPUI.Location = RUNUI.Location;
            SETUPUI.Location = RUNUI.Location;
            //USERLOTUI.Location = CTRLUI.Location;
            //CTRLALLREGIONUI.Location = CTRLUI.Location;

            InitialESSUI();
            InitialRCPUI();
            InitialSETUPUI();
            InitialCTRLUI();
            InitialMAINUI();
            //InitialRESULT();
            InitialRUNUI();

            mMainTick = new Timer();
            mMainTick.Interval = 20;
            mMainTick.Enabled = true;
            mMainTick.Tick += MMainTick_Tick;

            if(Universal.IsAutoLogin)
            {
                ESSUI.AutoLogin();
            }

#if !OPT_LETIAN_AUTO_LAYOUT
            // 很慢
            LanguageExClass.Instance.EnumControls(this);
#endif
        }

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
        private void SETUPUI_TriggerStringAction(string statusstr)
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
            MAINUI.OnChangeState += MAINUI_OnChangeState;
        }

        private void MAINUI_OnChangeState(MainS1State status)
        {

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
            }
        }

        void ESSUI_TriggerAction(ESSStatusEnum status)
        {
            switch (status)
            {
                case ESSStatusEnum.EXIT:
                    
                    MAINUI.Close();
                    Universal.Close();
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
                    }

                    break;
                case ESSStatusEnum.LOGOUT:

                    //picResult.Visible = false;
                    //btnOK.Visible = false;

                    //MAINUI.Enabled = false;
                    CTRLUI.SetEnable(false);
                    MAINUI.SetEnable(false);

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

                    //RCPDB.GetRCPItem(ESSDB.LastRecipeIndex);
                    RCPDB.Indicator = ESSDB.LastRecipeIndex;

                    //RecipeCHClass.Instance.ChangeIndex(ESSDB.LastRecipeIndex);
                    //RecipeTrayClass.Instance.ChangeIndex(ESSDB.LastRecipeIndex);
                    //VisionTrayClass.Instance.ChangeIndex(ESSDB.LastRecipeIndex);
                    //RecipeNeedleClass.Instance.ChangeIndex(ESSDB.LastRecipeIndex);
                    RecipeMiniX6Class.Instance.ChangeIndex(ESSDB.LastRecipeIndex);

                    //ViewPreload();
                    RCPUI.ChangeRecipe(true);
                    //ESSDB.RecipeChange(RCPItemNow.Index);
                    //RESULT.InitialBMPResult();
                    //DisplayStatus = DisplayStatusEnum.LIVE;
                    MAINUI.ChangeRecipe();

                    //int ialone = (int)RecipeCHClass.Instance.PMode;// myUserSelectFormX1.Mode;

                    //if (ialone == 1)
                    //{
                    //    VsMSG.Instance.Tishi("切換到 重工模式 完成");
                    //}

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

                    ESSUI.SetLanguage(INI.Instance.LANGUAGE);
                    SETUPUI.SetLanguage(INI.Instance.LANGUAGE);

                    break;
                case INIStatusEnum.EDIT:
                    ESSUI.Disable = true;

                    //MoveString = "";
                    //m_DispUI.SetDisplayType(DisplayTypeEnum.ADJUST);

                    break;
                case INIStatusEnum.EXIT:
                    ESSUI.Disable = false;

                    //m_DispUI.SetDisplayType(DisplayTypeEnum.SHOW);

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

                    break;
                case RCPStatusEnum.MODIFYCANCEL:
                    ESSUI.Disable = false;
                    break;
            }
        }
        //DetailForm DETAILFRM;
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
            }
        }

        //主程序扫描时间
        JzTimes JzMainScanTime = new JzTimes();
        int JzScanTimeMS = 0;

        private void MMainTick_Tick(object sender, EventArgs e)
        {
            JzScanTimeMS = JzMainScanTime.msDuriation;
            JzMainScanTime.Cut();

            MACHINECollection.Tick();

            ESSUI.Tick();
            CTRLUI.Tick();
            RUNUI.Tick();
            MAINUI.Tick();



#if OPT_STATION_S2
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

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
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

        private void _show_simulation_info_to_log()
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
        private void _auto_layout()
        {
            if (WindowState == FormWindowState.Minimized)
                return;

#if OPT_LETIAN_AUTO_LAYOUT
            //@LETIAN: 自動調整 layout
            int panelWidth = 235;
            var rcc = ClientRectangle;
            mainControlUI1.Width = rcc.Width - panelWidth - 2;
            mainControlUI1.Height = rcc.Height;
            ctrlUI1.Height = rcc.Bottom - ctrlUI1.Top;
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
#endif
        }
    }
}
