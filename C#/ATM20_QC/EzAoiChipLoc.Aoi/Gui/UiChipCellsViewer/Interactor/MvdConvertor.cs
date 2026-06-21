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

using JetEazy.QvMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using VisionDesigner;


namespace LaserAlignDX.Mvc.Gui.ChipCellsViewer
{
    internal static class MvdConvertor
    {
        public static QvBox2D ToBox2D(this CMvdRectangleF mvdRectF)
        {
            if (mvdRectF == null)
                return null;
            var cx = mvdRectF.CenterX;
            var cy = mvdRectF.CenterY;
            var cw = mvdRectF.Width;
            var ch = mvdRectF.Height;
            var angle = mvdRectF.Angle;
            var box2D = new QvBox2D();
            box2D.SetBox(new PointF(cx, cy), new SizeF(cw, ch));
            box2D.SetCenter(cx, cy);
            box2D.SetTheta(angle * Math.PI / 180);
            return box2D;
        }
        public static PointF[] ToCSharpLine(this CMvdLineSegmentF mvdLine)
        {
            if(mvdLine == null) 
                return null;
            return new[] { 
                new PointF(mvdLine.StartPoint.fX, mvdLine.StartPoint.fY),
                new PointF(mvdLine.EndPoint.fX, mvdLine.EndPoint.fY)
            };
        }
        public static PointF[][] ToCSharpLines(params CMvdLineSegmentF[] mvdLines)
        {
            if (mvdLines == null)
                return null;
            var lines = new List<PointF[]>();
            foreach(var mvdLine in mvdLines)
                if(mvdLine != null)
                    lines.Add(mvdLine.ToCSharpLine());
            if(lines.Count > 0)
                return lines.ToArray();
            return null;
        }
        public static PointF[][] ToCSharpLines(PointF offset, params CMvdLineSegmentF[] mvdLines)
        {
            var lines = ToCSharpLines(mvdLines);
            Offset(lines, offset);
            return lines;
        }
        public static void Offset(PointF[][] lines, PointF offset)
        {
            if (lines == null)
                return;
            foreach(var pts in lines)
            {
                for(var i = 0; i < pts.Length; i++)
                {
                    pts[i].X += offset.X;
                    pts[i].Y += offset.Y;
                }
            }
        }
    }
}
