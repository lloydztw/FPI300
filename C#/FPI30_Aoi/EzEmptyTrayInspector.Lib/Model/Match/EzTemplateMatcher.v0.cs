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

using JetEazy.Match;
using JetEazy.OpenCV;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CvPoint = OpenCvSharp.Point;


namespace EzAoiEmptyTrayInspector.Model.Aoi.v0
{
    /// <summary>
    /// 通用的 Template Match 
    /// </summary>
    public class EzTemplateMatcher
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
        JxTempMatchSettings _recipe;
        bool _usingBlur = true;
        bool _usingLocMax = true;
        int _iterations => _recipe != null ? Math.Max(_recipe.Iterations.Value, 1) : 1;
        double _threshold => _recipe!=null ? (double)_recipe.ScoreThres.Value : 0.75;
        double _thresholdLow => _recipe != null ? (double)_recipe.ScoreThresLow.Value : 0.75;
        bool _removeOverlap => _recipe != null ? _recipe.RemoveOverlap.Value : false;
        #endregion

        #region PRIVATE_RUNTIME_DATA
        int _goldenWidth;
        int _goldenHeight;
        #endregion


        /// <summary>
        /// 建構式 (shrinkFactor > 1 可以縮小圖, 來加速計算)
        /// </summary>
        public EzTemplateMatcher(int shrinkFactor = 1)
        {
            _shrinkFactor = Math.Max(1, shrinkFactor);
        }

        public void SetRecipe(JxTempMatchSettings settings)
        {
            _recipe = settings;
        }

        public int ShrinkFactor
        {
            get { return _shrinkFactor; }
            set { _shrinkFactor = Math.Max(1, value); }
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

        public EzBloc FindBestBloc(Mat image, Mat golden, IEnumerable<CvPoint> anchorsLeftTop = null, Func<Mat, Mat> externFilter = null)
        {
            var boundRect = new Rectangle(0, 0, image.Width, image.Height);

            using (var simularity = new Mat())
            {
                _goldenWidth = golden.Width;
                _goldenHeight = golden.Height;

                var garbagesCan = new List<Mat>();

                normalizeGolden(golden, image, out golden, garbagesCan);

                apply_shrink(golden, image, out golden, out image, garbagesCan);

                // Filter
                if (externFilter != null)
                {
                    var goldenOld = golden;
                    var imageOld = image;
                    golden = externFilter(goldenOld);
                    image = externFilter(imageOld);
                    if (golden != goldenOld) goldenOld?.Dispose();
                    if (image != imageOld) imageOld?.Dispose();
                }
                else
                {
                    apply_filters(golden, image, out golden, out image, garbagesCan);
                }

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
