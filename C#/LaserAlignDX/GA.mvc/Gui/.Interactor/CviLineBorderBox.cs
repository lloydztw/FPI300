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
using JetEazy.QvMath;
using LeTian.AoiLib;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using static System.Net.Mime.MediaTypeNames;

namespace LaserAlignDX.Mvc.Gui
{
    public class CviLineBorderBox : CviRcpBox
    {
        #region PRIVATE_DATA
        //EzLSD.LineSegment _constraintLine;
        #endregion

        #region GUI_MEMBERS
        static StringFormat _strFormat = new StringFormat()
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Far,
        };
        Font _font = null;
        #endregion

        public CviLineBorderBox(Brush br, int lineWidth, int cornerSize) : base(br, lineWidth, cornerSize)
        {
        }
        public string Text
        {
            get;
            set;
        }
        public object Tag
        {
            get;
            set;
        }

        #region OVERRIDES
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            base.OnDraw(viewer, gxView);

            if (_font == null)
                _font = viewer.Font;

            bool isWorld = viewer.IsInWorldCoordinate();
            if (!isWorld)
                viewer.SwitchToWorldCoordinate(gxView);

            drawText(viewer, gxView);

            if (!isWorld)
                viewer.SwitchToViewportCoordinate(gxView);
        }
        #endregion

        #region DRAW_FUNCTIONS
        void drawText(CvImageViewer viewer, Graphics gxView)
        {
            if (string.IsNullOrEmpty(Text))
                return;

            var pt = Box.Location;
            gxView.DrawString(Text, _font, m_brush, pt.X + 8, pt.Y, _strFormat);
        }
        #endregion
    }
}
