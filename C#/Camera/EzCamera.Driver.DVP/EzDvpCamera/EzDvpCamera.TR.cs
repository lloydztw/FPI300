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
using EzCamera.Driver.Base;
using EzCamera.Driver.Utils;
using EzCamera.Interface;
using System.Drawing;

namespace EzCamera.Driver.DVP
{
    using VendorCamera = Do3Camera;
    using VendorDeviceInfo = dvpCameraInfo;
    using VendorError = Do3Error;

    /// <summary>
    /// 度申 IEzCamera 的實作 (TR版)
    /// 支援 line trigger 
    /// 直接使用 callback 省去 polling thread.
    /// </summary>
    public partial class EzDvpCameraTR : EzAbstractCamera
    {
        #region DEFAULT
        public static EzCameraDeviceInfo DefaultDeviceInfo(int camID)
        {
            return new EzCameraDeviceInfo("MV", "海康威視", index: camID);
        }
        #endregion

        #region PRIVATE_核心成員
        /// <summary>
        /// VendorCamera 是對 廠家原生相機 的 簡易包裝, 
        /// 來提供: 
        /// <br/> (1) Callbacks, FrameBuffer 管理
        /// <br/> (2) Construct, Open, Close 流程控制
        /// <br/> (3) TriggerSource, TriggerMode, AcquisitionMode 設定
        /// <br/> (4) Camera Properties 讀(get)寫(set) 與 cache.
        /// </summary>
        private VendorCamera _mvCamera;
        private VendorDeviceInfo _mvDeviceInfo;
        private bool _isLiveMode = false;
        #endregion

        internal EzDvpCameraTR(IEzDeviceInfo info)
            : base(info)
        {
            //停用 EzAbstractCamera 內置的 polling thread.
            OPT_USE_POLLING_THREAD = false;

            //繼承者必須覆寫以下三個函式:
            //(1) IsLiveMode()
            //(2) StartLiveMode()
            //(3) StopLiveMode()

            _mvDeviceInfo = (VendorDeviceInfo)info.Tag;
            _mvCamera = new VendorCamera();

            //異常處理
            _mvCamera.OnError += _vendor_OnError;
            //使用 OnFrameCaptured, 來直接承接 callback
            _mvCamera.OnFrameCaptured += _vendor_OnFrameCaptured;
        }

        #region PROTECTED_OVERRIDES
        protected override EzImageInfo drvInit()
        {
            int ret = _mvCamera.ConstructDevice(ref _mvDeviceInfo);
            if (ret != (int)dvpStatus.DVP_STATUS_OK)
                return null;

            _mvCamera.SetRgb24Format();

            //// 是否要需要拍一張圖來取得資訊?
            //_mvCamera.getWidth(out uint width, out uint minW, out uint maxW);
            //_mvCamera.getHeight(out uint height, out uint minH, out uint maxH);
            //_mvCamera.getEnumValue("PixelFormat", out uint pixelFormat);
            //System.Diagnostics.Debug.WriteLine($"PixelFormat = {pixelFormat}");
            //var imgInfo = new EzImageInfo()
            //{
            //    Width = (int)width,
            //    Height = (int)height,
            //    BitsPerPixel = 24
            //};

            if (!_mvCamera.IsGrabbing())
                _mvCamera.StartGrabbing();

            // 默認模式應該如下:
            //  TriggerSource: Software
            //  TriggerMode:   ON
            System.Diagnostics.Debug.Assert(_mvCamera.TriggerSource == dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE);
            System.Diagnostics.Debug.Assert(_mvCamera.TriggerMode == true);

            //拍一張圖來取得資訊
            EzImageInfo imgInfo;
            using (var bmp = _mvCamera.TriggerSoftwareCapture())
            {
                imgInfo = EzImageInfo.FromBitmap(bmp);
            }

            // StopLiveMode();
            // changeToLineTrigger(0);
            changeToSoftwareTrigger();
            return imgInfo;
        }
        protected override void drvDispose()
        {
            _mvCamera?.CloseDevice();
            _mvCamera = null;
        }
        protected override Bitmap drvCaptureImage(object args = null)
        {
            var frame = _mvCamera.TriggerSoftwareCapture(quickAccess: true);
            var newBmp = convert_vendor_color_space(frame, true);
            return newBmp;
        }
        #endregion

        #region COLOR_SPACE_CONVERT_FOR_VENDOR
        private Bitmap convert_vendor_color_space(Bitmap bmp, bool alwaysClone)
        {
            if (bmp == null)
                return null;

            //// 海康 BGR -> RGB
            //if (bmp.PixelFormat == PixelFormat.Format24bppRgb)
            //{
            //    using (var bridge = new QxImageBridge(bmp))
            //    {
            //        var img = bridge.Image;
            //        Cv2.CvtColor(img, img, ColorConversionCodes.RGB2BGR);
            //    }
            //    return alwaysClone ? (Bitmap)bmp.Clone() : bmp;
            //}
            //else
            //{
            //    // 測試: 強制把其他格式都統一轉為 RGB 24 bit
            //    // (SLOW WAY)
            //    var bmpNew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);
            //    using (var dst = new QxImageBridge(bmpNew))
            //    using (var src = new QxImageBridge(bmp))
            //    {
            //        ColorConversionCodes convertCode;
            //        int channels = src.Image.Channels();
            //        switch (channels)
            //        {
            //            case 1: convertCode = ColorConversionCodes.GRAY2RGB; break;
            //            case 2: convertCode = ColorConversionCodes.BGR5652BGR; break;
            //            case 4: convertCode = ColorConversionCodes.RGBA2RGB; break;
            //            default: return null;
            //        }
            //        Cv2.CvtColor(src.Image, dst.Image, convertCode);
            //    }
            //    return bmpNew;
            //}

            if (alwaysClone)
                return (Bitmap)bmp.Clone();
            else
                return bmp;
        }
        #endregion

        #region INTERNAL_EVENT_HANDLERS
        private void _vendor_OnError(object sender, VendorError e)
        {
            fireErrorEvent(e);
        }
        private void _vendor_OnFrameCaptured(object sender, EzLiveImageEventArgs e)
        {
            var newBmp = convert_vendor_color_space((Bitmap)e.LiveImage, true);
            pushOneLiveImage(newBmp);

            if (IsLiveMode())
                generateNextTrigger();
        }
        #endregion

        public override bool IsLiveMode()
        {
            return _isLiveMode;
        }
        public override void StartLiveMode()
        {
            if (IsLiveMode())
                return;

            if(!_mvCamera.IsGrabbing())
                _mvCamera.StartGrabbing();

            generateNextTrigger();
        }
        public override void StopLiveMode()
        {
            _isLiveMode = false;

            if (isSoftwareTriggerSource())
            {
                // 海康: 軟體觸發源, 目前永遠保持在 Grabbing 狀態. 由 _isLiveMode 決定要不要發 Trigger
                if (!_mvCamera.IsGrabbing())
                    _mvCamera.StartGrabbing();
            }
            else
            {
                // 海康: 硬體觸發源, 使用 StartGrabbing 與 StopGrabbing 控制
                _mvCamera.StopGrabbing();
            }

            // RESERVED for advanced usage.
            // base.unlockBufs();
        }

        /// <summary>
        /// 目前限制 只能在 TriggerSource == Software 時, 才有作用.
        /// </summary>
        public override Bitmap Snapshot(bool fireEvent = false)
        {
            if (!isSoftwareTriggerSource())
                return null;
            return base.Snapshot(fireEvent);
        }
        
        /// <summary>
        /// 測試中: 尚未公開至 IEzCamera
        /// </summary>
        public bool ChangeToSoftwareTrigger()
        {
            return changeToLineTrigger();
        }

        /// <summary>
        /// 測試中: 尚未公開至 IEzCamera
        /// </summary>
        public bool ChangeToLineTrigger(int lineID = 0)
        {
            return changeToLineTrigger(lineID);
        }

        #region PRIVATE_TRIGGER_HELPER_FUNCTIONS
        bool isSoftwareTriggerSource()
        {
            return _mvCamera.TriggerSource == dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE;
        }
        void generateNextTrigger()
        {
            if (IsLiveMode())
            {
                if (isSoftwareTriggerSource())
                    _mvCamera.TriggerSoftware();
            }
        }
        bool changeToSoftwareTrigger()
        {
            if (isSoftwareTriggerSource())
                return true;
            
            // 停止 IEzCamera 的 Live Mode
            if (IsLiveMode())
                StopLiveMode();

            // 停止 Hikvision Grabbing
            if (_mvCamera.IsGrabbing())
                _mvCamera.StopGrabbing();

            var ret = _mvCamera.setTriggerSource(dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE);
            bool ok = _mvCamera.TriggerSource == dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE;
            return ok;
        }
        bool changeToLineTrigger(int lineID = 0)
        {
            if (lineID < 0 || lineID > 3)
                return false;

            var targetSource = dvpTriggerSource.TRIGGER_SOURCE_LINE1 + lineID;
            if (_mvCamera.TriggerSource == targetSource)
                return true;

            // 停止 IEzCamera 的 Live Mode
            if (IsLiveMode())
                StopLiveMode();

            // 停止 Hikvision Grabbing
            if (_mvCamera.IsGrabbing())
                _mvCamera.StopGrabbing();

            var ret = _mvCamera.setTriggerSource(targetSource);
            bool ok = _mvCamera.TriggerSource == targetSource;
            return ok;
        }
        #endregion

        #region CAMERA_PROPERTIES
        public override void GetExposureRange(out double min, out double max)
        {
            _mvCamera.getExposureTime(out double v, out double fmin, out double fmax);
            min = fmin; 
            max = fmax;
        }
        public override void GetHardwareGainRange(out double min, out double max)
        {
            _mvCamera.getGain(out double v, out double fmin, out double fmax);
            min = fmin;
            max = fmax;
        }
        public override double ExposureTime
        {
            get
            {
                return _mvCamera.ExposureTime;
            }
            set
            {
                _mvCamera.ExposureTime = value;
            }
        }
        public override double HardwareGain
        {
            get
            {
                return _mvCamera.Gain;
            }
            set
            {
                _mvCamera.Gain = value;
            }
        }
        #endregion
    }
}
