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
using JetEazy.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public class CviCalibResultBox : CviAbsTooltipBox
    {
        public event EventHandler OnRequestDumpBindaryImage;

        #region PRIVATE_DATA
        MatchResult _matchResult;
        EzBlocsGrid _grid => _matchResult?.Grid;
        IList<EzBloc> _suckerBlocs => _matchResult?.Blocs;
        IList<EzBloc> _outGridBlocs => _matchResult?.OutGridBlocs;
        #endregion

        #region RUNTIME_DATA
        bool _isCtrlPressed = false;
        int _debugOption = -1;
        #endregion

        public void Reset()
        {
            _matchResult = null;
        }
        public void UpdateResult(EzBlocsGrid grid)
        {
            if (grid == null)
            {
                Reset();
                return;
            }

            var blocs = new List<EzBloc>(grid.IterBlocs());
            var matchResult = new MatchResult(0, grid, blocs);
            _matchResult = matchResult;
            adjustFetchSize();
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
            if (!Visible)
                return;

            _wndHost = viewer;
            _isCtrlPressed = e.Control;

            if (true || !IsEmptyTrayMode)
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape: scanSelfErrors(-1); break;
                    case Keys.X: scanSelfErrors(0); break;
                    case Keys.Y: scanSelfErrors(1); break;
                    case Keys.B: scanSelfErrors(2); break;
                    case Keys.D: dumpBinary(); break;
                }
            }

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
                _toolTip?.Hide(viewer);
                _isCtrlPressed = false;
                return false;
            }

            return base.OnMouseDown(viewer, e);
        }
        public override bool OnMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            return base.OnMouseMove(viewer, e);
        }
        #endregion

        #region PRIVATE_DRAW_FUNCTIONS
        void drawEmptyTrayResult(CvImageViewer viewer, Graphics gxView)
        {
            // 畫出 grid 節點線
            draw_grid_lines(viewer, gxView, _matchResult?.Grid);
            // 畫出 正常 Blocs (有吸嘴)
            draw_bloc_rects(viewer, gxView, _suckerBlocs, Color.Lime, Color.DarkGreen, 0.05f);
            // 畫記 異常 Blocs (疑似有料)
            draw_bloc_rects(viewer, gxView, iter_ng_blocs(), Color.Red, Color.DarkRed, 0.25f);
        }
        void drawCalibResult(CvImageViewer viewer, Graphics gxView)
        {
            // 畫出 有效的 校正格位 Blocs (有吸嘴)
            draw_bloc_rects(viewer, gxView, iter_calib_grid_node_blocs(), Color.Blue, Color.DarkBlue, 0.25f);
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

        #region PRIVATE_HELPER_FUNCTIONS
        /// <summary>
        /// 枚舉 疑似有料 區塊 Blocs
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
        /// <summary>
        /// 枚舉 校正 之 格位點
        /// </summary>
        IEnumerable<EzBloc> iter_calib_grid_node_blocs()
        {
            if (_grid != null)
            {
                int rows = _grid.Rows;
                int cols = _grid.Cols;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        var bloc = _grid.Get(r, c);
                        if (bloc != null && bloc.IsMajorNode())
                            yield return bloc;
                    }
                }
            }
        }
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
        protected override IEnumerable<EzBloc> iterFetchableBlocs(int camX, int camY)
        {
            if (IsEmptyTrayMode)
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
            else
            {
                foreach (var bloc in iter_calib_grid_node_blocs())
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
            (var motorDelta, var worldDelta) = transformsModel.CalcPlcCompensation(ActiveCarrierID, bloc.Center, row, col);

            //sb.AppendLine();
            sb.AppendLine($"Physic 變動值 ΔX = {worldDelta.X:0.000} mm");
            sb.AppendLine($"Physic 變動值 ΔY = {worldDelta.Y:0.000} mm");
            sb.AppendLine();
            sb.AppendLine($"PLC 格點 補償量 dX = {motorDelta.X:0.000} mm");
            sb.AppendLine($"PLC 格點 補償量 dY = {motorDelta.Y:0.000} mm");
        }

        #region MENU_STRIP_FUNCTIONS
        //Form _frmOwner;
        Control _wndHost;
        ContextMenuStrip _menuStrip;
        void initMenuStrip(Control wnd)
        {
            if (_menuStrip == null)
            {
                //_frmOwner = wnd.FindForm();
                _wndHost = wnd;

                wnd.HandleDestroyed += (s, e) => disposeMenuStrip();
                var menu0 = new ToolStripMenuItem("檢視 格位 自我誤差 &X");
                var menu1 = new ToolStripMenuItem("檢視 格位 自我誤差 &Y");
                var menu2 = new ToolStripMenuItem("檢視 格位 自我誤差 &Both XY");
                var menu3 = new ToolStripMenuItem("&Dump 保存 二值化 圖檔");
                menu0.Click += (s, e) => scanSelfErrors(0);
                menu1.Click += (s, e) => scanSelfErrors(1);
                menu2.Click += (s, e) => scanSelfErrors(2);
                menu3.Click += (s, e) => dumpBinary();
                _menuStrip = new ContextMenuStrip();
                _menuStrip.Items.Add(menu0);
                _menuStrip.Items.Add(menu1);
                _menuStrip.Items.Add(menu2);
                _menuStrip.Items.Add(menu3);
            }
        }
        void disposeMenuStrip()
        {
            _menuStrip?.Dispose();
            _menuStrip = null;
        }
        void popupMenuStrip(Control wnd, Point pt)
        {
            //var activeCellBloc = _cursorBloc as CellBloc;
            //if (activeCellBloc == null) return;
            _cursorBloc2 = null;
            _toolTip.Hide(wnd);
            initMenuStrip(wnd);
            _menuStrip.Show(wnd, pt);
        }
        #endregion

        #region DEBUG_TRACE_FUNCTIONS
        void dumpBinary()
        {
            OnRequestDumpBindaryImage?.Invoke(this, null);
            _wndHost?.Invalidate();
        }
        void scanSelfErrors(int option)
        {
            _debugOption = option;

            //if (IsEmptyTrayMode) return;
            if (_grid == null) return;

            int rows = _grid.Rows;
            int cols = _grid.Cols;
            var transformsModel = GaMvcConfig.SysModel.TransformsModel;

            double maxErr = 0;
            int maxErrRow = -1;
            int maxErrCol = -1;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var bloc = _grid[r, c];
                    if (bloc == null) continue;

                    (var motorDelta, var worldDelta) = transformsModel.CalcPlcCompensation(ActiveCarrierID, bloc.Center, r, c);

                    double err;
                    if (option == 0)
                        err = Math.Abs(worldDelta.X);
                    else if (option == 1)
                        err = Math.Abs(worldDelta.Y);
                    else if (option == 2)
                        err = Math.Max(Math.Abs(worldDelta.X), Math.Abs(worldDelta.Y));
                    else
                        err = 0;
                    bloc.SQRatio = err;

                    if(maxErr < err)
                    {
                        maxErr = err;
                        maxErrRow = r;
                        maxErrCol = c;
                    }
                }
            }

            string msg = $"格位座標 最大誤差 在 [{maxErrRow},{maxErrCol}] = {maxErr:0.000}";
            GaUtil.LOG(msg, Color.Purple);

            _wndHost?.Invalidate();
        }
        Brush getDebugBrush(EzBloc bloc)
        {
            var err = Math.Abs(bloc.SQRatio);
            if (err < 0.010)
                return new SolidBrush(Color.FromArgb(16, Color.Blue));
            else if(err < 0.020)
                return new SolidBrush(Color.FromArgb(64, Color.Blue));
            else if (err < 0.030)
                return new SolidBrush(Color.FromArgb(64, Color.Yellow));
            else if (err < 0.040)
                return new SolidBrush(Color.FromArgb(32, Color.Orange));
            else if (err < 0.050)
                return new SolidBrush(Color.FromArgb(64, Color.Orange));
            else if(err < 0.080)
                return new SolidBrush(Color.FromArgb(64, Color.Red));
            else
                return new SolidBrush(Color.FromArgb(128, Color.Red));
        }
        #endregion
    }
}
