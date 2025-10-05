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

using JetEazy.Utils;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;


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
            foreach (var cell in _gaCells)
                yield return cell;
        }
        public void Dispose()
        {
            foreach (var cell in _gaCells)
                cell?.Dispose();
        }

        /// <summary>
        /// caller 負責 fullFovBmp 生命週期
        /// </summary>
        public static GaCellsGroup[] CollectGroups(int N, RecipeFPIX3Class xRecipe, Bitmap fullFovBmp)
        {
            return CollectGroups_000_spanY(N, xRecipe, fullFovBmp);
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
        static GaCellsGroup[] CollectGroups_001_simple(int N, RecipeFPIX3Class xRecipe, Bitmap fullFovBmp)
        {
            var allSrcCells = xRecipe.xRegionCells;
            int totalCount = allSrcCells.Count;
            if (totalCount == 0)
                return new GaCellsGroup[0];

            int span = totalCount >= N ? totalCount / N : 1;
            while( N * span < totalCount)
                span++; 

            var groups = new GaCellsGroup[N];
            for (int gid = 0; gid < N; gid++)
            {
                var collection = new List<RegionCellX3Class>();
                int idx = gid * span;
                int idx2 = Math.Min(idx + span, totalCount);
                for (int i = idx; i < idx2; i++)
                {
                    collection.Add(allSrcCells[i]);
                }
                //>>> collection.Sort((c1, c2) => (int)(c1.viewRectF.Y - c2.viewRectF.Y));
                var inflate = new Size(xRecipe.xExtendx, xRecipe.xExtendy);
                var grp = groups[gid] = new GaCellsGroup();
                grp.buildGaCells(fullFovBmp, inflate, collection);
            }

            verify(groups, alert: false);

            return groups;
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
