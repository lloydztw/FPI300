#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-31 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.ImageViewerEx;
using NLog.Time;
using System.Drawing;

namespace LaserAlignDX.Mvc.Gui
{
    public class CviLineSegmentsBox : CvImageViewerInteractor, IvDrawItem
    {
        #region PRIVATE_DATA
        PointF[][] _lines;
        Color _color;
        Brush _midPointBrush;
        float _midPointRadius;
        #endregion

        #region GUI_MEMBERS
#if (OPT_RESERVED)
        static StringFormat _strFormat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };
        Font _font = null;
#endif
        #endregion

        public CviLineSegmentsBox(Color color, params PointF[][] lines)
        {
            _color = color;
            _lines = lines;
        }
        public void Attach(params PointF[][] lines)
        {
            _lines = lines;
        }
        public void EnableMidPoint(Brush br, float radius)
        {
            _midPointBrush = br;
            _midPointRadius = radius;
        }
        public object Tag
        {
            get;
            set;
        }

        #region OVERRIDES
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            if (_lines == null || _lines.Length == 0)
                return;

            bool isWorld = viewer.IsInWorldCoordinate();
            if (!isWorld)
                viewer.SwitchToWorldCoordinate(gxView);

            Draw_Contents(viewer, gxView);

            if (!isWorld)
                viewer.SwitchToViewportCoordinate(gxView);
        }
        #endregion

        #region DRAW_FUNCTIONS
        void Draw_Contents(CvImageViewer viewer, Graphics gxView)
        {
            var pen = viewer.GetOnePixelPen(_color);

            foreach (var pts in _lines)
            {
                if (pts.Length < 2) continue;

                gxView.DrawLine(pen, pts[0], pts[1]);

                if (_midPointRadius > 0f && _midPointBrush != null)
                {
                    var x = (pts[0].X + pts[1].X) / 2f - _midPointRadius;
                    var y = (pts[0].Y + pts[1].Y) / 2f - _midPointRadius;
                    var d = _midPointRadius * 2f;
                    gxView.FillEllipse(_midPointBrush, x, y, d, d);
                }
            }
        }
        #endregion
    }
}
