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
    /// 度申 對 IEzCamera 的實作
    /// </summary>
    public partial class EzDvpCamera : EzAbstractCamera
    {
        #region DEFAULT
        public static EzCameraDeviceInfo DefaultDeviceInfo(int camID)
        {
            return new EzCameraDeviceInfo("DVP", "度申", index: camID);
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
        #endregion

        internal EzDvpCamera(IEzDeviceInfo info)
            : base(info)
        {
            _mvDeviceInfo = (VendorDeviceInfo)info.Tag;
            _mvCamera = new VendorCamera();
            _mvCamera.OnError += _venderCamera_OnError;
            //>>> _mvCamera.OnFrameCaptured += _hikvCamera_OnFrameCaptured;
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

            // 目前暫時只支援
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
            var bmp = frame != null ? (Bitmap)frame.Clone() : null;
            return bmp;
        }
        #endregion

        #region INTERNAL_EVENT_HANDLERS
        private void _venderCamera_OnError(object sender, VendorError e)
        {
            fireErrorEvent(e);
        }
        private void _vendorCamera_OnFrameCaptured(object sender, EzLiveImageEventArgs e)
        {
            // 保留
            //_capturedBmp?.Dispose();
            //_capturedBmp = clone_and_convert_color((Bitmap)e.LiveImage);
            //_captured.Set();
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
