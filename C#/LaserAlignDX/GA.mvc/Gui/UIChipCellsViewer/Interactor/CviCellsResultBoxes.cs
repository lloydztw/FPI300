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

using JetEazy.ImageViewerEx;
using JetEazy.Match;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model;
using LaserAlignDX.OPSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VisionDesigner;

using CELL = LaserAlignDX.OPSpace.RegionCellX3Class;
using InspectParams = LaserAlignDX.OPSpace.RecipeSpace.InspectX3ParaClass;
using RECIPE = LaserAlignDX.OPSpace.RecipeSpace.RecipeFPIX3Class;

namespace LaserAlignDX.UISpace.ChipCellsViewer
{
    public class CviCellsResultBoxes : CvImageViewerInteractor
    {
        #region INNER_CLASS
        class CellBloc : EzBloc
        {
            public CellBloc(CELL cell, RectangleF rect) : base(Rectangle.Round(rect), 1)
            {
                Cell = cell;
            }
            public CellBloc(CELL cell) : base(Rectangle.Empty, 1)
            {
                Cell = cell;
                var rcf = cell.DrawResultRectF();
                Rect = Rectangle.Round(GaImageUtil.ToRectangleF(rcf));
                Center = new JetEazy.QMath.QVector(rcf.CenterX, rcf.CenterY);
            }
            public CELL Cell
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
        RECIPE _xRecipe
        {
            get => RECIPE.Instance;
        }
        InspectParams _inspectParams
        {
            get => InspectParams.Instance;
        }
        #endregion
        
        #region PRIVATE_DATA
        ScanInspectMode _mode;
        EzBlocsGrid _grid;
        IList<EzBloc> _outGridBlocs;
        #endregion

        #region GUI_MEMBERS
        IxDispTextFormatter _formatter = new MainDispTextFormatter();
        ToolTip _toolTip = new ToolTip();
        Font _font = null;
        #endregion

        #region GUI_DRAW_ITEMS
        //List<CviRotRectBox> _boxes = new List<CviRotRectBox>();
        //List<CviLineSegmentsBox> _lineSegBoxes = new List<CviLineSegmentsBox>();
        List<IvDrawItem> _drawItems = new List<IvDrawItem>();
        #endregion

        public Control lblSummaryTitle
        {
            get;
            set;
        }

        public void Reset()
        {
            _grid?.Dispose();
            _grid = null;
            _outGridBlocs = null;
            _drawItems.Clear();
        }
        public void UpdateResult(IEnumerable<CELL> cells, int mode)
        {
            _mode = (ScanInspectMode)mode;

            Reset();

            updateCellGrid(cells, out _grid);

            if (_mode == ScanInspectMode.NOTRAY)
            {
                updateOutGridBlocs(_xRecipe.xOutBlocs, out _outGridBlocs);
            }

            updatePassNgEmptyCount(cells);
            updateDrawItems();
        }

        #region OVERRIDES
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            if (_font == null)
                _font = viewer.Font;

            if (_grid == null && _outGridBlocs == null)
                return;

            bool isWorld = viewer.IsInWorldCoordinate();
            if (!isWorld)
                viewer.SwitchToWorldCoordinate(gxView);

            foreach(var item in _drawItems) 
                item.OnDraw(viewer, gxView);

            draw_All_QrCodes(viewer, gxView);

            if (!isWorld)
                viewer.SwitchToViewportCoordinate(gxView);
        }
        public override bool OnMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            return handleMouseMove(viewer, e);
        }
        #endregion

        #region PRIVATE_TOOL_TIP_FUNCTIONS
        Point _hitPt = new Point();
        Size _fetchSize = new Size(100, 100);
        bool handleMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            if ((_grid != null) && Visible && Enabled)
            {
                int xx = e.X;
                int yy = e.Y;

                viewer.TransViewportToWorld(ref xx, ref yy);
                var boundary = viewer.GetWorldRect();
                _fetchSize.Width = (int)Math.Max(100, boundary.Width / 50);
                _fetchSize.Height = (int)Math.Max(100, boundary.Height / 50);

                var bloc = fetchOne(xx, yy) as CellBloc;
                updateTooltip(bloc, e.X, e.Y, viewer);
            }
            return false;
        }
        EzBloc fetchOne(int x, int y)
        {
            if (_grid != null)
            {
                var blobs = fetchKNN(x, y, 1, _fetchSize, _grid.IterBlocs());
                if (blobs != null && blobs.Length > 0)
                    return blobs[0];
            }
            //if (_suckerBlocs != null)
            //{
            //    var blobs = fetchKNN(x, y, 1, _fetchSize, _suckerBlocs);
            //    if (blobs != null && blobs.Length > 0)
            //        return blobs[0];
            //}
            //if (_outGridBlocs != null)
            //{
            //    var blobs = fetchKNN(x, y, 1, _fetchSize, _outGridBlocs);
            //    if (blobs != null && blobs.Length > 0)
            //        return blobs[0];
            //}
            return null;
        }
        EzBloc[] fetchKNN(int x, int y, int kNumber, Size range, IEnumerable<EzBloc> srcBlobs)
        {
            bool needsToSort = (kNumber >= 0);

            if (kNumber <= 0)
            {
                // Get All
                kNumber = int.MaxValue;
            }

            //if (range == Size.Empty)
            //{
            //    range = m_sizeCell;
            //}

            Rectangle rectRange = new Rectangle(
                    x - range.Width / 2,
                    y - range.Height / 2,
                    range.Width,
                    range.Height
                );


            var knn = new List<KeyValuePair<EzBloc, int>>();

            foreach (var spot in srcBlobs)
            {
                if (spot == null)
                    continue;

                //////if (chkList.IndexOf(spot) >= 0)
                //////{
                //////    System.Diagnostics.Trace.Assert(false);
                //////    continue;
                //////}

                if (rectRange.Contains(spot.CenterX, spot.CenterY))
                {
                    var dx = x - spot.CenterX;
                    var dy = y - spot.CenterY;
                    var dSQ = dx * dx + dy * dy;
                    knn.Add(new KeyValuePair<EzBloc, int>(spot, dSQ));
                    //////chkList.Add(spot);
                }
            }

            if (knn.Count == 0)
                return null;

            if (needsToSort && knn.Count > 1)
            {
                int _compareSpots(KeyValuePair<EzBloc, int> kp1, KeyValuePair<EzBloc, int> kp2)
                {
                    if (kp1.Value > kp2.Value)
                        return 1;
                    else if (kp1.Value < kp2.Value)
                        return -1;
                    return 0;
                }
                knn.Sort(_compareSpots);
            }

            kNumber = Math.Min(kNumber, knn.Count);
            var result = new EzBloc[kNumber];

            for (int k = 0; k < kNumber; k++)
                result[k] = (EzBloc)knn[k].Key;

            return result;
        }
        void updateTooltip(CellBloc bloc, int vx, int vy, Control wnd)
        {
            if (bloc == null || _mode == ScanInspectMode.NOTRAY)
            {
                _toolTip.Hide(wnd);
            }
            else
            {
                if (_hitPt.X == vx && _hitPt.Y == vy)
                    return;

                //string msg = $"(x,y)=({bloc.CenterX},{bloc.CenterY}), score={bloc.Score:0.00}, size={bloc.Rect.Width}x{bloc.Rect.Height}";
                //if (bloc.Tag is QuadLinkNode node && node.rowCol != null)
                //    msg = $"[{node.rowCol.Row},{node.rowCol.Col}] " + msg;

                string msg = formatDisplayText(bloc);
                if (!string.IsNullOrEmpty(msg))
                {
                    _toolTip.Show(msg, wnd, vx + 10, vy + 10);
                }

                _hitPt = new Point(vx, vy);
            }
        }
        #endregion

        void updateCellGrid(IEnumerable<CELL> cells, out EzBlocsGrid grid)
        {
            int rows = 0;
            int cols = 0;
            var blocs = new List<EzBloc>();

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

            var builder = new EzBlocsGridBuilder();
            grid = builder.BuildEmptyGrid(blocs, rows, cols);

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
        }
        void updateOutGridBlocs(IEnumerable<Rectangle> outGridRects, out IList<EzBloc> outGridBlocs)
        {
            outGridBlocs = new List<EzBloc>();
            foreach (var rect in outGridRects)
            {
                outGridBlocs.Add(new EzBloc(rect, 0));
            }
        }
        void updatePassNgEmptyCount(IEnumerable<CELL> cells)
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
                foreach (var (b,t) in iterNonEmptyBlocs())
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
            if(lblSummaryTitle != null)
                lblSummaryTitle.Text = text;
        }

        #region HELPER_FUCTIONS
        IEnumerable<(EzBloc, string)> iterNonEmptyBlocs()
        {
            if (_grid != null)
            {
                foreach (CellBloc bloc in _grid)
                {
                    var cell = bloc?.Cell;
                    if (cell != null && !bloc.IsEmpty)
                        yield return (bloc, bloc.NonEmptyDesc);
                }
            }

            if (_outGridBlocs != null)
            {
                foreach (EzBloc bloc in _outGridBlocs)
                {
                    if (bloc == null) continue;
                    yield return (bloc, "疑似有料");
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
        string formatDisplayText(CellBloc bloc)
        {
            //if (cell != null)
            //{
            //    bool isEmpty;
            //    if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
            //        isEmpty = false;
            //    else if (cell.inspectReason != InspectReason.INS_ALIGNERR)
            //        isEmpty = false;
            //    else
            //        isEmpty = true;
            //    if (isEmpty)
            //        return $"[{cell.Index}]\n空位";
            //    string msg = _formatter.Format(cell);
            //    return msg;
            //}

            var cell = bloc?.Cell;
            if (checkResult(cell, out bool pass, out bool empty))
            {
                if (empty)
                    return $"[{cell.Index}]\n缺";
                return _formatter.Format(cell);
            }
            return null;
        }
        bool checkResult(CELL cell, out bool isPass, out bool isEmpty)
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
                if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                    isPass = true;
                else if (cell.inspectReason != InspectReason.INS_ALIGNERR)
                    isPass = false;
                else
                    isEmpty = true;
            }
            return true;
        }
        #endregion

        #region DRAW_ITEMS_FUNCTIONS
        void updateDrawItems()
        {
            if (_mode == ScanInspectMode.NOTRAY)
            {
                updateDrawItems_For_EmptyTray();
            }
            else
            {
                updateDrawItems_For_ChipLocate();
                updateDrawItems_For_ChipMeasure();
                //updateDrawItems_For_QrCode();
            }
        }
        void updateDrawItems_For_EmptyTray()
        {
            var size = new SizeF(200, 200);
            foreach (var cBloc in iterEmptyBlocs())
            {
                var rect = JetEazy.Qcvt.CreateCenterRect((float)cBloc.CenterX, (float)cBloc.CenterY, size.Width, size.Height);
                var item = new CviRotRectBox(ref rect, Color.Lime, 0.25f);
                _drawItems.Add(item);
            }
            foreach ((var cBloc, string text) in iterNonEmptyBlocs())
            {
                var rect = JetEazy.Qcvt.CreateCenterRect((float)cBloc.CenterX, (float)cBloc.CenterY, size.Width, size.Height);
                var item = new CviRotRectBox(ref rect, Color.Red, 0.25f) { Text = text };
                _drawItems.Add(item);
            }
        }
        void updateDrawItems_For_ChipLocate()
        {
            foreach (CellBloc bloc in _grid)
            {
                var cell = bloc?.Cell;
                if (cell == null) continue;

                if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                {
                    // PASS
                    var mvdRect = cell.DrawResultRectF();
                    var item = new CviRotRectBox(mvdRect.ToBox2D(), Color.Lime, 0.10f);
                    _drawItems.Add(item);
                }
                else if (cell.inspectReason != InspectReason.INS_ALIGNERR)
                {
                    // NG
                    var mvdDrawResultRectF = cell.DrawResultRectF();
                    var item = new CviRotRectBox(mvdDrawResultRectF.ToBox2D(), Color.Red, 0.10f) { Text = "NG" };
                    _drawItems.Add(item);
                }
                else
                {
                    // 吸盤空位
                    var mvdRect = cell.DrawBaseRectFFixSize(false);
                    var item = new CviRotRectBox(mvdRect.ToBox2D(), Color.Red);
                    _drawItems.Add(item);
                }
            }
        }
        void updateDrawItems_For_ChipMeasure()
        {
            if (!_inspectParams.bOpenLineMeasure)
                return;

            var drawItemsOfBorderBoxes = new List<IvDrawItem>();
            var drawItemsOfLinesOutSide = new List<IvDrawItem>();
            var drawItemsOfLinesInSide = new List<IvDrawItem>();

            foreach (CellBloc bloc in _grid)
            {
                var cell = bloc?.Cell;
                RectangleF cellRect = cell.viewRectF;
                cellRect.Inflate(_xRecipe.xExtendx, _xRecipe.xExtendy);
                var offset = cellRect.Location;

                // 繪件: 找到的邊線
                var linesOut = MvdConvertor.ToCSharpLines(offset, cell.cMvdLineSegmentFsOut);
                if (linesOut != null && linesOut.Length > 0)
                    drawItemsOfLinesOutSide.Add(new CviLineSegmentsBox(Color.Cyan, linesOut));

                // 繪件: 邊線手拉框
                foreach (var mvdShape in cell.cMvdShapesForFindLineRegion)
                {
                    if (mvdShape is CMvdRectangleF mvdRect)
                        drawItemsOfBorderBoxes.Add(new CviRotRectBox(mvdRect.ToBox2D(), Color.DarkBlue));
                }

                // 繪件: cMvdLineSegmentFsInSide
                if (_inspectParams.bCheckMeasureOffset)
                {
                    var linesIn = MvdConvertor.ToCSharpLines(offset, cell.cMvdLineSegmentFsInSide);
                    if (linesIn != null && linesIn.Length > 0)
                        drawItemsOfLinesInSide.Add(new CviLineSegmentsBox(Color.FromArgb(112, 48, 160), linesIn));
                }
            }

            _drawItems.AddRange(drawItemsOfLinesOutSide);
            _drawItems.AddRange(drawItemsOfBorderBoxes);
            _drawItems.AddRange(drawItemsOfLinesInSide);
        }
        #endregion

        #region DRAW_QRCODE_FUNCTIONS
        void draw_All_QrCodes(CvImageViewer viewer, Graphics gxView)
        {
            foreach (CellBloc bloc in _grid)
            {
                draw_OneCellData_QrCode(viewer, gxView, bloc?.Cell);
            }
        }
        void draw_OneCellData_QrCode(CvImageViewer viewer, Graphics gxView, CELL cell)
        {
            ////二维码
            if (cell.DrawBarcodePosition != null)
            {
                //_mvsUI.mvdRenderActivex1.AddShape(cell.DrawBarcodePosition);
                //CMvdTextF _CodeText
                //    = new CMvdTextF(cell.DrawBarcodePosition.GetVertex(2).fX,
                //                                  cell.DrawBarcodePosition.GetVertex(2).fY + 120,
                //                                  cell.RunCodeInfo.Content);
                //_CodeText.BorderColor = new MVD_COLOR(0, 255, 0);
                ////_CodeText.FontWidth = 11;
                //_CodeText.FillColor = new MVD_COLOR(0, 0, 0);
                //_mvsUI.mvdRenderActivex1.AddShape(_CodeText);

                var poly = cell.DrawBarcodePosition;
                var cc = poly.GetVertex(2);
                var x = cc.fX;
                var y = cc.fY + 120;
                string text = cell.RunCodeInfo?.Content;
                gxView.DrawString(text, _font, Brushes.Black, x, y);

            }
        }
        #endregion
    }
}
