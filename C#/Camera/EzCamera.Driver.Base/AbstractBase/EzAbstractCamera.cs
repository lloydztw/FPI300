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

using EzCamera.Driver.Utils;
using EzCamera.Interface;
using System;
using System.Drawing;
using System.Threading;


namespace EzCamera.Driver.Base
{
    /// <summary>
    /// 相機的共用基礎抽象類別,
    /// <br/> (1) 使用 Async swapBuf 來增進效能
    /// <br/> (2) 預設提供了: 反向, 亮度, 對比 軟體運算
    /// <br/> (3) 可於實時設定 0, 90, 180, 270 四種旋轉
    /// <br/> (4) 其他家廠牌相機, 實作以下三個abstract functions
    /// <br/>         drvInit,
    /// <br/>         drvDispose
    /// <br/>         drvCaptureImage
    /// <br/>     應該就可以達成基本功能.
    /// </summary>
    public abstract class EzAbstractCamera : EzCameraProps, IEzCamera
    {
        #region NLOG
        // NOTE:
        // 不要做靜態初始化，而是要等到窗體的Load事件時才初始化Logger物件，
        // 且保證該窗體是 【首個使用】NLog 的 Class !!!
        // 這是因為NLog是在首次被使用時，才載入配置文件的。
        static NLog.ILogger _logger = null;
        protected NLog.ILogger _LOG
        {
            get
            {
                if (_logger == null)
                    _logger = NLog.LogManager.GetCurrentClassLogger();
                return _logger;
            }
        }
        #endregion

        #region CONFIGS_AND_OPTIONS
        const int TIMEOUT_MS = 3000;
        protected bool OPT_FORCE_RGB24 = false;
        protected bool OPT_FORCE_MONO_COLOR => base.Mono;
        protected bool OPT_USE_POLLING_THREAD = true;
        #endregion

        #region PRIVATE_ASYNC_SWAP_BUF
        private AutoResetEvent _asyncSwapBufReady = new AutoResetEvent(false);
        private Bitmap _swapBuf;
        private bool _isInitDone = false;
        #endregion

        #region PRIVATE_THREAD_MEMBERS
        private volatile bool _runFlag = false;
        private Thread _liveThread;
        private DateTime _lastLiveTime = DateTime.Now;
        #endregion

        #region PRIVATE_DATA
        IEzDeviceInfo _deviceInfo;
        EzCameraError _lastError = EzCameraError.NoError;
        EzPixelCursor _pixCur = new EzPixelCursor();
        //>>> QFrameRateGauge _frameRateMeter = new QFrameRateGauge();
        #endregion

        #region PROTECTED_IMAGE_PROCESS
        /// <summary>
        /// 影像處理輔助工具 
        /// </summary>
        protected EzImageProcess _imgProcess = new EzImageProcess();
        #endregion

        public event EventHandler<EzLiveImageEventArgs> OnLiveImage;
        public event EventHandler<EzCameraErrorEventArgs> OnError;
        public event EventHandler OnLiveModeChanged;
        public event EventHandler OnDeviceInfoChanged;

        protected EzAbstractCamera(IEzDeviceInfo info)
        {
            _deviceInfo = info;
            GlobalCamID = AllCreatedCameras.Register(this) - 1;
        }

        #region NAMES_AND_ID
        public virtual IEzDeviceInfo DeviceInfo
        {
            get => _deviceInfo;
            protected set => _deviceInfo = value;
        }
        public virtual string FriendlyName
        {
            // 默認直接使用 EzCameraDeviceInfo.ToString()
            // get => $"{DeviceInfo.ToString()}#{CamID}";
            get => _deviceInfo != null ?
                   _deviceInfo.ToString() :
                   EzDispText.Format(GetType().Name, CamID);
        }
        public int GlobalCamID
        {
            get;
            set;
        }
        public virtual int CamID
        {
            // 默認直接使用 DeviceInfo.Index 當作 CamID (局域)
            get => _deviceInfo != null ? _deviceInfo.Index : 0;
        }
        public override string ToString()
        {
            return FriendlyName;
        }
        #endregion

        public virtual void Dispose()
        {
            StopLiveMode();

            _swapBuf?.Dispose();
            _swapBuf = null;

            drvDispose();
            _isInitDone = false;

            AllCreatedCameras.Unregister(this);
        }
        public virtual void Init()
        {
            if (!_isInitDone)
            {
                _camImageInfo = drvInit();
                _asyncSwapBufReady.Set();
                _isInitDone = true;
            }
        }

        public override int RotateAngle
        {
            get => base.RotateAngle;
            set => runtime_change_rotation(value);
        }
        public EzCameraError GetError()
        {
            return _lastError;
        }
        public Color GetPixelColor(int x, int y)
        {
            // 移動 x, y 座標.
            _pixCur.MoveTo(x, y);

            if (IsLiveMode())
            {
                // 稍後會在 live_thread_func 取出顏色
                return _pixCur.Color;
            }
            else
            {
                // 立即取色
                try
                {
                    _pixCur.PickColor(_swapBuf);
                }
                catch
                {

                }
                return _pixCur.Color;
            }
        }

        public virtual bool IsLiveMode()
        {
            // 如果沒使用 polling thread, 則必須要覆寫此函式.
            if (!OPT_USE_POLLING_THREAD)
                throw new NotImplementedException("如果沒使用 polling thread, 則必須要覆寫此函式");

            return isLiveThreading();
        }
        public virtual void StartLiveMode()
        {
            // 如果沒使用 polling thread, 則必須要覆寫此函式.
            if (!OPT_USE_POLLING_THREAD)
                throw new NotImplementedException("如果沒使用 polling thread, 則必須要覆寫此函式");

            if (!_isInitDone || IsLiveMode())
                return;

            startLiveThread();
            fireLiveModeChanged();
        }
        public virtual void StopLiveMode()
        {
            // 如果沒使用 polling thread, 則必須要覆寫此函式.
            if (!OPT_USE_POLLING_THREAD)
                throw new NotImplementedException("如果沒使用 polling thread, 則必須要覆寫此函式");

            stopLiveThread();
            fireLiveModeChanged();
        }

        public virtual Bitmap Snapshot(bool fireEvent = false)
        {
            if (!_isInitDone)
                return null;

            if (IsLiveMode())
                StopLiveMode();

            var img = grab_one_frame_image(withImgProcess: true);

            if (img == null)
            {
                // 目前: 直接 return null 交給上層顯示警訊
                return null;
                // 保留: 或是生成一個 blank image 
                img = create_blank_image();
            }

            if (fireEvent)
            {
                bool ok = _asyncSwapBufReady.WaitOne(TIMEOUT_MS);
                if (ok)
                {
                    fireAsyncLiveImageEvent(img, true);
                    return (Bitmap)img?.Clone();
                }
            }

            return img;
        }
        public virtual void TriggerOneFrame()
        {
            if (!_isInitDone)
                return;

            if (IsLiveMode())
                return;

            using (var tmp = Snapshot(true))
            {
                // Just Do Nothing.
            }
        }

        public virtual bool HasPropertyPanel()
        {
            return false;
        }
        public virtual void ShowPropertyPanel(bool show, object parentWindow = null)
        {
            // parentWindow 可視狀況
            //  轉型為 System.Windows.Forms.Control
            //  (或 IntPtr)
        }
        public virtual bool IsSimulation()
        {
            return false;
        }
        public virtual string Browse(string filePath)
        {
            return null;
        }

        #region PRIVATE_FUNCTIONS
        /// <summary>
        /// 從相機抓取一張影像, 並進行軟體預置的影像處理
        /// </summary>
        private Bitmap grab_one_frame_image(bool withImgProcess)
        {
            var img = drvCaptureImage();

            if (withImgProcess)
                apply_internal_image_process(img);

            return img;
        }
        private Bitmap create_blank_image()
        {
            GetImageInfo(out int w, out int h, out int px);
            if (w <= 0) w = 1;
            if (h <= 0) h = 1;
            if (px <= 0) px = 1;
            var fmt = EzImageInfo.GetPixelFormat(px);
            var img = new Bitmap(w, h, fmt);
            using (var gx = Graphics.FromImage(img))
            using (var font = new Font("Ariel", 32f))
            {
                gx.DrawString($"Camera_{CamID} 無影像", font, Brushes.White, 32, 32);
            }
            return img;
        }
        private Bitmap apply_internal_image_process(Bitmap img)
        {
            var tmp = apply_color_transform(img);
            if (tmp != img) 
                img?.Dispose();

            img = apply_inv_bright_constrast(tmp);
            if (tmp != img) 
                tmp?.Dispose();

            return img;
        }
        protected virtual Bitmap apply_color_transform(Bitmap imgBuf)
        {
            if (imgBuf == null)
                return null;

            int bits = Image.GetPixelFormatSize(imgBuf.PixelFormat);

            // 強制轉成 RGB 24bit
            if (OPT_FORCE_RGB24 || bits > 24)
                imgBuf = _imgProcess.ToRgb24(imgBuf, true);

            // 強制轉成 單色
            if (OPT_FORCE_MONO_COLOR)
                _imgProcess.ApplyMonoColor(imgBuf);

            return imgBuf;
        }
        protected virtual Bitmap apply_inv_bright_constrast(Bitmap imgBuf)
        {
            if (imgBuf == null)
                return null;

            if (true)
            {
                // 影像處理結果直接 作用在同一個 imgBuf
                _imgProcess.ApplyInvBrightnessContrast(imgBuf, this);
                _imgProcess.ApplyRotation(imgBuf, this.RotateAngle);
                //_pixCur.PickColor(imgBuf);
                return imgBuf;
            }
            else
            {
                Bitmap result;

                _imgProcess.ApplyInvBrightnessContrast(imgBuf, this);
                if (this.RotateAngle == 0)
                {
                    result = imgBuf;
                }
                else
                {
                    // 生成新影像
                    // 然後旋轉: 90, 180, 270
                    result = (Bitmap)imgBuf.Clone();
                    _imgProcess.ApplyRotation(result, this.RotateAngle);

                }

                //_pixCur.PickColor(result);
                return result;
            }
        }
        private void software_delay_for_fps(double elapseSeconds)
        {
            var maxFps = this.MaxFps;

            if (maxFps <= 0)
                return;

            if (maxFps >= 100)
                return;

            var ms = (int)((1.0 / maxFps - elapseSeconds) * 1000);
            if (ms <= 0)
                return;

            // Slow FPS
            if (maxFps < 10)
            {
                Thread.Sleep(ms);
            }
            // Middle FPS
            else if (maxFps < 100)
            {
                Thread.Sleep((int)(0.8 * ms));
            }
            // High FPS
            else
            {
                // no delay
            }
        }
        private bool check_time_out(double elapseSeconds)
        {
            if (elapseSeconds > TIMEOUT_MS)
            {
                return true;
            }
            return false;
        }
        private void runtime_change_rotation(int rotation)
        {
            rotation = (rotation / 90) * 90 % 360;

            if (rotation != base.RotateAngle)
            {
                if (_isInitDone)
                {
                    bool isLive = IsLiveMode();

                    if (isLive)
                        StopLiveMode();

                    base.RotateAngle = rotation;

                    if (isLive)
                    {
                        new Action(() => { StartLiveMode(); }).BeginInvoke(null, null);
                    }
                    else
                    {
                        new Action(() => { TriggerOneFrame(); }).BeginInvoke(null, null);
                    }
                }
                else
                {
                    base.RotateAngle = rotation;
                }
            }
        }
        #endregion

        #region THREAD_FUNCTIONS
        protected bool isLiveThreading()
        {
            if (_liveThread != null && _liveThread.IsAlive)
                return true;

            return _runFlag;
        }
        protected void startLiveThread()
        {
            if (!_runFlag)
            {
                _runFlag = true;
                _liveThread = new Thread(live_thread_func);
                //_liveThread.Name = $"Camera[{DeviceInfo}]";
                _liveThread.Name = EzDispText.Format(GlobalCamID, DeviceInfo);
                _liveThread.Priority = ThreadPriority.Highest;
                _lastLiveTime = DateTime.Now;
                _liveThread.Start();
            }
        }
        protected void stopLiveThread()
        {
            if (_runFlag)
            {
                _LOG.Debug($"Cam_{GlobalCamID} [STOP]");

                _runFlag = false;
                _asyncSwapBufReady.Set();
                bool ok = _liveThread.Join(1500);
                if (!ok)
                {
                    _liveThread.Abort();
                }
            }
        }
        private void live_thread_func()
        {
            if (!OPT_USE_POLLING_THREAD)
                return;

            _LOG.Debug($"{_liveThread.Name} live_thread_func() ++");

            //>>> using (var bridge = new QxImageBridge())
            {
                while (_runFlag)
                {
                    //(1) 取像
                    var img = grab_one_frame_image(withImgProcess: false);
                    if (img == null)
                    {
                        var ts1 = DateTime.Now - _lastLiveTime;
                        if (check_time_out(ts1.TotalSeconds))
                        {
                            fireErrorEvent(EzCameraError.Timeout);
                            _runFlag = false;
                            break;
                        }
                        Thread.Sleep(1);
                        continue;
                    }

                    #region OLD_CODE
                    ////(2) 等待 _swapBuf 可使用
                    //_asyncSwapBufReady.WaitOne();
                    ////(2.1) 限制 MaxFps (software delay)
                    //var ts2 = DateTime.Now - _lastLiveTime;
                    //software_delay_for_fps(ts2.TotalSeconds);
                    ////(2.2) 非同步觸發 OnLiveImage 事件 (執行於額外隱式線程)
                    //fireAsyncLiveImageEvent(img);
                    ////(2.3) 標記時間
                    //_lastLiveTime = DateTime.Now;
                    #endregion

                    //(2) 推送 LiveImage
                    pushOneLiveImage(img, true);
                }
            }

            _LOG.Debug($"{_liveThread.Name} live_thread_func() --");
        }
        #endregion

        #region LIVE_IMG_PUSH_FUNCTION
        /// <summary>
        /// 將 liveImg 推送到 _swapBuf, 默認進行內置軟體影像處理, 並發出 OnLiveImage 事件. <br/>
        /// 此函式會接管 liveImg 的生命週期, 調用者如果離開此函式後要繼續使用, 
        /// 必須先自己在函式外 Clone 一個備份.
        /// </summary>
        protected void pushOneLiveImage(Bitmap liveImg, bool withImgProcess = true)
        {
            //_LOG.Debug("pushOneLiveImage() ++");

            //(1) Image Processing
            if (withImgProcess)
                apply_internal_image_process(liveImg);

            //(1.1) 保存像素顏色到 cache
            _pixCur.PickColor(liveImg);

            //(2) 等待 _swapBuf 可使用
            _asyncSwapBufReady.WaitOne();

            //(2.1) 限制 MaxFps (software delay)
            var ts2 = DateTime.Now - _lastLiveTime;
            software_delay_for_fps(ts2.TotalSeconds);

            //(2.2) 非同步觸發 OnLiveImage 事件 (執行於額外隱式線程)
            bool forceFlag = OPT_USE_POLLING_THREAD ? false : true;
            fireAsyncLiveImageEvent(liveImg, forceFlag);

            //(2.3) 標記時間
            _lastLiveTime = DateTime.Now;

            //_LOG.Debug("pushOneLiveImage() --");
        }
        protected void unlockBufs()
        {
            _asyncSwapBufReady.Set();
        }
        #endregion

        #region PROTECTED_EVENT_TRIGGER_FUNCTIONS
        protected void fireDeviceInfoChanged()
        {
            OnDeviceInfoChanged?.Invoke(this, EventArgs.Empty);
        }
        protected void fireLiveModeChanged()
        {
            OnLiveModeChanged?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// 發出異常通知
        /// </summary>
        protected void fireErrorEvent(EzCameraError err, Exception ex = null)
        {
            try
            {
                _LOG.Error($"Cam_{GlobalCamID} [Error] = {0}", err);
                _lastError = err != null ? err.Clone() : null;
                var ev = new EzCameraErrorEventArgs(err, ex);
                OnError?.Invoke(this, ev);
            }
            catch(Exception exx)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] {GetType().Name}.fireErrorEvent: {exx}");
            }
        }
        /// <summary>
        /// 非同步發出即時影像到位通知.
        /// <br/> 1. 此函式會接手 inBuf 的生命週期.
        /// <br/> 2. 內部會執行於另一個隱式線程
        /// <br/> 3. inBuf 會被交換到 _swapBuf
        /// <br/> 4. 舊的 _swapBuf 會被 Dispose()
        /// </summary>
        protected void fireAsyncLiveImageEvent(Bitmap inBuf, bool force = false)
        {
            bool fire = _runFlag || force;
            
            if (fire && OnLiveImage != null)
            {
                var old = _swapBuf;
                _swapBuf = inBuf;
                old?.Dispose();

                //>>> OnLiveImage.BeginInvoke(this, new EzLiveImageEventArgs(_swapBuf), endOfAsync, null);

                new Action(() =>
                {
                    var e = new EzLiveImageEventArgs(_swapBuf);
                    OnLiveImage?.Invoke(this, e);
                }).BeginInvoke(endOfAsync, null);
            }
            else
            {
                var old = _swapBuf;
                _swapBuf = inBuf;
                old?.Dispose();
                _asyncSwapBufReady.Set();
            }
        }
        private void endOfAsync(IAsyncResult ar)
        {
            try
            {
                _asyncSwapBufReady.Set();

                // 獲取委派
                //>>> var handler = (EventHandler<EzLiveImageEventArgs>)((System.Runtime.Remoting.Messaging.AsyncResult)ar).AsyncDelegate;
                var handler = (Action)((System.Runtime.Remoting.Messaging.AsyncResult)ar).AsyncDelegate;

                // 結束非同步調用
                handler.EndInvoke(ar);

                //Console.WriteLine("Event handler completed.");
            }
            catch (Exception ex)
            {
                _asyncSwapBufReady.Set();
                _LOG.Warn($"Cam_{GlobalCamID} [Error] endOfAsync: {ex.Message}");
            }
        }
        #endregion

        #region ABSTRACT_FUNCTIONS
        /// <summary>
        /// 初始化相機 (繼承者必須實作)
        /// </summary>
        protected abstract EzImageInfo drvInit();
        /// <summary>
        /// 結束前釋放資源 (繼承者必須實作)
        /// </summary>
        protected abstract void drvDispose();
        /// <summary>
        /// 抓取相機影像 (繼承者必須實作)
        /// </summary>
        protected abstract Bitmap drvCaptureImage(object args = null);
        #endregion

        #region RESERVED
#if (OPT_RESERVED)
        IEzTriggerSource _triggerSource;
        protected void configTriggerSource(IEzTriggerSource src)
        {
            if (_triggerSource != null)
                return;

            _triggerSource = src;
            _triggerSource.OnLiveImage += _extSource_OnLiveImage;
            _triggerSource.OnLiveModeChanged += _extSource_OnLiveModeChanged;
        }
        private void _extSource_OnLiveModeChanged(object sender, EventArgs e)
        {
            fireLiveModeChanged();
        }
        private void _extSource_OnLiveImage(object sender, EzLiveImageEventArgs e)
        {
            if (e.LiveImage is Bitmap bmp)
                pushOneLiveImage((Bitmap)bmp.Clone());
        }
#endif
        #endregion
    }
}
