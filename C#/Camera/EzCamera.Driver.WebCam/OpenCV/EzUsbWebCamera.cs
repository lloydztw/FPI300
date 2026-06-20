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
using OpenCvSharp;
using System.Drawing;

namespace EzCamera.Driver.WebCam.CV
{
    /// <summary>
    /// 常規標準 USB Web Camera
    /// </summary>
    public class EzUsbWebCamera : EzAbstractCamera
    {
        #region DEFAULT
        public static EzCameraDeviceInfo DefaultDeviceInfo(int camID)
        {
            return new EzCameraDeviceInfo("WebCam", "常規USB相機", index: camID);
        }
        #endregion

        #region PRIVATE_DATA
        private VideoCapture _videoCapture;
        private EzImageInfo _orgImgInfo;
        #endregion

        internal EzUsbWebCamera(int camID) 
            : this(DefaultDeviceInfo(camID))
        {
        }
        internal EzUsbWebCamera(IEzDeviceInfo info) 
            : base(info)
        {
        }

        #region PROTECTED_OVERRIDES
        protected override EzImageInfo drvInit()
        {
            int openCvIndex = DeviceInfo.Index;

            switch (DeviceInfo.VendorName)
            {
                case "USB2.0 Camera RGB":
                    // NOTE: 新的 3D 相機
                    instance_video_capture(openCvIndex, VideoCaptureAPIs.DSHOW);
                    _videoCapture.Set(VideoCaptureProperties.FourCC, VideoWriter.FourCC('M', 'J', 'P', 'G'));
                    _videoCapture.Set(VideoCaptureProperties.FrameWidth, 1280 * 2);
                    _videoCapture.Set(VideoCaptureProperties.FrameHeight, 800);
                    _videoCapture.Set(VideoCaptureProperties.Fps, 60);
                    dump_properties("設定值");
                    break;
                case "KS4A418":
                    // NOTE: FPS 只能到 2 ~ 3
                    instance_video_capture(openCvIndex, VideoCaptureAPIs.DSHOW);
                    _videoCapture.Set(VideoCaptureProperties.FrameWidth, 1920 * 2);
                    _videoCapture.Set(VideoCaptureProperties.FrameHeight, 1080);
                    _videoCapture.Set(VideoCaptureProperties.Fps, 30);
                    dump_properties("設定值");
                    break;
                case "HD Webcam":
                    instance_video_capture(openCvIndex, VideoCaptureAPIs.DSHOW);
                    //_videoCapture.Set(VideoCaptureProperties.FourCC, VideoWriter.FourCC('M', 'J', 'P', 'G'));
                    _videoCapture.Set(VideoCaptureProperties.FourCC, VideoWriter.FourCC('N', 'V', '1', '2'));
                    _videoCapture.Set(VideoCaptureProperties.FrameWidth, 1280);
                    _videoCapture.Set(VideoCaptureProperties.FrameHeight, 720);
                    _videoCapture.Set(VideoCaptureProperties.Fps, 60);
                    dump_properties("設定值");
                    break;
                default:
                    break;
            }

            using (Mat img = new Mat())
            {
                _videoCapture.Read(img);
                var imgInfo = EzImageInfo.FromMat(img);
                _orgImgInfo = EzImageInfo.FromMat(img);
                return imgInfo;
            }
        }
        protected override void drvDispose()
        {
            _videoCapture?.Release();
            _videoCapture?.Dispose();
            _videoCapture = null;
        }
        protected override Bitmap drvCaptureImage(object args = null)
        {
            if (_videoCapture == null)
                return null;

            bool ok;

            Bitmap buf = new Bitmap(_orgImgInfo.Width, _orgImgInfo.Height, _orgImgInfo.PixelFormat);

            //將 buf (C# Bitmap) 橋接到 OpenCV 的 Image (Mat)
            using (var bridge = new QxImageBridge(buf))
            {
                // _videoCapture 會把影像 抓到 Image (Mat) 內
                ok = _videoCapture.Read(bridge.Image);
            }

            if (!ok)
            {
                buf?.Dispose();
                return null;
            }

            return buf;
        }
        #endregion

        /// <summary>
        /// Web Camera 不見得會支援 Hardware ExposureTime!
        /// 在此還是準備了覆寫的範例代碼
        /// </summary>
        public override double ExposureTime
        {
            get
            {
                //double value;
                //if (_videoCapture != null)
                //    value = _videoCapture.Get(VideoCaptureProperties.Exposure);
                return base.ExposureTime;
            }
            set
            {
                base.ExposureTime = value;
                if (_videoCapture != null)
                    _videoCapture.Set(VideoCaptureProperties.Exposure, value);
            }
        }

        /// <summary>
        /// Web Camera 不見得會支援 Hardware Gain!
        /// 在此還是準備了覆寫的範例代碼
        /// </summary>
        public override double HardwareGain
        {
            get
            {
                //double value;
                //if (_videoCapture != null)
                //    value = _videoCapture.Get(VideoCaptureProperties.Exposure);
                return base.HardwareGain;
            }
            set
            {
                base.HardwareGain = value;
                if (_videoCapture != null)
                    _videoCapture.Set(VideoCaptureProperties.Gain, value);
            }
        }

        private void instance_video_capture(int openCvIndex, VideoCaptureAPIs api)
        {
            if (_videoCapture == null)
            {
                //var vp = new VideoCapturePara(VideoAccelerationType.D3D11, 0);
                //_videoCapture = new VideoCapture(openCvIndex, api, vp);
                _videoCapture = new VideoCapture(openCvIndex, api);
                dump_properties("初始值");
            }
        }
        private void dump_properties(string tag)
        {
            var fps = _videoCapture.Get(VideoCaptureProperties.Fps);
            var width = _videoCapture.Get(VideoCaptureProperties.FrameWidth);
            var height = _videoCapture.Get(VideoCaptureProperties.FrameHeight);
            var fourCC = _videoCapture.Get(VideoCaptureProperties.FourCC);
            string fourCCString = formatFourCC(_videoCapture);

            _LOG.Trace("[WebCam] {0}", tag);
            _LOG.Trace("[WebCam] vendor = {0}", this.DeviceInfo.VendorName);
            _LOG.Trace("[WebCam] fps = {0:0.0}", fps);
            _LOG.Trace("[WebCam] width = {0}", width);
            _LOG.Trace("[WebCam] height = {0}", height);
            _LOG.Trace("[WebCam] fourCC = {0}", fourCCString);
        }

        #region PRIVATE_FUNCTIONS
        private string formatFourCC(VideoCapture capture)
        {
            double fourccValue = capture.Get(VideoCaptureProperties.FourCC);
            int fourccInt = (int)fourccValue;

            var char1 = formatOneFourCC(fourccInt & 0xFF);
            var char2 = formatOneFourCC((fourccInt >> 8) & 0xFF);
            var char3 = formatOneFourCC((fourccInt >> 16) & 0xFF);
            var char4 = formatOneFourCC((fourccInt >> 24) & 0xFF);

            //string fourccString = string.Format(
            //    "{0}{1}{2}{3}", c
            //    isAscii(char1),
            //    isAscii(char2),
            //    isAscii(char3) ? char3 : '?',
            //    isAscii(char4) ? char4 : '?'
            //);

            return char1 + char2 + char3 + char4;
        }
        private string formatOneFourCC(int code)
        {
            if(isAscii((char)code))
                return ((char)code).ToString();
            return $"{code:X2} ";
        }
        private bool isAscii(char c)
        {
            return (c >= 32 && c <= 126);
        }
        #endregion
    }
}
