#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-10-05 精算優化 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QMath;
using System;
using System.Drawing;

namespace JetEazy.QvMath
{
    public class QvQuad2D : ICloneable
    {
        #region CONFIG
        const int NP = 4;
        #endregion

        #region PRIVATE_DATA
        //------------------------------------------
        // 順序: 0:左上, 1:右上, 2:右下, 3:左下
        //------------------------------------------
        //      0  1
        //      3  2 
        //------------------------------------------
        private QVector[] _corners = new QVector[NP]
        {
            new QVector(0, 0),
            new QVector(0, 0),
            new QVector(0, 0),
            new QVector(0, 0),
        };
        #endregion

        public QVector[] Corners
        {
            get
            {
                return _corners;
            }
            set
            {
                if (value != null && value.Length >= NP)
                {
                    _corners = Array.ConvertAll(value, p => p != null ? p : new QVector(0, 0));
                    SortByRelativeTheta();
                }
            }
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        public QvQuad2D Clone()
        {
            var corners = Array.ConvertAll(_corners, c => c != null ? new QVector(c) : new QVector(0, 0));
            var obj = new QvQuad2D
            {
                _corners = corners
            };
            return obj;
        }

        public QVector Center
        {
            get => GetCenter();
            set => SetCenter(value);
        }
        public double Theta
        {
            get => GetTheta();
            set => SetTheta(value);
        }
        public double Angle
        {
            get => GetTheta() * 180.0 / Math.PI;
            set => SetTheta(value / 180.0 * Math.PI);
        }

        public RectangleF BoundaryRect
        {
            get
            {
                GetRoundaryRect(out var rect);
                return rect;
            }
        }
        public void GetRoundaryRect(out RectangleF rect, int digits = -1)
        {
            var x = double.MaxValue;
            var y = double.MaxValue;
            var x2 = double.MinValue;
            var y2 = double.MinValue;

            foreach (var c in _corners)
            {
                x = Math.Min(x, c.X);
                y = Math.Min(y, c.Y);
                x2 = Math.Max(x2, c.X);
                y2 = Math.Max(y2, c.Y);
            }
            var w = x2 - x;
            var h = y2 - y;

            if (digits >= 0)
            {
                x = Math.Round(x, digits);
                y = Math.Round(y, digits);
                w = Math.Round(w, digits);
                h = Math.Round(h, digits);
            }

            rect = new RectangleF((float)x, (float)y, (float)(x2 - x), (float)(y2 - y));
        }
        public void GetMidSize(out SizeF size, int digits = -1)
        {
            //------------------------------------------
            // 順序: 0:左上, 1:右上, 2:右下, 3:左下
            //------------------------------------------
            //      0  1
            //      3  2 
            //------------------------------------------
            var L = (_corners[0] + _corners[3]) / 2;
            var R = (_corners[1] + _corners[2]) / 2;
            var T = (_corners[0] + _corners[1]) / 2;
            var B = (_corners[3] + _corners[2]) / 2;

            double w = (L - R).NormLength;
            double h = (T - B).NormLength;

            if (digits >= 0)
            {
                w = Math.Round(w, digits);
                h = Math.Round(h, digits);
            }

            size = new SizeF((float)w, (float)h);
        }

        public QVector GetCenter()
        {
            QVector center = new QVector(0, 0);
            foreach (var p in _corners)
            {
                center = center + p;
            }
            center.X /= (double)NP;
            center.Y /= (double)NP;
            return center;
        }
        public void SetCenter(QVector pt)
        {
            if (pt != null)
                SetCenter(pt.X, pt.Y);
        }
        public void SetCenter(double x, double y)
        {
            var org = GetCenter();
            var dx = x - org.X;
            var dy = y - org.Y;
            Offset(dx, dy);
        }
        public void Offset(double dx, double dy)
        {
            foreach (var c in _corners)
            {
                c.X += dx;
                c.Y += dy;
            }
        }

        public double GetTheta()
        {
            //------------------------------------------
            // 順序: 0:左上, 1:右上, 2:右下, 3:左下
            //------------------------------------------
            //      0  1
            //      3  2 
            //------------------------------------------
            var L = (_corners[0] + _corners[3]) / 2;
            var R = (_corners[1] + _corners[2]) / 2;
            var vector = R - L;
            return _getTheta(vector);
        }
        public void SetTheta(double theta)
        {
            var thetaDiff = theta - GetTheta();
            if (Math.Abs(thetaDiff) < 1e-12)
                return;

            var center = GetCenter();
            foreach (var c in _corners)
                Rotate(c, center, thetaDiff, inplace: true);

            SortByRelativeTheta();
        }

        #region SORT_FUNCTIONS
        /// <summary>
        /// 按照相對偏移角度排序
        /// </summary>
        public void SortByRelativeTheta()
        {
            int indexBase = 0;
            double thetaMin = double.MaxValue;

            for (int i = 0; i < NP; i++)
            {
                var p0 = _corners[i];
                var p1 = _corners[(i + 1) % NP];
                var vect = p1 - p0;
                var theta = _getTheta(vect);

                if (thetaMin > Math.Abs(theta))
                {
                    thetaMin = Math.Abs(theta);
                    indexBase = i;
                }
            }

            if (indexBase != 0)
            {
                var newCorners = new QVector[NP];
                for (int j = 0; j < NP; j++)
                    newCorners[j] = _corners[(j + indexBase) % 4];
                _corners = newCorners;
            }
        }
        #endregion

        /// <summary>
        /// 將 pt 以 center 為圓心, 旋轉 theta
        /// </summary>
        public static QVector Rotate(QVector pt, QVector center, double theta, bool inplace)
        {
            if (pt == null || center == null)
                return null;

            var vect = pt - center;
            var mag = vect.NormLength;
            var th0 = _getTheta(vect);
            var thNew = theta + th0;

            var x = mag * Math.Cos(thNew) + center.X;
            var y = mag * Math.Sin(thNew) + center.Y;

            if (inplace)
            {
                pt.X = x;
                pt.Y = y;
            }
            else
            {
                pt = new QVector(x, y);
            }

            return pt;
        }
        public static QvQuad2D From(QvBox2D box2d)
        {
            if(box2d == null) return null;
            var quad = new QvQuad2D()
            {
                Corners = Array.ConvertAll(box2d.Corners, c => new QVector(c.X, c.Y))
            };
            return quad;
        }
        public static QvQuad2D From(RectangleF rect)
        {
            var quad = new QvQuad2D();
            quad.SetBox(rect.Location, rect.Size);
            return quad;
        }
        public static QvQuad2D From(ref RectangleF rect)
        {
            var quad = new QvQuad2D();
            quad.SetBox(rect.Location, rect.Size);
            return quad;
        }
        public QvBox2D ToBox2D()
        {
            var box2d = new QvBox2D
            {
                Corners = Array.ConvertAll(_corners, c => new PointF((float)c.X, (float)c.Y))
            };
            return box2d;
        }
        public void SetBox(PointF pt, SizeF size)
        {
            _corners[0] = new QVector(pt.X, pt.Y);
            _corners[1] = new QVector(pt.X + size.Width, pt.Y);
            _corners[2] = new QVector(pt.X + size.Width, pt.Y + size.Height);
            _corners[3] = new QVector(pt.X, pt.Y + size.Height);
        }

        #region PRIVATE_FUNCTIONS
        static double _getTheta(QVector p)
        {
            if (p == null) return 0;
            return Math.Atan2(p.Y, p.X);
        }
        #endregion
    }
}
