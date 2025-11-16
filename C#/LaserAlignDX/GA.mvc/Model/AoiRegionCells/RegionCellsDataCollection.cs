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
using System;
using System.Collections.Generic;
using System.Drawing;
using XCell = LaserAlignDX.OPSpace.RegionCellX3Class;


namespace LaserAlignDX.AoiModel
{
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
        EzBlocsGrid _grid;
        IList<EzBloc> _outGridBlocs;
        #endregion

        #region PRIVATE_SETTINGS_DATA
        //ScanInspectMode _mode = ScanInspectMode.MEASUREAOI;
        #endregion

        public RegionCellsDataCollection(IEnumerable<XCell> cells = null)
        {
            if (cells != null)
                Update(cells);
        }
        public void Update(IEnumerable<XCell> cells)
        {
            try
            {
                cleanUp(_grid);
                cleanUp(_outGridBlocs);
                _grid = buildGrid(cells);
                _outGridBlocs = new List<EzBloc>();
            }
            catch (Exception ex)
            {
                GaUtil.LOG_ERROR(ex, $"{GetType().Name}.UpdateResult");
            }
        }
        public void Detach()
        {
            // 刻意避免 源頭的 cells 被 Dispose
            _grid?.Dispose();
            _grid = null;
            _outGridBlocs = null;
        }
        public void Dispose()
        {
            cleanUp(_grid);
            _grid = null;
            cleanUp(_outGridBlocs);
            _outGridBlocs = null;
        }
        public void Reset()
        {
            foreach (var cell in IterGridCells())
                cell?.Reset();
            _outGridBlocs = new List<EzBloc>();
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
            var xCell = cell?.OutGridLink;
            return xCell ?? cell;
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
                    if (cell != null)
                        yield return cell;
                }
            }
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

        /// <summary>
        /// 【空盤檢測】加入格位外 疑似有料的區塊
        /// </summary>
        public void AppendOutGridBloc(EzBloc bloc)
        {
            if (bloc != null)
                _outGridBlocs.Add(bloc);
        }

#if(OPT_RESERVED)
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
    }
}
