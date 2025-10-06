#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-10-06 重整 (by LeTian Chang)
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
using JetEazy.Utils;
using LaserAlignDX.OPSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using VisionDesigner;

using CELL = LaserAlignDX.OPSpace.RegionCellX3Class;
using RECIPE = LaserAlignDX.OPSpace.RecipeSpace.RecipeFPIX3Class;


namespace LaserAlignDX.Mvc.Gui.ChipCellsViewer
{
    public partial class CviChipDetailResultBox : CvImageViewerInteractor, IvDrawItem
    {
        #region GLOBAL_MESS
        RECIPE _xRecipe
        {
            get => RECIPE.Instance;
        }
        #endregion
        
        #region PRIVATE_DATA
        CELL _xCell;
        #endregion

        #region GUI_MEMBERS
        Font _font = null;
        List<IvDrawItem> _drawItems = new List<IvDrawItem>();
        List<IvDrawItem> _drawItemsEx = new List<IvDrawItem>();
        #endregion

        public void UpdateResult(CELL cell)
        {
            _xCell = cell;
            _drawItems.Clear();
            _drawItemsEx.Clear();

            if (cell == null)
                return;

            try
            {
                updateChipLoc(cell);
                updateLineSegments(cell);
                updatePads(cell);
            }
            catch (Exception ex)
            {
                GaUtil.LOG_ERROR(ex, $"{GetType().Name}.UpdateResult");
            }
        }
        public object Tag
        {
            get;
            set;
        }

        #region OVERRIDES
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            if (_font == null)
                _font = viewer.Font;

            if (_xCell != null)
            {
                bool isWorld = viewer.IsInWorldCoordinate();
                if (!isWorld)
                    viewer.SwitchToWorldCoordinate(gxView);

                try
                {
                    foreach (var item in _drawItems)
                        item.OnDraw(viewer, gxView);

                    foreach (var item in _drawItemsEx)
                        item.OnDraw(viewer, gxView);

                    draw_DimMeasurePoints(viewer, gxView);

                    draw_Defects(viewer, gxView);
                    draw_QrCode(viewer, gxView);
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
        #endregion

        #region DRAW_FUNCTIONS
        void draw_DimMeasurePoints(CvImageViewer viewer, Graphics gxView)
        {
            if (!_xRecipe.InspectParams.optChipMeasurement)
                return;

            var measurePts = _xCell?.ChipData?.DimMeasurePoints;
            if (measurePts != null)
            {
                foreach (var pt in measurePts)
                {
                    draw_Point(viewer, gxView, pt, Color.Yellow);
                }
            }
        }
        void draw_Defects(CvImageViewer viewer, Graphics gxView)
        {
            //// RESERVED
            //if (!_xRecipe.InspectParams.optChipDefectsInspect)
            //    return;
        }
        void draw_QrCode(CvImageViewer viewer, Graphics gxView)
        {
            if (!_xRecipe.InspectParams.optChipDefectsInspect)
                return;

            // 二维码
            var cell = _xCell;
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
        void draw_Point(CvImageViewer viewer, Graphics gxView, QVector pt, Color color)
        {
            if (pt == null)
                return;
            float sz = 20;
            var pen = viewer.GetOnePixelPen(color);
            var cx = (float)pt.X;
            var cy = (float)pt.Y;
            gxView.DrawLine(pen, cx - sz, cy, cx + sz, cy);
            gxView.DrawLine(pen, cx, cy - sz, cx, cy + sz);
        }
        #endregion

        #region PRIVATE_UPDATE_FUNCTIONS
        void updateChipLoc(CELL cell)
        {
            if (cell == null)
                return;

            var chipBox2D = cell.ChipData?.ChipBox2D;
            if (chipBox2D == null)
                chipBox2D = cell.DrawResultRectF()?.ToBox2D();

            if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0 && chipBox2D != null)
            {
                // PASS
                var item = new CviRotRectBox(chipBox2D, Color.Lime, blend: 0f);
                _drawItems.Add(item);
            }
            else if (cell.inspectReason != InspectReason.INS_ALIGNERR && chipBox2D != null)
            {
                // NG
                var item = new CviRotRectBox(chipBox2D, Color.Red, blend: 0.25f) { Text = "NG" };
                _drawItems.Add(item);
            }
            else
            {
                // 格點空位
                var rect = cell.viewRectF;
                var dx = rect.Width / 4;
                var dy = rect.Height / 4;
                rect.Inflate(-dx, -dy);
                var box2D = new QvBox2D();
                box2D.SetBox(rect.Location, rect.Size);
                var item = new CviRotRectBox(box2D, Color.Red, 0.25f);
                _drawItems.Add(item);
            }
        }
        void updateLineSegments(CELL cell)
        {
            if (cell == null)
                return;

            if (!_xRecipe.InspectParams.optChipMeasurement)
                return;

            var chipData = cell.ChipData;
            var drawItemsOfLineSegs = _drawItems;
            var drawItemsOfBorderBoxes = _drawItemsEx;

            #region 邊線
            // 繪件: 邊線 (左上右下)
            if (chipData != null)
            {
                foreach (var lineSeg in chipData.LineSegments)
                {
                    if (lineSeg != null)
                        drawItemsOfLineSegs.Add(new CviLineSegmentsBox(Color.Cyan, lineSeg.ToCSharpLine()));
                }
            }
            // 繪件: 邊線 (左上右下) (舊版)
            if (drawItemsOfLineSegs.Count == 0)
            {
                RectangleF cellRect = cell.viewRectF;
                cellRect.Inflate(_xRecipe.xExtendx, _xRecipe.xExtendy);
                var offset = cellRect.Location;
                var linesOut = GaMvdExt.ToCSharpLines(offset, cell.cMvdLineSegmentFsOut);
                if (linesOut != null && linesOut.Length > 0)
                    drawItemsOfLineSegs.Add(new CviLineSegmentsBox(Color.Cyan, linesOut));
            }
            #endregion

            #region 邊線拉框
            // 繪件: 邊線手拉框
            if (chipData != null)
            {
                foreach (var borderBox in chipData.LineBorderBoxes)
                {
                    if (borderBox != null)
                        drawItemsOfBorderBoxes.Add(new CviRotRectBox(borderBox, Color.DarkBlue));
                }
            }
            // 繪件: 邊線手拉框 (舊版)
            if (drawItemsOfBorderBoxes.Count == 0)
            {
                foreach (var mvdShape in cell.cMvdShapesForFindLineRegion)
                {
                    if (mvdShape is CMvdRectangleF mvdRect)
                        drawItemsOfBorderBoxes.Add(new CviRotRectBox(mvdRect.ToBox2D(), Color.DarkBlue));
                }
            }
            #endregion
        }
        void updatePads(CELL cell)
        {
            var padsGrid = cell?.ChipData?.PadsGrid;
            if (padsGrid != null)
            {
                EzBlocsGridAnalyzer.CalcRotatedBox2D(padsGrid, out var padsBox2D, true);
                _drawItemsEx.Add(new CviRotRectBox(padsBox2D, Color.HotPink));
            }
        }
        #endregion
    }
}
