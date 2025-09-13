#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-31 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QvMath;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using VisionDesigner;


namespace LaserAlignDX
{
    /// <summary>
    /// 為 Mvd 各式元件, 外掛各種轉換函式
    /// </summary>
    public static class GaMvdExt
    {
        public static QvBox2D ToBox2D(this CMvdRectangleF mvdRectF)
        {
            if (mvdRectF == null)
                return null;
            var cx = mvdRectF.CenterX;
            var cy = mvdRectF.CenterY;
            var cw = mvdRectF.Width;
            var ch = mvdRectF.Height;
            var angle = mvdRectF.Angle;
            var box2D = new QvBox2D();
            box2D.SetBox(new PointF(cx, cy), new SizeF(cw, ch));
            box2D.SetCenter(cx, cy);
            box2D.SetTheta(angle * Math.PI / 180);
            return box2D;
        }
        public static RectangleF ToRectangleF(this CMvdRectangleF cMvdRectangleF)
        {
            if (cMvdRectangleF != null)
                return new RectangleF(0, 0, cMvdRectangleF.Width, cMvdRectangleF.Height);
            return RectangleF.Empty;
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

        public static PointF[] ToCSharpLine(this CMvdLineSegmentF mvdLine)
        {
            if (mvdLine == null)
                return null;
            return new[] {
                new PointF(mvdLine.StartPoint.fX, mvdLine.StartPoint.fY),
                new PointF(mvdLine.EndPoint.fX, mvdLine.EndPoint.fY)
            };
        }
        public static PointF[][] ToCSharpLines(params CMvdLineSegmentF[] mvdLines)
        {
            if (mvdLines == null)
                return null;
            var lines = new List<PointF[]>();
            foreach (var mvdLine in mvdLines)
                if (mvdLine != null)
                    lines.Add(mvdLine.ToCSharpLine());
            if (lines.Count > 0)
                return lines.ToArray();
            return null;
        }
        public static PointF[][] ToCSharpLines(PointF offset, params CMvdLineSegmentF[] mvdLines)
        {
            var lines = ToCSharpLines(mvdLines);
            Offset(lines, offset);
            return lines;
        }
        public static void Offset(this CMvdLineSegmentF line, float offsetX, float offsetY)
        {
            var p1 = line.StartPoint;
            var p2 = line.EndPoint;
            p1.fX += offsetX;
            p1.fY += offsetY;
            p2.fX += offsetX;
            p2.fY += offsetY;
            line.StartPoint = p1;
            line.EndPoint = p2;
        }
        public static void Offset(PointF[][] lines, PointF offset)
        {
            if (lines == null)
                return;
            foreach (var pts in lines)
            {
                for (var i = 0; i < pts.Length; i++)
                {
                    pts[i].X += offset.X;
                    pts[i].Y += offset.Y;
                }
            }
        }
    }


    /// <summary>
    /// 為 CMvdImage, 外掛各種轉換函式
    /// </summary>
    public static class GaMvdImageExt
    {
        /// <summary>
        /// 會生成新的 CMvdImage.
        /// Caller 必須接管 mvdImage 與 srcBmp 之生命週期 !!!
        /// (有用到 Data Copy, 耗時)
        /// </summary>
        public static CMvdImage ToCMvdImage(Bitmap srcBmp)
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
        public static Bitmap ToBitmap(this CMvdImage mvdImage)
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
        public static Bitmap PeekBmp(this CMvdImage mvdImage)
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
        public static Mat PeekMat(this CMvdImage mvdImage)
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
    }


    /// <summary>
    /// 對舊版相容接口
    /// </summary>
    public class GaMvdConvertor
    {
        /// <summary>
        /// 會生成新的 CMvdImage.
        /// Caller 必須接管 mvdImage 與 srcBmp 之生命週期 !!!
        /// (有用到 Data Copy, 耗時)
        /// </summary>
        public static CMvdImage BitmapToCMvdImage(Bitmap srcBmp)
        {
            return GaMvdImageExt.ToCMvdImage(srcBmp);
        }

        /// <summary>
        /// 會生成新的 Bitmap.
        /// Caller 必須接管其生命週期 !!!
        /// (有用到 Data Copy, 耗時)
        /// </summary>
        public static Bitmap CMvdImageToBitmap(CMvdImage mvdImage)
        {
            return mvdImage?.ToBitmap();
        }

        /// <summary>
        /// Runtime 使用 C# Bitmap 來觀察 mvdImage 內容.
        /// Caller 不得釋放 此函式所取的取得的 Bitmap !!!
        /// </summary>
        public static Bitmap PeekBmp(CMvdImage mvdImage)
        {
            return mvdImage?.PeekBmp();
        }

        /// <summary>
        /// Runtime 使用 OpenCvSharp Mat 來觀察 mvdImage 內容.
        /// Caller 不得釋放 此函式所取的取得的 Mat !!!
        /// </summary>
        public static Mat PeekMat(CMvdImage mvdImage)
        {
            return mvdImage?.PeekMat();
        }

        public static CMvdRectangleF ToCMvdRectangleF(ref RectangleF viewRectF)
        {
            return GaMvdExt.ToCMvdRectangleF(ref viewRectF);
        }

        public static RectangleF ToRectangleF(CMvdRectangleF cMvdRect)
        {
            if (cMvdRect == null)
                return RectangleF.Empty;
            return cMvdRect.ToRectangleF();
        }
    }
}
