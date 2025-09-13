#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-01 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using FreeImageAPI;
using JetEazy.EzImage;
using JetEazy.OpenCV;
using LaserAlignDX;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using VisionDesigner;


namespace JetEazy.Utils
{
    public class GaImageUtil : GaMvdConvertor
    {
        /// <summary>
        /// 載入巨圖
        public static Bitmap LoadBigImage(string fileName, int option = 0)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new Exception($"檔案不存在: {fileName}");

            Bitmap bigBmp = null;

            if (option == 0)
            {
                //NOTE: 使用 QuickImage (IEzImage) 載入圖檔 耗時 1.2 seconds
                bigBmp = loadBigImageViaQuickImage(fileName, false);
            }
            else
            {
                //NOTE: 使用 FreeImageBitmap 載入圖檔 耗時 2.7 s
                bigBmp = loadBigImageViaFreeImageBitmap(fileName, false);
            }

            return bigBmp;
        }

        #region PRIVATE_FUNCTIONS
        /// <summary>
        /// 利用 EzQuickImage (IEzImage) (OpenCvSharp) 載入 8 bpp 影像檔
        /// (無法載入時, 會拋出異常!)
        /// </summary>
        static Bitmap loadBigImageViaQuickImage(string fileName, bool check = true)
        {
            if (check && string.IsNullOrEmpty(fileName))
                throw new Exception($"檔案不存在: {fileName}");

            //var _TM = new EzTiming();
            //_TM.Trace("LoadBigImage (via EzQuickImage)");

            using (EzQuickImage ezImage = new EzQuickImage())
            {
                ezImage.Load(fileName, bits: 8);
                Mat img = ezImage.Image as Mat;
                if (img != null)
                {
                    Bitmap bmp = BitmapConverter.ToBitmap(img);
                    return bmp;
                }
            }

            //_TM.Trace("LoadBigImage (via EzQuickImage)");
            Bitmap bigBmp = loadBigImageViaFreeImageBitmap(fileName);
            //_TM.Dump();
            return bigBmp;
        }
        /// <summary>
        /// 利用 FreeImageBitmap 載入 8 bpp 影像檔
        /// (無法載入時, 會拋出異常!)
        /// </summary>
        static Bitmap loadBigImageViaFreeImageBitmap(string fileName, bool check = true)
        {
            if (check && string.IsNullOrEmpty(fileName))
                throw new Exception($"檔案不存在: {fileName}");

            using (FreeImageBitmap freeImageBitmap = new FreeImageBitmap(fileName))
            using (Bitmap bigBmp = freeImageBitmap.ToBitmap())
            {
                var pixelFormat = bigBmp.PixelFormat;
                if (pixelFormat == PixelFormat.Format32bppArgb)
                {
                    var bmp = Convert32bppTo8bpp(bigBmp);
                    return bmp;
                }
                else if (pixelFormat == PixelFormat.Format24bppRgb)
                {
                    var bmp = Convert24bppTo8bpp(bigBmp);
                    return bmp;

                }
                else if (pixelFormat == PixelFormat.Format8bppIndexed)
                {
                    return (Bitmap)bigBmp.Clone();
                }
                else
                {
                    throw new Exception("加载图片格式不支持！");
                }
            }
        }
        #endregion

        /// <summary>
        /// caller 負責 original 的生命
        /// (直接調用 ToU8)
        /// </summary>
        public static Bitmap Convert32bppTo8bpp(Bitmap original)
        {
            //// 创建一个新的8bpp位图
            //Bitmap newBitmap = new Bitmap(original.Width, original.Height, PixelFormat.Format8bppIndexed);

            //// 设置调色板（这里使用灰度调色板）
            //ColorPalette palette = newBitmap.Palette;
            //for (int i = 0; i < 256; i++)
            //{
            //    palette.Entries[i] = Color.FromArgb(i, i, i);
            //}
            //newBitmap.Palette = palette;

            //// 锁定位图数据
            //BitmapData originalData = original.LockBits(
            //    new Rectangle(0, 0, original.Width, original.Height),
            //    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            //BitmapData newData = newBitmap.LockBits(
            //    new Rectangle(0, 0, newBitmap.Width, newBitmap.Height),
            //    ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            //// 转换像素数据
            //unsafe
            //{
            //    byte* originalPtr = (byte*)originalData.Scan0;
            //    byte* newPtr = (byte*)newData.Scan0;

            //    for (int y = 0; y < original.Height; y++)
            //    {
            //        for (int x = 0; x < original.Width; x++)
            //        {
            //            // 获取32bpp像素值
            //            byte b = originalPtr[y * originalData.Stride + x * 4];
            //            byte g = originalPtr[y * originalData.Stride + x * 4 + 1];
            //            byte r = originalPtr[y * originalData.Stride + x * 4 + 2];
            //            byte a = originalPtr[y * originalData.Stride + x * 4 + 3];

            //            // 转换为灰度值（8bpp）
            //            byte gray = (byte)((r * 0.299 + g * 0.587 + b * 0.114) * (a / 255.0));

            //            // 写入8bpp位图
            //            newPtr[y * newData.Stride + x] = gray;
            //        }
            //    }
            //}

            //// 解锁位图
            //original.UnlockBits(originalData);
            //newBitmap.UnlockBits(newData);

            //return newBitmap;

            return ToU8(original, false);
        }
        /// <summary>
        /// caller 負責 original 的生命.
        /// (直接調用 ToU8)
        /// </summary>
        public static Bitmap Convert24bppTo8bpp(Bitmap original)
        {
            ////if (original.PixelFormat != PixelFormat.Format24bppRgb)
            ////    throw new ArgumentException("源图像必须是24位位图");

            //// 创建新的8位位图
            //Bitmap newBitmap = new Bitmap(original.Width, original.Height, PixelFormat.Format8bppIndexed);

            //// 设置灰度调色板
            //ColorPalette palette = newBitmap.Palette;
            //for (int i = 0; i < 256; i++)
            //{
            //    palette.Entries[i] = Color.FromArgb(i, i, i);
            //}
            //newBitmap.Palette = palette;

            //// 锁定位图数据进行操作
            //BitmapData originalData = original.LockBits(
            //    new Rectangle(0, 0, original.Width, original.Height),
            //    ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            //BitmapData newData = newBitmap.LockBits(
            //    new Rectangle(0, 0, newBitmap.Width, newBitmap.Height),
            //    ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            //unsafe
            //{
            //    byte* originalPtr = (byte*)originalData.Scan0;
            //    byte* newPtr = (byte*)newData.Scan0;

            //    for (int y = 0; y < original.Height; y++)
            //    {
            //        for (int x = 0; x < original.Width; x++)
            //        {
            //            // 获取24bpp像素值
            //            byte b = originalPtr[y * originalData.Stride + x * 3];
            //            byte g = originalPtr[y * originalData.Stride + x * 3 + 1];
            //            byte r = originalPtr[y * originalData.Stride + x * 3 + 2];

            //            // 转换为灰度值（8bpp）
            //            byte gray = (byte)(r * 0.299 + g * 0.587 + b * 0.114);

            //            // 写入8bpp位图
            //            newPtr[y * newData.Stride + x] = gray;
            //        }
            //    }
            //}

            //// 解锁位图
            //original.UnlockBits(originalData);
            //newBitmap.UnlockBits(newData);

            //return newBitmap;

            return ToU8(original, false);
        }
        /// <summary>
        /// 使用 OpenCvSharp 轉比較快 !!!
        /// </summary>
        public static Bitmap ToU8(Bitmap src, bool autoDisposeSrc = false)
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
                if (autoDisposeSrc)
                    src.Dispose();
                SetGrayPalete(newBmp);
                return newBmp;
            }
        }
        /// <summary>
        /// 設定 8 bpp 調色盤
        /// </summary>
        public static void SetGrayPalete(Bitmap bmpU8)
        {
            if (bmpU8.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                // 設置調色板
                ColorPalette palette = bmpU8.Palette;
                int N = palette.Entries.Length;
                if (N != 256)
                    return;
                for (int i = 0; i < 256 && i < N; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i); // 設置灰度調色板
                }
                bmpU8.Palette = palette;
            }
        }
        /// <summary>
        /// 可指定 壓縮品質 之 jpg 存檔函式
        /// (將 Gaara 代碼 集中至此) 
        /// </summary>
        public static void SaveImageWithQuality(Bitmap bmpinput, string outputImagePath, long quality)
        {
            if (bmpinput == null)
                return;

            using (Bitmap image = (Bitmap)bmpinput.Clone(new Rectangle(0, 0, bmpinput.Width, bmpinput.Height), bmpinput.PixelFormat))
            {
                // 设置压缩参数
                EncoderParameters encoderParameters = new EncoderParameters(1);
                EncoderParameter encoderParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                encoderParameters.Param[0] = encoderParameter;

                // 获取图像编码信息
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

                // 保存图片，应用压缩参数
                image.Save(outputImagePath, jpgEncoder, encoderParameters);
            }
        }
        /// <summary>
        /// 可指定 壓縮品質 之 jpg 存檔函式
        /// (將 Gaara 代碼 集中至此) 
        /// </summary>
        public static void SaveImageWithQuality(IEzImage ezImage, string outputImagePath, long quality)
        {
            if (ezImage == null)
                return;

            if(ezImage.IsOpenCV)
            {
                if (ezImage.Image is Mat mat)
                {
                    using (Bitmap bmp = BitmapConverter.ToBitmap(mat))
                    {
                        SaveImageWithQuality(bmp, outputImagePath, quality);
                    }
                }
            }
            else
            {
                SaveImageWithQuality(ezImage.Bitmap, outputImagePath, quality);
            }
        }

        #region PRIVATE_FUNCTIONS
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        #endregion
    }
}
