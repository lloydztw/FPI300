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
using OpenCvSharp;
using System;
using System.Collections.Generic;
using CvPoint = OpenCvSharp.Point;


namespace EzAoiChipLocQC.Model.Aoi
{
    /// <summary>
    /// 吸嘴格點 之內 異常餘料 偵測
    /// </summary>
    internal class EzOnGridNgBlocsPredictor : EzAoiBase
    {
        #region RUNTIME_RECIPE_PARAMS
        JxQcRecipe _recipe;
        Mat _largeGoldenGridImage;
        bool _isDarkBkGnd;
        #endregion

        public void SetRecipeParams(JxQcRecipe recipe, Mat lggImage, bool isDarkBkGnd)
        {
            _recipe = recipe;
            _largeGoldenGridImage = lggImage;
            _isDarkBkGnd = isDarkBkGnd;
        }
        public EzBlocsGrid Predict(Mat srcImg, MatchResult matchResult, bool force = false)
        {
            var dumpPath = this.DumpPath;
            var resultGrid = matchResult?.Grid;

            try
            {
                // Null Condition
                if (srcImg == null || _recipe == null || matchResult == null)
                    return resultGrid;

                // FULL rows and cols
                var fullRows = _recipe.TrayDimSettings.FullRows;
                var fullCols = _recipe.TrayDimSettings.FullCols;

                // INPUT GRID 檢查是否已經滿盤定位
                var inputGrid = matchResult.Grid;
                if (inputGrid != null && inputGrid.Rows >= fullRows && inputGrid.Cols >= fullCols && !force)
                    return resultGrid;

                // NOTE: goldenGrid 是由 recipe runtime deSerialize 
                var goldenGrid = _recipe.TrayDimSettings.GetGoldenGrid();
                if (goldenGrid == null)
                    return resultGrid;
                //_LOG.Info($"GoldenGrid = {goldenGrid.Rows}x{goldenGrid.Cols}");

                // OFFSET (暫時使用固定的 0,0)
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
                        var bloc = new EzBloc(rect, score: SCORES.NG_PSEUDO);
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
                        if (b != null && b.Score <= SCORES.NG_PSEUDO)
                            b.Tag = "NG_PSEUDO";
                    }
                }

                // UPDATE to existing matchResult
                //>>> matchResult.Grid = newGrid;

                return newGrid;
            }
            catch (Exception ex)
            {
                _LOG.Error(ex);
                throw ex;
            }
            finally
            {
                //if (errEx != null)
                //{
                //    _ERROR(ErrCodes.ON_GRID_TEMPLATE_MATCH_ERROR, sideId, errEx);
                //}
                //else
                //{
                //    changeState("Ready", sideId);
                //    update_one_match_result(sideId, matchResult, notify: true);
                //}
            }
        }
        
        void find_golden_grid_offset(Mat srcImg, Mat goldenGridImage, EzBlocsGrid goldenGrid, EzBlocsGrid inputGrid, out int offset_x, out int offset_y)
        {
            offset_x = 0; 
            offset_y = 0;

            // 暫時使用固定的 (0,0) offset
            return;

            if (goldenGrid==null || inputGrid==null)
                 return;
            if (goldenGrid.Rows == inputGrid.Rows && goldenGrid.Cols == inputGrid.Cols)
                return;

            // LARGE GOLDEN TEMPLATE
            var lggRect = JetEazy.Qcvt.CV(goldenGrid.GetBoundary());
            var lggCenter = JetEazy.Qcvt.Center(ref lggRect);
            var lggTemplate = goldenGridImage[lggRect];

            // MATCH
            int shrink = EzAoiBaseUtil.GetShrinkFactor(srcImg.Width, srcImg.Height);
            var matcher = new EzTemplateMatcher(shrink);
            var settings = new JxTempMatchSettings();
            settings.ScoreThres.Value = 0.2m;
            matcher.SetRecipe(settings);

            // SEARCHING points
            var bestBloc = matcher.FindBestBloc(srcImg, lggTemplate, iterate_possible_offsets(goldenGrid, inputGrid), externFilter: filter);
            if (bestBloc != null && bestBloc.Score > 0.01)
            {
                var newCenter = bestBloc.Center;
                offset_x = (int)(newCenter.X - lggCenter.X);
                offset_y = (int)(newCenter.Y - lggCenter.Y);
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
            int fovWidth = _recipe.TrayDimSettings.FovWidth;
            int fovHeight = _recipe.TrayDimSettings.FovHeight;
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

        /// <summary>
        /// LETIAN: 2025-09-15 加掛 filter
        /// </summary>
        Mat filter(Mat src)
        {
            Mat output = new Mat();

            int ogThreshold = _recipe.VisionSettings.OutGridBlocThreshold.Value;
            if (ogThreshold > 0)
            {
                Cv2.Threshold(src, output, ogThreshold, 255, ThresholdTypes.Binary);
            }
            else
            {
                double thres = Cv2.Threshold(src, output, 0, 255, ThresholdTypes.Otsu);
                _LOG.Info($"OG_THRESHOLD = {(int)thres}");
            }

            //int dx = Math.Min(8, output.Width);
            //int dy = Math.Min(8, output.Height);
            //var ave = Cv2.Mean(output[0, dy, 0, dx]);
            //bool isBlackGnd = (ave.Val0 < 120);
            if (!_isDarkBkGnd)
                Cv2.BitwiseNot(output, output);     // 維持晶粒為 white blob !!!
            
            Cv2.Dilate(output, output, null, iterations: 2);
            Cv2.Erode(output, output, null, iterations: 2);
            
            Mat tmp = new Mat();
            var sz = new OpenCvSharp.Size(3, 3);
            Cv2.Blur(output, tmp, sz);

            output?.Dispose();
            output = tmp;
            return output;
        }

        #region PRIVATE_HELPER_FUNCTIONS
        void offset(EzBloc bloc, int dx, int dy)
        {
            if (bloc == null)
                return;
            var cc = bloc.Center?.Offset(dx, dy);
            bloc.Rect.Offset(dx, dy);
            bloc.Center = cc;
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
