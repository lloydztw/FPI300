using JetEazy.Utils;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using VisionDesigner;
using VisionDesigner.ImageArithmetic;

namespace LaserAlignDX.GA.mvc.Model.AoiModel
{
    internal class MvdDefectDetector
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
        //VisionDesigner.ImageAffineTransform.CImageAffineTransformTool cImageAffineTransformToolObj = null;
        CMvdRectangleF MvdRunPositionFix;
        #endregion

        #region MVD_DEFECT_INSPECT_MEMBERS
        private List<CMvdRectangleF> _blobMvdRectFNGList = new List<CMvdRectangleF>();
        private SizeF _defectRunRoiSize = new SizeF(100, 100);
        #endregion

        /// <summary>
        /// 瑕疵檢查 (單一晶粒) 
        /// </summary>
        private void RunOneChipDefects(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi)
        {
            try
            {
                var templateSize = _xRecipe.bmpDefectTemplate.Size;
                var chipCenter = cell.ChipData.ChipQuad2D.Center;
                var chipAngle = cell.ChipData.ChipQuad2D.Angle;

                // 取的 center 的 local 圖像座標 (cellRoi 左上角為 (0,0))
                double cx = chipCenter.X - cellRoi.X;
                double cy = chipCenter.Y - cellRoi.Y;

                // 以 (cx,cy) 為中心 建立 長寬為 templateSize, 角度為 chipAngle 的 海康矩形
                // 如果把 _xRecipe.bmpDefectTemplate 當成無角度的 rectangle 就不需要減 GoldenQuad2D.Angle
                var mvdRectX = new CMvdRectangleF((float)cx, (float)cy, templateSize.Width, templateSize.Height)
                {
                    Angle = (float)chipAngle
                };

                //var box2d = new QvBox2D();
                //box2d.SetBox(PointF.Empty, templateSize);
                //box2d.SetCenter((float)cx (float)cy;
                //box2d.SetTheta((float)chipAngle * Math.PI / 180.0);
                //var mvdRectX = GaMvdExt.ToCMvdRectangleF(box2d);

                var bmpTemplate = _xRecipe.bmpDefectTemplate;
                var bmpMask = _xRecipe.bmpprintmask;
                //var roi = _xRecipe.xRegionTrain;
                //roi.X += regionRoi.X;
                //roi.Y += regionRoi.Y;

                //*****************************************************************************
                // 利用海康 對 cellBmp Affine Transform 
                // (注意:此處會被多線程 同時調用)
                //*****************************************************************************
                using (var cMvdImage = GaImageUtil.BitmapToCMvdImage(cellBmp))
                using (var bmpRun = cell.GetAffineTrainsFormRunBmp(cMvdImage, mvdRectX))
                {
                    // 使用海康進行 瑕疵檢測
                    cell.DetectDefects(bmpTemplate, bmpRun, bmpMask);
                }
            }
            catch (Exception ex)
            {
                //_LOG_ERROR(ex, $"cell.DetectDefects 異常 @ ({cell.CellRow},{cell.CellCol})!");
                //_xInspect.optChipDefectsInspect = false;
                throw;
            }
        }

        public void DetectDefects(RegionCellX3Class cell, Bitmap bmpTemplate, Bitmap bmpRun, Bitmap bmpMask)
        {
            var ChipData = cell.ChipData;

            //string dumpFolder = System.IO.Path.Combine(SaveDebugPath, "Detect");
            //if (IsSaveDebugPicture)
            //{
            //    if (!System.IO.Directory.Exists(dumpFolder))
            //        System.IO.Directory.CreateDirectory(dumpFolder);
            //}

            CMvdRectangleF _roi = new CMvdRectangleF(bmpTemplate.Width / 2, bmpTemplate.Height / 2, bmpTemplate.Width, bmpTemplate.Height);

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

            cImageArithmeticToolObj.ROI = _roi;
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
            cBlobFindToolObj.ROI = _roi;
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

            _defectRunRoiSize = bmpRun.Size;
            _blobMvdRectFNGList.Clear();

            foreach (var item in cBlobFindToolObj.Result.BlobInfo)
            {
                //Console.WriteLine("Index: {0}, Angle {1}", item.DomainIndex, item.BoxInfo.Angle);
                if (item.AreaF > _xInspect.xCharArea)
                {
                    _blobMvdRectFNGList.Add(item.BoxInfo);
                }
                else if (item.LongAxis > _xInspect.xCharWidth && item.ShortAxis > _xInspect.xCharHeight)
                {
                    _blobMvdRectFNGList.Add(item.BoxInfo);
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

            if (_blobMvdRectFNGList.Count > 0)
            {
                //inspectReasons.Add(InspectReason.INS_DEFECTERR);
                cell.MarkResult(InspectReason.NG_APPEARANCE);

                List<CMvdRectangleF> cMvdRectangleFs = new List<CMvdRectangleF>();
                foreach (CMvdRectangleF mvdRectangleF in _blobMvdRectFNGList)
                {
                    mvdRectangleF.CenterX += MvdRunPositionFix.CenterX - _defectRunRoiSize.Width / 2;  // bmpItemRun.Width / 2;
                    mvdRectangleF.CenterY += MvdRunPositionFix.CenterY - _defectRunRoiSize.Height / 2; // bmpItemRun.Height / 2;
                    //mvdRectangleF.BorderColor = new MVD_COLOR(255, 0, 0);
                    //mvdRectangleF.BorderWidth = 1;
                    cMvdRectangleFs.Add(mvdRectangleF);
                }

                ChipData.DefectBlobs = Array.ConvertAll(cMvdRectangleFs.ToArray(), b => b.ToBox2D());
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

    }
}
