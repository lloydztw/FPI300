#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector.Model.Aoi;
using JetEazy.Match;
using JetEazy.OpenCV;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CvPoint = OpenCvSharp.Point;


namespace EzAoiEmptyTrayInspector.Model.Aoix
{
    /// <summary>
    /// 吸嘴格點 之內 異常餘料 偵測
    /// </summary>
    public class EzOnGridNgBlocsPredictor
    {
        public event EventHandler<ProgressEventArgs> OnProgress;

        #region NLOG
        // NOTE:
        // 不要做靜態初始化，而是要等到窗體的Load事件時才初始化Logger物件，
        // 且保證該窗體是 【首個使用】NLog 的 Class !!!
        // 這是因為NLog是在首次被使用時，才載入配置文件的。
        static NLog.ILogger _logger = null;
        static NLog.ILogger _LOG
        {
            get
            {
                if (_logger == null)
                    _logger = NLog.LogManager.GetCurrentClassLogger();
                return _logger;
            }
        }
        #endregion

        #region PRIVATE_DATA
        int _shrinkFactor = 1;
        #endregion

        #region RECIPE_PARAMS
        JxAoiRecipe _recipe;
        JxTempMatchSettings _recipeM;
        bool _usingBlur = true;
        bool _usingLocMax = true;
        int _iterations => _recipeM != null ? Math.Max(_recipeM.Iterations.Value, 1) : 1;
        double _threshold => _recipeM!=null ? (double)_recipeM.ScoreThres.Value : 0.75;
        double _thresholdLow => _recipeM != null ? (double)_recipeM.ScoreThresLow.Value : 0.75;
        bool _removeOverlap => _recipeM != null ? _recipeM.RemoveOverlap.Value : false;
        #endregion

        #region PRIVATE_RUNTIME_DATA
        int _goldenWidth;
        int _goldenHeight;
        #endregion


        /// <summary>
        /// 建構式 (shrinkFactor > 1 可以縮小圖, 來加速計算)
        /// </summary>
        public EzOnGridNgBlocsPredictor(int shrinkFactor = 1)
        {
            _shrinkFactor = Math.Max(1, shrinkFactor);
        }
        public void SetRecipe(JxAoiRecipe recipe)
        {
            _recipe = recipe;
        }
        public int ShrinkFactor
        {
            get { return _shrinkFactor; }
            set { _shrinkFactor = Math.Max(1, value); }
        }

        public Mat RebuildGoldenGridImage()
        {
            return rebuild_golden_grid_image();
        }

        public void Predict(Mat srcImg, Mat goldenGridImage, MatchResult matchResult, bool force = false)
        {
            Exception errEx = null;

            var _largeGoldenGridImage = goldenGridImage;
            var dumpPath = this.DumpPath;

            try
            {
                //changeState("POST Grid NG Matching", sideId);

                // Null Condition
                if (srcImg == null || _recipe == null || matchResult == null)
                    return;

                // FULL rows and cols
                var fullRows = _recipe.TrayMiscSettings.FullRows;
                var fullCols = _recipe.TrayMiscSettings.FullCols;

                // INPUT GRID 檢查是否已經滿盤定位
                var inputGrid = matchResult.Grid;
                if (inputGrid != null && inputGrid.Rows >= fullRows && inputGrid.Cols >= fullCols && !force)
                    return;

                // NOTE: goldenGrid 是由 recipe runtime deSerialize 
                var goldenGrid = _recipe.TrayMiscSettings.GetGoldenGrid();
                if (goldenGrid == null)
                    return;
                _LOG.Info($"GoldenGrid = {goldenGrid.Rows}x{goldenGrid.Cols}");

                // LARGE GOLDEN GRID IMAGE (rebuilt from recipe)
                if (_largeGoldenGridImage == null)
                    _largeGoldenGridImage = rebuild_golden_grid_image();
                if (_largeGoldenGridImage == null)
                {
                    _LOG.Warn("[AOI] largetGoldenGridImage 無重建!");
                    return;
                }
                //_DUMP_GOLDEN_GRID_IMAGE(_largeGoldenGridImage, goldenGrid, dumpPath);

                // OFFSET 
                int offset_x = 0;
                int offset_y = 0;
                if (inputGrid != null)
                    find_golden_grid_offset(srcImg, _largeGoldenGridImage, goldenGrid, inputGrid, out offset_x, out offset_y);

                // PSEUDO BLOCs (built by goldGrid + offset)
                var pseudoBlocs = new List<EzBloc>();
                foreach (var b in goldenGrid.IterBlocs())
                {
                    if (b != null)
                    {
                        var rect = b.Rect;
                        rect.Offset(offset_x, offset_y);
                        var bloc = new EzBloc(rect, score: -1.0);
                        pseudoBlocs.Add(bloc);
                    }
                }

                // EXISTING BLOCs (排除 null condition)
                var existingBlocs = matchResult.Blocs;
                if (existingBlocs == null)
                    existingBlocs = matchResult.Blocs = new List<EzBloc>();

                // PSEUDO BLOCs (remove overlaps)
                pseudoBlocs.RemoveAll(new Predicate<EzBloc>((pseudo) =>
                {
                    foreach (var bloc in existingBlocs)
                    {
                        if (bloc != null && bloc.Rect.Contains(pseudo.CenterX, pseudo.CenterY))
                            return true;
                    }
                    return false;
                }));
                pseudoBlocs.AddRange(existingBlocs);

                // 重建 grid
                var builder = new EzBlocsGridBuilder();
                var newGrid = builder.Build(pseudoBlocs);
                if (newGrid != null)
                {
                    foreach (var b in newGrid.IterBlocs())
                    {
                        if (b != null && b.Score < 0)
                            b.Tag = "pseudo";
                    }
                }

                // UPDATE to existing matchResult
                matchResult.Grid = newGrid;
            }
            catch (Exception ex)
            {
                errEx = ex;
            }
            finally
            {
                //if (errEx != null)
                //{
                //    _ERROR(ErrCodes.POST_GRID_MATCH_ERROR, sideId, errEx);
                //}
                //else
                //{
                //    changeState("Ready", sideId);
                //    update_one_match_result(sideId, matchResult, notify: true);
                //}
            }
        }


        #region PRIVATE_POST_PREDICT_GRID_NG_BLOCs
        void find_golden_grid_offset(Mat srcImg, Mat goldenGridImage, EzBlocsGrid goldenGrid, EzBlocsGrid inputGrid, out int offset_x, out int offset_y)
        {
            // LARGE GOLDEN TEMPLATE
            var ggRect = JetEazy.Qcvt.CV(goldenGrid.GetBoundary());
            var ggCenter = JetEazy.Qcvt.Center(ref ggRect);
            var ggTemplate = goldenGridImage[ggRect];

            // MATCH
            int shrink = EzAoiBaseUtil.GetShrinkFactor(srcImg.Width, srcImg.Height);
            var matcher = new EzTemplateMatcher(shrink);
            var settings = new JxTempMatchSettings();
            settings.ScoreThres.Value = 0.2m;
            matcher.SetRecipe(settings);

            // SEARCHING points
            var bestBloc = matcher.FindBestBloc(srcImg, ggTemplate, iterate_possible_offsets(goldenGrid, inputGrid));
            if (bestBloc != null && bestBloc.Score > 0.01)
            {
                var newCenter = bestBloc.Center;
                offset_x = (int)(newCenter.X - ggCenter.X);
                offset_y = (int)(newCenter.Y - ggCenter.Y);
            }
            else
            {
                offset_x = 0;
                offset_y = 0;
            }
        }
        IEnumerable<CvPoint> iterate_possible_offsets_000(EzBlocsGrid goldenGrid, EzBlocsGrid inputGrid)
        {
            int tryCount = 0;
            foreach (var ib in inputGrid.IterBlocs())
            {
                if (ib == null) continue;
                foreach (var gb in goldenGrid.IterBlocs())
                {
                    if (gb == null) continue;
                    var offset = ib.Center - gb.Center;
                    int left = (int)(ib.Rect.Left - offset.X);
                    int top = (int)(ib.Rect.Top - offset.Y);
                    yield return new CvPoint(left, top);
                }
                if (++tryCount >= 1000)
                    break;
            }
        }
        IEnumerable<CvPoint> iterate_possible_offsets_001(EzBlocsGrid goldenGrid, EzBlocsGrid inputGrid)
        {
            int fovWidth = _recipe.TrayMiscSettings.FovWidth;
            int fovHeight = _recipe.TrayMiscSettings.FovHeight;
            var boundary = new Rect(0, 0, fovWidth, fovHeight);
            var roi = JetEazy.Qcvt.CV(goldenGrid.GetBoundary());

            int tryCount = 0;
            foreach (var ib in inputGrid.IterBlocs())
            {
                if (ib == null) continue;
                foreach (var gb in goldenGrid.IterBlocs())
                {
                    if (gb == null) continue;
                    var offset = ib.Center - gb.Center;
                    roi.Left = (int)(ib.Rect.Left - offset.X);
                    roi.Top = (int)(ib.Rect.Top - offset.Y);
                    if (boundary.Contains(roi))
                        yield return roi.TopLeft;
                }
                if (++tryCount >= 1000)
                    break;
            }
        }
        IEnumerable<CvPoint> iterate_possible_offsets(EzBlocsGrid goldenGrid, EzBlocsGrid inputGrid)
        {
            int deltaRows = goldenGrid.Rows - inputGrid.Rows;
            int deltaCols = goldenGrid.Cols - inputGrid.Cols;
            for (int dRow = 0; dRow < deltaRows; dRow++)
            {
                for (int dCol = 0; dCol < deltaCols; dCol++)
                {
                    for (int inRow = 0; inRow < inputGrid.Rows; inRow++)
                    {
                        for (int inCol = 0; inCol < inputGrid.Cols; inCol++)
                        {
                            var inBloc = inputGrid.Get(inRow, inCol);
                            if (inBloc == null) continue;

                            int ggRow = inRow + dRow;
                            int ggCol = inCol + dCol;
                            var ggBloc = goldenGrid.Get(inRow, ggCol);
                            if (ggBloc == null) continue;

                            var offset = inBloc.Center - ggBloc.Center;
                            int left = (int)(inBloc.Rect.Left - offset.X);
                            int top = (int)(inBloc.Rect.Top - offset.Y);
                            yield return new CvPoint(left, top);
                        }
                    }
                }
            }
        }
        Mat rebuild_golden_grid_image(bool useColorFill = false)
        {
            if (_recipe == null)
                return null;

            var goldenGrid = _recipe.TrayMiscSettings.GetGoldenGrid();
            if (goldenGrid == null)
                return null;

            var W = _recipe.TrayMiscSettings.FovWidth;
            var H = _recipe.TrayMiscSettings.FovHeight;
            if (W < 10 || H < 10)
                return null;

            var largeGG = new Mat(H, W, MatType.CV_8UC1);
            largeGG.SetTo(Scalar.White);

            var goldenBmp = _recipe.VisionSettings.Match.GoldenBmp.Value as Bitmap;
            if (goldenBmp == null || useColorFill)
            {
                foreach (var bloc in goldenGrid.IterBlocs())
                {
                    if (bloc != null)
                    {
                        var rc = JetEazy.Qcvt.CV(bloc.Rect);
                        rc.Inflate(-8, -8);
                        largeGG.Rectangle(rc, Scalar.Gray, -1);
                    }
                }
            }
            else
            {
                using (var bridge = new QxImageBridge(goldenBmp))
                {
                    var goldenImg = bridge.Image;
                    int gw = goldenBmp.Width;
                    int gh = goldenBmp.Height;

                    foreach (var bloc in goldenGrid.IterBlocs())
                    {
                        if (bloc != null)
                        {
                            int x = bloc.CenterX - gw / 2;
                            int y = bloc.CenterY - gh / 2;
                            int x2 = x + gw;
                            int y2 = y + gh;
                            x = Math.Max(x, 0);
                            y = Math.Max(y, 0);
                            x2 = Math.Min(x2, W);
                            y2 = Math.Min(y2, H);
                            int ww = x2 - x;
                            int hh = y2 - y;
                            largeGG[y, y2, x, x2] = goldenImg[0, hh, 0, ww];
                        }
                    }
                }
            }

            //var ggBoundary = JetEazy.Qcvt.CV(goldenGrid.GetBoundary());
            //largeGG.Rectangle(ggBoundary, Scalar.Black, 5);

            return largeGG;
        }
        void clear_golden_grid_cache()
        {
            //_recipe?.TrayMiscSettings.GetGoldenGrid(true);
            //_largeGoldenGridImage?.Dispose();
            //_largeGoldenGridImage = null;
        }
        #endregion


        void run_post_out_grid_ng_detect(SideID sideId, Mat srcImg, MatchResult matchResult)
        {
            Exception errEx = null;

            try
            {
                //changeState("POST OutGrid NG Detecting", sideId);

                //// Null Condition
                //if (srcImg == null || _recipe == null || matchResult == null)
                //    return;

                //// FULL rows and cols
                //var fullRows = _recipeA.TrayMiscSettings.FullRows;
                //var fullCols = _recipeA.TrayMiscSettings.FullCols;

                //// RESULT GRID
                //var resultGrid = matchResult.Grid;
                //if (resultGrid != null)  // && inputGrid.Rows >= fullRows && inputGrid.Cols >= fullCols && !force)
                //    return;

                //// NOTE: goldenGrid 是由 recipe runtime deSerialize 
                //var goldenGrid = _recipe.TrayMiscSettings.GetGoldenGrid();
                //if (goldenGrid == null)
                //    return;
                //_LOG.Info($"GoldenGrid = {goldenGrid.Rows}x{goldenGrid.Cols}");

                //// LARGE GOLDEN GRID IMAGE (rebuilt from recipe)
                //if (_largeGoldenGridImage == null)
                //    _largeGoldenGridImage = rebuild_golden_grid_image();
                //if (_largeGoldenGridImage == null)
                //{
                //    _LOG.Warn("[AOI] largetGoldenGridImage 無重建!");
                //    return;
                //}
                //_DUMP_GOLDEN_GRID_IMAGE(_largeGoldenGridImage, goldenGrid, dumpPath);

                //// OFFSET 
                //int offset_x = 0;
                //int offset_y = 0;
                //if (resultGrid != null)
                //    find_golden_grid_offset(srcImg, _largeGoldenGridImage, goldenGrid, resultGrid, out offset_x, out offset_y);

                //// PSEUDO BLOCs (built by goldGrid + offset)
                //var pseudoBlocs = new List<EzBloc>();
                //foreach (var b in goldenGrid.IterBlocs())
                //{
                //    if (b != null)
                //    {
                //        var rect = b.Rect;
                //        rect.Offset(offset_x, offset_y);
                //        var bloc = new EzBloc(rect, -1);
                //        pseudoBlocs.Add(bloc);
                //    }
                //}

                //// EXISTING BLOCs (排除 null condition)
                //var existingBlocs = matchResult.Blocs;
                //if (existingBlocs == null)
                //    existingBlocs = matchResult.Blocs = new List<EzBloc>();

                //// PSEUDO BLOCs (remove overlaps)
                //pseudoBlocs.RemoveAll(new Predicate<EzBloc>((pseudo) =>
                //{
                //    foreach (var bloc in existingBlocs)
                //    {
                //        if (bloc != null && bloc.Rect.Contains(pseudo.CenterX, pseudo.CenterY))
                //            return true;
                //    }
                //    return false;
                //}));
                //pseudoBlocs.AddRange(existingBlocs);

                //// 重建 grid
                //var builder = new EzBlocsGridBuilder();
                //var newGrid = builder.Build(pseudoBlocs);
                //if (newGrid != null)
                //{
                //    foreach (var b in newGrid.IterBlocs())
                //    {
                //        if (b != null && b.Score < 0)
                //            b.Tag = "pseudo";
                //    }
                //}

                //// UPDATE to existing matchResult
                //matchResult.Grid = newGrid;
            }
            catch (Exception ex)
            {
                errEx = ex;
            }
            finally
            {
                //if (errEx != null)
                //{
                //    _ERROR(ErrCodes.POST_GRID_MATCH_ERROR, sideId, errEx);
                //}
                //else
                //{
                //    changeState("Ready", sideId);
                //    update_one_match_result(sideId, matchResult, notify: true);
                //}
            }
        }

        public List<EzBloc> FindBlocs(Bitmap sceneBmp, Bitmap goldenBmp)
        {
            using (var sceneBridge = new QxImageBridge(sceneBmp))
            using (var goldenBridge = new QxImageBridge(goldenBmp))
            {
                var image = sceneBridge.Image;
                var golden = goldenBridge.Image;
                return FindBlocs(image, golden);
            }
        }

        public List<EzBloc> FindBlocs(Mat image, Mat golden)
        {
            using (var simularity = new Mat())
            {
                _goldenWidth = golden.Width;
                _goldenHeight = golden.Height;

                var garbagesCan = new List<Mat>();
                
                normalizeGolden(golden, image, out golden, garbagesCan);

                apply_shrink(golden, image, out golden, out image, garbagesCan);

                apply_filters(golden, image, out golden, out image, garbagesCan);

                // Match
                _NOTIFY("Matching I");
                Cv2.MatchTemplate(image, golden, simularity, TemplateMatchModes.CCoeffNormed);

                // Runtime Golden
                using (var bestGolden = find_best_runtime_golden(image, golden, simularity, out double scoreG))
                {
                    _NOTIFY($"Matching I : low score = {scoreG:0.00}");
                    if (scoreG < _thresholdLow)
                    {
                        return new List<EzBloc>();
                    }

                    // Match again
                    _NOTIFY($"Matching II");
                    _DUMP_RUNTIME_GOLDEN(bestGolden);
                    Cv2.MatchTemplate(image, bestGolden, simularity, TemplateMatchModes.CCoeffNormed);
                }

                _DUMP_SIMULARITY(simularity);

                var locations = find_blocs_iteratively(simularity);

                #region CLEAN_UP
                foreach (var obj in garbagesCan)
                    obj?.Dispose();
                #endregion

                _NOTIFY("Matched");
                return locations;
            }
        }

        public EzBloc FindBestBloc(Mat image, Mat golden, IEnumerable<CvPoint> anchorsLeftTop = null)
        {
            var boundRect = new Rectangle(0, 0, image.Width, image.Height);

            using (var simularity = new Mat())
            {
                _goldenWidth = golden.Width;
                _goldenHeight = golden.Height;

                var garbagesCan = new List<Mat>();

                normalizeGolden(golden, image, out golden, garbagesCan);

                apply_shrink(golden, image, out golden, out image, garbagesCan);

                apply_filters(golden, image, out golden, out image, garbagesCan);

                // Match
                _NOTIFY("Matching (bestBloc)");
                Cv2.MatchTemplate(image, golden, simularity, TemplateMatchModes.CCoeffNormed);

                // Dump simularity
                _DUMP_SIMULARITY(simularity);

                // max simularity
                MinMaxLoc(simularity, out double minVal, out double maxVal, out var minLoc, out var maxLoc, anchorsLeftTop);
                double score = maxVal;
                double x = maxLoc.X;
                double y = maxLoc.Y;

                // 轉換到 world coordinates
                var zoomFactor = Math.Max(1, _shrinkFactor);
                int wX = (int)(x * zoomFactor);
                int wY = (int)(y * zoomFactor);

                var bestRect = new Rectangle(wX, wY, _goldenWidth, _goldenHeight);
                JetEazy.QUtilities.QUtility.ClipBoundary(ref bestRect, ref boundRect);
                var bestBloc = new EzBloc(bestRect, score);

                #region CLEAN_UP
                foreach (var obj in garbagesCan)
                    obj?.Dispose();
                #endregion

                _NOTIFY("Matched (bestBloc)");
                return bestBloc;
            }
        }

        void MinMaxLoc(Mat simularity, out double minVal, out double maxVal, out CvPoint minLoc, out CvPoint maxLoc, IEnumerable<CvPoint> anchorsLeftTop)
        {
            bool isFound = false;

            minVal = double.MaxValue;
            maxVal = double.MinValue;
            minLoc = new CvPoint(0, 0);
            maxLoc = new CvPoint(0, 0);

            if (anchorsLeftTop != null)
            {
                var bound = new Rect(0, 0, simularity.Width, simularity.Height);
                var zoomFactor = Math.Max(1, _shrinkFactor);
                foreach (var pt in anchorsLeftTop)
                {
                    int x = pt.X / zoomFactor;
                    int y = pt.Y / zoomFactor;
                    if (!bound.Contains(x, y))
                        continue;

                    var val = simularity.At<float>(y, x);
                    if(val > maxVal)
                    {
                        maxVal = val;
                        maxLoc.X= x; 
                        maxLoc.Y = y;
                    }    

                    if (val < minVal)
                    {
                        minVal = val;
                        minLoc.X= x;
                        minLoc.Y= y;
                    }

                    isFound = true;
                }
            }

            if (!isFound)
            {
                simularity.MinMaxLoc(out minVal, out maxVal, out minLoc, out maxLoc);
            }
        }

        #region PRIVATE_FUNCTIONS
        void normalizeGolden(Mat golden, Mat image, out Mat newGolden, List<Mat> garbagesCan)
        {
            // 注意: 使用 CvtColor 會影響比對 !!!
            if (golden.Channels() > image.Channels())
            {
                _NOTIFY("[Warning] golden 與 image 格式不一致!");
                Mat[] oldArr = golden.Split();
                Mat[] newArr = new Mat[image.Channels()];
                Array.Copy(oldArr, newArr, newArr.Length);
                newGolden = new Mat();
                Cv2.Merge(newArr, newGolden);
                garbagesCan.Add(newGolden);
            }
            else
            {
                newGolden = golden;
            }

            if (newGolden.Channels() != image.Channels())
                throw new Exception("golden 與 image 格式不一致!");
        }
        void apply_shrink(Mat golden, Mat image, out Mat newGolden, out Mat newImage, List<Mat> garbagesCan)
        {
            //var interpo = InterpolationFlags.Nearest;
            var interpo = InterpolationFlags.Linear;

            newGolden = golden;
            newImage = image;

            if (_shrinkFactor > 1)
            {
                _NOTIFY("Factoring");
                var size = golden.Size();
                size.Width /= _shrinkFactor;
                size.Height /= _shrinkFactor;
                if (size.Width > 2 && size.Height > 2)
                {
                    newGolden = golden.Resize(size, 0, 0, interpo);
                    garbagesCan.Add(newGolden);

                    size = image.Size();
                    size.Width /= _shrinkFactor;
                    size.Height /= _shrinkFactor;
                    newImage = image.Resize(size, 0, 0, interpo);
                    garbagesCan.Add(newImage);
                }
                else
                {
                    _shrinkFactor = 1;
                }
            }

            _DUMP_SHRINK(image, golden);
        }
        void apply_filters(Mat golden, Mat image, out Mat newGolden, out Mat newImage, List<Mat> garbagesCan)
        {
            if (_usingBlur)
            {
                _NOTIFY("Filtering");
                newGolden = apply_filters(golden);
                newImage = apply_filters(image);
                garbagesCan.Add(newGolden);
                garbagesCan.Add(newImage);
                _DUMP_FILTERED(image, golden);
            }
            else
            {
                newGolden = golden;
                newImage = image;
            }
        }
        Mat apply_filters(Mat src)
        {
            //return src.MedianBlur(7);
            var sz = new OpenCvSharp.Size(3, 3);
            //return src.GaussianBlur(sz, 1.0);
            return src.Blur(sz);
        }
        Mat find_best_runtime_golden(Mat image, Mat golden, Mat simularity, out double score)
        {
            simularity.MinMaxLoc(out double minVal, out double maxVal, out var minLoc, out var maxLoc);
            var roi = new Rect(maxLoc.X, maxLoc.Y, golden.Width, golden.Height);
            var bound = new Rect(0,0,image.Width,image.Height);
            JetEazy.Qcvt.ClipBoundary(ref roi, ref bound);
            var bestGolden = image[roi].Clone();
            score = maxVal;
            return bestGolden;
        }
        List<EzBloc> find_blocs_iteratively(Mat simularity)
        {
            var results = new List<EzBloc>();

            try
            {
                using (var binary = new Mat())
                using (var scoreT = new Mat())
                {
                    var scoreThres = _threshold;
                    var delta = _iterations > 1 ? Math.Abs(_threshold - _thresholdLow) / (_iterations - 1) : 0;
                    var zoomFactor = Math.Max(1, _shrinkFactor);

                    _NOTIFY("Finding Blocs");

                    for (int i = 0; i < _iterations; i++, scoreThres -= delta)
                    {
                        // Threshold
                        Cv2.Threshold(simularity, scoreT, scoreThres, 1.0, ThresholdTypes.Binary);
                        scoreT.ConvertTo(binary, MatType.CV_8U, 255);

                        // 抹去已經找過的 Bloc
                        foreach (var foundBloc in results)
                        {
                            Rect shrinkBound = new Rect(0, 0, binary.Width, binary.Height);
                            Rect roi = JetEazy.Qcvt.CV(foundBloc.Rect);
                            roi.X /= zoomFactor;
                            roi.Y /= zoomFactor;
                            roi.Width /= zoomFactor;
                            roi.Height /= zoomFactor;
                            JetEazy.Qcvt.ClipBoundary(ref roi, ref shrinkBound);
                            if (roi.Width > 0 && roi.Height > 0)
                                binary[roi].SetTo(Scalar.Black);
                        }

                        var blocs = find_blocs(binary, simularity, i, !_usingLocMax);

                        if (_removeOverlap)
                            blocs = remove_overlaps(blocs);

                        results.AddRange(blocs);
                    }
                }
            }
            catch (Exception ex)
            {
                _LOG.Error(ex.Message);
            }

            return results;
        }
        List<EzBloc> find_blocs(Mat binary, Mat simularity, int iterId = -1, bool useCentroid = true)
        {
            var blocs = new List<EzBloc>();

            var cc = Cv2.ConnectedComponentsEx(binary);
            if (cc.LabelCount <= 1)
                return blocs;

            #region DUMP
            if (iterId == 0)
                _DUMP_CC_BLOBs(cc);
            #endregion

            var zoomFactor = Math.Max(1, _shrinkFactor);
            int blobGoldenW = _goldenWidth / zoomFactor;
            int blobGoldenH = _goldenHeight / zoomFactor;

            foreach (var blob in cc.Blobs)
            {
                if (blob.Width > blobGoldenW || blob.Height > blobGoldenH)
                    continue;

                // 在 blob 中, 找出最高分的點位
                simularity[blob.Rect].MinMaxLoc(out double minVal, out double maxVal, out var minLoc, out var maxLoc);
                double score = maxVal;
                double x = useCentroid ? blob.Centroid.X : (double)maxLoc.X + blob.Rect.X;
                double y = useCentroid ? blob.Centroid.Y : (double)maxLoc.Y + blob.Rect.Y;

                int wX = (int)(x * zoomFactor);
                int wY = (int)(y * zoomFactor);
                var worldLoc = new EzBloc(new Rectangle(wX, wY, _goldenWidth, _goldenHeight), score);

                blocs.Add(worldLoc);
            }

            return blocs;
        }
        List<EzBloc> remove_overlaps(List<EzBloc> blocs, List<EzBloc> residuals = null)
        {
            var nonOverlappingBlocs = new List<EzBloc>();

            // 按 Score 大小排序，保留 Score 高的
            foreach (var bloc in blocs.OrderByDescending(b => b.Score))
            {
                // 如果與 nonOverlappingBlocs 中的任何一個不重疊，則加入
                //if (!nonOverlappingBlocs.Any(b => b.Rect.IntersectsWith(bloc.Rect)))
                if (!nonOverlappingBlocs.Any(b => b.Rect.Contains((int)bloc.Center.X, (int)bloc.Center.Y)))
                {
                    nonOverlappingBlocs.Add(bloc);
                }
            }

            return nonOverlappingBlocs;
        }
        #endregion

        public string DumpPath
        {
            get
            {
                return _dumpPath;
            }
            set
            {
                _DUMP_INIT(value);
            }
        }

        #region PRIVATE_DUMP_FUNCTIONS
        bool _isDumpEnabled = false;
        string _dumpPath = null;
        void _DUMP_INIT(string pathStem)
        {
            _dumpPath = pathStem;
            _isDumpEnabled = pathStem != null;
        }
        void _DUMP_SHRINK(Mat image, Mat golden)
        {
            if (_isDumpEnabled) // && !BmpUtil.IsLarge(image))
            {
                //image?.SaveImage(System.IO.Path.Combine(_dumpPathStem, "match_image_src.png"));
                //golden?.SaveImage(System.IO.Path.Combine(_dumpPathStem, "match_golden.png"));
                _DUMP(image, "_1-shrink-image.png");
                _DUMP(golden, "_1-shrink-golden.png");
            }
        }
        void _DUMP_FILTERED(Mat image, Mat golden)
        {
            if (_isDumpEnabled) // && !BmpUtil.IsLarge(image))
            {
                _DUMP(image, "_2-filter-image.png");
                _DUMP(golden, "_2-filter-golden.png");
            }
        }
        void _DUMP_RUNTIME_GOLDEN(Mat golden)
        {
            if (_isDumpEnabled)
            {
                _DUMP(golden, "_3-runtime-golden.png");
            }
        }
        void _DUMP_SIMULARITY(Mat simularity)
        {
            if (_isDumpEnabled)
            {
                _DUMP(simularity, "_4-simulariy.png");
            }
        }
        void _DUMP_CC_BLOBs(ConnectedComponents cc)
        {
            if (_isDumpEnabled && cc != null)
            {
                using (Mat canvas = new Mat())
                {
                    cc.RenderBlobs(canvas);
                    //canvas.SaveImage(System.IO.Path.Combine(_dumpPathStem, "match_blobs.png"));
                    _DUMP(canvas, "_5-ccBlobs.png");
                }
            }
        }
        void _DUMP(Mat image, string dumpFile)
        {
            try
            {
                if (image == null)
                    return;
                JetEazy.IO.QxPathUtility.InitDirectory(_dumpPath);
                string stem = System.IO.Path.GetFileName(_dumpPath);
                string size_tag = $"-{image.Width}x{image.Height}.png";
                dumpFile = System.IO.Path.Combine(_dumpPath, stem + dumpFile + size_tag);
                image?.SaveImage(dumpFile);
            }
            catch
            {
                _isDumpEnabled = false;
            }
        }
        void _NOTIFY(string message)
        {
            OnProgress?.Invoke(this, new ProgressEventArgs(message));
        }
        #endregion
    }
}
