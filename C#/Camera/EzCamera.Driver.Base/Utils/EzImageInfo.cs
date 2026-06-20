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


using OpenCvSharp;
using System.Drawing;
using System.Drawing.Imaging;

namespace EzCamera.Driver.Utils
{
    public class EzImageInfo
    {
        public int Width = 1;
        public int Height = 1;
        public int BitsPerPixel = 8;
        public PixelFormat PixelFormat
        {
            get
            {
                return GetPixelFormat(this.BitsPerPixel);
            }
        }

        public static EzImageInfo FromMat(Mat img)
        {
            var imgInfo = new EzImageInfo();
            if (img != null && !img.Empty())
            {
                imgInfo.Width = img.Width;
                imgInfo.Height = img.Height;
                imgInfo.BitsPerPixel = img.Channels() * 8;
            }
            return imgInfo;
        }
        public static EzImageInfo FromBitmap(Bitmap bmp)
        {
            var imgInfo = new EzImageInfo();
            if (bmp != null)
            {
                imgInfo.Width = bmp.Width;
                imgInfo.Height = bmp.Height;
                imgInfo.BitsPerPixel = GetBitsPerPixel(bmp.PixelFormat);
            }
            return imgInfo;
        }
        public static PixelFormat GetPixelFormat(int bitsPerPixel)
        {
            switch (bitsPerPixel)
            {
                case 8:
                    return PixelFormat.Format8bppIndexed;
                case 16:
                    return PixelFormat.Format16bppRgb565;
                case 32:
                    return PixelFormat.Format32bppArgb;
                default:
                    return PixelFormat.Format24bppRgb;
            }
        }
        public static int GetBitsPerPixel(PixelFormat fmt)
        {
            return System.Drawing.Image.GetPixelFormatSize(fmt);
        }
        public static bool AreEqual(EzImageInfo info1, EzImageInfo info2)
        {
            if (info1 == info2)
                return true;
            if (info1 == null || info2 == null)
                return false;
            return info1.Width == info2.Width &&
                   info1.Height == info2.Height &&
                   info1.BitsPerPixel == info2.BitsPerPixel;
        }
    }
}
