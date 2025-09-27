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
using LaserAlignDX.OPSpace.RecipeSpace;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Traveller106;
using VisionDesigner;
using VisionDesigner.BlobFind;
using VsCommon.ControlSpace.MachineSpace;


namespace LaserAlignDX.AoiModel
{
    public class FlyAoiModel
    {
        #region GLOBAL_MESS
        //ITravelerModel _sysModel => GaMvcConfig.SysModel;
        RecipeFPIX3Class _xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        FlyParaClass _xFlyPara
        {
            get { return FlyParaClass.Instance; }
        }
        #endregion

        #region MACHINE
        MainFPIX3MachineClass MACHINE
        {
            get { return (MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection?.MACHINE; }
        }
        #endregion

        #region RECIPE_OFFSETS
        PointF[] _xFlyOffsetUseStage
        {
            get
            {
                //var carrierID = _sysModel.ActiveCarrierID;
                //return carrierID == CarrierEnum.C1 ? xFlyPara.ptsOffset : xFlyPara.ptsOffset2;
                var plcIO = MACHINE?.PLCIO;
                var stageNo = plcIO != null ? plcIO.iScanStage : 1;
                return stageNo == 1 ? _xFlyPara.ptsOffset : _xFlyPara.ptsOffset2;
            }
        }
        #endregion

        #region PRIVATE_MEMBERS
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
            var aoiMetaData = new FlyMetaData() { AlgorithmName = "MVD_TemplateMatch" };
            var aoiResult = new FlyAoiResult() { MetaData = aoiMetaData };

            RectangleF roiRect = _xRecipe.xRectRegionPrintFly;
            PointF centerOrg = JetEazy.Qcvt.Center(ref roiRect);
            roiRect.Inflate(_xFlyPara.xExtendx, _xFlyPara.xExtendy);
            GaUtil.Clip(ref roiRect, bmpFly.Size);

            using (Bitmap bmpCrop = bmpFly.Clone(roiRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
            {
                bool ok = runMvd_TemplateMatch(bmpCrop);

                aoiResult.Code = ok ? PlcFlyResultCode.OK : PlcFlyResultCode.NG;

                // 記入 GUI 畫圖所需要的數據
                // 注意: xResults.Count 有可能為 0 !!!
                var xResults = _xRecipe.mvdprintFlytemp_Find.xResults;
                if (xResults.Count > 0)
                    aoiMetaData.xResult = xResults[0];
                else
                    aoiMetaData.xResult = null;

                aoiMetaData.xTemplateRect = _xRecipe.xRectRegionPrintFly;
                aoiMetaData.xBlobs = null;
                aoiMetaData.roiRect = roiRect;
                aoiMetaData.bmpFly = bmpFly;
                aoiMetaData.flyID = flyID;
                aoiMetaData.flyAoiResult = aoiResult;
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

            int flyStart = flyID.flyStart;
            if (flyStart >= 1)
            {
                if (aoiResult.Code == PlcFlyResultCode.OK)
                {
                    // 飛拍 像測 抓到的中心點
                    PointF centerRun = aoiMetaData.xCentroid;

                    // 補償量 (算出的 pix 需加入解析度)
                    float flyCamResolution = INI.Instance.FlyImageResolution;
                    aoiResult.OffsetX = -(centerRun.X - centerOrg.X) * flyCamResolution;    // + _xFlyOffsetUseStage[flyShowID1 - 1].X;
                    aoiResult.OffsetY = -(centerRun.Y - centerOrg.Y) * flyCamResolution;    // + _xFlyOffsetUseStage[flyShowID1 - 1].Y;
                    aoiResult.OffsetAngle = (aoiMetaData.xResult != null) ? aoiMetaData.xResult.Value.fAngle : 0f;

                    // 全域調整 (Global Offset)
                    var gIndex = flyID.ShowID - 1;
                    var gOffsets = _xFlyOffsetUseStage;
                    if (0 <= gIndex && gIndex < gOffsets.Length)
                    {
                        aoiResult.OffsetX += gOffsets[gIndex].X;
                        aoiResult.OffsetY += gOffsets[gIndex].Y;
                    }
                }
            }

            return aoiResult;
        }
        FlyAoiResult flyProcessProSpecial(FlyID flyID, Bitmap bmpFly)
        {
            var aoiMetaData = new FlyMetaData() { AlgorithmName = "MVD_CheckSpecialAngle" };
            var aoiResult = new FlyAoiResult() { MetaData = aoiMetaData };
            var flystopwatch = new Stopwatch();
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
                bool ok = runMvd_CheckSpecialAngle(bmpCrop, out var mvdBlobs, out float angle, out PointF centerPt);
                aoiResult.Code = ok ? PlcFlyResultCode.OK : PlcFlyResultCode.NG;
                aoiResult.OffsetAngle = ok ? angle : 0f;

                // 記入 GUI 畫圖所需要的數據
                aoiMetaData.xBlobs = mvdBlobs;
                aoiMetaData.roiRect = roiRect;
                aoiMetaData.bmpFly = bmpFly;
                aoiMetaData.flyID = flyID;
                aoiMetaData.flyAoiResult = aoiResult;
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
            if (flyStart >= 1)
            {
                //updateOneResult(flyID, aoiResult);
            }

            return aoiResult;
        }

        bool runMvd_TemplateMatch(Bitmap bmpFly)
        {
            var mvdTool = _xRecipe.mvdprintFlytemp_Find;

            mvdTool.xMvdAngle = _xFlyPara.xAngle;
            mvdTool.xMvdTolerance = _xFlyPara.xTolerance;

            mvdTool.bmpRun_Image?.Dispose();
            mvdTool.bmpRun_Image = (Bitmap)bmpFly.Clone();
            bool bOK = mvdTool.HikRunBmp();

            return bOK;
        }
        bool runMvd_CheckSpecialAngle(Bitmap ebmpInput, out List<CBlobInfo> resultBlobs, out float resultAngle, out PointF resultCenter)
        {
            bool ok = false;

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

                    ok = true;
                }
                #endregion

            }
            return ok;
        }
    }
}
