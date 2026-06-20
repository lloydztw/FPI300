#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2023-08-23 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.ImageViewerEx;
using JetEazy.QMath;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace EzAoiChipLocQC.Ctrl
{
    public class CviMouseSync : CvImageViewerInteractor
    {
        #region PRIVATE_MEMBERS
        private List<IvImageViewer> m_viewers;
        private IvImageViewer m_activeView;
        private bool m_showCursor = true;
        private bool m_isMouseDown;
        private int m_xCur;
        private int m_yCur;
        #endregion

        public CviMouseSync()
        {
            //> SetCursors(Cursors.Hand, Cursors.Hand);
        }
        public Color CursorColor { get; set; } = Color.Goldenrod;
        public void Attach(params IvImageViewer[] viewers)
        {
            m_viewers = new List<IvImageViewer>(viewers);
            m_activeView = m_viewers.Count > 0 ? m_viewers[0] : null;
            foreach (var view in m_viewers)
            {
                view.AddInteractor(this);
            }
        }
        public void Detach()
        {
            if (m_viewers != null)
            {
                foreach (var view in m_viewers)
                {
                    view.RemoveInteractor(this);
                }
            }
            m_viewers = null;
        }

        #region OFFSETs
        private Dictionary<object, QVector> _offsets = new Dictionary<object, QVector>();
        #endregion

        public void ClearOffsets()
        {
            _offsets.Clear();
        }
        public void SetOffset(object view, QVector offset)
        {
            if (_offsets.ContainsKey(view))
                _offsets[view] = offset;
            else
                _offsets.Add(view, offset);
        }
        QVector GetOffset(object view)
        {
            if(_offsets.TryGetValue(view, out var offset))
                return offset;
            return new QVector(0, 0);
        }
        
        #region FUNCTIONS
        public override bool OnMouseDown(CvImageViewer viewer, MouseEventArgs e)
        {
            _TRACE("OnMouseDown [{0:X}] {1} {2}", viewer.Handle, e.Location, e.Button);
            m_activeView = viewer;

            bool bRedraw = base.OnMouseDown(viewer, e);
            m_isMouseDown = true;
            m_xCur = e.Location.X;
            m_yCur = e.Location.Y;
            return bRedraw;
        }
        public override bool OnMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            bool bRedraw = base.OnMouseMove(viewer, e);
            m_xCur = e.Location.X;
            m_yCur = e.Location.Y;

            if (m_isMouseDown)
            {
                if (viewer == m_activeView)
                {
                    viewer.Refresh();
                    bRedraw = false;
                    foreach (var v in m_viewers)
                    {
                        if (v != m_activeView)
                            syncViewport(v, m_activeView, true);
                    }
                }
            }
            else if (m_showCursor)
            {
                m_activeView = viewer;
                if (viewer == m_activeView)
                {
                    viewer.Refresh();
                    bRedraw = false;
                    foreach (var v in m_viewers)
                    {
                        if (v != viewer)
                            refresh(v);
                    }
                }
            }

            return bRedraw;
        }
        public override bool OnMouseUp(CvImageViewer viewer, MouseEventArgs e)
        {
            bool bRedraw = base.OnMouseUp(viewer, e);

            if (m_isMouseDown)
            {
                m_isMouseDown = false;
                m_xCur = e.Location.X;
                m_yCur = e.Location.Y;

                if (viewer == m_activeView)
                {
                    viewer.Refresh();
                    bRedraw = false;
                    foreach (var v in m_viewers)
                    {
                        if (v != viewer)
                            syncViewport(v, viewer, bRedraw);
                    }
                }
            }

            return bRedraw;
        }
        public override bool OnMouseWheel(CvImageViewer viewer, MouseEventArgs e)
        {
            bool bRedraw = base.OnMouseWheel(viewer, e);
            m_xCur = e.Location.X;
            m_yCur = e.Location.Y;

            m_activeView = viewer;
            if (viewer == m_activeView)
            {
                viewer.Refresh();
                bRedraw = false;
                foreach (var v in m_viewers)
                {
                    if (v != viewer)
                    {
                        syncZoom(v, viewer, false);
                        syncViewport(v, viewer, false);
                        refresh(v);
                    }
                }
            }

            return bRedraw;
        }
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            base.OnDraw(viewer, gxView);
            drawCursor(viewer, gxView);
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void drawCursor(IvImageViewer viewer, Graphics gx)
        {
            if (m_showCursor)
            {
                // RESERVED 大圖反應很慢
                using (var pen = new Pen(CursorColor))
                {
                    float w = 16;
                    float h = 16;
                    bool needsToSwitch = viewer.IsInWorldCoordinate();
                    if (needsToSwitch) viewer.SwitchToViewportCoordinate(gx);
                    gx.DrawLine(pen, m_xCur - w, m_yCur, m_xCur + w, m_yCur);
                    gx.DrawLine(pen, m_xCur, m_yCur - h, m_xCur, m_yCur + h);
                    if (needsToSwitch) viewer.SwitchToWorldCoordinate(gx);
                }
            }
        }
        private void syncZoom(IvImageViewer from, IvImageViewer to, bool bRedraw)
        {
            if (from == to)
                return;
            
            _TRACE("syncZoom [{0:x}]", ((Control)from).Handle);
            from.Zoom(to.GetZoomScale());
            if (bRedraw)
                refresh(from);
        }
        private void syncViewport(IvImageViewer from, IvImageViewer to, bool bRedraw)
        {
            //----------------------------------------------------
            // NOTE:  to 是固定視窗, from 是從動視窗
            //----------------------------------------------------

            if (from == to)
                return;

            _TRACE("syncViewport [{0:x}]", ((Control)from).Handle);

#if (false)
            var rect = to.GetWorldRect();
            to.TransWorldToViewport(ref rect);
            var vptDst = JetEazy.Qcvt.Center(ref rect);

            rect = from.GetWorldRect();
            from.TransWorldToViewport(ref rect);
            var vptMov = JetEazy.Qcvt.Center(ref rect);
#else
            var dst_rect = to.GetWorldRect();
            var dst_offset = GetOffset(to);
            dst_rect.X += (float)dst_offset.X;
            dst_rect.Y += (float)dst_offset.Y;
            to.TransWorldToViewport(ref dst_rect);
            var vptDst = JetEazy.Qcvt.Center(ref dst_rect);

            var mov_rect = from.GetWorldRect();
            var mov_offset = GetOffset(from);
            mov_rect.X += (float)mov_offset.X;
            mov_rect.Y += (float)mov_offset.Y;
            from.TransWorldToViewport(ref mov_rect);
            var vptMov = JetEazy.Qcvt.Center(ref mov_rect);
#endif

            if (vptMov != vptDst)
            {
                var pt0 = Point.Round(vptMov);
                var pt2 = Point.Round(vptDst);
                from.MoveViewport(pt0.X, pt0.Y, pt2.X, pt2.Y);
                bRedraw = true;
            }

            if (bRedraw)
                refresh(from);
        }
        private void refresh(IvImageViewer viewer)
        {
            if (viewer is Control wnd)
            {
                wnd.Refresh();
            }
        }
        new void _TRACE(string fmt, params object[] args)
        {
        }
        #endregion
    }
}
