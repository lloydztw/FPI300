#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-20 配合 新校正板 (陣列圓點 + INK區塊 二合一)
 *      2025-09-07 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector.Model;
using EzAoiEmptyTrayInspector.Model.Aoi;
using JetEazy.EzImage;
using JetEazy.Match;
using JetEazy.QMath;
using JetEazy.QvMath;
using JetEazy.Utils;
using LeTian.AoiLib;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LaserAlignDX.AoiModel.Calib.V5
{
    public class CalibAoiModel : ICalibAoiModel
    {
        public static bool OPT_DUMP = false;

        #region PRIVATE_DATA
        #endregion

        public CalibAoiModel(object dummy = null)
        {
            // Caller 負責調用 imp.Dispose()
            //_externImp = imp;
        }

        public void Dispose()
        {
            // jxCalibRecipe 由 caller 維護其生命週期
            //_jxCalibRecipe = null;
        }

        /// <summary>
        /// 抓取 大校正板 的格點
        /// <br/> The caller 必須維護 fullfovImg 與 recipe 生命週期
        /// </summary>
        public EzBlocsGrid FetchBoardGrid(CarrierEnum carrierID, Mat fullfovImg, JxCalibRecipe recipe)
        {
            var jxSettings = recipe?.GridSettings;
            if (fullfovImg == null || jxSettings == null)
                return null;

            var roi = JetEazy.Qcvt.CV(recipe.GridSettings.BoundRect.Value);
            GaUtil.Clip(ref roi, fullfovImg.Width, fullfovImg.Height);

            var rows = jxSettings.EmptyTraySettings.FullRows.Value;
            var cols = jxSettings.EmptyTraySettings.FullCols.Value;
            var thres = jxSettings.GridVisionSettings.Threshold.Value;
            var minSz = jxSettings.GridVisionSettings.MinSize.Value;
            var maxSz = jxSettings.GridVisionSettings.MaxSize.Value;
            //>>> Math.Min(roi.Width, roi.Height) / 8;

            using (var imgCrop = fullfovImg[roi].Clone())
            {
                if (thres <= 0)
                    Cv2.Threshold(imgCrop, imgCrop, 0, 255, ThresholdTypes.Otsu);
                else
                    Cv2.Threshold(imgCrop, imgCrop, thres, 255, ThresholdTypes.Binary);
                
                _DUMP(imgCrop, "BoardGrid", $"binary_{carrierID}", ".jpg");

                var finder = new EzBlobFinder
                {
                    MorphIterations = 0,
                    OptFillBorder = false,
                    MinSize = new OpenCvSharp.Size(minSz, minSz),
                    MaxSize = new OpenCvSharp.Size(maxSz, maxSz)
                };
                finder.FindWhiteBlobs(imgCrop, out var blocs);

                var builder = new EzBlocsGridBuilder();
                var grid = builder.Build(blocs, targetRows: rows, targetCols: cols);
                grid?.Offset(roi.X, roi.Y);

                _DUMP_DOTS_PLATE_IMAGE(fullfovImg, null, "d:\\paso.log", carrierID);

                return grid;
            }
        }

        /// <summary>
        /// 抓取 墨點 (Ink Marks)
        /// <br/> 用 EzBloc[] 回傳
        /// <br/> The caller 必須維護 fullfovImg 與 recipe 生命週期 
        /// </summary>
        public (EzBloc[], EzBloc[]) FetchInkMarks(CarrierEnum carrierID, SuckerRowEnum suckerID, Mat fullfovImg, JxCalibRecipe recipe)
        {
            var inkMarks = new EzBloc[0];
            var padBlocs = new EzBloc[0];

            var jxSettings = suckerID == SuckerRowEnum.S1 ? recipe?.InkMarkSettings1 : recipe?.InkMarkSettings2;
            if (fullfovImg == null || jxSettings == null)
                return (inkMarks, padBlocs);

            int NP = 4;

            var roiBound = JetEazy.Qcvt.CV(recipe.GridSettings.BoundRect);
            GaUtil.Clip(ref roiBound, fullfovImg.Width, fullfovImg.Height);

            var thres = jxSettings.Vision.BlockThreshold.Value;
            var blockMinSz = jxSettings.Vision.BlockMinSize.Value;

            //(1) Find Calib Blocks 
            var calibChipBlocs = new List<EzBloc>();
            using (var imgBigCrop = fullfovImg[roiBound].Clone())
            {
                if (thres <= 0)
                    Cv2.Threshold(imgBigCrop, imgBigCrop, thres, 255, ThresholdTypes.Otsu);
                else
                    Cv2.Threshold(imgBigCrop, imgBigCrop, thres, 255, ThresholdTypes.Binary);

                var finder = new EzBlobFinder
                {
                    MorphIterations = 0,
                    OptFillBorder = false,
                    MinSize = new OpenCvSharp.Size(blockMinSz, blockMinSz)
                };

                finder.FindWhiteBlobs(imgBigCrop, out calibChipBlocs);
                if (calibChipBlocs == null)
                    return (inkMarks, padBlocs);

                if (calibChipBlocs.Count > 1)
                {
                    calibChipBlocs.Sort((a, b) =>
                    {
                        var px1 = a != null ? a.Pixels : 0;
                        var px2 = b != null ? b.Pixels : 0;
                        return px2 - px1;
                    });
                }

                if (calibChipBlocs.Count > NP)
                    calibChipBlocs.RemoveRange(NP, calibChipBlocs.Count - NP);

                foreach (var b in calibChipBlocs)
                    b?.Offset(roiBound.X, roiBound.Y);
            }

            //(2) Find the ink blocs in each PAD
            var inkBlocs = new List<EzBloc>();
            if (calibChipBlocs.Count > 0)
            {
                var inkVisionSettings = recipe.GridSettings.GridVisionSettings;
                var inkMinSz = Math.Max(inkVisionSettings.MinSize.Value, 18);
                foreach (var calibBloc in calibChipBlocs)
                {
                    var roi = JetEazy.Qcvt.CV(calibBloc.Rect);
                    roi.Inflate(-5, -5);
                    GaUtil.Clip(ref roi, fullfovImg.Width, fullfovImg.Height);

                    using (var imgCrop = fullfovImg[roi].Clone())
                    {
                        Cv2.Threshold(imgCrop, imgCrop, 0, 255, ThresholdTypes.Otsu);
                        Cv2.BitwiseNot(imgCrop, imgCrop);

                        EzBlobFinder finder = new EzBlobFinder
                        {
                            MorphIterations = 0,
                            OptFillBorder = true,
                            MinSize = new OpenCvSharp.Size(inkMinSz, inkMinSz)
                        };
                        finder.FindWhiteBlobs(imgCrop, out var whiteBlobs);

                        if (whiteBlobs != null && whiteBlobs.Count > 0)
                        {
                            if (whiteBlobs.Count > 1)
                            {
                                whiteBlobs.Sort((a, b) =>
                                {
                                    var px1 = a != null ? a.Pixels : 0;
                                    var px2 = b != null ? b.Pixels : 0;
                                    return px2 - px1;
                                });
                            }

                            foreach (var b in whiteBlobs)
                                b?.Offset(roi.X, roi.Y);

                            var bloc = whiteBlobs[0];
                            if (bloc != null)
                                inkBlocs.Add(bloc);
                        }
                    }
                }
            }

            //(3) Sort the InkMarks and PadBlocs in corner-4 order
            if (inkBlocs.Count >= NP)
            {
                var builder = new EzBlocsGridBuilder();
                var inkGrid = builder.Build(inkBlocs, targetRows: 2, targetCols: 2);
                if (inkGrid != null)
                    inkMarks = inkGrid.GetCornerBlocs();
            }
            if (calibChipBlocs.Count >= NP)
            {
                var builder = new EzBlocsGridBuilder();
                padBlocs = calibChipBlocs.ToArray();
                var grid = builder.Build(padBlocs, targetRows: 2, targetCols: 2);
                if (grid != null)
                    padBlocs = grid.GetCornerBlocs();
            }

            return (inkMarks, padBlocs);
        }

        /// <summary>
        /// 抓取 墨點 (Ink Marks)
        /// <br/> 用 QvQuad2D[] 回傳
        /// <br/> The caller 必須維護 fullfovImg 與 recipe 生命週期 
        /// </summary>
        public QvQuad2D[] FetchInkMarksQ(CarrierEnum carrierID, SuckerRowEnum suckerID, Mat fullfovImg, JxCalibRecipe recipe)
        {
            var inkMarks = new QvQuad2D[0];

            var jxSettings = suckerID == SuckerRowEnum.S1 ? recipe?.InkMarkSettings1 : recipe?.InkMarkSettings2;
            if (fullfovImg == null || jxSettings == null)
                return inkMarks;

            int NP = 4;

            var roiBound = JetEazy.Qcvt.CV(recipe.GridSettings.BoundRect);
            GaUtil.Clip(ref roiBound, fullfovImg.Width, fullfovImg.Height);

            var thres = jxSettings.Vision.BlockThreshold.Value;
            var blockMinSz = jxSettings.Vision.BlockMinSize.Value;

            //(1) Find Calib Blocks 
            var calibChipBlocs = new List<EzBloc>();
            using (var imgBigCrop = fullfovImg[roiBound].Clone())
            {
                if (thres <= 0)
                    Cv2.Threshold(imgBigCrop, imgBigCrop, thres, 255, ThresholdTypes.Otsu);
                else
                    Cv2.Threshold(imgBigCrop, imgBigCrop, thres, 255, ThresholdTypes.Binary);

                var finder = new EzBlobFinder
                {
                    MorphIterations = 0,
                    OptFillBorder = false,
                    MinSize = new OpenCvSharp.Size(blockMinSz, blockMinSz)
                };

                finder.FindWhiteBlobs(imgBigCrop, out calibChipBlocs);
                if (calibChipBlocs == null)
                    return inkMarks;

                if (calibChipBlocs.Count > 1)
                {
                    calibChipBlocs.Sort((a, b) =>
                    {
                        var px1 = a != null ? a.Pixels : 0;
                        var px2 = b != null ? b.Pixels : 0;
                        return px2 - px1;
                    });
                }

                if (calibChipBlocs.Count > NP)
                    calibChipBlocs.RemoveRange(NP, calibChipBlocs.Count - NP);

                foreach (var b in calibChipBlocs)
                    b?.Offset(roiBound.X, roiBound.Y);
            }

            //(2) Find the ink blocs
            var inkBlocs = new List<EzBloc>();
            if (calibChipBlocs.Count > 0)
            {
                var inkMinSz = Math.Max(2, blockMinSz / 20);
                foreach (var calibBloc in calibChipBlocs)
                {
                    var roi = JetEazy.Qcvt.CV(calibBloc.Rect);
                    roi.Inflate(-5, -5);
                    GaUtil.Clip(ref roi, fullfovImg.Width, fullfovImg.Height);

                    using (var imgCrop = fullfovImg[roi].Clone())
                    {
                        Cv2.Threshold(imgCrop, imgCrop, 0, 255, ThresholdTypes.Otsu);
                        Cv2.BitwiseNot(imgCrop, imgCrop);

                        EzBlobFinder finder = new EzBlobFinder
                        {
                            MorphIterations = 0,
                            OptFillBorder = false,
                            MinSize = new OpenCvSharp.Size(inkMinSz, inkMinSz)
                        };
                        finder.FindWhiteBlobs(imgCrop, out var whiteBlobs);

                        if (whiteBlobs != null && whiteBlobs.Count > 0)
                        {
                            if (whiteBlobs.Count > 1)
                            {
                                whiteBlobs.Sort((a, b) =>
                                {
                                    var px1 = a != null ? a.Pixels : 0;
                                    var px2 = b != null ? b.Pixels : 0;
                                    return px2 - px1;
                                });
                            }

                            foreach (var b in whiteBlobs)
                                b?.Offset(roi.X, roi.Y);

                            var bloc = whiteBlobs[0];
                            if (bloc != null)
                                inkBlocs.Add(bloc);
                        }
                    }
                }
            }

            //(3) Sort the ink blocs
            if (inkBlocs.Count >= NP)
            {
                var builder = new EzBlocsGridBuilder();
                var inkGrid = builder.Build(inkBlocs, targetRows: 2, targetCols: 2);
                var inkBlocCorners = inkGrid.GetCornerBlocs();
                inkMarks = new QvQuad2D[NP];
                for (int i = 0; i < NP; i++)
                {
                    var bloc = inkBlocCorners[i];
                    var quad = QvQuad2D.From(bloc.Rect);
                    quad.SetCenter(bloc.Center);
                    inkMarks[i] = quad;
                }
            }

            return inkMarks;
        }

        /// <summary>
        /// 保留
        /// </summary>
        public bool AdjustBadNodes(CarrierEnum carrierID, EzBlocsGrid grid)
        {
            bool isChanged = false;

#if (OPT_RESERVED)
            var transformsModel = _sysModel?.TransformsModel;
            var transCP = transformsModel?.GetCameraPhysicTransform(carrierID);
            if (transformsModel == null || grid == null)
                return false;

            int rows = grid.Rows;
            int cols = grid.Cols;

            int count = 0;
            double sumW = 0;
            double sumH = 0;

            for (int r = 0; r < rows; r++)
            {
                var c_min = (r == 0 || r == rows - 1) ? 1 : 0;
                var c_max = (r == 0 || r == rows - 1) ? cols - 1 : cols;
                for (int c = c_min; c < c_max; c++)
                {
                    var bloc = grid[r, c];
                    if (bloc == null)
                    {
                        isChanged = true;
                        continue;
                    }

                    (var motorDelta, var worldDelta) = transformsModel.CalcPlcCompensation(carrierID, bloc.Center, r, c);
                    double err = Math.Max(Math.Abs(worldDelta.X), Math.Abs(worldDelta.Y));
                    if (err >= 0.08)
                    {
                        if (transCP != null)
                        {
                            var adjustPt = transCP.Trans(bloc.Center);
                            adjustPt = adjustPt - worldDelta;
                            adjustPt = transCP.InvTrans(adjustPt);
                            bloc.Rect = JetEazy.Qcvt.CreateCenterRect((int)adjustPt.X, (int)adjustPt.Y, bloc.Rect.Width, bloc.Rect.Height);
                            bloc.Center = adjustPt;
                        }
                        else
                        {
                            grid[r, c] = null;
                        }
                        isChanged = true;
                        continue;
                    }

                    sumW += bloc.Rect.Width;
                    sumH += bloc.Rect.Height;
                    count++;
                }
            }

            if (isChanged && count > rows * cols / 2)
            {
                var aveSize = new SizeF((float)(sumW / count), (float)(sumH / count));
                var interpo = new EzBlocsGridInterpo(grid.GetPitch(), aveSize);
                interpo.RunInterpolation(grid, null, null);
                interpo.RunExpolation(grid, null, null, 2);

                ////暫時: 直接使用 QuadLinkNode() 設定成 MajorBloc.
                foreach (var bloc in grid.IterPredictedBlocs())
                {
                    bloc.Tag = new QuadLinkNode();
                }

                grid.RebuildRowColTags();
            }
#endif

            return isChanged;
        }

        /// <summary>
        /// 進階 抓取 空載台 格位點
        /// </summary>
        public MatchResult FetchGridNodes(IxEmptyTrayInspector aoi, CarrierEnum carrierID, Mat fullfovImg, bool refine)
        {
            if (aoi == null)
                return null;

            //(1) Peek the ezImage
            using (var ezImage = new EzQuickImage(fullfovImg, deepCopy: false))
            {
                //(2) Run Aoi
                aoi.RunMatch(SideID.A, ezImage);
                //_externImp.RunAll((IEzImage)ezImage, wait: true);

                //(3) Result
                var matchResult = aoi.GetMatchResult(SideID.A);
                //var result = _externImp.GetResult();

                //(4) Refine each detail locations
                if (refine)
                {
                    var recipe = aoi.GetRecipe();
                    RefineCentroidLocations(matchResult, ezImage, recipe);
                }

                //_DUMP_DOTS_PLATE_IMAGE(ezImage.Image as Mat, matchResult?.Grid, "d:\\paso.log", carrierID);

                return matchResult;
            }
        }

        #region PRIVATE_LOCATE_FUNCTIONS
        void RefineCentroidLocations(MatchResult matchResult, Mat fullfovImg, JxAoiRecipe jxRecipe)
        {
            var grid = matchResult?.Grid;
            if (grid == null || jxRecipe == null)
                return;

            int thresh = jxRecipe.VisionSettings.OutGridBlocThreshold.Value;
            var bound = new Rect(0, 0, fullfovImg.Width, fullfovImg.Height);
            int rows = grid.Rows;
            int cols = grid.Cols;

            bool isBlackCarrier = checkIfDarkBackground(fullfovImg);
            var findLocalCenter = isBlackCarrier ?
                (Func<Mat, Mat, int, string, QVector>)this.FindLocalCenter_black_carrier_cc :
                (Func<Mat, Mat, int, string, QVector>)this.FindLocalCenter_white_carrier;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var bloc = grid.Get(r, c);
                    if (bloc == null)
                        continue;


                    var roi = JetEazy.Qcvt.CV(bloc.Rect);
                    JetEazy.Qcvt.ClipBoundary(ref roi, ref bound);

                    using (var img = fullfovImg[roi].Clone())
                    using (var binary = new Mat())
                    {
                        string dumpTag = OPT_DUMP ? $"@{r}_{c}" : null;
                        var center = findLocalCenter(img, binary, thresh, dumpTag);

                        if (center == null)
                            continue;

                        center.X += roi.X;
                        center.Y += roi.Y;
                        bloc.Center = center;
                        var rect = JetEazy.Qcvt.CreateCenterRect((float)center.X, (float)center.Y, (float)bloc.Rect.Width, (float)bloc.Rect.Height);
                        bloc.Rect = Rectangle.Round(rect);
                    }
                }
            }
        }
        QVector FindLocalCenter_white_carrier(Mat img, Mat binary, int thresh, string dumpTag)
        {
            //(1) 將 白色載台影像 反向, 形成 淺色晶粒 深色背景
            Cv2.BitwiseNot(img, img);
            if (thresh <= 0)
                Cv2.Threshold(img, binary, 0, 255, ThresholdTypes.Otsu);
            else
                Cv2.Threshold(img, binary, thresh, 255, ThresholdTypes.Binary);

            //(2) DUMP
            _DUMP(img, "img", dumpTag);
            _DUMP(binary, "binary", dumpTag);

            //(3) Blobs
            findWhiteBlobs(binary, out var whiteBlobs);
            if (whiteBlobs.Count == 0)
                return null;

            whiteBlobs.Sort((b1, b2) => b2.Pixels - b1.Pixels);


            //(4) 使用質心 (for 白色載台)
            var center = whiteBlobs[0].Center;
            //center.X += roi.X;
            //center.Y += roi.Y;
            //bloc.Center = center;
            //var rc = JetEazy.Qcvt.CreateCenterRect((float)center.X, (float)center.Y, (float)bloc.Rect.Width, (float)bloc.Rect.Height);
            //bloc.Rect = Rectangle.Round(rc);
            return center;
        }
        QVector FindLocalCenter_black_carrier_00(Mat img, Mat binary, int thresh, string dumpTag)
        {
            if (thresh <= 0)
                Cv2.Threshold(img, binary, 0, 255, ThresholdTypes.Otsu);
            else
                Cv2.Threshold(img, binary, thresh, 255, ThresholdTypes.Binary);

            Cv2.Erode(binary, binary, null, iterations: 2);
            Cv2.Dilate(binary, binary, null, iterations: 2);
            Cv2.Rectangle(binary, new Rect(0, 0, binary.Width, binary.Height), Scalar.White, 2);
            Cv2.FloodFill(binary, new OpenCvSharp.Point(0, 0), Scalar.Black);

            findWhiteBlobs(binary, out var whiteBlobs);
            if (whiteBlobs.Count == 0)
                return null;

            whiteBlobs.Sort((b1, b2) => b2.Pixels - b1.Pixels);

            // 黑色載台 使用 質心誤差大, 改用 rect 中心
            //>>> var center = whiteBlobs[0].Center;
            var cc = JetEazy.Qcvt.CenterF(ref whiteBlobs[0].Rect);
            var center = new QVector(cc.X, cc.Y);
            return center;
        }
        QVector FindLocalCenter_black_carrier_cc(Mat img, Mat binary, int thresh, string dumpTag)
        {
            //(1) 二值化
            if (thresh <= 0)
                Cv2.Threshold(img, binary, 0, 255, ThresholdTypes.Otsu);
            else
                Cv2.Threshold(img, binary, thresh, 255, ThresholdTypes.Binary);

            //(2) Morph
            Cv2.Erode(binary, binary, null, iterations: 1);
            Cv2.Dilate(binary, binary, null, iterations: 1);
            //Cv2.Rectangle(binary, new Rect(0, 0, binary.Width, binary.Height), Scalar.White, 2);
            //Cv2.FloodFill(binary, new OpenCvSharp.Point(0, 0), Scalar.Black);

            //(3) Dump
            _DUMP(binary, "binary", dumpTag);

            //(4) 執行 FindContours
            Cv2.FindContours(
                binary,
                out var contours,
                out var hierarchy,
                mode: RetrievalModes.Tree,                      // 對應 cv2.RETR_TREE
                method: ContourApproximationModes.ApproxSimple  // 對應 cv2.CHAIN_APPROX_SIMPLE
            );

            //(4.1) 找到最大輪廓的索引和面積
            double maxArea = 0;
            int maxContourIndex = -1;
            for (int i = 0; i < contours.Length; i++)
            {
                // 獲取當前輪廓 (Point[] 型別)
                var contour = contours[i];
                // 計算輪廓面積
                double area = Cv2.ContourArea(contour);
                if (area > maxArea)
                {
                    maxArea = area;
                    maxContourIndex = i;
                }
            }
            if (maxContourIndex < 0)
            {
                LtDebug.LOG.Warn($"{GetType().Name} 無法找到 格位的 Contour!");
                return null;
            }

            //(5) 計算幾何矩 (Moments)
            var bestContour = contours[maxContourIndex];
            Moments M = Cv2.Moments(bestContour);
            //(5.1) 初始化質心座標
            double cX = 0;
            double cY = 0;
            //(5.2) 確保 M00 (面積) 不為零，避免除以零錯誤
            // 零階矩 M.M00 代表輪廓的面積
            if (M.M00 != 0)
            {
                // 3. 計算質心座標 (Centroid)
                cX = (M.M10 / M.M00);
                cY = (M.M01 / M.M00);
            }
            else
            {
                int n = 0;
                foreach (var pt in bestContour)
                {
                    cX += pt.X;
                    cY += pt.Y;
                    n++;
                }
                if (n > 1)
                {
                    cX /= n;
                    cY /= n;
                }
            }
            cX = Math.Round(cX, 3);
            cY = Math.Round(cY, 3);
            var center = new QVector(cX, cY);

            //(6) DUMP contour
            if (OPT_DUMP) 
            {
                using (var canvas = new Mat())
                {
                    Cv2.CvtColor(img, canvas, ColorConversionCodes.GRAY2BGR);
                    Cv2.DrawContours(canvas, contours, maxContourIndex, Scalar.Red, 3);
                    _DUMP(canvas, "Contour", dumpTag);
                }
            }

            return center;
        }
        #endregion

        #region PRIVATE_CHECK_FUNCTIONS
        bool checkIfDarkBackground(Mat img)
        {
            Mat imgU8 = GaImageUtil.ToU8(img);

            int bw = 8;
            int W = imgU8.Width;
            int H = imgU8.Height;
            var bound = new Rect(0, 0, W, H);
            var rois = new Rect[]
            {
                new Rect(0,0, bw,bw),
                new Rect(W-bw,0, bw,bw),
                new Rect(W-bw,H-bw, bw,bw),
                new Rect(0,H-bw, bw,bw),
            };

            var meanColor = imgU8.Mean().Val0;
            int countDark = 0;
            int countLight = 0;
            for (int i = 0, len = rois.Length; i < len; i++)
            {
                var roi = rois[i];
                JetEazy.Qcvt.ClipBoundary(ref roi, ref bound);
                if (roi.Width < 1 || roi.Height < 1)
                    continue;
                var color = (imgU8[roi]).Mean().Val0;
                if (color < meanColor)
                    countDark++;
                else
                    countLight++;
            }

            if (imgU8 != img)
                imgU8?.Dispose();

            return countDark > countLight;
        }
        #endregion

        #region PRIVATE_BLOB_FUNCTIONS
        void findWhiteBlobs(Mat img, out List<EzBloc> keyBlocs)
        {
            //var gc = new List<IDisposable>();

            keyBlocs = new List<EzBloc>();

            Mat whiteBlobsBinary = img;

            if (true) //_shrinkFactor < 8)
            {
                Cv2.Dilate(whiteBlobsBinary, whiteBlobsBinary, null);
                Cv2.Erode(whiteBlobsBinary, whiteBlobsBinary, null);

                if (true)   //_optFillOffBorder)
                {
                    var rect = new Rect(0, 0, whiteBlobsBinary.Width, whiteBlobsBinary.Height);
                    whiteBlobsBinary.Rectangle(rect, Scalar.White);
                    Cv2.FloodFill(whiteBlobsBinary, new OpenCvSharp.Point(0, 0), Scalar.Black);
                }
            }

            //if (imgDebugOutput != null)
            //{
            //    //whiteBlobsBinary.SaveImage(dumpFile);
            //    whiteBlobsBinary.CopyTo(imgDebugOutput);
            //}

            int min_w = img.Width / 3;
            int min_h = img.Height / 3;
            int max_w = img.Width;
            int max_h = img.Height;

            var cc = Cv2.ConnectedComponentsEx(whiteBlobsBinary);
            for (int i = 1; i < cc.Blobs.Count; i++)
            {
                var ccBlob = cc.Blobs[i];

                if (ccBlob.Width < min_w || ccBlob.Height < min_h ||
                    ccBlob.Width > max_w || ccBlob.Height > max_h)
                    continue;

                var rect = JetEazy.Qcvt.CC(ccBlob.Rect);

                //// UNSHRINK
                //if (_shrinkFactor > 1)
                //{
                //    rect.X *= _shrinkFactor;
                //    rect.Y *= _shrinkFactor;
                //    rect.Width *= _shrinkFactor;
                //    rect.Height *= _shrinkFactor;
                //}

                var bloc = new EzBloc(rect, 0);
                bloc.Pixels = ccBlob.Area;
                bloc.Center = new JetEazy.QMath.QVector(ccBlob.Centroid.X, ccBlob.Centroid.Y); // 保留精度 !
                keyBlocs.Add(bloc);
            }


            #region CLEAN_UP
            //foreach (var obj in gc)
            //    obj?.Dispose();
            #endregion
        }
        #endregion

        #region DUMP_FUNCTIONS
        void _DUMP(Mat img, string tag, int row, int col)
        {
            if (OPT_DUMP && img != null)
            {
                string path = $"d:\\paso.log\\Calib\\{tag}";
                JetEazy.IO.QxPathUtility.InitDirectory(path);
                string file = System.IO.Path.Combine(path, $"{tag}_{row}_{col}.png");
                img.SaveImage(file);
            }
        }
        void _DUMP(Mat img, string subFolder, string tag, string ext = ".png")
        {
            if (OPT_DUMP && img != null)
            {
                string path = $"d:\\paso.log\\Calib\\{subFolder}";
                JetEazy.IO.QxPathUtility.InitDirectory(path);
                string file = System.IO.Path.Combine(path, $"{subFolder}{tag}{ext}");
                img.SaveImage(file);
            }
        }
        void _DUMP_DOTS_PLATE_IMAGE(Mat srcFullfovImg, EzBlocsGrid grid, string folder, CarrierEnum C)
        {
#if (OPT_RESERVED)
            if (srcFullfovImg == null)
                return;

            string fileName = System.IO.Path.Combine(folder, $"calib_dots_plate_{C}.jpg");

            bool withRandomInking = true;
            int extraCornerSize = 800;
            int radius = 100;
            int blendDiv = 8;

            //var grid = matchResult?.Grid;
            if (grid == null)
            {
                var aoi = GaMvcConfig.SysModel.EmptyTrayAoiModel;
                var result = FetchGridNodes(aoi, C, srcFullfovImg, false);
                grid = result?.Grid;
                if (grid == null)
                    return;
            }

            int rows = grid.Rows;
            int cols = grid.Cols;

            using (Mat img = srcFullfovImg / blendDiv)
            {
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        var bloc = grid.Get(r, c);
                        if (bloc == null)
                            continue;

                        var cc = bloc.Center;
                        Cv2.Circle(img, (int)cc.X, (int)cc.Y, radius, Scalar.White, -1);
                    }
                }

                if (extraCornerSize > 0)
                {
                    var rnd = new Random();

                    var r = rows - 1;
                    var c = cols - 1;
                    var C0 = grid[0, 0].Center * 2.0 - grid[1, 1].Center;
                    var C1 = grid[0, c].Center * 2.0 - grid[1, c - 1].Center;
                    var C2 = grid[r, c].Center * 2.0 - grid[r - 1, c - 1].Center;
                    var C3 = grid[r, 0].Center * 2.0 - grid[r - 1, 1].Center;

                    foreach (var cc in new[] { C0, C1, C2, C3 })
                    {
                        var rect = Qcvt.CvCreateCenterRect((int)cc.X, (int)cc.Y, extraCornerSize, extraCornerSize);
                        Cv2.Rectangle(img, rect, Scalar.White, -1);

                        if (withRandomInking)
                        {
                            int iR = radius * 12 / 10;
                            rect.Inflate(-iR - 8, -iR - 8);
                            int ix = rect.X + rnd.Next(rect.Width);
                            int iy = rect.Y + rnd.Next(rect.Height);
                            Cv2.Circle(img, ix, iy, iR, Scalar.Gray, -1);
                        }
                    }
                }

                img.SaveImage(fileName);
            }

            //var file2 = System.IO.Path.ChangeExtension(fileName, "_empty.jpg");
            //srcFullfovImg.SaveImage(file2);
#endif
        }
        #endregion
    }
}