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

using EzAoiEmptyTrayInspector.Model;
using JetEazy.ImageViewerEx;
using JetEazy.Match;
using JetEazy.Transform;
using LaserAlignDX.Model.Coords;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public class CviCalibResultBox : CviAbsTooltipBox
    {
        #region PRIVATE_DATA
        MatchResult _matchResult;
        EzBlocsGrid _grid => _matchResult?.Grid;
        IList<EzBloc> _suckerBlocs => _matchResult?.Blocs;
        IList<EzBloc> _outGridBlocs => _matchResult?.OutGridBlocs;
        #endregion

        #region GUI_MEMBERS
        //ToolTip _toolTip = new ToolTip();
        //EzBloc _cursorBloc = null;
        //EzBloc _cursorBloc2 = null;
        #endregion

        #region RUNTIME_DATA
        int _debugOption = -1;
        #endregion

        public void Reset()
        {
            _matchResult = null;
        }
        public void UpdateResult(MatchResult result)
        {
            _matchResult = result;
            adjustFetchSize();
        }

        public bool IsEmptyTrayMode
        {
            get;
            set;
        }
        public ITransform TransCameraToMotor
        {
            get; set;
        }
        public ITransform TransCameraToWorld
        {
            get; set;
        }
        public CarrierEnum ActiveCarrierID
        {
            get; set;
        }
        public SuckerRowEnum ActiveSuckerRowID
        {
            get; set;
        }

        #region OVERRIDES
        public override void OnKeyDown(CvImageViewer viewer, KeyEventArgs e)
        {
            bool needsToRefresh = false;

            //if (e.Control && _cursorBloc != null)
            //{
            //    _cursorBloc2 = _cursorBloc;
            //    needsToRefresh = true;
            //}
            //else if (e.KeyCode == Keys.Escape && _cursorBloc2 != null)
            //{
            //    _cursorBloc2 = null;
            //    needsToRefresh = true;
            //}
            //else
            //{
            //}

            if (true || !IsEmptyTrayMode)
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape: scanSelfErrors(_debugOption = -1); needsToRefresh = true; break;
                    case Keys.X: scanSelfErrors(_debugOption = 0); needsToRefresh = true; break;
                    case Keys.Y: scanSelfErrors(_debugOption = 1); needsToRefresh = true; break;
                    case Keys.B: scanSelfErrors(_debugOption = 2); needsToRefresh = true; break;
                }
            }

            if (needsToRefresh)
                viewer.Invalidate();

            base.OnKeyDown(viewer, e);
        }
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            if (_matchResult != null)
            {
                bool isWorld = viewer.IsInWorldCoordinate();
                if (!isWorld)
                    viewer.SwitchToWorldCoordinate(gxView);

                if (IsEmptyTrayMode)
                    drawEmptyTrayResult(viewer, gxView);
                else
                    drawCalibResult(viewer, gxView);

                //// Cursors
                //draw_cursor(viewer, gxView, _cursorBloc2, Color.White);
                //draw_cursor(viewer, gxView, _cursorBloc, Color.Gold);
                //draw_line(viewer, gxView, _cursorBloc, _cursorBloc2, Color.Cyan);

                if (!isWorld)
                    viewer.SwitchToViewportCoordinate(gxView);
            }
            base.OnDraw(viewer, gxView);
        }
        public override bool OnMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            //return handleMouseMove(viewer, e);
            return base.OnMouseMove(viewer, e);
        }
        #endregion

        void drawEmptyTrayResult(CvImageViewer viewer, Graphics gxView)
        {
            // 畫出 grid 節點線
            draw_grid_lines(viewer, gxView, _matchResult?.Grid);
            // 畫出 正常 Blocs (有吸嘴)
            draw_bloc_rects(viewer, gxView, _suckerBlocs, Color.Lime, Color.DarkGreen, 0.05f);
            // 畫記 異常 Blocs (沒有吸嘴)
            draw_bloc_rects(viewer, gxView, iter_ng_blocs(), Color.Red, Color.DarkRed, 0.25f);
        }
        void drawCalibResult(CvImageViewer viewer, Graphics gxView)
        {
            // 畫出 正常 Blocs (有吸嘴)
            draw_bloc_rects(viewer, gxView, _suckerBlocs, Color.Blue, Color.DarkBlue, 0.25f);
        }

        #region PRIVATE_FUNCTIONS
        /// <summary>
        /// 枚舉 Empty Blocs (沒有吸嘴)
        /// </summary>
        IEnumerable<EzBloc> iter_ng_blocs()
        {
            if (_grid != null)
            {
                foreach (var bloc in _grid.IterPredictedBlocs())
                {
                    if (bloc != null)
                        yield return bloc;
                }
            }
            if (_outGridBlocs != null)
            {
                foreach (var bloc in _outGridBlocs)
                {
                    if (bloc != null)
                        yield return bloc;
                }
            }
        }
        void draw_grid_lines(CvImageViewer viewer, Graphics gxView, EzBlocsGrid grid)
        {
            if (grid == null)
                return;

            bool isWorldDrawing = viewer.IsInWorldCoordinate();
            var pen = viewer.GetOnePixelPen(Color.Gray);
            void draw_line(EzBloc from, EzBloc to)
            {
                if (from != null && to != null)
                {
                    var lx = (float)from.Center.X;
                    var ly = (float)from.Center.Y;
                    var cx = (float)to.Center.X;
                    var cy = (float)to.Center.Y;
                    if (!isWorldDrawing)
                    {
                        viewer.TransWorldToViewport(ref lx, ref ly);
                        viewer.TransWorldToViewport(ref cx, ref cy);
                    }
                    gxView.DrawLine(pen, lx, ly, cx, cy);
                }
            }

            int rows = grid.Rows;
            int cols = grid.Cols;
            for (int r = 0; r < rows; r++)
            {
                EzBloc last = null;
                for (int c = 0; c < cols; c++)
                {
                    var bloc = grid.Get(r, c);
                    if (bloc == null)
                        continue;

                    draw_line(last, bloc);
                    last = bloc;
                }
            }
            for (int c = 0; c < cols; c++)
            {
                EzBloc last = null;
                for (int r = 0; r < rows; r++)
                {
                    var bloc = grid.Get(r, c);
                    if (bloc == null)
                        continue;

                    draw_line(last, bloc);
                    last = bloc;
                }
            }
        }
        void draw_bloc_rects(CvImageViewer viewer, Graphics gxView, IEnumerable<EzBloc> blocs, Color color, Color color2, float blend = 0)
        {
            if (blocs == null)
                return;

            bool isWorldDrawing = viewer.IsInWorldCoordinate();

            // Blending alpah
            int alpha = blend > 0 && blend <= 1 ? (int)(255 * blend) : 0;
            Brush brush = alpha > 0 ? new SolidBrush(Color.FromArgb(alpha, color)) : null;

            foreach (var bloc in blocs)
            {
                if (bloc == null)
                    continue;

                var pen = bloc.IsMajorNode() ? viewer.GetOnePixelPen(color) : viewer.GetOnePixelPen(color2);

                if (_debugOption >= 0)
                {
                    brush?.Dispose();
                    brush = getDebugBrush(bloc);
                }

                if (isWorldDrawing)
                {
                    if (brush != null)
                        gxView.FillRectangle(brush, bloc.Rect);
                    gxView.DrawRectangle(pen, bloc.Rect);
                }
                else
                {
                    var rect = bloc.Rect;
                    viewer.TransCoordToView(ref rect);

                    if (brush != null)
                        gxView.FillRectangle(brush, bloc.Rect);

                    gxView.DrawRectangle(pen, rect);
                }
            }

            brush?.Dispose();
        }
        //void draw_cursor(CvImageViewer viewer, Graphics gxView, EzBloc bloc, Color color)
        //{
        //    if (bloc == null)
        //        return;

        //    var pen = viewer.GetOnePixelPen(color);
        //    int cx = bloc.CenterX;
        //    int cy = bloc.CenterY;
        //    int cw = bloc.Rect.Width / 2;
        //    int ch = bloc.Rect.Height / 2;

        //    bool isWorldDrawing = viewer.IsInWorldCoordinate();
        //    if (!isWorldDrawing)
        //    {
        //        int x2 = cx + cw;
        //        int y2 = cy + ch;
        //        viewer.TransCoordToView(ref cx, ref cy);
        //        viewer.TransCoordToView(ref x2, ref y2);
        //        cw = x2 - cx;
        //        ch = y2 - cy;
        //    }

        //    gxView.DrawLine(pen, cx - cw, cy, cx + cw, cy);
        //    gxView.DrawLine(pen, cx, cy - ch, cx, cy + ch);
        //}
        //void draw_line(CvImageViewer viewer, Graphics gxView, EzBloc from, EzBloc to, Color color)
        //{
        //    if (from == null || to == null)
        //        return;

        //    var pen = viewer.GetOnePixelPen(color);
        //    var lx = (float)from.Center.X;
        //    var ly = (float)from.Center.Y;
        //    var cx = (float)to.Center.X;
        //    var cy = (float)to.Center.Y;

        //    bool isWorldDrawing = viewer.IsInWorldCoordinate();
        //    if (!isWorldDrawing)
        //    {
        //        viewer.TransWorldToViewport(ref lx, ref ly);
        //        viewer.TransWorldToViewport(ref cx, ref cy);
        //    }

        //    gxView.DrawLine(pen, lx, ly, cx, cy);
        //}
        //void draw_centroids(CvImageViewer viewer, Graphics gxView, IEnumerable<EzBloc> blocs, bool debug = false)
        //{
        //    if (blocs == null)
        //        return;

        //    //bool isWorldO = viewer.IsInWorldCoordinate();
        //    //if (isWorldO)
        //    //    viewer.SwitchToViewportCoordinate(gxView);

        //    bool isWorldDrawing = viewer.IsInWorldCoordinate();
        //    var penMajorMark = new Pen(Color.Green, 5f);            
        //    var majorPoints = new List<PointF>();
        //    var predictPoints = new List<PointF>();
        //    var residuals = new List<PointF>();

        //    foreach (var bloc in blocs)
        //    {
        //        if (bloc == null)
        //            continue;

        //        var cx = (float)bloc.Center.X;
        //        var cy = (float)bloc.Center.Y;
        //        if (!isWorldDrawing)
        //            viewer.TransWorldToViewport(ref cx, ref cy);

        //        if (bloc.Owner == null)
        //        {
        //            residuals.Add(new PointF(cx, cy));
        //        }
        //        else if (bloc.Tag is QuadLinkNode link)
        //        {
        //            majorPoints.Add(new PointF(cx, cy));

        //            for (int i = 0; i < 4; i++)
        //            {
        //                EzBloc next = link[(QuadLinkNode.Dir)i];
        //                if (next == null)
        //                    continue;

        //                // centroid lines
        //                if (debug && i < 2)
        //                {
        //                    var cx2 = (float)next.Center.X;
        //                    var cy2 = (float)next.Center.Y;
        //                    if (!isWorldDrawing)
        //                        viewer.TransWorldToViewport(ref cx2, ref cy2);

        //                    var penLine = viewer.GetOnePixelPen(Color.Gray);
        //                    gxView.DrawLine(penLine, cx, cy, cx2, cy2);
        //                }

        //                // 小箭頭
        //                if (penMajorMark != null)
        //                {
        //                    var v = (next.Center - bloc.Center);
        //                    v = v / v.NormLength * 10.0;
        //                    var pt = bloc.Center + v;
        //                    var cx3 = (float)pt.X;
        //                    var cy3 = (float)pt.Y;
        //                    if (!isWorldDrawing)
        //                        viewer.TransWorldToViewport(ref cx3, ref cy3);

        //                    gxView.DrawLine(penMajorMark, cx, cy, cx3, cy3);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            predictPoints.Add(new PointF(cx, cy));
        //        }
        //    }

        //    // Dot
        //    var dot = new RectangleF(0, 0, 8, 8);
        //    foreach (var pt in predictPoints)
        //    {
        //        Qcvt.SetCenter(ref dot, pt.X, pt.Y);
        //        gxView.FillRectangle(Brushes.Purple, dot);
        //    }
        //    foreach (var pt in majorPoints)
        //    {
        //        Qcvt.SetCenter(ref dot, pt.X, pt.Y);
        //        gxView.FillRectangle(Brushes.Lime, dot);
        //    }

        //    if (residuals.Count > 0)
        //    {
        //        var pen = viewer.GetOnePixelPen(Color.Pink);
        //        dot.Inflate(dot.Width/2, dot.Height/2);
        //        foreach (var pt in residuals)
        //        {
        //            Qcvt.SetCenter(ref dot, pt.X, pt.Y);
        //            gxView.DrawEllipse(pen, dot);
        //        }
        //    }

        //    penMajorMark?.Dispose();

        //    //if (isWorldO)
        //    //    viewer.SwitchToWorldCoordinate(gxView);
        //}
        #endregion

        #region PRIVATE_TOOL_TIP_FUNCTIONS
        //Point _hitPt = new Point();
        //Size _fetchSize = new Size(100, 100);
        //bool handleMouseMove(CvImageViewer viewer, MouseEventArgs e)
        //{
        //    if ((_grid != null || _suckerBlocs != null) && Visible && Enabled)
        //    {
        //        int xx = e.X;
        //        int yy = e.Y;

        //        viewer.TransViewportToWorld(ref xx, ref yy);

        //        //var boundary = viewer.GetWorldRect();
        //        //_fetchSize.Width = (int)Math.Max(100, boundary.Width / 50);
        //        //_fetchSize.Height = (int)Math.Max(100, boundary.Height / 50);

        //        var bloc = fetchOne(xx, yy);
        //        bool isChanged = updateTooltip(bloc, e.X, e.Y, viewer);
        //        return isChanged;
        //    }
        //    return false;
        //}
        //void adjustFetchSize()
        //{
        //    if (_grid != null)
        //    {
        //        foreach (var bloc in _grid.IterBlocs())
        //        {
        //            if (bloc != null)
        //            {
        //                //_fetchSize = bloc.Rect.Size;
        //                adjustFetchSize(bloc.Rect.Size);
        //                return;
        //            }
        //        }
        //    }
        //}

        //EzBloc fetchOne(int x, int y)
        //{
        //    if (_grid != null)
        //    {
        //        var blobs = fetchKNN(x, y, 1, _fetchSize, _grid.IterBlocs());
        //        if (blobs != null && blobs.Length > 0)
        //            return blobs[0];
        //    }

        //    if (_suckerBlocs != null)
        //    {
        //        var blobs = fetchKNN(x, y, 1, _fetchSize, _suckerBlocs);
        //        if (blobs != null && blobs.Length > 0)
        //            return blobs[0];
        //    }

        //    if (_outGridBlocs != null)
        //    {
        //        var blobs = fetchKNN(x, y, 1, _fetchSize, _outGridBlocs);
        //        if (blobs != null && blobs.Length > 0)
        //            return blobs[0];
        //    }

        //    return null;
        //}
        //EzBloc[] fetchKNN(int x, int y, int kNumber, Size range, IEnumerable<EzBloc> srcBlobs)
        //{
        //    bool needsToSort = (kNumber >= 0);

        //    if (kNumber <= 0)
        //    {
        //        // Get All
        //        kNumber = int.MaxValue;
        //    }

        //    //if (range == Size.Empty)
        //    //{
        //    //    range = m_sizeCell;
        //    //}

        //    Rectangle rectRange = new Rectangle(
        //            x - range.Width / 2,
        //            y - range.Height / 2,
        //            range.Width,
        //            range.Height
        //        );


        //    var knn = new List<KeyValuePair<EzBloc, int>>();

        //    foreach (var spot in srcBlobs)
        //    {
        //        if (spot == null)
        //            continue;

        //        //////if (chkList.IndexOf(spot) >= 0)
        //        //////{
        //        //////    System.Diagnostics.Trace.Assert(false);
        //        //////    continue;
        //        //////}

        //        if (rectRange.Contains(spot.CenterX, spot.CenterY))
        //        {
        //            var dx = x - spot.CenterX;
        //            var dy = y - spot.CenterY;
        //            var dSQ = dx * dx + dy * dy;
        //            knn.Add(new KeyValuePair<EzBloc, int>(spot, dSQ));
        //            //////chkList.Add(spot);
        //        }
        //    }

        //    if (knn.Count == 0)
        //        return null;

        //    if (needsToSort && knn.Count > 1)
        //    {
        //        int _compareSpots(KeyValuePair<EzBloc, int> kp1, KeyValuePair<EzBloc, int> kp2)
        //        {
        //            if (kp1.Value > kp2.Value)
        //                return 1;
        //            else if (kp1.Value < kp2.Value)
        //                return -1;
        //            return 0;
        //        }
        //        knn.Sort(_compareSpots);
        //    }

        //    kNumber = Math.Min(kNumber, knn.Count);
        //    var result = new EzBloc[kNumber];

        //    for (int k = 0; k < kNumber; k++)
        //        result[k] = (EzBloc)knn[k].Key;

        //    return result;
        //}
        //bool updateTooltip(EzBloc bloc, int vx, int vy, Control wnd)
        //{
        //    if (bloc == null)
        //    {
        //        _toolTip.Hide(wnd);

        //        if (_cursorBloc != null)
        //        {
        //            _cursorBloc = null;
        //            return true;
        //        }

        //        return false;
        //    }
        //    else
        //    {
        //        if (_hitPt.X == vx && _hitPt.Y == vy)
        //            return false;

        //        _cursorBloc = bloc;

        //        bool showScore = true;

        //        var sb = new StringBuilder();

        //        var rowCol = (bloc.Tag as QuadLinkNode)?.rowCol;
        //        if (rowCol != null)
        //            sb.Append("格點: [").AppendValues(rowCol.Row, rowCol.Col).AppendLine("]");

        //        appendCameraCoords(sb, _cursorBloc, _cursorBloc2);

        //        if (TransCameraToMotor != null)
        //        {
        //            appendMotorCoords(sb, _cursorBloc, _cursorBloc2);
        //            showScore = false;
        //        }
        //        if (TransCameraToWorld != null)
        //        {
        //            appendWorldCoords(sb, _cursorBloc, _cursorBloc2);
        //            showScore = false;
        //        }
        //        if (TransCameraToMotor != null && TransCameraToMotor != null && _cursorBloc2 == null && rowCol != null)
        //        {
        //            appendPlcCompensation(sb, _cursorBloc, rowCol.Row, rowCol.Col);
        //            showScore = false;
        //        }

        //        if (showScore || IsEmptyTrayMode)
        //        {
        //            sb.AppendLine();
        //            sb.AppendLine($"Score= {bloc.Score:0.00}");
        //            sb.AppendLine($"Size= {bloc.Rect.Width}x{bloc.Rect.Height}");
        //        }

        //        _toolTip.ForeColor = Color.Black;
        //        _toolTip.BackColor = Color.LightBlue;

        //        _toolTip.Show(sb.ToString(), wnd, vx + 10, vy + 10);
        //        _hitPt = new Point(vx, vy);
        //        return true;
        //    }
        //}
        #endregion

        void adjustFetchSize()
        {
            if (_grid != null)
            {
                foreach (var bloc in _grid.IterBlocs())
                {
                    if (bloc != null)
                    {
                        //_fetchSize = bloc.Rect.Size;
                        adjustFetchSize(bloc.Rect.Size);
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

            if (_suckerBlocs != null)
            {
                foreach (var bloc in _suckerBlocs)
                    if (bloc != null)
                        yield return bloc;
            }

            if (_outGridBlocs != null)
            {
                foreach (var bloc in _outGridBlocs)
                    if (bloc != null)
                        yield return bloc;
            }
        }
        protected override string composeTooltipText(EzBloc cursor, EzBloc cursor2)
        {
            if (cursor == null)
                return "";

            var cursorBloc = cursor;
            var cursorBloc2 = cursor2;

            bool showScore = true;

            var sb = new StringBuilder();

            var rowCol = (cursorBloc.Tag as QuadLinkNode)?.rowCol;
            if (rowCol != null)
                sb.Append("格點: [").AppendValues(rowCol.Row, rowCol.Col).AppendLine("]");

            appendCameraCoords(sb, cursorBloc, cursorBloc2);

            if (TransCameraToMotor != null)
            {
                appendMotorCoords(sb, cursorBloc, cursorBloc2);
                showScore = false;
            }
            if (TransCameraToWorld != null)
            {
                appendWorldCoords(sb, cursorBloc, cursorBloc2);
                showScore = false;
            }
            if (TransCameraToMotor != null && TransCameraToMotor != null && cursorBloc2 == null && rowCol != null)
            {
                appendPlcCompensation(sb, cursorBloc, rowCol.Row, rowCol.Col);
                showScore = false;
            }

            if (showScore || IsEmptyTrayMode)
            {
                sb.AppendLine();
                sb.AppendLine($"Score= {cursorBloc.Score:0.00}");
                sb.AppendLine($"Size= {cursorBloc.Rect.Width}x{cursorBloc.Rect.Height}");
            }
            return sb.ToString();
        }

        void appendCameraCoords(StringBuilder sb, EzBloc bloc, EzBloc bloc2)
        {
            if (bloc == null)
                return;

            sb.Append($"相機座標: ({bloc.Center.X:0.0}, {bloc.Center.Y:0.0})").AppendLine();

            if (bloc != null && bloc2 != null && bloc != bloc2)
            {
                var dv = bloc.Center - bloc2.Center;
                var dist = dv.NormLength;
                sb.AppendLine($"相機座標 dX = {dv.X:0.0} pix");
                sb.AppendLine($"相機座標 dY = {dv.Y:0.0} pix");
                sb.AppendLine($"相機座標 距離 = {dist:0.0} pix");
            }
        }
        void appendMotorCoords(StringBuilder sb, EzBloc bloc, EzBloc bloc2)
        {
            var transform = TransCameraToMotor;
            if (bloc == null || transform == null)
                return;

            var motorCoord = transform.Trans(bloc.Center);

            sb.AppendLine();
            sb.AppendLine($"吸嘴馬達座標 X = {motorCoord.X:0.000} mm");
            sb.AppendLine($"載台馬達座標 Y = {motorCoord.Y:0.000} mm");

            if (bloc != null && bloc2 != null && bloc != bloc2)
            {
                var motorCoord2 = transform.Trans(bloc2.Center);
                var dv = motorCoord - motorCoord2;
                double dist = dv.NormLength;
                sb.AppendLine($"馬達座標 dX = {dv.X:0.000} mm");
                sb.AppendLine($"馬達座標 dY = {dv.Y:0.000} mm");
                sb.AppendLine($"馬達座標 距離 = {dist:0.000} mm");
            }
        }
        void appendWorldCoords(StringBuilder sb, EzBloc bloc, EzBloc bloc2)
        {
            var transform = TransCameraToWorld;
            if (bloc == null || transform == null)
                return;

            var worldCoord = transform.Trans(bloc.Center);

            sb.AppendLine();
            sb.AppendLine($"Physic座標 X = {worldCoord.X:0.000} mm");
            sb.AppendLine($"Physic座標 Y = {worldCoord.Y:0.000} mm");

            if (bloc != null && bloc2 != null && bloc != bloc2)
            {
                var worldCoord2 = transform.Trans(bloc2.Center);
                var dv = worldCoord - worldCoord2;
                double dist = dv.NormLength;
                sb.AppendLine($"Physic座標 dX = {dv.X:0.000} mm");
                sb.AppendLine($"Physic座標 dY = {dv.Y:0.000} mm");
                sb.AppendLine($"Physic座標 距離 = {dist:0.000} mm");
            }
        }
        void appendPlcCompensation(StringBuilder sb, EzBloc bloc, int row, int col)
        {
            if (bloc == null)
                return;

            var transformsModel = GaMvcConfig.SysModel.TransformsModel;
            //(var dV, var dErr) = transformsModel.CalcPlcCompensation(ActiveCarrierID, ActiveSuckerRowID, bloc.Center, row, col);
            (var dV, var dErr) = transformsModel.CalcPlcCompensation(ActiveCarrierID, bloc.Center, row, col);

            //sb.AppendLine();
            sb.AppendLine($"Phy 變動值 ΔX = {dErr.X:0.000} mm");
            sb.AppendLine($"Phy 變動值 ΔY = {dErr.Y:0.000} mm");
            sb.AppendLine();
            sb.AppendLine($"PLC 補償量 dX = {dV.X:0.000} mm");
            sb.AppendLine($"PLC 補償量 dY = {dV.Y:0.000} mm");
        }

        #region DEBUG_TRACE
        void scanSelfErrors(int option)
        {
            //if (IsEmptyTrayMode) return;
            if (_grid == null) return;

            int rows = _grid.Rows;
            int cols = _grid.Cols;
            var transformsModel = GaMvcConfig.SysModel.TransformsModel;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var bloc = _grid[r, c];

                    (var dV, var dErr) = transformsModel.CalcPlcCompensation(ActiveCarrierID, ActiveSuckerRowID, bloc.Center, r, c);

                    double err;
                    if (option == 0)
                        err = Math.Abs(dErr.X);
                    else if (option == 1)
                        err = Math.Abs(dErr.Y);
                    else if (option == 2)
                        err = Math.Max(Math.Abs(dErr.X), Math.Abs(dErr.Y));
                    else
                        err = 0;
                    bloc.SQRatio = err;
                }
            }
        }
        Brush getDebugBrush(EzBloc bloc)
        {
            var err = Math.Abs(bloc.SQRatio);
            if (err < 0.010)
                return new SolidBrush(Color.FromArgb(64, Color.Blue));
            else if(err < 0.020)
                return new SolidBrush(Color.FromArgb(64, Color.Yellow));
            else if (err < 0.030)
                return new SolidBrush(Color.FromArgb(64, Color.Orange));
            else if(err < 0.050)
                return new SolidBrush(Color.FromArgb(64, Color.Red));
            else
                return new SolidBrush(Color.FromArgb(128, Color.Red));
        }
        #endregion
    }
}
