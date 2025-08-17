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
using JetEazy.Match;
using JetEazy.OpenCV;
using JetEazy.Utils;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using Traveller106;
using VisionDesigner;
using VisionDesigner.AlmightyPatMatch;
using CvMat = OpenCvSharp.Mat;


namespace LaserAlignDX.RunSpace.Supports
{
    /// <summary>
    /// 進階 MvdChipMatcher
    /// 處理 相鄰 chip 錯抓問題
    /// </summary>
    public partial class MvdChipMatcherEx : IMvdTemplateMatcher
    {
        #region PRIVATE_MVD_VisionDesigner_Members
        VisionDesigner.AlmightyPatMatch.CAlmightyPattern cAlmightyPatternObj = null;
        VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchTool cAlmightyPatmatchToolObj = null;
        #endregion

        #region PRIVATE_RUNTIME_DATA
        bool _isMvdParamsChanged = false;
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

        ~MvdChipMatcherEx()
        {
            // for Garbage Collection
            Dispose();
        }
        public void Dispose()
        {
            cAlmightyPatmatchToolObj?.InputImage?.Dispose();
            cAlmightyPatmatchToolObj?.Dispose();
            cAlmightyPatmatchToolObj = null;

            cAlmightyPatternObj?.InputImage?.Dispose();
            cAlmightyPatternObj?.Dispose();
            cAlmightyPatternObj = null;

            disposeGoldenPadsGrid();
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

            //--------------------------------------------------------------------
            //NOTE: cAlmightyPatternObj 負責接管 xMvdObj_Image 生命週期
            //--------------------------------------------------------------------
            CMvdImage xMvdObj_Image = GaImageUtil.BitmapToCMvdImage(bmpTemplate);
            bool bOK = HikTrain2(xMvdObj_Image);

            // Golden PadsGrid 資訊
            updateGoldenPadsGrid(bmpTemplate);


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
            xResults.Clear();

            // 實時更新 MVD 主要設定
            update_mvd_params();
            //xMvdMaxOcc = 4;
            //xMvdTolerance = 0.5f;
            //xMaxOverlap = 50;

            //--------------------------------------------------------------------
            //HikRun 內部 cAlmightyPatmatchToolObj 負責接管 xMvdRun_Image 生命週期
            //----------------------------------------------------------------------
            CMvdImage xMvdRun_Image = GaImageUtil.BitmapToCMvdImage(bmpScene);
            bool bOK = HikRun(xMvdRun_Image, RectangleF.Empty, out var mvdMatchResult);

            sortForBestGridMatch(mvdMatchResult, bmpScene);

            convert_mvd_result(mvdMatchResult, xResults);

            bOK = xResults.Count > 0;
            return bOK;
        }

        #region PRIVATE_HIK_FUNCTIONS
        /// <summary>
        /// 沿用舊有 Gaara 的
        /// </summary>
        bool HikTrain2(CMvdImage xMvdObj_Image)
        {
            bool bOK = false;

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
                // cAlmightyPatternObj.BasicParam.FixPoint =
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
                LtDebug.LOG.Error(ex);
                //errMsg = ex.Message;
                bOK = false;
            }

            return bOK;
        }
        /// <summary>
        /// HikRun 內部 cAlmightyPatmatchToolObj 負責接管 xMvdRun_Image 生命週期
        /// </summary>
        bool HikRun(CMvdImage xMvdRun_Image, RectangleF roiRect, out CAlmightyPatMatchResult mvdMatchResult)
        {
            mvdMatchResult = null;

            // CreateToolInstance
            if (cAlmightyPatmatchToolObj == null)
            {
                cAlmightyPatmatchToolObj = new VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchTool();
                _isMvdParamsChanged = true;
            }

            //Set type
            cAlmightyPatmatchToolObj.Type = PatMatchAlgorithmType.HPFeature;
            //cAlmightyPatmatchToolObj.Type = PatMatchAlgorithmType.FastFeature;

            //// Set ROI region (optional)
            if (roiRect.IsEmpty)
                roiRect = new RectangleF(0, 0, xMvdRun_Image.Width, xMvdRun_Image.Height);

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

            // SetRunParam
            if (_isMvdParamsChanged)
            {
                cAlmightyPatmatchToolObj.SetRunParam("AngleStart", (-xMvdAngle).ToString());
                cAlmightyPatmatchToolObj.SetRunParam("AngleEnd", xMvdAngle.ToString());
                cAlmightyPatmatchToolObj.SetRunParam("MinScore", xMvdTolerance.ToString());
                cAlmightyPatmatchToolObj.SetRunParam("MaxOverlap", xMaxOverlap.ToString());
            }

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
                mvdMatchResult = cAlmightyPatmatchToolObj.Result;
            }
            catch (Exception ex)
            {
                LtDebug.LOG.Error(ex);
                mvdMatchResult = null;
            }

            //------------------------------------------------------
            // CAUTION: 在此不要釋放 cAlmightyPatmatchToolObj !!!
            //------------------------------------------------------
            //if (false && cAlmightyPatmatchToolObj != null)
            //{
            //    var oldImage = cAlmightyPatmatchToolObj.InputImage;
            //    cAlmightyPatmatchToolObj.Dispose();
            //    cAlmightyPatmatchToolObj = null;
            //    oldImage?.Dispose();
            //}
            var matchInfoList = mvdMatchResult?.MatchInfoList;
            bool bOK = matchInfoList != null && matchInfoList.Count > 0;
            return bOK;
        }
        #endregion

        #region PRIVATE_HELPER_FUNCTIONS
        bool update_mvd_params()
        {
            // 實時更新 MVD 主要設定
            _isMvdParamsChanged = true ||
                this.xMvdAngle != InspectX3ParaClass.Instance.xAngle ||
                this.xMvdTolerance != InspectX3ParaClass.Instance.xTolerance ||
                this.xMaxOverlap != InspectX3ParaClass.Instance.xMaxOverlap;

            if (_isMvdParamsChanged)
            {
                this.xMvdAngle = InspectX3ParaClass.Instance.xAngle;
                this.xMvdTolerance = InspectX3ParaClass.Instance.xTolerance;
                this.xMaxOverlap = InspectX3ParaClass.Instance.xMaxOverlap;
            }

            return _isMvdParamsChanged;
        }
        void convert_mvd_result(CAlmightyPatMatchResult cHPMatchRes, List<xFindResult> results)
        {
            results?.Clear();

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
        #endregion
    }


    partial class MvdChipMatcherEx
    {
        class ChipInfo
        {
            public CvMat Image;
            public EzBlocsGrid PadsGrid;
            public int KeyRow;
            public int KeyCol;
            public int KeyPixels;
            public double ScoreG;
            public double ScoreMvd;
            public void CleanUp()
            {
                Image?.Dispose();
                Image = null;
                PadsGrid?.Dispose();
                PadsGrid = null;
            }
        }

        #region PRIVATE_CV_DATA
        ChipInfo _goldenChipInfo;
        #endregion

        void disposeGoldenPadsGrid()
        {
            _goldenChipInfo?.CleanUp();
            _goldenChipInfo = null;
        }
        void updateGoldenPadsGrid(Bitmap bmpTemplate)
        {
            disposeGoldenPadsGrid();
            using (var bridge = new QxImageBridge(bmpTemplate))
            {
                _goldenChipInfo = new ChipInfo();

                _goldenChipInfo.Image = bridge.Image.Clone();

                var finder = new EzPadGridFinder();

                finder.FindPadsGrid( _goldenChipInfo.Image,
                                     out var grid,
                                     out var keyRow,
                                     out var keyCol,
                                     out var keyPixels );

                _goldenChipInfo.PadsGrid = grid;
                _goldenChipInfo.KeyRow = keyRow;
                _goldenChipInfo.KeyCol = keyCol;
                _goldenChipInfo.KeyPixels = keyPixels;
            }
        }

        void sortForBestGridMatch(CAlmightyPatMatchResult mvdMatchResult, Bitmap bmpScene)
        {
            if (mvdMatchResult == null ||
                mvdMatchResult.MatchInfoList.Count < 2)
                return;

            System.Diagnostics.Debug.Assert(_goldenChipInfo != null);
            
            var boundaryRect = new Rectangle(0, 0, bmpScene.Width, bmpScene.Height);
            var theSearchItemsDict = new Dictionary<CAlmightyMatchInfo, ChipInfo>();

            var finder = new EzPadGridFinder();

            using (var bridge = new QxImageBridge(bmpScene))
            {
                var imgScene = bridge.Image;
                foreach (var item in mvdMatchResult.MatchInfoList)
                {
                    var boxRectF = GaImageUtil.ToRectangleF(item.MatchBox);
                    var roiRect = Rectangle.Round(boxRectF);
                    GaUtil.ClipRect(ref roiRect, boundaryRect.Size);
                    var roi = JetEazy.Qcvt.CV(roiRect);

                    var img = imgScene[roi];

                    finder.FindPadsGrid(img,
                                        out var grid,
                                        out int keyRow,
                                        out int keyCol,
                                        out int keyPixels);

                    var searchItem = new ChipInfo()
                    {
                        Image = img,
                        PadsGrid = grid,
                        KeyRow = keyRow,
                        KeyCol = keyCol,
                        KeyPixels = keyPixels
                    };

                    searchItem.ScoreG = calcGridMatchScore(searchItem, _goldenChipInfo);
                    searchItem.ScoreMvd = item.Score;

                    theSearchItemsDict.Add(item, searchItem);
                }
            }

            mvdMatchResult.MatchInfoList.Sort((a, b) =>
            {
                var item1 = theSearchItemsDict[a];
                var item2 = theSearchItemsDict[b];
                if (item1.ScoreG > item2.ScoreG)
                    return -1;
                else if(item1.ScoreG < item2.ScoreG) 
                    return 1;
                else
                {
                    if (item1.ScoreMvd > item2.ScoreMvd)
                        return -1;
                    else if (item1.ScoreMvd < item2.ScoreMvd)
                        return 1;
                    else
                        return 0;
                }
            });
        }
        double calcGridMatchScore(ChipInfo itemScene, ChipInfo itemGolden)
        {
            var grid1 = itemScene?.PadsGrid;
            var grid2 = itemGolden?.PadsGrid;
            
            if (grid1 == null || grid2 == null)
                return -3;
            if (grid1.Rows != grid2.Rows || grid1.Cols != grid2.Cols)
                return -2;
            
            //if (itemScene.KeyRow != itemGolden.KeyRow || itemScene.KeyCol != itemGolden.KeyCol)
            //    return -1;

            double d = (double)(itemScene.KeyPixels - itemGolden.KeyPixels) / (itemGolden.KeyPixels + 1);
            return Math.Max(0, 1 - d);
        }

    }
}
