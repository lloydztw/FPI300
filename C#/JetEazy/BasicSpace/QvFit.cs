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

using System;
using System.Drawing;
using OpenCvSharp;

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

                using (IplImage img = new IplImage(iWidth, iHeight, BitDepth.U8, 3))
                {
                    img.Zero();


                    for (int i = 0; i < pts.Length; i++)
                    {
                        CvPoint pt = new CvPoint()
                        {
                            X = (int)((pts[i].X - xMin) * dZoom + x0),
                            Y = (int)((pts[i].Y - yMin) * dZoom + y0)
                        };
                        img.Circle(pt, 3, new CvColor(0, 255, 0), Cv.FILLED);
                    }

                    CvPoint ptvCenter = new CvPoint()
                    {
                        X = (int)((ptCenter.X - xMin) * dZoom + x0),
                        Y = (int)((ptCenter.Y - yMin) * dZoom + y0)
                    };
                    int iRadius = (int)(radius * dZoom);

                    img.Circle(ptvCenter, iRadius, CvColor.White);

                    string strTitle = string.Format("QCircleFit: Loops={0}, Err={1}", iTryLoops, error);
                    using (CvWindow w = new CvWindow(strTitle, WindowMode.AutoSize, img))
                    {
                        CvWindow.WaitKey(0);
                    }
                }
            }
        #endregion

        public static void SelfTest()
        {
            CvRNG rng = new CvRNG(DateTime.Now);
            PointF[] pts = new PointF[100];
            double R = 2000;
            double dPercent = 0.05;

            for (int i = 0; i < pts.Length; i++)
            {
                int u = (int)rng.RandInt() % 360;
                double a = (double)u / 180.0 * Math.PI;

                u = (int)rng.RandInt() % 100 - 50;
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

            CvMat MX1 = new CvMat(2, 2, MatrixType.F64C1);
            MX1[0, 0] = sxx; MX1[0, 1] = sx;
            MX1[1, 0] = sx; MX1[1, 1] = s1;
            CvMat MXY = new CvMat(2, 1, MatrixType.F64C1);
            MXY[0, 0] = sxy;
            MXY[1, 0] = sy;
            CvMat MXi = new CvMat(2, 2, MatrixType.F64C1);
            MX1.Inv(MXi, InvertMethod.Normal);
            CvMat AB = MXi * MXY;
            m_a = AB[0, 0];
            m_b = AB[1, 0];

            MX1.Dispose();
            MXi.Dispose();
            MXY.Dispose();
            AB.Dispose();

            _TRACE(points, this);
        }

        #region TRACE_FUCTIONS
        private static void _TRACE(PointF[] pts, QvLineFit lineFit)
        {
        }
        #endregion
    }
}
