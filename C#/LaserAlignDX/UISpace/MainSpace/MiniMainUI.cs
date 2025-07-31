using Common.RecipeSpace;
using Eazy_Project_III;
using FreeImageAPI;
using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.ControlSpace;
using JetEazy.DBSpace;
using JzDisplay;
using NeedleX.ProcessSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traveller106;
using Traveller106.FormSpace;
using TravellerMINIX6.OPSpace;
using TravellerMINIX6.ProcessSpace;
using VsCommon.ControlSpace;
using VsCommon.ControlSpace.IOSpace;
using VsCommon.ControlSpace.MachineSpace;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Universal = Traveller106.Universal;

namespace TravellerMINIX6.UISpace.MainSpace
{
    public partial class MiniMainUI : UserControl
    {
        //ICam CAM0
        //{
        //    get { return Universal.CAMERAS[0]; }
        //}

        const int TRACK_MAX_COUNT = 1;
        TrackItemUI[] m_TrackItemUI = new TrackItemUI[TRACK_MAX_COUNT];

        //protected RecipeMiniX6Class myRecipe
        //{
        //    get { return RecipeMiniX6Class.Instance; }
        //}

        //protected MachineCollectionClass MACHINECollection
        //{
        //    get
        //    {
        //        return Traveller106.Universal.MACHINECollection;
        //    }
        //}
        protected MiniX6MachineClass MACHINE
        {
            get { return (MiniX6MachineClass)Traveller106.Universal.MACHINECollection.MACHINE; }
        }
        protected TrackItemUI TrackItem
        {
            get { return trackItemUI1; }
        }
        protected AccDBClass ACCDB
        {
            get
            {
                return Traveller106.Universal.ACCDB;
            }
        }

        bool[] m_plcCommError;

        Button btnLinescanTest;
        Button btnStart;
        Button btnStop;
        Button btnReset;
        Button btnClearAlarm;
        Button btnMute;
        Button btnManualAuto;
        Button btnSingleTest;

        Button btnFeedCy;
        Button btnTakeCy;
        Button btnCylevel;
        Button btnCyVerical;
        Button btnPlcTestRun;

        Label lblAlarm;
        Label lblState;
        Label lblPlcTestRun;

        public MiniMainUI()
        {
            InitializeComponent();
        }
        public void Init()
        {
            init_Display();
            update_Display();
            CommonLogClass.Instance.SetRichTextBox(richTextBox1);

            lblAlarm = label5;
            lblState = label11;
            lblPlcTestRun = label1;

            btnLinescanTest = button2;
            btnLinescanTest.Click += BtnLinescanTest_Click;

            btnManualAuto = button1;

            btnStart = button6;
            btnStop = button4;
            btnReset = button7;
            btnSingleTest = button3;
            btnClearAlarm = button8;
            btnMute = button9;
            btnFeedCy = button5;
            btnTakeCy = button10;
            btnCylevel = button11;
            btnCyVerical = button12;
            btnPlcTestRun = button13;

            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnReset.Click += BtnReset_Click;
            btnSingleTest.Click += BtnSingleTest_Click;
            btnClearAlarm.Click += BtnClearAlarm_Click;
            btnMute.Click += BtnMute_Click;

            btnFeedCy.Click += BtnFeedCy_Click;
            btnTakeCy.Click += BtnTakeCy_Click;
            btnCylevel.Click += BtnCylevel_Click;
            btnCyVerical.Click += BtnCyVerical_Click;
            btnPlcTestRun.Click += BtnPlcTestRun_Click;

            m_TrackItemUI[0] = trackItemUI1;
            //m_TrackItemUI[1] = trackItemUI2;
            //m_TrackItemUI[2] = trackItemUI3;
            //m_TrackItemUI[3] = trackItemUI4;

            int i = 0;
            while (i < TRACK_MAX_COUNT)
            {
                m_TrackItemUI[i].Initial(Traveller106.Universal.VERSION, Traveller106.Universal.OPTION, MACHINE, (TrackArea)i);
                i++;
            }

            m_plcCommError = new bool[MACHINE.PLCCollection.Length];
            i = 0;
            while (i < MACHINE.PLCCollection.Length)
            {
                m_plcCommError[i] = false;
                i++;
            }

            MACHINE.EVENT.Initial(lblAlarm);
            MACHINE.TriggerAction += MACHINE_TriggerAction;
            MACHINE.EVENT.TriggerAlarm += EVENT_TriggerAlarm;
            MACHINE.MachineCommErrorStringAction += MACHINE_MachineCommErrorStringAction;

            InitAllProcesses();
            StopAllProcesses("INIT");

            tabControl2.Controls.RemoveAt(1);
            tabControl2.Controls.RemoveAt(1);
#if FX3U
            btnManualAuto.Visible = true;
            btnManualAuto.Click += BtnManualAuto_Click;
#endif
        }

        private void BtnManualAuto_Click(object sender, EventArgs e)
        {
            //MACHINE.PLCIO.ADR_ISAUTO_AND_MANUAL = !MACHINE.PLCIO.ADR_ISAUTO_AND_MANUAL;
        }

        private void BtnPlcTestRun_Click(object sender, EventArgs e)
        {
            //MACHINE.PLCIO.PlcTestRun = !MACHINE.PLCIO.PlcTestRun;
        }

        private void BtnCyVerical_Click(object sender, EventArgs e)
        {
            MACHINE.PLCIO.CyVerical = !MACHINE.PLCIO.CyVerical;
        }

        private void BtnCylevel_Click(object sender, EventArgs e)
        {
            MACHINE.PLCIO.CyLevel = !MACHINE.PLCIO.CyLevel;
        }

        private void BtnTakeCy_Click(object sender, EventArgs e)
        {
            MACHINE.PLCIO.CyTake = !MACHINE.PLCIO.CyTake;
        }

        private void BtnFeedCy_Click(object sender, EventArgs e)
        {
            MACHINE.PLCIO.CyFeed = !MACHINE.PLCIO.CyFeed;
        }

        private void BtnMute_Click(object sender, EventArgs e)
        {
            MACHINE.PLCIO.ADR_BUZZER = false;
        }

        private void BtnClearAlarm_Click(object sender, EventArgs e)
        {
            //CommonLogClass.Instance.LogMessage("點擊清除警報", Color.Blue);
            //if (MACHINE.EVENT.GetAlarmCount() == 0)
            //{
            //    CommonLogClass.Instance.LogMessage("當前無警報，請勿操作。", Color.Black);
            //    return;
            //}

            string onStrMsg = "請檢查警報是否清除？";
            string offStrMsg = "請檢查警報是否清除？";
            string msg = (true ? offStrMsg : onStrMsg);

            //if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                MACHINE.ClearAlarm = true;
                MACHINE.EVENT.RemoveAlarm();
            }
        }

        private void BtnSingleTest_Click(object sender, EventArgs e)
        {

            //string filenamestr = JzToolsClass.OpenFilePicker("BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            //if (string.IsNullOrEmpty(filenamestr))
            //    return;
            //FreeImageBitmap freeImageBitmap = new FreeImageBitmap(filenamestr);
            //FreeImageBitmap freeImageBitmaptemp = new FreeImageBitmap(freeImageBitmap);
            //freeImageBitmap.Dispose();

            //int m_CollectDataIndex = 0;
            ////模拟测试的数据
            //AnalyzeClass assignClass = new AnalyzeClass();
            //assignClass.Index = m_CollectDataIndex;
            //assignClass.SaveFileName = JzTimes.DateTimeSerialStringFFF;
            //assignClass.myPath = Traveller106.Universal.COLLECT + "\\tmp_" + m_CollectDataIndex.ToString("0000") + ".cfg";
            //assignClass.RectFStart = myRecipe.rect_start;
            //assignClass.RectFEnd = myRecipe.rect_end;
            //assignClass.CreateRowCol(myRecipe.TrayRowCount,
            //                                             myRecipe.TrayColCount);

            //assignClass.IsDrawRect = myRecipe.IsDrawRect;
            //assignClass.freeImageBitmapInput = new FreeImageBitmap(freeImageBitmaptemp);

            ////if (INI.Instance.IsSaveDebugBMP)
            ////    assignClass.freeImageBitmapInput.ToBitmap().Save(
            ////        Traveller106.Universal.DEBUGRESULTPATH + "\\" +
            ////        assignClass.SaveFileName + $"_Step_{m_CollectDataIndex.ToString()}.bmp",
            ////        System.Drawing.Imaging.ImageFormat.Bmp);

            //assignClass.Run();

            //freeImageBitmaptemp.Dispose();
        }

        bool IsAlarmsSeriousX = false;
        bool IsAlarmsCommonX = false;
        bool IsWarningCommonX = false;

        bool IsEMCTriggered = false;
        //string m_WarningStrTmp = string.Empty;//缓存报警信息
        //JzTimes m_WarningRefreshTimer = new JzTimes();
        int m_WaringValueTmp = 0;
        object m_AlmObj = new object();
        private void MACHINE_TriggerAction(MachineEventEnum machineevent, object obj)
        {
            m_AlmObj = obj;
            switch (machineevent)
            {
                case MachineEventEnum.ALARM_SERIOUS:
                    IsAlarmsSeriousX = true;
                    SetAbnormalLight();
                    break;
                case MachineEventEnum.ALARM_COMMON:
                    IsAlarmsCommonX = true;
                    SetAbnormalLight();
                    break;
                case MachineEventEnum.ALARM_WARNING:
                    IsWarningCommonX = true;
                    break;
                case MachineEventEnum.EMC:
                    IsEMCTriggered = true;
                    break;
            }
        }
        private void EVENT_TriggerAlarm(bool IsBuzzer)
        {
            //MACHINE.PLCIO.ADR_BUZZER = IsBuzzer && !INI.Instance.plc_mute_buzzer;
            if (!IsBuzzer)
            {
                SetNormalLight();
            }

        }
        private void MACHINE_MachineCommErrorStringAction(string str)
        {
            //輸出那個plc掉綫
            int index = 0;
            string _plcIndex = str.Replace("PLC", "");
            bool bOK = int.TryParse(_plcIndex, out index);
            string _errorStr = "plc通訊中斷!!!\r\n(編號Index=" + index.ToString() + ")\r\n是否重連?";
            //先停掉流程
            StopAllProcesses("DisPlc");
            if (!m_plcCommError[index])
            {
                m_plcCommError[index] = true;
                if (VsMSG.Instance.Question(_errorStr) == DialogResult.OK)
                {
                    //重連
                    MACHINE.PLCCollection[index].RetryConn();
                    m_plcCommError[index] = false;
                }
                else
                {
                    try
                    {
                        Environment.Exit(0);
                    }
                    catch
                    {

                    }
                }
            }
        }

        private void alarmTick()
        {

            #region ALARM
            if (IsEMCTriggered)
            {
                IsEMCTriggered = false;
                StopAllProcesses("EMC");
            }

            if (IsAlarmsSeriousX)
            {
                IsAlarmsSeriousX = false;
                StopAllProcesses("ALM");
            }

            if (IsAlarmsCommonX)
            {
                IsAlarmsCommonX = false;
                //StopAllProcesses();
            }

            if (IsWarningCommonX)
            {
                IsWarningCommonX = false;
            }

            #endregion
        }

        
        BaseProcess m_BuzzerProcess
        {
            get { return BuzzerProcess.Instance; }
        }
        BaseProcess m_resetprocess
        {
            get { return ResetProcess.Instance; }
        }
        BaseProcess m_LineScanProcess
        {
            get { return LineScanProcess.Instance; }
        }
        BaseProcess m_MainProcess
        {
            get { return MainProcess.Instance; }
        }

        void StopAllProcesses(string reason = "")
        {
            if (reason == "INIT")
                return;

            string _msg = string.Empty;
            if (m_AlmObj != null)
                _msg = (string)(m_AlmObj.ToString());

            m_resetprocess.Stop();
            m_LineScanProcess.Stop();
            m_MainProcess.Stop();
            FireChangeState(MainS1State.S1_READY);
            FireChangeState(MainS1State.LS_STOP);

            switch (reason)
            {
                case "INIT":
                    break;
                case "EMC":
                    CommonLogClass.Instance.LogMessage(reason + "-" + "接收到急停信號", Color.Red);
                    break;
                case "ALM":
                    CommonLogClass.Instance.LogMessage(reason + "-" + "接收到報警信號", Color.Red);
                    SetSeriousAlarms1();
                    break;
                case "DisPlc":
                    CommonLogClass.Instance.LogMessage(reason + "-" + "與plc的通訊中斷", Color.Red);
                    break;
                case "USERSTOP":
                    m_BuzzerProcess.Stop();
                    CommonLogClass.Instance.LogMessage(reason + "-" + "用戶手動停止流程", Color.Red);
                    break;
                //default:

                    
                //    break;
            }
            //if (INI.Instance.pcForceStopPlc)
            //{
            //    MACHINE.PLCIO.ForceStopPlcProcess = true;
            //    CommonLogClass.Instance.LogMessage("ForceStopPlcProcess强制停止PLC流程", Color.Red);
            //}
                
        }
        void InitAllProcesses()
        {
            //----------------------------------------------------------------
            // (1) 大部的 Processes 應該可以當成 MainProcess 的 Child Process,
            //      可以集中由 MainProcess 管理, 形成一體 Model.
            // (2) 以下對 Process Event Handler 的掛載.
            //      在 Model-View-Control 的架構規範下, 屬於 Control.
            //      ~ 以後再從 GUI(MainGdx3UI) 抽離出來.
            //----------------------------------------------------------------
            //m_mainprocess.OnCompleted += process_OnCompleted;
            // Buzzer 的結束 用來檢視是否有 NG 發生.
            m_MainProcess.OnCompleted += process_OnCompleted;
            m_BuzzerProcess.OnCompleted += buzzer_OnCompleted;
            m_resetprocess.OnCompleted += process_OnCompleted;
            m_LineScanProcess.OnCompleted += process_OnCompleted;
            m_LineScanProcess.OnLiveImage += M_LineScanProcess_OnLiveImage;
            m_MainProcess.OnMessage += process_OnMessage;
        }

        private void process_OnMessage(object sender, ProcessEventArgs e)
        {
            if (sender == m_MainProcess)
            {
                if (e.Message.Contains("Reset.Data"))
                {
                    TrackItem.ResetData();
                }
                else if (e.Message.Contains("Record.Start"))
                {
                    FireChangeState(MainS1State.LS_START);
                }
                else if (e.Message.Contains("Record.Stop"))
                {
                    FireChangeState(MainS1State.LS_STOP);
                }
            }

            try
            {
                //if (sender == m_mainprocess)
                //{
                //    handle_main_process_completed(sender, e);
                //    if (e.Message == "PartialCompleted")
                //        return;
                //}

                // Do whatever message you want to show to the operators.
                string msg = $"程序 {((BaseProcess)sender).Name}, 复位数据!\n";
                CommonLogClass.Instance.LogMessage(msg, Color.Black);
            }
            catch
            {
            }
        }

        void TickAllProcesses()
        {
            m_resetprocess.Tick();
            m_BuzzerProcess.Tick();
            m_LineScanProcess.Tick();
            m_MainProcess.Tick();
        }

        private void M_LineScanProcess_OnLiveImage(object sender, ProcessEventArgs e)
        {
            if (e.Tag != null && e.Tag is Bitmap)
            {
                try
                {
                    if (InvokeRequired)
                    {
                        EventHandler<ProcessEventArgs> h = M_LineScanProcess_OnLiveImage;
                        this.Invoke(h, sender, e);
                    }
                    else
                    {
                        //@LETIAN: 2022/07/01 改用 GdxDispUI 增加一些 fps
                        // bmp 由 Sender maintains life cycle.
                        // 在此不用 Dispose
                        Bitmap bmp = (Bitmap)e.Tag;
                        //dispUI1.UpdateLiveImage(bmp);
                        DS1.ReplaceDisplayImage(bmp);
                    }
                }
                catch (Exception ex)
                {
                    //>>> 此一層的 try - catch 以後可以省略.
                    //>>> 會由 Event Sender 處理 exception
                    throw ex;
                }
            }
        }
        private void process_OnCompleted(object sender, ProcessEventArgs e)
        {
            if (sender == m_resetprocess)
            {
                if (m_resetprocess.RelateString == "CloseWindows")
                {
                    //執行的關閉流程 這裏則跳出
                    return;
                }
                
            }

            try
            {
                //if (sender == m_mainprocess)
                //{
                //    handle_main_process_completed(sender, e);
                //    if (e.Message == "PartialCompleted")
                //        return;
                //}
                FireChangeState(MainS1State.S1_READY);
                // Do whatever message you want to show to the operators.
                string msg = $"程序 {((BaseProcess)sender).Name}, 已完成!\n";
                CommonLogClass.Instance.LogMessage(msg, Color.Black);
            }
            catch
            {
            }
        }
        private void buzzer_OnCompleted(object sender, ProcessEventArgs e)
        {
            if (InvokeRequired)
            {
                EventHandler<ProcessEventArgs> h = buzzer_OnCompleted;
                BeginInvoke(h, sender, e);
            }
            else
            {
                //if (m_mainprocess.LastNG != null)
                //{
                //    //>>> MessageBox.Show(m_mainprocess.LastNG);
                //    var errMsg = m_mainprocess.LastNG + "\n\r\n\r(後續可按 復位 排除NG態)";
                //    VsMSG.Instance.Warning(errMsg);
                //}
            }
        }
        private void handle_main_process_completed(object sender, ProcessEventArgs e)
        {
            if (InvokeRequired)
            {
                EventHandler<ProcessEventArgs> h = handle_main_process_completed;
                BeginInvoke(h, sender, e);
            }
            else
            {
                if (e.Message == "PartialCompleted")
                {
                    //var barcode = m_mainprocess.Barcode;
                    //var ngMsg = m_mainprocess.LastNG;
                    //int mirrorIdx = m_mainprocess.MainMirrorIndex;

                    //try
                    //{
                    //    var args = (object[])e.Tag;
                    //    mirrorIdx = (int)args[0];
                    //    ngMsg = (string)args[1];
                    //}
                    //catch (Exception ex)
                    //{
                    //    GdxGlobal.LOG.Warn(ex, "PartialCompleted Event 格式有誤!");
                    //    return;
                    //}

                    //var gen = new ZxReportGenerator();
                    //gen.GenerateReports(barcode, ngMsg, mirrorIdx);

                    //if (lblPassSign != null)
                    //{
                    //    lblPassSign.Text = (ngMsg == null) ? "PASS" : "NG";
                    //    lblPassSign.ForeColor = (ngMsg == null) ? Color.Green : Color.Red;
                    //}
                }
                else
                {
                    //// Final Completed 
                    //if (txtBarcode != null)
                    //    txtBarcode.Text = "";
                    //_sim_auto_barcode();
                }
            }
        }

        private void BtnLinescanTest_Click(object sender, EventArgs e)
        {
            string onStrMsg = "是否要进行线扫测试？";
            string offStrMsg = "是否要停止线扫测试？";
            string msg = (m_LineScanProcess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                INI.Instance.HistoryDataPath = Traveller106.Universal.HISTORY + "\\" + JzTimes.DateSerialString;
                INI.Instance.HistoryDataBarcode = JzTimes.DateTimeSerialString;

                //LineScanProcess.Instance.SetTestMode(Eazy_Project_III.LinescanTestMode.Ls_NoTray);
                if (!m_LineScanProcess.IsOn)
                {
                    m_LineScanProcess.Start();
                }
                else
                {
                    m_LineScanProcess.Stop();
                }
            }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            //string onStrMsg = "是否要进行复位？";
            //string offStrMsg = "是否要停止复位流程？";
            //string msg = (m_resetprocess.IsOn ? offStrMsg : onStrMsg);
            //if (MACHINE.PLCIO.ADR_ISAUTO_AND_MANUAL && !Universal.IsNoUseIO)
            //{
            //    VsMSG.Instance.Warning("請切換手動模式，无法启动。");
            //    CommonLogClass.Instance.LogMessage("請切換手動模式，无法启动。 ", Color.Yellow);
            //    return;
            //}
            //if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            //{
            //    if (!m_resetprocess.IsOn)
            //    {
            //        m_resetprocess.Start();
            //        FireChangeState(MainS1State.S1_RESETING);
            //    }
            //    else
            //    {
            //        m_resetprocess.Stop();
            //        FireChangeState(MainS1State.S1_READY);
            //    }
            //}
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            
            CommonLogClass.Instance.LogMessage("手动停止 ", Color.Yellow);
            StopAllProcesses("USERSTOP");
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            //if (!MACHINE.PLCIO.ADR_RESETCOMPLETE && !Universal.IsNoUseIO)
            //{
            //    VsMSG.Instance.Warning("請先執行復位，无法启动。");
            //    CommonLogClass.Instance.LogMessage("請先執行復位，无法启动。 ", Color.Yellow);
            //    return;
            //}
            //if (!MACHINE.PLCIO.ADR_ISAUTO_AND_MANUAL && !Universal.IsNoUseIO)
            //{
            //    VsMSG.Instance.Warning("請切換自動模式，无法启动。");
            //    CommonLogClass.Instance.LogMessage("請切換自動模式，无法启动。 ", Color.Yellow);
            //    return;
            //}
            //if (m_resetprocess.IsOn)
            //{
            //    VsMSG.Instance.Warning("复位流程中，无法启动。");
            //    CommonLogClass.Instance.LogMessage("复位流程中，无法启动。 ", Color.Yellow);
            //    return;
            //}
            //if (m_LineScanProcess.IsOn)
            //{
            //    VsMSG.Instance.Warning("綫掃流程中，无法启动。");
            //    CommonLogClass.Instance.LogMessage("綫掃流程中，无法启动。 ", Color.Yellow);
            //    return;
            //}

            //if (!m_MainProcess.IsOn)
            //{
            //    //LineScanProcess.Instance.SetTestMode(Eazy_Project_III.LinescanTestMode.Ls_Tray);
            //    CommonLogClass.Instance.LogMessage("手动启动 ", Color.Yellow);
            //    m_MainProcess.Start();
            //    FireChangeState(MainS1State.S1_RUNNING);
            //}
            //else
            //{
            //    VsMSG.Instance.Warning("自动跑线中");
            //    CommonLogClass.Instance.LogMessage("自动跑线中 ", Color.Yellow);
            //}
        }
        bool m_reset = false;
        bool m_start = false;
        public void _plcSignTick()
        {
            if (!Universal.IsNoUseIO)
            {
                if (MACHINE.PLCIO.ADR_ISRESET && btnReset.Enabled)
                {
                    if (!m_reset)
                    {
                        if (!m_resetprocess.IsOn && !MACHINE.PLCIO.ADR_RESETING)
                        {
                            m_reset = true;
                            //btnReset.PerformClick();
                            m_resetprocess.Start();
                            FireChangeState(MainS1State.S1_RESETING);
                        }
                    }
                }
                else
                {
                    m_reset = false;
                }

                if (MACHINE.PLCIO.ADR_ISSTART && btnStart.Enabled)
                {
                    if (!m_start)
                    {
                        if (!m_MainProcess.IsOn && !m_LineScanProcess.IsOn)
                        {
                            m_start = true;
                            btnStart.PerformClick();
                            //m_MainProcess.Start();
                        }
                    }
                }
                else
                {
                    m_start = false;
                }
            }
        }

        public void Tick()
        {
            _plcSignTick();
            alarmTick();
            updateStateUI();
            int i = 0;
            while (i < TRACK_MAX_COUNT)
            {
                m_TrackItemUI[i].Tick();
                i++;
            }

            //CAM0.Snap();
            //DS1.ReplaceDisplayImage(CAM0.GetSnap());

            btnFeedCy.BackColor = (MACHINE.PLCIO.CyFeed ? Color.Red : Color.FromArgb(192, 255, 192));
            btnTakeCy.BackColor = (MACHINE.PLCIO.CyTake ? Color.Red : Color.FromArgb(192, 255, 192));
            btnCylevel.BackColor = (MACHINE.PLCIO.CyLevel ? Color.Red : Color.FromArgb(192, 255, 192));
            btnCyVerical.BackColor = (MACHINE.PLCIO.CyVerical ? Color.Red : Color.FromArgb(192, 255, 192));
            
            lblfeedout.BackColor = (MACHINE.PLCIO.IsFeedOut ? Color.Lime : Control.DefaultBackColor);
            lblfeediin.BackColor = (MACHINE.PLCIO.IsFeedIn ? Color.Lime : Control.DefaultBackColor);

            lbltakeout.BackColor = (MACHINE.PLCIO.IsTakeOut ? Color.Lime : Control.DefaultBackColor);
            lbltakein.BackColor = (MACHINE.PLCIO.IsTakeIn ? Color.Lime : Control.DefaultBackColor);

            lbllevelclamp.BackColor = (MACHINE.PLCIO.IsLevelClamp ? Color.Lime : Control.DefaultBackColor);
            lbllevelloosen.BackColor = (MACHINE.PLCIO.IsLevelLoosen ? Color.Lime : Control.DefaultBackColor);

            lblvericalclamp.BackColor = (MACHINE.PLCIO.IsVericalClamp ? Color.Lime : Control.DefaultBackColor);
            lblvericalloosen.BackColor = (MACHINE.PLCIO.IsVericalLoosen ? Color.Lime : Control.DefaultBackColor);

            btnReset.BackColor = (m_resetprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));

#if FX3U
            lblPlcTestRun.Visible = MACHINE.PLCIO.PlcTestRun;
            btnPlcTestRun.BackColor = (MACHINE.PLCIO.PlcTestRun ? Color.Red : Color.FromArgb(192, 255, 192));
            btnPlcTestRun.Text = (MACHINE.PLCIO.PlcTestRun ? "調試模式" : "生產模式");
            btnManualAuto.BackColor = (MACHINE.PLCIO.ADR_ISAUTO_AND_MANUAL ? Color.Green : Color.FromArgb(192, 255, 192));
            btnManualAuto.Text = (MACHINE.PLCIO.ADR_ISAUTO_AND_MANUAL ? "自动" : "手动");
#endif


            btnLinescanTest.BackColor = (m_LineScanProcess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            btnStart.BackColor = (m_MainProcess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            TickAllProcesses();
        }
        public void ChangeRecipe()
        {
            int i = 0;
            while (i < TRACK_MAX_COUNT)
            {
                m_TrackItemUI[i].ChangeRecipe();
                i++;
            }
        }
        public void SetEnable(bool isendable)
        {
            //pnlButtons.Enabled = isendable;
            //btnStart.Enabled = isendable;
            //btnReset.Enabled = isendable;

            panel1.Enabled = isendable;
            panel2.Enabled = isendable;
            //trackItemUI1.Enabled = isendable;
            DS1.Enabled = isendable;
            trackItemUI1.Enable(isendable);
        }
        public void SetEnableState(bool isendable)
        {
            //pnlButtons.Enabled = isendable;
            btnStart.Enabled = isendable;
            btnReset.Enabled = isendable;

            panel1.Enabled = isendable;
            panel2.Enabled = isendable;
            trackItemUI1.Enabled = isendable;
            DS1.Enabled = isendable;
        }
        private void updateStateUI()
        {
            bool iAlarmsCommon = MACHINE.PLCIO.IsAlarmsCommon;
            bool iAlarmsSerious = MACHINE.PLCIO.IsAlarmsSerious;

            //int iAlarmsCommon = MACHINE.PLCIO.IntAlarmsCommon;
            //int iAlarmsSerious = MACHINE.PLCIO.IntAlarmsSerious;
            if (iAlarmsCommon || iAlarmsSerious)
            {
                lblState.Text = $"报警中 [{MACHINE.PLCIO.GetAlmValue}] [{iAlarmsCommon}]" + $" [{iAlarmsSerious}]";
                if (MACHINE.IsClearAlarmCache)
                    return;
                if (iAlarmsCommon)
                {
                    //CommonLogClass.Instance.LogMessage($"常规报警[{MACHINE.PLCIO.IntAlarmsCommon}] ", Color.Red);
                    SetCommonAlarms();
                }
                if (iAlarmsSerious)
                {
                    //CommonLogClass.Instance.LogMessage($"严重报警[{MACHINE.PLCIO.IntAlarmsSerious}] ", Color.Red);
                    SetSeriousAlarms1();
                }
            }
            //if (iAlarmsCommon != 0 || iAlarmsSerious != 0)
            //{
            //    lblState.Text = $"报警中 [{iAlarmsCommon}]" + $" [{iAlarmsSerious}]";
            //    if (MACHINE.IsClearAlarmCache)
            //        return;
            //    if (iAlarmsCommon != 0)
            //    {
            //        //CommonLogClass.Instance.LogMessage($"常规报警[{MACHINE.PLCIO.IntAlarmsCommon}] ", Color.Red);
            //        SetCommonAlarms();
            //    }
            //    if (iAlarmsSerious != 0)
            //    {
            //        //CommonLogClass.Instance.LogMessage($"严重报警[{MACHINE.PLCIO.IntAlarmsSerious}] ", Color.Red);
            //        SetSeriousAlarms1();
            //    }
            //}
            else if (MACHINE.PLCIO.ADR_ISPAUSE)
                lblState.Text = "设备暂停中  ";
            else if (m_LineScanProcess.IsOn)
                lblState.Text = "执行-线扫测试中  " + m_LineScanProcess.ID.ToString();
            else if (m_resetprocess.IsOn)
                lblState.Text = "复位中 " + m_resetprocess.ID.ToString();
            else if (m_MainProcess.IsOn)
                lblState.Text = "跑线中 " + m_MainProcess.ID.ToString();
            else
                lblState.Text = "待机";

            if (Traveller106.Universal.IsNoUseIO)
            {
                lblState.Text = "模擬運行";
                lblState.BackColor = Color.Red;
            }
            else if (MACHINE.PLCIO.ADR_ISEMC)
            {
                lblState.Text = "急停中";
                lblState.BackColor = Color.Red;
            }
            //else if (MACHINE.PLCIO.ADR_ISDOOR)
            //{
            //    //lblState.Text = "门被打开";
            //    //lblState.BackColor = Color.Red;
            //}
            else
            {
                lblState.BackColor = Color.Black;
            }

            lblState.Text = LanguageExClass.Instance.ToTraditionalChinese(lblState.Text);
        }
        void SetSeriousAlarms1()
        {
            if (MACHINE.PLCIO.PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS] == null)
                return;
            if(MACHINE.PLCIO.GetAlmValue > 0)
            {
                foreach (PLCAlarmsItemDescriptionClass item in MACHINE.PLCIO.PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS].PLCALARMSDESCLIST)
                {
                    if (MACHINE.PLCIO.GetAlarmsAddress(item.BitNo, item.ADR_Address))
                    {
                        MACHINE.EVENT.GenEvent("A0001", EventActionTypeEnum.AUTOMATIC, item.ADR_Chinese, ACCDB.AccNow);
                    }
                }
            }
        }
        void SetCommonAlarms()
        {
            if (MACHINE.PLCIO.PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON] == null)
                return;
            foreach (PLCAlarmsItemDescriptionClass item in MACHINE.PLCIO.PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON].PLCALARMSDESCLIST)
            {
                if (MACHINE.PLCIO.GetAlarmsAddress(item.BitNo, item.ADR_Address))
                {
                    MACHINE.EVENT.GenEvent("W0000", EventActionTypeEnum.AUTOMATIC, item.ADR_Chinese, ACCDB.AccNow);
                }
            }
        }

        void SetNormalLight()
        {
            MACHINE.PLCIO.ADR_RED = false;
            MACHINE.PLCIO.ADR_YELLOW = true;
            MACHINE.PLCIO.ADR_GREEN = false;
        }
        void SetAbnormalLight()
        {
            MACHINE.PLCIO.ADR_RED = true;
            MACHINE.PLCIO.ADR_YELLOW = false;
            MACHINE.PLCIO.ADR_GREEN = false;
        }
        void SetRunningLight()
        {
            MACHINE.PLCIO.ADR_RED = false;
            MACHINE.PLCIO.ADR_YELLOW = false;
            MACHINE.PLCIO.ADR_GREEN = true;
        }
        void init_Display()
        {
            DS1.Initial(100, 0.01f);
            DS1.SetDisplayType(DisplayTypeEnum.SHOW);
            //DS2.Initial(100, 0.01f);
            //DS2.SetDisplayType(DisplayTypeEnum.SHOW);
            //DS3.Initial(100, 0.01f);
            //DS3.SetDisplayType(DisplayTypeEnum.SHOW);
        }
        void update_Display()
        {
            DS1.Refresh();
            DS1.DefaultView();
            //DS2.Refresh();
            //DS2.DefaultView();
            //DS3.Refresh();
            //DS3.DefaultView();
        }

        public delegate void ChangeStateHandler(MainS1State status);
        public event ChangeStateHandler OnChangeState;
        protected void FireChangeState(MainS1State status)
        {
            if (OnChangeState != null)
            {
                OnChangeState(status);
            }
        }

    }
}
