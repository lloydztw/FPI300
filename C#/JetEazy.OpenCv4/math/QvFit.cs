/****************************************************************************
 *                                                                          
 * Copyright (c) 2012 Jet Eazy Corp. All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$    
 *	    2013/05/22 The class is created by LeTian Chang
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/

using OpenCvSharp;
using System;
using System.Drawing;
using CvColor = OpenCvSharp.Scalar;
using CvPoint = OpenCvSharp.Point;

namespace JetEazy.BasicSpace
{
    public class QvFit
    {
        const int N_MAX_SEARCH_LOOPS = 50000;
        const double D_TOLERANCE = 0.00001;

        #region PRIVATE_DATA
        private bool m_bTraceEnabled = false;
        #endregion

        /// <summary>
        /// (x-a)^2 + (y-b)^2 = r^2
        /// </summary>
        /// <returns>radius error</returns>

        public double Fit(PointF[] pts, out PointF ptCenter, out float radius)
        {
            if (pts.Length < 3)
                throw new Exception("QCircleFit.Fit needs 3 points at least!");

            double xAve;
            double yAve;
            _getAverage(pts, out xAve, out yAve);

            double a = xAve;
            double b = yAve;
            double r = 0;
            double err = double.MaxValue;

            int iTry = 0;
            for (iTry = 0; iTry < N_MAX_SEARCH_LOOPS; iTry++)
            {
                double L = _getAveL(pts, a, b);

                err = Math.Abs(r - L);
                if (err < D_TOLERANCE)
                {
                    //System.Diagnostics.Trace.WriteLine("@@@ QCircleFit.Fit (loops) = " + i);
                    r = L;
                    break;
                }

                double LA = _getAveLA(pts, a, b);
                double LB = _getAveLB(pts, a, b);
                a = xAve + L * LA;
                b = yAve + L * LB;
                r = L;
            }

            ptCenter = new PointF((float)a, (float)b);
            radius = (float)r;

            if (m_bTraceEnabled)
            {
                _TRACE(pts, ptCenter, radius, err, iTry);
            }

            //System.Diagnostics.Trace.WriteLine("@@@ QCircleFit.Fit (error) = " + err);
            return err;
        }

        public bool TraceEnabled
        {
            get { return m_bTraceEnabled; }
            set { m_bTraceEnabled = value; }
        }

        #region PRIVATE_FUNCTIONS
        private void _getAverage(PointF[] pts, out double xAve, out double yAve)
        {
            int iCount = pts.Length;

            xAve = 0;
            yAve = 0;
            for (int i = 0; i < iCount; i++)
            {
                xAve += pts[i].X;
                yAve += pts[i].Y;
            }
            xAve /= iCount;
            yAve /= iCount;
        }
        private double _getL(ref PointF pt, double a, double b)
        {
            double dx = pt.X - a;
            double dy = pt.Y - b;
            double L = Math.Sqrt(dx * dx + dy * dy);
            return L;
        }
        private double _getAveL(PointF[] pts, double a, double b)
        {
            int iCount = pts.Length;

            double aveL = 0;
            for (int i = 0; i < iCount; i++)
            {
                double L = _getL(ref pts[i], a, b);
                aveL += L;
            }
            aveL /= iCount;

            return aveL;
        }
        private double _getAveLA(PointF[] pts, double a, double b)
        {
            int iCount = pts.Length;

            double aveLA = 0;
            for (int i = 0; i < iCount; i++)
            {
                double Li = _getL(ref pts[i], a, b);
                double La = (a - pts[i].X) / Li;
                aveLA += La;
            }
            aveLA /= iCount;

            return aveLA;
        }
        private double _getAveLB(PointF[] pts, double a, double b)
        {
            int iCount = pts.Length;

            double aveLB = 0;
            for (int i = 0; i < iCount; i++)
            {
                double Li = _getL(ref pts[i], a, b);
                double Lb = (b - pts[i].Y) / Li;
                aveLB += Lb;
            }
            aveLB /= iCount;

            return aveLB;
        }
        #endregion

        #region PRIVATE_VISUAL_TRACE_FUNCTION
        private void _TRACE(PointF[] pts, PointF ptCenter, float radius, double error, int iTryLoops)
        {
            int iCount = pts.Length;
            int xMin = int.MaxValue;
            int yMin = int.MaxValue;
            int xMax = int.MinValue;
            int yMax = int.MinValue;
            for (int i = 0; i < iCount; i++)
            {
                xMin = Math.Min(xMin, (int)pts[i].X);
                yMin = Math.Min(yMin, (int)pts[i].Y);
                xMax = Math.Max(xMax, (int)pts[i].X);
                yMax = Math.Max(yMax, (int)pts[i].Y);
            }

            int iWidth = 640;
            int iHeight = 480;
            int iSpanW = (int)((xMax - xMin) * 1.2);
            int iSpanH = (int)((yMax - yMin) * 1.2);
            double dZoomX = (double)iWidth / iSpanW;
            double dZoomY = (double)iHeight / iSpanH;
            double dZoom = Math.Min(dZoomX, dZoomY);
            int x0 = 0;
            int y0 = 0;
            if (dZoom >= 1)
            {
                dZoom = 1.0;
                xMin = 0;
                yMin = 0;
                x0 = 0;
                y0 = 0;
            }
            else
            {
                x0 = (iWidth - (int)((xMax - xMin) * dZoom)) / 2;
                y0 = (iHeight - (int)((yMax - yMin) * dZoom)) / 2;
            }

            //>>> using (IplImage img = new IplImage(iWidth, iHeight, BitDepth.U8, 3))
            using (var img = new Mat(iHeight, iWidth, MatType.CV_8UC3))
            {
                //>>> img.Zero();
                img.SetTo(CvColor.Black);

                for (int i = 0; i < pts.Length; i++)
                {
                    CvPoint pt = new CvPoint()
                    {
                        X = (int)((pts[i].X - xMin) * dZoom + x0),
                        Y = (int)((pts[i].Y - yMin) * dZoom + y0)
                    };
                    //>>> img.Circle(pt, 3, new CvColor(0, 255, 0), Cv.FILLED);
                    Cv2.Circle(img, pt, 3, CvColor.Green, -1);
                }

                CvPoint ptvCenter = new CvPoint()
                {
                    X = (int)((ptCenter.X - xMin) * dZoom + x0),
                    Y = (int)((ptCenter.Y - yMin) * dZoom + y0)
                };
                int iRadius = (int)(radius * dZoom);

                img.Circle(ptvCenter, iRadius, CvColor.White);

                string strTitle = string.Format("QCircleFit: Loops={0}, Err={1}", iTryLoops, error);
                //using (CvWindow w = new CvWindow(strTitle, WindowMode.AutoSize, img))
                //{
                //    CvWindow.WaitKey(0);
                //}
                using (var w = new OpenCvSharp.Window(strTitle, img, WindowFlags.AutoSize))
                {
                    Cv2.WaitKey(0);
                }
            }
        }
        #endregion

        public static void SelfTest()
        {
            //CvRNG rng = new CvRNG(DateTime.Now);
            var rng = new Random();
            PointF[] pts = new PointF[100];
            double R = 2000;
            double dPercent = 0.05;

            for (int i = 0; i < pts.Length; i++)
            {
                //int u = (int)rng.RandInt() % 360;
                int u = rng.Next() % 360;
                double a = (double)u / 180.0 * Math.PI;

                //u = (int)rng.RandInt() % 100 - 50;
                u = rng.Next() % 100 - 50;
                double dR = (u / 100.0) * dPercent * R;

                pts[i] = new PointF()
                {
                    X = (float)((R + dR) * Math.Cos(a) + R),
                    Y = (float)((R + dR) * Math.Sin(a) + R)
                };
            }

            float radius;
            PointF ptCenter;
            QvFit cFit = new QvFit();

            cFit.TraceEnabled = true;
            cFit.Fit(pts, out ptCenter, out radius);
        }
    }

    public class QvLineFit2
    {
        #region PRIVATE_DATA
        private double m_a = 0.0;
        private double m_b = 0.0;
        private bool m_bSwap = false;
        #endregion

        public bool Swap
        {
            get
            {
                return m_bSwap;
            }
            set
            {
                m_bSwap = value;
            }
        }
        public double A { get { return m_a; } }
        public double B { get { return m_b; } }
        public void LeastSquareFit(PointF[] points)
        {
            ///-----------------------------------------------
            /// The plane equation for least-square-fit is
            /// y = ax + b (normal)
            /// x = ay + b (swap)
            ///-----------------------------------------------

            int iLen = points.Length;

            if (iLen < 2)
                throw new Exception("Error: QLineFitting.LeastSquareFit needs 2 or more points as input !");

            double sxx = 0;
            double sxy = 0;
            double sx = 0;
            double sy = 0;
            double s1 = 0;
            double x;
            double y;
            for (int i = 0; i < iLen; i++)
            {
                if (m_bSwap)
                {
                    y = points[i].X;
                    x = points[i].Y;
                }
                else
                {
                    x = points[i].X;
                    y = points[i].Y;
                }
                sxx += (x * x);
                sxy += (x * y);
                sx += x;
                sy += y;
                s1 += 1;
            }

            //CvMat MX1 = new CvMat(2, 2, MatType.CV_64FC1);
            //MX1[0, 0] = sxx;
            //MX1[0, 1] = sx;
            //MX1[1, 0] = sx;
            //MX1[1, 1] = s1;
            var MX1 = new Mat(2, 2, MatType.CV_64FC1);
            MX1.At<double>(0, 0) = sxx;
            MX1.At<double>(0, 1) = sx;
            MX1.At<double>(1, 0) = sx;
            MX1.At<double>(1, 1) = s1;
            
            //CvMat MXY = new CvMat(2, 1, MatType.CV_64FC1);
            //MXY[0, 0] = sxy;
            //MXY[1, 0] = sy;
            var MXY = new Mat(2, 1, MatType.CV_64FC1);
            MXY.At<double>(0, 0) = sxy;
            MXY.At<double>(1, 0) = sy;
            
            //CvMat MXi = new CvMat(2, 2, MatType.CV_64FC1);
            //MX1.Inv(MXi, InvertMethod.Normal);
            //CvMat AB = MXi * MXY;
            //m_a = AB[0, 0];
            //m_b = AB[1, 0];
            var mxi = MX1.Inv(DecompTypes.Normal);
            var ab = mxi * MXY;
            var AB = ab.ToMat();
            m_a = AB.At<double>(0, 0);
            m_b = AB.At<double>(1, 0);

            MXY.Dispose();
            MX1.Dispose();
            mxi.Dispose();
            AB.Dispose();
            ab.Dispose();

            _TRACE(points, this);
        }

        public double GetPointLength(PointF ptf)
        {
            double ret = 0;

            if (Swap)
                ret = Math.Abs(ptf.X - A * ptf.Y - B) / Math.Sqrt(Math.Pow(1, 2) + Math.Pow(-A, 2));
            else
                ret = Math.Abs(A * ptf.X - ptf.Y + B) / Math.Sqrt(Math.Pow(A, 2) + Math.Pow(-1, 2));

            return ret;

        }

        public static double GetAngle(QvLineFit2 L1, QvLineFit2 L2, bool usingDegree = true)
        {
            if (L1 == null || L2 == null)
                return 0.0;
            double A1, B1, C1;
            double A2, B2, C2;
            L1._getCoefs(out A1, out B1, out C1);
            L2._getCoefs(out A2, out B2, out C2);
            double d1 = Math.Sqrt(A1 * A1 + B1 * B1);
            double d2 = Math.Sqrt(A2 * A2 + B2 * B2);
            double c = (A1*A2 + B1*B2) / (d1 * d2);
            double a = Math.Acos(Math.Abs(c));
            if (usingDegree)
                return a * 180.0 / Math.PI;
            else
                return a;
        }

        public static PointF GetIntersectPoint(QvLineFit2 L1, QvLineFit2 L2)
        {
            if (L1 == null || L2 == null)
                return PointF.Empty;
            double A1, B1, C1;
            double A2, B2, C2;
            L1._getCoefs(out A1, out B1, out C1);
            L2._getCoefs(out A2, out B2, out C2);
            double det = A1 * B2 - A2 * B1;
            System.Diagnostics.Debug.Assert(Math.Abs(det) > (1e-9));
            double x =  (B1 * C2 - B2 * C1) / det;
            double y = -(A1 * C2 - A2 * C1) / det;
            return new PointF((float)x, (float)y);
        }


        #region TRACE_FUCTIONS
        /// <summary>
        /// 轉換到 A x + B y + C = 0
        /// </summary>
        /// <returns></returns>
        private void _getCoefs(out double A, out double B, out double C)
        {
            ///-----------------------------------------------
            /// The plane equation for least-square-fit is
            /// y = ax + b (normal)
            /// x = ay + b (swap)
            ///-----------------------------------------------
            /// 回歸到 Ax + By + C = 0
            ///-----------------------------------------------
            if (Swap)
            {
                A = 1;
                B = -m_a;
                C = -m_b;
            }
            else
            {
                A = m_a;
                B = -1;
                C = m_b;
            }
        }
        private static void _TRACE(PointF[] pts, QvLineFit2 lineFit)
        {
        }
        #endregion
    }

    public class QvLineFit
    {
        #region PRIVATE_DATA
        private double m_a = 0.0;
        private double m_b = 0.0;
        private bool m_bSwap = false;
        #endregion

        public bool Swap
        {
            get
            {
                return m_bSwap;
            }
            set
            {
                m_bSwap = value;
            }
        }
        public double A { get { return m_a; } }
        public double B { get { return m_b; } }
        public void LeastSquareFit(PointF[] points, bool autoSwap = true)
        {
            if (autoSwap)
            {
                QvLineFit lineBest = this;
                double smallA = double.MaxValue;
                for (int i = 0; i < 2; i++)
                {
                    var line = new QvLineFit();
                    line.Swap = (i == 0);
                    try
                    {
                        line._leastSquareFit(points);
                        if (Math.Abs(line.A) < smallA)
                        {
                            lineBest = line;
                            smallA = Math.Abs(line.A);
                        }
                    }
                    catch
                    {
                    }
                }
                this.Swap = lineBest.Swap;
                this.m_a = lineBest.m_a;
                this.m_b = lineBest.m_b;
                this.m_A = lineBest.m_A;
                this.m_B = lineBest.m_B;
                this.m_C = lineBest.m_C;
            }
            else
            {
                _leastSquareFit(points);
            }
        }
        private void _leastSquareFit(PointF[] points)
        {
            ///-----------------------------------------------
            /// The plane equation for least-square-fit is
            /// y = ax + b (normal)
            /// x = ay + b (swap)
            ///-----------------------------------------------

            int N = points.Length;

            if (N < 2)
                throw new Exception("Error: QLineFitting.LeastSquareFit needs 2 or more points as input !");

            double sxx = 0;
            double sxy = 0;
            double sx = 0;
            double sy = 0;
            double s1 = 0;
            double x;
            double y;
            for (int i = 0; i < N; i++)
            {
                if (m_bSwap)
                {
                    y = points[i].X;
                    x = points[i].Y;
                }
                else
                {
                    x = points[i].X;
                    y = points[i].Y;
                }
                sxx += (x * x);
                sxy += (x * y);
                sx += x;
                sy += y;
                s1 += 1;
            }

            //CvMat MX1 = new CvMat(2, 2, MatType.CV_64FC1);
            //MX1[0, 0] = sxx;
            //MX1[0, 1] = sx;
            //MX1[1, 0] = sx;
            //MX1[1, 1] = s1;
            var MX1 = new Mat(2, 2, MatType.CV_64FC1);
            MX1.At<double>(0, 0) = sxx;
            MX1.At<double>(0, 1) = sx;
            MX1.At<double>(1, 0) = sx;
            MX1.At<double>(1, 1) = s1;

            //CvMat MXY = new CvMat(2, 1, MatType.CV_64FC1);
            //MXY[0, 0] = sxy;
            //MXY[1, 0] = sy;
            var MXY = new Mat(2, 1, MatType.CV_64FC1);
            MXY.At<double>(0, 0) = sxy;
            MXY.At<double>(1, 0) = sy;

            //CvMat MXi = new CvMat(2, 2, MatType.CV_64FC1);
            //MX1.Inv(MXi, InvertMethod.Normal);
            //CvMat AB = MXi * MXY;
            //m_a = AB[0, 0];
            //m_b = AB[1, 0];
            var mxi = MX1.Inv(DecompTypes.Normal);
            var ab = mxi * MXY;
            var AB = ab.ToMat();
            m_a = AB.At<double>(0, 0);
            m_b = AB.At<double>(1, 0);

            double det = MX1.Determinant();
            if (Math.Abs(det) < 1e-15)
                m_a = 1 / det;

            MXY.Dispose();
            MX1.Dispose();
            mxi.Dispose();
            AB.Dispose();
            ab.Dispose();

            _getCoefs(out m_A, out m_B, out m_C);
            _TRACE(points, this);
        }

        public double GetPointLength(PointF ptf)
        {
            double ret = 0;

            if (Swap)
                ret = Math.Abs(ptf.X - A * ptf.Y - B) / Math.Sqrt(Math.Pow(1, 2) + Math.Pow(-A, 2));
            else
                ret = Math.Abs(A * ptf.X - ptf.Y + B) / Math.Sqrt(Math.Pow(A, 2) + Math.Pow(-1, 2));

            return ret;

        }
        public static double GetAngle(QvLineFit L1, QvLineFit L2, bool usingDegree = true)
        {
            if (L1 == null || L2 == null)
                return 0.0;
            double A1, B1, C1;
            double A2, B2, C2;
            ////L1._getCoefs(out A1, out B1, out C1);
            ////L2._getCoefs(out A2, out B2, out C2);
            A1 = L1.m_A;
            B1 = L1.m_B;
            A2 = L2.m_A;
            B2 = L2.m_B;
            double d1 = Math.Sqrt(A1 * A1 + B1 * B1);
            double d2 = Math.Sqrt(A2 * A2 + B2 * B2);
            double c = (A1 * A2 + B1 * B2) / (d1 * d2);
            double a = Math.Acos(Math.Abs(c));
            if (usingDegree)
                return a * 180.0 / Math.PI;
            else
                return a;
        }
        public static PointF GetIntersectPoint(QvLineFit L1, QvLineFit L2)
        {
            if (L1 == null || L2 == null)
                return PointF.Empty;
            double A1, B1, C1;
            double A2, B2, C2;
            //L1._getCoefs(out A1, out B1, out C1);
            //L2._getCoefs(out A2, out B2, out C2);
            A1 = L1.m_A;
            B1 = L1.m_B;
            C1 = L1.m_C;
            A2 = L2.m_A;
            B2 = L2.m_B;
            C2 = L2.m_C;
            double det = A1 * B2 - A2 * B1;
            System.Diagnostics.Debug.Assert(Math.Abs(det) > (1e-9));
            double x = (B1 * C2 - B2 * C1) / det;
            double y = -(A1 * C2 - A2 * C1) / det;
            return new PointF((float)x, (float)y);
        }

        public QvLineFit GetPerpendicularLine(PointF ptf)
        {
            var lineP = new QvLineFit();
            // A x + B y + C = 0
            double A2 = m_B;
            double B2 = -m_A;
            double C2 = -(A2 * ptf.X + B2 * ptf.Y);
            lineP.m_A = A2;
            lineP.m_B = B2;
            lineP.m_C = C2;
            return lineP;
        }
        public PointF GetPerpendicularPoint(PointF ptf)
        {
            var lineP = GetPerpendicularLine(ptf);
            var pt = GetIntersectPoint(this, lineP);
            return pt;
        }
        public double GetY(double x)
        {
            // Ax + By + C = 0
            return -(m_A * x + m_C) / m_B;
        }
        public double GetX(double y)
        {
            // Ax + By + C = 0
            return -(m_B * y + m_C) / m_A;
        }

        #region TRACE_FUCTIONS
        private double m_A = 0.0;
        private double m_B = 0.0;
        private double m_C = 0.0;
        /// <summary>
        /// 轉換到 A x + B y + C = 0
        /// </summary>
        /// <returns></returns>
        private void _getCoefs(out double A, out double B, out double C)
        {
            ///-----------------------------------------------
            /// The plane equation for least-square-fit is
            /// y = ax + b (normal)
            /// x = ay + b (swap)
            ///-----------------------------------------------
            /// 回歸到 Ax + By + C = 0
            ///-----------------------------------------------
            if (Swap)
            {
                A = 1;
                B = -m_a;
                C = -m_b;
            }
            else
            {
                A = m_a;
                B = -1;
                C = m_b;
            }
        }
        private static void _TRACE(PointF[] pts, QvLineFit lineFit)
        {
        }
        #endregion
    }
    
}
