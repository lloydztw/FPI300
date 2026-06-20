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

using EzCamera.Driver.Base;
using EzCamera.Driver.Utils;
using EzCamera.Interface;
using JetEazy.OpenCV;
using MvCamCtrl.NET;
using OpenCvSharp;
using System.Drawing;
using System.Drawing.Imaging;

namespace EzCamera.Driver.Hikvision
{
    /// <summary>
    /// 海康威視對 IEzCamera 的實作
    /// </summary>
    public partial class EzHikvCamera : EzAbstractCamera
    {
        #region DEFAULT
        public static EzCameraDeviceInfo DefaultDeviceInfo(int camID)
        {
            return new EzCameraDeviceInfo("MV", "海康威視", index: camID);
        }
        #endregion

        #region PRIVATE_HIK_核心成員
        /// <summary>
        /// HikvCameraH 是對原生 MvCamera 的簡易包裝, 
        /// 來提供: 
        /// <br/> (1) Callbacks, FrameBuffer 管理
        /// <br/> (2) Construct, Open, Close 流程控制
        /// <br/> (3) TriggerSource, TriggerMode, AcquisitionMode 設定
        /// <br/> (4) Camera Properties 讀(get)寫(set) 與 cache.
        /// </summary>
        private HikvCameraH _mvCamera;
        private MyCamera.MV_CC_DEVICE_INFO _mvDeviceInfo;
        #endregion

        internal EzHikvCamera(IEzDeviceInfo info)
            : base(info)
        {
            _mvDeviceInfo = (MyCamera.MV_CC_DEVICE_INFO)info.Tag;
            _mvCamera = new HikvCameraH();
            _mvCamera.OnError += _hikvCamera_OnError;
            //>>> _mvCamera.OnFrameCaptured += _hikvCamera_OnFrameCaptured;
        }

        #region PROTECTED_OVERRIDES
        protected override EzImageInfo drvInit()
        {
            int ret = _mvCamera.ConstructDevice(ref _mvDeviceInfo);
            if (ret != MyCamera.MV_OK)
                return null;

            // 是否要需要拍一張圖來取得資訊?
            _mvCamera.getWidth(out uint width, out uint minW, out uint maxW);
            _mvCamera.getHeight(out uint height, out uint minH, out uint maxH);
            _mvCamera.getEnumValue("PixelFormat", out uint pixelFormat);
            System.Diagnostics.Debug.WriteLine($"PixelFormat = {pixelFormat}");

            var imgInfo = new EzImageInfo()
            {
                Width = (int)width,
                Height = (int)height,
                BitsPerPixel = 24
            };

            if (!_mvCamera.IsGrabbing())
                _mvCamera.StartGrabbing();

            // 目前暫時只支援
            //  TriggerSource: Software
            //  TriggerMode:   ON
            System.Diagnostics.Debug.Assert(_mvCamera.TriggerSource == MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
            System.Diagnostics.Debug.Assert(_mvCamera.TriggerMode == MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
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
            var bmp = convert_color_space(frame, true);
            return bmp;
        }
        #endregion

        #region INTERNAL_EVENT_HANDLERS
        private void _hikvCamera_OnError(object sender, HikvError e)
        {
            fireErrorEvent(e);
        }
        private void _hikvCamera_OnFrameCaptured(object sender, EzLiveImageEventArgs e)
        {
            //_capturedBmp?.Dispose();
            //_capturedBmp = clone_and_convert_color((Bitmap)e.LiveImage);
            //_captured.Set();
        }
        #endregion

        #region COLOR_SPACE_CONVERT_FOR_HIKV
        private Bitmap convert_color_space(Bitmap bmp, bool alwaysClone = true)
        {
            if (bmp == null)
                return null;

            // Hikvision BGR -> RGB
            if (bmp.PixelFormat == PixelFormat.Format24bppRgb)
            {
                using (var bridge = new QxImageBridge(bmp))
                {
                    var img = bridge.Image;
                    Cv2.CvtColor(img, img, ColorConversionCodes.RGB2BGR);
                }
                return alwaysClone ? (Bitmap)bmp.Clone() : bmp;
            }
            else
            {
                // 測試: 強制把其他格式都統一轉為 RGB 24 bit
                // (SLOW WAY)
                var bmpNew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);
                using (var dst = new QxImageBridge(bmpNew))
                using (var src = new QxImageBridge(bmp))
                {
                    ColorConversionCodes convertCode;
                    int channels = src.Image.Channels();
                    switch (channels)
                    {
                        case 1: convertCode = ColorConversionCodes.GRAY2RGB; break;
                        case 2: convertCode = ColorConversionCodes.BGR5652BGR; break;
                        case 4: convertCode = ColorConversionCodes.RGBA2RGB; break;
                        default: return null;
                    }
                    Cv2.CvtColor(src.Image, dst.Image, convertCode);
                }
                return bmpNew;
            }
        }
        #endregion

        #region CAMERA_PROPERTIES
        public override void GetExposureRange(out double min, out double max)
        {
            _mvCamera.getExposureTime(out float v, out float fmin, out float fmax);
            min = fmin; 
            max = fmax;
        }
        public override void GetHardwareGainRange(out double min, out double max)
        {
            _mvCamera.getGain(out float v, out float fmin, out float fmax);
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
                _mvCamera.ExposureTime = (float)value;
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
                _mvCamera.Gain = (float)value;
            }
        }
        #endregion
    }
}
