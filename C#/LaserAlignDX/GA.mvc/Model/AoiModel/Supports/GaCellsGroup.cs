#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector.Model;
using JetEazy.Utils;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using Size = System.Drawing.Size;


namespace LaserAlignDX.AoiModel
{
    /// <summary>
    /// The decorated class of RegionCellX3Class
    /// </summary>
    public class GaCell : IDisposable
    {
        public RegionCellX3Class Cell
        {
            get; private set;
        }
        public Bitmap CellBmp
        {
            get; private set;
        }
        public Rectangle CellRoi
        {
            get; private set;
        }
        public GaCell(RegionCellX3Class cell, Bitmap cellBmp, Rectangle roi)
        {
            Cell = cell;
            CellBmp = cellBmp;
            CellRoi = roi;
        }
        public void Dispose()
        {
            CellBmp?.Dispose();
            CellBmp = null;
        }
    }
    
    public class GaCellsGroup : IEnumerable<GaCell>, IDisposable
    {
        #region PRIVATE_DATA
        List<GaCell> _gaCells;
        Rectangle _fullFovRect;
        #endregion

        #region PROTECTED_CONSTRUCTOR
        protected GaCellsGroup()
        {
        }
        #endregion

        public Rectangle FullFovRect
        {
            get => _fullFovRect;
        }
        public IEnumerator<GaCell> GetEnumerator()
        {
            if (_gaCells != null)
            {
                foreach (var cell in _gaCells)
                    yield return cell;
            }
        }
        public void Dispose()
        {
            if (_gaCells != null)
            {
                foreach (var cell in _gaCells)
                    cell?.Dispose();
            }
        }

        /// <summary>
        /// caller 負責 fullFovBmp 生命週期
        /// </summary>
        public static GaCellsGroup[] CollectGroups(int N, RecipeFPIX3Class xRecipe, Bitmap fullFovBmp, EzEmptyTrayResult preEmptyResult = null, bool outGrid = false)
        {
            if (!outGrid)
                return CollectGroups_on_grid(N, xRecipe, fullFovBmp, preEmptyResult);
            else
                return CollectGroups_out_grid(N, xRecipe, fullFovBmp, preEmptyResult);
        }
        static GaCellsGroup[] CollectGroups_000_spanY(int N, RecipeFPIX3Class xRecipe, Bitmap fullFovBmp)
        {
            float x_min = float.MaxValue;
            float y_min = float.MaxValue;
            float x_max = float.MinValue;
            float y_max = float.MinValue;
            int count = 0;

            foreach (var cell in xRecipe.xRegionCells)
            {
                if (cell == null) continue;
                var rect = cell.viewRectF;
                x_min = Math.Min(x_min, rect.X);
                x_max = Math.Max(x_max, rect.Right);
                y_min = Math.Min(y_min, rect.Y);
                y_max = Math.Max(y_max, rect.Bottom);
                count++;
            }
            if (count == 0)
                return new GaCellsGroup[0];

            float y_delta = (y_max - y_min) / N;
            if( y_delta < 100)
            {
                N = 1;
                y_delta = y_max - y_min;
            }

            var groups = new GaCellsGroup[N];
            for (int i = 0; i < N; i++)
            {
                var y0 = y_min + y_delta * i;
                var y1 = y0 + y_delta;

                var srcCells = new List<RegionCellX3Class>();
                foreach (var cell in xRecipe.xRegionCells)
                {
                    if (cell == null) continue;
                    var rect = cell.viewRectF;
                    var center = JetEazy.Qcvt.CenterF(ref rect);
                    if (y0 <= center.Y && center.Y < y1)
                        srcCells.Add(cell);
                }
                srcCells.Sort((c1, c2) => (int)(c1.viewRectF.Y - c2.viewRectF.Y));

                var inflate = new Size(xRecipe.xExtendx, xRecipe.xExtendy);
                var grp = groups[i] = new GaCellsGroup();
                grp.buildGaCells(fullFovBmp, inflate, srcCells);
            }

            verify(groups, alert: false);

            return groups;
        }
        static GaCellsGroup[] CollectGroups_on_grid(int N, RecipeFPIX3Class xRecipe, Bitmap fullFovBmp, EzEmptyTrayResult preEmptyResult)
        {
            var allSrcCells = new List<RegionCellX3Class>(xRecipe.xRegionCells);
            var emptyCells = new List<RegionCellX3Class>();
            if (preEmptyResult != null)
            {
                allSrcCells.RemoveAll(c =>
                {
                    if (c == null) return true;
                    preEmptyResult.GetBlocByRowCol(c.CellRow, c.CellCol, out var _, out bool isSucker);
                    if (isSucker)
                        emptyCells.Add(c);
                    return isSucker;
                }); 
            }

            foreach (var cell in emptyCells)
            {
                if (cell == null) continue;
                cell.inspectReason = InspectReason.INS_ALIGNERR;
                cell.inspectReasons.Add(InspectReason.INS_ALIGNERR);
            }

            //int totalCount = allSrcCells.Count;
            //if (totalCount == 0)
            //    return new GaCellsGroup[0];

            //int span = totalCount >= N ? totalCount / N : 1;
            //while (N * span < totalCount)
            //    span++;

            //var groups = new GaCellsGroup[N];
            //for (int gid = 0; gid < N; gid++)
            //{
            //    var collection = new List<RegionCellX3Class>();
            //    int idx = gid * span;
            //    int idx2 = Math.Min(idx + span, totalCount);
            //    for (int i = idx; i < idx2; i++)
            //    {
            //        collection.Add(allSrcCells[i]);
            //    }
            //    //>>> collection.Sort((c1, c2) => (int)(c1.viewRectF.Y - c2.viewRectF.Y));
            //    var inflate = new Size(xRecipe.xExtendx, xRecipe.xExtendy);
            //    var grp = groups[gid] = new GaCellsGroup();
            //    grp.buildGaCells(fullFovBmp, inflate, collection);
            //}

            //verify(groups, alert: false);
            //return groups;

            var groups = CollectGroups_simple(N, allSrcCells, fullFovBmp, new Size(xRecipe.xExtendx, xRecipe.xExtendy));
            return groups;
        }
        static GaCellsGroup[] CollectGroups_out_grid(int N, RecipeFPIX3Class xRecipe, Bitmap fullFovBmp, EzEmptyTrayResult preEmptyResult)
        {
            if (preEmptyResult == null)
                return new GaCellsGroup[0];

            SizeF cellViewSizeF = SizeF.Empty;

            #region 取得_cellViewSizeF
            foreach (var cell in xRecipe.xRegionCells)
            {
                if (cell != null)
                {
                    cellViewSizeF = cell.viewRectF.Size;
                    break;
                }
            }
            #endregion

            if (cellViewSizeF == SizeF.Empty)
                return new GaCellsGroup[0];

            var outGridCells = new List<RegionCellX3Class>();
            var boundaryPolygonPts = getCellsBoundRotRect(xRecipe).Points();
            foreach (var bloc in preEmptyResult.IterOutGridAbnormalBlocs())
            {
                if (bloc == null) continue;
                var centerPt = new Point2f((float)bloc.Center.X, (float)bloc.Center.Y);
                bool isInside = Cv2.PointPolygonTest(boundaryPolygonPts, centerPt, false) >= 0;
                if (isInside) continue;

                var cell = new RegionCellX3Class();
                int index = outGridCells.Count;
                cell.Index = index;
                cell.CellRow = -1;
                cell.CellCol = -1;
                cell.lblName = $"OUT-{index}";
                cell.viewRectF = JetEazy.Qcvt.CreateCenterRect(centerPt.X, centerPt.Y, ref cellViewSizeF);
                outGridCells.Add(cell);
            }

            var groups = CollectGroups_simple(N, outGridCells, fullFovBmp, new Size(xRecipe.xExtendx, xRecipe.xExtendy));
            return groups;
        }
        static GaCellsGroup[] CollectGroups_simple(int N, List<RegionCellX3Class> srcCells, Bitmap fullFovBmp, Size inflate)
        {
            int totalCount = srcCells.Count;
            if (totalCount == 0)
                return new GaCellsGroup[0];

            int span = totalCount >= N ? totalCount / N : 1;
            while (N * span < totalCount)
                span++;

            var groups = new GaCellsGroup[N];
            for (int gid = 0; gid < N; gid++)
            {
                var collection = new List<RegionCellX3Class>();
                int idx = gid * span;
                int idx2 = Math.Min(idx + span, totalCount);
                for (int i = idx; i < idx2; i++)
                {
                    collection.Add(srcCells[i]);
                }
                //>>> collection.Sort((c1, c2) => (int)(c1.viewRectF.Y - c2.viewRectF.Y));
                var grp = groups[gid] = new GaCellsGroup();
                grp.buildGaCells(fullFovBmp, inflate, collection);
            }

            verify(groups, alert: false);
            return groups;
        }

        /// <summary>
        /// 取得 所有 xRecipe.xRegionCells 形成的邊界範圍 
        /// </summary>
        static RotatedRect getCellsBoundRotRect(RecipeFPIX3Class xRecipe)
        {
            var points = new List<Point2f>();
            int rowMax = -1;
            int colMax = -1;
            foreach (var cell in xRecipe.xRegionCells)
            {
                if(cell == null) continue;
                int r = cell.CellRow;
                int c = cell.CellCol;

                bool needsToCollect = false;
                if (r > rowMax || c > colMax)
                {
                    r = rowMax;
                    c = colMax;
                    needsToCollect = true;
                }
                else if (r == 0 && c == 0)
                {
                    needsToCollect = true;
                }
                else if (r == 0 && c == colMax)
                {
                    needsToCollect = true;
                }
                else if (r == rowMax && c == colMax)
                {
                    needsToCollect = true;
                }
                else if (r == rowMax && c == 0)
                {
                    needsToCollect = true;
                }

                if (needsToCollect)
                {
                    var rect = cell.viewRectF;
                    points.Add(new Point2f(rect.X, rect.Y));
                    points.Add(new Point2f(rect.Right, rect.Y));
                    points.Add(new Point2f(rect.Right, rect.Bottom));
                    points.Add(new Point2f(rect.X, rect.Bottom));
                }
            }
            return Cv2.MinAreaRect(points);
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public static void DisposeAll(GaCellsGroup[] groups)
        {
            if (groups == null) return;
            foreach (var g in groups)
                g?.Dispose();
        }

        #region PRIVATE_BUILD_FUNCTIONS
        void buildGaCells(Bitmap fullFovBmp, Size inflate, IEnumerable<RegionCellX3Class> srcCells)
        {
            var fullFovSize = fullFovBmp.Size;
            var gaCells = new List<GaCell>();
            foreach (var cell in srcCells)
            {
                Rectangle roi = Rectangle.Round(cell.viewRectF);
                roi.Inflate(inflate.Width, inflate.Height);
                GaUtil.Clip(ref roi, fullFovSize);
                var cellBmp = fullFovBmp.Clone(roi, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                var gaCell = new GaCell(cell, cellBmp, roi);
                gaCells.Add(gaCell);
            }
            _gaCells = gaCells;
            _fullFovRect = new Rectangle(0, 0, fullFovSize.Width, fullFovSize.Height);
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region PRIVATE_DEBUG_FUNCTIONS
        static void verify(GaCellsGroup[] groups, bool alert = true)
        {
#if DEBUG
            for (int ig = 0; ig < groups.Length; ig++)
            {
                var grp = groups[ig];
                int count0 = get_roi_boundary(grp, out Rectangle rect0);
                System.Diagnostics.Debug.WriteLine("GRP[{0}] ymin={1}, ymax={2}, count={3}",  
                                                    ig, rect0.Y, rect0.Bottom, count0);
                int ig2 = ig + 2;
                if (ig2 < groups.Length)
                {
                    var grp2 = groups[ig + 2];
                    int count2 = get_roi_boundary(grp2, out Rectangle rect2);
                    System.Diagnostics.Debug.WriteLine("GRP[{0}] ymin={1}, ymax={2}, count={3}",
                                                        ig2, rect2.Y, rect2.Bottom, count2);
                    if (alert)
                        System.Diagnostics.Debug.Assert(!rect0.IntersectsWith(rect2), "跳號的 2 個 Groups 不能相交!");
                }
            }
#endif
        }
        static int get_roi_boundary(GaCellsGroup grp, out Rectangle boundaryRect)
        {
            var x_min = int.MaxValue;
            var y_min = int.MaxValue;
            var x_max = int.MinValue;
            var y_max = int.MinValue;
            int count = 0;

            foreach (var gaCell in grp)
            {
                if (gaCell == null) continue;
                //var cell = gaCell?.Cell;
                //if (cell == null) continue;
                //var rect = cell.viewRectF;
                var rect = gaCell.CellRoi;
                x_min = (int)Math.Min(x_min, rect.X);
                x_max = (int)Math.Max(x_max, rect.Right);
                y_min = (int)Math.Min(y_min, rect.Y);
                y_max = (int)Math.Max(y_max, rect.Bottom);
                count++;
            }

            boundaryRect = new Rectangle(x_min, y_min, x_max - x_min, y_max - y_min);
            return count;
        }
        #endregion
    }
}
