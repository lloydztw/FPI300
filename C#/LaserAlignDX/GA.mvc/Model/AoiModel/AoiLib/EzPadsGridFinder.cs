#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-16 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Match;
using JetEazy.QxCollections;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using CvSize = OpenCvSharp.Size;
using EzAoiBase = EzAoiEmptyTrayInspector.Model.Aoi.EzAoiBase;

namespace LeTian.AoiLib
{
    public class EzPadsGridFinder : EzAoiBase
    {
        public static bool VISUAL_DEBUG = false;

        #region PRIVATE_DATA
        List<EzBloc> _blocs;
        #endregion

        #region BLOB_FILTER_PARAMETERS
        bool _optFillOffBorder = true;
        bool _optUseInnerFilter = true;
        #endregion

        public EzPadsGridFinder(int shrink = 1)
        {
            _shrinkFactor = Math.Max(shrink, 1);
        }

        /// <summary>
        /// Runtime Parameter
        /// </summary>
        public CvSize GoldenPadSize
        {
            get; set;
        }

        /// <summary>
        /// Runtime Parameter
        /// </summary>
        public int PadThreshold
        {
            get; set;
        }

        /// <summary>
        /// 用於 DEBUG 
        /// </summary>
        public void FindBlocs(Mat img, out List<EzBloc> blocs, string dumpFile = null)
        {
            using (Mat binary = new Mat())
            {
                if (_optUseInnerFilter)
                {
                    findWhiteKeyPoints(img, out blocs, useInnerFilter: true, imgDebugOutput: binary);
                    _DUMP_VISUAL(binary, null, false, Scalar.Black, "Binary");
                }
                else
                {
                    applyPadsFilter(img, binary);
                    findWhiteKeyPoints(img, out blocs, useInnerFilter: false);
                    _DUMP_VISUAL(binary, null, false, Scalar.Black, "Binary");
                }
            }
            _blocs = blocs;
        }
        public void FindPadsGrid(Mat img, out EzBlocsGrid grid, out int keyRow, out int keyCol, out double keySQRatio)
        {
            keySQRatio = 1;
            keyRow = -1;
            keyCol = -1;

            if (_optUseInnerFilter)
            {
                findWhiteRigidGrid(img, out grid, useInnerFilter: true);
                FindSpecialKeyPad(img, grid, out keyRow, out keyCol, out keySQRatio);
            }
            else
            {
                using (Mat binary = new Mat())
                {
                    applyPadsFilter(img, binary);
                    findWhiteRigidGrid(binary, out grid, useInnerFilter: false);
                    FindSpecialKeyPad(img, grid, out keyRow, out keyCol, out keySQRatio);
                }
            }
        }
        public void RebuildPadsGrid(Mat img, double angle, out EzBlocsGrid grid, out int keyRow, out int keyCol, out double keySQRatio)
        {
            if (_blocs == null)
            {
                grid = null;
                keyRow = -1;
                keyCol = -1;
                keySQRatio = 1;
            }
            else
            {
                rebuild(_blocs, out grid, angle, img);
                FindSpecialKeyPad(img, grid, out keyRow, out keyCol, out keySQRatio);
            }
        }
        public List<EzBloc> GetBlocsPool()
        {
            return _blocs;
        }

        #region PRIVATE_BLOB_FUNCTIONS
        void getBlobFilterMinMaxSize(Mat img, out int min_w, out int min_h, out int max_w, out int max_h)
        {
            var gSize = GoldenPadSize;

            if (_shrinkFactor > 1)
            {
                gSize.Width /= _shrinkFactor;
                gSize.Height /= _shrinkFactor;
            }

            if (gSize == CvSize.Zero)
            {
                max_w = img.Width / 4;
                max_h = img.Height / 4;
                min_w = img.Width / 100;
                min_h = img.Height / 100;
                min_w = Math.Max(min_w, 3);
                min_h = Math.Max(min_h, 3);
            }
            else
            {
                max_w = (int)(gSize.Width * 1.5);
                max_h = (int)(gSize.Height * 1.5);
                min_w = (int)(gSize.Width * 0.6);
                min_h = (int)(gSize.Height * 0.6);
                max_w = Math.Min(max_w, img.Width / 4);
                max_h = Math.Min(max_h, img.Height / 4);
                min_w = Math.Max(min_w, 3);
                min_h = Math.Max(min_h, 3);
            }
        }
        void findWhiteRigidGrid(Mat img, out EzBlocsGrid gridPoints, bool useInnerFilter = false)
        {
            findWhiteKeyPoints(img, out _blocs, useInnerFilter: useInnerFilter);

            //// 以中心點排序
            if (false)
            {
                int cx = img.Width / 2;
                int cy = img.Height / 2;
                _blocs?.Sort((b1, b2) =>
                {
                    int dx1 = b1.CenterX - cx;
                    int dy1 = b1.CenterY - cy;
                    int dd1 = dx1 * dx1 + dy1 * dy1;
                    int dx2 = b2.CenterX - cx;
                    int dy2 = b2.CenterY - cy;
                    int dd2 = dx2 * dx2 + dy2 * dy2;
                    return dd1 - dd2;
                });
            }

            var builder = new EzBlocsGridBuilder();
            gridPoints = builder.Build(_blocs);

            if (gridPoints != null)
            {
                gridPoints.RowMin = 0;
                gridPoints.ColMin = 0;
            }
        }
        void findWhiteKeyPoints(Mat img, out List<EzBloc> keyBlocs, bool useInnerFilter = false, Mat imgDebugOutput = null)
        {
            var gc = new List<IDisposable>();

            int min_w, min_h;
            int max_w, max_h;
            getBlobFilterMinMaxSize(img, out min_w, out min_h, out max_w, out max_h);

            // SHRINK
            apply_shrink(img, out img, gc);

            keyBlocs = new List<EzBloc>();
            
            Mat whiteBlobsBinary = img;
            if (useInnerFilter)
            {
                whiteBlobsBinary = new Mat();
                applyPadsFilter(img, whiteBlobsBinary);
                gc.Add(whiteBlobsBinary);
            }

            if (true)
            {
                if (_shrinkFactor < 8)
                {
                    Cv2.Dilate(whiteBlobsBinary, whiteBlobsBinary, null);
                    Cv2.Erode(whiteBlobsBinary, whiteBlobsBinary, null);

                    if (_optFillOffBorder)
                    {
                        var rect = new Rect(0, 0, whiteBlobsBinary.Width, whiteBlobsBinary.Height);
                        whiteBlobsBinary.Rectangle(rect, Scalar.White);
                        Cv2.FloodFill(whiteBlobsBinary, new Point(0, 0), Scalar.Black);
                    }
                }

                if (imgDebugOutput != null)
                {
                    //whiteBlobsBinary.SaveImage(dumpFile);
                    whiteBlobsBinary.CopyTo(imgDebugOutput);
                }

                var cc = Cv2.ConnectedComponentsEx(whiteBlobsBinary);
                for (int i = 1; i < cc.Blobs.Count; i++)
                {
                    var ccBlob = cc.Blobs[i];

                    if (ccBlob.Width < min_w || ccBlob.Height < min_h ||
                        ccBlob.Width > max_w || ccBlob.Height > max_h)
                        continue;

                    //var kp = new KeyPoint((float)ccBlob.Centroid.X, (float)ccBlob.Centroid.Y, orb_keypoint_size);
                    //pts.Add(kp);

                    var rect = JetEazy.Qcvt.CC(ccBlob.Rect);

                    // UNSHRINK
                    if (_shrinkFactor > 1)
                    {
                        rect.X *= _shrinkFactor;
                        rect.Y *= _shrinkFactor;
                        rect.Width *= _shrinkFactor;
                        rect.Height *= _shrinkFactor;
                    }

                    var bloc = new EzBloc(rect, 0);
                    bloc.Pixels = ccBlob.Area;
                    keyBlocs.Add(bloc);
                }
            }

            #region CLEAN_UP
            foreach (var obj in gc)
                obj?.Dispose();
            #endregion
        }
        void applyPadsFilter(Mat img, Mat imgOut)
        {
            // 簡單使用 OTSU (容易受 極端點 干擾)
            //Cv2.AdaptiveThreshold(img, whiteBlobsBinary, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 51, 0);
            //Cv2.Threshold(img, whiteBlobsBinary, 0, 255, ThresholdTypes.Otsu);
            if (PadThreshold <= 0)
                Cv2.Threshold(img, imgOut, 0, 255, ThresholdTypes.Otsu);
            else
                Cv2.Threshold(img, imgOut, 200, 255, ThresholdTypes.Binary);
        }
        #endregion

        #region PRIVATE_REBUILD_FUNCTIONS
        void rebuild(IList<EzBloc> blocs, out EzBlocsGrid grid2, double angle, Mat imgDebug)
        {
            if (blocs == null)
            {
                grid2 = null;
                return;
            }

            _DUMP_VISUAL(imgDebug, blocs, true, Scalar.Pink, "Old BLOCs {0:0}", angle);

            var center = imgDebug != null ?
                            new Point2f(imgDebug.Width / 2, imgDebug.Height / 2) :
                            new Point2f(0, 0);

            var blocsR = Clone(blocs);
            rotateBlocs(blocsR, angle, center, null, true);

            _DUMP_VISUAL(imgDebug, blocsR, true, Scalar.Pink, "Rotated BLOCs {0:0}", angle);

            var builder = new EzBlocsGridBuilder();
            grid2 = builder.Build(blocsR);
         
            if (grid2 != null)
                rotateBlocs(grid2.IterBlocs(), -angle, center, grid2);
        }
        void rotateBlocs(IEnumerable<EzBloc> blocs, double angle, Point2f center, object owner = null, bool resetTag = false)
        {
            double radians = Math.PI * angle / 180;
            double cosTheta = Math.Cos(radians);
            double sinTheta = Math.Sin(radians);

            foreach (var b in blocs)
            {
                if (b == null) continue;
                double dx = b.Center.X - center.X;
                double dy = b.Center.Y - center.Y;
                double rotX = dx * cosTheta - dy * sinTheta;
                double rotY = dx * sinTheta + dy * cosTheta;
                rotX = rotX + center.X;
                rotY = rotY + center.Y;
                JetEazy.Qcvt.SetCenter(ref b.Rect, (int)rotX, (int)rotY);
                b.Center.X = rotX;
                b.Center.Y = rotY;
                b.Owner = owner;
                if (resetTag)
                    b.Tag = null;
            }
        }
        void getCentroid(IEnumerable<EzBloc> blocs, out Point2f center)
        {
            center = new Point2f(0, 0);
            int count = 0;
            foreach (var b in blocs)
            {
                if (b == null) continue;
                center.X += b.CenterX;
                center.Y += b.CenterY;
                count++;
            }
            if (count > 0)
            {
                center.X /= count;
                center.Y /= count;
            }
        }
        IList<EzBloc> Clone(IList<EzBloc> blocs)
        {
            var clone = new List<EzBloc>();
            foreach (var b in blocs)
            {
                if (b == null) continue;
                clone.Add(b.Clone());
            }
            return clone;
        }
        #endregion

        internal void FindSpecialKeyPad(Mat image, IxGridMap<EzBloc> grid, out int keyRow, out int keyCol, out double keySQRatio)
        {
            keyRow = -1;
            keyCol = -1;
            keySQRatio = 1;

            if (grid == null)
                return;

            grid.RowMin = 0;
            grid.ColMin = 0;

            // 暫時固定只找右下角 !!!
            //int[] rowss = new int[] { grid.Rows - 1, 0 };
            //int[] colss = new int[] { grid.Cols - 1, 0 };
            int[] rowss = new int[] { grid.Rows - 1 };
            int[] colss = new int[] { grid.Cols - 1 };

            var boundary = new Rect(0, 0, image.Width, image.Height);
            var bestRatio = double.MaxValue;

            foreach (int r in rowss)
            {
                foreach (int c in colss)
                {
                    var bloc = grid.Get(r, c);
                    if (bloc == null) continue;
                    if (!bloc.IsMajorNode()) continue;

                    var roi = JetEazy.Qcvt.CV(bloc.Rect);
                    roi.Inflate(2, 2);

                    bool is_clipped = JetEazy.Qcvt.ClipBoundary(ref roi, ref boundary);

                    var ratio = is_clipped ? 1 : calcSQRatio(image, ref roi, true);
                    if (ratio < bestRatio)
                    {
                        keySQRatio = bestRatio = ratio;
                        keyRow = r;
                        keyCol = c;
                    }
                }
            }
        }

        /// <summary>
        /// 計算矩形度(缺角)程度
        /// </summary>
        double calcSQRatio(Mat image, ref Rect roi, bool useFilter = true)
        {
            using (Mat binary = new Mat())
            {
                if (useFilter)
                    applyPadsFilter(image, binary);
                else
                    image.CopyTo(binary);

                var padImage = binary[roi];

                int whitePixels = padImage.CountNonZero();

                int area = roi.Width * roi.Height;

                if (findMinAreaRect(padImage, area / 8, out RotatedRect rotRect))
                {
                    area = (int)(rotRect.Size.Width * rotRect.Size.Height);
                }

                return whitePixels / (area + 0.001);

                //var minArea = roi.Width * roi.Height / 4;
                //return checkRectangleScore(image, minArea);
            }
        }

        /// <summary>
        /// 在二值化影像中，尋找面積最大的白色 blob，並計算其最小外接旋轉矩形。
        /// </summary>
        static bool findMinAreaRect(Mat binaryImage, int minAreaThres, out RotatedRect rect)
        {
            // 預設將 rect 設為 null 或 default，以防沒有找到輪廓
            rect = default(RotatedRect);

            // 1. 尋找所有輪廓
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(binaryImage, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            // 如果沒有找到任何輪廓，則直接返回
            if (contours.Length == 0)
            {
                return false;
            }

            // 2. 搜尋最大的白色 blob
            // OrderByDescending 會根據面積由大到小排序，然後 First() 取得第一個元素
            Point[] largestContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();

            // 檢查最大的輪廓是否足夠大，以排除雜訊。
            // 這個閾值可以根據您的應用場景進行調整。
            if (Cv2.ContourArea(largestContour) < minAreaThres)
            {
                return false;  // 面積太小，視為無效 blob
            }

            // 3. 為此白色 blob 找出 minAreaRect
            // Cv2.MinAreaRect 需要至少 5 個點才能計算
            if (largestContour.Length >= 4)
            {
                rect = Cv2.MinAreaRect(largestContour);
                return true;
            }

            return false;
        }

        #region RESERVED
        static void __FindSpecialKeyPad(IxGridMap<EzBloc> grid, out int keyRow, out int keyCol, out int keyPixels)
        {
            keyRow = -1;
            keyCol = -1;
            keyPixels = 0;

            if (grid == null)
                return;

            grid.RowMin = 0;
            grid.ColMin = 0;
            int[] rowss = new int[] { 0, grid.Rows - 1 };
            int[] colss = new int[] { 0, grid.Cols - 1 };

            var min = int.MaxValue;

            foreach (int r in rowss)
            {
                foreach (int c in colss)
                {
                    var bloc = grid.Get(r, c);
                    if (bloc == null) continue;
                    if (!bloc.IsMajorNode()) continue;

                    //var roi = JetEazy.Qcvt.CV(bloc.Rect);
                    //JetEazy.Qcvt.ClipBoundary(ref roi, ref boundary);
                    //var whiteArea = binary[roi].Sum();
                    //var pixels = (int)whiteArea.Val0;

                    var pixels = bloc.Pixels;

                    if (pixels < min)
                    {
                        min = pixels;
                        keyRow = r;
                        keyCol = c;
                        keyPixels = min;
                    }
                }
            }
        }
        /// <summary>
        /// 評估二值化影像中，面積最大的白色 blob 是 矩形的程度。
        /// </summary>
        static double __checkRectangleScore(Mat binaryImage, int minAreaThres = 0)
        {
            double emptyScore = 1;

            if (minAreaThres <= 0)
                minAreaThres = binaryImage.Width * binaryImage.Height / 8;

            // 尋找所有外部輪廓
            Cv2.FindContours(binaryImage, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
            {
                // 如果沒有找到任何輪廓，返回 1
                return emptyScore;
            }

            // 找到面積最大的輪廓
            Point[] largestContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();

            // 檢查面積最大的輪廓是否足夠大，以排除雜訊
            if (Cv2.ContourArea(largestContour) < minAreaThres)
            {
                // 如果最大的輪廓太小，也視為沒有找到有效的 blob
                return emptyScore;
            }

            // 第一部分：頂點數得分 (Vertex Score)
            double perimeter = Cv2.ArcLength(largestContour, true);
            double epsilon = 0.04 * perimeter;
            Point[] approx = Cv2.ApproxPolyDP(largestContour, epsilon, true);

            double vertexScore = 0.0;
            if (approx.Length == 4)
            {
                vertexScore = 1.0;
            }
            else if (approx.Length > 2)
            {
                vertexScore = 1.0 - Math.Abs(approx.Length - 4) / 4.0;
                if (vertexScore < 0) vertexScore = 0;
            }

            // 第二部分：面積比得分 (Area Ratio Score)
            double contourArea = Cv2.ContourArea(largestContour);
            RotatedRect minAreaRect = Cv2.MinAreaRect(largestContour);
            double rectArea = minAreaRect.Size.Width * minAreaRect.Size.Height;

            double areaRatioScore = 0.0;
            if (rectArea > 0)
            {
                double areaRatio = contourArea / rectArea;
                areaRatioScore = Math.Pow(areaRatio, 2);
            }

            // 第三部分：凸度得分 (Convexity Score)
            double convexityScore = 0.0;
            Point[] convexHull = Cv2.ConvexHull(largestContour);
            double convexHullArea = Cv2.ContourArea(convexHull);

            if (convexHullArea > 0)
            {
                convexityScore = Math.Pow(contourArea / convexHullArea, 2);
            }

            // 綜合分數
            double finalScore = (vertexScore * 0.5) + (areaRatioScore * 0.3) + (convexityScore * 0.2);

            return finalScore;
        }
        #endregion

        #region DEBUG_FUNCTIONS
        void _DUMP_VISUAL(Mat imgDebug, IEnumerable<EzBloc> blocs, bool withRect, Scalar color, string msg, params object[] args)
        {
            if (VISUAL_DEBUG && imgDebug != null)
            {
                string winName = String.Format(msg, args);
                VxDebugDrawer.Draw(imgDebug, blocs, withRect, color, winName);
            }
        }
        #endregion
    }
}
