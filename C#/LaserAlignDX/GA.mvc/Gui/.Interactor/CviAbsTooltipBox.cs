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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace LaserAlignDX.Mvc.Gui
{
    public abstract class CviAbsTooltipBox : CvImageViewerInteractor
    {
        #region GUI_MEMBERS
        protected ToolTip _toolTip = new ToolTip();
        protected EzBloc _cursorBloc = null;
        protected EzBloc _cursorBloc2 = null;
        #endregion

        #region PRIVATE_DATA
        Point _hitPt = new Point();
        Size _fetchSize = new Size(100, 100);
        #endregion

        #region OVERRIDES
        public override void OnKeyDown(CvImageViewer viewer, KeyEventArgs e)
        {
            bool needsToRefresh = false;

            // 設定 cursor2
            if (e.Control && _cursorBloc != null)
            {
                _cursorBloc2 = _cursorBloc;
                needsToRefresh = true;
            }
            // 清除 cursor2
            else if (e.KeyCode == Keys.Escape && _cursorBloc2 != null)
            {
                _cursorBloc2 = null;
                needsToRefresh = true;
            }
            else
            {
            }

            if (needsToRefresh)
                viewer.Invalidate();

            base.OnKeyDown(viewer, e);
        }
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            bool isWorld = viewer.IsInWorldCoordinate();
            if (!isWorld)
                viewer.SwitchToWorldCoordinate(gxView);

            draw_cursor(viewer, gxView, _cursorBloc2, Color.White);
            draw_cursor(viewer, gxView, _cursorBloc, Color.Gold);
            draw_line(viewer, gxView, _cursorBloc, _cursorBloc2, Color.Cyan);

            if (!isWorld)
                viewer.SwitchToViewportCoordinate(gxView);
        }
        public override bool OnMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            return handleMouseMove(viewer, e);
        }
        #endregion

        #region DRAW_FUNCTIONS
        //void draw_grid_lines(CvImageViewer viewer, Graphics gxView, EzBlocsGrid grid)
        //{
        //    if (grid == null)
        //        return;

        //    bool isWorldDrawing = viewer.IsInWorldCoordinate();
        //    var pen = viewer.GetOnePixelPen(Color.Gray);
        //    void draw_line(EzBloc from, EzBloc to)
        //    {
        //        if (from != null && to != null)
        //        {
        //            var lx = (float)from.Center.X;
        //            var ly = (float)from.Center.Y;
        //            var cx = (float)to.Center.X;
        //            var cy = (float)to.Center.Y;
        //            if (!isWorldDrawing)
        //            {
        //                viewer.TransWorldToViewport(ref lx, ref ly);
        //                viewer.TransWorldToViewport(ref cx, ref cy);
        //            }
        //            gxView.DrawLine(pen, lx, ly, cx, cy);
        //        }
        //    }

        //    int rows = grid.Rows;
        //    int cols = grid.Cols;
        //    for (int r = 0; r < rows; r++)
        //    {
        //        EzBloc last = null;
        //        for (int c = 0; c < cols; c++)
        //        {
        //            var bloc = grid.Get(r, c);
        //            if (bloc == null)
        //                continue;

        //            draw_line(last, bloc);
        //            last = bloc;
        //        }
        //    }
        //    for (int c = 0; c < cols; c++)
        //    {
        //        EzBloc last = null;
        //        for (int r = 0; r < rows; r++)
        //        {
        //            var bloc = grid.Get(r, c);
        //            if (bloc == null)
        //                continue;

        //            draw_line(last, bloc);
        //            last = bloc;
        //        }
        //    }
        //}
        //void draw_bloc_rects(CvImageViewer viewer, Graphics gxView, IEnumerable<EzBloc> blocs, Color color, Color color2, float blend = 0)
        //{
        //    if (blocs == null)
        //        return;

        //    bool isWorldDrawing = viewer.IsInWorldCoordinate();

        //    // Blending alpah
        //    int alpha = blend > 0 && blend <= 1 ? (int)(255 * blend) : 0;
        //    Brush brush = alpha > 0 ? new SolidBrush(Color.FromArgb(alpha, color)) : null;

        //    foreach (var bloc in blocs)
        //    {
        //        if (bloc == null)
        //            continue;

        //        var pen = bloc.IsMajorNode() ? viewer.GetOnePixelPen(color) : viewer.GetOnePixelPen(color2);

        //        if (_debugOption >= 0)
        //        {
        //            brush?.Dispose();
        //            brush = getDebugBrush(bloc);
        //        }

        //        if (isWorldDrawing)
        //        {
        //            if (brush != null)
        //                gxView.FillRectangle(brush, bloc.Rect);
        //            gxView.DrawRectangle(pen, bloc.Rect);
        //        }
        //        else
        //        {
        //            var rect = bloc.Rect;
        //            viewer.TransCoordToView(ref rect);

        //            if (brush != null)
        //                gxView.FillRectangle(brush, bloc.Rect);

        //            gxView.DrawRectangle(pen, rect);
        //        }
        //    }

        //    brush?.Dispose();
        //}
        void draw_cursor(CvImageViewer viewer, Graphics gxView, EzBloc bloc, Color color)
        {
            if (bloc == null)
                return;

            var pen = viewer.GetOnePixelPen(color);
            int cx = bloc.CenterX;
            int cy = bloc.CenterY;
            int cw = bloc.Rect.Width / 2;
            int ch = bloc.Rect.Height / 2;

            bool isWorldDrawing = viewer.IsInWorldCoordinate();
            if (!isWorldDrawing)
            {
                int x2 = cx + cw;
                int y2 = cy + ch;
                viewer.TransCoordToView(ref cx, ref cy);
                viewer.TransCoordToView(ref x2, ref y2);
                cw = x2 - cx;
                ch = y2 - cy;
            }

            gxView.DrawLine(pen, cx - cw, cy, cx + cw, cy);
            gxView.DrawLine(pen, cx, cy - ch, cx, cy + ch);
        }
        void draw_line(CvImageViewer viewer, Graphics gxView, EzBloc from, EzBloc to, Color color)
        {
            if (from == null || to == null)
                return;

            var pen = viewer.GetOnePixelPen(color);
            var lx = (float)from.Center.X;
            var ly = (float)from.Center.Y;
            var cx = (float)to.Center.X;
            var cy = (float)to.Center.Y;

            bool isWorldDrawing = viewer.IsInWorldCoordinate();
            if (!isWorldDrawing)
            {
                viewer.TransWorldToViewport(ref lx, ref ly);
                viewer.TransWorldToViewport(ref cx, ref cy);
            }

            gxView.DrawLine(pen, lx, ly, cx, cy);
        }
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
        //        dot.Inflate(dot.Width / 2, dot.Height / 2);
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

        #region PRIVATE_FETCH_FUNCTIONS
        protected EzBloc fetchOne(int x, int y)
        {
            var blobs = fetchKNN(x, y, 1, _fetchSize, iterFetchableBlocs());
            if (blobs != null && blobs.Length > 0)
                return blobs[0];
            return null;
        }
        protected EzBloc[] fetchKNN(int x, int y, int kNumber, Size range, IEnumerable<EzBloc> srcBlobs)
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
        #endregion

        #region PRIVATE_TOOL_TIP_FUNCTIONS
        bool handleMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            if (Visible && Enabled)
            {
                int xx = e.X;
                int yy = e.Y;

                viewer.TransViewportToWorld(ref xx, ref yy);

                var bloc = fetchOne(xx, yy);

                bool isChanged = updateTooltip(bloc, e.X, e.Y, viewer);

                return isChanged;
            }
            return false;
        }
        bool updateTooltip(EzBloc curBloc, int vx, int vy, Control wnd)
        {
            if (curBloc == null)
            {
                _toolTip.Hide(wnd);
                bool isChanged = _cursorBloc != null;
                _cursorBloc = null;
                return isChanged;
            }
            else
            {
                if (_hitPt.X == vx && _hitPt.Y == vy)
                    return false;

                _cursorBloc = curBloc;

                string txt = composeTooltipText(_cursorBloc, _cursorBloc2);
                if (!string.IsNullOrEmpty(txt))
                    _toolTip.Show(txt, wnd, vx + 10, vy + 10);

                _hitPt = new Point(vx, vy);
                return true;
            }
        }
        #endregion

        protected void adjustFetchSize(Size sz)
        {
            _fetchSize = sz;
        }
        protected virtual IEnumerable<EzBloc> iterFetchableBlocs()
        {
            yield break;
        }
        protected virtual string composeTooltipText(EzBloc cursor, EzBloc cursor2)
        {
            return null;
        }
    }
}
