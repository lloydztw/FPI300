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
using XRecipe = LaserAlignDX.OPSpace.RecipeSpace.RecipeFPIX3Class;


namespace LaserAlignDX.AoiModel
{
    public partial class RegionCellsDataHolder : IDisposable
    {
        #region GLOBAL_MESS
        XRecipe _xRecipe => XRecipe.Instance;
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
        ScanInspectMode _mode;
        #endregion


        public RegionCellsDataHolder()
        {
        }
        public void Dispose()
        {
            _grid?.Dispose();
            _grid = null;
            _outGridBlocs = null;
        }
        public void Reset()
        {
            this.Dispose();
        }

        public void Update(IEnumerable<XCell> cells, int mode)
        {
            _mode = (ScanInspectMode)mode;

            //_withPadGaps = _xRecipe.InspectParams.xAlgorithm == MatchAlgorithmEnum.GridMatch &&
            //               _xRecipe.InspectParams.optPadEdgeGapsMeasurement &&
            //               _xRecipe.InspectParams.optChipMeasurement;

            try
            {
                Reset();

                //_xRegionCells = cells != null ? cells : _xRecipe.xRegionCells;
                _grid = ConvertToGrid(cells);
                _outGridBlocs = ConvertToOutGridBlocsList(cells);
            }
            catch (Exception ex)
            {
                GaUtil.LOG_ERROR(ex, $"{GetType().Name}.UpdateResult");
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
                    var bloc = (CellBloc)_grid.Get(r, c);
                    var cell = bloc.Cell;
                    var xCell = cell?.OutGridLink;
                    if (xCell != null)
                        yield return xCell;
                    else
                        yield return cell;
                }
            }
        }
        public XCell GetCellByRowCol(int row, int col)
        {
            var bloc = (CellBloc)_grid?.Get(row, col);
            return bloc?.Cell;
        }

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
        bool checkResult(XCell cell, out bool isPass, out bool isEmpty)
        {
            isPass = false;
            isEmpty = false;

            if (cell == null)
                return false;

            if (_mode == ScanInspectMode.NOTRAY)
            {
                string abnormalStr = cell.GetNoTrayDesc();
                if (string.IsNullOrEmpty(abnormalStr))
                    isEmpty = true;
                else
                    isEmpty = false;
                isPass = isEmpty;
            }
            else
            {
                isEmpty = false;
                if (cell.IsResultPass())
                    isPass = true;
                else if (!cell.IsEmptyPlaceHold())
                    isPass = false;
                else
                    isEmpty = true;
            }
            return true;
        }

        #region PRIVATE_FUNCTIONS
        EzBlocsGrid ConvertToGrid(IEnumerable<XCell> cells)
        {
            int rows = 0;
            int cols = 0;
            var blocs = new List<EzBloc>();

            #region 蒐集_CELL_BLOCS
            foreach (var cell in cells)
            {
                if (cell == null) continue;
                CellBloc cbloc;
                if (_mode == ScanInspectMode.NOTRAY)
                    cbloc = new CellBloc(cell, cell.viewRectF);
                else
                    cbloc = new CellBloc(cell);
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

            #region 將空缺格點_填入_PLACE_HOLDER
            //var C = GaMvcConfig.SysModel.ActiveCarrierID;
            //var camGrid = C == CarrierEnum.C1 ? _xRecipe.xCamGrid1 : _xRecipe.xCamGrid2;
            //for (int r = 0; r < rows; r++)
            //{
            //    for (int c = 0; c < cols; c++)
            //    {
            //        var cb = (CellBloc)grid.Get(r, c);
            //        if (cb == null || cb?.Cell?.chipLocInCamera == null)
            //        {
            //            var placeHolder = camGrid.Get(r, c);
            //            var rect = placeHolder.Rect;
            //            cb = new CellBloc(cb?.Cell, rect);
            //            grid.Set(r, c, cb);
            //        }
            //    }
            //}
            #endregion

            return grid;
        }
        List<EzBloc> ConvertToOutGridBlocsList(IEnumerable<XCell> cells)
        {
            var outGridBlocs = new List<EzBloc>();
            if (_mode == ScanInspectMode.NOTRAY)
            {
                foreach (var rect in _xRecipe.xOutBlocs)
                {
                    outGridBlocs.Add(new EzBloc(rect, 0));
                }
            }
            else
            {
                foreach (var cell in cells)
                {
                    var ogCell = cell?.OutGridLink;
                    if (ogCell == null) continue;
                    CellBloc cb = new CellBloc(ogCell);
                    outGridBlocs.Add(cb);
                }
            }
            return outGridBlocs;
        }
        #endregion
    }
}
