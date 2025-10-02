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

using EzAoiEmptyTrayInspector.Model;
using JetEazy.EzImage;
using JetEazy.Match;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;


namespace LaserAlignDX.AoiModel
{
    //public interface ICalibAoiModel : IxEmptyTrayInspector
    //{
    //    void RefineCentroidLocations(MatchResult matchResult, Mat fullfovImg);
    //}

    public class CalibAoiModel : ICalibAoiModel
    {
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
        public bool CropGoldenTemplate(SideID sideId, IEzImage largeImg, Rectangle goldenRect)
        {
            return _externImp.CropGoldenTemplate(sideId, largeImg, goldenRect);
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
        public void RefineCentroidLocations(MatchResult matchResult, Mat fullfovImg)
        {
            var grid = matchResult?.Grid;
            if (grid == null || _jxRecipe == null)
                return;

            bool isBlackChip = !_jxRecipe.VisionSettings.Inverse.Value;
            int thresh = _jxRecipe.VisionSettings.OutGridBlocThreshold.Value;

            var bound = new Rect(0, 0, fullfovImg.Width, fullfovImg.Height);
            foreach (var bloc in grid.IterBlocs())
            {
                if (bloc == null) continue;

                var roi = JetEazy.Qcvt.CV(bloc.Rect);
                JetEazy.Qcvt.ClipBoundary(ref roi, ref bound);

                using (var img = fullfovImg[roi].Clone())
                using (var binary = new Mat())
                {
                    if (isBlackChip)
                        Cv2.BitwiseNot(img, img);

                    if (thresh <= 0)
                        Cv2.Threshold(img, binary, 0, 255, ThresholdTypes.Otsu);
                    else
                        Cv2.Threshold(img, binary, thresh, 255, ThresholdTypes.Binary);

                    findWhiteBlobs(binary, out var whiteBlobs);
                    if (whiteBlobs.Count == 0) continue;
                    whiteBlobs.Sort((b1, b2) => b2.Pixels - b1.Pixels);

                    var center = whiteBlobs[0].Center;
                    center.X += roi.X;
                    center.Y += roi.Y;
                    bloc.Center = center;
                    var rc = JetEazy.Qcvt.CreateCenterRect((float)center.X, (float)center.Y, (float)bloc.Rect.Width, (float)bloc.Rect.Height);
                    bloc.Rect = Rectangle.Round(rc);
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
        #endregion
    }
}