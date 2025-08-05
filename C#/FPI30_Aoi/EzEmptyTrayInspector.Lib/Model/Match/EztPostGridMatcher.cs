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


namespace EzAoiEmptyTrayInspector.Model.Aoi
{
    public class EztPostGridMatcher : IDisposable
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
        #endregion

        #region PRIVATE_RUNTIME_DATA
        Mat _largeGoldenGridImage;
        #endregion

        public void Dispose()
        {
            _largeGoldenGridImage?.Dispose();
            _largeGoldenGridImage = null;
        }

        public void SetRecipe(JxAoiRecipe settings)
        {
            _recipe = settings;
        }

        public void Predict(Mat srcImg, EzBlocsGrid matchGrid, bool force = false)
        {
            Exception errEx = null;

            try
            {
                if (_recipe == null || srcImg == null || matchGrid == null)
                    return;

                var fullRows = _recipe.TrayMiscSettings.FullRows;
                var fullCols = _recipe.TrayMiscSettings.FullCols;
                if (!force && matchGrid.Rows >= fullRows && matchGrid.Cols >= fullCols)
                {
                    return;
                }

                var goldenGrid = _recipe.TrayMiscSettings.GoldenGrid;
                if (goldenGrid == null)
                    return;

                using (var largeGG = rebuild_large_golden_grid_image())
                {
                    if (largeGG == null)
                        return;

                    //>>> largeGG.SaveImage("d:\\paso.log\\golden_grid.jpg");

                    var rect = JetEazy.Qcvt.CV(goldenGrid.GetBoundary());
                    largeGG[rect].SaveImage("d:\\paso.log\\golden_grid_crop.jpg");
                }

                //JxTempMatchSettings matchSettings = _recipe.GetMatchSettings((int)sideId);
                //if (srcImg == null || matchSettings == null)
                //    return;

                //var threshold = (double)matchSettings.ScoreThres.Value;
                //var goldenBmp = (Bitmap)matchSettings.GoldenBmp.Value;
                //if (goldenBmp == null)
                //    return;

                //double rotateAngle = 0;
                //var rotSettings = _recipe.GetSiteSettings((int)sideId).RotAngle;
                //if (rotSettings != null && rotSettings.Enabled)
                //{
                //    changeState("MATCH : finding angle", sideId);
                //    var finder = new EzRotAngleFinder();
                //    rotateAngle = finder.ApplyFilters(srcImg, rotSettings, true, out var rotRects);
                //    _LOG.Info("[{0}] MATCH : angle = {1:0.00}°", sideId, rotateAngle);
                //}

                //using (var bridge = new QxImageBridge(goldenBmp))
                //{
                //    var tm0 = DateTime.Now;
                //    var goldenImg = bridge.Image;

                //    var matcher = new EzTemplateMatcher(EzAoiBaseUtil.GetShrinkFactor(srcImg.Width, srcImg.Height));
                //    matcher.SetRecipe(matchSettings);
                //    matcher.DumpPath = dumpPath;
                //    matcher.OnProgress += (s, e) => _LOG.Info("[{0}] {1}", sideId, e.Message);

                //    _LOG.Info("[{0}] Matching", sideId);
                //    var blocs = matcher.FindBlocs(srcImg, goldenImg);

                //    EzBlocsGrid grid = null;
                //    if (matchSettings.UseGrid)
                //    {
                //        //changeState("Gridding", sideId);
                //        _LOG.Info("[{0}] Gridding", sideId);
                //        var builder = new EzBlocsGridBuilder();
                //        grid = builder.Build(blocs);
                //    }

                //    var ts = DateTime.Now - tm0;
                //    double secs = ts.TotalSeconds;
                //    matchResult = new MatchResult((int)sideId, grid, blocs: blocs, totalSeconds: secs);
                //    matchResult.RotateAngle = rotateAngle;
                //}
            }
            catch (Exception ex)
            {
                errEx = ex;
            }
            finally
            {
                if (errEx != null)
                {
                    _ERROR(ErrCodes.MATCH_ERROR, sideId, errEx);
                }
                else
                {
                    changeState("Ready", sideId);
                    update_one_match_result(sideId, matchResult, notify: true);
                }
            }
        }

        Mat rebuild_large_golden_grid_image(bool useBlackWhite = false, string dumpPath = null)
        {
            if (_recipe == null)
                return null;

            var goldenGrid = _recipe.TrayMiscSettings.GoldenGrid;
            if (goldenGrid == null)
                return null;

            var W = _recipe.TrayMiscSettings.FovWidth;
            var H = _recipe.TrayMiscSettings.FovHeight;
            if (W < 10 || H < 10)
                return null;

            var largeGG = new Mat(H, W, MatType.CV_8UC1);
            largeGG.SetTo(Scalar.White);

            var goldenBmp = _recipe.VisionSettings.Match.GoldenBmp.Value as Bitmap;
            if (goldenBmp == null || useBlackWhite)
            {
                foreach (var bloc in goldenGrid.IterBlocs())
                {
                    if (bloc != null)
                    {
                        var rc = JetEazy.Qcvt.CV(bloc.Rect);
                        rc.Inflate(-8, -8);
                        largeGG.Rectangle(rc, Scalar.Black, -1);
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

            return largeGG;
        }

        void _DUMP(Mat img, string dumpPath, string fileName)
        {

        }
    }
}
