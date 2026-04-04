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
using JetEazy.QMath;
using JetEazy.Utils;
using LaserAlignDX.OPSpace;
using LeTian.AoiLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using XCell = LaserAlignDX.OPSpace.RegionCellX3Class;


namespace LaserAlignDX.AoiModel
{
    /// <summary>
    /// 將來要把所有的 RegionCells 都交由 此容器載體管理
    /// </summary>
    public partial class RegionCellsDataCollection : IDisposable
    {
        #region GLOBAL_MESS
        //XRecipe _xRecipe => XRecipe.Instance;
        #endregion

        #region INNER_CLASS
        public class CellBloc : EzBloc
        {
            public CellBloc(XCell cell, RectangleF rect) : base(Rectangle.Round(rect), 1)
            {
                Cell = cell;
            }
            public CellBloc(XCell cell) : base(Rectangle.Empty, 1)
            {
                Cell = cell;

                //var mvdRectF = cell.DrawResultRectF();
                //Rect = Rectangle.Round(GaImageUtil.ToRectangleF(mvdRectF));
                //Center = new JetEazy.QMath.QVector(mvdRectF.CenterX, mvdRectF.CenterY);

                var chipQuad2D = cell?.ChipData?.ChipQuad2D;
                if (chipQuad2D != null)
                {
                    Rect = Rectangle.Round(cell.viewRectF);
                    Center = new QVector(chipQuad2D.Center);
                }
                else
                {
                    var cc = JetEazy.Qcvt.CenterF(ref cell.viewRectF);
                    Rect = Rectangle.Round(cell.viewRectF);
                    Center = new QVector(cc.X, cc.Y);
                }
            }
            public XCell Cell
            {
                get; private set;
            }
            public bool IsEmpty
            {
                //get => string.IsNullOrEmpty(NonEmptyDesc);
                get => Cell == null || Cell.IsEmptyPlaceHold();
            }
        };
        #endregion

        #region PRIVATE_DATA_HOLDERS
        object _sync = new object();
        bool _isCellsOwner;
        EzBlocsGrid _grid;
        IList<EzBloc> _outGridBlocs;
        #endregion

        #region PRIVATE_SETTINGS_DATA
        //ScanInspectMode _mode = ScanInspectMode.MEASUREAOI;
        #endregion

        /// <summary>
        /// Caller 必須負責 cells 生命週期
        /// </summary>
        public RegionCellsDataCollection(IEnumerable<XCell> cells = null)
        {
            if (cells != null)
                Attach(cells);
        }
        /// <summary>
        /// Caller 必須負責 cells 生命週期
        /// </summary>
        /// <param name="cells"></param>
        public void Attach(IEnumerable<XCell> cells)
        {
            lock (_sync)
            {
                Dispose();
                Update(cells);
                _isCellsOwner = false;
            }
        }
        /// <summary>
        /// cells 的生命週期 由 RegionCellsDataCollection 負責管理 (會在 Dispose 時一併 Dispose cells)
        /// </summary>
        /// <param name="cells"></param>
        public void TakeOver(IEnumerable<XCell> cells)
        {
            lock (_sync)
            {
                Dispose();
                Update(cells);
                _isCellsOwner = true;
            }
        }

        #region PRIVATE_FUNCTIONS
        void Update(IEnumerable<XCell> cells)
        {
            try
            {
                lock (_sync)
                {
                    cleanUp(_grid);
                    cleanUp(_outGridBlocs);
                    _grid = buildGrid(cells);
                    _outGridBlocs = new List<EzBloc>();
                    removeOverlapOutgridCells();
                }
            }
            catch (Exception ex)
            {
                GaUtil.LOG_ERROR(ex, $"{GetType().Name}.Update(cells)");
            }
        }
        void Detach()
        {
            // 調用此函式 避免 源頭的 cells 被 Dispose
            lock (_sync)
            {
                _grid?.Dispose();
                _grid = null;
                _outGridBlocs = null;
                _isCellsOwner = false;
            }
        }
        #endregion

        public void Dispose()
        {
            lock (_sync)
            {
                if (!_isCellsOwner)
                    Detach();
                cleanUp(_grid);
                _grid = null;
                cleanUp(_outGridBlocs);
                _outGridBlocs = null;
            }
        }
        public void Reset()
        {
            lock (_sync)
            {
                foreach (var cell in IterGridCells())
                    cell?.Reset();
                _outGridBlocs = new List<EzBloc>();
            }
        }

        public int Rows
        {
            get => _grid != null ? _grid.Rows : 0;
        }
        public int Cols
        {
            get => _grid != null ? _grid.Cols : 0;
        }

        public XCell GetGridCell(int row, int col)
        {
            var bloc = (CellBloc)_grid?.Get(row, col);
            var cell = bloc?.Cell;
            return cell;
        }
        public XCell GetFinalCell(int row, int col)
        {
            var bloc = (CellBloc)_grid?.Get(row, col);
            var cell = bloc?.Cell;
            var outGridCell = cell?.OutGridLink;
            
            if (outGridCell != null)
                return outGridCell;
            
            if (cell == null)
            {
                cell = new XCell() { CellRow = row, CellCol = col, viewRectF = RectangleF.Empty };
                cell.MarkResult(InspectReason.NG_EMPTY, reset: true);
            }

            return cell;
        }
        public IEnumerable<XCell> IterGridCells()
        {
            if (_grid != null)
            {
                int rows = _grid.Rows;
                int cols = _grid.Cols;
                foreach ((var r, var c) in Zigzag.IterZigzag(rows, cols))
                {
                    var cell = GetGridCell(r, c);
                    if (cell != null)
                        yield return cell;
                }
            }
        }
        public IEnumerable<XCell> IterFinalCells()
        {
            if (_grid != null)
            {
                int rows = _grid.Rows;
                int cols = _grid.Cols;
                foreach ((var r, var c) in Zigzag.IterZigzag(rows, cols))
                {
                    var cell = GetFinalCell(r, c);
                    yield return cell;
                }
            }
        }
        public int GetStatistics(out int passCount, out int ngCount, out int emptyCount, out int unknownCount)
        {
            int totalCellsCount = 0;
            ngCount = 0;
            passCount = 0;
            emptyCount = 0;
            unknownCount = 0;

            foreach (var cell in IterFinalCells())
            {
                if (cell != null)
                {
                    totalCellsCount++;
                    if (cell.IsResultPass())
                        passCount++;
                    else if (cell.IsEmptyPlaceHold())
                        emptyCount++;
                    else if (cell.IsAmbiguousBloc())
                        unknownCount++;
                    else if (cell.ChipData != null)
                        ngCount++;
                }
            }
            return totalCellsCount;
        }

        /// <summary>
        /// 【空盤檢測】加入格位外 疑似有料的區塊 (預備將來改版使用)
        /// </summary>
        public void AppendOutGridBloc(EzBloc bloc)
        {
            if (bloc != null)
                _outGridBlocs.Add(bloc);
        }

        #region OLD_CODE
#if (OPT_RESERVED)
        public IEnumerable<EzBloc> iterNonEmptyBlocs(bool onGrid)
        {
            if (onGrid)
            {
                if (_grid != null)
                {
                    foreach (CellBloc bloc in _grid)
                    {
                        var cell = bloc?.Cell;
                        if (cell != null && !bloc.IsEmpty)
                            yield return bloc;
                    }
                }
            }
            else
            {
                if (_outGridBlocs != null)
                {
                    foreach (EzBloc bloc in _outGridBlocs)
                    {
                        if (bloc == null) continue;
                        yield return bloc;
                    }
                }
            }
        }
        public IEnumerable<EzBloc> iterEmptyBlocs()
        {
            foreach (CellBloc bloc in _grid)
            {
                var cell = bloc?.Cell;
                if (cell != null && bloc.IsEmpty)
                    yield return bloc;
            }
        }
        public IEnumerable<CellBloc> iterCellBlocs()
        {
            if (_grid != null)
            {
                foreach (var cell in _grid)
                {
                    if (cell is CellBloc cb)
                        yield return cb;
                }
            }
            if (_outGridBlocs != null)
            {
                foreach (var bloc in _outGridBlocs)
                {
                    if (bloc is CellBloc cb)
                        yield return cb;
                }
            }
        }
#endif
        #endregion

        #region PRIVATE_FUNCTIONS
        EzBlocsGrid buildGrid(IEnumerable<XCell> cells)
        {
            int rows = 0;
            int cols = 0;
            var blocs = new List<EzBloc>();

            #region 蒐集_CELL_BLOCS
            foreach (var cell in cells)
            {
                if (cell == null) 
                    continue;

                //CellBloc cbloc;
                //if (_mode == ScanInspectMode.NOTRAY)
                //    cbloc = new CellBloc(cell, cell.viewRectF);
                //else
                //    cbloc = new CellBloc(cell);

                var cbloc = new CellBloc(cell, cell.viewRectF);
                blocs.Add(cbloc);
                rows = Math.Max(rows, cell.CellRow + 1);
                cols = Math.Max(cols, cell.CellCol + 1);
            }
            #endregion

            var builder = new EzBlocsGridBuilder();
            var grid = builder.BuildEmptyGrid(blocs, rows, cols);

            #region 將_CELL_BLOCS_填入_GRID
            foreach (CellBloc bloc in blocs)
            {
                int r = bloc.Cell.CellRow;
                int c = bloc.Cell.CellCol;
                grid.Set(r, c, bloc);
            }
            #endregion

            return grid;
        }
        void cleanUp(EzBlocsGrid grid)
        {
            if (grid != null)
            {
                foreach (var item in grid)
                {
                    if (item is CellBloc cb)
                    {
                        cb.Cell?.Dispose();
                    }
                }
            }
        }
        void cleanUp(IList<EzBloc> list)
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    if (item is CellBloc cb)
                    {
                        cb.Cell?.Dispose();
                    }
                }
            }
        }
        #endregion

        #region REPEATED_OVERLAP_CHECK_FUNCTIONS
        void checkFinalCells()
        {
            var repeateCells = new List<XCell>();
            var dict = new Dictionary<uint, XCell>();

            foreach (var cell in IterFinalCells())
            {
                var chipQuad2D = cell?.ChipData?.ChipQuad2D;
                if (chipQuad2D == null)
                    continue;

                int r = cell.CellRow;
                int c = cell.CellCol;
                uint rcKey = ((uint)r << 16) + (uint)c;

                if (!dict.ContainsKey(rcKey))
                {
                    dict.Add(rcKey, cell);
                }
                else
                {
                    repeateCells.Add(cell);
                }
            }

            if (repeateCells.Count != 0)
            {
                foreach (var cell in repeateCells)
                {
                    LtDebug.LOG.Error($"發現重複的 Cell[{cell.CellRow},{cell.CellCol}]，已將其 ChipData 設為 null 以避免後續錯誤");
                    cell.ChipData = null;
                }
            }
        }
        void removeOverlapOutgridCells()
        {
            //NOTE:
            //目前 如果有多個 OutGridLink 可能指向同一個 [row, col],
            //必須只保留最佳的 OutGridLink

            checkFinalCells();

            var outgridCellOwners = new List<XCell>();
            foreach (var cell in IterGridCells())
            {
                var outGridCell = cell?.OutGridLink;
                if (outGridCell != null)
                    outgridCellOwners.Add(cell);
            }

            if (outgridCellOwners.Count == 0)
                return;

            for (int N = outgridCellOwners.Count, i = 0; i < N; i++)
            {
                var ownerA = outgridCellOwners[i];
                var ogCellA = removeInvalideOutgridLink(ownerA);
                var ogQuadA = ogCellA?.ChipData?.ChipQuad2D;
                if (ogQuadA == null)
                    continue;

                var ownerCenterA = new QVector2(ownerA.viewRectF.X + ownerA.viewRectF.Width / 2.0, ownerA.viewRectF.Y + ownerA.viewRectF.Height / 2.0);
                var offsetA = (ogQuadA.Center - ownerCenterA).NormLength;
                var ogRectA = ogQuadA.BoundaryRect;
                var inflate = ogRectA.Height / 10;
                ogRectA.Inflate(-inflate, -inflate);

                for (int j = i + 1; j < N; j++)
                {
                    var owerB = outgridCellOwners[j];
                    var ogCellB = removeInvalideOutgridLink(owerB);
                    var ogQuadB = ogCellB?.ChipData?.ChipQuad2D;
                    if (ogQuadB == null)
                        continue;

                    var ogRectB = ogQuadB.BoundaryRect;
                    ogRectB.Inflate(-inflate, -inflate);

                    bool isOverlap = ogRectA.IntersectsWith(ogRectB);
                    if (!isOverlap)
                        continue;

                    //目前的邏輯是：保留與格位中心點距離較近的 OutGridLink
                    var ownerCenterB = new QVector2(owerB.viewRectF.X + owerB.viewRectF.Width / 2.0, owerB.viewRectF.Y + owerB.viewRectF.Height / 2.0);
                    var offsetB = (ogQuadB.Center - ownerCenterB).NormLength;
                    if (offsetB < offsetA)
                    {
                        ownerA.OutGridLink = null;
                        ownerCenterA = ownerCenterB;
                        ogRectA = ogRectB;
                        offsetA = offsetB;
                        ownerA = owerB;
                    }
                    else
                    {
                        owerB.OutGridLink = null;
                    }
                }
            }
        }
        XCell removeInvalideOutgridLink(XCell cell)
        {
            if (cell == null) return null;
            if (cell.OutGridLink != null)
            {
                var outGridCell = cell.OutGridLink;
                var chipData = outGridCell.ChipData;
                if (chipData == null || chipData.ChipQuad2D == null)
                    cell.OutGridLink = null;
            }
            return cell.OutGridLink;
        }
        #endregion
    }
}
