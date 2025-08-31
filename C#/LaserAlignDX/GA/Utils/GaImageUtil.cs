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
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using VisionDesigner;


namespace JetEazy.Utils
{
    public class GaMvdConvertor
    {
        /// <summary>
        /// 會生成新的 CMvdImage.
        /// Caller 必須接管 mvdImage 與 srcBmp 之生命週期 !!!
        /// (有用到 Data Copy, 耗時)
        /// </summary>
        public static CMvdImage BitmapToCMvdImage(Bitmap srcBmp)
        {
            CMvdImage cMvdImage = new CMvdImage();

            var bitPixelFormat = srcBmp.PixelFormat;
            BitmapData bmData = srcBmp.LockBits(new Rectangle(0, 0, srcBmp.Width, srcBmp.Height), ImageLockMode.ReadOnly, bitPixelFormat);//锁定
            if (bitPixelFormat == PixelFormat.Format8bppIndexed)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex++];
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08, stImageData);
            }
            else if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width * 3;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height * 3;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 2];
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 1];
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex];
                        bitmapIndex += 3;
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width * 3;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_RGB_RGB24_C3, stImageData);
            }
            else if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width * 4;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        // 获取32bpp像素值
                        byte b = _BitImageBufferBytes[bitmapIndex];
                        byte g = _BitImageBufferBytes[bitmapIndex + 1];
                        byte r = _BitImageBufferBytes[bitmapIndex + 2];
                        byte a = _BitImageBufferBytes[bitmapIndex + 3];
                        bitmapIndex += 4;
                        // 转换为灰度值（8bpp）
                        byte gray = (byte)((r * 0.299 + g * 0.587 + b * 0.114) * (a / 255.0));

                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = gray;// _BitImageBufferBytes[bitmapIndex++];
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08, stImageData);
            }
            else
            {
                cMvdImage.InitImage((uint)srcBmp.Width, (uint)srcBmp.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            }
            srcBmp.UnlockBits(bmData);  // 解除锁定
            return cMvdImage;
        }

        /// <summary>
        /// 會生成新的 Bitmap.
        /// Caller 必須接管其生命週期 !!!
        /// (有用到 Data Copy, 耗時)
        /// </summary>
        public static Bitmap CMvdImageToBitmap(CMvdImage mvdImage)
        {
            MVD_IMAGE_DATA_INFO _MvdImage = mvdImage.GetImageData();
            MVD_DATA_CHANNEL_INFO ch0 = _MvdImage.stDataChannel[0];
            //Bitmap _bmpFromMVD = ByteArrayToBitmap(ch0.arrDataBytes, (int)ch0.nRowStep, (int)(ch0.nLen / ch0.nRowStep));
            return ByteArrayToBitmap(ch0.arrDataBytes, (int)ch0.nRowStep, (int)(ch0.nLen / ch0.nRowStep));
        }

        #region PRIVATE_FUNCTIONS
        /// <summary>
        /// 會生成新的 Bitmap.
        /// Caller 必須接管其生命週期 !!!
        /// </summary>
        public static Bitmap ByteArrayToBitmap(byte[] srcArray, int width, int height)
        {
            // 创建 Bitmap 对象
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            // 设置调色板为灰度
            ColorPalette palette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bitmap.Palette = palette;
            // 锁定 Bitmap 数据
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            // 将字节数组复制到 Bitmap 数据中
            System.Runtime.InteropServices.Marshal.Copy(srcArray, 0, bitmapData.Scan0, srcArray.Length);
            // 解锁 Bitmap 数据
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }
        #endregion

        /// <summary>
        /// Runtime 使用 C# Bitmap 來觀察 mvdImage 內容.
        /// Caller 不得釋放 此函式所取的取得的 Bitmap !!!
        /// </summary>
        public static Bitmap PeekBmp(CMvdImage mvdImage)
        {
            //if (mvdImage == null)
            //    return null;
            var mat = PeekMat(mvdImage);
            if (mat == null)
                return null;
            IntPtr dataPtr = mat.Data;
            Bitmap bmp = new Bitmap(mat.Width, mat.Height, (int)mat.Step(), PixelFormat.Format8bppIndexed, dataPtr);
            return bmp;
        }

        /// <summary>
        /// Runtime 使用 OpenCvSharp Mat 來觀察 mvdImage 內容.
        /// Caller 不得釋放 此函式所取的取得的 Mat !!!
        /// </summary>
        public static Mat PeekMat(CMvdImage mvdImage)
        {
            if (mvdImage == null)
                return null;

            MVD_IMAGE_DATA_INFO info = mvdImage.GetImageData();
            MVD_DATA_CHANNEL_INFO ch0 = info.stDataChannel[0];
            var dataBytes = ch0.arrDataBytes;
            int width = (int)ch0.nRowStep;
            int height = (int)(ch0.nLen / ch0.nRowStep);
            int stride = width;
            Mat mat = new Mat(height, width, MatType.CV_8UC1, dataBytes, stride);

            return mat;
        }

        public static CMvdRectangleF ToCMvdRectangleF(ref RectangleF viewRectF)
        {
            return new CMvdRectangleF(
                            viewRectF.X + viewRectF.Width / 2,
                            viewRectF.Y + viewRectF.Height / 2,
                            viewRectF.Width,
                            viewRectF.Height
                        );
        }

        public static RectangleF ToRectangleF(CMvdRectangleF cMvdRectangleF)
        {
            if (cMvdRectangleF != null)
                return new RectangleF(0, 0, cMvdRectangleF.Width, cMvdRectangleF.Height);
            return RectangleF.Empty;
        }
    }


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


    /// <summary>
    /// 即將 把 EzMvdImageConvertor 改名成 GaImageUtil 或 GaMvdConvertor
    /// </summary>
    public class EzMvdImageConvertor : GaImageUtil
    {
        
    }
}
