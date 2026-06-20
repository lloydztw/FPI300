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
using System;
using System.Drawing;

namespace EzCamera.Driver.Sim
{
    public class EzSimVideoCamera : EzAbstractCamera
    {
        #region FILEs
        private EzFileUtil _fileUtil = new EzFileUtil("Sim Video", ".mp4", ".avi", ".wmv");
        #endregion

        #region DEFAULT
        public static EzCameraDeviceInfo DefaultDeviceInfo(int camID)
        {
            return new EzCameraDeviceInfo("SimVideo", "模擬相機(視頻)", index: camID);
        }
        #endregion

        #region PRIVATE_DATA
        private VideoCapture _videoCapture;
        private EzImageInfo _orgImgInfo;
        #endregion

        public EzSimVideoCamera(int camID) 
            : this(DefaultDeviceInfo(camID))
        {
        }
        internal EzSimVideoCamera(IEzDeviceInfo info) 
            : base(info)
        {
            loadIni();
        }

        public override bool IsSimulation()
        {
            return true;
        }
        public override string Browse(string filePath)
        {
            string lastFileName = _fileUtil.ActiveFileName;

            if (filePath == EzFileUtil.CMD_LAST_FILE)
                return lastFileName;

            bool isLiveMode = IsLiveMode();
            if (isLiveMode)
                StopLiveMode();

            try
            {
                // 使用 EzFileUtil 選擇檔案
                filePath = _fileUtil.Browse(filePath);

                bool isDirty = (filePath != lastFileName || _videoCapture == null);

                if (filePath != null && isDirty)
                {
                    filePath = _fileUtil.GetSafeAvailableFile(filePath);

                    _videoCapture?.Release();
                    _videoCapture?.Dispose();
                    _videoCapture = new VideoCapture(filePath);

                    if (lastFileName != filePath)
                    {
                        saveIni();
                    }

                    sync_image_info();
                    dump_properties();

                    fireDeviceInfoChanged();
                }
            }
            catch (Exception ex)
            {
                var err = new EzCameraError(-1, "LoadSimFileFailed", $"模擬檔案載入失敗 @ {filePath}");
                fireErrorEvent(err, ex);
                return null;
            }

            try
            {
                //// 重新紀錄 _camImgInfo
                //_camImageInfo = EzImageInfo.FromBitmap(_simBmp);

                //// 重新設定字型大小
                //adjustFont(_simBmp.Height);

                //// 自動調整 MaxFps
                //if (OPT_AUTO_FPS)
                //    autoAdjustMaxFps(_simBmp.Width, _simBmp.Height);

                // 自動轉換狀態
                if (isLiveMode)
                    StartLiveMode();
                else
                    TriggerOneFrame();

                //fireDeviceInfoChanged();
                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override void StopLiveMode()
        {
            base.StopLiveMode();
            System.Threading.Thread.Sleep(500);
        }

        #region PROTECTED_OVERRIDES
        protected override EzImageInfo drvInit()
        {
            if (_videoCapture == null)
            {
                Browse(_fileUtil.ActiveFileName);
            }
            else
            {
                sync_image_info();
            }
            return _orgImgInfo;
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

        #region PRIVATE_FUNCTIONS
        private EzImageInfo sync_image_info()
        {
            using (Mat img = new Mat())
            {
                _videoCapture.Read(img);
                var imgInfo = EzImageInfo.FromMat(img);
                _orgImgInfo = EzImageInfo.FromMat(img);
                return imgInfo;
            }
        }
        private void dump_properties()
        {
            var fps = _videoCapture.Get(VideoCaptureProperties.Fps);
            var width = _videoCapture.Get(VideoCaptureProperties.FrameWidth);
            var height = _videoCapture.Get(VideoCaptureProperties.FrameHeight);
            var fourCC = _videoCapture.Get(VideoCaptureProperties.FourCC);
            string fourCCString = string.Format("{0}{1}{2}{3}",
                (char)((int)fourCC & 0xFF),
                (char)(((int)fourCC >> 8) & 0xFF),
                (char)(((int)fourCC >> 16) & 0xFF),
                (char)(((int)fourCC >> 24) & 0xFF)
            );
            _LOG.Trace("[SimCam] vendor = {0}", this.DeviceInfo.VendorName);
            _LOG.Trace("[SimCam] fps = {0}", fps);
            _LOG.Trace("[SimCam] width = {0}", width);
            _LOG.Trace("[SimCam] height = {0}", height);
            _LOG.Trace("[SimCam] fourCC = {0}", fourCCString);
        }
        #endregion

        #region PRIVATE_INI_FUNCTIONS
        private void loadIni()
        {
            _fileUtil.LoadIni(null, $"SimCamera{CamID}");
        }
        private void saveIni()
        {
            _fileUtil.SaveIni(null, $"SimCamera{CamID}");
        }
        #endregion
    }
}
