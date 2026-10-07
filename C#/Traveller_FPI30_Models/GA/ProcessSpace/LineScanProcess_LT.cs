using JetEazy.BasicSpace;
using JetEazy.Utils;
using LaserAlignDX;
using LaserAlignDX.AoiModel;
using NeedleX.ProcessSpace;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using Traveller106;
using VsCommon.ControlSpace.IOSpace;

namespace TravellerMINIX6.ProcessSpace
{
    public class LineScanProcess : BaseProcess
    {
        #region ACCESS_TO_OTHER_PROCESSES
        //bool m_IsPass = false;
        System.Diagnostics.Stopwatch m_Stopwatch = new System.Diagnostics.Stopwatch();
        //List<AnalyzeClass> m_AutoAssignClassesTmp = new List<AnalyzeClass>();
        //int m_CollectDataIndex = 0;//收集数据的编号
        //Bitmap m_bmpCacheOrg = new Bitmap(1, 1);
        //bool isGetImageReset = false;
        //bool IsGetImageResetOld = false;
        #endregion

        #region GLOBAL_MESS
        IPlcIoFPIX3 _plcIO => MACHINEx3.PLCIO;
        #endregion

        #region RUNTIME_DATA
        bool _isUserOneshotTrigger;
        #endregion

        #region SINGLETON
        static LineScanProcess _singleton = null;
        private LineScanProcess()
        {
            //Task.Run(() =>
            //{
            //    TcpRun();
            //});
        }
        #endregion


        public static LineScanProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new LineScanProcess();
                return _singleton;
            }
        }

        public override void Start(params object[] args)
        {
            string optionStr = args.Length > 0 ? args[0]?.ToString() : null;
            if (optionStr == "USER_TRIGGER" || this.RelateString == "Snap")
                _isUserOneshotTrigger = true;

            base.Start(args);
        }

        public override void Tick()
        {
            var Process = this;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        FireStarted(new ProcessEventArgs("Record.Start"));

                        switch (Process.RelateString)
                        {
                            case "Snap":
                                break;
                            default:
                                break;
                        }

                        _LOG($"{ToChangeLanguage("等待plc通知pc信号")}", Color.Black);
                        //CommonLogClass.Instance.LogMessage("等待plc通知pc信号... ", Color.Black);
                        Process.NextDuriation = 0;
                        Process.ID = 501;

                        break;

                    #region 控制灯光
                    case 501:
                        if (Process.IsTimeup)
                        {
                            //if (MACHINE.PLCIO.LineScanStart)
                            {
                                int _lightValue = xRecipe.xChValue;
                                switch (xRecipe.xChNum)
                                {
                                    case 4:
                                    case 3:
                                        LightValue(_lightValue, 1);
                                        LightOnOff(true);
                                        LightValue(_lightValue, 2);
                                        LightOnOff(true);
                                        break;
                                    default:
                                        LightValue(_lightValue, xRecipe.xChNum);
                                        LightOnOff(true);
                                        break;
                                }
                                
                                _LOG($"{ToChangeLanguage("打开灯光")}[{_lightValue}]", Color.Black);
                                //CommonLogClass.Instance.LogMessage($"{ToChangeLanguage("打开灯光")}[{_lightValue}]... ", Color.Black);

                                Process.NextDuriation = INI.Instance.LightDelayTime;
                                Process.ID = 502;
                            }
                        }
                        break;
                    case 502:
                        if (Process.IsTimeup)
                        {
                            //if (MACHINE.PLCIO.LineScanStart)
                            {
                                _LOG($"{ToChangeLanguage("打开线扫采集")}", Color.Black);
                                //CommonLogClass.Instance.LogMessage("打开线扫采集... ", Color.Black);
                                IScanCam.IsGrapImageComplete = false;
                                IScanCam.IsGrapImageOK = false;
                                if (!Traveller106.Universal.IsNoUseCCD)
                                {
                                    IScanCam.StartGrab();
                                    _LOG($"{ToChangeLanguage("StartGrab")}", Color.Black);
                                    //CommonLogClass.Instance.LogMessage($"StartGrab {IScanCam.OperateShowMessage}", Color.Black);
                                }

                                m_Stopwatch.Restart();

                                Process.NextDuriation = 0;
                                Process.ID = 503;
                            }
                        }
                        break;
                    case 503:
                        if (Process.IsTimeup)
                        {
                            //if (MACHINE.PLCIO.LineScanStart)
                            {
                                MACHINEx3.PLCIO.bScanReady = true;
                                _LOG($"{ToChangeLanguage("通知 PLC 采集开始")}", Color.Black);
                                //CommonLogClass.Instance.LogMessage("通知plc采集开始... ", Color.Black);

                                Process.NextDuriation = 0;
                                Process.ID = 10;
                            }
                        }
                        break;
                    #endregion

                    case 10:
                        if (Process.IsTimeup)
                        {
                            //IScanCam.IsGrapImageComplete = false;
                            Process.NextDuriation = INI.Instance.DelayImageTime;
                            Process.ID = 10200;
                            _LOG($"{ToChangeLanguage("线扫取像延时")}{INI.Instance.DelayImageTime.ToString()}", Color.Black);
                        }
                        break;

                    case 10200:
                        if (Process.IsTimeup)
                        {
                            if (IScanCam.IsGrapImageComplete || IsNoUseIO)
                            {
#if(OPT_OLD_MESS_CODE)
                                if (IScanCam.IsGrapImageOK || IsNoUseIO)
                                {
                                    if (IsNoUseIO)
                                    {

                                    }
                                    else
                                    {
                                        // 2025-08-28 LETIAN:
                                        //  巨圖 統一由 LineScanCamImageHolder 保管其生命週期
                                        //  不再使用不安全的 cMvdInput !!!
                                        Bitmap bitmap = IScanCam.GetFreeImageBitmap()?.ToBitmap();
                                        pRun.LineScanCamImageHolder.TakeOver(bitmap, "LineScanCamera");
                                    }

                                    switch(Process.RelateString)
                                    {
                                        case "Snap":
                                            //m_DLGetImageOK.Start();
                                            _LOG($"{ToChangeLanguage("单次线扫取像完成")}", Color.Black);
                                            Process.Stop();

                                            break;
                                        default:
                                            _LOG($"{ToChangeLanguage("线扫取像完成")}", Color.Black);
                                            Process.NextDuriation = 0;
                                            Process.ID = 10210;
                                            break;
                                    }
                                }
                                else
                                {
                                    Process.Stop();
                                    _LOG($"{ToChangeLanguage("线扫取像失败")}", Color.Red);

                                    //m_IsPass = false;
                                    //m_DLResultOK.Start();
                                    //ResultStart();
                                    
                                    _LOG($"{ToChangeLanguage("发送结果为")}FAIL-1", Color.Red);
                                    
                                    //Task task = new Task(() =>
                                    //{
                                    //    m_DLResultOK.Start();
                                    //});
                                    //task.Start();

                                    MACHINEx3.PLCIO.bScanDone = true;
                                    MACHINEx3.PLCIO.iScanResult = 2;

                                    //FireMessage(new ProcessEventArgs("Record.Stop"));
                                    //Process.Stop();
                                    //CommonLogClass.Instance.LogMessage("线扫抓图失败 ", Color.Red);
                                    GC.Collect();
                                }

                                //IScanCam.IsGrapImageComplete = false;
                                //MACHINEx3.PLCIO.bScanReady = false;
                                IScanCam.IsGrapImageComplete = false;
                                _plcIO.bScanReady = false;

                                if (!Traveller106.Universal.IsNoUseCCD)
                                {
                                    IScanCam.StopGrab();
                                    _LOG($"{ToChangeLanguage("StopGrab")}", Color.Black);
                                }

                                LightOnOff(false);
                                LightOnOff(false, 2);
                                _LOG($"{ToChangeLanguage("关闭灯光")}", Color.Black);
#endif
                                bool goNext = Handle_LineScan_ImageCompleted();

                                if (goNext)
                                {
                                    Process.NextDuriation = 0;
                                    Process.ID = 10210;
                                }
                            }
                            else if (m_Stopwatch.ElapsedMilliseconds >= INI.Instance.GetImageDelayTime * 1000)
                            {
#if(OPT_OLD_MESS_CODE)
                                m_Stopwatch.Stop();

                                MACHINEx3.PLCIO.bScanDone = true;
                                MACHINEx3.PLCIO.iScanResult = 2;
                                //FireMessage(new ProcessEventArgs("Record.Stop"));
                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("线扫抓图超时 ", Color.Red);
                                GC.Collect();

                                IScanCam.IsGrapImageComplete = false;
                                MACHINEx3.PLCIO.bScanReady = false;

                                if (!Traveller106.Universal.IsNoUseCCD)
                                {
                                    IScanCam.StopGrab();
                                    _LOG($"{ToChangeLanguage("StopGrab")}", Color.Black);
                                }
                                LightOnOff(false);
                                LightOnOff(false, 2);
                                _LOG($"{ToChangeLanguage("关闭灯光")}", Color.Black);
#endif
                                Handle_LineScan_Timeout();
                                Process.Stop();
                            }
                        }
                        break;

                    case 10210:
                        if (Process.IsTimeup)
                        {
#if(OPT_OLD_MESS_CODE)
                            if (MACHINEx3.PLCIO.iScanStatus == 3)
                            {
                                pRun.xScanInspectMode = ScanInspectMode.NOTRAY;
                            }
                            else if (MACHINEx3.PLCIO.iScanStatus == 2)
                            {
                                pRun.xScanInspectMode = ScanInspectMode.QRCODE;
                                pRun.QrUsed = MACHINEx3.PLCIO.bQRUsed;
                                pRun.QrJudged = MACHINEx3.PLCIO.bQRJudgeUsed;
                            }
                            else
                            {
                                pRun.xScanInspectMode = ScanInspectMode.MEASUREAOI;
                            }

                            pRun.FileBarcodeStr = JzTimes.DateTimeSerialString;
                            pRun.StripId = StripID;
                            pRun.LotId = LotID;
                            _LOG($"StripID:{pRun.StripId}", Color.Black);
                            _LOG($"LotID:{pRun.LotId}", Color.Black);

                            pRun.Run();
#endif
                            StartAoiProcess();
                            Process.NextDuriation = 100;
                            Process.ID = 30;
                        }
                        break;

                    case 30:
                        if (Process.IsTimeup)
                        {
                            bool isFinished = !pRun.Running;
                            if (isFinished)
                            {
                                Process.Stop();
                                
                                SendAoiResultsToPLC();

                                FireCompleted(new ProcessEventArgs("Show.X", $"{(pRun.ElapsedTime * 1.0 / 1000).ToString("0.0")} s"));
                            }
                        }
                        break;
                }
            }
        }


        #region 通讯流程
#if (OPT_NOT_USED_CODE)
        //private void TcpRun()
        //{
        //    while (true)
        //    {
        //        _TcpAndIoTick();
        //        Thread.Sleep(50);
        //    }
        //}
        //private void _TcpAndIoTick()
        //{
        //    DLResultOKTick();
        //    DLGetImageOKTick();
        //    //Trigger Reset
        //    isGetImageReset = MACHINEx2.PLCIO.IsGetImageReset;

        //    if (isGetImageReset && IsGetImageResetOld != isGetImageReset)
        //    {
        //        Stop();
        //        MACHINEx2.PLCIO.Busy = false;
        //        MACHINEx2.PLCIO.GetImageOK = false;//完成信号关闭
        //        if (m_DLGetImageOK.IsOn)
        //            m_DLGetImageOK.Stop();
        //        if (m_DLResultOK.IsOn)
        //            m_DLResultOK.Stop();

        //        MACHINEx2.PLCIO.Pass = false;
        //        MACHINEx2.PLCIO.Fail = false;

        //    }
        //    IsGetImageResetOld = isGetImageReset;

        //}

        ////public ProcessClass m_DLGetImageProcess = new ProcessClass();
        ////void DLGetImageTick()
        ////{
        ////    ProcessClass Process = m_DLGetImageProcess;

        ////    if (Process.IsOn)
        ////    {
        ////        switch (Process.ID)
        ////        {
        ////            case 5:

        ////                Process.TimeUnit = TimeUnitEnum.ms;
        ////                Process.NextDuriation = INI.MAINSD_GETIMAGE_DELAYTIME;
        ////                JetEazy.LoggerClass.Instance.WriteLog($"手动抓取图像{Universal.CAMACT.ToString()}");

        ////                switch (Universal.CAMACT)
        ////                {
        ////                    case CameraActionMode.CAM_MOTOR_LINESCAN:

        ////                        m_IxLinescanCamera.IsGrapImageComplete = false;
        ////                        Process.ID = 20;

        ////                        break;
        ////                    default:

        ////                        Process.ID = 10;

        ////                        break;
        ////                }

        ////                break;

        ////            #region LINESCAN GETIMAGE PROCESS

        ////            case 20:
        ////                if (Process.IsTimeup)
        ////                {
        ////                    if (m_IxLinescanCamera.IsGrapImageComplete)
        ////                    {
        ////                        if (m_IxLinescanCamera.IsGrapImageOK)
        ////                        {
        ////                            Process.Stop();
        ////                            //using (FreeImageAPI.FreeImageBitmap bmp =
        ////                            //    new FreeImageAPI.FreeImageBitmap(m_IxLinescanCamera.ImageWidth,
        ////                            //    m_IxLinescanCamera.ImageHeight,
        ////                            //    m_IxLinescanCamera.ImageWidth,
        ////                            //    PixelFormat.Format8bppIndexed,
        ////                            //    m_IxLinescanCamera.ImagePbuffer)
        ////                            //)
        ////                            using (Bitmap bitmap = m_IxLinescanCamera.GetFreeImageBitmap().ToBitmap())
        ////                            {
        ////                                //bmp.Rotate(m_IxLinescanCamera.ImageRotate);

        ////                                if (CamActClass.Instance.StepCurrent >= CamActClass.Instance.StepCount)
        ////                                    CamActClass.Instance.ResetStepCurrent();
        ////                                //bitmap.Save("D:\\LOA\\TEST.PNG", ImageFormat.Png);
        ////                                CamActClass.Instance.SetImage(bitmap, CamActClass.Instance.StepCurrent);
        ////                                OnTriggerOP(ResultStatusEnum.SET_CURRENT_IMAGE, "ONLINE#" + CamActClass.Instance.StepCurrent.ToString());
        ////                                CamActClass.Instance.StepCurrent++;

        ////                                m_DLGetImageOK.Start();
        ////                            }
        ////                            JetEazy.LoggerClass.Instance.WriteLog($"手动抓取图像完成{Universal.CAMACT.ToString()}");

        ////                        }
        ////                        else
        ////                        {
        ////                            Process.Stop();
        ////                            //失败
        ////                            M_WARNING_FRM = new MessageForm(true, "线扫抓图失败，请检查。。。", "");
        ////                            if (DialogResult.Yes == M_WARNING_FRM.ShowDialog())
        ////                            {
        ////                            }
        ////                            M_WARNING_FRM.Close();
        ////                            M_WARNING_FRM.Dispose();
        ////                        }
        ////                    }
        ////                    else
        ////                    {
        ////                        //可以添加超时
        ////                    }
        ////                }
        ////                break;

        ////            #endregion

        ////            case 10:
        ////                if (Process.IsTimeup)
        ////                {
        ////                    Process.Stop();
        ////                    Universal.CCDCollection.GetImage();
        ////                    //Bitmap bitmap = Universal.CCDCollection.GetBMP(0, false);
        ////                    using (Bitmap bitmap = Universal.CCDCollection.GetBMP(0, false))
        ////                    {
        ////                        if (CamActClass.Instance.StepCurrent >= CamActClass.Instance.StepCount)
        ////                            CamActClass.Instance.ResetStepCurrent();
        ////                        CamActClass.Instance.SetImage(bitmap, CamActClass.Instance.StepCurrent);
        ////                        OnTriggerOP(ResultStatusEnum.SET_CURRENT_IMAGE, "ONLINE#" + CamActClass.Instance.StepCurrent.ToString());
        ////                        CamActClass.Instance.StepCurrent++;

        ////                        m_DLGetImageOK.Start();
        ////                    }
        ////                    JetEazy.LoggerClass.Instance.WriteLog($"手动抓取图像完成{Universal.CAMACT.ToString()}");


        ////                }
        ////                break;
        ////        }
        ////    }
        ////}

        //ProcessClass m_DLGetImageOK = new ProcessClass();
        //void DLGetImageOKTick()
        //{
        //    ProcessClass Process = m_DLGetImageOK;

        //    if (Process.IsOn)
        //    {
        //        switch (Process.ID)
        //        {
        //            case 5:

        //                Process.TimeUnit = TimeUnitEnum.ms;
        //                Process.NextDuriation = INI.Instance.handle_delaytime;
        //                Process.ID = 10;
        //                MACHINEx2.PLCIO.GetImageOK = true;

        //                _LOG(Process.RelateString + "Finish ON");
        //                _tcpSendCompleteOKSign(1, 0, -1);

        //                break;
        //            case 10:
        //                if (Process.IsTimeup)
        //                {
        //                    Process.Stop();
        //                    MACHINEx2.PLCIO.GetImageOK = false;
        //                    _LOG(Process.RelateString + "Finish OFF");
        //                }
        //                break;
        //        }
        //    }
        //}
        //ProcessClass m_DLResultOK = new ProcessClass();
        //void DLResultOKTick()
        //{
        //    ProcessClass Process = m_DLResultOK;

        //    if (Process.IsOn)
        //    {
        //        switch (Process.ID)
        //        {
        //            case 5:

        //                Process.TimeUnit = TimeUnitEnum.ms;
        //                Process.NextDuriation = INI.Instance.handle_delaytime;
        //                Process.ID = 10;

        //                if (MACHINEx2.PLCIO.Ready)
        //                {
        //                    MACHINEx2.PLCIO.Pass = m_IsPass;
        //                    MACHINEx2.PLCIO.Fail = !m_IsPass;
        //                }

        //                MACHINEx2.PLCIO.GetImageOK = true;
        //                _LOG(Process.RelateString + "Result Finish ON");
        //                _tcpSendCompleteOKSign(1, 0, (m_IsPass ? 0 : 1));

        //                break;

        //            case 10:
        //                if (Process.IsTimeup)
        //                {
        //                    Process.Stop();
        //                    MACHINEx2.PLCIO.Busy = false;
        //                    MACHINEx2.PLCIO.GetImageOK = false;
        //                    _LOG(Process.RelateString + "Result Finish OFF");
        //                }
        //                break;
        //        }
        //    }
        //}

        //private void _tcpSendCompleteOKSign(int eCompleteSign, int eCurrentStep, int eResult)
        //{
        //    string Str = "CompleteSign=" + eCompleteSign.ToString();
        //    Str += "CurrentStep=" + eCurrentStep.ToString();
        //    Str += "Result=" + eResult.ToString();

        //    X6_HANDLE_CLIENT.Log.Log2("tcpCmd.CMD_SENDCOMPLETESIGN" + Str);
        //    string _cmd = eCompleteSign.ToString() + "," + eCurrentStep.ToString() + "," + eResult.ToString();
        //    byte[] bytedata2 = Encoding.UTF8.GetBytes(_cmd);

        //    byte[] bytedata = new byte[32 + bytedata2.Length];
        //    bytedata[0] = 29;
        //    //bytedata[4] = 4;
        //    bytedata[8] = 0;
        //    //bytedata[32] = (iret == 0 ? (byte)1 : (byte)3);

        //    for (int i = 0; i < bytedata2.Length; i++)
        //    {
        //        bytedata[32 + i] = bytedata2[i];
        //    }

        //    int tu5x = bytedata2.Length;
        //    bytedata[4] = (byte)(tu5x & 0xFF);
        //    bytedata[5] = (byte)(tu5x >> 8 & 0xFF);
        //    bytedata[6] = (byte)(tu5x >> 16 & 0xFF);
        //    bytedata[7] = (byte)(tu5x >> 24 & 0xFF);

        //    //byte[] a2 = bytes.Skip(4).Take(4).ToArray();
        //    //Int32 aa2 = BitConverter.ToInt32(bytes5x, 0);

        //    //bytedata[4] = (byte)bytedata2.Length;//数据长度

        //    try
        //    {
        //        X6_HANDLE_CLIENT.Send(bytedata);
        //    }
        //    catch (Exception ex)
        //    {
        //        X6_HANDLE_CLIENT.Log.Log2("tcpCmd.CMD_SENDCOMPLETESIGN:Exception" + ex.Message);
        //    }

        //}
#endif
        #endregion


        /// <summary>
        /// 處理 線掃相機 影像到位
        /// </summary>
        bool Handle_LineScan_ImageCompleted()
        {
            bool GO = false;

            // --- 1. 處理取像失敗的狀況 (Guard Clause) ---
            if (!IScanCam.IsGrapImageOK && !IsNoUseIO)
            {
                _LOG($"{ToChangeLanguage("线扫取像失败")}", Color.Red);
                _LOG($"{ToChangeLanguage("发送结果为")}FAIL-1", Color.Red);

                // 停止相機、重置PLC訊號與關閉燈光
                Reset_LineScan_Status(isSuccess: false);
                return false;
            }

            // --- 2. 處理取像成功的狀況 ---
            if (!IsNoUseIO)
            {
                // 巨圖 統一由 LineScanCamImageHolder 保管生命週期
                Bitmap bitmap = IScanCam.GetFreeImageBitmap()?.ToBitmap();
                pRun.LineScanCamImageHolder.TakeOver(bitmap, "LineScanCamera");
            }

            // --- 3. 是否只是 USER_TRIGGER ---
            if (_isUserOneshotTrigger)  //(this.RelateString == "Snap")
            {
                _LOG($"{ToChangeLanguage("单次线扫取像完成")}", Color.Black);
                this.Stop();
                GO = false;
            }
            // --- 4. 繼續正常跑線流程 ---
            else
            {
                _LOG($"{ToChangeLanguage("线扫取像完成")}", Color.Black);
                //this.NextDuriation = 0;
                //this.ID = 10210;
                GO = true;
            }

            // --- 5. 停止相機、重置PLC訊號與關閉燈光 ---
            Reset_LineScan_Status(isSuccess: true);
            return GO;
        }

        /// <summary>
        /// 處理 線掃相機 逾時
        /// </summary>
        void Handle_LineScan_Timeout()
        {
            m_Stopwatch.Stop();
            CommonLogClass.Instance.LogMessage("线扫抓图超时 ", Color.Red);

            // 停止相機、重置PLC訊號與關閉燈光
            Reset_LineScan_Status(isSuccess: false);
        }

        /// <summary>
        /// 公用清理邏輯：負責停止相機、重置PLC訊號與關閉燈光
        /// </summary>
        void Reset_LineScan_Status(bool isSuccess)
        {
            // 1. PLC 狀態重置
            _plcIO.bScanReady = false;
            IScanCam.IsGrapImageComplete = false;

            if (!isSuccess)
            {
                _plcIO.bScanDone = true;
                _plcIO.iScanResult = 2; // 失敗代碼
                this.Stop();
                GC.Collect();           // 僅在失敗或超時等非常規狀況下執行
            }

            // 2. 停止相機硬體取像
            if (!Traveller106.Universal.IsNoUseCCD)
            {
                IScanCam.StopGrab();
                _LOG($"{ToChangeLanguage("StopGrab")}", Color.Black);
            }

            // 3. 關閉燈光
            LightOnOff(false);
            LightOnOff(false, 2);
            _LOG($"{ToChangeLanguage("关闭灯光")}", Color.Black);
        }

        /// <summary>
        /// 啟動 AOI 的程序
        /// </summary>
        void StartAoiProcess()
        {
            // 詢問 PLC 所要執行的 AOI 檢測項目
            int scanStatus = _plcIO.iScanStatus;

            if (scanStatus == 3)        // (MACHINEx3.PLCIO.iScanStatus == 3)
            {
                pRun.xScanInspectMode = ScanInspectMode.NOTRAY;
            }
            else if (scanStatus == 2)   //  MACHINEx3.PLCIO.iScanStatus == 2)
            {
                pRun.xScanInspectMode = ScanInspectMode.QRCODE;
                pRun.QrJudged = _plcIO.bQRJudgeUsed;
                pRun.QrUsed = _plcIO.bQRUsed;
            }
            else
            {
                pRun.xScanInspectMode = ScanInspectMode.MEASUREAOI;
            }

            pRun.FileBarcodeStr = JzTimes.DateTimeSerialString;
            pRun.StripId = StripID;
            pRun.LotId = LotID;

            _LOG($"StripID:{pRun.StripId}", Color.Black);
            _LOG($"LotID:{pRun.LotId}", Color.Black);

            pRun.Run();
        }

        /// <summary>
        /// 发送数据到 PLC
        /// </summary>
        void SendAoiResultsToPLC()
        {
            bool m_IsPass = pRun.IsPass;

            int[] ints0 = pRun.GetSingleResult();
            float[] floats0 = pRun.GetScanOffset();

            StringBuilder sb = new StringBuilder();
            foreach (var ix in ints0)
            {
                sb.Append(ix.ToString() + ",");
            }
            _LOG("SingleResult:" + sb.ToString(), Color.Black);

            StringBuilder sb1 = new StringBuilder();
            foreach (var ix in floats0)
            {
                sb1.Append(ix.ToString() + ",");
            }
            _LOG("ScanOffset:" + sb1.ToString(), Color.Black);

            switch (pRun.xScanInspectMode)
            {
                case ScanInspectMode.MEASUREAOI:

                    MACHINEx3.PLCIO.iSingleResult(ints0);
                    MACHINEx3.PLCIO.rScanOffset(floats0);

                    break;
                case ScanInspectMode.QRCODE:

                    int[] ints1 = pRun.GetQrResult();

                    StringBuilder sb2 = new StringBuilder();
                    foreach (var ix in ints1)
                    {
                        sb2.Append(ix.ToString() + ",");
                    }
                    _LOG("QrResult:" + sb2.ToString(), Color.Black);

                    MACHINEx3.PLCIO.iQRResult(ints1);

                    MACHINEx3.PLCIO.iSingleResult(ints0);
                    MACHINEx3.PLCIO.rScanOffset(floats0);

                    break;
                case ScanInspectMode.NOTRAY:

                    MACHINEx3.PLCIO.iSingleResult(ints0);

                    break;
            }

            MACHINEx3.PLCIO.bScanDone = true;
            MACHINEx3.PLCIO.iScanResult = 1;

            _LOG($"{ToChangeLanguage("发送结果为")}{(m_IsPass ? "PASS" : "FAIL")}", Color.Red);
        }
    }
}
