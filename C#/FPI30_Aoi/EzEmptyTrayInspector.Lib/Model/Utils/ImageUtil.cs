#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.EzImage;
using JetEazy.OpenCV;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using FlipMode = OpenCvSharp.FlipMode;
using Mat = OpenCvSharp.Mat;


namespace EzAoiEmptyTrayInspector
{
    public enum MirrorMode : int
    {
        //[Description("無鏡像")]
        None = 0,
        //[Description("上下鏡像")]
        X_Axis,
        //[Description("左右鏡像")]
        Y_Axis,
    }

    public class ImageUtil
    {
        public static async Task<IEzImage> LoadLargeImageAsync(string fileName, MirrorMode mirror = MirrorMode.None, PixelFormat fmt = PixelFormat.Format24bppRgb)
        {
            // 使用 Task.Run 將同步操作轉換為異步
            return await Task.Run(() => LoadLargeImage(fileName, mirror, fmt));
        }
        public static IEzImage LoadLargeImage(string fileName, MirrorMode mirror = MirrorMode.None, PixelFormat fmt = PixelFormat.Format24bppRgb)
        {
            var largeImg = new EzQuickImage();

            int bits = Image.GetPixelFormatSize(fmt);
            largeImg.Load(fileName, bits);
            auto_dump_to_jpeg(largeImg, fileName);

            if (largeImg != null && mirror != MirrorMode.None)
            {
                ApplyMirror(largeImg, mirror, true);
            }

            return largeImg;
        }
        public static Bitmap CropBmp(IEzImage largeImg, Rectangle cropRect)
        {
            if (largeImg == null)
                return null;

            Bitmap bmp = null;
            if (largeImg.IsOpenCV)
            {
                Mat img = largeImg.Image as Mat;
                if (img != null)
                {
                    int x1 = cropRect.X;
                    int y1 = cropRect.Y;
                    int x2 = cropRect.Right;
                    int y2 = cropRect.Bottom;
                    bmp = BitmapConverter.ToBitmap(img[y1, y2, x1, x2]);
                }
            }
            else
            {
                Bitmap largeSrcBmp = largeImg.Bitmap;
                if (largeSrcBmp != null)
                {
                    bmp = largeSrcBmp.Clone(cropRect, largeSrcBmp.PixelFormat);
                }
            }

            if (bmp != null)
                SetGrayPalette(bmp);

            return bmp;
        }

        public static MirrorMode GetMirrorTag(IEzImage largeImg)
        {
            object tag = largeImg?.Tag;
            if (tag != null && tag is MirrorMode mirror)
                return mirror;
            return MirrorMode.None;
        }
        public static bool ApplyMirror(IEzImage largeImg, MirrorMode mode, bool markingDirtyPixel)
        {
            if (largeImg == null)
                return false;

            var oldMode = GetMirrorTag(largeImg);
            if (mode == oldMode)
                return false;

            QxImageBridge bridge = null;
            Mat img = null;

            if (largeImg.IsOpenCV)
            {
                img = largeImg.Image as Mat;
                if (img == null)
                    return false;
            }
            else
            {
                if (largeImg.Bitmap == null)
                    return false;

                bridge = new QxImageBridge(largeImg.Bitmap);
                img = bridge.Image;
            }

            if(img != null)
            {
                if (oldMode != MirrorMode.None)
                {
                    // 之前已經有 套用過 鏡像
                    // 使用 toggle 方式, 恢復原始圖片
                    apply_flip(img, oldMode);
                    largeImg.Tag = null;
                }

                // 套用新鏡像
                apply_flip(img, mode);
                largeImg.Tag = mode;

                // Set Dirty Pixel0
                if (markingDirtyPixel)
                    MarkDirtyPixel(img);
            }

            bridge?.Dispose();
            return true;
        }

        public static bool GetCombinedMark(IEzImage largeImg)
        {
            object tag = largeImg?.Tag;
            if (tag != null && tag is string mark && mark=="combined")
                return true;
            return false;
        }
        public static void SetCombinedMark(IEzImage largeImg)
        {
            if (largeImg != null)
                largeImg.Tag = "combined";
        }

        public static int GetPixelBits(IEzImage largeImg)
        {
            if (largeImg == null)
                return 0;
            if (largeImg.Image is Mat mat && mat != null)
                return mat.Channels() * 8;
            if (largeImg.Bitmap is Bitmap bmp && bmp != null)
                return System.Drawing.Image.GetPixelFormatSize(bmp.PixelFormat);
            return 0;
        }
        public static void SetGrayPalette(Bitmap bmp)
        {
            if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                // 設置調色板
                ColorPalette palette = bmp.Palette;
                for (int i = 0; i < 256; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i); // 設置灰度調色板
                }
                bmp.Palette = palette;
            }
        }
        public static void MarkDirtyPixel(Bitmap bmp)
        {
            if (bmp != null)
            {
                using (var bridge = new QxImageBridge(bmp))
                {
                    MarkDirtyPixel(bridge.Image);
                }
            }
        }
        public static void MarkDirtyPixel(Mat img)
        {
            if (img != null)
                Cv2.BitwiseNot(img[0, 1, 0, 1], img[0, 1, 0, 1]);
        }
        
        public static Mat ToU8(Mat src, bool autoDisposeSrc = false)
        {
            if (src == null || src.Channels() == 1)
            {
                return src;
            }
            else
            {
                var dst = new Mat();
                {
                    switch (src.Channels())
                    {
                        case 4:
                            Cv2.CvtColor(src, dst, ColorConversionCodes.BGRA2GRAY);
                            break;
                        case 3:
                            Cv2.CvtColor(src, dst, ColorConversionCodes.BGR2GRAY);
                            break;
                        case 2:
                            Cv2.CvtColor(src, dst, ColorConversionCodes.BGR5652GRAY);
                            break;
                        default:
                            throw new Exception("[ToU8] 錯誤的 channels number!");
                    }
                }
                if (autoDisposeSrc)
                    src.Dispose();
                return dst;
            }
        }

        #region PRIVATE_FUNCTIONS
        static void apply_flip(Bitmap bmp, MirrorMode mode)
        {
            if (bmp != null && mode != MirrorMode.None)
            {
                using (var bridge = new QxImageBridge(bmp))
                {
                    apply_flip(bridge.Image, mode);
                }
            }
        }
        static void apply_flip(Mat img, MirrorMode mode)
        {
            if (img != null && mode != MirrorMode.None)
            {
                if (mode == MirrorMode.X_Axis)
                {
                    OpenCvSharp.Cv2.Flip(img, img, FlipMode.X);
                }
                else if (mode == MirrorMode.Y_Axis)
                {
                    OpenCvSharp.Cv2.Flip(img, img, FlipMode.Y);
                }
            }
        }
        #endregion

        #region TRACE_AND_DUMP
        static void auto_dump_to_jpeg(IEzImage img, string fileName)
        {
#if (DEBUG)
            if (img != null && !string.IsNullOrEmpty(fileName))
            {
                if (System.IO.Path.GetExtension(fileName).ToLower() != ".jpg")
                {
                    var dstFile = System.IO.Path.ChangeExtension(fileName, ".jpg");
                    img.Save(dstFile);
                }
            }
#endif
        }
        static void _DUMP(Bitmap bmp, string fname)
        {
            //if (bmp != null)
            //{
            //    try
            //    {
            //        string fileName = System.IO.Path.Combine(Global.WorkPath("Dump"), fname);
            //        bmp.Save(fileName);
            //    }
            //    catch
            //    {

            //    }
            //}
        }
        #endregion
    }
}
