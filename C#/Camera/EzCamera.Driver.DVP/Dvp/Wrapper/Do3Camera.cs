#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-05-05 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using DVPCameraType;
using EzCamera.Interface;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;


namespace EzCamera.Driver.DVP
{
    /// <summary>
    /// The wrapper Class for DVP (Do3Think) Camera 
    /// 度申相機 包裝後的類 
    /// <br/> (1) 基本函式 內部自帶 error message 處理.
    /// <br/> (2) 具體的 控制流程, Callbacks, 與 FrameBuf 管理
    /// </summary>
    public partial class Do3Camera
    {
        public event EventHandler<EzLiveImageEventArgs> OnFrameCaptured;
        public event EventHandler<Do3Error> OnError;

        #region PROTECTED_DVP_核心成員
        protected uint _cameraHandle;
        protected dvpCameraInfo _deviceInfo;
        #endregion

        #region PRIVATE_MEMBERS_私有成員
        private IntPtr _hwndDirectDisplay = IntPtr.Zero;
        private DVPCamera.dvpStreamCallback _proc;
        private bool _isCreatedOpen = false;
        private bool _isGrabbing = false;
        #endregion

        //--- ERROR Handling Fuctions ------------------------------------------------------------------------------------
        #region ERROR_HANDLING
        /// <summary>
        /// 使用 Event 傳遞 Error Messages
        /// </summary>
        protected void _ERR(dvpStatus errCode, string errMsg, string description = null)
        {
            OnError?.Invoke(this, new Do3Error((int)errCode, errMsg, description));
        }
        /// <summary>
        /// 返回第一個 err
        /// </summary>
        private dvpStatus _FIRST_ERROR(params dvpStatus[] rets)
        {
            foreach (var ret in rets)
            {
                if (ret != dvpStatus.DVP_STATUS_OK)
                    return ret;
            }
            return dvpStatus.DVP_STATUS_OK;
        }
        #endregion

        //--- 事件發送函式 ------------------------------------------------------------------------------------------------
        #region EVENT_FIRERS
        /// 發送 OnFrameCaptured 事件
        /// <br/> (1) 發送者 (Sender) 負責 img 的生命週期.
        /// <br/> (2) 接受者 (EventHandler) 不要對 img 調用 Dispose().
        protected void fireFrameCapturedEvent(EzLiveImageEventArgs e)
        {
            OnFrameCaptured?.Invoke(this, e);
        }
        /// 發送 OnFrameCaptured 事件
        /// <br/> (1) 發送者 (Sender) 負責 img 的生命週期.
        /// <br/> (2) 接受者 (EventHandler) 不要對 img 調用 Dispose().
        protected void fireFrameCapturedEvent(Bitmap img)
        {
            OnFrameCaptured?.Invoke(this, new EzLiveImageEventArgs(img));
        }
        #endregion

        //--- Camera Image Info ------------------------------------------------------------------------------------------
        #region CAMERA_IMAGE_INFO
#if (false)
        public int getPixelFormat(out uint fmt)
        {
            int ret = getEnumValue("PixelFormat", out fmt);
            return ret;
        }
        public int getWidth(out uint value, out uint min, out uint max)
        {
            var mvccValue = new MyCamera.MVCC_INTVALUE();
            int ret = getUint("Width", ref mvccValue);
            if (MyCamera.MV_OK != ret)
            {
                value = 0; min = 0; max = 0;
            }
            else
            {
                value = mvccValue.nCurValue;
                min = mvccValue.nMin;
                max = mvccValue.nMax;
            }
            return ret;
        }
        public int setWidth(uint value)
        {
            return setUnit("Width", value);
        }
        public int getHeight(out uint value, out uint min, out uint max)
        {
            var mvccValue = new MyCamera.MVCC_INTVALUE();
            int ret = getUint("Height", ref mvccValue);
            if (MyCamera.MV_OK != ret)
            {
                value = 0; min = 0; max = 0;
            }
            else
            {
                value = mvccValue.nCurValue;
                min = mvccValue.nMin;
                max = mvccValue.nMax;
            }
            return ret;
        }
        public int setHeight(uint value)
        {
            return setUnit("Height", value);
        }
#endif
        #endregion

        //--- Camera Properties -------------------------------------------------------------------------------------------
        #region CAMERA_PROPERTIES

        #region PRIVATE_CACHE_DATA
        private double _cacheExpo;
        private double _cacheGain;
        #endregion

        public double ExposureTime
        {
            get => _cacheExpo;
            set => setExposureTime(value);
        }
        public double Gain
        {
            get => _cacheGain;
            set => setGain(value);
        }

        public dvpStatus getExposureTime(out double value, out double min, out double max)
        {
            var descriptor = new dvpDoubleDescr();
            var ret = DVPCamera.dvpGetExposureDescr(_cameraHandle, ref descriptor);
            if (dvpStatus.DVP_STATUS_OK != ret)
            {
                _ERR(ret, "失敗於 dvpGetExposureDescr");
                value = 0; min = 0; max = 0;
            }
            else
            {
                value = descriptor.fDefault;
                min = descriptor.fMin;
                max = descriptor.fMax;
                _cacheExpo = value;
            }
            return ret;
        }
        public dvpStatus setExposureTime(double value)
        {
            var ret = DVPCamera.dvpSetExposure(_cameraHandle, value);
            if (dvpStatus.DVP_STATUS_OK != ret)
            {
                _ERR(ret, "失敗於 dvpSetExposure");
            }
            return ret;
        }
        public dvpStatus getGain(out double value, out double min, out double max)
        {
            var descriptor = new dvpFloatDescr();
            var ret = DVPCamera.dvpGetAnalogGainDescr(_cameraHandle, ref descriptor);
            if (dvpStatus.DVP_STATUS_OK != ret)
            {
                _ERR(ret, "失敗於 dvpGetAnalogGainDescr");
                value = 0; min = 0; max = 0;
            }
            else
            {
                value = descriptor.fDefault;
                min = descriptor.fMin;
                max = descriptor.fMax;
                _cacheExpo = value;
            }
            return ret;
        }
        public dvpStatus setGain(double value)
        {
            var ret = DVPCamera.dvpSetAnalogGain(_cameraHandle, (float)value);
            if (dvpStatus.DVP_STATUS_OK != ret)
            {
                _ERR(ret, "失敗於 dvpSetAnalogGain");
            }
            return ret;
        }
        public dvpStatus getFPS(out float value, out float min, out float max)
        {
            throw new NotImplementedException();
        }
        public int setFPS(float value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 關掉自動白平衡
        /// </summary>
        public int disableBalanceWhiteAuto()
        {
            //var mode = MyCamera.MV_CAM_BALANCEWHITE_AUTO.MV_BALANCEWHITE_AUTO_OFF;
            //return setEnumValue("BalanceWhiteAuto", (uint)mode);
            throw new NotImplementedException();
        }
        /// <summary>
        /// 關掉自動增益
        /// </summary>
        public int disableGainAuto()
        {
            //var mode = MyCamera.MV_CAM_GAIN_MODE.MV_GAIN_MODE_OFF;
            //return setEnumValue("GainAuto", (uint)mode);
            throw new NotImplementedException();
        }
        #endregion

        //--- Camera Trigger Settings -------------------------------------------------------------------------------------
        #region CAMERA_TRIGGER_SETTINGS

        #region PRIVATE_CACHE_DATA
        //private int _cacheAcquisitionMode;
        private dvpTriggerSource _cacheTriggerSource;
        private bool _cacheTriggerMode;
        #endregion

        public dvpTriggerSource TriggerSource
        {
            get
            {
                return _cacheTriggerSource;
            }
            set
            {
                setTriggerSource(value);
            }
        }
        public bool TriggerMode
        {
            get
            {
                return _cacheTriggerMode;
            }
            set
            {
                setTriggerMode(value);
            }
        }

        /// <summary>
        /// 触发源选择:
        /// </summary>
        public dvpStatus setTriggerSource(dvpTriggerSource value, bool readBack = true)
        {
            var ret = DVPCamera.dvpSetTriggerSource(_cameraHandle, value);
            if (ret == dvpStatus.DVP_STATUS_OK)
            {
                if (readBack)
                {
                    ret = DVPCamera.dvpGetTriggerSource(_cameraHandle, ref value);
                    if (ret == dvpStatus.DVP_STATUS_OK)
                        _cacheTriggerSource = value;
                }
                else
                {
                    _cacheTriggerSource = value;
                }
            }
            return ret;
        }
        /// <summary>
        /// 設定 触发模式 ON/OFF
        /// </summary>
        public dvpStatus setTriggerMode(bool value, bool readBack = true)
        {
            var ret = DVPCamera.dvpSetTriggerState(_cameraHandle, value);
            if (ret == dvpStatus.DVP_STATUS_OK)
            {
                if (readBack)
                {
                    ret = DVPCamera.dvpGetTriggerState(_cameraHandle, ref value);
                    if (ret == dvpStatus.DVP_STATUS_OK)
                        _cacheTriggerMode = value;
                }
                else
                {
                    _cacheTriggerMode = value;
                }
            }
            return ret;
        }
        #endregion

        //--- Private DVP Functions ---------------------------------------------------------------------------------------
        #region PRIVATE_DVP_輔助函式
        private dvpStatus registerCallbacks()
        {
            IntPtr pUserContext = IntPtr.Zero;

            //确保不被回收
            GCHandle.Alloc(_proc);

            _proc = callback_stream;

            ///回调函数要绑定在主线程开启,不然会被垃圾回调函数处理掉
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                //为设备注册回调函数
                var status = DVPCamera.dvpRegisterStreamCallback(
                                _cameraHandle, _proc,
                                dvpStreamEvent.STREAM_EVENT_PROCESSED,
                                pUserContext);
                if (status != dvpStatus.DVP_STATUS_OK)
                {
                    _ERR(status, "注册回调函数失败");
                    return status;
                }
            }

            return dvpStatus.DVP_STATUS_OK;
        }

        /// <summary>
        /// 图像缓存队列尺寸
        /// </summary>
        private dvpStatus setImageBuffer()
        {
            dvpStatus status = DVPCamera.dvpSetBufferQueueSize(_cameraHandle, 1);
            if (status != dvpStatus.DVP_STATUS_OK)
                _ERR(status, "失敗於 dvpSetBufferQueueSize", "设置触发缓存队列失败");
            return status;
        }

        /// <summary>
        /// 相机图片缓存模式
        /// </summary>
        private dvpStatus setImageBufferMode()
        {
            //最新帧输出，旧帧将被覆盖 
            var config = new dvpBufferConfig();
            config.mode = dvpBufferMode.BUFFER_MODE_NEWEST;
            config.uQueueSize = 1;

            var status = DVPCamera.dvpSetBufferConfig(_cameraHandle, config);
            if (status != dvpStatus.DVP_STATUS_OK)
                _ERR(status, "失敗於 dvpSetBufferConfig", "设置相机缓存模式失败");

            return status;
        }

        /// <summary>
        /// 获取采集流状态
        /// </summary>
        private dvpStatus getStreamingState(out dvpStreamState state)
        {
            state = dvpStreamState.STATE_STOPED;
            dvpStatus ret = DVPCamera.dvpGetStreamState(_cameraHandle, ref state);
            if (dvpStatus.DVP_STATUS_OK != ret)
            {
                _ERR(ret, "失敗於 dvpGetStreamState", "获取采集流状态失败");
            }
            return ret;
        }

        /// <summary>
        /// 設定采集流状态
        /// </summary>
        private dvpStatus setStreamingState(dvpStreamState state)
        {
            dvpStatus ret = DVPCamera.dvpSetStreamState(_cameraHandle, state);
            if (dvpStatus.DVP_STATUS_OK != ret)
            {
                _ERR(ret, "失敗於 dvpSetStreamState", "設定采集流状态失敗");
            }
            return ret;
        }

        private bool isValidHandle(uint handle)
        {
            bool isValid = false;

            dvpStatus status = DVPCamera.dvpIsValid(handle, ref isValid);
            if (status != dvpStatus.DVP_STATUS_OK)
            {
                _ERR(status, "失敗於 dvpIsValid", "IsValidHandle");
                isValid = false;
            }
            return isValid;
        }

        /// <summary>
        /// 重新初始化掉线相机
        /// </summary>
        private dvpStatus reInitDevice()
        {
            // 保留
            throw new NotImplementedException();

            //int ret1 = registerCallbacks();

            //// 重新回復原有 Properties (從 cache 取得的數據)
            //disableBalanceWhiteAuto();
            //disableGainAuto();
            //setExposureTime(ExposureTime);
            //setGain(Gain);

            //// Trigger Source
            //setTriggerSource(TriggerSource);

            //int ret2 = StartGrabbing();
            //return ret1 != MyCamera.MV_OK ? ret1 : ret2;
        }
        #endregion

    }


    //--- 對外最常用的函式 -------------------------------------------------------------------------------------------------
    partial class Do3Camera
    {
        /// <summary>
        /// 建構 DVP 相機
        /// </summary>
        public int ConstructDevice(ref dvpCameraInfo deviceInfo)
        {
            _deviceInfo = deviceInfo;


            //(1) 通过相机默认名称开启相机,例如 M3S1207 - H - O2C@U016000000226
            var ret = (dvpStatus)OpenDevice();
            if (ret != dvpStatus.DVP_STATUS_OK)
                return (int)ret;

            //(2) 設定/註冊回調函式
            var ret2 = (dvpStatus)registerCallbacks();

            //(3) 相机缓存队列尺寸
            var ret3 = setImageBuffer();

            //(4) 相机图片缓存模式
            var ret4 = setImageBufferMode();

            //(5) 設定默認的 采集模式, 觸發源, 與觸發模式
            var ret5 = (dvpStatus)SetDefaultAcqAndTriggerModes();

            //(6) StartGrabbing
            var ret6 = (dvpStatus)StartGrabbing();

            //返回 第一個 錯誤碼
            ret = _FIRST_ERROR(ret, ret2, ret3, ret4, ret5, ret6);

            return (int)ret;
        }

        public bool IsOpen()
        {
            return _isCreatedOpen;
        }

        /// <summary>
        /// 關閉相機設備
        /// </summary>
        public int CloseDevice(bool releaseAll = true)
        {
            var ret = dvpStatus.DVP_STATUS_OK;

            if (releaseAll)
            {
                SetDefaultAcqAndTriggerModes();
            }

            if (IsGrabbing())
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
                m_currentBmpReady.Set();
            }

            if (_isCreatedOpen)
            {
                ret = DVPCamera.dvpClose(_cameraHandle);
                if (ret != dvpStatus.DVP_STATUS_OK)
                    _ERR(ret, "錯誤於 dvpClose");

                _isCreatedOpen = false;
            }

            return (int)ret;
        }

        /// <summary>
        /// 開啟相機設備
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public int OpenDevice(int delay = 0)
        {
            var ret = dvpStatus.DVP_STATUS_OK;

            if (!_isCreatedOpen)
            {
                // 通过相机句柄开启相机
                // dvpStatus status = DVPCamera.dvpOpen(i, dvpOpenMode.OPEN_NORMAL, ref handles[i]);

                //(1) 通过相机默认名称开启相机,例如 M3S1207 - H - O2C@U016000000226
                ret = DVPCamera.dvpOpenByName(_deviceInfo.FriendlyName,
                                              dvpOpenMode.OPEN_NORMAL,
                                              ref _cameraHandle);

                if (ret != dvpStatus.DVP_STATUS_OK)
                {
                    _ERR(ret, "失敗於 dvpOpenByName", "默认名称开启相机失敗");
                    return (int)ret;
                }

                // DELAY
                if (delay > 0) Thread.Sleep(delay);

                // 句柄應該為合法
                System.Diagnostics.Debug.Assert(isValidHandle(_cameraHandle));

                _isCreatedOpen = true;
            }

            return (int)ret;
        }

        /// <summary>
        /// 設定默認的 采集模式, 觸發源, 與觸發模式
        /// <br/>   TriggerSource:  Software
        /// <br/>   TriggerMode:    ON
        /// </summary>
        public int SetDefaultAcqAndTriggerModes()
        {
            var ret = (dvpStatus)StopGrabbing();
            var ret2 = setTriggerSource(dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE);
            var ret3 = setTriggerMode(true);
            ret = _FIRST_ERROR(ret, ret2, ret3);
            return (int)ret;
        }

        public bool IsGrabbing(bool queryAgain = false)
        {
            if (queryAgain)
            {
                var ret = getStreamingState(out dvpStreamState streamState);
                if (ret == dvpStatus.DVP_STATUS_OK)
                {
                    _isGrabbing = (streamState == dvpStreamState.STATE_STARTED);
                }
            }
            return _isGrabbing;
        }

        /// <summary>
        /// 开始采集图像
        /// </summary>
        public int StartGrabbing()
        {
            // 如果目前已經開始, 就直接回 OK
            var ret = getStreamingState(out dvpStreamState streamState);
            if (ret == dvpStatus.DVP_STATUS_OK)
            {
                if (streamState == dvpStreamState.STATE_STARTED)
                {
                    _isGrabbing = true;
                    return (int)ret;
                }
            }

            // 強制 dvpStart
            ret = DVPCamera.dvpStart(_cameraHandle);

            if (dvpStatus.DVP_STATUS_OK != ret)
            {
                // 標記 cache
                _isGrabbing = false;
                _ERR(ret, "失敗於 dvpStart", "开启采集流失败");
                return (int)ret;
            }

            // 標記 cache
            _isGrabbing = true;
            return (int)ret;
        }

        /// <summary>
        /// 相继停止采集图像
        /// </summary>
        public int StopGrabbing()
        {
            // 如果目前已經停止, 就直接回 OK
            var ret = getStreamingState(out dvpStreamState streamState);
            if (ret == dvpStatus.DVP_STATUS_OK)
            {
                if (streamState == dvpStreamState.STATE_STOPED)
                {
                    _isGrabbing = false;
                    return (int)ret;
                }
            }

            // 強制 dvpStop
            ret = DVPCamera.dvpStop(_cameraHandle);

            if (dvpStatus.DVP_STATUS_OK != ret)
            {
                //>>> _isGrabbing = true;
                _ERR(ret, "失敗於 dvpStop", "获取采集流状态失败");
                return (int)ret;
            }

            // 標記 cache
            _isGrabbing = false;
            return (int)ret;
        }

        /// <summary>
        /// 下達軟體觸發指令
        /// </summary>
        public int TriggerSoftware()
        {
            var ret = DVPCamera.dvpTriggerFire(_cameraHandle);
            if (ret != dvpStatus.DVP_STATUS_OK)
                _ERR(ret, "失敗於 dvpTriggerFire", "触发失败");
            return (int)ret;
        }

        /// <summary>
        /// 下達軟體觸發指令, 並等待影像擷取完成, 傳回該張影像
        /// <br/> (1) quickAccess= true 時, 直接存取 FrameBuf, 調用者不需要 Dispose(), 但是要自行負責多線程衝突.
        /// <br/> (2) quickAccess= false 時, 安全 clone FrameBuf, 調用者負責 Dispose().
        /// <br/> (3) 目前搭配: TriggerSource = Software , TriggerMode = ON
        /// </summary>
        public Bitmap TriggerSoftwareCapture(bool quickAccess = false)
        {
            m_currentBmpReady.Reset();
            TriggerSoftware();

            m_currentBmpReady.WaitOne();

            if (quickAccess)
            {
                // 直接快速傳回 FrameBuffer 的影像,
                // 調用者必須自己處理多線程共享問題.
                return m_currentBmp;
            }
            else
            {
                // 保護性 lock
                lock (m_currentBmpLocker)
                {
                    return (Bitmap)m_currentBmp.Clone();
                }
            }
        }
    }


    //--- CALLBACKS_實作 --------------------------------------------------------------------------------------
    #region CALLBACKS_實作
    partial class Do3Camera
    {
        #region PRIVATE_IMAGE_BUF
        private AutoResetEvent m_currentBmpReady = new AutoResetEvent(false);
        protected object m_currentBmpLocker = new object();
        protected Bitmap m_currentBmp;
        #endregion

        /// <summary>
        /// 回调函数接收相机图像数据
        /// </summary>
        /// <param name="handle">相机句柄(度申相机句柄第一个是1不是0)</param>
        /// <param name="_event">事件类型</param>
        /// <param name="pContext">用户指针</param>
        /// <param name="refFrame">帧信息</param>
        /// <param name="pBuffer">图像数据</param>
        /// <returns></returns>
        private int callback_stream(uint handle, dvpStreamEvent _event, IntPtr pContext, ref dvpFrame refFrame, IntPtr pBuffer)
        {
            #region 直接將影像_Render_到_hwndDirectDisplay_所指定的視窗
            if (false && _hwndDirectDisplay != IntPtr.Zero)
            {
                DVPCamera.dvpDrawPicture(ref refFrame, pBuffer, _hwndDirectDisplay, (IntPtr)0, (IntPtr)0);
            }
            #endregion

            lock (m_currentBmpLocker)
            {
                m_currentBmp?.Dispose();
                m_currentBmp = get_frame_bmp(pBuffer, ref refFrame);
            }
            m_currentBmpReady.Set();

            fireFrameCapturedEvent(m_currentBmp);

            return 0;
        }

        /// <summary>
        /// 單純從 frameInfo 取出 C# Bitmap
        /// <br/> (1) 調用者必須負責 Dispose()
        /// <br/> (2) 所有影像顏色轉換與旋轉交於後續處理
        /// <br/> (3) 此處不進行 lock()
        /// </summary>
        private Bitmap get_frame_bmp(IntPtr pData, ref dvpFrame frameInfo)
        {
            Bitmap bmpFrame = null;

            if (frameInfo.format == dvpImageFormat.FORMAT_BGR24 ||
                frameInfo.format == dvpImageFormat.FORMAT_RGB24)
            {
                bmpFrame = new Bitmap(frameInfo.iWidth, frameInfo.iHeight, frameInfo.iWidth * 3, PixelFormat.Format24bppRgb, pData);
            }
            else if (frameInfo.format == dvpImageFormat.FORMAT_MONO)
            {
                bmpFrame = new Bitmap(frameInfo.iWidth, frameInfo.iHeight, frameInfo.iWidth, PixelFormat.Format8bppIndexed, pData);
                ColorPalette cp = bmpFrame.Palette;
                // init palette
                for (int i = 0; i < 256; i++)
                {
                    cp.Entries[i] = Color.FromArgb(i, i, i);
                }
                // set palette back
                bmpFrame.Palette = cp;
            }

            return bmpFrame;
        }

    }
    #endregion


    //--- 度申特有函式 -----------------------------------------------------------------------------------------
    #region 度申特有的函式
    partial class Do3Camera
    {
        public dvpStatus getStreamFormat(out dvpStreamFormat format)
        {
            format = dvpStreamFormat.S_BGR24;
            var ret = DVPCamera.dvpGetTargetFormat(_cameraHandle, ref format);
            if (ret != dvpStatus.DVP_STATUS_OK)
                _ERR(ret, "失敗於 dvpGetTargetFormat");
            return ret;
        }

        public dvpStatus setStreamFormat(dvpStreamFormat format)
        {
            var ret = getStreamFormat(out dvpStreamFormat curFormat);
            if (ret == dvpStatus.DVP_STATUS_OK && curFormat == format)
                return ret;

            ret = DVPCamera.dvpSetTargetFormat(_cameraHandle, format);
            if (ret != dvpStatus.DVP_STATUS_OK) 
                _ERR(ret, "失敗於 dvpSetTargetFormat");
            return ret;
        }

        /// <summary>
        /// 设置黑白模式
        /// </summary>
        /// <returns></returns>
        public dvpStatus SetMonoFormat()
        {
            return setStreamFormat(dvpStreamFormat.S_MONO8);
        }

        /// <summary>
        /// 设置彩色模式
        /// </summary>
        /// <returns></returns>
        public dvpStatus SetRgb24Format()
        {
            return setStreamFormat(dvpStreamFormat.S_RGB24);
        }

        /// <summary>
        /// 加载相机设定
        /// </summary>
        /// <param name="CCDIndex">相机索引</param>
        /// <param name="fileName">相机设定档</param>
        public dvpStatus LoadConfig(string fileName)
        {
            var ret = DVPCamera.dvpLoadConfig(_cameraHandle, fileName);
            if (ret != dvpStatus.DVP_STATUS_OK)
            {
                _ERR(ret, "失敗於 dvpLoadConfig", "加载相机设定失败");
            }
            return ret;
        }

        /// <summary>
        /// 保存config
        /// </summary>
        /// <param name="CCDIndex">相机索引</param>
        /// <param name="fileName">相机当保存目录</param>
        public dvpStatus SaveConfig(string fileName)
        {
            //if (IsValidHandle(CCDIndex))
            //{
            //    uint handle = handles[CCDIndex];
            //    dvpStatus status = DVPCamera.dvpSaveConfig(handle, $"{ccdConfigSaveDir}\\{DeviceInfo[CCDIndex].FriendlyName}.INI");
            //    if (status != dvpStatus.DVP_STATUS_OK)
            //    {

            //        ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}保存相机设定失败", status);
            //    }
            //}
            var ret = DVPCamera.dvpSaveConfig(_cameraHandle, fileName);
            if (ret != dvpStatus.DVP_STATUS_OK)
            {
                _ERR(ret, "失敗於 dvpSaveConfig", "保存相机设定失败");
            }
            return ret;
        }

        /// <summary>
        /// 显示属性框
        /// </summary>
        public dvpStatus ShowProperty(IntPtr hwndParent)
        {
            var ret = DVPCamera.dvpShowPropertyModalDialog(_cameraHandle, hwndParent);
            if (ret != dvpStatus.DVP_STATUS_OK)
            {
                _ERR(ret, "失敗於 dvpShowPropertyModalDialog", "打开属性框设定失败");
            }
            return ret;
        }
    }
    #endregion

}
