using Eazy_Project_III;
using JetEazy.BasicSpace;
using JetEazy.Interface;
using LaserAlignDX.ControlSpace.MachineSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LaserAlignDX.RunSpace;
using System;
using System.Drawing;
using System.Threading;
using Traveller106;
using TravellerMINIX6.OPSpace;
using VsCommon.ControlSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace NeedleX.ProcessSpace
{
    /// <summary>
    /// 中光電 station III processes 共用之 abstract class <br/>
    /// (1) 只與 station III 設備 (model) 相依 <br/>
    /// (2) 抽離所有 GUI 元件 <br/>
    /// (3) CommonLogClass 也有 GUI 相依的部分, 將來也要抽離. <br/>
    /// @LETIAN: 20220619 creation
    /// </summary>
    public abstract class BaseProcess : AbsProcess
    {
        public BaseProcess()
        {
            // 300 ms
            // NextDurtimeTmp = 300;
            _initPlcEventHandlers();
            _initChannelBarcode();
        }

        #region COMMON_ACCESS_TO_THE_GLOBAL_COMPONENTS
        
        protected bool IsNoUseIO
        {
            get { return Universal.IsNoUseIO; }
        }

        //ICam ICamForCali
        //{
        //    get { return Universal.CAMERAS[0]; }
        //}
        //ICam ICamForBlackBox
        //{
        //    get { return Universal.CAMERAS[1]; }
        //}

        protected ClientSocket X6_HANDLE_CLIENT
        {
            get { return Universal.X6_HANDLE_CLIENT; }
        }

        protected IxLineScanCam IScanCam
        {
            get { return Universal.IxLineScan; }
        }

        #region NOT_USED
        //protected RecipeMiniX6Class myRecipe
        //{
        //    get { return RecipeMiniX6Class.Instance; }
        //}
        //protected ProcessRunClass pRun
        //{
        //    get { return ProcessRunClass.Instance; }
        //}
        #endregion

        /// <summary>
        /// 主要的 Process
        /// </summary>
        protected ProcessRunFPIClass pRun
        {
            get { return ProcessRunFPIClass.Instance; }
        }

        //protected RecipeMainX2Class xRecipe
        //{
        //    get { return RecipeMainX2Class.Instance; }
        //}
        protected RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        protected ICam GetCamera(int camID)
        {
            return Universal.CAMERAS[camID];
        }
        protected IAxis GetAxis(int axisID)
        {
            return ((MiniX6MachineClass)MACHINECollection.MACHINE).PLCMOTIONCollection[axisID];
        }
        protected MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }
        protected MainX1MachineClass MACHINE
        {
            get { return (MainX1MachineClass)Universal.MACHINECollection.MACHINE; }
        }
        protected MainX2MachineClass MACHINEx2
        {
            get { return (MainX2MachineClass)Universal.MACHINECollection.MACHINE; }
        }
        protected MainFPIX3MachineClass MACHINEx3
        {
            get { return (MainFPIX3MachineClass)Universal.MACHINECollection.MACHINE; }
        }
        //protected MiniX6MachineClass MACHINE
        //{
        //    get { return (MiniX6MachineClass)Universal.MACHINECollection.MACHINE; }
        //}
        protected ChannelBarcodeClass BarcodeClass
        {
            get { return Universal.ChannelBarcode[(int)TrackArea.TrackINSPECT]; }
        }
        protected string StripID
        {
            get { return MACHINEx3.PLCIO.sStripID; }
        }
        protected string LotID
        {
            get { return MACHINEx3.PLCIO.sLotID; }
        }

        //protected VsLight LightControl
        //{
        //    get { return MACHINE.LightCollection[0]; }
        //}
        protected void LightValue(int eVal, int eChNum = 1)
        {
            foreach (var machine in MACHINEx3.LightCollection)
            {
                machine.ChNum = eChNum;
                machine.CstLightValue = eVal;
            }
        }
        protected void LightOnOff(bool eOn, int eChnum = 1)
        {
            foreach (var machine in MACHINEx3.LightCollection)
            {
                machine.ChNum = eChnum;
                machine.LightONOFF(eOn);
            }
        }
        #endregion

        #region LEGACY_CODE
#if (false)
        protected CMvdImage BitmapToCMvdImage(Bitmap bmpInputImg)
        {
            CMvdImage cMvdImage = new CMvdImage();
            System.Drawing.Imaging.PixelFormat bitPixelFormat = bmpInputImg.PixelFormat;
            BitmapData bmData = bmpInputImg.LockBits(new Rectangle(0, 0, bmpInputImg.Width, bmpInputImg.Height), ImageLockMode.ReadOnly, bitPixelFormat);//锁定

            if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex++];
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08, stImageData);
            }
            else if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width * 3;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height * 3;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 2];
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 1];
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex];
                        bitmapIndex += 3;
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width * 3;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_RGB_RGB24_C3, stImageData);
            }
            else if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width * 4;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        // 获取32bpp像素值
                        byte b = _BitImageBufferBytes[bitmapIndex];
                        byte g = _BitImageBufferBytes[bitmapIndex + 1];
                        byte r = _BitImageBufferBytes[bitmapIndex + 2];
                        byte a = _BitImageBufferBytes[bitmapIndex + 3];
                        bitmapIndex += 4;
                        // 转换为灰度值（8bpp）
                        byte gray = (byte)((r * 0.299 + g * 0.587 + b * 0.114) * (a / 255.0));

                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = gray;// _BitImageBufferBytes[bitmapIndex++];
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08, stImageData);
            }
            else
            {
                cMvdImage.InitImage((uint)bmpInputImg.Width, (uint)bmpInputImg.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            }
            bmpInputImg.UnlockBits(bmData);  // 解除锁定
            return cMvdImage;
        }
#endif
        #endregion

        #region COMMON_DATA_FOR_STATION_3
        // 為了相容以前的舊 Tick 內容
        protected int NextDurtimeTmp
        {
            get { return _defaultDuration; }
            set { _defaultDuration = value; }
        }
        protected void InitDefaultDelay()
        {
            //_defaultDuration = RecipeCHClass.Instance.ProcessDelay;
            _LOG("程序預設 Delay(ms)", _defaultDuration);
        }
        public override void Start(params object[] args)
        {
            //@LETIAN 20221026 啟用
            InitDefaultDelay();
            base.Start(args);
        }
        #endregion

        #region COMMON_MACHINE_FUCTIONS_FOR_STATION_3
        protected void SetNormalLight()
        {
            //MACHINE.PLCIO.ADR_RED = false;
            //MACHINE.PLCIO.ADR_YELLOW = true;
            //MACHINE.PLCIO.ADR_GREEN = false;
        }
        protected void SetAbnormalLight()
        {
            //MACHINE.PLCIO.ADR_RED = true;
            //MACHINE.PLCIO.ADR_YELLOW = false;
            //MACHINE.PLCIO.ADR_GREEN = false;
        }
        protected void SetRunningLight()
        {
            //MACHINE.PLCIO.ADR_RED = false;
            //MACHINE.PLCIO.ADR_YELLOW = false;
            //MACHINE.PLCIO.ADR_GREEN = true;
        }
        #endregion

        #region PLC_ON_SCANNED_EVENT_HANDLER

        #region PRIVATE_MEMBERS
        private ManualResetEvent m_plcScanEv = new ManualResetEvent(false);
        private int m_plcScanCount = 0;
        private void _initPlcEventHandlers()
        {
            if (MACHINECollection.MACHINE.PLCCollection == null)
                return;
            var plc = MACHINECollection.MACHINE.PLCCollection[0];
            plc.OnScanned += new EventHandler((sender, e) =>
            {
                OnPlcScanned(sender, e);
            });
        }
        private string m_barcodetemp = string.Empty;
        public string BarcodeStr
        {
            get { return m_barcodetemp; }
            set { m_barcodetemp = value; }
        }
        private void _initChannelBarcode()
        {
            //BarcodeClass.OnChangeState += BarcodeClass_OnChangeState;
        }


        #endregion
        private void BarcodeClass_OnChangeState(string statusstr)
        {
            string[] vs = statusstr.Split('$');
            switch (vs[0])
            {
                case "1":
                    m_barcodetemp = vs[1].Trim();
                    break;
                case "2":
                    break;
            }
        }
        protected virtual void OnPlcScanned(object sender, EventArgs e)
        {
            m_plcScanEv.Set();
            m_plcScanCount++;
        }

        /// <summary>
        /// 清除 Scanned 標記 
        /// </summary>
        protected override void InvalidatePlcScanned()
        {
            m_plcScanEv.Reset();
            m_plcScanCount = 0;
        }

        // <summary>
        // PLC 是否已經 有效 scanned 更新
        // </summary>
        protected bool IsValidPlcScanned(int validScannedCount = 0)
        {
            bool ok = WaitForPlcScanned(0);
            if (ok)
            {
                if (validScannedCount > 1)
                    ok = (m_plcScanCount > validScannedCount);
            }
            return ok;
        }

        /// <summary>
        /// 等待 PLC 有效 scanned 更新
        /// </summary>
        /// <param name="waitTime">ms</param>
        protected bool WaitForPlcScanned(int waitTime)
        {
            bool ok = m_plcScanEv.WaitOne(waitTime);
            return ok;
        }

        /// <summary>
        /// 放棄等待 PLC 更新 <br/>
        /// @LETIAN: 20221022 搭配 WaitForPlcScanned 使用
        /// </summary>
        protected void AbortWaitPlcScanned()
        {
            m_plcScanCount = 1000;
            m_plcScanEv.Set();
        }
        #endregion

        #region 通讯流程

        //protected void ResultStart()
        //{
        //    m_DLResultOK.Start();
        //}
        //protected void GetImageStart()
        //{
        //    m_DLGetImageOK.Start();
        //}

        //public bool IsPass
        //{
        //    get { return m_IsPass; }
        //    set { m_IsPass = value; }
        //}
        //bool m_IsPass = false;

        //bool isGetImageReset = false;
        //bool IsGetImageResetOld = false;

        //public void TcpRun()
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

        //public ProcessClass m_DLGetImageOK = new ProcessClass();
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
        //public ProcessClass m_DLResultOK = new ProcessClass();
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

        #region COMMON_LOG_FUNCTIONS

        /// <summary>
        /// Generic LOG (dual) <br/>
        /// 指定紅色 會調用 NLog.Warning 其他則調用 NLog.Debug <br/>
        /// (將來有待抽離 GUI 之部分) <br/>
        /// @LETIAN: 202206
        /// </summary>
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
            sb.Append(Name);
            sb.Append(", ");
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
            msg = Name + ", " + msg;
            //GdxGlobal.LOG.Log(msg, args);
        }

        /// <summary>
        /// Generic LOG (dual) <br/>
        /// 會額外調用 NLog.Warning <br/>
        /// @LETIAN: 202206
        /// </summary>
        protected void _LOG(Exception ex, string msg)
        {
#if (false)
            msg = Name + ", " + msg;
            CommonLogClass.Instance.LogMessage(msg, Color.Red);
            GdxGlobal.LOG.Warn(ex, msg);
#endif
            msg = Name + ", " + msg;
            //GdxGlobal.LOG.Log(ex, msg);
        }

        protected string ToChangeLanguage(string eText)
        {
            string retStr = eText;
            retStr = LanguageExClass.Instance.GetLanguageText(eText);
            return retStr;
        }

        #endregion
    }
}
