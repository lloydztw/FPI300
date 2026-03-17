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

using JetEazy.OpenCV;
using JetEazy.QvMath;
using JetEazy.Utils;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using VisionDesigner;
using VisionDesigner.ImageArithmetic;


namespace LaserAlignDX.Model.Defects.V3
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

        #region COMMON_IMAGE
        CMvdImage _imgTemplate;
        CMvdImage _imgMask;
        #endregion

        public void Dispose()
        {
            _imgTemplate?.Dispose();
            _imgTemplate = null;

            _imgMask?.Dispose();
            _imgMask = null;

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

        public void Init()
        {
            _imgTemplate?.Dispose();
            _imgTemplate = GaImageUtil.BitmapToCMvdImage(_xRecipe.bmpDefectTemplate);

            _imgMask?.Dispose();
            _imgMask = GaImageUtil.BitmapToCMvdImage(_xRecipe.bmpprintmask);

            if (cImageArithmeticToolObj == null)
                cImageArithmeticToolObj = new CImageArithmeticTool();
            if (cImageBinaryToolObj == null)
                cImageBinaryToolObj = new VisionDesigner.ImageBinary.CImageBinaryTool();
            if (cImageMorphToolObj == null)
                cImageMorphToolObj = new VisionDesigner.ImageMorph.CImageMorphTool();
            if (cBlobFindToolObj == null)
                cBlobFindToolObj = new VisionDesigner.BlobFind.CBlobFindTool();
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

                using (var imgRun = CropRuntimeChipImage(cell, cellBmp))
                {
                    // 使用海康進行 瑕疵檢測
                    var blobs = detectDefectBlobs(_imgTemplate, imgRun, _imgMask, cell);
                    chipData.DefectBlobs = blobs;
                }
            }
            catch (Exception ex)
            {
                // 簡單 throw, 交給上一層處理.
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        #region PRIVATE_TRANSFORM_FUNCTIONS
        /// <summary>
        /// 根據 runtime cell 數據 與 瑕疵參數設定 取得 轉換矩陣
        /// </summary>
        void getPerspectiveTransformMatrices(RegionCellX3Class cell, out Mat matrixToGolden)
        {
            //(1) 取得 模板的 "手拉框" 四邊形 (來自 參數設定)
            var templateSize = new OpenCvSharp.Size(_imgTemplate.Width, _imgTemplate.Height);
            var templateQuad = QvQuad2D.From(new RectangleF(0, 0, templateSize.Width, templateSize.Height));

            //(2) 取得 模板的 "黃金" 四邊形 (經由 ChipLocator 分析 參數設定 算出來的)
            var chipData = cell.ChipData;
            var goldenChipQuad = chipData.GoldenQuad2D.Clone();
            var goldenChipCenter = goldenChipQuad.Center;

            //(3) 取的 當下定位出的 "晶粒" 四邊形 (經由 ChipLocator 實時定位出來的)
            var runtimeChipQuad = chipData.ChipQuad2D.Clone();
            //(3.1) 平移 (以 cellRoi 左上角為零點)
            var cellRoi = chipData.CellRoi;
            runtimeChipQuad.Offset(-cellRoi.X, -cellRoi.Y);

            //(4) 轉換成 OpenCvSharp.Point2f[]
            var templatePoints = Array.ConvertAll(templateQuad.Corners, c => new OpenCvSharp.Point2f((float)c.X, (float)c.Y));
            var goldenChipPoints = Array.ConvertAll(goldenChipQuad.Corners, c => new OpenCvSharp.Point2f((float)c.X, (float)c.Y));
            var runtimeChipPoints = Array.ConvertAll(runtimeChipQuad.Corners, c => new OpenCvSharp.Point2f((float)c.X, (float)c.Y));

            //(5) 取得 Golden To Runtime 的投影陣列
            using (var matrixToRuntime = Cv2.GetPerspectiveTransform(goldenChipPoints, runtimeChipPoints))
            {
                //(6) 取得 Runtime To Golden 的投影陣列
                var runtimeCropPoints = Cv2.PerspectiveTransform(templatePoints, matrixToRuntime);
                matrixToGolden = Cv2.GetPerspectiveTransform(runtimeCropPoints, templatePoints);
            }
        }
        
        /// <summary>
        /// 根據 runtime cell 數據 與 轉換矩陣, 取出 MVD 瑕疵檢測 所需要的 imgRun (CMvdImage)
        /// </summary>
        CMvdImage CropRuntimeChipImage(RegionCellX3Class cell, Bitmap cellBmp, Mat matrixToGolden = null)
        {
            if (_xInspect.xAlgorithm == MatchAlgorithmEnum.GridMatch)
                return CropRuntimeChipImage_perspectively(cell, cellBmp, matrixToGolden);
            else
                return CropRuntimeChipImage_mvd(cell, cellBmp);
        }

        /// <summary>
        /// 【格點型 晶粒】根據 runtime cell 數據 與 轉換矩陣, 取出 MVD 瑕疵檢測 所需要的 imgRun (CMvdImage)
        /// </summary>
        CMvdImage CropRuntimeChipImage_perspectively(RegionCellX3Class cell, Bitmap cellBmp, Mat matrixToGoldenArg = null)
        {
            //(0) 投影轉換矩陣
            Mat matrixToGolden = matrixToGoldenArg;
            if (matrixToGolden == null)
                getPerspectiveTransformMatrices(cell, out matrixToGolden);

            //(1) 取得 模板的 "手拉框" 四邊形 (來自 參數設定)
            var templateSize = new OpenCvSharp.Size(_imgTemplate.Width, _imgTemplate.Height);

            //(2) 使用 OpenCvSharp, 把 cellBmp 內的 runtime chip 影像 投影 至 golden domain.
            using (var srcImgBridge = new QxImageBridge(cellBmp))
            using (var dstImg = new Mat(templateSize, srcImgBridge.Image.Type()))
            {
                // 進行投影
                Cv2.WarpPerspective(
                    srcImgBridge.Image,
                    dstImg,
                    matrixToGolden,
                    templateSize,
                    InterpolationFlags.Linear | InterpolationFlags.Cubic // 混合插值提昇品質
                );

                // clean up
                if (matrixToGolden != matrixToGoldenArg)
                    matrixToGolden?.Dispose();

                // 轉換影像格式至 CMvdImage
                using (Bitmap bmpRun = dstImg.ToBitmap())
                {
                    CMvdImage imgRun = bmpRun.ToCMvdImage();
                    return imgRun;
                }
            }
        }

        /// <summary>
        /// 【一般型 晶粒】根據 runtime cell 數據 與 轉換矩陣, 取出 MVD 瑕疵檢測 所需要的 imgRun (CMvdImage)
        /// </summary>
        CMvdImage CropRuntimeChipImage_mvd(RegionCellX3Class cell, Bitmap cellBmp)
        {
            //(1) 使用海康, 取的 映射 旋轉矩形
            
            //(1.1) 模板大小
            var templateSize = new System.Drawing.Size((int)_imgTemplate.Width, (int)_imgTemplate.Height);

            //(1.2) 從定位取得的 chipCenter 與 chipAngle 
            var chipData = cell.ChipData;
            var chipCenter = chipData.ChipQuad2D.Center;
            var chipAngle = chipData.ChipQuad2D.Angle;

            //(1.3) 平移後的 chipCenter 的圖像座標 (以 cellRoi 左上角為 零點)
            var cellRoi = chipData.CellRoi;
            double cx = chipCenter.X - cellRoi.X;
            double cy = chipCenter.Y - cellRoi.Y;

            //(1.4) 以 (cx,cy) 為中心 建立 長寬為 templateSize, 角度為 chipAngle 的 海康矩形
            //      如果把 _xRecipe.bmpDefectTemplate 當成無角度的 rectangle 就不需要減 GoldenQuad2D.Angle
            var mvdAffineRect = new CMvdRectangleF((float)cx, (float)cy, templateSize.Width, templateSize.Height)
            {
                Angle = (float)chipAngle
            };

            //(2) 使用海康 進行 affine transform
            using (var imgCell = GaImageUtil.BitmapToCMvdImage(cellBmp))
            {
                if (cImageAffineTransformToolObj == null)
                    cImageAffineTransformToolObj = new VisionDesigner.ImageAffineTransform.CImageAffineTransformTool();
                cImageAffineTransformToolObj.BasicParam.Aspect = 1;
                cImageAffineTransformToolObj.InputImage = imgCell;
                cImageAffineTransformToolObj.ROIShape = mvdAffineRect;
                cImageAffineTransformToolObj.Run();
                CMvdImage imgRun = cImageAffineTransformToolObj.Result.OutputImage.Clone();
                return imgRun;
            }
        }
        #endregion

        #region PRIVATE_MVD_DEFECT_FUNCTIONS
        /// <summary>
        /// 使用海康, 進行瑕疵檢測 (簡單減法)
        /// 此處 pixel 座標是以 imgTemplate 左上角為零點
        /// </summary>
        QvBox2D[] detectDefectBlobs(CMvdImage imgTemplate, CMvdImage imgRun, CMvdImage imgMask, RegionCellX3Class cell)
        {
            var chipData = cell?.ChipData;
            if (chipData == null)
                return null;
            var chipCenter = chipData?.ChipQuad2D.Center;
            if (chipCenter == null)
                return null;

            //string dumpFolder = System.IO.Path.Combine(SaveDebugPath, "Detect");
            //if (IsSaveDebugPicture)
            //{
            //    if (!System.IO.Directory.Exists(dumpFolder))
            //        System.IO.Directory.CreateDirectory(dumpFolder);
            //}

            CMvdRectangleF mvdRoi = new CMvdRectangleF(imgTemplate.Width / 2f, imgTemplate.Height / 2f, imgTemplate.Width, imgTemplate.Height);

            //if (cImageArithmeticToolObj == null)
            //    cImageArithmeticToolObj = new CImageArithmeticTool();
            //if (cImageBinaryToolObj == null)
            //    cImageBinaryToolObj = new VisionDesigner.ImageBinary.CImageBinaryTool();
            //if (cImageMorphToolObj == null)
            //    cImageMorphToolObj = new VisionDesigner.ImageMorph.CImageMorphTool();
            //if (cBlobFindToolObj == null)
            //    cBlobFindToolObj = new VisionDesigner.BlobFind.CBlobFindTool();

            cImageArithmeticToolObj.InputImage1 = imgTemplate;  // GaImageUtil.BitmapToCMvdImage(bmpTemplate);
            cImageArithmeticToolObj.InputImage2 = imgRun;       // GaImageUtil.BitmapToCMvdImage(bmpRun);
            cImageArithmeticToolObj.SetRunParam("ArithmeticType", "Subtract");

            cImageArithmeticToolObj.ROI = mvdRoi;
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
            cBlobFindToolObj.ROI = mvdRoi;
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
                    //-------------------------------------------------------------------------------------------------------------
                    // 因為此處 mvdRectangleF 已經是投影在 golden domain 的座標,
                    // 直接加平移 不太準確
                    // 將來最好作法是: 交給上層 進行 回歸投影.
                    //-------------------------------------------------------------------------------------------------------------
                    mvdRectangleF.CenterX += (float)(chipCenter.X - imgRun.Width / 2f);
                    mvdRectangleF.CenterY += (float)(chipCenter.Y - imgRun.Height / 2f);
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
