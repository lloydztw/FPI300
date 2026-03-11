#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QMath;
using OpenCvSharp;
using System;
using System.Collections.Generic;


namespace JetEazy.Transform
{
    public class DynamicRangeUtil
    {
        public static Point2d[] Normalize(QVector[] coords, DynamicRange[] ranges)
        {
            var pts = Array.ConvertAll(coords, (c) =>
            {
                var pt = new Point2d()
                {
                    X = ranges[0].Normalize(c.X),
                    Y = ranges[1].Normalize(c.Y)
                };
                return pt;
            });
            return pts;
        }
        public static Point2d[] Normalize(QVector[,] coords, DynamicRange[] ranges)
        {
            var pts = new List<Point2d>();
            foreach (var c in coords)
            {
                if (c == null) continue;
                var pt = new Point2d()
                {
                    X = ranges[0].Normalize(c.X),
                    Y = ranges[1].Normalize(c.Y)
                };
                pts.Add(pt);
            }
            return pts.ToArray();
        }
        public static QVector[] DeNormalize(Point2d[] points, DynamicRange[] ranges)
        {
            var coords = Array.ConvertAll(points, (p) =>
            {
                double x = ranges[0].DeNormalize(p.X);
                double y = ranges[1].DeNormalize(p.Y);
                var coord = new QVector(x, y);
                return coord;
            });
            return coords;
        }
    }
}
