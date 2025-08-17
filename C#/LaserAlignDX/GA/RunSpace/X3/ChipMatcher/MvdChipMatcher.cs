#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 開始重整優化 Gaara 原來的 MvdFindClass (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using AUVision;
using JetEazy.Utils;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using VisionDesigner;
using VisionDesigner.AlmightyPatMatch;


namespace LaserAlignDX.RunSpace.Supports
{
    /// <summary>
    /// 從 Gaara MvdFindClass 抽出, 只保留 與 晶粒定位 有相關的部分. 
    /// </summary>
    public class MvdChipMatcher : IMvdTemplateMatcher
    {
        #region PRIVATE_RUNTIME_IMAGE_CACHEs
        //Bitmap bmpObj_Image = null;
        //Bitmap bmpRun_Image = null;
        //CMvdImage xMvdObj_Image = null;
        //CMvdImage xMvdRun_Image = null;
        #endregion

        #region PRIVATE_MVD_VisionDesigner_Members
        VisionDesigner.AlmightyPatMatch.CAlmightyPattern cAlmightyPatternObj = null;
        VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchTool cAlmightyPatmatchToolObj = null;
        #endregion

        #region PUBLIC_SETTINGS
        public float xMvdAngle { get; set; } = 5;
        public int xMvdMaxOcc { get; set; } = 1;
        public float xMvdTolerance { get; set; } = 0.5f;
        public PointF xMvdFixed { get; set; } = new PointF(-1, -1);
        public int xMaxOverlap { get; set; } = 80;
        #endregion

        public Size TemplateSize
        {
            get;
            private set;
        } = new Size(1, 1);
        public List<xFindResult> xResults
        {
            get;
            private set;
        } = new List<xFindResult>();

        ~MvdChipMatcher()
        {
            // for Garbage Collection
            Dispose();
        }
        public void Dispose()
        {
            //bmpObj_Image?.Dispose();
            //bmpObj_Image = null;

            //bmpRun_Image?.Dispose();
            //bmpRun_Image = null;

            //xMvdObj_Image?.Dispose();
            //xMvdObj_Image = null;

            //xMvdRun_Image?.Dispose();
            //xMvdRun_Image = null;

            cAlmightyPatmatchToolObj?.Dispose();
            cAlmightyPatmatchToolObj = null;

            cAlmightyPatternObj?.Dispose();
            cAlmightyPatternObj = null;
        }

        /// <summary>
        /// MVD 訓練 (對外統一接口 !)
        /// (1) 不要跟別的專案混雜在一起, 不要暴露一大堆 HikTrain2, HikTrain3 這些雜亂的函式!
        /// (2) 如果眾多專案有共同的部分, 請用抽出 Interface 或 Abstract Class
        /// </summary>
        /// <param name="bmpTemplate">由 caller 維護其生命週期</param>
        public bool Train(Bitmap bmpTemplate)
        {
            this.TemplateSize = bmpTemplate.Size;

            //bmpObj_Image?.Dispose();
            //bmpObj_Image = (Bitmap)bmpTemplate.Clone();

            ////CMvdRectangleF cMvd = new CMvdRectangleF(
            ////    xRegionTrain.Width / 2,
            ////    xRegionTrain.Height / 2,
            ////    xRegionTrain.Width,
            ////    xRegionTrain.Height);

            //// bool bOK = HikTrainBmp();

            //---------------------------------------------------------------
            //NOTE: cAlmightyPatternObj 負責接管 xMvdObj_Image 生命週期
            //---------------------------------------------------------------
            var xMvdObj_Image = GaImageUtil.BitmapToCMvdImage(bmpTemplate);

            bool bOK = HikTrain2(xMvdObj_Image);

            return bOK;
        }

        /// <summary>
        /// MVD 執行比對 (對外統一接口 !)
        /// (1) 不要跟別的專案混雜在一起, 不要暴露一大堆 HikTrain2, HikTrain3 這些雜亂的函式!
        /// (2) 如果眾多專案有共同的部分, 請用抽出 Interface 或 Abstract Class
        /// </summary>
        /// <param name="bmpScene">由 caller 維護其生命週期</param>
        public bool RunMatch(Bitmap bmpScene)
        {
            // 實時更新 MVD 主要設定
            this.xMvdAngle = InspectX3ParaClass.Instance.xAngle;
            this.xMvdTolerance = InspectX3ParaClass.Instance.xTolerance;
            this.xMaxOverlap = InspectX3ParaClass.Instance.xMaxOverlap;

            bool bOK;

            //// OLD:
            ////      HikRunBmp()
            ////          xMvdRun_Image?.Dispose();
            ////          xMvdRun_Image = BitmapToCMvdImage(bmpRun_Image);
            ////      HikRun2()
            ////      HikRun3(new RectangleF(0, 0, bmpRun_Image.Width, bmpRun_Image.Height));
            ////          xResults
            ////
            //this.bmpRun_Image?.Dispose();
            //this.bmpRun_Image = (Bitmap)bmpScene.Clone();
            //bOK = HikRunBmp();

            //---------------------------------------------------------------
            //NOTE: cAlmightyPatmatchToolObj 負責接管 xMvdRun_Image 生命週期
            //---------------------------------------------------------------
            var xMvdRun_Image = GaImageUtil.BitmapToCMvdImage(bmpScene);

            bOK = HikRun2(xMvdRun_Image);

            return bOK;
        }

        #region PRIVATE_HIK_FUNCTIONS
#if NO_USED_CODE
        bool HikTrainBmp()
        {
            xMvdObj_Image?.Dispose();
            xMvdObj_Image = BitmapToCMvdImage(bmpObj_Image);
            return HikTrain2(xMvdObj_Image);
        }
        bool HikTrainBmp(CMvdRectangleF cMvdRectangleF)
        {
            xMvdObj_Image?.Dispose();
            xMvdObj_Image = BitmapToCMvdImage(bmpObj_Image);
            return HikTrain2(cMvdRectangleF);
        }
        bool HikRunBmp()
        {
            xMvdRun_Image?.Dispose();
            xMvdRun_Image = BitmapToCMvdImage(bmpRun_Image);
            return HikRun2(xMvdRun_Image);
        }
#endif

        bool HikTrain2(CMvdImage xMvdObj_Image)
        {
            bool bOK = false;
            string errMsg = string.Empty;

            #region HIK_TRAIN
            try
            {
                // CreatePatternInstance
                if (cAlmightyPatternObj == null)
                    cAlmightyPatternObj = new VisionDesigner.AlmightyPatMatch.CAlmightyPattern();

                //Set type
                cAlmightyPatternObj.Type = PatMatchAlgorithmType.HPFeature;
                //cAlmightyPatternObj.Type = PatMatchAlgorithmType.FastFeature;

                if (xMvdObj_Image.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                {
                    //当前程序仅支持mono8。因此像素格会转换.
                    xMvdObj_Image.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                }

                // cAlmightyPatternObj 接管 xMvdObj_Image
                var oldImage = cAlmightyPatternObj.InputImage;
                cAlmightyPatternObj.InputImage = xMvdObj_Image;
                oldImage?.Dispose();

                // Set ROI region (optional)
                cAlmightyPatternObj.RegionList.Clear();
                float extendvalue = 1f;
                var region1 = new VisionDesigner.CMvdRectangleF(
                    xMvdObj_Image.Width * 0.5f,
                    xMvdObj_Image.Height * 0.5f,
                    xMvdObj_Image.Width * extendvalue,
                    xMvdObj_Image.Height * extendvalue
                );

                cAlmightyPatternObj.RegionList.Add(new CAlmightyPatMatchRegion(region1, true));

                // Set basic parameter
                //cAlmightyPatternObj.BasicParam.FixPoint =
                //    new VisionDesigner.MVD_POINT_F(Convert.ToSingle(xMvdObj_Image.Width * extendvalue) / 2,
                //                                                               Convert.ToSingle(xMvdObj_Image.Height * extendvalue) / 2);

                if (xMvdFixed.X < 0 || xMvdFixed.X > xMvdObj_Image.Width || xMvdFixed.Y < 0 || xMvdFixed.Y > xMvdObj_Image.Height)
                {
                    cAlmightyPatternObj.BasicParam.FixPoint =
                        new VisionDesigner.MVD_POINT_F(
                            Convert.ToSingle(xMvdObj_Image.Width * extendvalue) / 2,
                            Convert.ToSingle(xMvdObj_Image.Height * extendvalue) / 2
                        );
                }
                else
                {
                    cAlmightyPatternObj.BasicParam.FixPoint =
                        new VisionDesigner.MVD_POINT_F(xMvdFixed.X, xMvdFixed.Y);
                }

                // Train

                cAlmightyPatternObj.Train();
                bOK = true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                bOK = false;
            }
            #endregion

            return bOK;
        }

#if NO_USED_CODE
        bool HikTrain2(CMvdRectangleF cMvdRectangleF)
        {
            bool bOK = false;
            string errmessage = string.Empty;

            #region HIK_TRAIN
            try
            {
                // CreatePatternInstance
                if (cAlmightyPatternObj == null)
                    cAlmightyPatternObj = new VisionDesigner.AlmightyPatMatch.CAlmightyPattern();

                //Set type
                cAlmightyPatternObj.Type = PatMatchAlgorithmType.HPFeature;
                //cAlmightyPatternObj.Type = PatMatchAlgorithmType.FastFeature;

                if (xMvdObj_Image.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                {
                    //当前程序仅支持mono8。因此像素格会转换.
                    xMvdObj_Image.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                }
                cAlmightyPatternObj.InputImage = xMvdObj_Image;
                // Set ROI region (optional)
                cAlmightyPatternObj.RegionList.Clear();
                var region1 = cMvdRectangleF;

                cAlmightyPatternObj.RegionList.Add(new CAlmightyPatMatchRegion(region1, true));

                // Set basic parameter

                cAlmightyPatternObj.BasicParam.FixPoint =
                    new VisionDesigner.MVD_POINT_F(cMvdRectangleF.CenterX, cMvdRectangleF.CenterY);

                // Train

                cAlmightyPatternObj.Train();
                bOK = true;
            }
            catch (Exception ex)
            {
                errmessage = ex.Message;
                bOK = false;
            }
            #endregion

            return bOK;

        }
#endif
        bool HikRun2(CMvdImage xMvdRun_Image)
        {
            var W = xMvdRun_Image.Width;
            var H = xMvdRun_Image.Height;

            //bool bOK = HikRun3(new RectangleF(0, 0, bmpRun_Image.Width, bmpRun_Image.Height));
            bool bOK = HikRun3(xMvdRun_Image, new RectangleF(0, 0, W, H));

            //xResults.Clear();

            //#region HIK_RUN

            //// CreateToolInstance
            //if (cAlmightyPatmatchToolObj == null)
            //    cAlmightyPatmatchToolObj = new VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchTool();

            ////Set type

            //cAlmightyPatmatchToolObj.Type = PatMatchAlgorithmType.HPFeature;

            ////// Set ROI region (optional)
            //cAlmightyPatmatchToolObj.RegionList.Clear();
            //var region2 = new VisionDesigner.CMvdRectangleF(xMvdRun_Image.Width * 0.5f,
            //    xMvdRun_Image.Height * 0.5f,
            //    xMvdRun_Image.Width,
            //    xMvdRun_Image.Height);
            //cAlmightyPatmatchToolObj.RegionList.Add(new CAlmightyPatMatchRegion(region2, true));

            //// Set basic parameter

            //cAlmightyPatmatchToolObj.BasicParam.ShowOutlineStatus = false;

            //// Set Pattern

            //cAlmightyPatmatchToolObj.Pattern = cAlmightyPatternObj;

            //cAlmightyPatmatchToolObj.SetRunParam("AngleStart", (-xMvdAngle).ToString());
            //cAlmightyPatmatchToolObj.SetRunParam("AngleEnd", xMvdAngle.ToString());
            //cAlmightyPatmatchToolObj.SetRunParam("MinScore", xMvdTolerance.ToString());

            //if (xMvdRun_Image.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
            //{
            //    //当前程序仅支持mono8。因此像素格会转换.
            //    xMvdRun_Image.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            //}
            //cAlmightyPatmatchToolObj.InputImage = xMvdRun_Image;

            //// Running

            //cAlmightyPatmatchToolObj.Run();

            //// Get the result

            //VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchResult cHPMatchRes = cAlmightyPatmatchToolObj.Result;

            ////foreach (var item in cHPMatchRes.MatchInfoList)

            ////{

            ////    Console.WriteLine("MatchPoint: ({0},{1})", item.MatchPoint.fX, item.MatchPoint.fY);

            ////}
            //int resultcount = cHPMatchRes.MatchInfoList.Count;
            //if (resultcount > 0)
            //{
            //    foreach (var item in cHPMatchRes.MatchInfoList)
            //    {
            //        //Console.WriteLine("MatchPoint: ({0},{1})", item.MatchPoint.fX, item.MatchPoint.fY);
            //        if (item.Score >= xMvdTolerance)
            //        {
            //            xFindResult result = new xFindResult();
            //            //var item = cHPMatchRes.MatchInfoList[0];
            //            result.fCenterX = item.MatchBox.CenterX;
            //            result.fCenterY = item.MatchBox.CenterY;
            //            result.fAngle = item.MatchBox.Angle;
            //            result.fScale = item.Scale;
            //            result.fScore = item.Score;

            //            xResults.Add(result);
            //        }
            //    }
            //}

            //bOK = xResults.Count > 0;

            //#endregion

            return bOK;
        }
        bool HikRun3(CMvdImage xMvdRun_Image, RectangleF roiRect)
        {
            bool bOK = false;

            xResults.Clear();

            #region HIK_RUN

            // CreateToolInstance
            if (cAlmightyPatmatchToolObj == null)
                cAlmightyPatmatchToolObj = new VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchTool();

            //Set type

            cAlmightyPatmatchToolObj.Type = PatMatchAlgorithmType.HPFeature;
            //cAlmightyPatmatchToolObj.Type = PatMatchAlgorithmType.FastFeature;

            //// Set ROI region (optional)
            cAlmightyPatmatchToolObj.RegionList.Clear();
            var region2 = new VisionDesigner.CMvdRectangleF(
                                roiRect.X + roiRect.Width / 2,
                                roiRect.Y + roiRect.Height / 2,
                                roiRect.Width,
                                roiRect.Height);
            cAlmightyPatmatchToolObj.RegionList.Add(new CAlmightyPatMatchRegion(region2, true));

            // Set basic parameter

            cAlmightyPatmatchToolObj.BasicParam.ShowOutlineStatus = false;

            // Set Pattern

            cAlmightyPatmatchToolObj.Pattern = cAlmightyPatternObj;

            cAlmightyPatmatchToolObj.SetRunParam("AngleStart", (-xMvdAngle).ToString());
            cAlmightyPatmatchToolObj.SetRunParam("AngleEnd", xMvdAngle.ToString());
            cAlmightyPatmatchToolObj.SetRunParam("MinScore", xMvdTolerance.ToString());
            cAlmightyPatmatchToolObj.SetRunParam("MaxOverlap", xMaxOverlap.ToString());

            if (xMvdRun_Image.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
            {
                //当前程序仅支持mono8。因此像素格会转换.
                xMvdRun_Image.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            }

            // cAlmightyPatmatchToolObj 接管 xMvdRun_Image 生命週期
            var old = cAlmightyPatmatchToolObj.InputImage;
            cAlmightyPatmatchToolObj.InputImage = xMvdRun_Image;
            old?.Dispose();

            try
            {
                // Running

                cAlmightyPatmatchToolObj.Run();

                // Get the result

                VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchResult cHPMatchRes = cAlmightyPatmatchToolObj.Result;

                //foreach (var item in cHPMatchRes.MatchInfoList)
                //{

                //    Console.WriteLine("MatchPoint: ({0},{1})", item.MatchPoint.fX, item.MatchPoint.fY);

                //}
                int resultcount = cHPMatchRes.MatchInfoList.Count;
                if (resultcount > 0)
                {
                    foreach (var item in cHPMatchRes.MatchInfoList)
                    {
                        //Console.WriteLine("MatchPoint: ({0},{1})", item.MatchPoint.fX, item.MatchPoint.fY);
                        if (item.Score >= xMvdTolerance)
                        {
                            xFindResult result = new xFindResult();
                            //var item = cHPMatchRes.MatchInfoList[0];
                            result.fCenterX = item.MatchBox.CenterX;
                            result.fCenterY = item.MatchBox.CenterY;
                            result.fAngle = item.MatchBox.Angle;
                            result.fScale = item.Scale;
                            result.fScore = item.Score;

                            xResults.Add(result);
                        }
                    }
                }

            }
            catch
            {

            }



            #endregion

            if(false && cAlmightyPatmatchToolObj != null)
            {
                var oldImage = cAlmightyPatmatchToolObj.InputImage;
                cAlmightyPatmatchToolObj.Dispose();
                cAlmightyPatmatchToolObj = null;
                oldImage?.Dispose();
            }

            bOK = xResults.Count > 0;
            return bOK;
        }

#if NO_USED_CODE
        bool HikRun4Pre()
        {
            bool bOK = true;

            xResults.Clear();

            #region HIK_RUN

            // CreateToolInstance
            if (cAlmightyPatmatchToolObj == null)
                cAlmightyPatmatchToolObj = new VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchTool();

            cAlmightyPatmatchToolObj.ThreadNum = 4;
            //Set type

            cAlmightyPatmatchToolObj.Type = PatMatchAlgorithmType.HPFeature;

            //// Set ROI region (optional)
            cAlmightyPatmatchToolObj.RegionList.Clear();
            // Set basic parameter

            cAlmightyPatmatchToolObj.BasicParam.ShowOutlineStatus = false;

            // Set Pattern

            cAlmightyPatmatchToolObj.Pattern = cAlmightyPatternObj;

            cAlmightyPatmatchToolObj.SetRunParam("AngleStart", (-xMvdAngle).ToString());
            cAlmightyPatmatchToolObj.SetRunParam("AngleEnd", xMvdAngle.ToString());
            cAlmightyPatmatchToolObj.SetRunParam("MinScore", xMvdTolerance.ToString());

            if (xMvdRun_Image.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
            {
                //当前程序仅支持mono8。因此像素格会转换.
                xMvdRun_Image.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            }
            cAlmightyPatmatchToolObj.InputImage = xMvdRun_Image;

            #endregion

            return bOK;
        }
        bool HikRun4(RectangleF eRectF)
        {
            bool bOK = false;

            xResults.Clear();

            #region HIK_RUN

            //// Set ROI region (optional)
            cAlmightyPatmatchToolObj.RegionList.Clear();
            var region2 = new VisionDesigner.CMvdRectangleF(eRectF.X + eRectF.Width / 2,
               eRectF.Y + eRectF.Height / 2,
                eRectF.Width,
                eRectF.Height);
            cAlmightyPatmatchToolObj.RegionList.Add(new CAlmightyPatMatchRegion(region2, true));

            try
            {
                // Running

                cAlmightyPatmatchToolObj.Run();

                // Get the result

                VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchResult cHPMatchRes = cAlmightyPatmatchToolObj.Result;

                //foreach (var item in cHPMatchRes.MatchInfoList)

                //{

                //    Console.WriteLine("MatchPoint: ({0},{1})", item.MatchPoint.fX, item.MatchPoint.fY);

                //}
                int resultcount = cHPMatchRes.MatchInfoList.Count;
                if (resultcount > 0)
                {
                    foreach (var item in cHPMatchRes.MatchInfoList)
                    {
                        //Console.WriteLine("MatchPoint: ({0},{1})", item.MatchPoint.fX, item.MatchPoint.fY);
                        if (item.Score >= xMvdTolerance)
                        {
                            xFindResult result = new xFindResult();
                            //var item = cHPMatchRes.MatchInfoList[0];
                            result.fCenterX = item.MatchBox.CenterX;
                            result.fCenterY = item.MatchBox.CenterY;
                            result.fAngle = item.MatchBox.Angle;
                            result.fScale = item.Scale;
                            result.fScore = item.Score;

                            xResults.Add(result);
                        }
                    }
                }

            }
            catch
            {

            }
            bOK = xResults.Count > 0;
            //if (cAlmightyPatmatchToolObj != null)
            //{
            //    cAlmightyPatmatchToolObj.Dispose();
            //    cAlmightyPatmatchToolObj = null;
            //}

            #endregion

            return bOK;
        }
#endif
        #endregion

        #region PRIVATE_FUNCTIONS
        private CMvdImage BitmapToCMvdImage(Bitmap bmpInputImg)
        {
            //CMvdImage cMvdImage = new CMvdImage();
            //System.Drawing.Imaging.PixelFormat bitPixelFormat = bmpInputImg.PixelFormat;
            //BitmapData bmData = bmpInputImg.LockBits(new Rectangle(0, 0, bmpInputImg.Width, bmpInputImg.Height), ImageLockMode.ReadOnly, bitPixelFormat);//锁定

            //if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            //{
            //    Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
            //    int offset = bmData.Stride - bmData.Width;
            //    Int32 ImageBaseDataSize = bmData.Width * bmData.Height;//imageBaseData_V2图像真正的缓存长度
            //    byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
            //    byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
            //    Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
            //    int bitmapIndex = 0;
            //    int ImageBaseDataIndex = 0;
            //    for (int i = 0; i < bmData.Height; i++)
            //    {
            //        for (int j = 0; j < bmData.Width; j++)
            //        {
            //            _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex++];
            //        }
            //        bitmapIndex += offset;
            //    }
            //    MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
            //    stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width;
            //    stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
            //    stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
            //    stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
            //    cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08, stImageData);
            //}
            //else if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            //{
            //    Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
            //    int offset = bmData.Stride - bmData.Width * 3;
            //    Int32 ImageBaseDataSize = bmData.Width * bmData.Height * 3;//imageBaseData_V2图像真正的缓存长度
            //    byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
            //    byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
            //    Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
            //    int bitmapIndex = 0;
            //    int ImageBaseDataIndex = 0;
            //    for (int i = 0; i < bmData.Height; i++)
            //    {
            //        for (int j = 0; j < bmData.Width; j++)
            //        {
            //            _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 2];
            //            _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 1];
            //            _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex];
            //            bitmapIndex += 3;
            //        }
            //        bitmapIndex += offset;
            //    }
            //    MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
            //    stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width * 3;
            //    stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
            //    stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
            //    stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
            //    cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_RGB_RGB24_C3, stImageData);
            //}
            //bmpInputImg.UnlockBits(bmData);  // 解除锁定
            //return cMvdImage;

            return GaImageUtil.BitmapToCMvdImage(bmpInputImg);
        }
        #endregion
    }
}
