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

using JetEazy.EzImage;
using JetEazy.ImageViewerEx;
using JetEazy.Match;
using JetEazy.OpenCV;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model;
using LaserAlignDX.OPSpace;
using LeTian.AoiLib;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VisionDesigner;

using CELL = LaserAlignDX.OPSpace.RegionCellX3Class;
using InspectParams = LaserAlignDX.OPSpace.RecipeSpace.InspectX3ParaClass;
using RECIPE = LaserAlignDX.OPSpace.RecipeSpace.RecipeFPIX3Class;

namespace LaserAlignDX.Mvc.Gui.ChipCellsViewer
{
    public class CviCellsResultBoxes : CviAbsTooltipBox
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
        Font _font = null;
        #endregion

        public Control lblSummaryTitle
        {
            get;
            set;
        }

        #region GUI_DRAW_ITEMS
        List<IvDrawItem> _drawItems = new List<IvDrawItem>();
        CviRotRectBox _cviRegionBox;
        #endregion

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
            adjustFetchSize();
        }

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
                foreach (var b in iterNonEmptyBlocs(true))
                    if (b != null)
                        ng++;

                foreach (var b in iterNonEmptyBlocs(false))
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

        #region OVERRIDES
        public override void OnKeyDown(CvImageViewer viewer, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F10)
            {
                if (_cursorBloc != null)
                    DebugMatching(_cursorBloc as CellBloc);
            }
            if (e.KeyCode == Keys.Escape)
            {
                DebugMatchingOff();
            }

            base.OnKeyDown(viewer, e);
        }
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            base.OnDraw(viewer, gxView);

            if (_cviRegionBox != null && _cviRegionBox.Visible)
                _cviRegionBox.OnDraw(viewer, gxView);


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
            return base.OnMouseMove(viewer, e);
        }
        #endregion

        #region TOOL_TIP_FUNCTIONS
        void adjustFetchSize()
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
                        _cviRegionBox = new CviRotRectBox(cellRect, Color.White);
                        _cviRegionBox.Visible = false;
                        return;
                    }
                }
            }
        }
        protected override IEnumerable<EzBloc> iterFetchableBlocs()
        {
            if (_grid != null)
            {
                foreach (var bloc in _grid.IterBlocs())
                    if (bloc != null)
                        yield return bloc;
            }
        }
        protected override string composeTooltipText(EzBloc cursor, EzBloc cursor2)
        {
            var cellBloc = cursor as CellBloc;

            _cviRegionBox.Box2D.SetCenter((float)cellBloc.Center.X, (float)cellBloc.Center.Y);
            _cviRegionBox.Visible = true;

            string txt = formatDisplayText(cellBloc);
            return txt;
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

        static string PATH_DUMP => "d:\\paso.log\\chipLoc";
        void DebugMatching(CellBloc cellBloc)
        {
            var cell = cellBloc?.Cell;
            if (cell == null) return;

            // (0) Directory
            JetEazy.IO.QxPathUtility.InitDirectory(PATH_DUMP);
            string fname = cellBloc.Cell.lblName + ".png";
            string dumpFile = System.IO.Path.Combine(PATH_DUMP, fname);

            // (1) Golden and Thresh
            var goldenBmp = _xRecipe.bmpprinttemplate;
            var thresh = InspectParams.Instance.xGridPadThreshold;
            var extendX = _xRecipe.xExtendx;
            var extendY = _xRecipe.xExtendy;

            // (2) VISUAL_DEBUG
            EzPadsGridFinder.VISUAL_DEBUG = true;

            // (3) Matcher
            var matcher = new EzRigidBodyGridMatcher(shrink: 1);
            matcher.SetGoldenTemplate(goldenBmp);
            matcher.PadThreshold = thresh;

            // (4) 測試資料
            var lineScanImageHolder = GaMvcConfig.SysModel.LineScanImageHolder;
            var fullfovBmp = lineScanImageHolder.PeekBitmap();
            using (var bridge = new QxImageBridge(fullfovBmp))
            {
                var cellRect = Rectangle.Round(cell.viewRectF);
                cellRect.Inflate(_xRecipe.xExtendx, _xRecipe.xExtendy);
                GaUtil.BoundRect(ref cellRect, fullfovBmp.Size);
                var roi = JetEazy.Qcvt.CV(cellRect);

                Mat imgScene = bridge.Image[roi].Clone();

                var bestResult = matcher.FindBestMatch(imgScene, dumpFile);

                if (bestResult != null)
                {
                    var bestGrid = bestResult.Grid;
                    var box2d = bestResult.CalcBox2D();
                    //EzPadsGridFinder.FindSpecialKeyPad(bestGrid, out int kr, out int kc, out int px);
                    //EzPadsGridFinder.FindSpecialKeyPad(imgScene, bestGrid, out int kr, out int kc, out double kSQ);
                    int kr = bestResult.KeyRow;
                    int kc = bestResult.KeyCol;
                    var kSQ = bestResult.KeySQRatio;
                    VxDebugDrawer.Draw(imgScene, box2d, bestGrid, kr, kc, Scalar.Lime, $"Best Grid [{bestGrid.Rows}x{bestGrid.Cols}] = {bestGrid.GetMajorCount()} @ {fname}");
                }

                Cv2.WaitKey();
                Cv2.DestroyAllWindows();
                EzPadsGridFinder.VISUAL_DEBUG = false;
                imgScene?.Dispose();
            }
        }
        void DebugMatchingOff()
        {
            Cv2.DestroyAllWindows();
            EzPadsGridFinder.VISUAL_DEBUG = false;
        }
    }
}
