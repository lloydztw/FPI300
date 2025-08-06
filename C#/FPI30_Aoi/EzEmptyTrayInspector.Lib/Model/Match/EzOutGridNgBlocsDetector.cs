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
using CvPoint = OpenCvSharp.Point;


namespace EzAoiEmptyTrayInspector.Model.Aoi
{
    /// <summary>
    /// 吸嘴格點 之外 異常餘料 偵測
    /// </summary>
    public class EzOutGridNgBlocsDetector
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
        Bitmap _suckerGoldenBmp => _recipe.VisionSettings.Match.GoldenBmp;
        //JxTempMatchSettings _recipeM;
        //bool _usingBlur = true;
        //bool _usingLocMax = true;
        //int _iterations => _recipeM != null ? Math.Max(_recipeM.Iterations.Value, 1) : 1;
        //double _threshold => _recipeM!=null ? (double)_recipeM.ScoreThres.Value : 0.75;
        //double _thresholdLow => _recipeM != null ? (double)_recipeM.ScoreThresLow.Value : 0.75;
        //bool _removeOverlap => _recipeM != null ? _recipeM.RemoveOverlap.Value : false;
        int _fovWidth;
        int _fovHeight;
        //int _goldenWidth;
        //int _goldenHeight;
        #endregion

        /// <summary>
        /// 建構式 (shrinkFactor > 1 可以縮小圖, 來加速計算)
        /// </summary>
        public EzOutGridNgBlocsDetector(int shrinkFactor = 1)
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

        public List<EzBloc> FindNgBlocs(Mat srcImg, EzBlocsGrid grid)
        {
            if (srcImg == null)
                return null;

            var garbagesCan = new List<Mat>();
            _fovWidth = srcImg.Width;
            _fovHeight = srcImg.Height;

            try
            {
                using (var imgWork = new Mat())
                using (var bridgeSG = new QxImageBridge(_suckerGoldenBmp))
                {
                    var suckerGoldenImg = bridgeSG.Image;
                    var meanColor = suckerGoldenImg.Mean();

                    // 0 To Shrink Domaon
                    normalizeGolden(suckerGoldenImg, srcImg, out suckerGoldenImg, garbagesCan);
                    apply_shrink(suckerGoldenImg, srcImg, out suckerGoldenImg, out srcImg, garbagesCan);

                    // 1 gridMask: filled by OnGrid Blocs
                    Mat gridMask = new Mat(srcImg.Size(), srcImg.Type());
                    gridMask.SetTo(Scalar.Black);
                    fill_grid_blocs(gridMask, grid.IterBlocs(), Scalar.White, _shrinkFactor);
                    _DUMP(gridMask, "gridMask_1.png");

                    // 2 gridMask: get minAreaRect
                    find_major_minAreaRect(gridMask, out RotatedRect minAreaRect);
                    _DUMP(gridMask, "gridMask_2.png");

                    // 3 gridMask: fill minAreaRect
                    gridMask.SetTo(Scalar.Black);
                    fill_minAreaRect(gridMask, ref minAreaRect, Scalar.White);
                    _DUMP(gridMask, "gridMask_3.png");

                    // 4 srcImg | gridMask ==> imgWork
                    Cv2.BitwiseOr(srcImg, gridMask, imgWork);
                    _DUMP(imgWork, "imgWork.png");

                    // 5. fill mean before otsu
                    imgWork.SetTo(meanColor, gridMask);
                    _DUMP(imgWork, "imgWork_m.png");

                    // 6. otsu
                    Cv2.Threshold(imgWork, imgWork, 0, 255, ThresholdTypes.Otsu);
                    _DUMP(imgWork, "imgWork_o.png");

                    // 7. mask again
                    Cv2.BitwiseOr(imgWork, gridMask, imgWork);
                    _DUMP(imgWork, "imgWork_om.png");

                    // 8. 反白, blob 變白色
                    Cv2.BitwiseNot(imgWork, imgWork);
                    _DUMP(imgWork, "imgWork_omw.png");

                    // 9. 除邊
                    Cv2.Rectangle(imgWork, new Rect(0, 0, imgWork.Width, imgWork.Height), Scalar.White, 1);
                    Cv2.FloodFill(imgWork, new CvPoint(0, 0), Scalar.Black);
                    _DUMP(imgWork, "imgWork_omwb.png");

                    // 10. CC blocs
                    var ngBlocs = find_black_ng_blocs(imgWork);
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

        #region PRIVATE_HELPER_FUNCTIONS
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
        List<EzBloc> find_black_ng_blocs(Mat binary, int minLen = 3)
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

                int bx = (int)(blob.Rect.X * zoomFactor);
                int by = (int)(blob.Rect.Y * zoomFactor);
                int bw = (int)(blob.Rect.Width * zoomFactor);
                int bh = (int)(blob.Rect.Height * zoomFactor);

                if (bw < minLen || bh < minLen)
                    continue;

                if (bw > maxWidth - 2 || bh > maxHeight - 2)
                    continue;

                var worldLoc = new EzBloc(new Rectangle(bx, by, bw, bh), NG_SCORE, NG_TAG);
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
}
