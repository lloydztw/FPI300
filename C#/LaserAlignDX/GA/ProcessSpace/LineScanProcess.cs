using JetEazy.BasicSpace;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using NeedleX.ProcessSpace;
using System;
using System.Drawing;
using System.Text;
using Traveller106;

namespace TravellerMINIX6.ProcessSpace
{
    public class LineScanProcess : BaseProcess
    {
        #region ACCESS_TO_OTHER_PROCESSES

        bool m_IsPass = false;

        System.Diagnostics.Stopwatch m_Stopwatch = new System.Diagnostics.Stopwatch();

        //List<AnalyzeClass> m_AutoAssignClassesTmp = new List<AnalyzeClass>();
        //int m_CollectDataIndex = 0;//收集数据的编号
        //Bitmap m_bmpCacheOrg = new Bitmap(1, 1);

        bool isGetImageReset = false;
        bool IsGetImageResetOld = false;

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

        public override void Tick()
        {
            var Process = this;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        FireMessage(new ProcessEventArgs("Record.Start"));

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
                                _LOG($"{ToChangeLanguage("通知plc采集开始")}", Color.Black);
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
                                if (IScanCam.IsGrapImageOK || IsNoUseIO)
                                {
                                    if (IsNoUseIO)
                                    {

                                    }
                                    else
                                    {
                                        //using (Bitmap bitmap = IScanCam.GetFreeImageBitmap().ToBitmap())
                                        //{
                                        //    pRun.cMvdInput = EzMvdImageConvertor.BitmapToCMvdImage(bitmap);
                                        //    using (var dummy = new Bitmap(1, 1))
                                        //    {
                                        //        //LETIAN: FireLiveImaging 必須由 caller 負責 bitmap 的 life-cycle
                                        //        FireLiveImaging(dummy);
                                        //    }
                                        //}

                                        Bitmap bitmap = IScanCam.GetFreeImageBitmap().ToBitmap();
                                        pRun.LineScanCamImageHolder.TakeOver(bitmap);
                                        FireLiveImaging(bitmap);
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

                                    m_IsPass = false;
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

                            }
                            else if (m_Stopwatch.ElapsedMilliseconds >= INI.Instance.GetImageDelayTime * 1000)
                            {
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
                            }
                        }
                        break;
                    case 10210:
                        if (Process.IsTimeup)
                        {
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

                            pRun.StripId = StripID;
                            pRun.LotId = LotID;

                            _LOG($"StripID:{pRun.StripId}", Color.Black);
                            _LOG($"LotID:{pRun.LotId}", Color.Black);

                            pRun.FileBarcodeStr = JzTimes.DateTimeSerialString;
                            pRun.Run();

                            Process.NextDuriation = 100;
                            Process.ID = 30;
                        }
                        break;
                    case 30:
                        if (Process.IsTimeup)
                        {
                            bool ret = !pRun.Running;
                            if (ret)
                            {
                                Process.Stop();
                                m_IsPass = pRun.IsPass;

                                #region 发送数据到plc

                                //这里待添加发送数据结果

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

                                #endregion

                                MACHINEx3.PLCIO.iScanResult = 1;
                                _LOG($"{ToChangeLanguage("发送结果为")}{(m_IsPass ? "PASS" : "FAIL")}", Color.Red);
                                FireMessage(new ProcessEventArgs("Show.X", $"{(pRun.ElapsedTime * 1.0 / 1000).ToString("0.0")} s"));
                            }
                        }
                        break;
                }
            }
        }

        #region 通讯流程

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

        #endregion
    }
}
