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
using JetEazy.QvMath;
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

namespace LaserAlignDX.UISpace.ChipCellsViewer.V0
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
        StringFormat _sformat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };
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

            Draw_Contents(viewer, gxView);

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
                    return $"[{cell.Index}]\n空位";
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

        #region DRAW_FUNCTIONS
        void Draw_Contents(CvImageViewer viewer, Graphics gxView)
        {
            switch (_mode)
            {
                case ScanInspectMode.NOTRAY:
                    Draw_EmptyTray_Cells(viewer, gxView);
                    break;
                case ScanInspectMode.MEASUREAOI:
                case ScanInspectMode.QRCODE:
                default:
                    Draw_ProductionTray_Cells(viewer, gxView);
                    break;
            }
        }
        void Draw_EmptyTray_Cells(CvImageViewer viewer, Graphics gxView)
        {
            var size = new Size(200, 200);
            draw_blocs(viewer, gxView, iterEmptyBlocs(), Color.Lime, size, 0.25f);
            draw_blocs(viewer, gxView, iterNonEmptyBlocs(), Color.Red, size, 0.25f);
        }
        void Draw_ProductionTray_Cells(CvImageViewer viewer, Graphics gxView)
        {
            foreach (CellBloc bloc in _grid)
            {
                draw_OneCellData(viewer, gxView, bloc);
            }
        }

        void draw_OneCellData(CvImageViewer viewer, Graphics gxView, CellBloc bloc)
        {
            var cell = bloc?.Cell;
            if (cell == null)
                return;

            RectangleF cellRect = cell.viewRectF;
            cellRect.Inflate(_xRecipe.xExtendx, _xRecipe.xExtendy);
            var offset = cellRect.Location;

            if (_inspectParams.bOpenLineMeasure)
            {
                // 繪製 找到的邊線
                draw_OneCellData_LinesOutSide(viewer, gxView, cell, offset);

                // 繪製 邊線手拉框
                draw_OneCellData_LinesOutsideBoxes(viewer, gxView, cell);

                if (_inspectParams.bCheckMeasureOffset)
                {
                    // 繪製 cMvdLineSegmentFsInSide
                    draw_OneCellData_LinesInSide(viewer, gxView, cell, offset);
                }
            }

            // 晶粒定位
            draw_OneCellData_ChipLoc(viewer, gxView, cell);

            //二维码
            draw_OneCellData_Barcode(viewer, gxView, cell);
        }
        void draw_OneCellData_LinesOutsideBoxes(CvImageViewer viewer, Graphics gxView, CELL cell)
        {
            //var color = Color.FromArgb(38, 127, 0);
            var color = Color.DarkBlue;

            //if (_inspectParams.bOpenLineMeasure)
            {
                // 繪製 邊線手拉框
                foreach (var mvdShape in cell.cMvdShapesForFindLineRegion)
                {
                    if (mvdShape is CMvdRectangleF mvdRectF)
                    {
                        //var cx = mvdRectF.CenterX;
                        //var cy = mvdRectF.CenterY;
                        //var cw = mvdRectF.Width;
                        //var ch = mvdRectF.Height;
                        //var angle = mvdRectF.Angle;
                        //var box2D = new QvBox2D();
                        //box2D.SetBox(new PointF(cx,cy), new SizeF(cw, ch));
                        //box2D.SetCenter(cx, cy);
                        //box2D.SetTheta(angle * Math.PI / 180);
                        //gxView.DrawPolygon(pen, box2D.Corners);
                        draw_mvdRectF(viewer, gxView, color, mvdRectF);
                    }
                }
            }
        }
        void draw_OneCellData_LinesOutSide(CvImageViewer viewer, Graphics gxView, CELL cell, PointF offset)
        {
            var pen = viewer.GetOnePixelPen(Color.Cyan);

            foreach (var mLine in cell.cMvdLineSegmentFsOut)
            {
                if (mLine != null)
                {
                    var p1 = mLine.StartPoint;
                    var p2 = mLine.EndPoint;
                    p1.fX += offset.X;
                    p1.fY += offset.Y;
                    p2.fX += offset.X;
                    p2.fY += offset.Y;
                    ////MVD_POINT_F s0 = new MVD_POINT_F(mLine.StartPoint.fX + offset.X,
                    ////    mLine.StartPoint.fY + offset.Y);
                    ////MVD_POINT_F s1 = new MVD_POINT_F(mLine.EndPoint.fX + offset.X,
                    ////    mLine.EndPoint.fY + offset.Y);
                    ////CMvdLineSegmentF newLine = new CMvdLineSegmentF(s0, s1);
                    //CMvdLineSegmentF newLine = new CMvdLineSegmentF(p1, p2);
                    //newLine.BorderColor = new MVD_COLOR(255, 0, 255);
                    //_mvsUI.mvdRenderActivex1.AddShape(newLine);
                    gxView.DrawLine(pen, p1.fX, p1.fY, p2.fX, p2.fY);
                }
            }
        }
        void draw_OneCellData_LinesInSide(CvImageViewer viewer, Graphics gxView, CELL cell, PointF offset)
        {
            //if (_inspectParams.bOpenLineMeasure && _inspectParams.bCheckMeasureOffset)
            {
                var pen = viewer.GetOnePixelPen(Color.FromArgb(112, 48, 160));

                foreach (CMvdLineSegmentF mLine in cell.cMvdLineSegmentFsInSide)
                {
                    if (mLine != null)
                    {
                        var p1 = mLine.StartPoint;
                        var p2 = mLine.EndPoint;
                        p1.fX += offset.X;
                        p1.fY += offset.Y;
                        p2.fX += offset.X;
                        p2.fY += offset.Y;
                        ////MVD_POINT_F s0 = new MVD_POINT_F(
                        ////      mLine.StartPoint.fX + offset.X,
                        ////      mLine.StartPoint.fY + offset.Y);
                        ////MVD_POINT_F s1 = new MVD_POINT_F(
                        ////      mLine.EndPoint.fX + offset.X,
                        ////      mLine.EndPoint.fY + offset.Y);
                        //CMvdLineSegmentF newLine = new CMvdLineSegmentF(p1, p2);
                        //newLine.BorderColor = new MVD_COLOR(112, 48, 160);
                        //_mvsUI.mvdRenderActivex1.AddShape(newLine);
                        gxView.DrawLine(pen, p1.fX, p1.fY, p2.fX, p2.fY);
                    }
                }
            }
        }
        void draw_OneCellData_ChipLoc(CvImageViewer viewer, Graphics gxView, CELL cell)
        {
            //// 使用 toolTip 顯示文字
            //var formatter = new MainDispTextFormatter();
            //string cellInfoStr = formatter.Format(cell);

            var mvdDrawResultRectF = cell.DrawResultRectF();

            ////显示结果的xy angle
            //CMvdTextF cMvdTextFShowMain = new CMvdTextF(
            //                                mvdDrawResultRectF.CenterX,
            //                                mvdDrawResultRectF.CenterY,
            //                                cellInfoStr);

            //cMvdTextFShowMain.BorderColor = new MVD_COLOR(0, 255, 0);// cell.DrawResultRectF().BorderColor;// new MVD_COLOR(0, 255, 0);
            //cMvdTextFShowMain.FontWidth = 11;
            //cMvdTextFShowMain.FillColor = new MVD_COLOR(0, 0, 0, 50);

            if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
            {
                ////引导数据
                //if (INI.Instance.IsResultShowChar)
                //    _mvsUI.mvdRenderActivex1.AddShape(cMvdTextFShowMain);
                ////定位框
                //_mvsUI.mvdRenderActivex1.AddShape(mvdDrawResultRectF);
                ////DSMain.mvdRenderActivex1.AddShape(cell.DrawBaseRectFFixSize(true));
                draw_mvdRectF(viewer, gxView, Color.Lime, mvdDrawResultRectF);
            }
            else if (cell.inspectReason != InspectReason.INS_ALIGNERR)
            {
                //if (INI.Instance.IsResultShowChar)
                //{
                //    cMvdTextFShowMain = new CMvdTextF(
                //        mvdDrawResultRectF.CenterX,
                //        mvdDrawResultRectF.CenterY,
                //        $"{cellInfoStr}{Environment.NewLine}{GaUtil.GetEnumDescription(cell.inspectReason)}");
                //    cMvdTextFShowMain.BorderColor = new MVD_COLOR(255, 0, 0);
                //    _mvsUI.mvdRenderActivex1.AddShape(cMvdTextFShowMain);
                //    //定位框
                //    _mvsUI.mvdRenderActivex1.AddShape(mvdDrawResultRectF);
                //}

                draw_mvdRectF(viewer, gxView, Color.Red, mvdDrawResultRectF, "NG", 0.10f);
            }
            else
            {
                //>>> _mvsUI.mvdRenderActivex1.AddShape(cell.DrawBaseRectFFixSize(false));

                // 吸盤空位
                var rect = cell.DrawBaseRectFFixSize(false);
                draw_mvdRectF(viewer, gxView, Color.Red, rect);
            }
        }
        void draw_OneCellData_Barcode(CvImageViewer viewer, Graphics gxView, CELL cell)
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

        void draw_blocs(CvImageViewer viewer, Graphics gxView, IEnumerable<(EzBloc,string)> blocs, Color color, Size? size, float blend = 0)
        {
            if (blocs == null)
                return;

            bool isWorldDrawing = viewer.IsInWorldCoordinate();

            // Blending alpha
            int alpha = blend > 0 && blend <= 1 ? (int)(255 * blend) : 0;
            Brush bkBrush = alpha > 0 ? new SolidBrush(Color.FromArgb(alpha, color)) : null;
            Brush txtBrush = new SolidBrush(color);

            foreach ((EzBloc bloc, string text) in blocs)
            {
                if (bloc == null)
                    continue;

                var pen = viewer.GetOnePixelPen(color);

                var rect = bloc.Rect;

                if (size != null)
                {
                    int dx = (size.Value.Width - rect.Width) / 2;
                    int dy = (size.Value.Height - rect.Height) / 2;
                    rect.Inflate(dx, dy);
                }

                if (isWorldDrawing)
                {
                    if (bkBrush != null)
                        gxView.FillRectangle(bkBrush, rect);

                    gxView.DrawRectangle(pen, rect);

                    if (!string.IsNullOrEmpty(text))
                        gxView.DrawString(text, _font, txtBrush, bloc.Rect, _sformat);
                }
                else
                {
                    var txtRect = bloc.Rect;
                    viewer.TransCoordToView(ref txtRect);
                    viewer.TransCoordToView(ref rect);

                    if (bkBrush != null)
                        gxView.FillRectangle(bkBrush, rect);

                    gxView.DrawRectangle(pen, rect);

                    if (!string.IsNullOrEmpty(text))
                        gxView.DrawString(text, _font, txtBrush, txtRect, _sformat);
                }
            }

            bkBrush?.Dispose();
            txtBrush?.Dispose();
        }
        void draw_blocs(CvImageViewer viewer, Graphics gxView, IEnumerable<EzBloc> blocs, Color color, Size? size, float blend = 0)
        {
            IEnumerable<(EzBloc, string)> iter()
            {
                foreach (var b in blocs)
                    if (b != null)
                        yield return (b, "");
            };
            draw_blocs(viewer, gxView, iter(), color, size, blend);
        }
        void draw_mvdRectF(CvImageViewer viewer, Graphics gxView, Color color, CMvdRectangleF mvdRectF, string text = null, float blend = 0)
        {
            var pen = viewer.GetOnePixelPen(color);

            // Blending alpha
            int alpha = blend > 0 && blend <= 1 ? (int)(255 * blend) : 0;
            Brush bkBrush = alpha > 0 ? new SolidBrush(Color.FromArgb(alpha, color)) : null;

            var cx = mvdRectF.CenterX;
            var cy = mvdRectF.CenterY;
            var cw = mvdRectF.Width;
            var ch = mvdRectF.Height;
            var angle = mvdRectF.Angle;
            var box2D = new QvBox2D();
            box2D.SetBox(new PointF(cx, cy), new SizeF(cw, ch));
            box2D.SetCenter(cx, cy);
            box2D.SetTheta(angle * Math.PI / 180);

            if (bkBrush != null)
                gxView.FillPolygon(bkBrush, box2D.Corners);
            gxView.DrawPolygon(pen, box2D.Corners);

            if(!string.IsNullOrEmpty(text))
            {
                using (var br = new SolidBrush(color))
                {
                    gxView.DrawString(text, _font, br, box2D.BoundaryRect, _sformat);
                }
            }

            bkBrush?.Dispose();
        }
        #endregion
    }
}
