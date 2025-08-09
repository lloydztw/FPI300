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
    /// 吸嘴格點 之內 異常餘料 偵測
    /// </summary>
    internal class EzOnGridNgBlocsPredictor : EzAoiBase
    {
        #region RECIPE_PARAMS
        JxAoiRecipe _recipe;
        #endregion

        #region RUNTIME_DATA
        static Mat _largeGoldenGridImage;               // 暫時用 static
        static IDisposable _lastLargeGoldenGrid;        // 暫時用 static
        #endregion

        public void SetRecipe(JxAoiRecipe recipe)
        {
            _recipe = recipe;
        }

        public EzBlocsGrid Predict(Mat srcImg, MatchResult matchResult, bool force = false)
        {
            var dumpPath = this.DumpPath;
            var resultGrid = matchResult?.Grid;

            try
            {
                //changeState("POST Grid NG Matching", sideId);

                // Null Condition
                if (srcImg == null || _recipe == null || matchResult == null)
                    return resultGrid;

                // FULL rows and cols
                var fullRows = _recipe.TrayMiscSettings.FullRows;
                var fullCols = _recipe.TrayMiscSettings.FullCols;

                // INPUT GRID 檢查是否已經滿盤定位
                var inputGrid = matchResult.Grid;
                if (inputGrid != null && inputGrid.Rows >= fullRows && inputGrid.Cols >= fullCols && !force)
                    return resultGrid;

                // NOTE: goldenGrid 是由 recipe runtime deSerialize 
                var goldenGrid = _recipe.TrayMiscSettings.GetGoldenGrid();
                if (goldenGrid == null)
                    return resultGrid;
                _LOG.Info($"GoldenGrid = {goldenGrid.Rows}x{goldenGrid.Cols}");

                // LARGE GOLDEN GRID IMAGE (rebuilt from recipe)
                #region FULL_FOV_GOLDEN_GRID_IMAGE
                bool needsToRebuild = _lastLargeGoldenGrid != goldenGrid || _largeGoldenGridImage == null;
                _lastLargeGoldenGrid = goldenGrid;
                if(needsToRebuild)
                {
                    _largeGoldenGridImage?.Dispose();
                    _largeGoldenGridImage = rebuild_golden_grid_image();
                }
                if (_largeGoldenGridImage == null)
                {
                    _LOG.Warn("[AOI] largetGoldenGridImage 無重建!");
                    return resultGrid;
                }
                _DUMP_GOLDEN_GRID_IMAGE(_largeGoldenGridImage, goldenGrid, dumpPath);
                #endregion

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

                return newGrid;
            }
            catch (Exception ex)
            {
                //errEx = ex;
                _LOG.Error(ex);
                throw ex;
            }
        }

        #region PRIVATE_AOI_FUNCTIONS
        void find_golden_grid_offset(Mat srcImg, Mat goldenGridImage, EzBlocsGrid goldenGrid, EzBlocsGrid inputGrid, out int offset_x, out int offset_y)
        {
            // LARGE GOLDEN TEMPLATE
            var ggRect = JetEazy.Qcvt.CV(goldenGrid.GetBoundary());
            var ggCenter = JetEazy.Qcvt.Center(ref ggRect);
            var ggTemplate = goldenGridImage[ggRect];

            // MATCH
            int shrink = EzAoiBaseUtil.GetShrinkFactor(srcImg);
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
        #endregion

        public static void DisposeAll()
        {
            _largeGoldenGridImage?.Dispose();
            _lastLargeGoldenGrid?.Dispose();
        }

        #region FULL_FOV_GOLDEN_IMAGE
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
            _recipe?.TrayMiscSettings.GetGoldenGrid(true);
            _largeGoldenGridImage?.Dispose();
            _largeGoldenGridImage = null;
        }
        #endregion

        #region DUMP_FUNCTIONS
        internal void _DUMP_GOLDEN_GRID_IMAGE(Mat largeGoldenGridImage, EzBlocsGrid goldenGrid, string dumpPath = null)
        {
            if (largeGoldenGridImage == null)
                return;

            if (dumpPath == null)
                return;

            JetEazy.IO.QxPathUtility.InitDirectory(dumpPath);

            largeGoldenGridImage.SaveImage($"{dumpPath}\\large_golden_grid.jpg");

            if (goldenGrid != null)
            {
                var roi = JetEazy.Qcvt.CV(goldenGrid.GetBoundary());
                largeGoldenGridImage[roi].SaveImage($"{dumpPath}\\large_golden_grid_template.jpg");
            }
        }
        #endregion
    }
}
