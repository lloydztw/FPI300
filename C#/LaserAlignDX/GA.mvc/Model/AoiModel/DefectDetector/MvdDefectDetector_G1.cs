#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-07-09 優化多執行緒重啟快取問題、啟用形態學開運算消除邊緣對齊假點
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
using Traveller106;
using VisionDesigner;
using VisionDesigner.ImageArithmetic;

namespace LaserAlignDX.Model.Defects.G1
{
    /// <summary>
    /// 使用海康套件, 對單一晶粒進行瑕疵檢測.
    /// (注意：此 Class 外部設計為每個執行緒單獨 new 出來應用)
    /// </summary>
    internal class MvdDefectDetector : IDisposable
    {
        #region GLOBAL_MESS
        RecipeFPIX3Class _xRecipe => RecipeFPIX3Class.Instance;
        InspectX3ParaClass _xInspect => _xRecipe.InspectParams;
        INI _ini => INI.Instance;
        #endregion

        #region MVD_TOOLS
        CImageArithmeticTool _cImageArithmeticTool = null;
        VisionDesigner.ImageBinary.CImageBinaryTool _cImageBinaryTool = null;
        VisionDesigner.ImageMorph.CImageMorphTool _cImageMorphTool = null;
        VisionDesigner.BlobFind.CBlobFindTool _cBlobFindTool = null;
        VisionDesigner.ImageAffineTransform.CImageAffineTransformTool _cImageAffineTransformTool = null;
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

            _cImageArithmeticTool?.Dispose();
            _cImageArithmeticTool = null;
            _cImageBinaryTool?.Dispose();
            _cImageBinaryTool = null;
            _cImageMorphTool?.Dispose();
            _cImageMorphTool = null;
            _cBlobFindTool?.Dispose();
            _cBlobFindTool = null;
            _cImageAffineTransformTool?.Dispose();
            _cImageAffineTransformTool = null;
        }

        public void Init()
        {
            // 1. 重新載入標準影像與遮罩
            _imgTemplate?.Dispose();
            _imgTemplate = GaImageUtil.BitmapToCMvdImage(_xRecipe.bmpDefectTemplate);

            _imgMask?.Dispose();
            _imgMask = GaImageUtil.BitmapToCMvdImage(_xRecipe.bmpprintmask);

            // 2. 關鍵修正：重啟時強制 Dispose 舊工具並重新 new，逼迫海康清空底層 C++ 影像指標與快取
            _cImageArithmeticTool?.Dispose();
            _cImageArithmeticTool = new CImageArithmeticTool();

            _cImageBinaryTool?.Dispose();
            _cImageBinaryTool = new VisionDesigner.ImageBinary.CImageBinaryTool();

            _cImageMorphTool?.Dispose();
            _cImageMorphTool = new VisionDesigner.ImageMorph.CImageMorphTool();

            _cBlobFindTool?.Dispose();
            _cBlobFindTool = new VisionDesigner.BlobFind.CBlobFindTool();

            _cImageAffineTransformTool?.Dispose();
            _cImageAffineTransformTool = new VisionDesigner.ImageAffineTransform.CImageAffineTransformTool();
        }

        /// <summary>
        /// 使用海康套件, 對單一晶粒進行瑕疵檢測.
        /// </summary>
        public void RunOneChipDefects(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi)
        {
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
                throw new Exception("MVD : detectDefectBlobs Error!", ex);
            }
        }

        #region PRIVATE_TRANSFORM_FUNCTIONS
        /// <summary>
        /// 根據 runtime cell 數據 與 瑕疵參數設定 取得 轉換矩陣
        /// </summary>
        void getPerspectiveTransformMatrices(RegionCellX3Class cell, out Mat matrixToGolden)
        {
            var templateSize = new OpenCvSharp.Size(_imgTemplate.Width, _imgTemplate.Height);
            var templateQuad = QvQuad2D.From(new RectangleF(0, 0, templateSize.Width, templateSize.Height));

            var chipData = cell.ChipData;
            var goldenChipQuad = chipData.GoldenQuad2D.Clone();

            var runtimeChipQuad = chipData.ChipQuad2D.Clone();
            var cellRoi = chipData.CellRoi;
            runtimeChipQuad.Offset(-cellRoi.X, -cellRoi.Y);

            var templatePoints = Array.ConvertAll(templateQuad.Corners, c => new OpenCvSharp.Point2f((float)c.X, (float)c.Y));
            var goldenChipPoints = Array.ConvertAll(goldenChipQuad.Corners, c => new OpenCvSharp.Point2f((float)c.X, (float)c.Y));
            var runtimeChipPoints = Array.ConvertAll(runtimeChipQuad.Corners, c => new OpenCvSharp.Point2f((float)c.X, (float)c.Y));

            using (var matrixToRuntime = Cv2.GetPerspectiveTransform(goldenChipPoints, runtimeChipPoints))
            {
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
            Mat matrixToGolden = matrixToGoldenArg;
            if (matrixToGolden == null)
                getPerspectiveTransformMatrices(cell, out matrixToGolden);

            var templateSize = new OpenCvSharp.Size(_imgTemplate.Width, _imgTemplate.Height);

            using (var srcImgBridge = new QxImageBridge(cellBmp))
            using (var dstImg = new Mat(templateSize, srcImgBridge.Image.Type()))
            {
                Cv2.WarpPerspective(
                    srcImgBridge.Image,
                    dstImg,
                    matrixToGolden,
                    templateSize,
                    InterpolationFlags.Linear | InterpolationFlags.Cubic
                );

                if (matrixToGolden != matrixToGoldenArg)
                    matrixToGolden?.Dispose();

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
            var templateSize = new System.Drawing.Size((int)_imgTemplate.Width, (int)_imgTemplate.Height);

            var chipData = cell.ChipData;
            var chipCenter = chipData.ChipQuad2D.Center;
            var chipAngle = chipData.ChipQuad2D.Angle;

            var cellRoi = chipData.CellRoi;
            double cx = chipCenter.X - cellRoi.X;
            double cy = chipCenter.Y - cellRoi.Y;

            var mvdAffineRect = new CMvdRectangleF((float)cx, (float)cy, templateSize.Width, templateSize.Height)
            {
                Angle = (float)chipAngle
            };

            using (var imgCell = GaImageUtil.BitmapToCMvdImage(cellBmp))
            {
                // cImageAffineTransformToolObj 已在 Init() 統一安全初始化
                _cImageAffineTransformTool.BasicParam.Aspect = 1;
                _cImageAffineTransformTool.InputImage = imgCell;
                _cImageAffineTransformTool.ROIShape = mvdAffineRect;
                _cImageAffineTransformTool.Run();
                CMvdImage imgRun = _cImageAffineTransformTool.Result.OutputImage.Clone();
                return imgRun;
            }
        }
        #endregion

        /// <summary>
        /// 使用海康, 進行瑕疵檢測 (簡單減法)
        /// </summary>
        QvBox2D[] detectDefectBlobs(CMvdImage imgTemplate, CMvdImage imgRun, CMvdImage imgMask, RegionCellX3Class cell)
        {
            var chipData = cell?.ChipData;
            if (chipData == null) return null;
            var chipCenter = chipData?.ChipQuad2D.Center;
            if (chipCenter == null) return null;
            if (_xInspect.RoiCount <= 0) return null;

            CMvdRectangleF mvdRoi = new CMvdRectangleF(imgTemplate.Width / 2f, imgTemplate.Height / 2f, imgTemplate.Width, imgTemplate.Height);
            bool isException = false;
            string phase = "";

            try
            {
                // 1. 影像相減 (Subtract)
                phase = "D1. 影像相減 (Subtract)";
                _cImageArithmeticTool.InputImage1 = imgTemplate;
                _cImageArithmeticTool.InputImage2 = imgRun;

                // 將原本的 "Subtract" 改為 "AbsSubtract"
                //_cImageArithmeticTool.SetRunParam("ArithmeticType", "Subtract");
                _cImageArithmeticTool.SetRunParam("ArithmeticType", "AbsDiff");

                _cImageArithmeticTool.ROI = mvdRoi;
                _cImageArithmeticTool.Run();
                VisionDesigner.CMvdImage OutputImage = _cImageArithmeticTool.Result.OutputImage;

                // 2. 二值化 (Binary)
                phase = "D2. 二值化 (Binary)";
                _cImageBinaryTool.InputImage = OutputImage;
                _cImageBinaryTool.ROI = null;
                _cImageBinaryTool.SetRunParam("LowThreshold", _xInspect.xThresholdValue.ToString());
                _cImageBinaryTool.Run();

                // 3. 【優化注入】形態學開運算 (Morphology Open) - 消除對齊引起的邊緣細小假點
                phase = "D3. 形態學開運算 (Morphology Open)";
                _cImageMorphTool.InputImage = _cImageBinaryTool.Result.OutputImage;
                _cImageMorphTool.ROI = mvdRoi;
                _cImageMorphTool.SetRunParam("Type", "Open");
                _cImageMorphTool.Run();

                // 4. Blob 分析 - 輸入源修改為「形態學開運算處理後」的影像
                phase = "D4. Blob 分析";
                _cBlobFindTool.InputImage = _cImageMorphTool.Result.OutputImage;
                _cBlobFindTool.RegionImage = imgMask;
                _cBlobFindTool.ROI = mvdRoi;
                _cBlobFindTool.SetRunParam("Polarity", "BrightObject");
                _cBlobFindTool.BasicParam.ShowBlobImageStatus = true;
                _cBlobFindTool.Run();
            }
            catch (Exception ex)
            {
                //------------------------------------------------------
                // 注意: Blob 分析 在 _cBlobFindTool.InputImage 全黑時,
                // 海康套件會拋出異常, 可以不用理會.
                //------------------------------------------------------ 
                if (!phase.Contains("Blob 分析"))
                {
                    isException = true;
                    GaUtil.LOG($"海康瑕疵檢異常 @ {phase}", ex);
                    //LtDebug.LOG.Error(ex, $"海康瑕疵檢異常 @ {phase}");
                }
            }

            VisionDesigner.BlobFind.CBlobFindResult cBlobFindRes = _cBlobFindTool?.Result;
            var blobMvdRectFNGList = new List<CMvdRectangleF>();

            #region 挑出超標的 Blobs
            if (cBlobFindRes != null)
            {
                foreach (var item in cBlobFindRes.BlobInfo)
                {
                    if (item.AreaF > _xInspect.xCharArea)
                    {
                        blobMvdRectFNGList.Add(item.BoxInfo);
                    }
                    else if (item.LongAxis > _xInspect.xCharWidth && item.ShortAxis > _xInspect.xCharHeight)
                    {
                        blobMvdRectFNGList.Add(item.BoxInfo);
                    }
                }
            }
            #endregion

            QvBox2D[] defectResults = null;

            if (blobMvdRectFNGList.Count > 0)
            {
                cell.MarkResult(InspectReason.NG_APPEARANCE);

                List<CMvdRectangleF> cMvdRectangleFs = new List<CMvdRectangleF>();
                foreach (CMvdRectangleF mvdRectangleF in blobMvdRectFNGList)
                {
                    mvdRectangleF.CenterX += (float)(chipCenter.X - imgRun.Width / 2f);
                    mvdRectangleF.CenterY += (float)(chipCenter.Y - imgRun.Height / 2f);
                    cMvdRectangleFs.Add(mvdRectangleF);
                }

                defectResults = Array.ConvertAll(cMvdRectangleFs.ToArray(), b => b.ToBox2D());
            }

            // 保存調試用的圖片檔 (包含形態學處理後的結果，利於工程師現場調機)
            if (_ini.IsSaveTestImage && (blobMvdRectFNGList.Count > 0 || isException))
            {
                _DUMP_IMAGES(cell, cBlobFindRes);
            }

            return defectResults;
        }

        #region DUMP_FUNCTIONS
        void _DUMP_IMAGES(RegionCellX3Class cell, VisionDesigner.BlobFind.CBlobFindResult cBlobFindRes)
        {
            // 保存調試用的圖片檔 (包含形態學處理後的結果，利於工程師現場調機)

            //string dumpFolder = System.IO.Path.Combine(RegionCellX3Class.SaveDebugPath, "Detect");
            //JetEazy.IO.QxPathUtility.InitDirectory(dumpFolder);

            ////string fileStem = System.IO.Path.Combine(dumpFolder, cell.lblName);
            //string fileStem = $"Cell_{cell.Index}@{cell.CellRow}_{cell.CellCol}";

            //_cImageArithmeticTool?.InputImage1?.SaveImage(fileStem + "_Template.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
            //_cImageArithmeticTool?.InputImage2?.SaveImage(fileStem + "_Run.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
            //_cImageBinaryTool?.Result?.OutputImage?.SaveImage(fileStem + "_Diff_Binary.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
            //_cImageMorphTool?.Result?.OutputImage?.SaveImage(fileStem + "_Diff_MorphOpen.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP); // 新增儲存開運算圖
            //_cBlobFindTool?.RegionImage?.SaveImage(fileStem + "_MaskRegion.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
            //cBlobFindRes?.BlobImage?.SaveImage(fileStem + "_BlobResult.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);

            var dumper = LotDataHolder.Instance;
            dumper.DumpImage(cell, _cImageArithmeticTool?.InputImage1, "Defects", "Template");
            dumper.DumpImage(cell, _cImageArithmeticTool?.InputImage2, "Defects", "Scene");
            dumper.DumpImage(cell, _cImageBinaryTool?.Result?.OutputImage, "Defects", "Diff-Binary");
            dumper.DumpImage(cell, _cImageMorphTool?.Result?.OutputImage, "Defects", "Diff-MorphOpen");
            dumper.DumpImage(cell, _cBlobFindTool?.RegionImage, "Defects", "MaskRegion");
            dumper.DumpImage(cell, cBlobFindRes?.BlobImage, "Defects", "BlobResult");
        }
        #endregion
    }
}