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
using JetEazy.EzImage;
using JetEazy.Match;
using JetEazy.Utils;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Forms;


namespace LaserAlignDX.AoiModel
{
    //public interface ICalibAoiModel : IxEmptyTrayInspector
    //{
    //    void RefineCentroidLocations(MatchResult matchResult, Mat fullfovImg);
    //}

    public class CalibAoiModel : ICalibAoiModel
    {
        public static bool OPT_DUMP = false;

        #region PRIVATE_DATA
        IxEmptyTrayInspector _externImp;
        JxAoiRecipe _jxRecipe;
        #endregion

        #region WRAPPER_FUNCTIONS_For_IxEmptyTrayInspector
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
        #endregion

        public CalibAoiModel(IxEmptyTrayInspector imp)
        {
            // Caller 負責調用 imp.Dispose()
            _externImp = imp;
        }
        public void Dispose()
        {
            // Caller 負責調用 _externImp.Dispose()
        }
        public void SetRecipe(JxAoiRecipe recipe)
        {
            // recipe 會被 _externImp 持有,
            // 所以 _jxRecipe  不用 Dispose
            _externImp.SetRecipe(recipe);
            _jxRecipe = recipe;
        }
        public bool CropGoldenTemplate(SideID sideId, IEzImage largeImg, Rectangle goldenRect)
        {
            #region 自動精選_goldenRect
            if (false)
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
        public void RefineCentroidLocations(MatchResult matchResult, Mat fullfovImg)
        {
            var grid = matchResult?.Grid;
            if (grid == null || _jxRecipe == null)
                return;

            bool isBlackCarrier = checkIfDarkBackground(fullfovImg);
            if (isBlackCarrier)
            {
                RefineCentroidLocations_BlackCarrier(matchResult, fullfovImg);
            }
            else
            {
                RefineCentroidLocations_WhiteCarrier(matchResult, fullfovImg);
            }
        }
        
        void RefineCentroidLocations_WhiteCarrier(MatchResult matchResult, Mat fullfovImg)
        {
            var grid = matchResult?.Grid;
            if (grid == null || _jxRecipe == null)
                return;

            //bool isBlackCarrier = checkIfDarkBackground(fullfovImg);
            int thresh = _jxRecipe.VisionSettings.OutGridBlocThreshold.Value;

            var bound = new Rect(0, 0, fullfovImg.Width, fullfovImg.Height);
            int rows = grid.Rows;
            int cols = grid.Cols;
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

                        //_DUMP(img, "img", r, c);
                        _DUMP(binary, "binary", r, c);
                        binary.Mean();

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
        }
        void RefineCentroidLocations_BlackCarrier(MatchResult matchResult, Mat fullfovImg)
        {
            var grid = matchResult?.Grid;
            if (grid == null || _jxRecipe == null)
                return;

            //bool isBlackCarrier = checkIfDarkBackground(fullfovImg);
            int thresh = _jxRecipe.VisionSettings.OutGridBlocThreshold.Value;

            var bound = new Rect(0, 0, fullfovImg.Width, fullfovImg.Height);
            int rows = grid.Rows;
            int cols = grid.Cols;
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

                        //_DUMP(img, "img", r, c);
                        _DUMP(binary, "binary", r, c);
                        binary.Mean();

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
        }
        
        #region PRIVATE_FUNCTIONS
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
        #endregion
    }
}