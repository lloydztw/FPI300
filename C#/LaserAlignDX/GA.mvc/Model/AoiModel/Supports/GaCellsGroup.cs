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
using JetEazy.OpenCV;
using JetEazy.QMath;
using JetEazy.Utils;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
        public void TakeOver(GaCell src)
        {
            if (this != src && src != null)
            {
                var oldBmp = this.CellBmp;
                this.Cell = src.Cell;
                this.CellBmp = src.CellBmp;
                this.CellRoi = src.CellRoi;
                if (oldBmp != this.CellBmp)
                    oldBmp?.Dispose();
            }
        }
        public void Dispose()
        {
            CellBmp?.Dispose();
            CellBmp = null;
        }

        public override string ToString()
        {
            return Cell?.ToString();
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
        public static GaCellsGroup[] CollectGroups(int N, RecipeFPIX3Class xRecipe, Bitmap fullFovBmp, EzEmptyTrayResult preEmptyResult = null, string option = null)
        {
            switch(option)
            {
                case "IN_GRID_BOUND":
                default:
                    return CollectGroups_in_grid_bound(N, xRecipe, fullFovBmp, preEmptyResult);
                case "ON_GRID_BOUND":
                    return CollectGroups_on_grid_bound(N, xRecipe, fullFovBmp, preEmptyResult);
                case "OUT_GRID_BOUND":
                    return CollectGroups_out_grid_bound(N, xRecipe, fullFovBmp, preEmptyResult);
                case "NG_DIM_ONLY":
                    return CollectGroups_ng_dim_for_2nd_trial(N, xRecipe, fullFovBmp);
            }
        }

        #region PRIVATE_COLLECT_FUNCTIONS
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
        static GaCellsGroup[] CollectGroups_in_grid_bound(int N, RecipeFPIX3Class xRecipe, Bitmap fullFovBmp, EzEmptyTrayResult preEmptyResult)
        {
            var inGridBoundCells = new List<RegionCellX3Class>(xRecipe.xRegionCells);
            var suckerEmptyCells = new List<RegionCellX3Class>();

            //(1) 蒐集 吸嘴空格
            if (preEmptyResult != null)
            {
                inGridBoundCells.RemoveAll(c =>
                {
                    if (c == null) return true;
                    preEmptyResult.GetBlocByRowCol(c.CellRow, c.CellCol, out var _, out bool isSucker);
                    if (isSucker)
                        suckerEmptyCells.Add(c);
                    return isSucker;
                }); 
            }

            //(2) 標記 吸嘴空格
            foreach (var cell in suckerEmptyCells)
            {
                if (cell == null) continue;
                cell.MarkResult(InspectReason.NG_EMPTY, reset: true);
            }

            //(3) 分配多線呈的群組
            var groups = CollectGroups_simple_balance(N, inGridBoundCells, fullFovBmp, new Size(xRecipe.xExtendx, xRecipe.xExtendy));
            return groups;
        }
        static GaCellsGroup[] CollectGroups_on_grid_bound(int N, RecipeFPIX3Class xRecipe, Bitmap fullFovBmp, EzEmptyTrayResult preEmptyResult)
        {
            if (preEmptyResult == null || preEmptyResult.Grid == null)
                return null;

            //(0) 蒐集 "邊界" & "可能有料" 的格位
            var collectedCells = new List<RegionCellX3Class>();
            if (true)
            {
                var emptyGrid = preEmptyResult.Grid;
                int rows = emptyGrid.Rows;
                int cols = emptyGrid.Cols;
                foreach (var cell in xRecipe.xRegionCells)
                {
                    //(a) Empty
                    if (cell == null) continue;

                    //(b) 排除 非邊界格位
                    int r = cell.CellRow;
                    int c = cell.CellCol;
                    bool isBoundary = (r == 0 || r == rows - 1) || (c == 0 || c == cols - 1);
                    if (!isBoundary) continue;
                    //>>> System.Diagnostics.Debug.WriteLine("Boundary [{0},{1}]", r, c);

                    //(c) 排除已經定位之晶粒
                    if (cell.IsLocated()) continue;                                     // 已經被佔位
                    if (cell.OutGridLink != null) continue;                             // 已經被佔位

                    //(e) 排除空格
                    preEmptyResult.GetBlocByRowCol(r, c, out var _, out bool isSukcer);
                    if (isSukcer) continue;

                    //(f) 蒐集 剩下可能有料 cell 之 副本
                    //>>> System.Diagnostics.Debug.WriteLine("Boundary [{0},{1}]", r, c);
                    collectedCells.Add(cell);
                }
                if (collectedCells.Count == 0)
                    return null;
            }

            using (var workBmp = (Bitmap)fullFovBmp.Clone())
            {
                //(1) 把已經定位到的晶粒塗成平均色
                FillMeanColor_To_Located_Chips(workBmp, xRecipe.xRegionCells);

                //(2) 重新設定 collectedCells
                for (int i = 0, count = collectedCells.Count; i < count; i++)
                {
                    var cellOriginal = collectedCells[i];
                    var cellCopy = new RegionCellX3Class
                    {
                        Index = cellOriginal.Index,
                        CellRow = cellOriginal.CellRow,
                        CellCol = cellOriginal.CellCol,
                        lblName = cellOriginal.lblName,
                        viewRectF = cellOriginal.viewRectF,
                        ChipData = cellOriginal.ChipData,
                        OrgX = cellOriginal.OrgX,
                        OrgY = cellOriginal.OrgY,
                    };
                    collectedCells[i] = cellCopy;
                    cellOriginal.OutGridLink = cellCopy;
                }

                //(3) 分配多線呈的群組 (使用 2倍 Extendx, Extendy)
                var inflate = new System.Drawing.Size(xRecipe.xExtendx * 2, xRecipe.xExtendy * 2);
                var boundaryGrps = CollectGroups_simple_balance(N, collectedCells, workBmp, inflate);
                return boundaryGrps;
            }
        }
        static GaCellsGroup[] CollectGroups_out_grid_bound(int N, RecipeFPIX3Class xRecipe, Bitmap fullFovBmp, EzEmptyTrayResult preEmptyResult)
        {
            if (preEmptyResult == null)
                return null;

            //(0) 取得 cellViewSizeF
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
            if (cellViewSizeF == SizeF.Empty)
                return null;
            #endregion

            //(1) 蒐集 外圍 "可能有料" 的 Cells
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
            if (outGridCells.Count == 0)
                return null;

            using (var workBmp = (Bitmap)fullFovBmp.Clone())
            {
                //(2) 把已經定位到的晶粒塗成平均色
                FillMeanColor_To_Located_Chips(workBmp, xRecipe.xRegionCells);

                //(3) 分配多線呈的群組
                var groups = CollectGroups_simple_balance(N, outGridCells, workBmp, new Size(xRecipe.xExtendx, xRecipe.xExtendy));
                return groups;
            }
        }
        static GaCellsGroup[] CollectGroups_ng_dim_for_2nd_trial(int N, RecipeFPIX3Class xRecipe, Bitmap fullFovBmp, object dummy = null)
        {
            var passCells = new List<RegionCellX3Class>();
            var ngDimCells = new List<RegionCellX3Class>();

            //(1) 蒐集 Cells
            foreach (var cell in xRecipe.xRegionCells)
            {
                var xCell = cell?.OutGridLink;
                if (xCell == null) xCell = cell;
                if (xCell == null) continue;
                if (!xCell.IsLocated()) continue;

                bool isPass = xCell.IsResultPass();
                bool isPlaceHold = xCell.IsEmptyPlaceHold();

                if (isPass)
                    passCells.Add(cell);
                else if (!isPlaceHold)
                    ngDimCells.Add(cell);
            }
            if (ngDimCells.Count == 0 || passCells.Count == 0)
                return null;

            //(2) 清除 NG 標記
            foreach (var cell in ngDimCells)
            {
                if (cell == null) continue;
                cell.MarkResult(InspectReason.PASS, reset: true);
            }

            using (var workBmp = (Bitmap)fullFovBmp.Clone())
            {
                //(3) 把 PASS晶粒 塗成 背景暗色
                FillBackInvColor_To_PassDimChips(workBmp, passCells);

                //(4) 分配多線呈的群組
                var groups = CollectGroups_simple_balance(N, ngDimCells, workBmp, new Size(xRecipe.xExtendx, xRecipe.xExtendy));
                return groups;
            }
        }
        static GaCellsGroup[] CollectGroups_simple_balance(int N, List<RegionCellX3Class> srcCells, Bitmap fullFovBmp, Size inflate)
        {
            int totalCount = srcCells.Count;
            if (totalCount == 0)
                return new GaCellsGroup[0];

            //int span = totalCount >= N ? totalCount / N : 1;
            //while (N * span < totalCount)
            //    span++;
            int span = Math.Max(1, (int)Math.Ceiling(1.0 * totalCount / N));

            var groups = new GaCellsGroup[N];
            for (int gid = 0; gid < N; gid++)
            {
                var collection = new List<RegionCellX3Class>();

                int idxStart = gid * span;
                int idxEnd = Math.Min(idxStart + span, totalCount);
                for (int i = idxStart; i < idxEnd; i++)
                    collection.Add(srcCells[i]);

                var grp = groups[gid] = new GaCellsGroup();
                grp.buildGaCells(fullFovBmp, inflate, collection);
            }

            verify(groups, alert: false);
            return groups;
        }

        /// <summary>
        /// 把 已經定位 的 晶粒 塗成 平均色
        /// (有 OutGridLink 者優先)
        /// </summary>
        static void FillMeanColor_To_Located_Chips(Bitmap fullFovBmp2, IEnumerable<RegionCellX3Class> xRegionCells)
        {
            using (var bridge = new QxImageBridge(fullFovBmp2))
            {
                var imgFullfov = bridge.Image;

                // 計算 中央區塊 的平均色
                var sz = Math.Min(imgFullfov.Width, imgFullfov.Height);
                var roi = new Rect(0, 0, sz / 4, sz / 4);
                JetEazy.Qcvt.SetCenter(ref roi, imgFullfov.Width / 2, imgFullfov.Height / 2);
                Scalar mean = imgFullfov[roi].Mean();

                // 枚舉 已經定位 的 晶粒
                foreach (var cell in xRegionCells)
                {
                    var xCell = cell?.OutGridLink;
                    if (xCell == null) xCell = cell;

                    var chipQuad2D = xCell?.ChipData?.ChipQuad2D;
                    if (chipQuad2D == null)
                        continue;

                    var pts = Array.ConvertAll(chipQuad2D.Corners, c => new OpenCvSharp.Point((int)c.X, (int)c.Y));
                    Cv2.FillConvexPoly(imgFullfov, pts, mean);
                }
            }
        }
        
        /// <summary>
        /// 把 PASS 晶粒 塗成 背景反色
        /// </summary>
        static void FillBackInvColor_To_PassDimChips(Bitmap fullFovBmp2, IEnumerable<RegionCellX3Class> xRegionCells)
        {
            using (var bridge = new QxImageBridge(fullFovBmp2))
            {
                var imgFullfov = bridge.Image;

                bool isDarkCarrier = RecipeFPIX3Class.Instance.InspectParams.xCarrierBackground == BasicSpace.EdgeBackGroundType.Dark;
                Scalar fillColor = isDarkCarrier ? Scalar.White : Scalar.Black;

                // 枚舉 已經抓到 邊線 的 晶粒
                int NP = 4;
                foreach (var cell in xRegionCells)
                {
                    var xCell = cell?.OutGridLink;
                    if (xCell == null) xCell = cell;
                    if (xCell == null || !xCell.IsResultPass()) 
                        continue;
                    
                    var lines = xCell?.ChipData?.LineSegments;
                    if (lines == null || lines.Length < NP) 
                        continue;

                    NP = lines.Length;
                    var corners = new List<QVector>();
                    for (int i = 0; i < NP; i++)
                    {
                        var k = i == 0 ? NP - 1 : i - 1;
                        var L0 = lines[k];
                        var L1 = lines[i];
                        if (L0 == null || L1 == null)
                            continue;
                        var pt = L0.CalcIntersectedPoint(L1);
                        if (pt != null)
                            corners.Add(pt);
                    }
                    if (corners.Count < NP)
                        continue;

                    var pts = Array.ConvertAll(corners.ToArray(), c => new OpenCvSharp.Point((int)c.X, (int)c.Y));
                    Cv2.FillConvexPoly(imgFullfov, pts, fillColor);
                }
            }
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
        #endregion

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
#if (DEBUG && false)
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
