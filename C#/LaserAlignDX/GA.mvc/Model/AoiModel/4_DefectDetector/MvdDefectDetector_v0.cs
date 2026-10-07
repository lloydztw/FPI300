#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-03-16 根據 Gaara 瑕疵檢測代碼, 進行重整包裝
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QvMath;
using JetEazy.Utils;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using VisionDesigner;
using VisionDesigner.ImageArithmetic;

namespace LaserAlignDX.AoiModel.Defects.V0
{
    internal class MvdDefectDetector : IDisposable
    {
        #region GLOBAL_MESS
        RecipeFPIX3Class _xRecipe => RecipeFPIX3Class.Instance;
        InspectX3ParaClass _xInspect => _xRecipe.InspectParams;
        #endregion

        #region MVD_TOOLS
        CImageArithmeticTool cImageArithmeticToolObj = null;// new CImageArithmeticTool();
        VisionDesigner.ImageBinary.CImageBinaryTool cImageBinaryToolObj = null;// new VisionDesigner.ImageBinary.CImageBinaryTool();
        VisionDesigner.ImageMorph.CImageMorphTool cImageMorphToolObj = null;// new VisionDesigner.ImageMorph.CImageMorphTool();
        VisionDesigner.BlobFind.CBlobFindTool cBlobFindToolObj = null;// new VisionDesigner.BlobFind.CBlobFindTool();
        VisionDesigner.ImageAffineTransform.CImageAffineTransformTool cImageAffineTransformToolObj = null;
        //CMvdRectangleF MvdRunPositionFix;
        #endregion

        public void Dispose()
        {
            cImageArithmeticToolObj?.Dispose();
            cImageArithmeticToolObj = null;
            cImageBinaryToolObj?.Dispose();
            cImageBinaryToolObj = null;
            cImageMorphToolObj?.Dispose();
            cImageMorphToolObj = null;
            cBlobFindToolObj?.Dispose();
            cBlobFindToolObj = null;
            cImageAffineTransformToolObj?.Dispose();
            cImageAffineTransformToolObj = null;
        }

        /// <summary>
        /// 使用海康套件, 對單一晶粒進行瑕疵檢測.
        /// </summary>
        public void RunOneChipDefects(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi)
        {
            try
            {
                CMvdRectangleF mvdRectX;

                if (_xInspect.xAlgorithm == MatchAlgorithmEnum.GridMatch)
                {
                    // 框選的模板
                    var templateSize = _xRecipe.bmpDefectTemplate.Size;
                    // 由框選的矩形 轉換成為 Quad
                    var templateQuad = QvQuad2D.From(new RectangleF(0, 0, templateSize.Width, templateSize.Height));

                    var goldenChipQuad = cell.ChipData.GoldenQuad2D;
                    var goldenChipCenter = goldenChipQuad.Center;

                    var runtimeChipQuad = cell.ChipData.ChipQuad2D.Clone();
                    runtimeChipQuad.Offset(-cellRoi.X, -cellRoi.Y);

                    var templatePoints = Array.ConvertAll(templateQuad.Corners, c => new OpenCvSharp.Point2f((float)c.X, (float)c.Y));
                    var goldenPoints = Array.ConvertAll(goldenChipQuad.Corners, c => new OpenCvSharp.Point2f((float)c.X, (float)c.Y));
                    var runtimePoints = Array.ConvertAll(runtimeChipQuad.Corners, c => new OpenCvSharp.Point2f((float)c.X, (float)c.Y));

                    // 將 templatePoints 從 golden domain 投影回到 runtime domain
                    using (var matrix = Cv2.GetPerspectiveTransform(goldenPoints, runtimePoints))
                    {
                        var crop_pts = Cv2.PerspectiveTransform(templatePoints, matrix);
                        var crop_rotatedRect = Cv2.MinAreaRect(crop_pts);
                        var crop_cx = crop_rotatedRect.Center.X;
                        var crop_cy = crop_rotatedRect.Center.Y;
                        var crop_width = crop_rotatedRect.Size.Width;
                        var crop_height = crop_rotatedRect.Size.Height;
                        var angle = crop_rotatedRect.Angle;
                        mvdRectX = new CMvdRectangleF((float)crop_cx, (float)crop_cy, crop_width, crop_height)
                        {
                            Angle = (float)angle
                        };
                    }
                }
                else
                {
                    var templateSize = _xRecipe.bmpDefectTemplate.Size;
                    var chipCenter = cell.ChipData.ChipQuad2D.Center;
                    var chipAngle = cell.ChipData.ChipQuad2D.Angle;

                    // 取的 center 的 local 圖像座標 (cellRoi 左上角為 (0,0))
                    double cx = chipCenter.X - cellRoi.X;
                    double cy = chipCenter.Y - cellRoi.Y;

                    // 以 (cx,cy) 為中心 建立 長寬為 templateSize, 角度為 chipAngle 的 海康矩形
                    // 如果把 _xRecipe.bmpDefectTemplate 當成無角度的 rectangle 就不需要減 GoldenQuad2D.Angle
                    mvdRectX = new CMvdRectangleF((float)cx, (float)cy, templateSize.Width, templateSize.Height)
                    {
                        Angle = (float)chipAngle
                    };
                }

                var bmpTemplate = _xRecipe.bmpDefectTemplate;
                var bmpMask = _xRecipe.bmpprintmask;

                //*****************************************************************************
                // 利用海康 對 cellBmp Affine Transform 
                // (注意:此處會被多線程 同時調用)
                //*****************************************************************************
                using (var cMvdImage = GaImageUtil.BitmapToCMvdImage(cellBmp))
                using (var bmpRun = this.GetAffineTrainsFormRunBmp(cMvdImage, mvdRectX))
                {
                    // 使用海康進行 瑕疵檢測
                    detectDefects(bmpTemplate, bmpRun, bmpMask, cell);
                }
            }
            catch (Exception ex)
            {
                // 簡單 throw, 交給上一層處理.
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        #region PRIVATE_FUNCTIONS
        /// <summary>
        /// // 使用海康進行 瑕疵檢測
        /// </summary>
        void detectDefects(Bitmap bmpTemplate, Bitmap bmpRun, Bitmap bmpMask, RegionCellX3Class cell)
        {
            var chipData = cell?.ChipData;
            var chipCenter = chipData?.ChipQuad2D?.Center;
            if (chipCenter == null)
                return;

            //string dumpFolder = System.IO.Path.Combine(SaveDebugPath, "Detect");
            //if (IsSaveDebugPicture)
            //{
            //    if (!System.IO.Directory.Exists(dumpFolder))
            //        System.IO.Directory.CreateDirectory(dumpFolder);
            //}

            CMvdRectangleF roi = new CMvdRectangleF(bmpTemplate.Width / 2, bmpTemplate.Height / 2, bmpTemplate.Width, bmpTemplate.Height);

            if (cImageArithmeticToolObj == null)
                cImageArithmeticToolObj = new CImageArithmeticTool();
            if (cImageBinaryToolObj == null)
                cImageBinaryToolObj = new VisionDesigner.ImageBinary.CImageBinaryTool();
            if (cImageMorphToolObj == null)
                cImageMorphToolObj = new VisionDesigner.ImageMorph.CImageMorphTool();
            if (cBlobFindToolObj == null)
                cBlobFindToolObj = new VisionDesigner.BlobFind.CBlobFindTool();

            cImageArithmeticToolObj.InputImage1 = GaImageUtil.BitmapToCMvdImage(bmpTemplate);
            cImageArithmeticToolObj.InputImage2 = GaImageUtil.BitmapToCMvdImage(bmpRun);
            cImageArithmeticToolObj.SetRunParam("ArithmeticType", "Subtract");

            cImageArithmeticToolObj.ROI = roi;
            //= new VisionDesigner.CMvdRectangleF(eTemplate.Width / 2, eTemplate.Height / 2, eTemplate.Width, eTemplate.Height);
            cImageArithmeticToolObj.Run();
            VisionDesigner.CMvdImage OutputImage = cImageArithmeticToolObj.Result.OutputImage;

            //OutputImage.SaveImage($"{_path}\\{lblName}_Diff.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);

            //二值化
            cImageBinaryToolObj.InputImage = OutputImage;
            cImageBinaryToolObj.ROI = null;
            //= new CMvdRectangleF(OutputImage.Width / 2, OutputImage.Height / 2, OutputImage.Width / 4, OutputImage.Height / 4);
            cImageBinaryToolObj.SetRunParam("LowThreshold", _xInspect.xThresholdValue.ToString());
            //cImageArithmeticToolObj.SetRunParam("HighThreshold", BlobHighThreshold.ToString());
            cImageBinaryToolObj.Run();
            //cImageBinaryToolObj.Result.OutputImage.SaveImage($"{_path}\\{lblName}_Diff2.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);

            ////形态学
            //cImageMorphToolObj.InputImage = cImageBinaryToolObj.Result.OutputImage;
            //cImageMorphToolObj.SetRunParam("Type", "Open");
            //cImageMorphToolObj.ROI = _roi;
            ////= new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width / 4, cInputImg.Height / 4);
            //cImageMorphToolObj.Run();

            //blob
            cBlobFindToolObj.InputImage = cImageBinaryToolObj.Result.OutputImage;
            cBlobFindToolObj.RegionImage = GaImageUtil.BitmapToCMvdImage(bmpMask);
            cBlobFindToolObj.ROI = roi;
            //= new CMvdRectangleF(OutputImage.Width / 2, OutputImage.Height / 2, OutputImage.Width, OutputImage.Height);
            cBlobFindToolObj.SetRunParam("Polarity", "BrightObject");

            cBlobFindToolObj.BasicParam.ShowBlobImageStatus = true;
            cBlobFindToolObj.Run();
            VisionDesigner.BlobFind.CBlobFindResult cBlobFindRes = cBlobFindToolObj.Result;

            //cBlobFindToolObj.RegionImage.SaveImage($"{_path}\\{lblName}_Diff2_1.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);

            //if (cBlobFindRes.BlobImage != null)
            //    cBlobFindRes.BlobImage.SaveImage($"{_path}\\{lblName}_Diff3.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);

            //Console.WriteLine("Blob Num: {0}", cBlobFindRes.BlobInfo.Count);
            //bool bOK = true;

            var defectRunRoiSize = bmpRun.Size;
            var blobMvdRectFNGList = new List<CMvdRectangleF>();

            foreach (var item in cBlobFindToolObj.Result.BlobInfo)
            {
                //Console.WriteLine("Index: {0}, Angle {1}", item.DomainIndex, item.BoxInfo.Angle);
                if (item.AreaF > _xInspect.xCharArea)
                {
                    blobMvdRectFNGList.Add(item.BoxInfo);
                }
                else if (item.LongAxis > _xInspect.xCharWidth && item.ShortAxis > _xInspect.xCharHeight)
                {
                    blobMvdRectFNGList.Add(item.BoxInfo);
                }
                //else if (item.LongAxis > xInspectPara.xCharWidth)
                //{
                //    blobMvdRectFNGList.Add(item.BoxInfo);
                //}
                //else if (item.ShortAxis > xInspectPara.xCharHeight)
                //{
                //    blobMvdRectFNGList.Add(item.BoxInfo);
                //}
            }

            if (blobMvdRectFNGList.Count > 0)
            {
                //inspectReasons.Add(InspectReason.INS_DEFECTERR);
                cell.MarkResult(InspectReason.NG_APPEARANCE);

                List<CMvdRectangleF> cMvdRectangleFs = new List<CMvdRectangleF>();
                foreach (CMvdRectangleF mvdRectangleF in blobMvdRectFNGList)
                {
                    mvdRectangleF.CenterX += (float)(chipCenter.X - defectRunRoiSize.Width / 2);  // bmpItemRun.Width / 2;
                    mvdRectangleF.CenterY += (float)(chipCenter.Y - defectRunRoiSize.Height / 2); // bmpItemRun.Height / 2;
                    //mvdRectangleF.BorderColor = new MVD_COLOR(255, 0, 0);
                    //mvdRectangleF.BorderWidth = 1;
                    cMvdRectangleFs.Add(mvdRectangleF);
                }

                chipData.DefectBlobs = Array.ConvertAll(cMvdRectangleFs.ToArray(), b => b.ToBox2D());

                if (cell.IsSaveDebugPicture)
                {
                    //cImageBinaryToolObj?.Result?.OutputImage?.SaveImage($"{SaveDebugPath}\\Detect\\{lblName}_Diff2.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    //cBlobFindToolObj?.RegionImage?.SaveImage($"{SaveDebugPath}\\Detect\\{lblName}_Diff2_1.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    ////if (cBlobFindRes.BlobImage != null)
                    //cBlobFindRes?.BlobImage?.SaveImage($"{SaveDebugPath}\\Detect\\{lblName}_Diff3.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);

                    string dumpFolder = System.IO.Path.Combine(cell.SaveDebugPath, "Detect");
                    if (!System.IO.Directory.Exists(dumpFolder))
                        System.IO.Directory.CreateDirectory(dumpFolder);

                    string fileStem = System.IO.Path.Combine(dumpFolder, cell.lblName);
                    cImageArithmeticToolObj?.InputImage1?.SaveImage(fileStem + "_Template.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    cImageArithmeticToolObj?.InputImage2?.SaveImage(fileStem + "_Run.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    cImageBinaryToolObj?.Result?.OutputImage?.SaveImage(fileStem + "_Diff2.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    cBlobFindToolObj?.RegionImage?.SaveImage(fileStem + "_Diff2_1.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    cBlobFindRes?.BlobImage?.SaveImage(fileStem + "_Diff3.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                }
            }
        }
        #endregion

        #region MVD_IMAGE_AFFINETRANSFORM
        /// <summary>
        /// 抓取全域图形的转正图像 位置框由前期定位决定
        /// </summary>
        public Bitmap GetAffineTrainsFormRunBmp(CMvdImage cMvdImage, CMvdShape cMvdShape)
        {
            if (cImageAffineTransformToolObj == null)
                cImageAffineTransformToolObj = new VisionDesigner.ImageAffineTransform.CImageAffineTransformTool();
            cImageAffineTransformToolObj.BasicParam.Aspect = 1;
            cImageAffineTransformToolObj.InputImage = cMvdImage;
            cImageAffineTransformToolObj.ROIShape = cMvdShape;
            cImageAffineTransformToolObj.Run();
            //输出结果
            return CMvdImageToBitmapEx(cImageAffineTransformToolObj.Result.OutputImage);
        }
        /// <summary>
        /// cMvdImage 转 Bitmap
        /// </summary>
        Bitmap CMvdImageToBitmapEx(CMvdImage cMvdImage)
        {
            Bitmap bmpInputImg = null;
            byte[] buffer = new byte[cMvdImage.GetImageData(0).arrDataBytes.Length];
            buffer = cMvdImage.GetImageData(0).arrDataBytes;

            if (MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08 == cMvdImage.PixelFormat)
            {
                Int32 imageWidth = Convert.ToInt32(cMvdImage.Width);
                Int32 imageHeight = Convert.ToInt32(cMvdImage.Height);
                PixelFormat bitMaPixelFormat = PixelFormat.Format8bppIndexed;
                bmpInputImg = new Bitmap(imageWidth, imageHeight, bitMaPixelFormat);
                int offset = imageWidth % 4 != 0 ? (4 - imageWidth % 4) : 0;//添加冗余位，变成4的倍数
                int strid = imageWidth + offset;
                int bitmapBytesLenth = strid * imageHeight;
                byte[] bitmapDataBytes = new byte[bitmapBytesLenth];
                for (int i = 0; i < imageHeight; i++)
                {
                    for (int j = 0; j < strid; j++)
                    {
                        int bitIndex = i * strid + j;
                        int mvdIndex = i * imageWidth + j;
                        if (j >= imageWidth)
                        {
                            bitmapDataBytes[bitIndex] = 0;//冗余位填充0
                        }
                        else
                        {
                            bitmapDataBytes[bitIndex] = buffer[mvdIndex];
                        }
                    }
                }
                BitmapData bitmapData = bmpInputImg.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.WriteOnly, bitMaPixelFormat);
                IntPtr imageBufferPtr = bitmapData.Scan0;
                Marshal.Copy(bitmapDataBytes, 0, imageBufferPtr, bitmapBytesLenth);
                bmpInputImg.UnlockBits(bitmapData);

                var colorPalettes = bmpInputImg.Palette;
                for (int j = 0; j < 256; j++)
                {
                    colorPalettes.Entries[j] = Color.FromArgb(j, j, j);
                }
                bmpInputImg.Palette = colorPalettes;
            }
            else if (MVD_PIXEL_FORMAT.MVD_PIXEL_RGB_RGB24_C3 == cMvdImage.PixelFormat)
            {
                Int32 imageWidth = Convert.ToInt32(cMvdImage.Width);
                Int32 imageHeight = Convert.ToInt32(cMvdImage.Height);
                PixelFormat bitMaPixelFormat = PixelFormat.Format24bppRgb;
                bmpInputImg = new Bitmap(imageWidth, imageHeight, bitMaPixelFormat);
                int offset = imageWidth % 4 != 0 ? (4 - (imageWidth * 3) % 4) : 0;//添加冗余位，变成4的倍数
                int strid = imageWidth * 3 + offset;
                int bitmapBytesLenth = strid * imageHeight;
                byte[] bitmapDataBytes = new byte[bitmapBytesLenth];
                for (int i = 0; i < imageHeight; i++)
                {
                    for (int j = 0; j < imageWidth; j++)
                    {
                        int mvdIndex = i * imageWidth * 3 + j * 3;
                        int bitIndex = i * strid + j * 3;
                        bitmapDataBytes[bitIndex] = buffer[mvdIndex + 2];
                        bitmapDataBytes[bitIndex + 1] = buffer[mvdIndex + 1];
                        bitmapDataBytes[bitIndex + 2] = buffer[mvdIndex];
                    }
                    for (int k = 0; k < offset; k++)
                    {
                        bitmapDataBytes[i * strid + imageWidth * 3 + k] = 0;
                    }
                }
                BitmapData bitmapData = bmpInputImg.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.WriteOnly, bitMaPixelFormat);
                IntPtr imageBufferPtr = bitmapData.Scan0;
                Marshal.Copy(bitmapDataBytes, 0, imageBufferPtr, bitmapBytesLenth);
                bmpInputImg.UnlockBits(bitmapData);
            }
            return bmpInputImg;
        }
        #endregion
    }
}
