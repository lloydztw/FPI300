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
using JetEazy.QMath;
using JetEazy.QvMath;
using LaserAlignDX.AoiModel.AI;
using LaserAlignDX.OPSpace;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using XCell = LaserAlignDX.OPSpace.RegionCellX3Class;
using XRecipe = LaserAlignDX.OPSpace.RecipeSpace.RecipeFPIX3Class;


namespace LaserAlignDX.Mvc.Gui.ChipCellsViewer
{
    public partial class CviChipResultBox : CvImageViewerInteractor, IvDrawItem
    {
        #region CONFIG
        static bool OPT_SHOW_AI_TRAIN_CORNERS = false;
        #endregion

        #region GLOBAL_MESS
        XRecipe _xRecipe
        {
            get => XRecipe.Instance;
        }
        #endregion

        #region PRIVATE_CELL_DATA
        XCell _cell;
        bool _bypassNg;
        #endregion

        #region GUI_DRAW_ITEMS
        List<IvDrawItem> _drawItems = new List<IvDrawItem>();
        List<IvDrawItem> _drawItemsDetails = new List<IvDrawItem>();
        List<IvDrawItem> _drawItemsNgGapCorners = new List<IvDrawItem>();
        Font _font = null;
        #endregion

        #region RUNTIME_DATA
        bool _isActive;
        //bool _isCtrlPressed;
        OpenCvSharp.Point2f[] _boundaryPolygon;
        #endregion

        public CviChipResultBox(XCell cell, bool bypassNg)
        {
            Visible = true;
            Enabled = true;
            UpdateResult(cell, bypassNg);
        }
        public object Tag { get; set; }

        public void Reset()
        {
            _cell = null;
            _drawItems.Clear();
            _drawItemsDetails.Clear();
            _drawItemsNgGapCorners.Clear();
            _bypassNg = false;
        }
        public void UpdateResult(XCell cell, bool bypassNg)
        {
            Reset();
            _cell = cell;
            _bypassNg = bypassNg;
            updateBoundaryPolygon();
            updateDrawItems();
        }

        #region OVERRIDES
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            //>>> base.OnDraw(viewer, gxView);

            if (_font == null)
                _font = viewer.Font;

            if (_cell != null)
            {
                bool isWorld = viewer.IsInWorldCoordinate();
                if (!isWorld)
                    viewer.SwitchToWorldCoordinate(gxView);

                draw_ChipsLoc(viewer, gxView);
                draw_ChipDetails(viewer, gxView);
                draw_QrCode_One(viewer, gxView, _cell);

                if (!isWorld)
                    viewer.SwitchToViewportCoordinate(gxView);
            }
        }
        public override bool OnMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            bool isChanged = base.OnMouseMove(viewer, e);
            bool active = false;

            var chipQuad2D = _cell?.ChipData?.ChipQuad2D;
            if (chipQuad2D != null)
            {
                float x = e.Location.X;
                float y = e.Location.Y;
                viewer.TransViewportToWorld(ref x, ref y);

                if (_boundaryPolygon != null)
                {
                    active = (Cv2.PointPolygonTest(_boundaryPolygon, new Point2f(x, y), false) >= 0);
                }
            }

            if (_isActive != active)
            {
                _isActive = active;
                isChanged = true;
            }

            return isChanged;
        }
        public override bool OnMouseDown(CvImageViewer viewer, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Right && _isCtrlPressed)
            //{
            //    popupMenuStrip(viewer, e.Location);
            //    _isCtrlPressed = false;
            //    return false;
            //}
            return base.OnMouseDown(viewer, e);
        }
        public override void OnKeyDown(CvImageViewer viewer, KeyEventArgs e)
        {
            //if (!Visible)
            //    return;

            //_isCtrlPressed = e.Control;

            //switch (e.KeyCode)
            //{
            //    case Keys.Escape:
            //        DebugMatchingOff();
            //        _isCtrlPressed = false;
            //        break;

            //    case Keys.F4:
            //        if (!BY_PASS && e.Control)
            //        {
            //            if (_cursorBloc is CellBloc cellBloc)
            //                DebugMatching(cellBloc);
            //            _isCtrlPressed = false;
            //        }
            //        break;

            //    case Keys.F2:
            //        if (!BY_PASS && e.Control)
            //        {
            //            DumpCellRegions(viewer);
            //            _isCtrlPressed = false;
            //        }
            //        break;

            //    case Keys.C:
            //        if (!BY_PASS && e.Control)
            //        {
            //            CopyOneChipDimsToClipboard();
            //            _isCtrlPressed = false;
            //        }
            //        break;
            //}

            base.OnKeyDown(viewer, e);
        }
        #endregion

        #region UPDATE_DRAW_ITEMS_FUNCTIONS
        void updateBoundaryPolygon()
        {
            _boundaryPolygon = null;

            if (_cell != null)
            {
                if (_xRecipe.InspectParams.optChipMeasurement)
                {
                    var mpts = new List<QVector>();
                    var lines = _cell.ChipData?.LineSegments;
                    if (lines != null && lines.Length >= 4)
                    {
                        for (int i = 0, NP = lines.Length; i < NP; i++)
                        {
                            int j = (i + 1) % NP;
                            var line1 = lines[i];
                            var line2 = lines[j];
                            if (line1 == null || line2 == null) continue;
                            var pt = line1.CalcIntersectedPoint(line2);
                            if (pt == null) continue;
                            mpts.Add(pt);


                        }
                    }

                    if (mpts.Count >= 4)
                    {
                        _boundaryPolygon = Array.ConvertAll(mpts.ToArray(), p => new OpenCvSharp.Point2f((float)p.X, (float)p.Y));
                        return;
                    }
                }

                var chipQuad = _cell.ChipData?.ChipQuad2D;
                if (chipQuad != null)
                {
                    _boundaryPolygon = Array.ConvertAll(chipQuad.Corners, p => new OpenCvSharp.Point2f((float)p.X, (float)p.Y));
                }
            }
        }
        void updateDrawItems()
        {
            updateDrawItems_For_ChipLocate();
            updateDrawItems_For_ChipMeasure();
            updateDrawItems_For_AiTrainCorners();
        }
        void updateDrawItems_For_ChipLocate()
        {
            var cell = _cell;
            if (cell == null) 
                return;

            var chipQuad2D = cell.ChipData?.ChipQuad2D;
            if (chipQuad2D == null)
            {
                //>>> chipBox2D = cell.DrawResultRectF()?.ToBox2D();
                chipQuad2D = cell.DrawResultRectF()?.ToQuad2D();
            }

            if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0 && chipQuad2D != null)
            {
                // PASS
                var item = new CviRotRectBox(chipQuad2D, Color.Lime, 0f) { Tag = cell };
                _drawItems.Add(item);
            }
            else if (cell.inspectReason != InspectReason.INS_ALIGNERR && chipQuad2D != null)
            {
                // NG
                var item = _bypassNg ?
                    new CviRotRectBox(chipQuad2D, Color.Green, 0f) { Tag = cell } :
                    new CviRotRectBox(chipQuad2D, Color.Red, 0.20f) { Text = "NG", Tag = cell };
                _drawItems.Add(item);
            }
            else
            {
                // 吸盤 格位
                var center = JetEazy.Qcvt.CenterF(ref cell.viewRectF);
                var rect = JetEazy.Qcvt.CreateCenterRect(center.X, center.Y, 200f, 200f);
                var quad2D = new QvQuad2D();
                quad2D.SetBox(rect.Location, rect.Size);

                var color = (cell.OutGridLink != null) ? Color.Blue : Color.Purple;
                var item = new CviRotRectBox(quad2D, color, 0.10f) { Tag = cell };
                _drawItems.Add(item);
            }

            #region PADS_BOX2D_FOR_DEBUG
            if (false)
            {
                var padsGrid = cell.ChipData.PadsGrid;
                if (padsGrid != null)
                {
                    int i = 0;
                    var cornerBlocs = padsGrid.GetCornerBlocs();
                    foreach (var pad in cornerBlocs)
                    {
                        var text = $"P{i}";
                        if (pad.ExtraBox2D != null)
                            _drawItemsDetails.Add(new CviRotRectBox(pad.ExtraBox2D, Color.Blue) { Text = text });
                        else
                            _drawItemsDetails.Add(new CviRotRectBox((RectangleF)pad.Rect, Color.Blue) { Text = text });
                        i++;
                    }
                }
            }
            #endregion
        }
        void updateDrawItems_For_ChipMeasure()
        {
            if (!_xRecipe.InspectParams.optChipMeasurement)
                return;

            var xInspect = _xRecipe.InspectParams;
            bool withPads = xInspect.optPadEdgeGapsMeasurement && xInspect.xAlgorithm == MatchAlgorithmEnum.GridMatch;

            var drawItemsOfBorderBoxes = new List<IvDrawItem>();
            var drawItemsOfLineSegments = new List<IvDrawItem>();
            var drawItemsOfNgGapPads = new List<IvDrawItem>();

            var cell = _cell;
            var chipData = cell?.ChipData;
            if (chipData == null || chipData.IsEmpty())
                return;

            #region 邊線
            // 繪件: 邊線 (左上右下)
            foreach (var lineSeg in chipData.LineSegments)
            {
                if (lineSeg != null)
                    drawItemsOfLineSegments.Add(new CviLineSegmentsBox(Color.Cyan, lineSeg.ToCSharpLine()) { Tag = cell });
            }
            #endregion

            #region 邊線拉框
            // 繪件: 邊線手拉框
            int borderIdx = 0;
            foreach (var borderBox in chipData.LineBorderBoxes)
            {
                if (borderBox != null)
                {
                    //drawItemsOfBorderBoxes.Add(new CviRotRectBox(borderBox, Color.DarkBlue) { Tag = cell, Text = $"{borderIdx}" });
                    drawItemsOfBorderBoxes.Add(new CviRotRectBox(borderBox, Color.DarkBlue) { Tag = cell });
                }
                borderIdx++;
            }
            #endregion

            #region 廢除_LINES_INSIDE
            //// 繪件: cMvdLineSegmentFsInSide
            //if (_inspectParams.optChipEdgesDiffCompare)
            //{
            //    //var linesIn = MvdConvertor.ToCSharpLines(cell.cMvdLineSegmentFsInSide);
            //    var linesIn = GaMvdExt.ToCSharpLines(offset, cell.cMvdLineSegmentFsInSide);
            //    if (linesIn != null && linesIn.Length > 0)
            //        drawItemsOfLinesInSide.Add(new CviLineSegmentsBox(Color.FromArgb(112, 48, 160), linesIn));
            //}
            #endregion

            #region DIM_MEASURE_POINTS
            //var dimMeasurePoints = chipData?.ChipDimension?.DimMeasurePoints;
            //if (dimMeasurePoints != null)
            //    _chipDimMeasurePointsDict.Add(cell, dimMeasurePoints);
            #endregion

            #region CHIP_PAD_MID_QUAD_2D
            var padsGrid = cell.ChipData.PadsGrid;
            if (padsGrid != null)
            {
                bool isPass = cell.inspectReason == InspectReason.PASS;
                Color itemColor = isPass ? Color.Lime : _bypassNg ? Color.Green : Color.Red;
                EzBlocsGridAnalyzer.CalcQuad2D(padsGrid, out var quad2D, false);
                var item = new CviRotRectBox(quad2D, itemColor, 0.20f) { Tag = cell };
                _drawItemsDetails.Add(item);
            }
            #endregion

            #region NG_PAD_GAPS
            if (withPads)
            {
                var gaps = cell.ChipData?.PadEdgeGaps;
                var grid = cell.ChipData?.PadsGrid;
                if (gaps != null && grid != null)
                {
                    int i = 0;
                    foreach (var gap in gaps.IterItems())
                    {
                        if (gap.X < xInspect.PadEdgeGapX_Min ||
                            gap.X > xInspect.PadEdgeGapX_Max ||
                            gap.Y < xInspect.PadEdgeGapY_Min ||
                            gap.Y > xInspect.PadEdgeGapY_Max)
                        {
                            // [0,0], [0,c], [r,c], [r,0]
                            int r = i == 2 || i == 3 ? grid.Rows - 1 : 0;
                            int c = i == 1 || i == 2 ? grid.Cols - 1 : 0;
                            var pad = grid.Get(r, c);
                            if (pad != null)
                            {
                                if (pad.ExtraBox2D != null)
                                    drawItemsOfNgGapPads.Add(new CviRotRectBox(pad.ExtraBox2D, Color.Yellow, 0.5f));
                                else
                                    drawItemsOfNgGapPads.Add(new CviRotRectBox((RectangleF)pad.Rect, Color.Yellow, 0.5f));
                            }
                        }
                        i++;
                    }
                }
            }
            #endregion

            _drawItemsDetails.AddRange(drawItemsOfLineSegments);
            _drawItemsDetails.AddRange(drawItemsOfBorderBoxes);
            _drawItemsNgGapCorners.AddRange(drawItemsOfNgGapPads);
        }
        void updateDrawItems_For_AiTrainCorners()
        {
#if (OPT_USE_AI)
            if (OPT_SHOW_AI_TRAIN_CORNERS && _cell != null)
            {
                var aiData = new AiDataCropper();
                var drawItems = aiData.GetDrawItems(_cell);
                if (drawItems != null && drawItems.Count > 0)
                    _drawItemsDetails.AddRange(drawItems);
            }
#endif
        }
        #endregion

        #region DRAW_FUNCTIONS
        void draw_ChipsLoc(CvImageViewer viewer, Graphics gxView)
        {
            var toDraw = !(_xRecipe.InspectParams.optChipMeasurement && _isActive);
            if (toDraw)
            {
                foreach (var item in _drawItems)
                    item?.OnDraw(viewer, gxView);

                foreach(var item in _drawItemsNgGapCorners)
                    item?.OnDraw(viewer, gxView);
            }
        }
        void draw_ChipDetails(CvImageViewer viewer, Graphics gxView)
        {
            var toDraw = _xRecipe.InspectParams.optChipMeasurement && _isActive;
            if (!toDraw)
                return;

            foreach (var item in _drawItemsDetails)
                item?.OnDraw(viewer, gxView);

            foreach (var item in _drawItemsNgGapCorners)
                item?.OnDraw(viewer, gxView);

            draw_DimMeasurePoints(viewer, gxView, _cell);
            draw_GapMeasurePoints(viewer, gxView, _cell);
        }
        #endregion

        #region DRAW_DIM_MEASURE_POINTS_FUNCTIONS
        void draw_GapMeasurePoints(CvImageViewer viewer, Graphics gxView, XCell activeCell)
        {
            bool withPadGaps = _xRecipe.InspectParams.xAlgorithm == MatchAlgorithmEnum.GridMatch &&
                               _xRecipe.InspectParams.optPadEdgeGapsMeasurement &&
                               _xRecipe.InspectParams.optChipMeasurement;

            if (!withPadGaps)
                return;

            var chipData = activeCell?.ChipData;
            if (chipData == null || chipData.IsEmpty())
                return;

            var gaps = chipData.PadEdgeGaps;
            if (gaps == null) return;

            var gapMeasurePts = gaps?.GapMeasurePoints;
            if (gapMeasurePts == null)
                return;

            //var xInspect = _xRecipe.InspectParams;
            //var min = new QVector(xInspect.PadEdgeGapX_Min, xInspect.PadEdgeGapY_Min);
            //var max = new QVector(xInspect.PadEdgeGapX_Max, xInspect.PadEdgeGapY_Max);
            //var gapResults = gaps.Check(out bool isPass, min, max);

            var gapResults = gaps?.PassNgResults;
            if (gapResults == null) 
                return;

            bool toShowText = viewer.GetZoomScale() > 0.35;
            for (int i = 0, N = gapResults.Length; i < N; i++)
            {
                bool isPass = gapResults[i];
                int k = i * 2;
                int k1 = k + 1;
                if (k1 >= gapMeasurePts.Length)
                    break;

                var p0 = gapMeasurePts[k];
                var p1 = gapMeasurePts[k1];
                if (p0 == null || p1 == null)
                    continue;

                draw_MeasureLine(viewer, gxView, p0, p1, isPass ? Color.Purple : Color.Red);

                if (toShowText)
                    gxView.DrawString($"{i}", _font, isPass ? Brushes.Purple : Brushes.Red, (float)p0.X, (float)p0.Y);
            }
        }
        void draw_DimMeasurePoints(CvImageViewer viewer, Graphics gxView, XCell activeCell)
        {
            if (!_xRecipe.InspectParams.optChipMeasurement)
                return;

            var measurePts = activeCell?.ChipData?.ChipDimension?.DimMeasurePoints;
            if (measurePts != null)
            {
                var dimResults = activeCell.ChipData.ChipDimension?.PassNgResults;

                int idx = 0;
                foreach (var p in measurePts)
                {
                    bool isPass = dimResults != null ? dimResults[idx % 2] : true;
                    draw_MeasurePoint(viewer, gxView, p, isPass);
                    idx++;
                }

                #region DRAW_TEXT
                if (viewer.GetZoomScale() > 0.35)
                {
                    idx = 0;
                    var offset = new QVector(25, -25);
                    foreach (var p in measurePts)
                    {
                        if (p != null)
                        {
                            var pt = p + offset;
                            gxView.DrawString($"{idx}", _font, Brushes.Orange, (float)pt.X, (float)pt.Y);
                        }
                        idx++;
                    }
                }
                #endregion

                draw_MeasureLine(viewer, gxView, measurePts[0], measurePts[2], Color.Yellow);
                draw_MeasureLine(viewer, gxView, measurePts[1], measurePts[3], Color.Yellow);
            }
        }
        void draw_MeasurePoint(CvImageViewer viewer, Graphics gxView, QVector pt, bool isPass)
        {
            if (pt == null)
                return;

            float sz = 15;
            var cx = (float)pt.X;
            var cy = (float)pt.Y;
            //>>> gxView.DrawLine(pen, cx - sz, cy, cx + sz, cy);
            //>>> gxView.DrawLine(pen, cx, cy - sz, cx, cy + sz);

            var rect = new RectangleF(cx - sz, cy - sz, sz * 2, sz * 2);

            if (isPass)
            {
                var pen = viewer.GetOnePixelPen(Color.Yellow);
                gxView.DrawEllipse(pen, rect);
            }
            else
            {
                var br = Brushes.Red;
                gxView.FillEllipse(br, rect);
            }
        }
        void draw_MeasureLine(CvImageViewer viewer, Graphics gxView, QVector pt, QVector pt2, Color color)
        {
            if (pt == null || pt2 == null)
                return;
            var pen = viewer.GetOnePixelPen(color);
            var cx = (float)pt.X;
            var cy = (float)pt.Y;
            var cx2 = (float)pt2.X;
            var cy2 = (float)pt2.Y;
            gxView.DrawLine(pen, cx, cy, cx2, cy2);
        }
        #endregion

        #region DRAW_QRCODE_FUNCTIONS
        void draw_QrCode_One(CvImageViewer viewer, Graphics gxView, XCell cell)
        {
            if (!_xRecipe.InspectParams.optChipDefectsInspect)
                return;

            if (cell != null && cell.DrawBarcodePosition != null)
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

        #region PRIVATE_HELPER_FUNCTIONS
        IEnumerable<QVector> iterMeasurePoints()
        {
            if (_cell != null)
            {
                if (_xRecipe.InspectParams.optChipMeasurement)
                {
                    var mpts = _cell.ChipData?.ChipDimension?.DimMeasurePoints;
                    if (mpts != null)
                        foreach (var p in mpts)
                            yield return p;
                }
                if(_xRecipe.InspectParams.optPadEdgeGapsMeasurement)
                {
                    var mpts = _cell.ChipData?.PadEdgeGaps?.GapMeasurePoints;
                    if (mpts != null)
                        foreach (var p in mpts)
                            yield return p;
                }
            }
        }
        #endregion
    }
}
