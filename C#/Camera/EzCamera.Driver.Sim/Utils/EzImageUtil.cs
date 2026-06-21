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

using EzCamera.Driver.Utils;
using JetEazy.EzImage;
using JetEazy.OpenCV;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;

namespace EzCamera.Driver.Sim
{
    public static class EzImageUtil
    {
        /// <summary>
        /// 載入巨圖 (caller 需要負責釋放 new 出來的 Bitmap 資源)
        /// <summary>
        public static Bitmap LoadBigImage(string fileName, bool toU8 = true, bool autoSaveJpg = false)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new Exception($"檔案不存在: {fileName}");

            Bitmap bigBmp = loadBigImageViaQuickImage(fileName, toU8);

            //if (option == 0)
            //{
            //    //NOTE: 使用 QuickImage (IEzImage) 載入圖檔 耗時 1.2 seconds
            //    bigBmp = loadBigImageViaQuickImage(fileName, false);
            //}
            //else
            //{
            //    //NOTE: 使用 FreeImageBitmap 載入圖檔 耗時 2.7 s
            //    bigBmp = loadBigImageViaFreeImageBitmap(fileName, false);
            //}

            if (autoSaveJpg && System.IO.Path.GetExtension(fileName).ToLower() != ".jpg")
            {
                var autoJpgFile = System.IO.Path.ChangeExtension(fileName, ".jpg");
                SaveBigImage(autoJpgFile, bigBmp);
            }

            return bigBmp;
        }
        public static bool SaveBigImage(string fileName, Bitmap bigBmp)
        {
            if (bigBmp == null) 
                return false;
            
            string path = System.IO.Path.GetDirectoryName(fileName);
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            bool ok;
            using (QxImageBridge bridge = new QxImageBridge(bigBmp))
            {
                ok = bridge.Image.SaveImage(fileName);
            }
            return ok;
        }

        #region PRIVATE_FUNCTIONS
        /// <summary>
        /// 利用 EzQuickImage (IEzImage) (OpenCvSharp) 載入 8 bpp 影像檔
        /// (無法載入時, 會拋出異常!)
        /// </summary>
        static Bitmap loadBigImageViaQuickImage(string fileName, bool toU8 = true, bool check = true)
        {
            if (check && string.IsNullOrEmpty(fileName))
                throw new Exception($"檔案不存在: {fileName}");

            using (EzQuickImage ezImage = new EzQuickImage())
            {
                try
                {
                    if (toU8)
                    {
                        ezImage.Load(fileName, bits: 8);
                        if (ezImage.Image is Mat img)
                        {
                            Bitmap bmp = BitmapConverter.ToBitmap(img);
                            return bmp;
                        }
                    }
                    else
                    {
                        ezImage.Load(fileName);
                        if (ezImage.Image is Mat img)
                        {
                            Bitmap bmp = BitmapConverter.ToBitmap(img);
                            return bmp;
                        }
                    }
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("loadBigImageViaQuickImage 異常 :\n" + ex.Message);
                    throw;
                }
            }

            // 如果使用 EzQuickImage 失敗.
            // 最後機會, 再使用 FreeBitmap.
            Bitmap bigBmp = loadBigImageViaFreeImageBitmap(fileName);
            return bigBmp;
        }
        /// <summary>
        /// 利用 FreeImageBitmap 載入 8 bpp 影像檔
        /// (無法載入時, 會拋出異常!)
        /// </summary>
        static Bitmap loadBigImageViaFreeImageBitmap(string fileName, bool toU8 = true, bool check = true)
        {
            if (check && string.IsNullOrEmpty(fileName))
                throw new Exception($"檔案不存在: {fileName}");

            using (IEzImage image = new EzFreeBitmap())
            {
                image.Load(fileName);
                Bitmap bigBmp = image.Bitmap;

                try
                {
                    var ps = new EzImageProcess();
                    if (toU8)
                    {
                        var bmp = ps.ToU8(bigBmp, false);
                        if (bmp == bigBmp)
                            bmp = (Bitmap)bmp.Clone();
                        return bmp;
                    }
                    else
                    {
                        var bmp = ps.ToRgb24(bigBmp, false);
                        if (bmp == bigBmp)
                            bmp = (Bitmap)bmp.Clone();
                        return bmp;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("loadBigImageViaFreeImageBitmap 異常 :\n" + ex.Message);
                    throw;
                }
            }
        }
        #endregion
    }
}
