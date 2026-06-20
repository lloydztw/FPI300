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


using EzCamera.Interface;
using JetEazy.OpenCV;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace EzCamera.Driver.Utils
{
    /// <summary>
    /// 影像處理: 反向, 亮度, 對比, 角度 0, 90, 180, 270
    /// </summary>
    public class EzImageProcess
    {
        #region PRIVATE_DATA
        int _lastBrightness;
        int _lastContrast;
        byte[] _lut;
        #endregion

        public void ApplyInvBrightnessContrast(Bitmap bmp, IEzCameraProps settings)
        {
            if (settings.InverseModeEnabled || settings.Contrast != 0 || settings.Brightness != 0)
            {
                using (var bridge = new QxImageBridge(bmp))
                {
                    var paraZones = getParallelZones(bmp.Width, bmp.Height);
                    if (paraZones == null)
                    {
                        if (settings.InverseModeEnabled)
                            ApplyInverse(bridge.Image);
                        ApplyBrightnessContrast(bridge.Image, settings.Brightness, settings.Contrast);
                    }
                    else
                    {
                        // 目前對超大 Bitmap 圖形 Parallel 運算 沒啥幫助
                        // 主要時間貌似都耗在 Bitmap 轉 OpenCvSharp
                        Parallel.ForEach(paraZones, pts =>
                        {
                            int x = pts[0];
                            int y = pts[1];
                            int x2 = pts[2];
                            int y2 = pts[3];
                            var roiImg = bridge.Image[y, y2, x, x2];
                            if (settings.InverseModeEnabled)
                                ApplyInverse(roiImg);
                            ApplyBrightnessContrast(roiImg, settings.Brightness, settings.Contrast);
                        });
                    }
                }
            }
        }
        public Bitmap ApplyRotation(Bitmap bmp, int rotateAngle)
        {
            RotateFlipType rotateFlip;

            switch (rotateAngle)
            {
                case 90:
                    rotateFlip = RotateFlipType.Rotate90FlipNone;
                    break;
                case 270:
                    rotateFlip = RotateFlipType.Rotate270FlipNone;
                    break;
                case 180:
                    rotateFlip = RotateFlipType.Rotate180FlipNone;
                    break;
                case 0:
                default:
                    return bmp;
            }

            bmp.RotateFlip(rotateFlip);
            return bmp;
        }

        public void ApplyInverse(Bitmap bmp)
        {
            using (var bridge = new QxImageBridge(bmp))
            {
                ApplyInverse(bridge.Image);
            }
        }
        public void ApplyInverse(Mat img)
        {
            if (img.Channels() < 4)
            {
                Cv2.BitwiseNot(img, img, null);
            }
            else
            {
                var imgs = img.Split();
                for (int i = 0; i < 3; i++)
                    Cv2.BitwiseNot(imgs[i], imgs[i], null);
                Cv2.Merge(imgs, img);
            }
        }
        public void ApplyBrightnessContrast(Bitmap bmp, int brightness, int contrast)
        {
            using (var bridge = new QxImageBridge(bmp))
            {
                ApplyBrightnessContrast(bridge.Image, brightness, contrast);
            }
        }
        public void ApplyBrightnessContrast(Mat img, int brightness, int contrast)
        {
            if (brightness != 0 || contrast != 0)
            {
                if (_lut == null || _lastBrightness != brightness || _lastContrast != contrast)
                {
                    _lastBrightness = brightness;
                    _lastContrast = contrast;
                    _lut = _calcLut(contrast, brightness);
                }
                Cv2.LUT(img, _lut, img);
            }
        }

        /// <summary>
        /// 簡單轉換為單色 (channels數量不變)
        /// </summary>
        public void ApplyMonoColor(Bitmap bmp, int targetChannel = 1)
        {
            if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                return;

            using (var bridge = new QxImageBridge(bmp))
            {
                var paraZones = getParallelZones(bmp.Width, bmp.Height);
                if (paraZones == null)
                {
                    ApplyMonoColor(bridge.Image, targetChannel);
                }
                else
                {
                    // 目前對超大 Bitmap 圖形 Parallel 運算 沒啥幫助
                    // 主要時間貌似都耗在 Bitmap 轉 OpenCvSharp
                    Parallel.ForEach(paraZones, pts =>
                    {
                        int x = pts[0];
                        int y = pts[1];
                        int x2 = pts[2];
                        int y2 = pts[3];
                        var roiImg = bridge.Image[y, y2, x, x2];
                        ApplyMonoColor(roiImg, targetChannel);
                    });
                }
            }
        }
        public void ApplyMonoColor(Mat img, int targetChannel = 1)
        {
            int N = img.Channels();
            if (N == 1)
                return;

            targetChannel = Math.Min(targetChannel, N - 1);
            var imgs = img.Split();

            for (int i = 0; i < imgs.Length && i < 3; i++)
                imgs[i] = imgs[targetChannel];

            if (N == 4)
            {
                imgs[3].SetTo(Scalar.White);
            }

            Cv2.Merge(imgs, img);
        }

        public Bitmap ToRgb24(Bitmap src, bool autoDispose = false)
        {
            if (src.PixelFormat != PixelFormat.Format24bppRgb)
            {
                //------------------------------------------------------------------------------
                // NOTE:  32Bit 的 Alpha channel 可能會讓 畫面黑掉
                //------------------------------------------------------------------------------
                //var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format24bppRgb);
                //using (var gx = Graphics.FromImage(bmp))
                //{
                //    // 設定 CompositingMode 讓 Alpha 不要影響本色
                //    gx.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                //    gx.DrawImageUnscaled(src, 0, 0);
                //}
                var rect = new Rectangle(0, 0, src.Width, src.Height);
                var bmp = src.Clone(rect, PixelFormat.Format24bppRgb);
                if (autoDispose)
                    src.Dispose();
                return bmp;
            }
            return src;
        }
        public Bitmap ToU8(Bitmap src, bool autoDispose = false)
        {
            if (src.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                SetGrayPalete(src);
                return src;
            }
            else
            {
                var newBmp = new Bitmap(src.Width, src.Height, PixelFormat.Format8bppIndexed);
                using (var srcBridge = new QxImageBridge(src))
                using (var dstBridge = new QxImageBridge(newBmp))
                {
                    int bits = Image.GetPixelFormatSize(src.PixelFormat);
                    switch (bits)
                    {
                        case 32:
                            Cv2.CvtColor(srcBridge.Image, dstBridge.Image, ColorConversionCodes.BGRA2GRAY);
                            break;
                        case 24:
                            Cv2.CvtColor(srcBridge.Image, dstBridge.Image, ColorConversionCodes.BGR2GRAY);
                            break;
                        case 16:
                            Cv2.CvtColor(srcBridge.Image, dstBridge.Image, ColorConversionCodes.BGR5652GRAY);
                            break;
                        default:
                            throw new Exception("[ToU8] 未知的 PixelFormat!");
                    }
                }
                if (autoDispose)
                    src.Dispose();
                SetGrayPalete(newBmp);
                return newBmp;
            }
        }
        public static void SetGrayPalete(Bitmap bmpU8)
        {
            if (bmpU8.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                // 設置調色板
                ColorPalette palette = bmpU8.Palette;
                for (int i = 0; i < 256; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i); // 設置灰度調色板
                }
                bmpU8.Palette = palette;
            }
        }

        #region PRIVATE_FUNCTIONS
        private static byte[] _calcLut(int contrast, int brightness)
        {
            byte[] lut = new byte[256];

            if (contrast > 0)
            {
                double delta = 127.0 * contrast / 100;
                double a = 255.0 / (255.0 - delta * 2);
                double b = a * (brightness - delta);
                for (int i = 0; i < 256; i++)
                {
                    int v = (int)Math.Round(a * i + b);
                    if (v < 0)
                        v = 0;
                    if (v > 255)
                        v = 255;
                    lut[i] = (byte)v;
                }
            }
            else
            {
                double delta = -128.0 * contrast / 100;
                double a = (256.0 - delta * 2) / 255.0;
                double b = a * brightness + delta;
                for (int i = 0; i < 256; i++)
                {
                    int v = (int)Math.Round(a * i + b);
                    if (v < 0)
                        v = 0;
                    if (v > 255)
                        v = 255;
                    lut[i] = (byte)v;
                }
            }
            return lut;
        }
        private List<int[]> getParallelZones(int W, int H)
        {
            if (W < 4000 && H < 4000)
            {
                return null;
            }
            else
            {
                int cols = 2;
                int rows = 2;
                int dw = W / cols;
                int dh = H / rows;
                var points = new List<int[]>();
                for (int r = 0; r < rows; r++)
                {
                    int y = dh * r;
                    int y2 = Math.Min(y + dh, H);
                    for (int c = 0; c < cols; c++)
                    {
                        int x = dw * c;
                        int x2 = Math.Min(x + dw, W);
                        points.Add(new int[] { x, y, x2, y2 });
                    }
                }
                return points;
            }
        }
        #endregion
    }
}
