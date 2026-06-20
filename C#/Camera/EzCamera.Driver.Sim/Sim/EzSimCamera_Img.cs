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
using JetEazy.EzImage;
using JetEazy.OpenCV;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using RESOURCES = EzCamera.Driver.Sim.Properties.Resources;


namespace EzCamera.Driver.Sim
{
    /// <summary>
    /// 模擬相機 (使用圖檔)
    /// </summary>
    public class EzSimImageCamera : EzAbstractCamera
    {
        #region CONFIGS
        public bool OPT_AUTO_CIRCULATED = false;
        public bool OPT_SHOW_TIME_STAMP = true;
        public bool OPT_AUTO_FPS = true;
        #endregion

        #region FILES
        EzFileUtil _fileUtil = new EzFileUtil("Sim Image", ".jpg", ".png", ".bmp");
        #endregion

        #region DEFAULT
        public static EzCameraDeviceInfo DefaultDeviceInfo(int camID)
        {
            return new EzCameraDeviceInfo("Sim", "模擬相機", index: camID);
        }
        #endregion

        #region PRIVATE_DATA
        Bitmap _simBmp = null;
        Font _font = null;
        #endregion

        public EzSimImageCamera(int camID)
            : this(DefaultDeviceInfo(camID))
        {
        }
        public EzSimImageCamera(IEzDeviceInfo info)
            : base(info)
        {
            MaxFps = 30;

            loadIni();

            var fileName = _fileUtil.ActiveFileName;
            if (System.IO.File.Exists(fileName))
                Browse(fileName);
            else
                Browse("[DEFAULT]");
        }

        public override bool IsSimulation()
        {
            return true;
        }
        public override string Browse(string filePath)
        {
            var lastFileName = _fileUtil.ActiveFileName;

            if (filePath == EzFileUtil.CMD_LAST_FILE)
                return lastFileName;

            bool isLiveMode = IsLiveMode();
            if (isLiveMode)
                StopLiveMode();

            try
            {
                if (filePath != EzFileUtil.CMD_DEFAULT_FILE)
                    filePath = _fileUtil.Browse(filePath);

                bool isDirty = (filePath != lastFileName || _simBmp == null);

                if (filePath != null && isDirty)
                {
                    Bitmap bmp = loadBmp(filePath);

                    if (lastFileName != filePath)
                    {
                        //lastFileName = filePath;
                        saveIni();
                    }

                    var old = _simBmp;
                    _simBmp = bmp;
                    old?.Dispose();

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
                // 重新紀錄 _camImgInfo
                _camImageInfo = EzImageInfo.FromBitmap(_simBmp);

                // 重新設定字型大小
                adjustFont(_simBmp.Height);

                // 自動調整 MaxFps
                if (OPT_AUTO_FPS)
                    autoAdjustMaxFps(_simBmp.Width, _simBmp.Height);

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

        #region PROTECTED_OVERRIDES
        protected override EzImageInfo drvInit()
        {
            if (_simBmp == null)
            {
                //Browse("[DEFAULT]");
                _simBmp = loadBmp("[DEFAULT]");
            }
            var imgInfo = EzImageInfo.FromBitmap(_simBmp);
            return imgInfo;
        }
        protected override void drvDispose()
        {
            _simBmp?.Dispose();
            _simBmp = null;
            _font?.Dispose();
            _font = null;
        }
        protected override Bitmap drvCaptureImage(object args = null)
        {
            _SIM_ERROR();

            if (_simBmp != null)
            {
                var bmp = (Bitmap)_simBmp.Clone();

                if (OPT_SHOW_TIME_STAMP)
                    drawTimeStamp(bmp);

                if (OPT_AUTO_CIRCULATED)
                    auto_load_next_image();

                return bmp;
            }

            return null;
        }
        void auto_load_next_image()
        {
            string file = _fileUtil.Browse("[NEXT_FILE]", silent: true);

            if (string.IsNullOrEmpty(file))
                return;

            var newBmp = loadBmp(file);
            if (newBmp != null)
            {
                var old = _simBmp;
                _simBmp = newBmp;
                bool isSizeChanged = old == null || old.Size != newBmp.Size;
                old?.Dispose();
                if (isSizeChanged)
                    fireDeviceInfoChanged();
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private Bitmap getDefaultBmpFromResource(int id)
        {
            Bitmap bmp = null;
            id = id % 3;
            switch (id)
            {
                case 2:
                    bmp = RESOURCES.SIM_2;
                    break;
                case 1:
                    bmp = RESOURCES.SIM_1;
                    break;
                case 0:
                default:
                    bmp = RESOURCES.SIM_0;
                    break;
            }
            return bmp;
        }
        private Bitmap loadBmp(string srcFileName)
        {
            Bitmap newBmp = null;

            try
            {
                bool isInverse = srcFileName.Contains("INV_");

                if (srcFileName == "[DEFAULT]")
                {
                    newBmp = (Bitmap)getDefaultBmpFromResource(CamID).Clone();
                }
                else
                {
                    #region SEARH_AVALABLE_IMG_FILES            
                    //if (!System.IO.File.Exists(srcFileName))
                    //{
                    //    string fileFound = null;
                    //    string fileSearh = srcFileName;
                    //    string path = System.IO.Path.GetDirectoryName(fileSearh);
                    //    if (System.IO.Directory.Exists(path))
                    //    {
                    //        string stem = System.IO.Path.GetFileNameWithoutExtension(fileSearh);
                    //        foreach (var extN in IMG_EXTS)
                    //        {
                    //            fileSearh = System.IO.Path.Combine(path, stem + extN);
                    //            if (System.IO.File.Exists(fileSearh))
                    //            {
                    //                fileFound = fileSearh;
                    //                break;
                    //            }
                    //        }
                    //    }
                    //    if (fileFound == null)
                    //    {
                    //        throw new ApplicationException("找不到模擬檔案: " + srcFileName);
                    //    }
                    //    srcFileName = fileFound;
                    //}
                    #endregion

                    srcFileName = _fileUtil.GetSafeAvailableFile(srcFileName);

                    newBmp = loadLargeImage(srcFileName);
                }

                newBmp = apply_color_transform(newBmp);

                if (srcFileName.Contains("INV_"))
                    _imgProcess.ApplyInverse(newBmp);

                return newBmp;
            }
            catch (Exception ex)
            {
                newBmp?.Dispose();
                //>>> throw new ApplicationException("無法載入: " + srcFileName);
                throw ex;
            }
        }
        private Bitmap loadLargeImage(string fileName)
        {
            _TRACE($"[Loading] {fileName}");

            try
            {
                var tm0 = DateTime.Now;

                //Bitmap bmp = null;
                //using (IEzImage image = new EzFreeBitmap())
                //{
                //    image.Load(fileName);
                //    Bitmap src = image.Bitmap;
                //    bmp = src != null ? (Bitmap)src.Clone() : null;
                //}

                Bitmap bmp = EzImageUtil.LoadBigImage(fileName, toU8: false);

                var ts = DateTime.Now - tm0;

                if (bmp != null)
                {
                    _TRACE($"[LoadImage] = {(int)ts.TotalMilliseconds} ms");
                    return bmp;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                throw ex;
            }

            _TRACE("[LoadImage] Failed!");
            return null;
        }
        private void adjustFont(int imgHeight)
        {
            // 重新設定字型大小
            float emSize = Math.Max(16, imgHeight / 32f);
            _font?.Dispose();
            _font = new Font("Ariel", emSize);
        }
        private void autoAdjustMaxFps(int imgWidth, int imgHeight)
        {
            var size = imgWidth * imgHeight;
            if (size <= 1600 * 1200)
                MaxFps = 60;
            else if (size <= 2456 * 2058)
                MaxFps = 30;
            else if (size <= 4032 * 3036)
                MaxFps = 16;
            else
                MaxFps = 8;
        }
        private void drawTimeStamp(Bitmap bmp)
        {
            if (true || bmp.Width * bmp.Height <= 1600 * 1200)
            {
                // TIME STAMP
                string timeStamp = DateTime.Now.ToString("HH:mm:ss.fff");

                // 目前 8bpp 只能用 OpenCV 畫字
                if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    using (var bridge = new QxImageBridge(bmp))
                    {
                        bridge.Image.PutText(timeStamp,
                                        new OpenCvSharp.Point(30, 100),
                                        OpenCvSharp.HersheyFonts.HersheyComplex,
                                        3.0, new OpenCvSharp.Scalar(255, 255, 255), 3,
                                        bottomLeftOrigin: false
                                    );
                    }
                }
                else
                {
                    using (var gx = Graphics.FromImage(bmp))
                    {
                        gx.DrawString(timeStamp, _font, Brushes.Lime, 3f, 3f);
                    }
                }
            }
            else
            {
                // 可能記憶體不足
            }
        }
        private void _TRACE(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
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

        #region 模擬異常
        EzCameraError _simError = null; // new EzCameraError(-1, "Timeout", "模擬異常");
        private void _SIM_ERROR()
        {
            if (_simError != null)
            {
                // 模擬 3秒後 發出異常.
                asyncFireSimError(_simError, 3000);
                _simError = null;
            }
        }
        private void asyncFireSimError(EzCameraError err, int delay)
        {
            new Action<EzCameraError, int>((e, t) =>
            {
                System.Threading.Thread.Sleep(t);
                fireErrorEvent(e);

                if (!EzCameraError.IsNoError(e))
                {
                    // 模擬 5秒後 異常自動消失.
                    asyncFireSimError(null, 5000);
                }

            }).BeginInvoke(err, delay, null, null);
        }
        #endregion
    }
}
