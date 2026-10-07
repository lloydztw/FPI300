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
using VisionDesigner;
using VisionDesigner.ImageArithmetic;


namespace LaserAlignDX.Model.Defects.V2
{
    /// <summary>
    /// 使用海康套件, 對單一晶粒進行瑕疵檢測.
    /// </summary>
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
            //*****************************************************************************
            // 此處, 可能會被 "多線程" 同時調用!
            //*****************************************************************************

            try
            {
                var chipData = cell?.ChipData;
                if (chipData == null)
                    return;

                CMvdRectangleF mvdAffineRect = getAffineRect(cell, ref cellRoi);

                using (var imgTemplate = GaImageUtil.BitmapToCMvdImage(_xRecipe.bmpDefectTemplate))
                using (var imgMask = GaImageUtil.BitmapToCMvdImage(_xRecipe.bmpprintmask))
                using (var imgCell = GaImageUtil.BitmapToCMvdImage(cellBmp))
                using (var imgRun = applyAffineTransform(imgCell, mvdAffineRect))
                {
                    if( imgRun.Width != imgTemplate.Width || 
                        imgRun.Height != imgTemplate.Height ||
                        imgRun.PixelFormat != imgTemplate.PixelFormat )
                    {
                        var errMsg = $"imgRun @ [{cell.CellRow},{cell.CellCol}] 尺寸X,尺寸Y,或格式不同!";
                        System.Diagnostics.Debug.WriteLine( errMsg );
                    }
                    // 使用海康進行 瑕疵檢測
                    chipData.DefectBlobs = detectDefectBlobs(imgTemplate, imgRun, imgMask, cell);
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
        /// 使用海康, 取的 映射 旋轉矩形
        /// </summary>
        CMvdRectangleF getAffineRect(RegionCellX3Class cell, ref RectangleF cellRoi)
        {
            CMvdRectangleF mvdAffineRect;

            if (_xInspect.xAlgorithm == MatchAlgorithmEnum.GridMatch)
            {
                //(1) 取得 模板的 "手拉框" 四邊形 (來自 參數設定)
                var templateSize = _xRecipe.bmpDefectTemplate.Size;
                var templateQuad = QvQuad2D.From(new RectangleF(0, 0, templateSize.Width, templateSize.Height));

                //(2) 取得 模板的 "黃金" 四邊形 (經由 ChipLocator 分析 參數設定 算出來的)
                var goldenChipQuad = cell.ChipData.GoldenQuad2D;
                var goldenChipCenter = goldenChipQuad.Center;

                //(3) 取的 當下定位出的 "晶粒" 四邊形 (經由 ChipLocator 實時定位出來的)
                var runtimeChipQuad = cell.ChipData.ChipQuad2D.Clone();
                runtimeChipQuad.Offset(-cellRoi.X, -cellRoi.Y);

                //(4) 轉換成 OpenCvSharp.Point2f[]
                var templatePoints = Array.ConvertAll(templateQuad.Corners, c => new OpenCvSharp.Point2f((float)c.X, (float)c.Y));
                var goldenPoints = Array.ConvertAll(goldenChipQuad.Corners, c => new OpenCvSharp.Point2f((float)c.X, (float)c.Y));
                var runtimePoints = Array.ConvertAll(runtimeChipQuad.Corners, c => new OpenCvSharp.Point2f((float)c.X, (float)c.Y));

                //(5) 將 templatePoints 從 Golden domain 投影回到 Runtime domain. 然後建構 mvdAffineRect
                using (var matrix = Cv2.GetPerspectiveTransform(goldenPoints, runtimePoints))
                {
                    var crop_pts = Cv2.PerspectiveTransform(templatePoints, matrix);
                    var crop_rotatedRect = Cv2.MinAreaRect(crop_pts);
                    var crop_cx = crop_rotatedRect.Center.X;
                    var crop_cy = crop_rotatedRect.Center.Y;
                    var crop_width = templateSize.Width;    // crop_rotatedRect.Size.Width;
                    var crop_height = templateSize.Height;  // crop_rotatedRect.Size.Height;
                    var angle = crop_rotatedRect.Angle;
                    mvdAffineRect = new CMvdRectangleF((float)crop_cx, (float)crop_cy, crop_width, crop_height)
                    {
                        Angle = (float)angle
                    };
                }
            }
            else
            {
                // 模板大小
                var templateSize = _xRecipe.bmpDefectTemplate.Size;

                // 從定位取得的 chipCenter 與 chipAngle 
                var chipCenter = cell.ChipData.ChipQuad2D.Center;
                var chipAngle = cell.ChipData.ChipQuad2D.Angle;

                // 平移後的 chipCenter 的圖像座標 (以 cellRoi 左上角為 零點)
                double cx = chipCenter.X - cellRoi.X;
                double cy = chipCenter.Y - cellRoi.Y;

                // 以 (cx,cy) 為中心 建立 長寬為 templateSize, 角度為 chipAngle 的 海康矩形
                // 如果把 _xRecipe.bmpDefectTemplate 當成無角度的 rectangle 就不需要減 GoldenQuad2D.Angle
                mvdAffineRect = new CMvdRectangleF((float)cx, (float)cy, templateSize.Width, templateSize.Height)
                {
                    Angle = (float)chipAngle
                };
            }

            return mvdAffineRect;
        }

        /// <summary>
        /// 使用海康, 對影像進行映射轉換
        /// </summary>
        CMvdImage applyAffineTransform(CMvdImage cMvdImage, CMvdShape cMvdShape)
        {
            if (cImageAffineTransformToolObj == null)
                cImageAffineTransformToolObj = new VisionDesigner.ImageAffineTransform.CImageAffineTransformTool();
            cImageAffineTransformToolObj.BasicParam.Aspect = 1;
            cImageAffineTransformToolObj.InputImage = cMvdImage;
            cImageAffineTransformToolObj.ROIShape = cMvdShape;
            cImageAffineTransformToolObj.Run();
            return cImageAffineTransformToolObj.Result.OutputImage.Clone();
        }

        /// <summary>
        /// 使用海康, 進行瑕疵檢測 (簡單減法)
        /// </summary>
        QvBox2D[] detectDefectBlobs(CMvdImage imgTemplate, CMvdImage imgRun, CMvdImage imgMask, RegionCellX3Class cell)
        {
            var chipData = cell?.ChipData;
            var chipCenter = chipData?.ChipQuad2D?.Center;
            if (chipCenter == null)
                return null;

            //string dumpFolder = System.IO.Path.Combine(SaveDebugPath, "Detect");
            //if (IsSaveDebugPicture)
            //{
            //    if (!System.IO.Directory.Exists(dumpFolder))
            //        System.IO.Directory.CreateDirectory(dumpFolder);
            //}

            CMvdRectangleF roi = new CMvdRectangleF(imgTemplate.Width / 2f, imgTemplate.Height / 2f, imgTemplate.Width, imgTemplate.Height);

            if (cImageArithmeticToolObj == null)
                cImageArithmeticToolObj = new CImageArithmeticTool();
            if (cImageBinaryToolObj == null)
                cImageBinaryToolObj = new VisionDesigner.ImageBinary.CImageBinaryTool();
            if (cImageMorphToolObj == null)
                cImageMorphToolObj = new VisionDesigner.ImageMorph.CImageMorphTool();
            if (cBlobFindToolObj == null)
                cBlobFindToolObj = new VisionDesigner.BlobFind.CBlobFindTool();

            cImageArithmeticToolObj.InputImage1 = imgTemplate;  // GaImageUtil.BitmapToCMvdImage(bmpTemplate);
            cImageArithmeticToolObj.InputImage2 = imgRun;       // GaImageUtil.BitmapToCMvdImage(bmpRun);
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
            cBlobFindToolObj.RegionImage = imgMask; // GaImageUtil.BitmapToCMvdImage(bmpMask);
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

            var defectRunRoiSize = new System.Drawing.Size((int)imgRun.Width, (int)imgRun.Height);
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

                //chipData.DefectBlobs = Array.ConvertAll(cMvdRectangleFs.ToArray(), b => b.ToBox2D());

                if (cell.IsSaveDebugPicture)
                {
                    #region 保存調試用的圖片檔
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
                    #endregion
                }

                var results =Array.ConvertAll(cMvdRectangleFs.ToArray(), b => b.ToBox2D());
                return results;
            }

            return null;
        }
        #endregion
    }
}
