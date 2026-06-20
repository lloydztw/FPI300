#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-04-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using EzCamera.Driver.Base;
using EzCamera.Driver.Utils;
using EzCamera.Interface;
using JetEazy.OpenCV;
using MvCamCtrl.NET;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

// 
// AcquistionMode:
//      single,
//      multi,
//      continuous
// 
// TriggerMode: 
//      On
//      Off
//
// TriggerSource:
//      line0 ~ line3
//      Counter0
//      Software
//      FrequencyConverter
//

namespace EzCamera.Driver.Hikvision
{
    /// <summary>
    /// 海康威視
    /// </summary>
    public partial class HikvCameraL : EzAbstractCamera
    {
        #region DEFAULT
        public static EzCameraDeviceInfo DefaultDeviceInfo(int camID)
        {
            return new EzCameraDeviceInfo("MV", "海康威視", index: camID);
        }
        #endregion

        internal HikvCameraL(IEzDeviceInfo info)
            : base(info)
        {
            _mvccDeviceInfo = (MyCamera.MV_CC_DEVICE_INFO)info.Tag;
            _myCamera = new MyCamera();

            if (null == _myCamera)
            {
                //>>> handleError(new HikvisionError(-400, $"無法建構 MyCamera [{DeviceInfo.Index}]", $"{DeviceInfo}"));
                throw new Exception(_ERR("無法建構 MyCamera"));
            }

            initCallbacks();
        }

        #region PROTECTED_OVERRIDES
        protected override EzImageInfo drvInit()
        {
            int ret = OpenDevice();
            if (ret != MyCamera.MV_OK)
                return null;

            ret = InitDevice();
            if (ret != MyCamera.MV_OK)
            {
                CloseDevice();
                return null;
            }

            // 可拍一張, 取出圖檔資訊
            getUint("Width", ref _mvccWidth);
            getUint("Height", ref _mvccHeight);
            getEnumValue("PixelFormat", out uint pixelFormat);
            System.Diagnostics.Debug.WriteLine($"PixelFormat = {pixelFormat}");
            
            int width = (int)_mvccWidth.nCurValue;
            int height = (int)_mvccHeight.nCurValue;
            var imgInfo = new EzImageInfo()
            {
                Width = width,
                Height = height,
                BitsPerPixel = 24
            };

            StartGrabbing();

            return imgInfo;
        }
        protected override void drvDispose()
        {
            CloseDevice();
        }
        protected override Bitmap drvCaptureImage(object args = null)
        {
            //if (_videoCapture == null)
            //    return null;

            //bool ok;

            //Bitmap buf = new Bitmap(_orgImgInfo.Width, _orgImgInfo.Height, _orgImgInfo.PixelFormat);

            ////將 buf (C# Bitmap) 橋接到 OpenCV 的 Image (Mat)
            //using (var bridge = new QxImageBridge(buf))
            //{
            //    // _videoCapture 會把影像 抓到 Image (Mat) 內
            //    ok = _videoCapture.Read(bridge.Image);
            //}

            //if (!ok)
            //{
            //    buf?.Dispose();
            //    return null;
            //}

            //return buf;
            return null;
        }
        #endregion

        #region CAMERA_PROPERTIES
        public override void GetExposureRange(out double min, out double max)
        {
            min = _expo.fMin; 
            max = _expo.fMax;
        }
        public override void GetHardwareGainRange(out double min, out double max)
        {
            min = _gain.fMin;
            max = _gain.fMax;
        }
        public override double ExposureTime
        {
            get
            {
                //base.ExposureTime;
                return _expo.fCurValue;
            }
            set
            {
                //base.ExposureTime = value;
                setFloat("ExposureTime", (float)value);
                getFloat("ExposureTime", ref _expo);
            }
        }
        public override double HardwareGain
        {
            get
            {
                //base.HardwareGain;
                return _gain.fCurValue;
            }
            set
            {
                //base.HardwareGain = value;
                setFloat("Gain", (float)value);
                getFloat("Gain", ref _gain);
            }
        }
        #endregion
    }

    partial class HikvCameraL
    { 
        #region PRIVATE_HIK_核心成員
        private MyCamera _myCamera;
        private MyCamera.MV_CC_DEVICE_INFO _mvccDeviceInfo;
        private MyCamera.cbOutputExdelegate _imageCallback;
        private MyCamera.cbExceptiondelegate _exceptionCallback;
        private object _sync = new object();
        #endregion

        #region PRIVATE_HIK_狀態變數
        private bool _isOpen = false;
        private bool _isGrabbing = false;
        #endregion

        #region CACHE_DATA
        private MyCamera.MV_CAM_ACQUISITION_MODE _acquisitionMode;
        private MyCamera.MV_CAM_TRIGGER_SOURCE _triggerSource;
        private MyCamera.MV_CAM_TRIGGER_MODE _triggerMode;
        private MyCamera.MVCC_INTVALUE _mvccWidth = new MyCamera.MVCC_INTVALUE();
        private MyCamera.MVCC_INTVALUE _mvccHeight = new MyCamera.MVCC_INTVALUE();
        private MyCamera.MVCC_FLOATVALUE _expo = new MyCamera.MVCC_FLOATVALUE();
        private MyCamera.MVCC_FLOATVALUE _gain = new MyCamera.MVCC_FLOATVALUE();
        //private MyCamera.MVCC_FLOATVALUE _fps = new MyCamera.MVCC_FLOATVALUE();
        #endregion

        #region PRIVATE_HIV_OTHER_DATA
        MyCamera.MV_FRAME_OUT_INFO_EX _mvFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
#if (OPT_RESERVED)
        /// <summary>
        /// 相机帧数数组
        /// </summary>
        private int m_nFrames;
        /// <summary>
        /// 保存图像的缓存大小
        /// </summary>
        private UInt32 m_nBufSizeForSaveImage = 0;
        /// <summary>
        /// 用于保存图像的缓存
        /// </summary>
        private IntPtr m_pBufForSaveImage = IntPtr.Zero;
        /// <summary>
        /// 监控每个相机是否要保存图像
        /// </summary>
        private bool m_bSaveImg = false;
        /// <summary>
        /// 图像保存路径
        /// </summary>
        private string saveBmpFilePath = "C:\\";
        /// <summary>
        /// 图像保存名称
        /// </summary>
        private string saveBmpName = "temp.bmp";
        /// <summary>
        /// 显示句柄: 硬體直接顯示到視窗的 Display Handle (HDC)
        /// </summary>
        private IntPtr m_hDisplayHandle;
#endif
#if (OPT_RESERVED)
        /// <summary>
        /// 用于从驱动获取图像的缓存大小
        /// </summary>
        private UInt32 m_nBufSizeForDriver = 0;
        /// <summary>
        /// 用于从驱动获取图像的缓存
        /// </summary>
        private IntPtr m_BufForDriver;
#endif
        #endregion

        #region ERROR_HANDLING_FUNCTIONS
        void ShowErrorMsg(string msg, int err)
        {
        }
        void handleError(string msg, int err)
        {

        }
        void handleError(EzCameraError err)
        {
            fireErrorEvent(err);
        }
        void _ERR(int err, string msg)
        {

        }
        void _ERR(string msg, int err)
        {

        }
        string _ERR(string msg)
        {
            return $"海康[{DeviceInfo}] {msg}";
        }
        #endregion


        private int OpenDevice(int delay = 0)
        {
            //-----------------------------------------------------------------
            // MV_CC_DEVICE_INFO 
            // 已經於 Factory Class 拿到, 並存入
            // _mvccDeviceInfo
            //-----------------------------------------------------------------

            int ret = MyCamera.MV_OK;

            if (!_isOpen)
            {
                //(1) Create Device
                ret = _myCamera.MV_CC_CreateDevice_NET(ref _mvccDeviceInfo);
                if (MyCamera.MV_OK != ret)
                {
                    _ERR(ret, "錯誤於 MV_CC_CreateDevice_NET");
                    return ret;
                }

                if (delay > 0)
                    Thread.Sleep(delay);

                //(2) Open Device
                ret = _myCamera.MV_CC_OpenDevice_NET();
                if (MyCamera.MV_OK != ret)
                {
                    if (delay > 0)
                        Thread.Sleep(delay);

                    // 強制 Destroy Device
                    _myCamera.MV_CC_DestroyDevice_NET();
                    _ERR(ret, "錯誤於 MV_CC_OpenDevice_NET");
                    return ret;
                }

                initCallbacks();
                _isOpen = true;
            }

            return ret;
        }
        private int InitDevice(bool reOpen = false)
        {
            int ret = MyCamera.MV_OK;

            if (!_isOpen)
                return ret;

            // Gaara: 載入 FeatureFile
            loadFeatureFile(null);

            // 探测网络最佳包大小 (for GigE)
            detectOptimalPackageSize();

            // 註冊 Callbacks
            registerCallbacks();

            // 初始態
            // AquisitionMode: 連續
            // TriggerSource:  Software
            // TriggerMode:    OFF (free-run)
            if (!reOpen)
            {
                _acquisitionMode = MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS;
                _triggerSource = MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE;
                _triggerMode = MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF;
            }
            setAcquisitionMode(_acquisitionMode);
            setTriggerSource(_triggerSource);
            setTriggerMode(_triggerMode);

            #region CAMERA_PROPERTIES
            if (reOpen)
                updateCameraProperties(true);
            else
                updateCameraProperties(false);
            #endregion


            #region OTHERS
            //// From MVS Setting
            //MyCamera.MVCC_ENUMVALUE mVCC_ENUMVALUE = new MyCamera.MVCC_ENUMVALUE();
            //m_MyCamera.MV_CC_GetEnumValue_NET("PixelFormat", ref mVCC_ENUMVALUE);

            //// 设置Enum型参数-相机图像格式
            //// 注意点1：相机图像格式设置时，只有在MV_CC_Startgrab接口调用前才能设置,取流过程中，不能修改图像格式
            //ret = m_MyCamera.MV_CC_SetEnumValue_NET("PixelFormat", (uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG8);
            //if (ret != MyCamera.MV_OK)
            //{
            //    ShowErrorMsg("Set PixelFormat fail!", ret);
            //}
            #endregion

            //StartGrabbing();

            // ch:获取参数 | en:Get parameters
            //// ch:控件操作 | en:Control operation
            //SetCtrlWhenOpen();
            return ret;
        }
        private void initCallbacks()
        {
            //設定 影像回调函数
            _imageCallback = new MyCamera.cbOutputExdelegate(callback_Image);
            //設定 相机异常函数回调
            _exceptionCallback = new MyCamera.cbExceptiondelegate(callback_Exception);
        }
        private void registerCallbacks()
        {
            // 暫時不需要 pUser 
            IntPtr pUser = IntPtr.Zero;

            //註冊 影像回調函式
            int ret = _myCamera.MV_CC_RegisterImageCallBackEx_NET(_imageCallback, pUser);
            if (ret != 0)
                _ERR(ret, "無法註冊 影像回調函式!");

            //註冊 異常回調函式
            ret = _myCamera.MV_CC_RegisterExceptionCallBack_NET(_exceptionCallback, pUser);
            if (ret != 0)
                _ERR(ret, "無法註冊 異常回調函式!");
        }
        private int loadFeatureFile(string featureFileName)
        {
            //>>> string _featureFilenamePath = m_CameraPara.CfgPath + "\\" + m_CameraPara.SerialNumber + ".mfs";
            
            if (featureFileName == null)
            {
                // Bypass
                return 0;
            }

            int ret = _myCamera.MV_CC_FeatureLoad_NET(featureFileName);

            if (MyCamera.MV_OK != ret)
            {
                // ShowErrorMsg("FeatureLoad fail!", ret);
                _ERR(ret, $"無法載入 FeatureFile {featureFileName}");
            }

            return ret;
        }
        private int detectOptimalPackageSize()
        {
            // ch: 探测网络最佳包大小(只对GigE相机有效)
            // en: Detection network optimal package size(It only works for the GigE camera)
            int ret = 0;
            if (_mvccDeviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                int nPacketSize = _myCamera.MV_CC_GetOptimalPacketSize_NET();
                if (nPacketSize > 0)
                {
                    ret = _myCamera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                    if (ret != MyCamera.MV_OK)
                    {
                        //ShowErrorMsg("Set Packet Size failed!", ret);
                        _ERR(ret, $"無法設定數據包大小 {nPacketSize}");
                    }
                }
                else
                {
                    //ShowErrorMsg("Get Packet Size failed!", nPacketSize);
                    _ERR(ret = -500, $"最佳數據包大小 {nPacketSize} 必須 > 0!");
                }
            }
            return ret;
        }
        private void updateCameraProperties(bool cacheToDevice)
        {
            // Always Disable BalanceWhiteAuto
            setEnumValue("BalanceWhiteAuto", 0);
            // Always Disable GainAuto
            setEnumValue("GainAuto", 0);

            if (cacheToDevice)
            {
                setFloat("ExposureTime", (float)_expo.fCurValue);
                setFloat("Gain", (float)_gain.fCurValue);
            }
            else
            {
                getFloat("ExposureTime", ref _expo);
                getFloat("Gain", ref _gain);
                //getUint("Width", ref _mvccWidth);
                //getUint("Height", ref _mvccHeight);
                //getFloat("ResultingFrameRate", ref _fps);
            }
        }

        private void CloseDevice(bool releaseAll = true)
        {
            if (releaseAll)
            {
                // 將 AcquisitionMode, TriggerMode 設定成最省資源的狀態?
                setAcquisitionMode(MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
                setTriggerMode(MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
            }

            if (_isGrabbing)
                StopGrabbing();

            if (releaseAll)
            {
#if (OPT_RESERVED_釋放其他資源)
                if (m_BufForDriver != IntPtr.Zero)
                {
                    Marshal.Release(m_BufForDriver);
                }

                if (m_BufForSaveImage != IntPtr.Zero)
                {
                    Marshal.Release(m_BufForSaveImage);
                }
#endif
            }

            if (_isOpen)
            {
                _myCamera.MV_CC_CloseDevice_NET();
                _myCamera.MV_CC_DestroyDevice_NET();
                _isOpen = false;
            }
        }
        private void StartGrabbing()
        {
            // ch:标志位置位true | en:Set position bit true
            _isGrabbing = true;

            //> m_hReceiveThread = new Thread(ReceiveThreadProcess);
            //> m_hReceiveThread.Start();

            _mvFrameInfo.nFrameLen = 0;//取流之前先清除帧长度
            _mvFrameInfo.enPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Undefined;

            // ch:开始采集 | en:Start Grabbing
            int ret = _myCamera.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != ret)
            {
                _isGrabbing = false;
                //m_hReceiveThread.Join();
                //ShowErrorMsg("Start Grabbing Fail!", ret);
                _ERR(ret, "錯誤於 MV_CC_StartGrabbing_NET");
                return;
            }

            // ch:控件操作 | en:Control Operation
            // SetCtrlWhenStartGrab();
        }
        private void StopGrabbing()
        {
            // ch:标志位设为false | en:Set flag bit false
            _isGrabbing = false;
            //m_hReceiveThread.Join();

            // ch:停止采集 | en:Stop Grabbing
            int ret = _myCamera.MV_CC_StopGrabbing_NET();
            if (ret != MyCamera.MV_OK)
            {
                //ShowErrorMsg("Stop Grabbing Fail!", ret);
                _ERR(ret, "錯誤於 MV_CC_StopGrabbing_NET");
            }
        }


        /// <summary>
        /// 连续采集模式
        /// </summary>
        private void setAcquisitionMode(MyCamera.MV_CAM_ACQUISITION_MODE mode)
        {
            //_myCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)mode);
            int ret = setEnumValue("AcquisitionMode", (uint)mode);
            if (ret == MyCamera.MV_OK)
                _acquisitionMode = mode;
        }
        private void acquisitionStart()
        {
            //int ret = _myCamera.MV_CC_SetCommandValue_NET("AcquisitionStart");
            setCommand("AcquisitionStart");
        }
        private void acquisitionStop()
        {
            //int ret = _myCamera.MV_CC_SetCommandValue_NET("AcquisitionStop");
            setCommand("AcquisitionStop");
        }

        /// <summary>
        /// 設定 触发模式 on/off
        /// </summary>
        private int setTriggerMode(MyCamera.MV_CAM_TRIGGER_MODE mode)
        {
            //int ret = _myCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)mode);
            int ret = setEnumValue("TriggerMode", (uint)mode);
            if (ret == MyCamera.MV_OK)
                _triggerMode = mode;
            return ret;
        }
        /// <summary>
        /// 触发源选择:
        /// 0 - Line0; 
        /// 1 - Line1; 
        /// 2 - Line2; 
        /// 3 - Line3;
        /// 4 - Counter; 
        /// 7 - Software
        /// </summary>
        private int setTriggerSource(MyCamera.MV_CAM_TRIGGER_SOURCE source)
        {
            //int ret = _myCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)source);
            int ret = setEnumValue("TriggerSource", (uint)source);
            if (ret == MyCamera.MV_OK)
                _triggerSource = source;
            return ret;
        }
        /// <summary>
        /// 下達 TriggerSoftware 指令
        /// </summary>
        /// <returns></returns>
        private int commandTriggerSoftware()
        {
            //int ret = _myCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
            int ret = setCommand("TriggerSoftware");
            return ret;
        }

#if(false)
        public Bitmap CaptureBmp(int roattion)
        {
            //if (false == m_GetImageOK)
            //{
            //    ShowErrorMsg("GetImage Error", 0);
            //    return null;
            //}
            if (false == m_bGrabbing)
            {
                ShowErrorMsg("Not Start Grabbing", 0);
                return null;
            }

            if (RemoveCustomPixelFormats(m_stFrameInfo.enPixelType))
            {
                ShowErrorMsg("Not Support!", 0);
                return null;
            }

            IntPtr pTemp = IntPtr.Zero;
            MyCamera.MvGvspPixelType enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Undefined;
            if (m_stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8 || m_stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed)
            {
                pTemp = m_BufForDriver;
                enDstPixelType = m_stFrameInfo.enPixelType;
            }
            else
            {
                UInt32 nSaveImageNeedSize = 0;
                MyCamera.MV_PIXEL_CONVERT_PARAM stConverPixelParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();

                lock (BufForDriverLock)
                {
                    if (m_stFrameInfo.nFrameLen == 0)
                    {
                        ShowErrorMsg("Save Bmp Fail!", 0);
                        return null;
                    }

                    if (IsMonoData(m_stFrameInfo.enPixelType))
                    {
                        enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8;
                        nSaveImageNeedSize = (uint)m_stFrameInfo.nWidth * m_stFrameInfo.nHeight;
                    }
                    else if (IsColorData(m_stFrameInfo.enPixelType))
                    {
                        enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed;
                        nSaveImageNeedSize = (uint)m_stFrameInfo.nWidth * m_stFrameInfo.nHeight * 3;
                    }
                    else
                    {
                        ShowErrorMsg("No such pixel type!", 0);
                        return null;
                    }

                    if (m_nBufSizeForSaveImage < nSaveImageNeedSize)
                    {
                        if (m_BufForSaveImage != IntPtr.Zero)
                        {
                            Marshal.Release(m_BufForSaveImage);
                        }
                        m_nBufSizeForSaveImage = nSaveImageNeedSize;
                        m_BufForSaveImage = Marshal.AllocHGlobal((Int32)m_nBufSizeForSaveImage);
                    }

                    stConverPixelParam.nWidth = m_stFrameInfo.nWidth;
                    stConverPixelParam.nHeight = m_stFrameInfo.nHeight;
                    stConverPixelParam.pSrcData = m_BufForDriver;
                    stConverPixelParam.nSrcDataLen = m_stFrameInfo.nFrameLen;
                    stConverPixelParam.enSrcPixelType = m_stFrameInfo.enPixelType;
                    stConverPixelParam.enDstPixelType = enDstPixelType;
                    stConverPixelParam.pDstBuffer = m_BufForSaveImage;
                    stConverPixelParam.nDstBufferSize = m_nBufSizeForSaveImage;
                    int ret = m_MyCamera.MV_CC_ConvertPixelType_NET(ref stConverPixelParam);
                    if (MyCamera.MV_OK != ret)
                    {
                        ShowErrorMsg("Convert Pixel Type Fail!", ret);
                        return null;
                    }
                    pTemp = m_BufForSaveImage;
                }
            }

            lock (BufForDriverLock)
            {
                if (enDstPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
                {
                    //************************Mono8 转 Bitmap*******************************
                    Bitmap bmp = new Bitmap(m_stFrameInfo.nWidth, m_stFrameInfo.nHeight, m_stFrameInfo.nWidth * 1, PixelFormat.Format8bppIndexed, pTemp);

                    System.Drawing.Imaging.ColorPalette cp = bmp.Palette;
                    // init palette
                    for (int i = 0; i < 256; i++)
                    {
                        cp.Entries[i] = Color.FromArgb(i, i, i);
                    }
                    // set palette back
                    bmp.Palette = cp;
                    //bmp.Save("image.bmp", ImageFormat.Bmp);

                    switch (roattion)
                    {
                        case 90:
                            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case 270:
                            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        case 180:
                            bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                    }

                    //bmp.Save("image.bmp", ImageFormat.Bmp);
                    return bmp;
                }
                else
                {
                    //*********************BGR8 转 Bitmap**************************
                    try
                    {
                        Bitmap bmp = new Bitmap(m_stFrameInfo.nWidth, m_stFrameInfo.nHeight, m_stFrameInfo.nWidth * 3, PixelFormat.Format24bppRgb, pTemp);
                        //bmp.Save("image.bmp", ImageFormat.Bmp);

                        switch (roattion)
                        {
                            case 90:
                                bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                break;
                            case 270:
                                bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                break;
                            case 180:
                                bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                break;
                        }

                        return bmp;
                    }
                    catch
                    {
                        ShowErrorMsg("Write File Fail!", 0);
                    }
                }
            }

            //ShowErrorMsg("Save Succeed!", 0);
            return null;
        }
        public Bitmap GetImageNow()
        {
            commandTriggerSoftware();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (; ; )
            {
                if (m_GetImageOK)
                {
                    if (m_bmpCurrent == null)
                        return null;
                    return m_bmpCurrent.Clone() as Bitmap;
                }
                if (watch.ElapsedMilliseconds > 1000)
                    break;
            }
            return null;
        }
#endif

#if (false)
        /// <summary>
        /// 获取相机丢失帧数
        /// </summary>
        /// <param name="cameraIndex">相机编号</param>
        /// <returns>返回相机丢失帧数数目</returns>
        public int GetLostFrame(int cameraIndex)
        {
            if (m_bGrabbing != null && m_bGrabbing[cameraIndex])
            {
                MyCamera.MV_ALL_MATCH_INFO pstInfo = new MyCamera.MV_ALL_MATCH_INFO();
                if (m_pDeviceInfo[cameraIndex].nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    MyCamera.MV_MATCH_INFO_NET_DETECT MV_NetInfo = new MyCamera.MV_MATCH_INFO_NET_DETECT();
                    pstInfo.nInfoSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(MyCamera.MV_MATCH_INFO_NET_DETECT));
                    pstInfo.nType = MyCamera.MV_MATCH_TYPE_NET_DETECT;
                    int size = Marshal.SizeOf(MV_NetInfo);
                    pstInfo.pInfo = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(MV_NetInfo, pstInfo.pInfo, false);
                    _myCamera.MV_CC_GetAllMatchInfo_NET(ref pstInfo);
                    MV_NetInfo = (MyCamera.MV_MATCH_INFO_NET_DETECT)Marshal.PtrToStructure(pstInfo.pInfo, typeof(MyCamera.MV_MATCH_INFO_NET_DETECT));
                    int count = (int)MV_NetInfo.nLostFrameCount;
                    Marshal.FreeHGlobal(pstInfo.pInfo);
                    return count;
                }
                else if (m_pDeviceInfo[cameraIndex].nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    MyCamera.MV_MATCH_INFO_USB_DETECT MV_NetInfo = new MyCamera.MV_MATCH_INFO_USB_DETECT();
                    pstInfo.nInfoSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(MyCamera.MV_MATCH_INFO_USB_DETECT));
                    pstInfo.nType = MyCamera.MV_MATCH_TYPE_USB_DETECT;
                    int size = Marshal.SizeOf(MV_NetInfo);
                    pstInfo.pInfo = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(MV_NetInfo, pstInfo.pInfo, false);
                    _myCamera.MV_CC_GetAllMatchInfo_NET(ref pstInfo);
                    MV_NetInfo = (MyCamera.MV_MATCH_INFO_USB_DETECT)Marshal.PtrToStructure(pstInfo.pInfo, typeof(MyCamera.MV_MATCH_INFO_USB_DETECT));
                    int count = (int)MV_NetInfo.nErrorFrameCount;
                    Marshal.FreeHGlobal(pstInfo.pInfo);
                    return count;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return -1;
            }
        }
        /// <summary>
        /// 设定设备中图像有效负载大小
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        public void SetPayloadSize(int cameraIndex)
        {
            MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
            int ret = _myCamera.MV_CC_GetIntValue_NET("PayloadSize", ref stParam);
            if (MyCamera.MV_OK != ret)
            {
                ShowErrorMsg("获取有效负载大小失败", ret);
                return;
            }
            UInt32 nPayloadSize = stParam.nCurValue;
            if (nPayloadSize > m_nBufSizeForDriver)
            {
                if (m_BufForDriver != IntPtr.Zero)
                {
                    Marshal.Release(m_BufForDriver);
                }
                m_nBufSizeForDriver = nPayloadSize;
                m_BufForDriver = Marshal.AllocHGlobal((Int32)m_nBufSizeForDriver);
            }
            if (m_BufForDriver == IntPtr.Zero)
            {
                return;
            }
        }
#endif

        /// <summary>
        /// 相机异常回调函数
        /// </summary>
        private void callback_Exception(uint nMsgType, IntPtr pUser)
        {
            if (nMsgType == MyCamera.MV_EXCEPTION_DEV_DISCONNECT)
            {
                ////// 停止采集
                ////_isGrabbing = false;
                ////_myCamera.MV_CC_StopGrabbing_NET();

                ////// 关闭设备
                ////_myCamera.MV_CC_CloseDevice_NET();
                ////_myCamera.MV_CC_DestroyDevice_NET();
                ////_isOpen = false;
                
                CloseDevice(releaseAll: false);

                ////// 获取选择的设备信息 
                ////IntPtr deviceInfoPtr = IntPtr.Zero;
                ////var device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(
                ////                    deviceInfoPtr,
                ////                    typeof(MyCamera.MV_CC_DEVICE_INFO));

                // 重新打開設備 
                while (true)
                {
                    // ToDO: 迴圈應該加入 Timeout 檢查
                    OpenDevice(5);
                    if (!_isOpen)
                    {
                        Thread.Sleep(5);
                        continue;
                    }

                    int ret = InitDevice(reOpen: true);
                    if (ret != MyCamera.MV_OK)
                    {
                        Thread.Sleep(5);
                        // 关闭设备
                        CloseDevice(releaseAll: false);
                        continue;
                    }

                    StartGrabbing();
                    break;
                }
            }
        }

        /// <summary>
        /// 取流回调函数 
        /// </summary>
        /// <param name="pData">图像指针</param>
        /// <param name="pFrameInfo">帧信息</param>
        /// <param name="pUser"></param>
        private void callback_Image(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
#if(false)
            int nIndex = (int)pUser;

            // 抓取的累计帧数 
            ++m_nFrames[nIndex];

            getOneBmp(pData, pFrameInfo, nIndex);

            #region 直接绑定显示图像控件句柄
            //MyCamera.MV_DISPLAY_FRAME_INFO stDisplayInfo = new MyCamera.MV_DISPLAY_FRAME_INFO();
            //stDisplayInfo.hWnd = m_hDisplayHandle[nIndex];
            //stDisplayInfo.pData = pData;
            //stDisplayInfo.nDataLen = pFrameInfo.nFrameLen;
            //stDisplayInfo.nWidth = pFrameInfo.nWidth;
            //stDisplayInfo.nHeight = pFrameInfo.nHeight;
            //stDisplayInfo.enPixelType = pFrameInfo.enPixelType;
            //m_pMyCamera[nIndex].MV_CC_DisplayOneFrame_NET(ref stDisplayInfo);
            #endregion
#endif
        }

        /// <summary>
        /// 获取一张图像
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="stFrameInfo"></param>
        private void getOneBmp(IntPtr pData, MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo, int CCDIndex)
        {
#if (true)
            lock (_sync)
            {
                if (stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
                {
                    Bitmap bmp = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth, PixelFormat.Format8bppIndexed, pData);
                    ColorPalette cp = bmp.Palette;
                    for (int i = 0; i < 256; i++)
                    {
                        cp.Entries[i] = Color.FromArgb(i, i, i);
                    }
                    bmp.Palette = cp;
                    base.fireAsyncLiveImageEvent(bmp);
                }
                else if (stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_HB_BGR8_Packed ||
                         stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed)
                {
                    //RGB8
                    //bmpNows[CCDIndex] = BGR2RGB(new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 3, PixelFormat.Format24bppRgb, pData));
                    Bitmap bmp = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 3, PixelFormat.Format24bppRgb, pData);
                    //using (var bridge = new QxImageBridge(bmp))
                    //{
                    //    //cv2.cvtColor(bridge.Image, bridge.Image);
                    //}
                    base.fireAsyncLiveImageEvent(bmp);
                }
                else if (stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_HB_RGBA8_Packed)
                {
                    //RGBA8
                    var bmp= new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 4, PixelFormat.Format32bppArgb, pData);
                    base.fireAsyncLiveImageEvent(bmp);
                }
            }
#endif
        }

        //-- HikVision Basic Functions -----------------------------------------------------------------------------------
        #region HikVision_Basic_Functions
        int setCommand(string command)
        {
            int ret = _myCamera.MV_CC_SetCommandValue_NET(command);
            if (MyCamera.MV_OK != ret)
                _ERR(ret, $"錯誤於 命令 {command}");
            return ret;
        }
        int setEnumValue(string propertyName, uint value)
        {
            int ret = _myCamera.MV_CC_SetEnumValue_NET(propertyName, value);
            if (ret != MyCamera.MV_OK)
                _ERR(ret, $"錯誤於 Set {propertyName} ({value})");
            return ret;
        }
        int getEnumValue(string propertyName, out uint value)
        {
            var mvccValue = new MyCamera.MVCC_ENUMVALUE();
            int ret = _myCamera.MV_CC_GetEnumValue_NET(propertyName, ref mvccValue);
            if (ret != MyCamera.MV_OK)
            {
                _ERR(ret, $"錯誤於 Get {propertyName}");
                value = 0;
            }
            else
            {
                value = mvccValue.nCurValue;
            }
            return ret;
        }

        int setUnit(string propertyName, uint value)
        {
            int ret = _myCamera.MV_CC_SetIntValue_NET(propertyName, value);
            if (ret != MyCamera.MV_OK)
                _ERR(ret, $"錯誤於 Set {propertyName} ({value})");
            return ret;
        }
        int getUint(string propertyName, out uint value)
        {
            var mvccValue = new MyCamera.MVCC_INTVALUE();
            int ret = getUint(propertyName, ref mvccValue);
            value = (ret == MyCamera.MV_OK) ? mvccValue.nCurValue : 0;
            return ret;
        }
        int getUint(string propertyName, ref MyCamera.MVCC_INTVALUE mvccValue)
        {
            int ret = _myCamera.MV_CC_GetIntValue_NET(propertyName, ref mvccValue);
            if (MyCamera.MV_OK != ret)
                _ERR(ret, $"錯誤於 Get {propertyName}");
            return ret;
        }

        int setFloat(string propertyName, float value)
        {
            int ret = _myCamera.MV_CC_SetFloatValue_NET(propertyName, value);
            if (ret != MyCamera.MV_OK)
                _ERR(ret, $"錯誤於 Set {propertyName} ({value})");
            return ret;
        }
        int getFloat(string propertyName, out float value)
        {
            var mvccValue = new MyCamera.MVCC_FLOATVALUE();
            int ret = getFloat(propertyName, ref mvccValue);
            value = (ret == MyCamera.MV_OK) ? mvccValue.fCurValue : 0f;
            return ret;
        }
        int getFloat(string propertyName, ref MyCamera.MVCC_FLOATVALUE mvccValue)
        {
            int ret = _myCamera.MV_CC_GetFloatValue_NET(propertyName, ref mvccValue);
            if (MyCamera.MV_OK != ret)
                _ERR(ret, $"錯誤於 Get {propertyName}");
            return ret;
        }
        #endregion
    }
}
