#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-07-06 全面改用 DirectShow (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using DirectShowLib;
using EzCamera.Driver.Base;
using EzCamera.Driver.Utils;
using EzCamera.Interface;
using JetEazy.DShow;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace EzCamera.Driver.DShow
{
    using DsCamera = Camera_NET.Camera;

    public class EzDshowCamera : EzCameraProps, IEzCamera, ISampleGrabberCB
    {
        public event EventHandler<EzLiveImageEventArgs> OnLiveImage;
        public event EventHandler<EzCameraErrorEventArgs> OnError;
        public event EventHandler OnLiveModeChanged;

        #region DEFAULT
        public static EzCameraDeviceInfo DefaultDeviceInfo(int dsCamIndex)
        {
            return new EzCameraDeviceInfo("WebCam", "常規 DirectShow 相機", index: dsCamIndex);
        }
        #endregion

        #region NLOG
        static NLog.Logger _singleton = null;
        static NLog.Logger _LOG
        {
            get
            {
                if (_singleton == null)
                    _singleton = NLog.LogManager.GetCurrentClassLogger();
                return _singleton;
            }
        }
        #endregion

        #region PRIVATE_KERNEL_DATA
        DsCamera _imp;
        Control _dummyWnd;
        Func<double, IntPtr, int, int> _bufferCallback;
        #endregion

        #region PRIVATE_RUNTIME_DATA
        bool _bypassCallback = false;
        #endregion

        public EzDshowCamera(IEzDeviceInfo info)
        {
            _deviceInfo = info;
            _createDsCamera(info, out DsCamera cam, out _dummyWnd);
            _attach(cam);
        }

        #region PRIVATE_CREATION_FUNCTIONS
        void _createDsCamera(IEzDeviceInfo info, out DsCamera dsCamera, out Control dummyDisplayWnd)
        {
            var uniqueName = info.VendorName;
            var factory = DshowCameraFactory.Instance;
            dummyDisplayWnd = _createDummyWindow();
            dsCamera = factory.OpenCamera(dummyDisplayWnd, uniqueName);
        }
        void _attach(DsCamera imp)
        {
            _imp = imp;
            if (_imp != null)
            {
                _prepareCallback("RGB24");
                _imp.SetExtraBufferCB(this);
                //updateDefaultEzCameraProperties();
            }
            this.GlobalCamID = AllCreatedCameras.Register(this) - 1;
        }
        #endregion

        #region DUMMY_DISPLAY_WINDOW_FUNCTIONS
        Control _createDummyWindow()
        {
            var wnd = new Panel();
            wnd.Visible = false;
            var forms = Application.OpenForms;
            if (forms == null && forms.Count > 0)
            {
                forms[0].Controls.Add(wnd);
                wnd.Parent = forms[0];
            }
            _LOG.Debug("_createDummyWindow: wnd={0} , parent={1}", wnd.Handle, wnd.Parent);
            return wnd;
        }
        void _disposeDummpyWindow(Control wnd)
        {
            if (wnd != null && wnd.Parent == null)
                wnd.Dispose();
        }
        #endregion

        public void Dispose()
        {
            AllCreatedCameras.Unregister(this);
            _imp?.Dispose();
            _imp = null;
            _disposeDummpyWindow(_dummyWnd);
            _dummyWnd = null;
        }

        public DsCamera GetNativeDshowCamera()
        {
            return _imp;
        }
        public bool ShowPinConfigDlg(IntPtr hwndOwner)
        {
            if (_imp == null)
                return false;

            _bypassCallback = true;

            var oldFourCC = _imp.FourCC;
            var oldSize = _imp.TargetFrameSize;
            var oldRate = _imp.TargetFrameRate;
            
            _imp.ShowPinConfigDlg(hwndOwner);
            
            bool isChanged = oldSize != _imp.TargetFrameSize || 
                             oldRate != _imp.TargetFrameRate ||
                             oldFourCC != _imp.FourCC;

            if (isChanged)
            {
                _LOG.Debug("ShowPinConfig : frameSize={0}, frameRate={1}, fourCC={2}", _imp.TargetFrameSize, _imp.TargetFrameRate, _imp.FourCC);
            }

            _bypassCallback = false;

            return isChanged;
        }
        public void ShowCameraCtrlDlg(IntPtr hwndOwner)
        {
            if (_imp == null)
                return;

            _bypassCallback = true;

            _imp?.ShowCameraCtrlDlg(hwndOwner);

            _bypassCallback = false;
        }

        int ISampleGrabberCB.SampleCB(double SampleTime, IMediaSample pSample)
        {
            throw new NotImplementedException();
        }
        int ISampleGrabberCB.BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            if (_bypassCallback || !((IEzCamera)this).IsLiveMode())
                return 0;

            return _bufferCallback != null ? _bufferCallback(SampleTime, pBuffer, BufferLen) : 0;
        }
        void fire_OnLiveImage(Bitmap bmp, bool bypass_img_processing = false)
        {
            if (OnLiveImage != null)
            {
                if (!bypass_img_processing)
                    bmp = apply_internal_image_process(bmp);

                OnLiveImage?.Invoke(this, new EzLiveImageEventArgs(bmp));
            }
        }

        #region DSHOW_CALLBACKS
        void _prepareCallback(string fourCC)
        {
            _LOG.Debug("_prepareCallback : fourCC = {0}", fourCC);

            if (string.IsNullOrEmpty(fourCC))
                _bufferCallback = BufferCB_Dummy;

            switch (fourCC.ToUpper())
            {
                case "MJPG":
                    _bufferCallback = BufferCB_MJPG;
                    break;
                case "NV2":
                    _bufferCallback = BufferCB_NV2;
                    break;
                case "YUY2":
                    _bufferCallback = BufferCB_YUY2;
                    break;
                case "RGB24":
                    _bufferCallback = BufferCB_RGB24;
                    break;
                default:
                    _bufferCallback = BufferCB_Dummy;
                    break;
            }
        }
        int BufferCB_NV2(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            // NV12 是一種 YUV420 半平面格式：
            // 前 width *height bytes 是 Y plane（亮度）
            // 後 width *height / 2 bytes 是 UV interleaved（U 和 V 每兩個像素共用）

            if (_imp == null || OnLiveImage == null)
                return 0;

            var frameSize = _imp.TargetFrameSize;
            if (frameSize.Width < 2 || frameSize.Height < 2)
                return 0;

            using (Bitmap bmp = new Bitmap(frameSize.Width, frameSize.Height, PixelFormat.Format32bppArgb))
            {
                unsafe
                {
                    BitmapData bmpd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                    //_copyNV12to24(pBuffer, bmpd, ref m_bufRect);
                    _convertNV12ToRGB24((byte*)pBuffer.ToPointer(), bmpd, bmpd.Width, bmpd.Height);
                    bmp.UnlockBits(bmpd);

                }
                //OnLiveImage?.Invoke(this, new EzLiveImageEventArgs(bmp));
                fire_OnLiveImage(bmp);
            }

            return 0;
        }
        int BufferCB_YUY2(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            //lock (m_lockBuf)    //>>>@ BufferCB_LogiTech
            //{
            //    if (m_imgBuf32 == null)
            //        return 0;

            //    if (m_videoInfoHeader != null)
            //    {
            //        unsafe
            //        {
            //            BitmapData bmpd = m_imgBuf32.LockBits();

            //            _copyYUY2to32(pBuffer, bmpd, ref m_rectVideo);

            //            m_imgBuf32.UnlockBits(bmpd);

            //            if (m_onLiveImageCallback != null)
            //            {
            //                m_onLiveImageCallback(m_imgBuf32, m_rectVideo, true);
            //            }
            //        }
            //    }
            //}
            return 0;
        }
        int BufferCB_RGB24(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            if (_imp == null || OnLiveImage == null)
                return 0;

            var frameSize = _imp.TargetFrameSize;
            if (frameSize.Width < 2 || frameSize.Height < 2)
                return 0;

            System.Diagnostics.Debug.Assert(BufferLen == frameSize.Width * frameSize.Height * 3);

            try
            {
                // RGB24 格式每個像素有 3 個位元組（R、G、B），沒有 Alpha 通道。
                // 由於 RGB24 是未壓縮的，我們可以直接從 pBuffer 建立 Bitmap。

                // 計算掃描線的步幅 (stride)。
                // 對於 RGB24，步幅通常是 Width * 3。
                // 但如果影像資料有對齊要求，可能會是 Width * 3 的倍數，
                // 通常是 4 位元組對齊。
                // 在此假設為緊密打包（packed），即 Width * 3。
                // 如果您遇到影像失真，請檢查您的來源是否使用了特定的步幅。

                int stride = frameSize.Width * 3;

                // 建立一個新的 Bitmap 物件。
                // 這個建構函式允許您直接從非受控記憶體創建 Bitmap，而無需先複製到受控記憶體。
                Bitmap bitmap = new Bitmap(
                    frameSize.Width,
                    frameSize.Height,
                    stride, // 每行位元組數 (Stride)
                    PixelFormat.Format24bppRgb, // 像素格式：24 位元組 RGB
                    pBuffer // 指向原始像素資料的 IntPtr
                );

                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

                // 注意：使用此建構函式創建的 Bitmap 會直接讀取 pBuffer 指向的記憶體。
                // 如果 pBuffer 指向的記憶體在 Bitmap 使用期間被釋放或修改，將會導致問題。
                // 如果您需要一個獨立的 Bitmap 副本，請在建立後立即複製它。
                // return (Bitmap)bitmap.Clone(); // 複製一份，這樣原始 pBuffer 的記憶體可以被釋放
                //return bitmap;

                fire_OnLiveImage(bitmap);

                bitmap?.Dispose();

                return 0;
            }
            catch (Exception ex)
            {
                _LOG.Error($"將 RGB24 pBuffer 轉換為 Bitmap 時發生錯誤: {ex.Message}");
                // 適當地處理例外狀況
                return 0;
            }
        }
        int BufferCB_MJPG(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            if (_imp == null || OnLiveImage == null)
                return 0;

            var frameSize = _imp.TargetFrameSize;
            if (frameSize.Width < 2 || frameSize.Height < 2)
                return 0;

            // 1. 把 MJPG 壓縮資料從 IntPtr 複製到 byte[]
            byte[] jpegData = new byte[BufferLen];
            Marshal.Copy(pBuffer, jpegData, 0, BufferLen);

            // 2. 使用 MemoryStream + Bitmap 解碼 JPEG
            using (var ms = new MemoryStream(jpegData))
            {
                try
                {
                    using (Bitmap srcBmp = new Bitmap(ms))
                    {
                        fire_OnLiveImage(srcBmp);
                    }
                }
                catch (Exception ex)
                {
                    _LOG.Error("JPEG 解碼失敗: " + ex.Message);
                    _dumpMemoryStreamToFile(ms, "d:\\tmp.jpg");
                    return 0;
                }
            }
            return 0;
        }
        int BufferCB_Dummy(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            return 0;
        }
        void _dumpMemoryStreamToFile(MemoryStream ms, string filePath)
        {
            if (ms == null)
            {
                _LOG.Error("MemoryStream 參考為 null，無法儲存。");
                return;
            }

            try
            {
                // 將 MemoryStream 的內容轉換為位元組陣列
                byte[] fileBytes = ms.ToArray();

                // 將位元組陣列寫入檔案
                File.WriteAllBytes(filePath, fileBytes);

                _LOG.Debug($"檔案已成功儲存至: {filePath}");
            }
            catch (Exception ex)
            {
                _LOG.Error($"儲存檔案時發生錯誤: {ex.Message}");
                // 處理例外狀況，例如權限不足、路徑無效等
            }
        }
        #endregion

        #region PRIVATE_BUFFER_CONVERT_FUNCTIONS
        /// <summary>
        /// 使用 unsafe 方式將 NV12 影像資料從 IntPtr 直接轉換到 BitmapData (RGB24)
        /// </summary>
        static void _copyNV12to24_000(IntPtr nv12Ptr, BitmapData bmpdTo)
        {
            if (nv12Ptr == IntPtr.Zero || bmpdTo == null)
                return;

            int width = bmpdTo.Width;
            int height = bmpdTo.Height;
            if (width < 2 || height < 2)
                return;

            // Y 平面每行的位元組數（通常是 width）
            int yStride = width;
            // UV 平面的實際 "寬度" 是 Y 寬度的一半
            int uvStride = yStride / 2;
            // RGB24 的行對齊寬度 (Stride)
            // BitmapData.Stride 已經考慮了 4 位元組對齊
            int rgbStride = bmpdTo.Stride;

            unsafe
            {
                // 創建 Bitmap，格式為 24bppRgb
                //using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
                {
                    //BitmapData bmpData = bmp.LockBits(
                    //    new Rectangle(0, 0, width, height),
                    //    ImageLockMode.WriteOnly,
                    //    PixelFormat.Format24bppRgb
                    //);

                    // 計算 NV12 Y 平面和 UV 平面的起始位置
                    byte* pNv12 = (byte*)nv12Ptr.ToPointer();
                    byte* pY = pNv12;
                    byte* pUV = pNv12 + (long)yStride * height; // Y 平面結束後就是 UV 平面

                    //// UV 平面的實際 "寬度" 是 Y 寬度的一半
                    //int uvStride = width / 2;

                    //// RGB24 的行對齊寬度 (Stride)
                    //// BitmapData.Stride 已經考慮了 4 位元組對齊
                    //int rgbStride = bmpdTo.Stride;

                    //// 固定點數優化 (乘數放大 256 倍)
                    //const int FIX_R_V = (int)(1.402 * 256);
                    //const int FIX_G_U = (int)(-0.344136 * 256);
                    //const int FIX_G_V = (int)(-0.714136 * 256);
                    //const int FIX_B_U = (int)(1.772 * 256);
                    //const int OFFSET_128 = 128;

                    // 遍歷每一個像素
                    for (int y = 0; y < height; y++)
                    {
                        // 獲取當前行在 BitmapData 中的指針
                        // 注意：Bitmap 默認是從下到上儲存的，所以需要 (height - 1 - y) 來反轉 Y 軸
                        byte* pRgbRow = (byte*)bmpdTo.Scan0 + (height - 1 - y) * rgbStride;

                        // 獲取當前行在 NV12 Y 平面中的指針
                        byte* pYRow = pY + y * yStride;

                        // 獲取當前行在 NV12 UV 平面中的指針 (注意：UV 是 2:1 採樣)
                        byte* pUvRow = pUV + (y / 2) * uvStride * 2; // uvStride * 2 是因為 U V 交錯

                        for (int x = 0; x < width; x++)
                        {
                            byte Y = pYRow[x];

                            // U 和 V 的索引 (注意 NV12 的 UV 交錯和 2:1 採樣)
                            byte U = pUvRow[(x / 2) * 2];     // U 在偶數位
                            byte V = pUvRow[(x / 2) * 2 + 1]; // V 在奇數位

                            //int C_U = U - OFFSET_128;
                            //int C_V = V - OFFSET_128;

                            //// 轉換公式
                            //int R = (Y * 256 + FIX_R_V * C_V) / 256;
                            //int G = (Y * 256 + FIX_G_U * C_U + FIX_G_V * C_V) / 256;
                            //int B = (Y * 256 + FIX_B_U * C_U) / 256;

                            //// 裁剪到 0-255
                            //R = Math.Min(255, Math.Max(0, R));
                            //G = Math.Min(255, Math.Max(0, G));
                            //B = Math.Min(255, Math.Max(0, B));

                            //// 將 RGB 值寫入 BitmapData (BGR 順序)
                            //pRgbRow[x * 3] = (byte)B;     // Blue
                            //pRgbRow[x * 3 + 1] = (byte)G; // Green
                            //pRgbRow[x * 3 + 2] = (byte)R; // Red.

                            _YUV2RGB(Y, U, V, out int R, out int G, out int B);
                            pRgbRow[x * 3] = (byte)B;     // Blue
                            pRgbRow[x * 3 + 1] = (byte)G; // Green
                            pRgbRow[x * 3 + 2] = (byte)R; // Red
                        }
                    }
                    //bmp.UnlockBits(bmpData);
                    //bmp.Save(filePath, ImageFormat.Bmp);
                    //Console.WriteLine($"Unsafe RGB24 frame saved to {filePath}");
                }
            }
        }
        static void _copyNV12to24(IntPtr nv12Ptr, BitmapData bmpdTo, ref Rectangle rect)
        {
            // NV12 是一種 YUV420 半平面格式：
            // 前 width *height bytes 是 Y plane（亮度）
            // 後 width *height / 2 bytes 是 UV interleaved（U 和 V 每兩個像素共用）

            if (nv12Ptr == IntPtr.Zero || bmpdTo == null)
                return;

            int srcBytesPerPixel = 1;
            int dstBytesPerPixel = 3;

            int width = bmpdTo.Width;
            int height = bmpdTo.Height;

            unsafe
            {
                byte* yPlane = (byte*)nv12Ptr;
                byte* uvPlane = (byte*)nv12Ptr + (width * height);
                byte* yLine = yPlane;
                int yStride = width;

                byte* dstLine = (byte*)bmpdTo.Scan0;
                int dstStride = bmpdTo.Stride;

                int R, G, B;

                for (int y = 0; y < height; y++)
                {
                    byte* yPix = yLine;
                    byte* dstPix = dstLine;

                    for (int x = 0; x < width; x++)
                    {
                        //int yIndex = y * width + x;
                        //int uvIndex = (y / 2) * width + (x / 2) * 2;

                        byte Y = yPix[0];

                        //byte U = uvPlane[uvIndex];
                        //byte V = uvPlane[uvIndex + 1];
                        //// YUV → RGB 轉換（BT.601）
                        //int C = Y - 16;
                        //int D = U - 128;
                        //int E = V - 128;

                        //int R = (298 * C + 409 * E + 128) >> 8;
                        //int G = (298 * C - 100 * D - 208 * E + 128) >> 8;
                        //int B = (298 * C + 516 * D + 128) >> 8;

                        R = Y;
                        G = Y;
                        B = Y;

                        R = Math.Min(Math.Max(R, 0), 255);
                        G = Math.Min(Math.Max(G, 0), 255);
                        B = Math.Min(Math.Max(B, 0), 255);

                        dstPix[0] = (byte)B;
                        dstPix[1] = (byte)G;
                        dstPix[2] = (byte)R;

                        dstPix += dstBytesPerPixel;
                        yPix += 1;
                    }

                    yLine += yStride;
                    dstLine += dstStride;
                }
            }
        }
        static void _copy24to32(BitmapData bmpdSrc, BitmapData bmpdTo, ref Rectangle rect)
        {
            const int iSrcBytesPerPixel = 3;
            const int iDstBytesPerPixel = 4;

            if (bmpdTo == null || bmpdSrc == null)
                return;

            unsafe
            {

                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // scan0 must pointer to the PixelFormat.Format32bppArgb 
                // Bitmap data
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                int x, xmax, xmin;
                int y, ymax, ymin;
                byte* pucPtr;
                byte* pucLine;

                xmin = rect.X;
                ymin = rect.Y;
                ymax = rect.Y + rect.Height;
                xmax = rect.X + rect.Width;

                int iStride = bmpdTo.Stride;
                pucLine = (byte*)bmpdTo.Scan0 + (ymin * iStride + xmin * iDstBytesPerPixel);


                int iStrideSrc = bmpdSrc.Stride;
                byte* pucLineSrc = (byte*)bmpdSrc.Scan0 + (ymin * iStrideSrc + xmin * iSrcBytesPerPixel);
                byte* pucPtrSrc;

                int iMinBytesPerPixel = Math.Min(iSrcBytesPerPixel, iDstBytesPerPixel);

                //uint uNewColor;

                for (y = ymin; y < ymax; y++)
                {
                    pucPtr = pucLine;
                    pucPtrSrc = pucLineSrc;

                    for (x = xmin; x < xmax; x++)
                    {
                        // pucPtr[0]: Blue
                        // pucPtr[1]: Green
                        // pucPtr[2]: Red
                        // pucPtr[3]: Alpha

                        int i = 0;
                        for (i = 0; i < iMinBytesPerPixel; i++)
                        {
                            pucPtr[i] = pucPtrSrc[i];

                            /*
                            if (pucPtrSrc[i] > iThreshold)
                                pucPtr[i] = 0xFF;
                            else
                                pucPtr[i] = 0x00;
                            */
                        }

                        for (; i < iDstBytesPerPixel; i++)
                            pucPtr[i] = 0xFF;

#if (OPT_LPC_CAMERA_UPMOST)
#else
                        int r = pucPtr[2] * 2;
                        pucPtr[0] = (byte)Math.Min(pucPtr[0] + r, 255);
                        pucPtr[1] = (byte)Math.Min(pucPtr[1] + r, 255);
                        pucPtr[2] = (byte)Math.Min(pucPtr[2] + r, 255);
#endif


                        pucPtr += iDstBytesPerPixel;
                        pucPtrSrc += iSrcBytesPerPixel;
                    }

                    pucLine += iStride;
                    pucLineSrc += iStrideSrc;
                }
            }
        }
        static void _copyYUY2to32(IntPtr pScanSrc, BitmapData bmpdTo, ref Rectangle rect)
        {
            const int iSrcBytesPerPixel = 2;
            const int iDstBytesPerPixel = 4;

            if (bmpdTo == null || pScanSrc == IntPtr.Zero)
                return;

            unsafe
            {

                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // scan0 must pointer to the PixelFormat.Format32bppArgb 
                // Bitmap data
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                int x, xmax, xmin;
                int y, ymax, ymin;
                byte* pucPtr;
                byte* pucLine;

                xmin = rect.X;
                ymin = rect.Y;
                ymax = rect.Y + rect.Height;
                xmax = rect.X + rect.Width;

                int iStride = bmpdTo.Stride;
                pucLine = (byte*)bmpdTo.Scan0 + (ymin * iStride + xmin * iDstBytesPerPixel);

                int iStrideSrc = rect.Width * iSrcBytesPerPixel * 2;
                byte* pucLineSrc = (byte*)pScanSrc + (ymin * iStrideSrc + xmin * iSrcBytesPerPixel);
                byte* pucPtrSrc = pucLineSrc;

                for (y = ymin; y < ymax; y++)
                {
                    pucPtr = pucLine;

#if (OPT_YUV_TO_COLOR || true)
                    for (x = xmin; x < xmax; x += 2)
                    {
                        // pucPtr[0]: Blue
                        // pucPtr[1]: Green
                        // pucPtr[2]: Red
                        // pucPtr[3]: Alpha

                        int r;
                        int g;
                        int b;
                        int Y0 = (int)(pucPtrSrc[0]);
                        int U0 = (int)(pucPtrSrc[1]);
                        int Y1 = (int)(pucPtrSrc[2]);
                        int V0 = (int)(pucPtrSrc[3]);

                        _YUV2RGB(Y0, U0, V0, out r, out g, out b);

                        pucPtr[0] = (byte)b;
                        pucPtr[1] = (byte)g;
                        pucPtr[2] = (byte)r;
                        pucPtr[3] = 0xFF;

                        _YUV2RGB(Y1, U0, V0, out r, out g, out b);

                        pucPtr[4] = (byte)b;
                        pucPtr[5] = (byte)g;
                        pucPtr[6] = (byte)r;
                        pucPtr[7] = 0xFF;

                        pucPtr += (iDstBytesPerPixel * 2);
                        pucPtrSrc += (iSrcBytesPerPixel * 2);
                    }
#else
                        for (x = xmin; x < xmax; x++)
                        {
                            // pucPtr[0]: Blue
                            // pucPtr[1]: Green
                            // pucPtr[2]: Red
                            // pucPtr[3]: Alpha

                            byte ucLum = pucPtrSrc[0];
                            pucPtr[0] = ucLum;
                            pucPtr[1] = ucLum;
                            pucPtr[2] = ucLum;
                            pucPtr[3] = 0xFF;

                            pucPtr += (iDstBytesPerPixel);
                            pucPtrSrc += (iSrcBytesPerPixel);
                        }
#endif

                    pucLine += iStride;
                }
            }
        }
        static void _YUV2RGB(int y, int u, int v, out int r, out int g, out int b)
        {
            //> y += 16;
            u -= 128;
            v -= 128;
            r = (int)Math.Round(y + 1.13983 * v);
            g = (int)Math.Round(y - 0.39465 * u - 0.58060 * v);
            b = (int)Math.Round(y + 2.03211 * u);
            r = (int)Math.Min(Math.Max(r, 0), 255);
            g = (int)Math.Min(Math.Max(g, 0), 255);
            b = (int)Math.Min(Math.Max(b, 0), 255);
        }
        unsafe void _convertNV12ToRGB24(byte* nv12, BitmapData bitmapData, int width, int height)
        {
            byte* yPlane = nv12;
            byte* uvPlane = nv12 + width * height;

            byte* dst = (byte*)bitmapData.Scan0;
            int stride = bitmapData.Stride;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int yIndex = y * width + x;
                    int uvIndex = (y / 2) * width + (x / 2) * 2;

                    byte Y = yPlane[yIndex];
                    byte U = uvPlane[uvIndex];
                    byte V = uvPlane[uvIndex + 1];

                    int C = Y - 16;
                    int D = U - 128;
                    int E = V - 128;

                    int R = (298 * C + 409 * E + 128) >> 8;
                    int G = (298 * C - 100 * D - 208 * E + 128) >> 8;
                    int B = (298 * C + 516 * D + 128) >> 8;

                    R = Clamp(R);
                    G = Clamp(G);
                    B = Clamp(B);

                    byte* pixel = dst + y * stride + x * 3;
                    pixel[0] = (byte)B;
                    pixel[1] = (byte)G;
                    pixel[2] = (byte)R;
                }
            }
        }
        int Clamp(int val) => val < 0 ? 0 : val > 255 ? 255 : val;
        unsafe void _doFlipAndMirror(byte* scan0, ref Rectangle rect, int iStride, int iBytesPerPixel)
        {
            int x, xmax, xmid;
            int y, ymax, ymid;
            byte* pucPtr;
            byte* pucPtrB;
            byte* pucLine;
            byte* pucLineB;

            ymax = rect.Y + rect.Height;
            xmax = rect.X + rect.Width;
            ymid = rect.Y + (rect.Height >> 1);
            xmid = rect.X + (rect.Width >> 1);

            pucLine = scan0 + (rect.Y * iStride) + (rect.X * iBytesPerPixel);
            pucLineB = scan0 + (ymax * iStride) + (rect.X * iBytesPerPixel);

            for (y = rect.Y; y < ymax; y++)
            {
                pucPtr = pucLine;
                pucPtrB = pucLineB - iBytesPerPixel;

                for (x = rect.X; x < xmid; x++)
                {
#if (OPT_USING_LUM || true)
                    int y1 = _getLum(pucPtr);
                    int y2 = _getLum(pucPtrB);
                    for (int i = 0; i < iBytesPerPixel; i++)
                    {
                        pucPtr[i] = (byte)y2;
                        pucPtrB[i] = (byte)y1;
                    }

#else
                        for (int i = 0; i < iBytesPerPixel; i++)
                        {
                            byte ucTmp = pucPtr[i];
                            pucPtr[i] = pucPtrB[i];
                            pucPtrB[i] = ucTmp;
                        }
#endif

                    pucPtr += iBytesPerPixel;
                    pucPtrB -= iBytesPerPixel;
                }

                pucLine += iStride;
                pucLineB -= iStride;
            }
        }
        unsafe int _getLum(byte* pucPtr)
        {
            //int y = pucPtr[2];
            int y = (int)(0.299 * pucPtr[2] + 0.587 * pucPtr[1] + 0.114 * pucPtr[0]);
            if (y > 255) y = 255;
            return y;
        }
        #endregion

        #region NAMES_AND_ID
        IEzDeviceInfo _deviceInfo;
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
            //get => _deviceInfo != null ? _deviceInfo.Index : 0;
            get => _imp != null ? _imp.CamIndex : 0;
        }
        public override string ToString()
        {
            return FriendlyName;
        }
        #endregion

        #region IEzCamera
        DshowNormalizedValue _dsExposure = new DshowNormalizedValue(0, 10000, 100);
        DshowNormalizedValue _dsGain = new DshowNormalizedValue(0, 15, 0.2);

        public override void GetImageInfo(out int width, out int height, out int bitsPerPixel)
        {
            var size = _imp != null ? _imp.TargetFrameSize : new Size(1, 1);
            width = size.Width;
            height = size.Height;
            bitsPerPixel = 3;
        }
        public override float MaxFps
        {
            get => _imp != null ? (float)_imp.TargetFrameRate : 5f;
            set { }
        }

        public override void GetHardwareGainRange(out double min, out double max)
        {
            if (_imp != null)
            {
                _imp.GetGainRange(out min, out max, out double step, out double defaultValue);
                _dsGain.AdjustFromDs(min, max, step, defaultValue, false);
            }
            min = _dsGain.Min;
            max = _dsGain.Max;
        }
        public override void GetExposureRange(out double min, out double max)
        {
            if (_imp != null)
            {
                _imp.GetExposureRange(out min, out max, out double step, out double defaultValue);
                _dsExposure.AdjustFromDs(min, max, step, defaultValue, true);
            }
            min = _dsExposure.Min;
            max = _dsExposure.Max;
        }
        public override double HardwareGain
        {
            get
            {
                if (_imp != null)
                {
                    _imp.GetGain(out double value, out bool auto);
                    _dsGain.AdjustFromDs(value, auto);
                }
                return _dsGain.Value;
            }
            set
            {
                _dsGain.Value = value;
                _dsGain.GetDsValue(out double dsValue, out bool auto);
                _imp?.SetGain(dsValue, auto);
            }
        }
        public override double ExposureTime
        {
            get
            {
                if (_imp != null)
                {
                    _imp.GetExposure(out double value, out bool auto);
                    _dsExposure.AdjustFromDs(value, auto);
                }
                return _dsGain.Value;
            }
            set
            {
                _dsExposure.Value = value;
                _dsExposure.GetDsValue(out double dsValue, out bool auto);
                _imp?.SetExposure(dsValue, auto);
            }
        }

#if (false)
        public override int Brightness
        {
            get;
            set;
        }
        public override int Contrast
        {
            get;
            set;
        }
        public override bool Mono
        {
            get;
            set;
        }
        public override bool InverseModeEnabled
        {
            get;
            set;
        }
        public override int RotateAngle
        {
            get;
            set;
        }
        void IEzCameraProps.GetBrightnessRange(out int min, out int max)
        {
            min = 0;
            max = 100;
        }
        void IEzCameraProps.GetContrastRange(out int min, out int max)
        {
            min = 0;
            max = 100;
        }
        bool IEzCameraProps.IsLarge()
        {
            return false;
        }
#endif

        void IEzCamera.Init()
        {
            //throw new NotImplementedException();
        }
        EzCameraError IEzCamera.GetError()
        {
            return EzCameraError.NoError;
        }
        bool IEzCamera.IsLiveMode()
        {
            return _imp != null && _imp.IsLiveMode();
        }
        void IEzCamera.StartLiveMode()
        {
            //this.StartGrab();
            _imp?.StartLiveMode();
        }
        void IEzCamera.StopLiveMode()
        {
            //this.StopGrab();
            _imp?.StopLiveMode();
        }
        Bitmap IEzCamera.Snapshot(bool fireEvent)
        {
            var bmp = _imp?.Snapshot();
            if (bmp != null)
            {
                bmp = this.apply_internal_image_process(bmp);
                if (fireEvent)
                    fire_OnLiveImage(bmp, bypass_img_processing: true);
            }
            return bmp;
        }
        void IEzCamera.TriggerOneFrame()
        {
            var cam = (IEzCamera)this;
            if (cam.IsLiveMode())
                return;
            var bmp = cam.Snapshot();
            bmp?.Dispose();
        }
        Color IEzCamera.GetPixelColor(int x, int y)
        {
            return Color.Black;
        }

        bool IEzCamera.HasPropertyPanel()
        {
            return false;
        }
        void IEzCamera.ShowPropertyPanel(bool show, object parentWindow)
        {
        }
        bool IEzCamera.IsSimulation()
        {
            return false;
        }
        string IEzCamera.Browse(string filePath)
        {
            return string.Empty;
        }
        #endregion

        #region PRIVATE_IMAGE_PROCESS_FUNCTIONS
        EzImageProcess _imgProcess = new EzImageProcess();
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
        private Bitmap apply_color_transform(Bitmap imgBuf)
        {
            if (imgBuf == null)
                return null;

            int bits = Image.GetPixelFormatSize(imgBuf.PixelFormat);

            //// 強制轉成 RGB 24bit
            //if (OPT_FORCE_RGB24 || bits > 24)
            //    imgBuf = _imgProcess.ToRgb24(imgBuf, true);

            // 強制轉成 單色
            if (this.Mono)
                _imgProcess.ApplyMonoColor(imgBuf);

            return imgBuf;
        }
        private Bitmap apply_inv_bright_constrast(Bitmap imgBuf)
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
        #endregion
    }



    class DshowNormalizedValue
    {
        #region PRIVATE_DATA
        double _minDs, _maxDs, _stepDs;
        double _minT, _maxT, _stepT;
        double _zoomT;
        double _valueT;
        #endregion

        public DshowNormalizedValue(double minT, double maxT, double stepT)
        {
            _minT = minT;
            _maxT = maxT;
            _stepT = stepT;
            _minDs = minT;
            _maxDs = maxT;
            _stepDs = stepT;
        }

        public void AdjustFromDs(double min, double max, double step, double defaultValue, bool autoAdjustMax)
        {
            _minDs = min;
            _maxDs = max;
            _stepDs = step;
            
            var spanDs = _maxDs - _minDs + _stepDs;
            var spanT = _maxT - _minT + _stepT;

            if (autoAdjustMax)
            {
                _zoomT = _stepT / Math.Max(_stepDs, 1e-6);
                _maxT = _minT + spanDs * _zoomT;
            }
            else
            {
                _zoomT = spanT / Math.Max(spanDs, 1e-6);
            }
        }
        public void AdjustFromDs(double valueDs, bool auto)
        {
            var spanDs = (valueDs - _minDs);
            _valueT = _minT + spanDs * _zoomT;
        }
        public void GetDsValue(out double valueDs, out bool auto)
        {
            var spanT = (_valueT - _minT);
            valueDs = _minDs + spanT / _zoomT;
            auto = valueDs < _minDs || valueDs > _maxDs;
            //auto = false;
        }

        public double Min
        {
            get => _minT;
        }
        public double Max
        {
            get => _maxT;
            set => _maxT = value;
        }
        public double Step
        {
            get => _stepT;
        }
        public double Value
        {
            get => _valueT;
            set => _valueT = value;
        }
    }
}
