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
using System.Drawing;

namespace LaserAlignDX.Mvc.Gui
{
    public class CviLineSegmentsBox : CvImageViewerInteractor, IvDrawItem
    {
        #region PRIVATE_DATA
        PointF[][] _lines;
        Color _color;
        #endregion

        #region GUI_MEMBERS
        static StringFormat _strFormat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };
        Font _font = null;
        #endregion

        public CviLineSegmentsBox(Color color, params PointF[][] lines)
        {
            _color = color;
            _lines = lines;
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
            foreach (var line in _lines)
            {
                if (line.Length >= 2)
                    gxView.DrawLine(pen, line[0], line[1]);
            }
        }
        #endregion
    }
}
