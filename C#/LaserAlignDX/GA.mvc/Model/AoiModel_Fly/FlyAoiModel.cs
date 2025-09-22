#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-22 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Utils;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Traveller106;
using VisionDesigner;
using VisionDesigner.BlobFind;


namespace LaserAlignDX.AoiModel
{
    public class FlyAoiModel
    {
        #region GLOBAL_MESS
        ITravelerModel _sysModel => GaMvcConfig.SysModel;
        RecipeFPIX3Class _xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        FlyParaClass _xFlyPara
        {
            get { return FlyParaClass.Instance; }
        }
        #endregion

        #region OFFSETS_IN_RECIPE
        PointF[] FlyOffsetUseStage
        {
            get
            {
                var carrierID = _sysModel.ActiveCarrierID;
                return carrierID == CarrierEnum.C1 ? _xFlyPara.ptsOffset : _xFlyPara.ptsOffset2;
            }
        }
        #endregion

        #region PRIVATE_MEMBERS
        Stopwatch _stopWatch = new Stopwatch();
        #endregion

        public bool Train(object recipe)
        {
            var bmpFlyTemplate = _xRecipe.bmpprintFlytemplate;

            var mvdTool = _xRecipe.mvdprintFlytemp_Find;
            mvdTool.bmpObj_Image?.Dispose();
            mvdTool.bmpObj_Image = (Bitmap)bmpFlyTemplate.Clone();

            bool bOK = mvdTool.HikTrainBmp();
            return bOK;
        }
        public FlyAoiResult RunAoiOne(FlyID flyID, Bitmap bmpFly)
        {
            FlyAoiResult result;
            if (FlyParaClass.Instance.xIsOpenMuit)
                result = flyProcessProSpecial(flyID, bmpFly);
            else
                result = flyProcessPro(flyID, bmpFly);
            return result;
        }

        FlyAoiResult flyProcessPro(FlyID flyID, Bitmap bmpFly)
        {
            var aoiMetaData = new FlyMetaData() { AlgorithmName = "Pro" };
            var aoiResult = new FlyAoiResult() { MetaData = aoiMetaData };

            var flystopwatch = this._stopWatch;
            flystopwatch.Restart();

            #region OLD_CODE
            //bmpFlyOperate.Dispose();
            //bmpFlyOperate = bmpInput;
            //RectangleF _rectF = new RectangleF(
            //    xRecipe.xRectRegionPrintFly.X,
            //    xRecipe.xRectRegionPrintFly.Y,
            //    xRecipe.xRectRegionPrintFly.Width,
            //    xRecipe.xRectRegionPrintFly.Height);
            //_rectF.Inflate(xFlyPara.xExtendx, xFlyPara.xExtendy);
            //BoundRect(ref _rectF, bmpFlyOperate.Size);
            #endregion

            #region OLD_CODE_FOR_CENTER_POINTS
            //PointF centerOrg = new PointF(
            //    xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
            //    xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);
            //PointF centerRun = new PointF(
            //    xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
            //    xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);
            #endregion

            RectangleF roiRect = _xRecipe.xRectRegionPrintFly;
            PointF centerOrg = JetEazy.Qcvt.Center(ref roiRect);
            PointF centerRun = centerOrg;

            roiRect.Inflate(_xFlyPara.xExtendx, _xFlyPara.xExtendy);
            GaUtil.Clip(ref roiRect, bmpFly.Size);

            using (Bitmap bmpCrop = bmpFly.Clone(roiRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
            {
                //int err = _xRecipe.PrintTempFlyRun(bmpCrop);
                bool ok = this.mvdRunAoi(bmpCrop);

                aoiResult.Code = ok ? PlcFlyResultCode.OK : PlcFlyResultCode.NG;

                aoiMetaData.xTemplateRect = _xRecipe.xRectRegionPrintFly;
                aoiMetaData.xResult = _xRecipe.mvdprintFlytemp_Find.xResults[0];
                aoiMetaData.bmpFly = bmpFly;
                aoiMetaData.roiRect = roiRect;
            }

            #region OLD_CODE
            //PointF centerOrg = new PointF(
            //    xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
            //    xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);
            //PointF centerRun = new PointF(
            //    xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
            //    xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);
            #endregion

            #region OLD_CODE
            //int flyShowIndex = flyIndex + 1;
            //switch (flyStart)
            //{
            //    case 1:
            //        flyShowIndex = flyIndex + 1;
            //        break;
            //    case 2:
            //        flyShowIndex = flyIndex + 1 + 4;
            //        break;
            //}
            #endregion

            #region OLD_CODE
            //float _resolutionFly = INI.Instance.FlyImageResolution;
            //switch (flyStart)
            //{
            //    case 1:
            //        iFlyResult[flyIndex] = (err == 0 ? 1 : 2);
            //        if (err == 0)
            //        {
            //            //centerRun = new PointF(
            //            //        xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + _rectF.X,
            //            //        xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + _rectF.Y);
            //            ////算出的pix需加入解析度
            //            //iFlyOffset[flyIndex * 3 + 0] = -(centerRun.X - centerOrg.X) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].X;
            //            //iFlyOffset[flyIndex * 3 + 1] = -(centerRun.Y - centerOrg.Y) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].Y;
            //            //iFlyOffset[flyIndex * 3 + 2] = xRecipe.mvdprintFlytemp_Find.xResults[0].fAngle;

            //            centerRun.X = foundResult.fCenterX + roiRect.X;
            //            centerRun.Y = foundResult.fCenterY + roiRect.Y;

            //            //算出的pix需加入解析度
            //            offsetX = -(centerRun.X - centerOrg.X) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].X;
            //            offsetY = -(centerRun.Y - centerOrg.Y) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].Y;
            //            offsetAngle = foundResult.fAngle;

            //            iFlyOffset[flyIndex * 3 + 0] = offsetX;
            //            iFlyOffset[flyIndex * 3 + 1] = offsetY;
            //            iFlyOffset[flyIndex * 3 + 2] = offsetAngle;
            //        }
            //        else
            //        {
            //            iFlyOffset[flyIndex * 3 + 0] = 0;
            //            iFlyOffset[flyIndex * 3 + 1] = 0;
            //            iFlyOffset[flyIndex * 3 + 2] = 0;
            //        }
            //        break;

            //    case 2:
            //        iFlyResult[flyIndex] = (err == 0 ? 1 : 2);
            //        if (err == 0)
            //        {
            //            //centerRun = new PointF(
            //            //    xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + roiRect.X,
            //            //    xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + roiRect.Y);

            //            ////算出的pix需加入解析度
            //            //iFlyOffset[flyIndex * 3 + 0] = -(centerRun.X - centerOrg.X) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].X;
            //            //iFlyOffset[flyIndex * 3 + 1] = -(centerRun.Y - centerOrg.Y) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].Y;
            //            //iFlyOffset[flyIndex * 3 + 2] = xRecipe.mvdprintFlytemp_Find.xResults[0].fAngle;

            //            centerRun.X = foundResult.fCenterX + roiRect.X;
            //            centerRun.Y = foundResult.fCenterY + roiRect.Y;

            //            //算出的 pix 需加入解析度
            //            offsetX = -(centerRun.X - centerOrg.X) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].X;
            //            offsetY = -(centerRun.Y - centerOrg.Y) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].Y;
            //            offsetAngle = foundResult.fAngle;

            //            iFlyOffset[flyIndex * 3 + 0] = offsetX;
            //            iFlyOffset[flyIndex * 3 + 1] = offsetY;
            //            iFlyOffset[flyIndex * 3 + 2] = offsetAngle;
            //        }
            //        else
            //        {
            //            iFlyOffset[flyIndex * 3 + 0] = 0;
            //            iFlyOffset[flyIndex * 3 + 1] = 0;
            //            iFlyOffset[flyIndex * 3 + 2] = 0;
            //        }
            //        break;
            //}
            #endregion

            int showID1 = flyID.ShowID;
            int flyStart = flyID.flyStart;
            if (flyStart == 1 || flyStart == 2)
            {
                if (aoiResult.Code == PlcFlyResultCode.OK)
                {
                    centerRun.X = aoiMetaData.xResult.fCenterX + roiRect.X;
                    centerRun.Y = aoiMetaData.xResult.fCenterY + roiRect.Y;
                    aoiMetaData.xCentroid = centerRun;

                    //算出的pix需加入解析度
                    float _resolutionFly = INI.Instance.FlyImageResolution;
                    aoiResult.OffsetX = -(centerRun.X - centerOrg.X) * _resolutionFly + FlyOffsetUseStage[showID1 - 1].X;
                    aoiResult.OffsetY = -(centerRun.Y - centerOrg.Y) * _resolutionFly + FlyOffsetUseStage[showID1 - 1].Y;
                    aoiResult.OffsetAngle = aoiMetaData.xResult.fAngle;
                }
            }

            flystopwatch.Stop();
            return aoiResult;
        }
        FlyAoiResult flyProcessProSpecial(FlyID flyID, Bitmap bmpFly)
        {
            var aoiMetaData = new FlyMetaData() { AlgorithmName = "ProSpecial" };
            var aoiResult = new FlyAoiResult() { MetaData = aoiMetaData };

            var flystopwatch = this._stopWatch;
            flystopwatch.Restart();

            #region OLD_CODE
            //////转换图像
            ////byte[] bmpbytes = new byte[cameraFrame.uBytes];
            ////Marshal.Copy(pBuffer, bmpbytes, 0, bmpbytes.Length);
            //int iw = cameraFrame.iWidth;
            //int ih = cameraFrame.iHeight;
            //bmpFlyOperate.Dispose();
            //bmpFlyOperate = ConvertFromMONO(eBytes, iw, ih);
            //RectangleF _rectF = new RectangleF(
            //    xRecipe.xRectRegionPrintFly.X,
            //    xRecipe.xRectRegionPrintFly.Y,
            //    xRecipe.xRectRegionPrintFly.Width,
            //    xRecipe.xRectRegionPrintFly.Height);
            #endregion

            RectangleF roiRect = _xRecipe.xRectRegionPrintFly;
            roiRect.Inflate(_xFlyPara.xExtendx, _xFlyPara.xExtendy);
            GaUtil.Clip(ref roiRect, bmpFly.Size);

            using (Bitmap bmpCrop = bmpFly.Clone(roiRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
            {
                //bool ok = _xRecipe.CheckSpecialAngle(bmpCrop, out var blobsList, out float angle, out PointF centerPt);
                bool ok = this.mvdCheckSpecialAngle(bmpCrop, out var blobsList, out float angle, out PointF centerPt);

                aoiResult.Code = ok ? PlcFlyResultCode.OK : PlcFlyResultCode.NG;
                aoiResult.OffsetAngle = ok ? angle : 0f;

                aoiMetaData.xBlobs = blobsList;
                aoiMetaData.xTemplateRect = _xRecipe.xRectRegionPrintFly;
            }

            #region OLD_CODE
            //int flyShowIndex = flyIndex + 1;
            //switch (flyStart)
            //{
            //    case 1:
            //        flyShowIndex = flyIndex + 1;
            //        break;
            //    case 2:
            //        flyShowIndex = flyIndex + 1 + 4;
            //        break;
            //}
            #endregion

            #region OLD_CODE
            //switch (flyStart)
            //{
            //    case 1:
            //        iFlyResult[flyIndex] = (bOK ? 1 : 2);
            //        if (bOK)
            //        {
            //            //算出的pix需加入解析度
            //            iFlyOffset[flyIndex * 3 + 0] = 0;
            //            iFlyOffset[flyIndex * 3 + 1] = 0;
            //            iFlyOffset[flyIndex * 3 + 2] = angle;
            //        }
            //        else
            //        {
            //            iFlyOffset[flyIndex * 3 + 0] = 0;
            //            iFlyOffset[flyIndex * 3 + 1] = 0;
            //            iFlyOffset[flyIndex * 3 + 2] = 0;
            //        }
            //        break;
            //    case 2:
            //        iFlyResult[flyIndex] = (bOK ? 1 : 2);
            //        if (bOK)
            //        {
            //            //算出的pix需加入解析度
            //            iFlyOffset[flyIndex * 3 + 0] = 0;
            //            iFlyOffset[flyIndex * 3 + 1] = 0;
            //            iFlyOffset[flyIndex * 3 + 2] = angle;
            //        }
            //        else
            //        {
            //            iFlyOffset[flyIndex * 3 + 0] = 0;
            //            iFlyOffset[flyIndex * 3 + 1] = 0;
            //            iFlyOffset[flyIndex * 3 + 2] = 0;
            //        }
            //        break;
            //}
            #endregion

            int flyStart = flyID.flyStart;
            if (flyStart == 1 || flyStart == 2)
            {
                //updateOneResult(flyID, aoiResult);
            }
            else
            {
            }

            flystopwatch.Stop();
            return aoiResult;
        }

        bool mvdRunAoi(Bitmap bmpFly)
        {
            var mvdTool = _xRecipe.mvdprintFlytemp_Find;

            mvdTool.xMvdAngle = _xFlyPara.xAngle;
            mvdTool.xMvdTolerance = _xFlyPara.xTolerance;

            mvdTool.bmpRun_Image?.Dispose();
            mvdTool.bmpRun_Image = (Bitmap)bmpFly.Clone();
            bool bOK = mvdTool.HikRunBmp();

            return bOK;
        }
        bool mvdCheckSpecialAngle(Bitmap ebmpInput, out List<CBlobInfo> resultBlobs, out float resultAngle, out PointF resultCenter)
        {
            bool bOK = false;

            resultAngle = 0;
            resultCenter = new PointF();
            resultBlobs = new List<CBlobInfo>();

            using (var mvdBinaryTool = new VisionDesigner.ImageBinary.CImageBinaryTool())
            using (var mvdBlobFindTool = new VisionDesigner.BlobFind.CBlobFindTool())
            {
                // 二值化
                #region 用笨重的_MVD_來二值化影像
                mvdBinaryTool.InputImage?.Dispose();
                mvdBinaryTool.InputImage = GaImageUtil.BitmapToCMvdImage(ebmpInput);
                mvdBinaryTool.ROI = null;
                mvdBinaryTool.SetRunParam("LowThreshold", _xFlyPara.xThresholdValue.ToString());
                //cImageArithmeticToolObj.SetRunParam("HighThreshold", BlobHighThreshold.ToString());
                mvdBinaryTool.Run();
                #endregion

                // BLOBs
                #region 用笨重的_MVD_來找_BLOBS
                mvdBlobFindTool.InputImage?.Dispose();
                mvdBlobFindTool.InputImage = mvdBinaryTool.Result.OutputImage;
                //mvdBlobFindTool.RegionImage = BitmapToCMvdImage(eBmpMask);
                mvdBlobFindTool.ROI = null;

                if (_xFlyPara.xBlobMode == Eazy_Project_III.BlobMode.White)
                    mvdBlobFindTool.SetRunParam("Polarity", "BrightObject");
                else
                    mvdBlobFindTool.SetRunParam("Polarity", "DarkObject");

                mvdBlobFindTool.BasicParam.ShowBlobImageStatus = true;
                mvdBlobFindTool.Run();
                var cBlobFindRes = mvdBlobFindTool.Result;
                foreach (var blob in mvdBlobFindTool.Result.BlobInfo)
                {
                    if (blob.AreaF >= _xFlyPara.xBlobAreaMin && blob.AreaF <= _xFlyPara.xBlobAreaMax)
                    {
                        resultBlobs.Add(blob);
                    }
                }
                #endregion

                // Get Center and Angle from BLOBS
                #region 用笨重的_MVD_來找尋兩個_blobs_連線的中心點_與_其角度
                if (resultBlobs.Count >= 2)
                {
                    CBlobInfo b0 = resultBlobs[0];
                    CBlobInfo b1 = resultBlobs[1];

                    using (var mvdP2PMeasureTool = new VisionDesigner.P2PMeasure.CP2PMeasureTool())
                    {
                        mvdP2PMeasureTool.BasicParam.Point1 = new MVD_POINT_F(b0.RectInfo.CenterX, b0.RectInfo.CenterY);
                        mvdP2PMeasureTool.BasicParam.Point2 = new MVD_POINT_F(b1.RectInfo.CenterX, b1.RectInfo.CenterY);

                        mvdP2PMeasureTool.Run();

                        var mvdP2pResult = mvdP2PMeasureTool.Result;

                        if (_xFlyPara.xIsShuiPing)
                        {
                            resultAngle = mvdP2pResult.Angle;
                        }
                        else
                        {
                            resultAngle = mvdP2pResult.Angle + 90;
                        }

                        //if (mvdP2pResult.Angle < 0)
                        //{
                        //    if (_xFlyPara.xIsShuiPing)
                        //    {
                        //        resultAngle = -mvdP2pResult.Angle;
                        //    }
                        //    else
                        //    {
                        //        resultAngle = -mvdP2pResult.Angle + 90;
                        //    }
                        //}
                        //else
                        //{
                        //    if (_xFlyPara.xIsShuiPing)
                        //    {
                        //        resultAngle = mvdP2pResult.Angle;
                        //    }
                        //    else
                        //    {
                        //        resultAngle = mvdP2pResult.Angle - 90;
                        //    }
                        //}

                        resultCenter = new PointF(mvdP2pResult.MidPoint.fX, mvdP2pResult.MidPoint.fY);

                        //Console.WriteLine("Angle: {0}", cP2PMeasureRes.Angle);
                        //Console.WriteLine("Distance: {0}", cP2PMeasureRes.Dist);
                    }

                    bOK = true;
                }
                #endregion

            }
            return bOK;
        }
    }
}
