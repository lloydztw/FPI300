using Common.RecipeSpace;
using Eazy_Project_III;
using FreeImageAPI;
using JetEazy.BasicSpace;
using JetEazy.ControlSpace;
using JetEazy;
using JzDisplay;
using NeedleX.ProcessSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
//using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using Traveller106;
using TravellerMINIX6.OPSpace;
using TravellerMINIX6.ProcessSpace;
using TravellerMINIX6.UISpace.MainSpace;
using VsCommon.ControlSpace.IOSpace;
using JetEazy.DBSpace;
using VsCommon.ControlSpace.MachineSpace;
using VsCommon.ControlSpace;
using AForge.Imaging.Filters;
using JetEazy.UISpace;
using PhotoMachine.UISpace;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using LaserAlignDX.BasicSpace;
using MoveGraphLibrary;
using WorldOfMoveableObjects;
using Traveller106;
using JetEazy.FormSpace;
using OpenCvSharp.Flann;
using System.IO;
using System.Drawing.Imaging;
//using Universal = Traveller106.Universal;

namespace LaserAlignDX.UISpace.MainSpace
{
    public partial class MainX1UI : UserControl, IMainUI
    {
        Mover myMover = new Mover();

        Button btnSoftwareReady;
        Label lblState;
        bool m_plcLineStartOld = false;

        protected RecipeMiniX6Class myRecipe
        {
            get { return RecipeMiniX6Class.Instance; }
        }
        protected MachineCollectionClass MACHINECollection
        {
            get
            {
                return Traveller106.Universal.MACHINECollection;
            }
        }
        protected MainX1MachineClass MACHINE
        {
            get { return (MainX1MachineClass)Traveller106.Universal.MACHINECollection.MACHINE; }
        }
        protected AccDBClass ACCDB
        {
            get
            {
                return Traveller106.Universal.ACCDB;
            }
        }
        protected RCPDBClass RCPDB
        {
            get
            {
                return Traveller106.Universal.RCPDB;
            }
        }


        JetEazy.VersionEnum VERSION
        {
            get
            {
                return Traveller106.Universal.VERSION;
            }
        }
        JetEazy.OptionEnum OPTION
        {
            get
            {
                return Traveller106.Universal.OPTION;
            }
        }

        public MainX1UI()
        {
            InitializeComponent();
        }

        public void Init()
        {
            init_Display();
            update_Display();
            CommonLogClass.Instance.SetRichTextBox(richTextBox1);
            InitAllProcesses();

            lblState = label11;

            btnSoftwareReady = button6;
            btnSoftwareReady.Click += BtnSoftwareReady_Click;

            SizeChanged += MainX1UI_SizeChanged;


        }

        private void BtnSoftwareReady_Click(object sender, EventArgs e)
        {

            MACHINE.PLCIO.SoftwareReady = !MACHINE.PLCIO.SoftwareReady;
            if (m_LineScanProcess.IsOn)
                m_LineScanProcess.Stop();
            //MACHINE.PLCIO.LineScanRecipe = RCPDB.RCPItemNow.Name;

            //string onStrMsg = "是否要进行线扫测试？";
            //string offStrMsg = "是否要停止线扫测试？";
            //string msg = (m_LineScanProcess.IsOn ? offStrMsg : onStrMsg);

            //if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            //{
            //    INI.Instance.HistoryDataPath = Traveller106.Universal.HISTORY + "\\" + JzTimes.DateSerialString;
            //    INI.Instance.HistoryDataBarcode = JzTimes.DateTimeSerialString;

            //    LineScanProcess.Instance.SetTestMode(Eazy_Project_III.LinescanTestMode.Ls_NoTray);
            //    if (!m_LineScanProcess.IsOn)
            //    {
            //        m_LineScanProcess.Start();
            //    }
            //    else
            //    {
            //        m_LineScanProcess.Stop();
            //    }
            //}
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
        BaseProcess m_SingleProcess
        {
            get { return LineScanSingleProcess.Instance; }
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
            m_LineScanProcess.OnLiveImage += process_OnLiveImage;
            m_MainProcess.OnMessage += process_OnMessage;
            m_SingleProcess.OnMessage += process_OnMessage;
            m_LineScanProcess.OnMessage += process_OnMessage;
            m_SingleProcess.OnLiveImage += process_OnLiveImage;

        }

        private void process_OnMessage(object sender, ProcessEventArgs e)
        {
            if (sender == m_MainProcess)
            {
                if (e.Message.Contains("Reset.Data"))
                {

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
            else if (sender == m_LineScanProcess || sender == m_SingleProcess)
            {
                if (e.Message.Contains("Result.1"))
                {
                    FireChangeState(MainS1State.M_PASS);
                }
                else if (e.Message.Contains("Result.2"))
                {
                    FireChangeState(MainS1State.M_NG);
                }
                else if (e.Message.Contains("Record.Start"))
                {
                    myMover.Clear();
                    FireChangeState(MainS1State.LS_START);
                }
                else if (e.Message.Contains("Record.Stop"))
                {
                    FireChangeState(MainS1State.LS_STOP);
                }
                else if (e.Message.Contains("Show.X"))
                {

                    List<ShowRectClass> list = e.Tag as List<ShowRectClass>;
                    if (list != null)
                    {
                        if (list.Count > 0)
                        {
                            int i = 0;
                            foreach (ShowRectClass showRect in list)
                            {
                                JzRectEAG _rect1_1 = new JzRectEAG(Color.FromArgb(0, Color.Blue), showRect.Bounds);

                                _rect1_1.RelateNo = i;
                                _rect1_1.PenWidth = INI.Instance.DrawLineWidth;
                                _rect1_1.FontSize = INI.Instance.DrawFontSize;
                                _rect1_1.Desc = showRect.Description;
                                _rect1_1.NoShowCorner = true;
                                if (showRect.IsPass)
                                    _rect1_1.RelateLevel = 2;
                                else
                                    _rect1_1.RelateLevel = 7;
                                myMover.Add(_rect1_1);

                                i++;
                            }

                            DS1.SetMover(myMover);
                            update_Display();

                            #region 保存显示的图片


                            string _outputimage_path = $"{INI.Instance.ResultImagePath}\\S{myRecipe.ViewAnalyzeClass.SaveFileName}.jpg";

                            //FreeImageBitmap freeImageBitmapInputResult = new FreeImageBitmap(freeImageBitmapOutputDraw);

                            Task task = new Task(() =>
                            {
                                try
                                {
                                    if (INI.Instance.IsOpenUpload)
                                    {
                                        string _path = $"D:\\FtpUpload\\{INI.Instance.CurrentLotName}\\";
                                        if (!Directory.Exists(_path))
                                            Directory.CreateDirectory(_path);

                                        string _filename = $"{_path}{INI.Instance.CurrentBarcodeStr}.jpg";

                                        Bitmap _img = new Bitmap(DS1.GetScreen(INI.Instance.DrawLineWidth, INI.Instance.DrawFontSize));
                                        Bitmap _imgUpload = new Bitmap(_img, new Size(_img.Width >> 2, _img.Height >> 2));
                                        _imgUpload.Save(_filename, ImageFormat.Jpeg);

                                        _img.Dispose();
                                        _imgUpload.Dispose();

                                    }

                                    if (INI.Instance.IsSaveStripImage)
                                    {
                                        SaveImageWithQuality(DS1.GetScreen(INI.Instance.DrawLineWidth, INI.Instance.DrawFontSize), _outputimage_path, INI.Instance.ImageQuality);
                                        //freeImageBitmapInputResult.Save(_outputimage_path, FREE_IMAGE_FORMAT.FIF_JPEG);
                                        //freeImageBitmapOutputDraw.Dispose();
                                        //freeImageBitmapInputResult.Dispose();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //logInfo.Log($"保存图片错误{ex.Message} ");
                                }
                            });
                            task.Start();

                            #endregion
                        }
                    }
                }
                else if (e.Message.Contains("ResultX.Code"))
                {
                    INI.Instance.CurrentBarcodeStr = e.Tag as string;
                    FireChangeState(MainS1State.M_SHOWCODE, e.Tag as string);
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
                string msg = $"Process {((BaseProcess)sender).Name}, {e.Message}\n";
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
            m_SingleProcess.Tick();
        }

        private void process_OnLiveImage(object sender, ProcessEventArgs e)
        {
            if (e.Tag != null && e.Tag is Bitmap)
            {
                try
                {
                    if (InvokeRequired)
                    {
                        EventHandler<ProcessEventArgs> h = process_OnLiveImage;
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
                string msg = $"Process {((BaseProcess)sender).Name}, Completed!\n";
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

        public void Tick()
        {
            updateStateUI();
            TickAllProcesses();
        }
        public void ChangeRecipe()
        {

        }
        public void SetEnable(bool isendable)
        {
            //pnlButtons.Enabled = isendable;
            //btnStart.Enabled = isendable;
            //btnReset.Enabled = isendable;

            //panel1.Enabled = isendable;
            //panel2.Enabled = isendable;
            //trackItemUI1.Enabled = isendable;
            //DS1.Enabled = isendable;
            //trackItemUI1.Enable(isendable);
        }
        public void SetEnableState(bool isendable)
        {
            //pnlButtons.Enabled = isendable;
            //btnStart.Enabled = isendable;
            //btnReset.Enabled = isendable;

            //panel1.Enabled = isendable;
            //panel2.Enabled = isendable;
            //trackItemUI1.Enabled = isendable;
            //DS1.Enabled = isendable;
        }
        private void updateStateUI()
        {

            btnSoftwareReady.BackColor = (MACHINE.PLCIO.SoftwareReady ? Color.Red : Color.FromArgb(192, 255, 192));
            if (m_LineScanProcess.IsOn)
                lblState.Text = ToChangeLanguage("执行-线扫测试中") + m_LineScanProcess.ID.ToString();
            else
                lblState.Text = ToChangeLanguage("等待");

            if (MACHINE.PLCIO.SoftwareReady)
            {
                if (MACHINE.PLCIO.LineScanStart)
                {
                    if (!m_plcLineStartOld)
                    {
                        m_plcLineStartOld = true;

                        CommonLogClass.Instance.LogMessage("接收到plc启动信号 ", Color.Black);
                        if (!m_LineScanProcess.IsOn)
                        {
                            Traveller106.INI.Instance.HistoryDataPath = Traveller106.Universal.HISTORY + "\\" + JzTimes.DateSerialString;
                            Traveller106.INI.Instance.HistoryDataBarcode = JzTimes.DateTimeSerialString;

                            switch (MACHINE.PLCIO.ADR_LineScannWhatFor)
                            {
                                case 1:
                                    m_LineScanProcess.Start("nWhatFor1");
                                    break;
                                default:
                                    m_LineScanProcess.Start();
                                    break;
                            }
                        }
                        else
                        {
                            CommonLogClass.Instance.LogMessage("测试中，PLC重复启动 ", Color.Black);
                        }
                    }
                }
                else
                {
                    m_plcLineStartOld = false;
                }
            }


            //bool iAlarmsCommon = MACHINE.PLCIO.IsAlarmsCommon;
            //bool iAlarmsSerious = MACHINE.PLCIO.IsAlarmsSerious;

            ////int iAlarmsCommon = MACHINE.PLCIO.IntAlarmsCommon;
            ////int iAlarmsSerious = MACHINE.PLCIO.IntAlarmsSerious;
            //if (iAlarmsCommon || iAlarmsSerious)
            //{
            //    lblState.Text = $"报警中 [{MACHINE.PLCIO.GetAlmValue}] [{iAlarmsCommon}]" + $" [{iAlarmsSerious}]";
            //    if (MACHINE.IsClearAlarmCache)
            //        return;
            //    if (iAlarmsCommon)
            //    {
            //        //CommonLogClass.Instance.LogMessage($"常规报警[{MACHINE.PLCIO.IntAlarmsCommon}] ", Color.Red);
            //        SetCommonAlarms();
            //    }
            //    if (iAlarmsSerious)
            //    {
            //        //CommonLogClass.Instance.LogMessage($"严重报警[{MACHINE.PLCIO.IntAlarmsSerious}] ", Color.Red);
            //        SetSeriousAlarms1();
            //    }
            //}
            ////if (iAlarmsCommon != 0 || iAlarmsSerious != 0)
            ////{
            ////    lblState.Text = $"报警中 [{iAlarmsCommon}]" + $" [{iAlarmsSerious}]";
            ////    if (MACHINE.IsClearAlarmCache)
            ////        return;
            ////    if (iAlarmsCommon != 0)
            ////    {
            ////        //CommonLogClass.Instance.LogMessage($"常规报警[{MACHINE.PLCIO.IntAlarmsCommon}] ", Color.Red);
            ////        SetCommonAlarms();
            ////    }
            ////    if (iAlarmsSerious != 0)
            ////    {
            ////        //CommonLogClass.Instance.LogMessage($"严重报警[{MACHINE.PLCIO.IntAlarmsSerious}] ", Color.Red);
            ////        SetSeriousAlarms1();
            ////    }
            ////}
            //else if (MACHINE.PLCIO.ADR_ISPAUSE)
            //    lblState.Text = "设备暂停中  ";
            //else if (m_LineScanProcess.IsOn)
            //    lblState.Text = "执行-线扫测试中  " + m_LineScanProcess.ID.ToString();
            //else if (m_resetprocess.IsOn)
            //    lblState.Text = "复位中 " + m_resetprocess.ID.ToString();
            //else if (m_MainProcess.IsOn)
            //    lblState.Text = "跑线中 " + m_MainProcess.ID.ToString();
            //else
            //    lblState.Text = "待机";

            //if (Traveller106.Universal.IsNoUseIO)
            //{
            //    lblState.Text = "模擬運行";
            //    lblState.BackColor = Color.Red;
            //}
            //else if (MACHINE.PLCIO.ADR_ISEMC)
            //{
            //    lblState.Text = "急停中";
            //    lblState.BackColor = Color.Red;
            //}
            ////else if (MACHINE.PLCIO.ADR_ISDOOR)
            ////{
            ////    //lblState.Text = "门被打开";
            ////    //lblState.BackColor = Color.Red;
            ////}
            //else
            //{
            //    lblState.BackColor = Color.Black;
            //}

            //lblState.Text = LanguageExClass.Instance.ToTraditionalChinese(lblState.Text);
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

        public delegate void ChangeStateHandler(MainS1State status,object tag=null);
        public event ChangeStateHandler OnChangeState;
        protected void FireChangeState(MainS1State status, object tag = null)
        {
            if (OnChangeState != null)
            {
                OnChangeState(status, tag);
            }
        }

        private string ToChangeLanguage(string eText)
        {
            string retStr = eText;
            retStr = LanguageExClass.Instance.GetLanguageText(eText);
            return retStr;
        }
        public void SaveImageWithQuality(Bitmap bmpinput, string outputImagePath, long quality)
        {
            using (Image image = bmpinput)
            {
                // 设置压缩参数
                EncoderParameters encoderParameters = new EncoderParameters(1);
                EncoderParameter encoderParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                encoderParameters.Param[0] = encoderParameter;

                // 获取图像编码信息
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

                // 保存图片，应用压缩参数
                image.Save(outputImagePath, jpgEncoder, encoderParameters);
            }
        }
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        #region AUTO_LAYOUT
        private void MainX1UI_SizeChanged(object sender, EventArgs e)
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

            var rcc = ClientRectangle;
            int pad = 3;

            int w = tabControl1.Width;
            int h = tabControl1.Height;

            tabControl2.Width = rcc.Width - pad * 2;
            tabControl2.Height = rcc.Height - pad * 3 - h;
            tabControl2.Location = new Point(pad, pad);

            groupBox1.Location = new Point(pad, tabControl2.Height + pad);
            groupBox1.Width = rcc.Width - pad * 3 - w;
            groupBox1.Height = h;

            tabControl1.Location = new Point(groupBox1.Width + pad, tabControl2.Height + pad);

#endif
        }
        #endregion
    }
}
