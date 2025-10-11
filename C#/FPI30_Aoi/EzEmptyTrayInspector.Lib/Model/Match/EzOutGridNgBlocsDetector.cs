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
using JetEazy.QvMath;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using CvPoint = OpenCvSharp.Point;


namespace EzAoiEmptyTrayInspector.Model.Aoi
{
    /// <summary>
    /// 吸嘴格點 之外 異常餘料 偵測
    /// </summary>
    public partial class EzOutGridNgBlocsDetector
    {
        public event EventHandler<ProgressEventArgs> OnProgress;

        public static double NG_SCORE => SCORES.NG_OUT_GRID;
        public static string NG_TAG = "OUT_GRID";

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
        Bitmap _suckerGoldenBmp => _recipe?.VisionSettings.Match.GoldenBmp;
        bool _isDarkBkGnd;
        int _fovWidth;
        int _fovHeight;
        #endregion

        /// <summary>
        /// 建構式 (shrinkFactor > 1 可以縮小圖, 來加速計算)
        /// </summary>
        public EzOutGridNgBlocsDetector(int shrinkFactor = 1)
        {
            _shrinkFactor = Math.Max(1, shrinkFactor);
        }

        public void SetRecipe(JxAoiRecipe recipe, bool isDarkBkGnd)
        {
            _recipe = recipe;
            _isDarkBkGnd = isDarkBkGnd;

        }
        public int ShrinkFactor
        {
            get { return _shrinkFactor; }
            set { _shrinkFactor = Math.Max(1, value); }
        }

        public List<EzBloc> FindNgBlocs(Mat srcImg, EzBlocsGrid grid)
        {
            if (srcImg == null)
                return null;

            var garbagesCan = new List<Mat>();
            _fovWidth = srcImg.Width;
            _fovHeight = srcImg.Height;
            int ogThreshold = _recipe.VisionSettings.OutGridBlocThreshold.Value;

            try
            {
                using (var imgWork = new Mat())
                using (var bridgeSG = new QxImageBridge(_suckerGoldenBmp))
                {
                    var suckerGoldenImg = bridgeSG.Image;
                    //bool isDarkBkGnd = EzBlobFinder.CheckIfDarkBackGround(suckerGoldenImg);

                    // 0 To Shrink Domain
                    normalizeGolden(suckerGoldenImg, srcImg, out suckerGoldenImg, garbagesCan);
                    apply_shrink(suckerGoldenImg, srcImg, out suckerGoldenImg, out srcImg, garbagesCan);

                    // 1 gridMask: filled by OnGrid Blocs
                    Mat gridMask = new Mat(srcImg.Size(), srcImg.Type());
                    gridMask.SetTo(Scalar.Black);
                    fill_grid_blocs(gridMask, grid.IterBlocs(), Scalar.White, _shrinkFactor);
                    _DUMP(gridMask, "gridMask_1.png");

                    // 2 gridMask: get minAreaRect
                    find_major_minAreaRect(gridMask, out RotatedRect minAreaRect);
                    //>>> _DUMP(gridMask, "gridMask_2.png");

                    // 3 gridMask: fill minAreaRect
                    gridMask.SetTo(Scalar.Black);
                    fill_minAreaRect(gridMask, ref minAreaRect, Scalar.White);
                    _DUMP(gridMask, "gridMask_3.png");

                    // 4 srcImg | gridMask ==> imgWork
                    Cv2.BitwiseOr(srcImg, gridMask, imgWork);
                    _DUMP(imgWork, "imgWork.png");

                    // 5. threshold
                    double otsu;
                    if (ogThreshold <= 0)
                    {
                        // 5.O1 fill mean color before otsu
                        var bkColor = suckerGoldenImg.Mean();
                        imgWork.SetTo(bkColor, gridMask);
                        _DUMP(imgWork, "imgWork_5_bk.png");

                        // 5.O2 Otsu
                        otsu = Cv2.Threshold(imgWork, imgWork, 0, 255, ThresholdTypes.Otsu);
                        _LOG.Info("OutGrid Blocs Otsu = {0}", otsu);
                        _DUMP(imgWork, "imgWork_5_o.png");
                    }
                    else
                    {
                        // 5.T1 fill ogThreshold color into gridMask before thresh
                        var bkColor = _isDarkBkGnd ? new Scalar(ogThreshold - 1) : new Scalar(ogThreshold + 1);
                        imgWork.SetTo(bkColor, gridMask);
                        _DUMP(imgWork, "imgWork_5_bk.png");

                        // 5.T2 Threshold
                        Cv2.Threshold(imgWork, imgWork, ogThreshold, 255, ThresholdTypes.Binary);
                        _DUMP(imgWork, "imgWork_5_t.png");
                    }

                    // 6. 反向, 讓 blob 變白色
                    if (!_isDarkBkGnd)
                    {
                        Cv2.BitwiseNot(imgWork, imgWork);
                        _DUMP(imgWork, "imgWork_6_inv.png");
                    }

                    // 7. 除邊
                    if (false)
                    {
                        Cv2.Rectangle(imgWork, new Rect(0, 0, imgWork.Width, imgWork.Height), Scalar.White, 1);
                        Cv2.FloodFill(imgWork, new CvPoint(0, 0), Scalar.Black);
                        _DUMP(imgWork, "imgWork_8_fborder.png");
                    }

                    // 8. 除去 與 gridMask 相接觸的區塊
                    if (false)
                    {
                        var firstBloc = grid[0, 0];
                        if (firstBloc != null)
                        {
                            var seedPt = new CvPoint(firstBloc.CenterX / _shrinkFactor, firstBloc.CenterY / _shrinkFactor);
                            using (Mat whiteCover = new Mat())
                            {
                                Cv2.Dilate(gridMask, whiteCover, null, iterations: 2);
                                Cv2.BitwiseOr(whiteCover, imgWork, imgWork);
                                Cv2.FloodFill(imgWork, seedPt, Scalar.Black);
                            }
                        }
                        _DUMP(imgWork, "imgWork_5.png");
                    }

                    // 9. 除圓孔 (變異太大, 保留)
                    if (false)
                    {
                        exclude_circles(imgWork, Scalar.Black, 80, 100);
                        _DUMP(imgWork, "imgWork_omwbc.png");
                    }

                    // 10. CC blocs
                    int goldenSize = Math.Min(_suckerGoldenBmp.Width, _suckerGoldenBmp.Height);
                    int minSizeW = Math.Max(_recipe.VisionSettings.OutGridBlocMinSize.Value, goldenSize / 8);
                    var ngBlocs = find_ng_blocs(imgWork, minSizeW);

                    // 11. 還原 gridBox2D
                    QvBox2D gridBox2D = new QvBox2D();
                    gridBox2D.SetBox(minAreaRect);
                    var sz = gridBox2D.MinAreaRectSize;
                    sz.Width += 2;
                    sz.Height += 2;
                    gridBox2D.MinAreaRectSize = sz;
                    if (_shrinkFactor > 1)
                    {
                        var corners = Array.ConvertAll(gridBox2D.Corners, c => new PointF(c.X * _shrinkFactor, c.Y * _shrinkFactor));
                        gridBox2D.Corners = corners;
                    }

                    // 12. 除去 與 gridBox2D 相接觸 的 小區塊
                    remove_slim_out_grid_blocs(ngBlocs, gridBox2D);
                    return ngBlocs;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region CLEAN_UP
                foreach (var obj in garbagesCan)
                    obj?.Dispose();
                #endregion
            }
        }

        #region POST_PROCESS_FOR_OUT_GRID_BLOCS
        void remove_slim_out_grid_blocs(List<EzBloc> outGridNgBlocs, EzBlocsGrid grid)
        {
            if (outGridNgBlocs == null || grid == null)
                return;

            int dilate = 8 * _shrinkFactor;

            // 剔除 小的 off-grid ng blocs 且與 on-grid ng blocs 邊緣 相交
            outGridNgBlocs.RemoveAll((ng) =>
            {
                if (ng == null)
                    return true;

                var dilateRect = ng.Rect;
                dilateRect.Inflate(dilate, dilate);

                // 掃描 grid 邊緣 (for each row)
                for (int row = grid.RowMin; row < grid.RowMax; row++)
                {
                    var cols = new int[] { grid.ColMin, grid.ColMax - 1 };
                    foreach (var col in cols)
                    {
                        var onGridBloc = grid.Get(row, col);
                        bool isNG = EzEmptyTrayResult.IsSolidNG(onGridBloc);

                        if (isNG)
                        {
                            if (dilateRect.IntersectsWith(onGridBloc.Rect))
                            {
                                if (ng.Rect.Width < onGridBloc.Rect.Width / 4)
                                    return true;
                                if (onGridBloc.Rect.Contains(ng.CenterX, ng.CenterY))
                                    return true;
                            }
                        }
                    }
                }

                // 掃描 grid 邊緣 (for each col)
                for (int col = grid.ColMin; col < grid.ColMax; col++)
                {
                    var rows = new int[] { grid.RowMin, grid.RowMax - 1 };
                    foreach (var row in rows)
                    {
                        var onGridBloc = grid.Get(row, col);
                        bool isNG = EzEmptyTrayResult.IsSolidNG(onGridBloc);

                        if (isNG)
                        {
                            if (dilateRect.IntersectsWith(onGridBloc.Rect))
                            {
                                if (ng.Rect.Height < onGridBloc.Rect.Height / 4)
                                    return true;
                                if (onGridBloc.Rect.Contains(ng.CenterX, ng.CenterY))
                                    return true;
                            }
                        }
                    }
                }
                return false;
            });
        }
        void remove_slim_out_grid_blocs(List<EzBloc> outGridNgBlocs, QvBox2D gridBox2D)
        {
            if (outGridNgBlocs == null || gridBox2D == null)
                return;

            var gridCorrners = Array.ConvertAll(gridBox2D.Corners, c => new Point2f(c.X, c.Y));
            var minSize = _recipe.VisionSettings.OutGridBlocMinSize.Value;

            // 剔除 小的 off-grid ng blocs 且與 on-grid ng blocs 邊緣 相交
            outGridNgBlocs.RemoveAll((ng) =>
            {
                if (ng == null)
                    return true;

                //(0) 太細長
                if (ng.Rect.Width < minSize || ng.Rect.Height < minSize)
                    return true;

                //(1) 中心在 gridBox2D 內
                if (Cv2.PointPolygonTest(gridCorrners, new Point2f((float)ng.Center.X, (float)ng.Center.Y), false) >= 0)
                    return true;

                ////(2) 獲取 ng bloc 的四個頂點
                //Point2f[] corners = new Point2f[]
                //{
                //    new Point2f(ng.Rect.X, ng.Rect.Y),
                //    new Point2f(ng.Rect.Right, ng.Rect.Y),
                //    new Point2f(ng.Rect.Right, ng.Rect.Bottom),
                //    new Point2f(ng.Rect.X, ng.Rect.Bottom)
                //};

                ////(2) Cv2.IntersectConvexConvex(多邊形1, 多邊形2, 交集結果)
                //var area = Cv2.IntersectConvexConvex(gridCorrners, corners, out var _);
                //if (area > 0)
                //{
                //}

                return false;
            });
        }
        #endregion

        #region PRIVATE_AOI_FUNCTIONS
        void fill_grid_blocs(Mat img, IEnumerable<EzBloc> blocs, Scalar color, int shrinkFactor)
        {
            foreach(var b in blocs)
            {
                if (b != null)
                {
                    var rcv = JetEazy.Qcvt.CV(b.Rect);
                    rcv.X /= shrinkFactor;
                    rcv.Y /= shrinkFactor;
                    rcv.Width /= shrinkFactor;
                    rcv.Height /= shrinkFactor;
                    img.Rectangle(rcv, color, -1);
                }
            }
        }
        bool find_major_minAreaRect(Mat thresholdedImage, out RotatedRect minAreaRect)
        {
            // 1. 尋找所有輪廓 (thresholdedImage 內容會被修改)
            // `contours` 會儲存找到的所有輪廓點集
            CvPoint[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(thresholdedImage, out contours, out hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            //// 2. 找出面積最大的輪廓
            //double maxArea = 0;
            //int maxAreaContourIndex = -1;

            //for (int i = 0; i < contours.Length; i++)
            //{
            //    double area = Cv2.ContourArea(contours[i]);
            //    if (area > maxArea)
            //    {
            //        maxArea = area;
            //        maxAreaContourIndex = i;
            //    }
            //}

            //// 5. 如果找到了面積最大的輪廓，則計算其 MinAreaRect
            //if (maxAreaContourIndex != -1)
            //{
            //    // Cv2.MinAreaRect 會返回一個 RotatedRect 物件
            //    minAreaRect = Cv2.MinAreaRect(contours[maxAreaContourIndex]);

            //    //// 現在您可以取得 minAreaRect 的各項屬性
            //    //Console.WriteLine($"最大輪廓面積：{maxArea}");
            //    //Console.WriteLine($"MinAreaRect 中心點：({minAreaRect.Center.X}, {minAreaRect.Center.Y})");
            //    //Console.WriteLine($"MinAreaRect 尺寸：({minAreaRect.Size.Width}, {minAreaRect.Size.Height})");
            //    //Console.WriteLine($"MinAreaRect 角度：{minAreaRect.Angle}");

            //    //// (選用) 在原始影像上繪製最小外接矩形，以供視覺化
            //    //using Mat displayImage = matA.CvtColor(ColorConversionCodes.GrayToBgr); // 轉換為 BGR 才能畫彩色
            //    //Point2f[] rectPoints = minAreaRect.Points();
            //    //for (int i = 0; i < 4; i++)
            //    //{
            //    //    Cv2.Line(displayImage, (Point)rectPoints[i], (Point)rectPoints[(i + 1) % 4], Scalar.Red, 2);
            //    //}

            //    //// 顯示結果
            //    //Cv2.ImShow("結果", displayImage);
            //    //Cv2.WaitKey(0);

            //    //// 3. 在新的 Mat 上填滿白色多邊形
            //    //Point2f[] rectPoints = minAreaRect.Points();
            //    //Cv2.FillConvexPoly(whiteFilledRect, rectPointsInt, Scalar.White);
            //    return true;
            //}
            //else
            //{
            //    //Console.WriteLine("沒有找到任何輪廓。");
            //    minAreaRect = new RotatedRect();
            //    return false;
            //}

            // 3 找出 minAreaRect 把 thresholdedImage 內 所有 while pixels 都包起來
            var allPoints = new List<CvPoint>();
            foreach (var contour in contours)
            {
                allPoints.AddRange(contour);
            }

            // 4 Check if any points were found
            if (allPoints.Count > 0)
            {
                // Calculate the minAreaRect for all the combined points.
                minAreaRect = Cv2.MinAreaRect(allPoints);

                //// Now, minAreaRect contains the single bounding box for ALL white pixels.
                //Console.WriteLine($"MinAreaRect Center: ({minAreaRect.Center.X}, {minAreaRect.Center.Y})");
                //Console.WriteLine($"MinAreaRect Size: ({minAreaRect.Size.Width}, {minAreaRect.Size.Height})");
                //Console.WriteLine($"MinAreaRect Angle: {minAreaRect.Angle}");
                return true;
            }
            else
            {
                //Console.WriteLine("No white pixels found, so no minAreaRect could be calculated.");
                minAreaRect = new RotatedRect();
                return false;
            }
        }
        void fill_minAreaRect(Mat dst, ref RotatedRect minAreaRect, Scalar color)
        {
            Point2f[] rectPointsFloat = minAreaRect.Points();
            CvPoint[] rectPointsInt = Array.ConvertAll(rectPointsFloat, Point2f => (CvPoint)Point2f);
            Cv2.FillConvexPoly(dst, rectPointsInt, color);
        }
        List<EzBloc> find_ng_blocs(Mat binary, int minLenInWorld = 8, int minAreaInShrink = 16)
        {
            var blocs = new List<EzBloc>();

            var cc = Cv2.ConnectedComponentsEx(binary);
            if (cc.LabelCount <= 1)
                return blocs;

            _DUMP(binary, "cc_binary.png");
            _DUMP_CC_BLOBs(cc);

            var zoomFactor = Math.Max(1, _shrinkFactor);
            var maxWidth = _fovWidth;
            var maxHeight = _fovHeight;

            // 忽略標籤為 0 的背景
            for (int i = 1; i < cc.LabelCount; i++)
            {
                var blob = cc.Blobs[i];

                int bX = (int)(blob.Rect.X * zoomFactor);
                int bY = (int)(blob.Rect.Y * zoomFactor);
                int bW = (int)(blob.Rect.Width * zoomFactor);
                int bH = (int)(blob.Rect.Height * zoomFactor);
                
                if (blob.Area < minAreaInShrink)
                    continue;

                if (bW < minLenInWorld && bH < minLenInWorld)
                    continue;

                if (bW > maxWidth - 2 || bH > maxHeight - 2)
                    continue;

                var worldLoc = new EzBloc(new Rectangle(bX, bY, bW, bH), NG_SCORE, NG_TAG);
                blocs.Add(worldLoc);
            }

            return blocs;
        }
        #endregion

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
                if (image == null || !_isDumpEnabled)
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


    partial class EzOutGridNgBlocsDetector
    {
        double MIN_CIRCULARITY_THRESHOLD = 0.85; // 設定一個閾值，圓度大於此值的視為接近圓形
        void exclude_circles(Mat binary, Scalar fillColor, int dmin, int dmax, int minArea = 25, bool dump = false)
        {
            // 用來儲存符合條件的輪廓
            List<CvPoint[]> filteredContours = new List<CvPoint[]>();

            // 1. 尋找所有輪廓。**必須複製影像，因為 FindContours 會修改它**。
            using (Mat binaryCopy = binary.Clone())
            {
                CvPoint[][] contours;
                HierarchyIndex[] hierarchy;
                Cv2.FindContours(binaryCopy, out contours, out hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

                // 2. 遍歷每個輪廓，計算圓度並進行篩選
                for (int i = 0; i < contours.Length; i++)
                {

                    double area = Cv2.ContourArea(contours[i]);
                    if (area > minArea) // 排除太小的輪廓
                    {
                        // 1. 計算最小外接圓
                        float radius;
                        Point2f center;
                        Cv2.MinEnclosingCircle(contours[i], out center, out radius);

                        double diameter = radius * 2;

                        // 2. 計算圓度
                        double perimeter = Cv2.ArcLength(contours[i], true);
                        double circularity = (perimeter > 0) ? (4 * Math.PI * area) / (perimeter * perimeter) : 0;

                        // 3. 檢查直徑和圓度是否符合條件
                        if (diameter >= dmin &&
                            diameter <= dmax &&
                            circularity >= MIN_CIRCULARITY_THRESHOLD)
                        {
                            //Console.WriteLine($"輪廓 {i}: 面積={area}, 直徑={diameter:F2}, 圓度={circularity:F3}, 符合條件！");
                            filteredContours.Add(contours[i]);

                            // 在新的 Mat 上，將符合條件的圓形填滿 fillColor
                            Cv2.Circle(binary, (CvPoint)center, (int)radius, fillColor, -1);
                        }
                        else
                        {
                            //Console.WriteLine($"輪廓 {i}: 面積={area}, 直徑={diameter:F2}, 圓度={circularity:F3}, 不符合條件。");
                        }
                    }
                }
            }

            if (dump)
            {
                using (Mat displayImage = binary.CvtColor(ColorConversionCodes.GRAY2BGR))
                {
                    // 繪製篩選後的輪廓
                    Cv2.DrawContours(displayImage, filteredContours.ToArray(), -1, Scalar.Red, 2);
                    _DUMP(displayImage, "circles.png");
                    //Cv2.ImShow("Filtered Blobs (Non-Circular)", displayImage);
                    //Cv2.WaitKey(0);
                }
            }
        }
        void find_circle_arc(Mat binary)
        {
            using (Mat imgWork = binary.Clone())
            {
                CvPoint[][] contours;
                HierarchyIndex[] hierarchy;
                Cv2.FindContours(imgWork, out contours, out hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

                // 用來儲存符合條件 (非凸形) 的輪廓
                var filteredContours = new List<CvPoint[]>();

                // 3. 遍歷每個輪廓，並使用 IsContourConvex 進行篩選
                for (int i = 0; i < contours.Length; i++)
                {
                    double area = Cv2.ContourArea(contours[i]);
                    if (area > 50) // 排除太小的輪廓
                    {
                        // Cv2.IsContourConvex 會判斷輪廓是否為凸多邊形
                        bool isConvex = Cv2.IsContourConvex(contours[i]);

                        // 如果輪廓不是凸形 (即 isConvex == false)，則保留它。
                        // 這是為了排除完整的圓形，因為完整的圓形是凸形。
                        if (!isConvex)
                        {
                            filteredContours.Add(contours[i]);
                            Console.WriteLine($"輪廓 {i} 是非凸形，已保留。面積: {area}");
                        }
                        else
                        {
                            Console.WriteLine($"輪廓 {i} 是凸形，已排除。面積: {area}");
                        }
                    }
                }
            }
        }
    }
}
