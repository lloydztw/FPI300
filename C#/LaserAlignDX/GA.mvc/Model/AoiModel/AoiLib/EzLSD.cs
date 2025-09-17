#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-23 使用 LSD (C++) 偵測直線 (by LeTian Chang)
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
using System.Drawing;
using VisionDesigner;


namespace LeTian.AoiLib
{
    public class EzLSD
    {
        public class LineSegment
        {
            #region PRIVATE_DATA
            QVector _p1;
            QVector _p2;
            #endregion

            public PointF P1F { get => new PointF((float)_p1.X, (float)_p1.Y); }
            public PointF P2F { get => new PointF((float)_p2.X, (float)_p2.Y); }
            public QVector P1 { get => _p1; }
            public QVector P2 { get => _p2; }

            public LineSegment(double x1, double y1, double x2, double y2)
            {
                _p1 = new QVector(x1, y1);
                _p2 = new QVector(x2, y2);
            }
            public LineSegment(QVector p1, QVector p2)
            {
                _p1 = p1;
                _p2 = p2;
            }
            public LineSegment(PointF p1, PointF p2) : this(p1.X, p1.Y, p2.X, p2.Y)
            {
            }
            public LineSegment(Vec4f cvLine)
            {
                _p1 = new QVector(cvLine.Item0, cvLine.Item1);
                _p2 = new QVector(cvLine.Item2, cvLine.Item3);
            }

            public double GetSegmentLengthSQ()
            {
                var dx = _p1.X - _p2.X;
                var dy = _p1.Y - _p2.Y;
                return dx * dx + dy * dy;
            }
            public double GetSegmentLength()
            {
                return Math.Sqrt(GetSegmentLengthSQ());
            }

            public QVector CalcIntersectedPoint(LineSegment line2)
            {
                return CalculateIntersectPoint(this, line2);
            }
            public double CalcDistance(PointF point)
            {
                return CalculateDistance(this, point.X, point.Y);
            }
            public double CalcDistance(QVector point)
            {
                return CalculateDistance(this, point.X, point.Y);
            }
            public void Offset(double dx, double dy)
            {
                _p1.X += dx;
                _p1.Y += dy;
                _p2.X += dx;
                _p2.Y += dy;
            }
            
            public QVector ToVector(bool normalize = false)
            {
                //var p1 = new QVector(P1.X, P1.Y);
                //var p2 = new QVector(P2.X, P2.Y);
                var vect = _p2 - _p1;
                if (normalize)
                    vect = vect / vect.NormLength;
                return vect;
            }
            public QVector GetMidPoint()
            {
                //var x = (_p1.X + _p2.X) / 2.0;
                //var y = (_p1.Y + _p2.Y) / 2.0;
                //return new QVector(x, y);
                var mid = (_p1 + _p2) / 2.0;
                return mid;
            }

            public LineSegment CreateNewScale(double xscale, double yscale)
            {
                double x1 = _p1.X * xscale;
                double y1 = _p1.Y * yscale;
                double x2 = _p2.X * xscale;
                double y2 = _p2.Y * yscale;
                var line = new LineSegment((float)x1, (float)y1, (float)x2, (float)y2);
                return line;
            }

            static QVector CalculateIntersectPoint(LineSegment line1, LineSegment line2)
            {
                double p1_x = line1.P1.X, p1_y = line1.P1.Y;
                double p2_x = line1.P2.X, p2_y = line1.P2.Y;
                double p3_x = line2.P1.X, p3_y = line2.P1.Y;
                double p4_x = line2.P2.X, p4_y = line2.P2.Y;

                // 計算分母
                double denominator = (p4_y - p3_y) * (p2_x - p1_x) - (p4_x - p3_x) * (p2_y - p1_y);

                // 如果分母接近零，表示直線平行（或重疊）。
                if (Math.Abs(denominator) < 0.0001f)
                {
                    //_LOG.Error("{0} 兩線段接近平行, 沒有交點!", GetType().Name);
                    return null; // 平行，沒有單一交點。
                }

                // 計算參數 t，用於找出交點。
                double t = ((p3_x - p1_x) * (p4_y - p3_y) - (p3_y - p1_y) * (p4_x - p3_x)) / denominator;

                // 根據參數 t 計算交點的座標。
                double intersectionX = p1_x + t * (p2_x - p1_x);
                double intersectionY = p1_y + t * (p2_y - p1_y);

                return new QVector(intersectionX, intersectionY);
            }
            static double CalculateDistance(LineSegment lineSegment, double p_x, double p_y)
            {
                //var lf = new QxLineFormula();
                //lf.Build(lineSegment.P1, lineSegment.P2);
                //return lf.Distance(p_x, p_y);

                double p1_x = lineSegment.P1.X;
                double p1_y = lineSegment.P1.Y;
                double p2_x = lineSegment.P2.X;
                double p2_y = lineSegment.P2.Y;

                // 計算分子 (Numerator)
                // 這是向量 P1P2 和 P1P 的 Cross Product 的 Z 分量
                double numerator = Math.Abs((p2_x - p1_x) * (p1_y - p_y) - (p1_x - p_x) * (p2_y - p1_y));

                // 計算分母 (Denominator)
                // 這是線段 P1P2 的長度
                double denominator = (float)Math.Sqrt(Math.Pow(p2_x - p1_x, 2) + Math.Pow(p2_y - p1_y, 2));

                // 如果線段長度為零（P1 和 P2 重疊），則回傳點到 P1 的距離。
                if (Math.Abs(denominator) < 0.0001f)
                {
                    return (double)Math.Sqrt(Math.Pow(p_x - p1_x, 2) + Math.Pow(p_y - p1_y, 2));
                }

                return numerator / denominator;
            }

            public static void Offset(IEnumerable<LineSegment> lines, float dx, float dy)
            {
                foreach (LineSegment line in lines)
                    line?.Offset(dx, dy);
            }
            public static void MergeLines(LineSegment major, LineSegment minor)
            {
                var minorMidPt = minor.GetMidPoint();
                var mP1 = new QVector(major.P1.X, major.P1.Y);
                var mP2 = new QVector(major.P2.X, major.P2.Y);

                var dd0 = (mP1 - mP2).NormLengthSQ;
                var dd1 = (minorMidPt - mP1).NormLengthSQ;
                var dd2 = (minorMidPt - mP2).NormLengthSQ;

                if (dd1 > dd0 && dd1 > dd2)
                {
                    major._p2 = new QVector(minorMidPt);    // new PointF((float)minorMidPt.X, (float)minorMidPt.Y);
                }
                if (dd2 > dd0 && dd2 > dd1)
                {
                    major._p1 = new QVector(minorMidPt);    // new PointF((float)minorMidPt.X, (float)minorMidPt.Y);
                }
            }
        }
    }


    public static class EzLsdExt
    {
        public static EzLSD.LineSegment ToLineSegment(this CMvdLineSegmentF line)
        {
            return new EzLSD.LineSegment(
                    line.StartPoint.fX,
                    line.StartPoint.fY,
                    line.EndPoint.fX,
                    line.EndPoint.fY );
        }
    }
}
