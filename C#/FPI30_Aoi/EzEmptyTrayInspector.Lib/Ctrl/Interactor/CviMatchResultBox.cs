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

using JetEazy;
using JetEazy.ImageViewerEx;
using JetEazy.Match;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace EzEmptyTrayInspector.Ctrl
{
    public class CviMatchResultBox : CvImageViewerInteractor
    {
        #region PRIVATE_DATA
        IList<EzBloc> _blocs;
        EzBlocsGrid _grid;
        #endregion

        #region GUI_MEMBERS
        ToolTip _toolTip = new ToolTip();
        #endregion

        #region RUNTIME_DATA
        //string _dumpFileName;
        #endregion

        public void Reset()
        {
            _grid = null;
            _blocs = null;
        }
        public void UpdateResult(EzBlocsGrid src, IList<EzBloc> pool = null)
        {
            _grid = src;
            _blocs = pool;
        }
        public void UpdateResult(EzEmptyTrayInspector.Model.MatchResult result)
        {
            _grid = result?.Grid;
            _blocs = result?.Blocs;
        }

        #region OVERRIDES
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            bool isWorld = viewer.IsInWorldCoordinate();
            if (!isWorld)
                viewer.SwitchToWorldCoordinate(gxView);

            // 畫出 grid 節點線
            draw_grid_lines(viewer, gxView, _grid);

            // DEBUG
            //>>> draw_centroids(viewer, gxView, _pool, debug: true);

            // 畫出 沒有分配到 grid 節點的 bloc 外框
            if (_grid == null )
            {
                draw_bloc_rects(viewer, gxView, iter_off_grid_blocs(), Color.Blue, Color.Blue);
            }

            draw_centroids(viewer, gxView, iter_off_grid_blocs());

#if (false)
            if (_grid != null)
            {
                // 畫出 位於 grid 節點的 bloc 外框
                draw_centroids(viewer, gxView, _grid.IterBlocs());

                // 畫出 位於 grid 節點的 bloc 中心點
                draw_bloc_rects(viewer, gxView, _grid.IterBlocs(), Color.Cyan, Color.DarkCyan);
            }
#endif

            // 畫出 有吸嘴 Blocs 外框
            draw_bloc_rects(viewer, gxView, _blocs, Color.Lime, Color.DarkGreen);

            // 標記 無吸嘴 Blocs
            draw_bloc_rects(viewer, gxView, iter_empty_blocs(), Color.Red, Color.DarkRed);


            if (!isWorld)
                viewer.SwitchToViewportCoordinate(gxView);
        }
        public override bool OnMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            return handleMouseMove(viewer, e);
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        /// <summary>
        /// 枚舉 沒有位於節點的 Blocs
        /// </summary>
        IEnumerable<EzBloc> iter_off_grid_blocs()
        {
            if (_blocs != null)
            {
                foreach (var bloc in _blocs)
                {
                    if (bloc != null && bloc.Owner == null)
                        yield return bloc;
                }
            }
        }
        /// <summary>
        /// 枚舉 Empty Blocs
        /// </summary>
        IEnumerable<EzBloc> iter_empty_blocs()
        {
            if (_grid != null)
            {
                foreach (var bloc in _grid.IterPredictedBlocs())
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
        void draw_bloc_rects(CvImageViewer viewer, Graphics gxView, IEnumerable<EzBloc> blocs, Color color, Color color2)
        {
            if (blocs == null)
                return;

            bool isWorldDrawing = viewer.IsInWorldCoordinate();

            foreach (var bloc in blocs)
            {
                if (bloc == null)
                    continue;

                var pen = bloc.IsMajorNode() ? viewer.GetOnePixelPen(color) : viewer.GetOnePixelPen(color2);

                if (isWorldDrawing)
                {
                    gxView.DrawRectangle(pen, bloc.Rect);
                }
                else
                {
                    var rect = bloc.Rect;
                    viewer.TransCoordToView(ref rect);
                    gxView.DrawRectangle(pen, rect);
                }
            }
        }
        void draw_centroids(CvImageViewer viewer, Graphics gxView, IEnumerable<EzBloc> blocs, bool debug = false)
        {
            if (blocs == null)
                return;

            //bool isWorldO = viewer.IsInWorldCoordinate();
            //if (isWorldO)
            //    viewer.SwitchToViewportCoordinate(gxView);

            bool isWorldDrawing = viewer.IsInWorldCoordinate();
            var penMajorMark = new Pen(Color.Green, 5f);            
            var majorPoints = new List<PointF>();
            var predictPoints = new List<PointF>();
            var residuals = new List<PointF>();

            foreach (var bloc in blocs)
            {
                if (bloc == null)
                    continue;

                var cx = (float)bloc.Center.X;
                var cy = (float)bloc.Center.Y;
                if (!isWorldDrawing)
                    viewer.TransWorldToViewport(ref cx, ref cy);

                if (bloc.Owner == null)
                {
                    residuals.Add(new PointF(cx, cy));
                }
                else if (bloc.Tag is QuadLinkNode link)
                {
                    majorPoints.Add(new PointF(cx, cy));

                    for (int i = 0; i < 4; i++)
                    {
                        EzBloc next = link[(QuadLinkNode.Dir)i];
                        if (next == null)
                            continue;

                        // centroid lines
                        if (debug && i < 2)
                        {
                            var cx2 = (float)next.Center.X;
                            var cy2 = (float)next.Center.Y;
                            if (!isWorldDrawing)
                                viewer.TransWorldToViewport(ref cx2, ref cy2);

                            var penLine = viewer.GetOnePixelPen(Color.Gray);
                            gxView.DrawLine(penLine, cx, cy, cx2, cy2);
                        }

                        // 小箭頭
                        if (penMajorMark != null)
                        {
                            var v = (next.Center - bloc.Center);
                            v = v / v.NormLength * 10.0;
                            var pt = bloc.Center + v;
                            var cx3 = (float)pt.X;
                            var cy3 = (float)pt.Y;
                            if (!isWorldDrawing)
                                viewer.TransWorldToViewport(ref cx3, ref cy3);

                            gxView.DrawLine(penMajorMark, cx, cy, cx3, cy3);
                        }
                    }
                }
                else
                {
                    predictPoints.Add(new PointF(cx, cy));
                }
            }
            
            // Dot
            var dot = new RectangleF(0, 0, 8, 8);
            foreach (var pt in predictPoints)
            {
                Qcvt.SetCenter(ref dot, pt.X, pt.Y);
                gxView.FillRectangle(Brushes.Purple, dot);
            }
            foreach (var pt in majorPoints)
            {
                Qcvt.SetCenter(ref dot, pt.X, pt.Y);
                gxView.FillRectangle(Brushes.Lime, dot);
            }

            if (residuals.Count > 0)
            {
                var pen = viewer.GetOnePixelPen(Color.Pink);
                dot.Inflate(dot.Width/2, dot.Height/2);
                foreach (var pt in residuals)
                {
                    Qcvt.SetCenter(ref dot, pt.X, pt.Y);
                    gxView.DrawEllipse(pen, dot);
                }
            }

            penMajorMark?.Dispose();

            //if (isWorldO)
            //    viewer.SwitchToWorldCoordinate(gxView);
        }
        #endregion

        #region PRIVATE_TOOL_TIP_FUNCTIONS
        Point _hitPt = new Point();
        Size _fetchSize = new Size(100, 100);
        bool handleMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            if ((_grid != null || _blocs != null) && Visible && Enabled)
            {
                int xx = e.X;
                int yy = e.Y;

                viewer.TransViewportToWorld(ref xx, ref yy);
                var boundary = viewer.GetWorldRect();
                _fetchSize.Width = (int)Math.Max(100, boundary.Width / 50);
                _fetchSize.Height = (int)Math.Max(100, boundary.Height / 50);

                var bloc = fetchOne(xx, yy);
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
            if (_blocs != null)
            {
                var blobs = fetchKNN(x, y, 1, _fetchSize, _blocs);
                if (blobs != null && blobs.Length > 0)
                    return blobs[0];
            }
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
        void updateTooltip(EzBloc bloc, int vx, int vy, Control wnd)
        {
            if (bloc == null)
            {
                _toolTip.Hide(wnd);
            }
            else
            {
                if (_hitPt.X == vx && _hitPt.Y == vy)
                    return;

                string msg = $"(x,y)=({bloc.CenterX},{bloc.CenterY}), score={bloc.Score:0.00}";
                if (bloc.Tag is QuadLinkNode node && node.rowCol != null)
                    msg = $"[{node.rowCol.Row},{node.rowCol.Col}] " + msg;
                _toolTip.Show(msg, wnd, vx + 10, vy + 10);

                _hitPt = new Point(vx, vy);
            }
        }
        #endregion
    }
}
