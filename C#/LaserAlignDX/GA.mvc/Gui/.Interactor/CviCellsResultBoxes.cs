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

using JetEazy.FormSpace;
using JetEazy.ImageViewerEx;
using JetEazy.Match;
using JetEazy.OpenCV;
using JetEazy.QMath;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.OPSpace;
using LeTian.AoiLib;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


using Point = System.Drawing.Point;
using XCell = LaserAlignDX.OPSpace.RegionCellX3Class;
using XRecipe = LaserAlignDX.OPSpace.RecipeSpace.RecipeFPIX3Class;


namespace LaserAlignDX.Mvc.Gui.ChipCellsViewer
{
    public partial class CviCellsResultBoxes : CviAbsTooltipBox
    {
        #region INNER_CLASS
        class CellBloc : EzBloc
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
            public string NonEmptyDesc
            {
                get; internal set;
            }
            public bool IsEmpty => string.IsNullOrEmpty(NonEmptyDesc);
        };
        #endregion

        #region GLOBAL_MESS
        XRecipe _xRecipe
        {
            get => XRecipe.Instance;
        }
        bool _withPadGaps = false;
        #endregion

        #region GUI_ITEMS
        CviRotRectBox _cviRegionBox;
        List<IvDrawItem> _drawItems = new List<IvDrawItem>();
        Font _font = null;
        #endregion

        public Control lblSummaryTitle
        {
            get;
            set;
        }

        #region RUNTIME_DATA
        bool _isCtrlPressed;
        #endregion

        #region PRIVATE_BLOCS_DATA
        EzBlocsGrid _grid;
        IList<EzBloc> _outGridBlocs;
        ScanInspectMode _mode;
        #endregion

        public CviCellsResultBoxes()
        {
            base.OnCursorsChanged += CviCellsResultBoxes_OnCursorsChanged;
        }
        public void Reset()
        {
            _grid?.Dispose();
            _grid = null;
            _outGridBlocs = null;
            _drawItems.Clear();
        }
        public void UpdateResult(IEnumerable<XCell> cells, int mode)
        {
            _mode = (ScanInspectMode)mode;

            _withPadGaps = _xRecipe.InspectParams.xAlgorithm == MatchAlgorithmEnum.GridMatch &&
                           _xRecipe.InspectParams.optPadEdgeGapsMeasurement &&
                           _xRecipe.InspectParams.optChipMeasurement;

            try
            {
                Reset();

                updateCellsToGrid(cells, out _grid);
                updateOutGridCellBlocs(cells, out _outGridBlocs);
                updatePassNgEmptyCount(cells);
                updateDrawItems();

                autoAdjustFetchSize();
            }
            catch (Exception ex)
            {
                GaUtil.LOG_ERROR(ex, $"{GetType().Name}.UpdateResult");
            }
        }

        #region OVERRIDES
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            //>>> base.OnDraw(viewer, gxView);

            if (_cviRegionBox != null && _cviRegionBox.Visible)
                _cviRegionBox.OnDraw(viewer, gxView);

            if (_font == null)
                _font = viewer.Font;

            if (_grid != null || _outGridBlocs != null)
            {
                bool isWorld = viewer.IsInWorldCoordinate();
                if (!isWorld)
                    viewer.SwitchToWorldCoordinate(gxView);

                try
                {
                    foreach (var item in _drawItems)
                    {
                        item?.OnDraw(viewer, gxView);
                    }
                }
                catch (Exception ex)
                {
                    GaUtil.LOG_ERROR(ex, $"{GetType().Name}.OnDraw");
                }

                if (!isWorld)
                    viewer.SwitchToViewportCoordinate(gxView);
            }

            base.OnDraw(viewer, gxView);
        }
        public override bool OnMouseDown(CvImageViewer viewer, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && _isCtrlPressed)
            {
                popupMenuStrip(viewer, e.Location);
                _isCtrlPressed = false;
                return false;
            }
            return base.OnMouseDown(viewer, e);
        }
        public override bool OnMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            bool isChanged = false;
            foreach (var item in _drawItems)
            {
                if (item != null)
                    isChanged |= item.OnMouseMove(viewer, e);
            }
            isChanged |= base.OnMouseMove(viewer, e);
            return isChanged;
        }
        public override void OnKeyDown(CvImageViewer viewer, KeyEventArgs e)
        {
            if (!Visible)
                return;

            _isCtrlPressed = e.Control;

            switch (e.KeyCode)
            {
                case Keys.Escape:
                    DebugMatchingOff();
                    _isCtrlPressed = false;
                    break;

                case Keys.F4:
                    if (!BY_PASS && e.Control)
                    {
                        if (_cursorBloc is CellBloc cellBloc)
                            DebugMatching(cellBloc);
                        _isCtrlPressed = false;
                    }
                    break;

                case Keys.F2:
                    if (!BY_PASS && e.Control)
                    {
                        DumpCellRegions(viewer);
                        _isCtrlPressed = false;
                    }
                    break;

                case Keys.C:
                    if (!BY_PASS && e.Control)
                    {
                        CopyOneChipDimsToClipboard();
                        _isCtrlPressed = false;
                    }
                    break;
            }

            base.OnKeyDown(viewer, e);
        }
        private void CviCellsResultBoxes_OnCursorsChanged(object sender, EventArgs e)
        {
            var cursorBloc = GetCursorBloc(0);
            //var cellBloc = cursorBloc is CellBloc cb ? cb : cursorBloc?.Tag as CellBloc;
            //var activeCell = cellBloc?.Cell;
            //_cviRegionBox.Tag = activeCell;
            //_cviRegionBox.Visible = cursorBloc != null; // == cellBloc;
            syncRegionBox(cursorBloc);
        }
        #endregion

        #region UPDATE_FUNCTIONS
        bool isBypassIndividualNg()
        {
            bool isBypassNg = false;

            if (_mode != ScanInspectMode.NOTRAY)
            {
                if (_xRecipe.InspectParams.optChipMeasurement)
                {
                    bool optUseTotalNgPercentage = _xRecipe.InspectParams.optUseTotalNgPercentage;
                    bool optShowIndividualNG = _xRecipe.InspectParams.optShowIndividualNG;
                    if (optUseTotalNgPercentage && !optShowIndividualNG)
                    {
                        var aoiModel = GaMvcConfig.SysModel?.AoiModel;
                        if (aoiModel != null && aoiModel.IsPass)
                            isBypassNg = true;
                    }
                }
            }
            return isBypassNg;
        }
        void updateCellsToGrid(IEnumerable<XCell> cells, out EzBlocsGrid grid)
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
            grid = builder.BuildEmptyGrid(blocs, rows, cols);

            #region 將_CELL_BLOCS_填入_GRID
            foreach (CellBloc bloc in blocs)
            {
                int r = bloc.Cell.CellRow;
                int c = bloc.Cell.CellCol;
                grid.Set(r, c, bloc);

                //checkResult(bloc.Cell, out bool isPass, out bool isEmpty);
                //if (isEmpty)
                //    ((IxBlob)bloc).Bin = (int)BIN.Empty;
                //else if (isPass)
                //    ((IxBlob)bloc).Bin = (int)BIN.Pass;
                //else
                //    ((IxBlob)bloc).Bin = (int)BIN.NG;

                bloc.NonEmptyDesc = bloc.Cell.GetNoTrayDesc();
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
        }
        void updateOutGridCellBlocs(IEnumerable<XCell> cells, out IList<EzBloc> outGridBlocs)
        {
            outGridBlocs = new List<EzBloc>();
            if (_mode != ScanInspectMode.NOTRAY)
            {
                foreach (var cell in cells)
                {
                    var ogCell = cell?.OutGridLink;
                    if (ogCell == null) continue;
                    CellBloc cb = new CellBloc(ogCell);
                    outGridBlocs.Add(cb);
                }
            }
            else
            {
                foreach(var rect in _xRecipe.xOutBlocs)
                {
                    outGridBlocs.Add(new EzBloc(rect, 0));
                }
            }
        }
        void updatePassNgEmptyCount(IEnumerable<XCell> cells)
        {
            int ng = 0;
            int pass = 0;
            int empty = 0;

            if (cells == null)
            {
                updateTitle("待測中");
                return;
            }

            if (_mode == ScanInspectMode.NOTRAY)
            {
                foreach (var b in iterNonEmptyBlocs(onGrid: true))
                    if (b != null)
                        ng++;

                foreach (var b in iterNonEmptyBlocs(onGrid: false))
                    if (b != null)
                        ng++;

                foreach (var b in iterEmptyBlocs())
                    if (b != null)
                        empty++;

                updateTitle($"疑似有料= {ng}, 空位= {empty}");
            }
            else
            {
                foreach (var cell in cells)
                {
                    if (checkResult(cell, out bool isPass, out bool isEmpty))
                    {
                        if (isEmpty)
                            empty++;
                        else if (isPass)
                            pass++;
                        else
                            ng++;
                    }
                }

                updateTitle($"OK= {pass}, NG= {ng}, 疑似空位= {empty}");
            }
        }
        void updateTitle(string text)
        {
            if (lblSummaryTitle != null)
                lblSummaryTitle.Text = text;
        }
        #endregion

        #region UPDATE_DRAW_ITEMS_FUNCTIONS
        void updateDrawItems()
        {
            if (_mode == ScanInspectMode.NOTRAY)
            {
                updateDrawItems_For_EmptyTray();
            }
            else
            {
                updateDrawItems_For_ChipResults();
                //updateDrawItems_For_ChipLocate();
                //updateDrawItems_For_ChipMeasure();
            }
        }
        void updateDrawItems_For_ChipResults()
        {
            bool isBypassNg = isBypassIndividualNg();
            foreach (CellBloc bloc in iterCellBlocs())
            {
                var cell = bloc?.Cell;
                if (cell == null) continue;
                var item = new CviChipResultBox(cell, isBypassNg);
                _drawItems.Add(item);
            }
        }
        void updateDrawItems_For_EmptyTray()
        {
            var size = new SizeF(200, 200);
            foreach (var cBloc in iterEmptyBlocs())
            {
                var rect = JetEazy.Qcvt.CreateCenterRect((float)cBloc.CenterX, (float)cBloc.CenterY, size.Width, size.Height);
                var item = new CviRotRectBox(ref rect, Color.Lime, 0.10f);
                _drawItems.Add(item);
            }
            foreach (var cBloc in iterNonEmptyBlocs(onGrid: true))
            {
                var rect = JetEazy.Qcvt.CreateCenterRect((float)cBloc.CenterX, (float)cBloc.CenterY, size.Width, size.Height);
                var item = new CviRotRectBox(ref rect, Color.Red, 0.25f) { Text = "有料" };
                _drawItems.Add(item);
            }
            foreach (var cBloc in iterNonEmptyBlocs(onGrid: false))
            {
                var rect = JetEazy.Qcvt.CreateCenterRect((float)cBloc.CenterX, (float)cBloc.CenterY, size.Width, size.Height);
                var item = new CviRotRectBox(ref rect, Color.DarkOrange, 0.25f) { Text = "疑似有料" };
                _drawItems.Add(item);
            }
        }
        #endregion

        #region HELPER_FUCTIONS
        IEnumerable<EzBloc> iterNonEmptyBlocs(bool onGrid)
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
        IEnumerable<EzBloc> iterEmptyBlocs()
        {
            foreach (CellBloc bloc in _grid)
            {
                var cell = bloc?.Cell;
                if (cell != null && bloc.IsEmpty)
                    yield return bloc;
            }
        }
        IEnumerable<CellBloc> iterCellBlocs()
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
        #endregion
    }


    partial class CviCellsResultBoxes
    {
        public bool IsEmptyTrayMode
        {
            get => _mode == ScanInspectMode.NOTRAY;
        }
        public CarrierEnum ActiveCarrierID
        {
            get; set;
        }
        public ITravellerTransforms TransformsModel
        {
            get;
            set;
        }

        #region TOOL_TIP_FUNCTIONS
        void autoAdjustFetchSize()
        {
            if (_grid != null)
            {
                foreach (var bloc in _grid.IterBlocs())
                {
                    if (bloc is CellBloc cellBloc)
                    {
                        var cell = cellBloc.Cell;
                        var cellRect = Rectangle.Round(cell.viewRectF);
                        cellRect.Inflate(_xRecipe.xExtendx, _xRecipe.xExtendy);
                        base.adjustFetchSize(cellRect.Size);
                        _cviRegionBox = new CviRotRectBox(cellRect, Color.LightBlue);
                        _cviRegionBox.Visible = false;
                        return;
                    }
                }
            }
        }
        EzBloc createPseudoBloc(QVector pt, object tag)
        {
            if (pt == null)
                return null;
            int sz = 50;
            var cx = (int)pt.X;
            var cy = (int)pt.Y;
            var rect = new Rectangle(cx - sz, cy - sz, sz * 2, sz * 2);
            var pseudoBloc = new EzBloc(rect, 0, tag: tag)
            {
                Center = pt
            };
            return pseudoBloc;
        }
        protected override IEnumerable<EzBloc> iterFetchableBlocs(int camX, int camY)
        {
            if (_grid != null)
            {
                foreach (var bloc in iterCellBlocs())
                {
                    if (bloc == null)
                    {
                        //>>> DEBUG <<< System.Diagnostics.Debug.WriteLine("EMPTY_BLOC!");
                        continue;
                    }

                    //(1) Cell Bloc
                    yield return bloc;

                    var cell = (bloc as CellBloc)?.Cell;
                    var chipQuad2D = cell.ChipData?.ChipQuad2D;
                    if (chipQuad2D != null && chipQuad2D.BoundaryRect.Contains(camX, camY))
                    {
                        bool isFetched = false;

                        //(2) iterate MEASURE Points
                        var measurePts = cell?.ChipData.ChipDimension.DimMeasurePoints;
                        if (measurePts != null)
                        {
                            foreach (var pt in measurePts)
                            {
                                if (pt == null) continue;
                                var pseudoBloc = createPseudoBloc(pt, bloc);
                                yield return pseudoBloc;
                                isFetched = true;
                            }
                        }

                        //(3) iterate PAD Points
                        var padsGrid = cell?.ChipData.PadsGrid;
                        if (padsGrid != null)
                        {
                            var padBlocs = padsGrid.GetCornerBlocs();
                            foreach (var pb in padBlocs)
                            {
                                var pad = pb?.Center;
                                if (pad == null) continue;
                                var pseudoBloc = createPseudoBloc(pad, bloc);
                                yield return pseudoBloc;
                                isFetched = true;
                            }

                            if (_withPadGaps)
                            {
                                var gapMeasurePts = cell?.ChipData.PadEdgeGaps.GapMeasurePoints;
                                if (gapMeasurePts != null)
                                {
                                    foreach (var gp in gapMeasurePts)
                                    {
                                        if (gp == null) continue;
                                        var pseudoBloc = createPseudoBloc(gp, bloc);
                                        yield return pseudoBloc;
                                        isFetched = true;
                                    }
                                }
                            }
                        }

                        if (isFetched)
                            yield break;
                    }
                }
            }
        }
        protected override string composeTooltipText(EzBloc cursor, EzBloc cursor2)
        {
            //if (cursor is CellBloc cellBloc)
            //{
            //    _cviRegionBox.Quad2D.SetCenter(cellBloc.Center.X, cellBloc.Center.Y);
            //    _cviRegionBox.Visible = true;
            //}
            //else if(cursor is EzBloc bloc &&  bloc.Tag is CellBloc cb)
            //{
            //    _cviRegionBox.Quad2D.SetCenter(cb.Center.X, cb.Center.Y);
            //    _cviRegionBox.Visible = true;
            //}

            syncRegionBox(cursor);

            string txt = composeTooltipTextTrf(cursor, cursor2);
            return txt;
        }
        void syncRegionBox(EzBloc cursorBloc)
        {
            //>>> var cursorBloc = GetCursorBloc(0);
            var cellBloc = cursorBloc is CellBloc cb ? cb : cursorBloc?.Tag as CellBloc;
            var activeCell = cellBloc?.Cell;
            _cviRegionBox.Tag = activeCell;
            _cviRegionBox.Visible = cursorBloc != null; // == cellBloc;
            if (cellBloc != null)
                _cviRegionBox.Quad2D.SetCenter(cellBloc.Center.X, cellBloc.Center.Y);
        }
        #endregion

        #region DETAIL_TOOL_TIP_FUNCTIONS
        void initDefaultTrfs()
        {
            if (TransformsModel == null)
                TransformsModel = GaMvcConfig.SysModel.TransformsModel;
        }
        string composeTooltipTextTrf(EzBloc cursorBloc, EzBloc cursorBloc2)
        {
            try
            {
                if (cursorBloc == null)
                    return "";

                initDefaultTrfs();

                var cursorCellBloc = cursorBloc as CellBloc;
                var cellBloc = cursorCellBloc != null ? cursorCellBloc : cursorBloc.Tag as CellBloc;

                var cell = cellBloc?.Cell;
                if (cell == null || !checkResult(cell, out bool isPass, out bool isEmpty))
                    return null;

                int row = cell.CellRow;
                int col = cell.CellCol;

                bool isShowScore = true;
                var sb = new StringBuilder();

                if (cell != null)
                {
                    sb.Append("格點(").Append(cell.Index).Append(") : [").AppendValues(row, col).AppendLine("]");
                }

                appendCameraCoords(sb, cursorBloc, cursorBloc2);

                if (!IsEmptyTrayMode)
                {
                    if (TransformsModel != null)
                    {
                        if (cursorCellBloc != null)
                            appendDetailCoordsInfo(sb, row, col, cursorBloc, cursorBloc2);
                        else
                            appendCursorsDistInfo(sb, cursorBloc, cursorBloc2);
                        isShowScore = false;
                    }
                }

                if (isShowScore || IsEmptyTrayMode)
                {
                    sb.AppendLine();
                    sb.AppendLine($"Score= {cursorBloc.Score:0.00}");
                    sb.AppendLine($"Size= {cursorBloc.Rect.Width}x{cursorBloc.Rect.Height}");
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        void appendCameraCoords(StringBuilder sb, EzBloc bloc, EzBloc bloc2)
        {
            var camPt = getCentroid(bloc);
            if (camPt == null)
                return;

            sb.Append($"相機座標: ({camPt.X:0.0}, {camPt.Y:0.0})").AppendLine();

            if (bloc != null && bloc2 != null && bloc != bloc2)
            {
                var camPt2 = getCentroid(bloc2);
                if (camPt2 != null)
                {
                    var dv = camPt - camPt2;
                    var dist = dv.NormLength;
                    sb.AppendLine($"相機座標 dX = {dv.X:0.0} pix");
                    sb.AppendLine($"相機座標 dY = {dv.Y:0.0} pix");
                    sb.AppendLine($"相機座標 距離 = {dist:0.0} pix");
                }
            }
        }
        void appendDetailCoordsInfo(StringBuilder sb, int row, int col, EzBloc bloc, EzBloc bloc2)
        {
            if (TransformsModel == null)
                return;

            (var err, var errMsg) = TransformsModel.GetNodeCoords(ActiveCarrierID, row, col, out var _, out var world_target, out var s1_target, out var s2_target);
            if (err != Model.ErrorCodes.OK)
                return;

            QVector world_current = null;
            QVector s1_current = null;
            QVector s2_current = null;

            var camPt = getCentroid(bloc);
            if (camPt != null)
            {
                var tranCM1 = TransformsModel.GetCameraMotorTransform(ActiveCarrierID, SuckerRowEnum.S1);
                var tranCM2 = TransformsModel.GetCameraMotorTransform(ActiveCarrierID, SuckerRowEnum.S2);
                var tranCP = TransformsModel.GetCameraPhysicTransform(ActiveCarrierID);
                s1_current = tranCM1.Trans(camPt);
                s2_current = tranCM2.Trans(camPt);
                world_current = tranCP.Trans(camPt);
            }

            #region S1_S2_馬達座標
            if (!_withPadGaps)
            {
                sb.AppendLine().Append("S1 馬達目標(X,Y) = (").AppendValues((float)s1_target.X, (float)s1_target.Y).Append(") mm");
                if (camPt != null)
                    sb.AppendLine().Append("S1 馬達座標(X,Y) = (").AppendValues((float)s1_current.X, (float)s1_current.Y).Append(") mm");

                sb.AppendLine().Append("S2 馬達目標(X,Y) = (").AppendValues((float)s2_target.X, (float)s2_target.Y).Append(") mm");
                if (camPt != null)
                    sb.AppendLine().Append("S2 馬達座標(X,Y) = (").AppendValues((float)s2_current.X, (float)s2_current.Y).Append(") mm");
            }
            #endregion

            #region PHYSIC_目標座標
            sb.AppendLine();
            sb.AppendLine().Append("Physic 目標(X,Y) = (").AppendValues((float)world_target.X, (float)world_target.Y).Append(") mm");
            if (camPt != null)
                sb.AppendLine().Append("Physic 座標(X,Y) = (").AppendValues((float)world_current.X, (float)world_current.Y).Append(") mm");
            #endregion

            #region 變動值
            if (camPt != null)
            {
                var world_delta = world_current - world_target;
                var motor_delta = s1_current - s1_target;
                sb.AppendLine();
                sb.AppendLine($"Physic 變動值 ΔX = {world_delta.X:0.000} mm");
                sb.AppendLine($"Physic 變動值 ΔY = {world_delta.Y:0.000} mm");
                if (!_withPadGaps)
                {
                    sb.AppendLine();
                    sb.AppendLine($"PLC 格點 補償量 ΔX = {motor_delta.X:0.000} mm");
                    sb.AppendLine($"PLC 格點 補償量 ΔY = {motor_delta.Y:0.000} mm");
                }
            }
            #endregion

            if (bloc != null && bloc2 != null && bloc != bloc2)
            {
                #region 量測兩點距離
                var camPt2 = getCentroid(bloc2);
                if (camPt2 != null)
                {
                    var transCP = TransformsModel.GetCameraPhysicTransform(ActiveCarrierID);
                    var world_last = transCP.Trans(camPt2);
                    var dv = world_current - world_last;
                    double dist = dv.NormLength;
                    sb.AppendLine();
                    sb.AppendLine($"Physic 座標 DX = {dv.X:0.000} mm");
                    sb.AppendLine($"Physic 座標 DY = {dv.Y:0.000} mm");
                    sb.AppendLine($"Physic 座標 距離 = {dist:0.000} mm");
                }
                #endregion
            }
            else
            {
                var cell = (bloc as CellBloc)?.Cell;
                if (cell != null)
                {
                    #region 最後補償量
                    sb.AppendLine();
                    sb.AppendLine($"RunX = {cell.RunX:0.000} mm");
                    sb.AppendLine($"RunY = {cell.RunY:0.000} mm");
                    sb.AppendLine($"Angle = {cell.RunAngle:0.00}°");
                    #endregion

                    if (_xRecipe.InspectParams.optTiltDetectEnabled)
                    {
                        #region 傾斜(踩腳)
                        var chipCoords = cell?.ChipData?.ChipCoords;
                        if (chipCoords != null)
                        {
                            sb.AppendLine();
                            sb.AppendLine($"傾斜(踩腳)程度 = {chipCoords.TiltRatio:0.000}");
                        }
                        #endregion
                    }

                    if (_xRecipe.InspectParams.optChipMeasurement)
                    {
                        #region 尺寸量測結果
                        var cW = _xRecipe.InspectParams.mWidthStand;
                        var cH = _xRecipe.InspectParams.mHeightStand;
                        var dx = Math.Round(cell.RunWidth - cW, 3);
                        var dy = Math.Round(cell.RunHeight - cH, 3);
                        sb.AppendLine().Append($"晶粒.寬 = {cell.RunWidth:0.000} mm").Append($" (Δ = {dx:0.000} mm)");
                        sb.AppendLine().Append($"晶粒.高 = {cell.RunHeight:0.000} mm").Append($" (Δ = {dy:0.000} mm)");
                        #endregion

                        #region 尺寸量測詳細點位
                        var chipDim = cell?.ChipData?.ChipDimension;
                        if (chipDim != null && chipDim.GetPixelSize(out double dpX, out double dpY))
                        {
                            sb.AppendLine();
                            sb.AppendLine().Append($"晶粒.寬 = {dpX:0.0} pix");
                            sb.AppendLine().Append($"晶粒.高 = {dpY:0.0} pix");
                        }
                        #endregion

                        #region 晶粒_PAD_跨距
                        var padsGrid = cell?.ChipData?.PadsGrid;
                        if (getChapPadsSpan(padsGrid, out double padSpanW, out double padSpanH))
                        {
                            sb.AppendLine();
                            sb.AppendLine().Append($"PAD.跨距.寬 = {padSpanW:0.0} pix");
                            sb.AppendLine().Append($"PAD.跨距.高 = {padSpanH:0.0} pix");
                        }
                        #endregion

                        #region PAD_邊隙
                        if (_withPadGaps)
                        {
                            var gaps = cell?.ChipData?.PadEdgeGaps;
                            if (gaps != null)
                            {
                                sb.AppendLine();
                                sb.AppendLine().Append($"LUX = {gaps.LU.X:0.000} mm");
                                sb.AppendLine().Append($"RUX = {gaps.RU.X:0.000} mm");
                                sb.AppendLine().Append($"RDX = {gaps.RD.X:0.000} mm");
                                sb.AppendLine().Append($"LDX = {gaps.LD.X:0.000} mm");
                                sb.AppendLine();
                                sb.AppendLine().Append($"LUY = {gaps.LU.Y:0.000} mm");
                                sb.AppendLine().Append($"RUY = {gaps.RU.Y:0.000} mm");
                                sb.AppendLine().Append($"RDY = {gaps.RD.Y:0.000} mm");
                                sb.AppendLine().Append($"LDY = {gaps.LD.Y:0.000} mm");
                            }
                        }
                        #endregion
                    }
                }
            }
        }
        void appendCursorsDistInfo(StringBuilder sb, EzBloc bloc, EzBloc bloc2)
        {
            if (TransformsModel == null)
                return;


            QVector world_current = null;

            var camPt = bloc.Center;
            if (camPt != null)
            {
                var tranCP = TransformsModel.GetCameraPhysicTransform(ActiveCarrierID);
                world_current = tranCP.Trans(camPt);
                sb.AppendLine().Append("Physic 座標(X,Y) = (").AppendValues((float)world_current.X, (float)world_current.Y).Append(") mm");
            }

            if (bloc != null && bloc2 != null && bloc != bloc2)
            {
                var camPt2 = bloc2.Center;
                if (camPt2 != null)
                {
                    var transCP = TransformsModel.GetCameraPhysicTransform(ActiveCarrierID);
                    var world_last = transCP.Trans(camPt2);
                    var dv = world_current - world_last;
                    double dist = dv.NormLength;
                    sb.AppendLine();
                    sb.AppendLine($"Physic 座標 DX = {dv.X:0.000} mm");
                    sb.AppendLine($"Physic 座標 DY = {dv.Y:0.000} mm");
                    sb.AppendLine($"Physic 座標 距離 = {dist:0.000} mm");
                }
            }
        }
        static QVector getCentroid(EzBloc bloc)
        {
            var cell = (bloc as CellBloc)?.Cell;

            //if (cell == null)
            //    return bloc?.Center;
            //var cx = cell.xFindResult.fCenterX;
            //var cy = cell.xFindResult.fCenterY;
            //if (cx > 0 && cy > 0)
            //    return new QVector(cx, cy);
            //return bloc.Center;

            var chipQuad = cell?.ChipData?.ChipQuad2D;
            if (chipQuad != null)
                return chipQuad.Center;
            return bloc?.Center;
        }
        #endregion

        #region MENU_STRIP_FUNCTIONS
        Form _frmOwner;
        ContextMenuStrip _menuStrip;
        void initMenuStrip(Control wnd)
        {
            if (_menuStrip == null)
            {
                _frmOwner = wnd.FindForm();
                wnd.HandleDestroyed += (s, e) => disposeMenuStrip();

                var menuRcps = new[]
                {
                    new ToolStripMenuItem("參數: 設定 晶粒 標準尺寸"),
                    new ToolStripMenuItem("參數: 設定 晶粒 PAD 門限值"),
                };
                var menuDumps = new[]
                {
                    //new ToolStripMenuItem("Dump 尺寸量測結果 (in row)"),
                    new ToolStripMenuItem("調試: 複製 單一晶粒 尺寸量測結果"),
                    new ToolStripMenuItem("調試: 輸出 單一區域 圖像檔案"),
                    new ToolStripMenuItem("調試: 輸出 所有區域 圖像檔案"),
                    new ToolStripMenuItem("調試: 顯示 晶粒定位 所有演算圖像"),
                };

                int i = 0;
                menuRcps[i++].Click += (s, e) => OpenGoldenDimensionEditorDlg();
                menuRcps[i++].Click += (s, e) => OpenPadsFiltersEditorDlg();

                int k = 0;
                menuDumps[k++].Click += (s, e) => CopyOneChipDimsToClipboard();
                menuDumps[k++].Click += (s, e) => DumpOneCellRegion(wnd);
                menuDumps[k++].Click += (s, e) => DumpCellRegions(wnd);
                menuDumps[k++].Click += (s, e) => DebugMatching(null);

                _menuStrip = new ContextMenuStrip();
                _menuStrip.Items.AddRange(menuRcps);
                _menuStrip.Items.Add(new ToolStripSeparator());
                _menuStrip.Items.AddRange(menuDumps);

#if (OPT_USE_AI)
                var menuAI = new ToolStripMenuItem("AI: 生成 AI 訓練數據");
                menuAI.Click += (s, e) => GenAiData();
                _menuStrip.Items.Add(new ToolStripSeparator());
                _menuStrip.Items.Add(menuAI);
#endif
            }
        }
        void disposeMenuStrip()
        {
            _menuStrip?.Dispose();
            _menuStrip = null;
        }
        void popupMenuStrip(Control wnd, Point pt)
        {
            var activeCellBloc = _cursorBloc as CellBloc;
            if (activeCellBloc == null) return;

            _cursorBloc2 = null;
            _toolTip.Hide(wnd);

            if (_mode == ScanInspectMode.NOTRAY)
                return;

            initMenuStrip(wnd);
            _menuStrip.Show(wnd, pt);
        }
        #endregion

        #region DUMP_FUNCTIONS
        static string PATH_DUMP => "d:\\paso.log\\chipLoc";
        bool checkPrivilege()
        {
            var account = Traveller106.Universal.ACCDB.AccNow;
            if (account == null || !account.IsAllowSetupRecipe)
            {
                //VsMessageBox.Warning("請先登入 擁有修改參數權限 的 帳號!");
                VsMessageBox.Warning(GaUtil.GetEnumDescription(Prompts.No_Privilege));
                return false;
            }
            return true;
        }
        void DebugMatching_000(CellBloc cellBloc = null)
        {
#if (OPT_OLD_CODE)
            if (IsEmptyTrayMode)
                return;

            if (cellBloc == null)
                cellBloc = _cursorBloc as CellBloc;
            
            var cell = cellBloc?.Cell;
            if (cell == null) return;

            if (!checkPrivilege())
                return;

            // (0) DEBUG OPIONS
            EzPadsGridFinder.VISUAL_DEBUG = true;
            VxDebugDrawer.OPT_USE_OPENCV_WINDOW = false;

            // (1) ImageHolder
            var lineScanImageHolder = GaMvcConfig.SysModel.LineScanImageHolder;
            //string srcName = lineScanImageHolder.SrcName;

            // (2) Directory
            string dstPath = System.IO.Path.Combine(PATH_DUMP, "dumpOne");
            JetEazy.IO.QxPathUtility.InitDirectory(dstPath);
            string fname = $"{cell.Index}@{cell.CellRow}_{cell.CellCol}.png";
            string dumpFile = System.IO.Path.Combine(dstPath, fname);

            // (3) Thresh and Golden
            var goldenBmp = _xRecipe.bmpprinttemplate;

            // (4) Matcher
            var matcher = new EzRigidBodyGridMatcher(shrink: 1);
            matcher.PadThreshold = _xRecipe.InspectParams.xGridPadThreshold;                //<<< PadThresh 要先設定, 才能取 Golden
            matcher.DistTransThreshold = _xRecipe.InspectParams.xDistTransThreshold;
            matcher.LargeAngleEnabled = _xRecipe.InspectParams.xUseLargePadGridAngle;
            matcher.SetGoldenTemplate(goldenBmp);

            // (5) 測試資料
            var fullfovBmp = lineScanImageHolder.PeekBitmap();
            using (var bridge = new QxImageBridge(fullfovBmp))
            {
                // (5.1) ROI
                var cellRect = Rectangle.Round(cell.viewRectF);
                cellRect.Inflate(_xRecipe.xExtendx, _xRecipe.xExtendy);
                GaUtil.Clip(ref cellRect, fullfovBmp.Size);
                var roi = JetEazy.Qcvt.CV(cellRect);

                // (5.2) Crop
                Mat imgRegion = bridge.Image[roi];

                var bestResult = matcher.FindBestMatch(imgRegion, dumpFile);

                if (bestResult != null)
                {
                    var bestGrid = bestResult.Grid;
                    //var box2d = bestResult.CalcBox2D();
                    //>>> EzPadsGridFinder.FindSpecialKeyPad(bestGrid, out int kr, out int kc, out int px);
                    //>>> EzPadsGridFinder.FindSpecialKeyPad(imgScene, bestGrid, out int kr, out int kc, out double kSQ);
                    int kr = bestResult.KeyRow;
                    int kc = bestResult.KeyCol;
                    var kSQ = bestResult.KeySQRatio;
                    VxDebugDrawer.Draw(imgRegion, (QvBox2D)null, bestGrid, kr, kc, Scalar.Lime, $"Best Grid [{bestGrid.Rows}x{bestGrid.Cols}] = {bestGrid.GetMajorCount()} @ {fname}");
                }
            }

            // (6) Turn Off VISUAL_DEBUG
            EzPadsGridFinder.VISUAL_DEBUG = false;
#endif
        }
        void DebugMatching_001(CellBloc cellBloc = null)
        {
#if (OPT_OLD_CODE)
            if (IsEmptyTrayMode)
                return;

            if (cellBloc == null)
                cellBloc = _cursorBloc as CellBloc;

            var cell = cellBloc?.Cell;
            if (cell == null) return;

            if (!checkPrivilege())
                return;

            // (0) DEBUG OPIONS
            EzPadsGridFinder.VISUAL_DEBUG = true;
            VxDebugDrawer.OPT_USE_OPENCV_WINDOW = false;

            // (1) ImageHolder
            var lineScanImageHolder = GaMvcConfig.SysModel.LineScanImageHolder;

            // (2) Bitmap
            var fullfovBmp = lineScanImageHolder.PeekBitmap();

            // (3) ROI
            var roi = Rectangle.Round(cell.viewRectF);
            roi.Inflate(_xRecipe.xExtendx, _xRecipe.xExtendy);
            GaUtil.Clip(ref roi, fullfovBmp.Size);

            using (var bmpCrop = fullfovBmp.Clone(roi, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
            {
                //(4) AOI
                var aoiModel = GaMvcConfig.SysModel.AoiModel;
                var cropRect = new RectangleF(0, 0, bmpCrop.Width, bmpCrop.Height);
                aoiModel.LocateOneChip(bmpCrop, cropRect, out var chipData);

                //(5) Draw
                if (chipData != null && chipData.DebugRigidBodyData is EzRigidBodyGridMatcher.RigidBody rig)
                {
                    using (var bridge = new QxImageBridge(bmpCrop))
                    {
                        string fname = $"{cell.Index}@{cell.CellRow}_{cell.CellCol}.png";
                        var padsGrid = chipData.PadsGrid;
                        var chipQuad = chipData.ChipQuad2D;
                        //>>> EzPadsGridFinder.FindSpecialKeyPad(bestGrid, out int kr, out int kc, out int px);
                        //>>> EzPadsGridFinder.FindSpecialKeyPad(imgScene, bestGrid, out int kr, out int kc, out double kSQ);
                        int kr = rig.KeyRow;
                        int kc = rig.KeyCol;
                        var kSQ = rig.KeySQRatio;
                        var imgRegion = bridge.Image;
                        VxDebugDrawer.Draw(imgRegion, chipQuad, padsGrid, kr, kc, Scalar.Lime, $"Best Grid [{padsGrid.Rows}x{padsGrid.Cols}] = {padsGrid.GetMajorCount()} @ {fname}");
                    }
                }
            }

            // (6) Turn Off VISUAL_DEBUG
            EzPadsGridFinder.VISUAL_DEBUG = false;
#endif
        }
        void DebugMatching(CellBloc cellBloc = null)
        {
            if (IsEmptyTrayMode)
                return;

            if (cellBloc == null)
                cellBloc = _cursorBloc as CellBloc;

            var cell = cellBloc?.Cell;
            if (cell == null) return;

            if (!checkPrivilege())
                return;

            // (0) DEBUG OPIONS
            EzPadsGridFinder.VISUAL_DEBUG = true;
            VxDebugDrawer.OPT_USE_OPENCV_WINDOW = false;

            // (1) ImageHolder
            var lineScanImageHolder = GaMvcConfig.SysModel.LineScanImageHolder;

            // (2) Bitmap
            var fullfovBmp = lineScanImageHolder.PeekBitmap();

            // (3) ROI
            //>>> var cellRoi = Rectangle.Round(cell.viewRectF);
            //>>> cellRoi.Inflate(_xRecipe.xExtendx, _xRecipe.xExtendy);
            var cellRoi = Rectangle.Round(_cviRegionBox.Quad2D.BoundaryRect);
            GaUtil.Clip(ref cellRoi, fullfovBmp.Size);

            using (var cellBmp = fullfovBmp.Clone(cellRoi, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
            {
                //(4) AOI
                var aoiModel = GaMvcConfig.SysModel.AoiModel;
                bool ok = aoiModel.TryRunOneChip(cell, cellBmp, cellRoi);

                //(5) Draw
                var chipData = cell.ChipData;
                if (ok && chipData != null && chipData.DebugRigidBodyData is EzRigidBodyGridMatcher.RigidBody rig)
                {
                    using (var bridge = new QxImageBridge(cellBmp))
                    {
                        var chipQuad = chipData.ChipQuad2D?.Clone();
                        var padsGrid = chipData.PadsGrid;
                        if (padsGrid != null)
                        {
                            int kr = rig.KeyRow;
                            int kc = rig.KeyCol;
                            var kSQ = rig.KeySQRatio;
                            var imgRegion = bridge.Image;
                            string fname = $"{cell.Index}@{cell.CellRow}_{cell.CellCol}.png";
                            chipQuad.Offset(-cellRoi.X, -cellRoi.Y);
                            padsGrid.Offset(-cellRoi.X, -cellRoi.Y);
                            VxDebugDrawer.Draw(imgRegion, chipQuad, padsGrid, kr, kc, Scalar.Lime, $"Best Grid [{padsGrid.Rows}x{padsGrid.Cols}] = {padsGrid.GetMajorCount()} @ {fname}");
                            padsGrid.Offset(cellRoi.X, cellRoi.Y);
                        }
                    }
                }
            }

            // (6) Turn Off VISUAL_DEBUG
            EzPadsGridFinder.VISUAL_DEBUG = false;
        }
        void DebugMatchingOff()
        {
            VxDebugDrawer.DestroyAllWindows();
            EzPadsGridFinder.VISUAL_DEBUG = false;
        }
        void DumpCellRegions(Control viewer, IEnumerable<XCell> xRegionCells = null)
        {
            if (xRegionCells == null)
                xRegionCells = _xRecipe.xRegionCells;

            var oldCursor = GaUtil.SetCursor(viewer, Cursors.AppStarting);

            EzPadsGridFinder.VISUAL_DEBUG = false;

            // (0) ImageHolder
            var lineScanImageHolder = GaMvcConfig.SysModel.LineScanImageHolder;
            string srcName = lineScanImageHolder.SrcName;

            // (1) Directory
            string dstPath = PATH_DUMP;
            if (srcName != null)
                dstPath = System.IO.Path.Combine(dstPath, System.IO.Path.GetFileNameWithoutExtension(srcName));
            JetEazy.IO.QxPathUtility.InitDirectory(dstPath);

            // (2) Golden
            var goldenBmp = _xRecipe.bmpprinttemplate;
            goldenBmp?.Save(System.IO.Path.Combine(dstPath, "0_golden.png"));

            // (3) LOOP region cells

            var extendX = _xRecipe.xExtendx;
            var extendY = _xRecipe.xExtendy;
            var fullfovBmp = lineScanImageHolder.PeekBitmap();

            using (var bridge = new QxImageBridge(fullfovBmp))
            {
                Mat fullfovImg = bridge.Image;

                foreach (var cell in xRegionCells)
                {
                    // ROI
                    var cellRect = Rectangle.Round(cell.viewRectF);
                    cellRect.Inflate(_xRecipe.xExtendx, _xRecipe.xExtendy);
                    GaUtil.Clip(ref cellRect, fullfovBmp.Size);
                    var roi = JetEazy.Qcvt.CV(cellRect);

                    // Save the crop
                    string fileName = System.IO.Path.Combine(dstPath, $"{cell.Index}@{cell.CellRow}_{cell.CellCol}.png");
                    Mat imgRegion = bridge.Image[roi];
                    imgRegion.SaveImage(fileName);
                }
            }

            MessageBox.Show($"已存入 Region Cell Images 至\n\r{dstPath}", "DEBUG", MessageBoxButtons.OK, MessageBoxIcon.Information);

            GaUtil.SetCursor(viewer, oldCursor);
        }
        void DumpOneCellRegion(Control viewer)
        {
            var cellBloc = _cursorBloc as CellBloc;
            var targetCell = cellBloc?.Cell;
            if (targetCell == null) return;
            DumpCellRegions(viewer, new[] { targetCell });
        }
        void CopyOneChipDimsToClipboard()
        {
            try
            {
                var cellBloc = _cursorBloc as CellBloc;
                var cell = cellBloc?.Cell;
                var chipData = cell?.ChipData;
                string textToCopy;
                if (chipData != null)
                {
                    var sb = new StringBuilder();

                    // header
                    sb.AppendLine("Row, Col, Width, Height, PixWidth, PixHeight, PadsSpanW, PadsSpanH");

                    // row, col
                    sb.AppendValues(cell.CellRow, cell.CellCol);

                    // runWidth, runHeight
                    sb.Append(", ").AppendValues(cell.RunWidth, cell.RunHeight);

                    // pixWidth, pixHeight
                    var chipDim = cell?.ChipData?.ChipDimension;
                    if (chipDim != null && chipDim.GetPixelSize(out double dpX, out double dpY))
                    {
                        sb.Append(", ").Append($"{dpX:0.0}");
                        sb.Append(", ").Append($"{dpY:0.0}");
                    }

                    // padSpanWidth, padSpanHeight
                    var padsGrid = cell?.ChipData?.PadsGrid;
                    if (getChapPadsSpan(padsGrid, out double padSpanW, out double padSpanH))
                    {
                        sb.Append(", ").Append($"{padSpanW:0.0}");
                        sb.Append(", ").Append($"{padSpanH:0.0}");
                    }

                    // total text
                    textToCopy = sb.ToString();
                }
                else
                {
                    textToCopy = "";
                }

                //System.Windows.Forms.Clipboard.SetText(text);
                System.Windows.Forms.Clipboard.SetDataObject(textToCopy, true, 10, 200);
            }
            catch (System.Exception ex)
            {
                // 處理可能發生的錯誤
                // Console.WriteLine("複製到剪貼簿時發生錯誤: " + ex.Message);
                // 可以在WPF中使用MessageBox或其他方式提示用戶
                //VsMessageBox.Warning("複製到剪貼簿失敗: " + ex.Message);
                MessageBox.Show("複製到剪貼簿失敗: " + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        void GenAiData()
        {
#if (OPT_USE_AI)
            if (IsEmptyTrayMode)
                return;

            if (!checkPrivilege())
                return;

            var cellBloc = _cursorBloc as CellBloc;
            var cell = cellBloc?.Cell;
            var chipDim = cell?.ChipData?.ChipDimension;

            // (0) 尺寸量測結果為 PASS 才納入 AI 訓練數據
            if (chipDim == null || !chipDim.IsAllPass())
                return;

            // (1) ImageHolder
            var lineScanImageHolder = GaMvcConfig.SysModel.LineScanImageHolder;

            // (2) Bitmap
            var fullfovBmp = lineScanImageHolder.PeekBitmap();

            //// (3) 
            var aiDataCropper = new LaserAlignDX.AoiModel.AI.AiDataCropper();
            aiDataCropper.GenerateData(lineScanImageHolder.SrcName, fullfovBmp, _xRecipe.xRegionCells);
            MessageBox.Show("AI 訓練數據 生成於\n\r" + aiDataCropper.PATH_DATA_ROOT, "AI Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
#endif
        }
        bool getChapPadsSpan(EzBlocsGrid padsGrid, out double spanWidth, out double spanHeight, int digits = 1)
        {
            spanWidth = 0;
            spanHeight = 0;
            if (padsGrid != null)
            {
                //------------------------------------------------------------------------------------------
                // 0 1
                // 3 2
                //------------------------------------------------------------------------------------------
                var pts = Array.ConvertAll(padsGrid.GetCornerBlocs(), pb => pb?.Center);
                if (pts[0] != null && pts[1] != null && pts[2] != null && pts[3] != null)
                {
                    var L = (pts[0] + pts[3]) / 2;
                    var R = (pts[1] + pts[2]) / 2;
                    var T = (pts[0] + pts[1]) / 2;
                    var B = (pts[3] + pts[2]) / 2;
                    spanWidth = Math.Round((R - L).NormLength, digits);
                    spanHeight = Math.Round((B - T).NormLength, digits);
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region DEBUG_RESERVED_CODE
#if (OPT_RESERVED)
        void DumpChipDimsInCol(int col)
        {
            if (col < 0 && _cursorBloc is CellBloc cb && cb.Cell != null)
                col = cb.Cell.CellCol;

            if (_grid == null || col < 0 || col >= _grid.Cols)
                return;

            var sb = new StringBuilder();
            for (int r = 0; r < _grid.Rows; r++)
            {
                cb = _grid.Get(r, col) as CellBloc;
                var cell = cb?.Cell;
                if (cell == null) continue;
                var w = cell.RunWidth;
                var h = cell.RunHeight;
                if (w > 0 && h > 0)
                    sb.Append(r).Append(",").AppendValues(w, h).AppendLine();
            }

            if (!System.IO.Directory.Exists(PATH_DUMP))
                System.IO.Directory.CreateDirectory(PATH_DUMP);

            string fileName = System.IO.Path.Combine(PATH_DUMP, $"measure_dims_in_col_{col:00}.csv");
            GaUtil.SaveData(sb.ToString(), fileName);
            VsMessageBox.Info($"已保存 尺寸數據 至 {fileName}");
        }
        void DumpChipDimsInRow(int row)
        {
            if (row < 0 && _cursorBloc is CellBloc cb && cb.Cell != null)
                row = cb.Cell.CellRow;

            if (_grid == null || row < 0 || row >= _grid.Cols)
                return;

            var sb = new StringBuilder();
            for (int c = 0; c < _grid.Cols; c++)
            {
                cb = _grid.Get(row, c) as CellBloc;
                var cell = cb?.Cell;
                if (cell == null) continue;
                var w = cell.RunWidth;
                var h = cell.RunHeight;
                if (w > 0 && h > 0)
                    sb.Append(c).Append(",").AppendValues(w, h).AppendLine();
            }

            if (!System.IO.Directory.Exists(PATH_DUMP))
                System.IO.Directory.CreateDirectory(PATH_DUMP);

            string fileName = System.IO.Path.Combine(PATH_DUMP, $"measure_dims_in_row_{row:00}.csv");
            GaUtil.SaveData(sb.ToString(), fileName);
            VsMessageBox.Info($"已保存 尺寸數據 至 {fileName}");
        }
#endif
        #endregion

        #region RECIPE_DIALOG_FUNCTIONS
        void OpenGoldenDimensionEditorDlg()
        {
            var cell = (_cursorBloc as CellBloc)?.Cell;
            if (cell == null) return;

            if (!checkPrivilege())
                return;

            using (var dlg = new FormChipTemplateDim())
            {
                dlg.ActiveCell = cell;
                dlg.ShowDialog(_frmOwner);
            }
        }
        void OpenPadsFiltersEditorDlg()
        {
            // REV_2026-03-09 整合海康 Template Match
            if (_xRecipe.InspectParams.xAlgorithm == MatchAlgorithmEnum.TemplateMatch)
            {
                var msg = GaUtil.GetEnumDescription(MatchAlgorithmEnum.TemplateMatch) + "\n\r" 
                        + GaUtil.GetEnumDescription(Prompts.Warn_the_function_is_not_supported);
                VsMessageBox.Warning(msg);
                return;
            }

            var cell = (_cursorBloc as CellBloc)?.Cell;
            if (cell == null) return;

            if (!checkPrivilege())
                return;

            // (0) ImageHolder
            var lineScanImageHolder = GaMvcConfig.SysModel.LineScanImageHolder;
            Bitmap fullfovBmp = lineScanImageHolder.PeekBitmap();

            // (1) ROI
            var roi = Rectangle.Round(cell.viewRectF);
            roi.Inflate(_xRecipe.xExtendx, _xRecipe.xExtendy);
            GaUtil.Clip(ref roi, fullfovBmp.Size);

            // (2) DialogBox and Bmp
            using (var dlg = new FormPadThresholdsEditor())
            {
                Bitmap bmp = fullfovBmp.Clone(roi, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                dlg.SetSrcImage(bmp, true);
                dlg.ShowDialog(_frmOwner);
            }
        }
        #endregion
    }
}
