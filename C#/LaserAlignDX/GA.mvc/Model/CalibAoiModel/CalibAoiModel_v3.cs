#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-07 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector;
using EzAoiEmptyTrayInspector.Model;
using EzAoiEmptyTrayInspector.Model.Aoi;
using JetEazy.EzImage;
using JetEazy.Match;
using JetEazy.QMath;
using JetEazy.QvMath;
using JetEazy.Utils;
using LaserAlignDX.AoiModel.Calib;
using LaserAlignDX.Mvc.Model;
using LeTian.AoiLib;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using ErrCodes = EzAoiEmptyTrayInspector.Model.ErrCodes;


namespace LaserAlignDX.AoiModel
{
    public class CalibAoiModel : ICalibAoiModel
    {
        public static bool OPT_DUMP = false;

        #region PRIVATE_DATA
        ITravelerModel _sysModel => GaMvcConfig.SysModel;
        JxCalibRecipe _jxCalibRecipe;
        //IxEmptyTrayInspector _externImp;
        //JxAoiRecipe _jxRecipe;
        #endregion

        #region WRAPPER_FUNCTIONS_For_IxEmptyTrayInspector
#if (OPT_DEPRECATED_AFTER_VER_3200)
        public event EventHandler<MatchResultEventArgs> OnMatched
        {
            add
            {
                _externImp.OnMatched += value;
            }

            remove
            {
                _externImp.OnMatched -= value;
            }
        }
        public event EventHandler<AoiResultEventArgs> OnFinalResulted
        {
            add
            {
                _externImp.OnFinalResulted += value;
            }

            remove
            {
                _externImp.OnFinalResulted -= value;
            }
        }
        public event EventHandler OnStateChanged
        {
            add
            {
                _externImp.OnStateChanged += value;
            }

            remove
            {
                _externImp.OnStateChanged -= value;
            }
        }
        public int ID => _externImp.ID;
        public object State => _externImp.State;
        public int AddRef()
        {
            return _externImp.AddRef();
        }
        public ErrCodes BuildGoldenGridTemplate(SideID sideId, IEzImage largeImg)
        {
            return _externImp.BuildGoldenGridTemplate(sideId, largeImg);
        }
        public ErrCodes CanMatch(SideID sideId, IEzImage img)
        {
            return _externImp.CanMatch(sideId, img);
        }
        public ErrCodes CanRunAll(IEzImage imgA, IEzImage imgB = null)
        {
            return _externImp.CanRunAll(imgA, imgB);
        }
        public MatchResult GetMatchResult(SideID sideId)
        {
            return _externImp.GetMatchResult(sideId);
        }
        public EzEmptyTrayResult GetResult()
        {
            return _externImp.GetResult();
        }
        public bool IsError()
        {
            return _externImp.IsError();
        }
        public bool IsReady()
        {
            return _externImp.IsReady();
        }
        public bool IsSafeToExit()
        {
            return _externImp.IsSafeToExit();
        }
        public void ResetAndClear(SideID sideId = SideID.All)
        {
            _externImp.ResetAndClear(sideId);
        }
        public void RunAll(IEzImage imgA, IEzImage imgB = null, string outputFile = null, string dumpPath = null, bool wait = false)
        {
            _externImp.RunAll(imgA, imgB, outputFile, dumpPath, wait);
        }
        public void RunAll(Bitmap bmp, bool wait = true, string dumpPath = null)
        {
            _externImp.RunAll(bmp, wait, dumpPath);
        }
        public void RunMatch(SideID sideId, IEzImage img, string dumpPath = null)
        {
            _externImp.RunMatch(sideId, img, dumpPath);
        }
        public void TryApplyFilters(SideID sideId, IEzImage img, JxRotAngleSettings settings, out object result)
        {
            _externImp.TryApplyFilters(sideId, img, settings, out result);
        }
        public bool CropGoldenTemplate(SideID sideId, IEzImage largeImg, Rectangle goldenRect)
        {
            // 目前 在此 '不需要' 精選
            // 由 RefineCentroidLocations 負責 調整 更精確位置
            #region 自動精選_goldenRect
            if (true)
            {
                using (var bmpCrop = ImageUtil.CropBmp(largeImg, goldenRect))
                using (var bridge = new JetEazy.OpenCV.QxImageBridge(bmpCrop))
                using (var mask = new Mat())
                {
                    bool inversed = _jxRecipe.VisionSettings.Inverse.Value;
                    double thresh = _jxRecipe.VisionSettings.OutGridBlocThreshold.Value;
                    if (thresh > 0)
                    {
                        Cv2.Threshold(bridge.Image, mask, thresh, 255, ThresholdTypes.Binary);
                    }
                    else
                    {
                        thresh = Cv2.Threshold(bridge.Image, mask, 0, 255, ThresholdTypes.Otsu);
                        GaUtil.LOG($"Otsu Thresh = {(int)thresh}");
                    }
                    if (inversed)
                        Cv2.BitwiseNot(mask, mask);

                    findWhiteBlobs(mask, out var whiteBlobs);
                    if (whiteBlobs.Count > 0)
                    {
                        whiteBlobs.Sort((b1, b2) => b2.Pixels - b1.Pixels);
                        var rect = whiteBlobs[0].Rect;
                        rect.X += goldenRect.X;
                        rect.Y += goldenRect.Y;
                        goldenRect = rect;
                    }
                }
            }
            #endregion

            return _externImp.CropGoldenTemplate(sideId, largeImg, goldenRect);
        }
#endif
        #endregion

        public CalibAoiModel(object dummy = null)
        {
            // Caller 負責調用 imp.Dispose()
            //_externImp = imp;
        }

        public void Dispose()
        {
            // jxCalibRecipe 由 caller 維護其生命週期
            _jxCalibRecipe = null;
        }

        public void ResetAndClear()
        {
            //_externImp?.ResetAndClear();
        }

        public void SetRecipe(JxCalibRecipe recipe)
        {
            _jxCalibRecipe = recipe;
        }

#if (OPT_DEPRECATED_AFTER_VER_3200)
        /// <summary>
        /// 設定參數
        /// </summary>
        public void SetRecipe(JxAoiRecipe recipe)
        {
            // recipe 會被 _externImp 持有,
            // 所以 _jxRecipe  不用 Dispose
            _externImp.SetRecipe(recipe);
            _jxRecipe = recipe;
        }

        public ErrCodes BuildGoldenGridTemplate(object dummy, IEzImage ezImage)
        {
            return _externImp.BuildGoldenGridTemplate(SideID.A, ezImage);
        }
#endif

        /// <summary>
        /// 抓取 空載台格位點
        /// </summary>
        /// <param name="fullfovImg"></param>
        /// <param name="refine"></param>
        public MatchResult FetchGridNodes(CarrierEnum carrierID, Mat fullfovImg, bool refine)
        {
            ////(1) Peek the ezImage
            //using (var ezImage = new EzQuickImage(fullfovImg, deepCopy: false))
            //{
            //    //(2) Run Aoi
            //    _externImp.RunMatch(SideID.A, ezImage);
            //    //_externImp.RunAll((IEzImage)ezImage, wait: true);

            //    //(3) Result
            //    var matchResult = _externImp.GetMatchResult(SideID.A);
            //    //var result = _externImp.GetResult();

            //    //(4) Refine each detail locations
            //    if (refine)
            //    {
            //        RefineCentroidLocations(matchResult, ezImage);
            //    }

            //    _DUMP_DOTS_PLATE_IMAGE(ezImage.Image as Mat, matchResult, $"d:\\paso.log\\calib_dots_plate_{carrierID}.jpg");

            //    return matchResult;
            //}
            return null;
        }

        /// <summary>
        /// Caller 必須維護 fullfovImg 與 recipe 生命週期
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
            var maxSz = Math.Min(roi.Width, roi.Height) / 8;

            using (var imgCrop = fullfovImg[roi].Clone())
            {
                if (thres <= 0)
                    Cv2.Threshold(imgCrop, imgCrop, 0, 255, ThresholdTypes.Otsu);
                else
                    Cv2.Threshold(imgCrop, imgCrop, thres, 255, ThresholdTypes.Binary);

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
                return grid;
            }
        }

        /// <summary>
        /// Caller 必須維護 fullfovImg 與 recipe 生命週期
        /// </summary>
        public EzBloc[] FetchInkMarks(CarrierEnum carrierID, SuckerRowEnum suckerID, Mat fullfovImg, JxCalibRecipe recipe)
        {
            var inkMarks = new EzBloc[0];

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
                if (inkGrid != null)
                    inkMarks = inkGrid.GetCornerBlocs();
            }

            return inkMarks;
        }

        /// <summary>
        /// Caller 必須維護 fullfovImg 與 recipe 生命週期
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

        public bool AdjustBadNodes(CarrierEnum carrierID, EzBlocsGrid grid)
        {
            bool isChanged = false;

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

            return isChanged;
        }

#if (OPT_RESERVED)
        Mat TryRun(Mat fullfovImg, out MatchResult matchResult, bool optOutputBindaryImage = false)
        {
            //(1) Peek the ezImage
            using (var ezImage = new EzQuickImage(fullfovImg, deepCopy: false))
            {
                //(2) Run Aoi
                _externImp.RunMatch(SideID.A, ezImage);

                //(3) Result
                matchResult = _externImp.GetMatchResult(SideID.A);

                //(4) Refine each detail locations
                this.RefineCentroidLocations(matchResult, ezImage);

                return null;
            }
        }
        Mat RefineCentroidLocations_000(MatchResult matchResult, Mat fullfovImg, bool optOutputBindaryImage = false)
        {
            var grid = matchResult?.Grid;
            if (grid == null || _jxRecipe == null)
                return null;

            bool isBlackCarrier = checkIfDarkBackground(fullfovImg);
            if (isBlackCarrier)
            {
                return _RefineCentroidLocations_BlackCarrier(matchResult, fullfovImg, optOutputBindaryImage);
            }
            else
            {
                return  _RefineCentroidLocations_WhiteCarrier(matchResult, fullfovImg, optOutputBindaryImage);
            }
        }
        Mat _RefineCentroidLocations_WhiteCarrier(MatchResult matchResult, Mat fullfovImg, bool optOutputBinaryImage = false)
        {
            var grid = matchResult?.Grid;
            if (grid == null || _jxRecipe == null)
                return null;

            //bool isBlackCarrier = checkIfDarkBackground(fullfovImg);
            int thresh = _jxRecipe.VisionSettings.OutGridBlocThreshold.Value;
            var bound = new Rect(0, 0, fullfovImg.Width, fullfovImg.Height);
            int rows = grid.Rows;
            int cols = grid.Cols;

            Mat imgOutput = optOutputBinaryImage ? Mat.Zeros(fullfovImg.Size(), MatType.CV_8UC1) : null;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var bloc = grid.Get(r, c);
                    if (bloc == null) continue;

                    var roi = JetEazy.Qcvt.CV(bloc.Rect);
                    JetEazy.Qcvt.ClipBoundary(ref roi, ref bound);

                    using (var img = fullfovImg[roi].Clone())
                    using (var binary = new Mat())
                    {
                        // 將 白色載台影像 反向, 形成 淺色晶粒 深色背景
                        Cv2.BitwiseNot(img, img);
                        if (thresh <= 0)
                            Cv2.Threshold(img, binary, 0, 255, ThresholdTypes.Otsu);
                        else
                            Cv2.Threshold(img, binary, thresh, 255, ThresholdTypes.Binary);

                        _DUMP(img, "img", r, c);
                        _DUMP(binary, "binary", r, c);
                        if (imgOutput != null)
                            imgOutput[roi] = binary;

                        findWhiteBlobs(binary, out var whiteBlobs);
                        if (whiteBlobs.Count == 0) continue;
                        whiteBlobs.Sort((b1, b2) => b2.Pixels - b1.Pixels);

                        // 使用質心 (for 白色載台)
                        var center = whiteBlobs[0].Center;
                        center.X += roi.X;
                        center.Y += roi.Y;
                        bloc.Center = center;
                        var rc = JetEazy.Qcvt.CreateCenterRect((float)center.X, (float)center.Y, (float)bloc.Rect.Width, (float)bloc.Rect.Height);
                        bloc.Rect = Rectangle.Round(rc);
                    }
                }
            }

            return imgOutput;
        }
        Mat _RefineCentroidLocations_BlackCarrier(MatchResult matchResult, Mat fullfovImg, bool optOutputBinaryImage = false)
        {
            var grid = matchResult?.Grid;
            if (grid == null || _jxRecipe == null)
                return null;

            //>>> bool isBlackCarrier = checkIfDarkBackground(fullfovImg);
            int thresh = _jxRecipe.VisionSettings.OutGridBlocThreshold.Value;
            var bound = new Rect(0, 0, fullfovImg.Width, fullfovImg.Height);
            int rows = grid.Rows;
            int cols = grid.Cols;

            Mat imgOutput = null;
            if (optOutputBinaryImage)
                imgOutput = Mat.Zeros(fullfovImg.Size(), MatType.CV_8UC1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var bloc = grid.Get(r, c);
                    if (bloc == null) continue;
                    if (!bloc.IsMajorNode())
                        continue;

                    var roi = JetEazy.Qcvt.CV(bloc.Rect);
                    JetEazy.Qcvt.ClipBoundary(ref roi, ref bound);

                    using (var img = fullfovImg[roi].Clone())
                    using (var binary = new Mat())
                    {
                        if (thresh <= 0)
                            Cv2.Threshold(img, binary, 0, 255, ThresholdTypes.Otsu);
                        else
                            Cv2.Threshold(img, binary, thresh, 255, ThresholdTypes.Binary);

                        Cv2.Erode(binary, binary, null, iterations: 2);
                        Cv2.Dilate(binary, binary, null, iterations: 2);
                        Cv2.Rectangle(binary, new Rect(0, 0, binary.Width, binary.Height), Scalar.White, 2);
                        Cv2.FloodFill(binary, new OpenCvSharp.Point(0, 0), Scalar.Black);

                        _DUMP(img, "img", r, c);
                        _DUMP(binary, "binary", r, c);
                        if (imgOutput != null)
                            imgOutput[roi] = binary;

                        findWhiteBlobs(binary, out var whiteBlobs);
                        if (whiteBlobs.Count == 0) continue;
                        whiteBlobs.Sort((b1, b2) => b2.Pixels - b1.Pixels);

                        //>>> var center = whiteBlobs[0].Center;
                        // 黑色載台 使用 質心誤差大, 改用 rect 中心
                        var center = JetEazy.Qcvt.CenterF(ref whiteBlobs[0].Rect);
                        center.X += roi.X;
                        center.Y += roi.Y;
                        bloc.Center = new JetEazy.QMath.QVector(center.X, center.Y);
                        var rc = JetEazy.Qcvt.CreateCenterRect((float)center.X, (float)center.Y, (float)bloc.Rect.Width, (float)bloc.Rect.Height);
                        bloc.Rect = Rectangle.Round(rc);
                    }
                }
            }

            return imgOutput;
        }
#endif

        #region PRIVATE_LOCATE_FUNCTIONS
        void RefineCentroidLocations(MatchResult matchResult, Mat fullfovImg)
        {
            //var grid = matchResult?.Grid;
            //if (grid == null || _jxRecipe == null)
            //    return;

            //int thresh = _jxRecipe.VisionSettings.OutGridBlocThreshold.Value;
            //var bound = new Rect(0, 0, fullfovImg.Width, fullfovImg.Height);
            //int rows = grid.Rows;
            //int cols = grid.Cols;

            //bool isBlackCarrier = checkIfDarkBackground(fullfovImg);
            //var findLocalCenter = isBlackCarrier ?
            //    (Func<Mat, Mat, int, string, QVector>)this.FindLocalCenter_black_carrier_cc :
            //    (Func<Mat, Mat, int, string, QVector>)this.FindLocalCenter_white_carrier;

            //for (int r = 0; r < rows; r++)
            //{
            //    for (int c = 0; c < cols; c++)
            //    {
            //        var bloc = grid.Get(r, c);
            //        if (bloc == null)
            //            continue;


            //        var roi = JetEazy.Qcvt.CV(bloc.Rect);
            //        JetEazy.Qcvt.ClipBoundary(ref roi, ref bound);

            //        using (var img = fullfovImg[roi].Clone())
            //        using (var binary = new Mat())
            //        {
            //            string dumpTag = OPT_DUMP ? $"@{r}_{c}" : null;
            //            var center = findLocalCenter(img, binary, thresh, dumpTag);

            //            if (center == null)
            //                continue;

            //            center.X += roi.X;
            //            center.Y += roi.Y;
            //            bloc.Center = center;
            //            var rect = JetEazy.Qcvt.CreateCenterRect((float)center.X, (float)center.Y, (float)bloc.Rect.Width, (float)bloc.Rect.Height);
            //            bloc.Rect = Rectangle.Round(rect);
            //        }
            //    }
            //}
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
        void _DUMP(Mat img, string subFolder, string tag)
        {
            if (OPT_DUMP && img != null)
            {
                string path = $"d:\\paso.log\\Calib\\{subFolder}";
                JetEazy.IO.QxPathUtility.InitDirectory(path);
                string file = System.IO.Path.Combine(path, $"{subFolder}{tag}.png");
                img.SaveImage(file);
            }
        }
        void _DUMP_DOTS_PLATE_IMAGE(Mat srcFullfovImg, MatchResult matchResult, string fileName)
        {
            return;

            //int radius = 250;

            //var grid = matchResult?.Grid;
            //if (grid == null || _jxRecipe == null || srcFullfovImg == null)
            //    return;

            //int rows = grid.Rows;
            //int cols = grid.Cols;

            //using(Mat img = srcFullfovImg / 4)
            //{
            //    for (int r = 0; r < rows; r++)
            //    {
            //        for (int c = 0; c < cols; c++)
            //        {
            //            var bloc = grid.Get(r, c);
            //            if (bloc == null)
            //                continue;

            //            var cc = bloc.Center;
            //            Cv2.Circle(img, (int)cc.X, (int)cc.Y, radius, Scalar.White, -1);
            //        }
            //    }

            //    img.SaveImage(fileName);
            //}

            //var file2 = System.IO.Path.ChangeExtension(fileName, "_empty.jpg");
            //srcFullfovImg.SaveImage(file2);
        }
        #endregion
    }
}