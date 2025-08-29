#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-28 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Utils;
using System;
using System.Drawing;
using VisionDesigner;


namespace LaserAlignDX.AoiModel
{
    public class GaBigImageHolder : IDisposable
    {
        public event EventHandler OnImageChanged;

        #region PRIVATE_DATA
        Bitmap _bitmap;
        CMvdImage _mvdImage;
        #endregion

        public void Dispose()
        {
            cleanUp();
        }

        public bool IsEmpty()
        {
            return _bitmap == null && _mvdImage == null;
        }

        /// <summary>
        /// 接管 bitmap
        /// </summary>
        public void TakeOver(Bitmap bitmap)
        {
            if (_bitmap == bitmap)
                return;

            cleanUp();
            _bitmap = bitmap;
            OnImageChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 接管 mvdImage
        /// </summary>
        public void TakeOver(CMvdImage image)
        {
            if (_mvdImage == image)
                return;

            cleanUp();
            _mvdImage = image;
            OnImageChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Runtime 使用 C# Bitmap 來觀察 mvdImage 內容.
        /// Caller 不得釋放 此函式所取的取得的 Bitmap !!!
        /// 由於沒有 data copy, 此函式可以大幅增進效能.
        /// </summary>
        public Bitmap PeekBitmap()
        {
            if (_bitmap != null)
                return _bitmap;

            if (_mvdImage != null)
                return GaImageUtil.PeekBmp(_mvdImage);

            return null;
        }

        /// <summary>
        /// Runtime 使用 OpenCvSharp Mat 來觀察 mvdImage 內容.
        /// Caller 不得釋放 此函式所取的取得的 Mat !!!
        /// 由於沒有 data copy, 此函式可以大幅增進效能.
        /// </summary>
        public CMvdImage PeekMvdImage()
        {
            if (_mvdImage != null)
                return _mvdImage;

            if (_bitmap != null)
            {
                _mvdImage = GaImageUtil.BitmapToCMvdImage(_bitmap);
                _bitmap.Dispose();
                _bitmap = null;
            }

            return _mvdImage;
        }

        #region PRIVATE_DATA
        void cleanUp()
        {
            _bitmap?.Dispose();
            _bitmap = null;
            _mvdImage?.Dispose();
            _mvdImage = null;
        }

        #endregion

        #region IMAGE_UTIL_FUNCTIONS
#if (OPT_RESERVED)
        /// <summary>
        /// 暫時性使用 C# Bitmap 來觀察 mvdImage 內容.
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
        /// 暫時性使用 OpenCvSharp Mat 來觀察 mvdImage 內容.
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
#endif
        #endregion
    }
}
