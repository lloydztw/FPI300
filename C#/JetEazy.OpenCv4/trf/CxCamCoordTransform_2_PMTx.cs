//#define OPT_USING_BI_LINEAR

using JetEazy.QMath;
using OpenCvSharp;
using System;
using System.Drawing;


namespace JetEazy.Aoi.TransformD
{
    public partial class CxCamCoordTransform : IxCoordTransform
    {
        MatType MAT_TYPE = MatType.CV_32FC1;

#if(OPT_USING_BI_LINEAR)
        private double SCALE_OF_WORLD = 1f;
        private double SCALE_OF_VIEW = 1f;
#else
        private double SCALE_OF_WORLD
        {
            get
            {
                return 1;
            }
            set
            {
            }
        }
        private double SCALE_OF_VIEW
        {
            get
            {
                return 1;
            }
            set
            {
            }
        }
#endif

        #region PRIVATE_TRACE_DATA
        private static int m_iCount = 0;
        private string m_name;
        private int m_id;
        #endregion

        #region PRIVATE_DATA
        private int m_iRows = 2;
        private int m_iCols = 2;
        private QVector[,] m_pointsInWorld;
        private QVector[,] m_pointsInView;
        private Mat[,] m_matriceW2V;      // World to View
        private Mat[,] m_matriceV2W;      // View to World
        #endregion

        public CxCamCoordTransform()
        {
            m_id = m_iCount++;
            _initArrays();
        }
        public CxCamCoordTransform(CxCamCoordTransform src)
        {
#if(false)
            if (src == null)
            {
                _initArrays();
                return;
            }

            m_iRows = src.m_iRows;
            m_iCols = src.m_iCols;

            _initArrays();

            for (int i = 0; i < m_iRows; i++)
            {
                for (int j = 0; j < m_iCols; j++)
                {
                    m_pointsInWorld[i, j] = src.m_pointsInWorld[i, j];
                    m_pointsInView[i, j] = src.m_pointsInView[i, j];
                }
            }

            for (int i = 0; i < m_iRows - 1; i++)
            {
                for (int j = 0; j < m_iCols - 1; j++)
                {
                    if (src.m_matriceW2V[i, j] != null)
                    {
                        JetEazy.QUtilities.QUtility.SafeDisposeObject(m_matriceW2V[i, j]);
                        m_matriceW2V[i, j] = src.m_matriceW2V[i, j].Clone();
                    }
                    if (src.m_matriceV2W[i, j] != null)
                    {
                        JetEazy.QUtilities.QUtility.SafeDisposeObject(m_matriceV2W[i, j]);
                        m_matriceV2W[i, j] = src.m_matriceV2W[i, j].Clone();
                    }
                }
            }
#endif
            m_id = m_iCount++;
            CopyFrom(src);
        }
        public void CopyFrom(CxCamCoordTransform src)
        {
            if (src == null)
            {
                _initArrays();
                return;
            }

            SCALE_OF_WORLD = src.SCALE_OF_WORLD;
            SCALE_OF_VIEW = src.SCALE_OF_VIEW;

            m_iRows = src.m_iRows;
            m_iCols = src.m_iCols;

            _initArrays();

            for (int i = 0; i < m_iRows; i++)
            {
                for (int j = 0; j < m_iCols; j++)
                {
                    m_pointsInWorld[i, j] = src.m_pointsInWorld[i, j];
                    m_pointsInView[i, j] = src.m_pointsInView[i, j];
                }
            }

            for (int i = 0; i < m_iRows - 1; i++)
            {
                for (int j = 0; j < m_iCols - 1; j++)
                {
                    if (src.m_matriceW2V[i, j] != null)
                    {
                        JetEazy.QUtilities.QUtility.SafeDisposeObject(m_matriceW2V[i, j]);
                        m_matriceW2V[i, j] = src.m_matriceW2V[i, j].Clone();
                    }
                    if (src.m_matriceV2W[i, j] != null)
                    {
                        JetEazy.QUtilities.QUtility.SafeDisposeObject(m_matriceV2W[i, j]);
                        m_matriceV2W[i, j] = src.m_matriceV2W[i, j].Clone();
                    }
                }
            }
        }
        public IxCoordTransform Clone()
        {
            CxCamCoordTransform obj = new CxCamCoordTransform(this);
            return obj;
        }
        object ICloneable.Clone()
        {
            return new CxCamCoordTransform(this);
        }
        public virtual void Dispose()
        {
            _disposeMatrice();
        }
        public override string ToString()
        {
            if (m_name != null)
                return m_name;
            string str = base.ToString();
            return str;
        }

        #region ORIGINAL_INTERFACE_I
        public void GetCalibrationPoints(out PointF[,] pointsInView, out PointF[,] pointsInWorld)
        {
            pointsInWorld = new PointF[m_iRows, m_iCols];
            pointsInView = new PointF[m_iRows, m_iCols];
            for (int i = 0; i < m_iRows; i++)
            {
                for (int j = 0; j < m_iCols; j++)
                {
                    var w = this.m_pointsInWorld[i, j] * SCALE_OF_WORLD;
                    var v = this.m_pointsInView[i, j] * SCALE_OF_VIEW;
                    pointsInWorld[i, j] = new PointF((float)w.x, (float)w.y);
                    pointsInView[i, j] = new PointF((float)v.x, (float)v.y);
                }
            }
        }
        public void SetCalibrationPoints(PointF[,] pointsInView, PointF[,] pointsInWorld)
        {
            int iRows = Math.Min(pointsInView.GetUpperBound(0), pointsInWorld.GetUpperBound(0)) + 1;
            int iCols = Math.Min(pointsInView.GetUpperBound(1), pointsInWorld.GetUpperBound(1)) + 1;
            var ww = new QVector[iRows, iCols];
            var vv = new QVector[iRows, iCols];
            for (int i = 0; i < iRows; i++)
            {
                for (int j = 0; j < iCols; j++)
                {
                    ww[i, j] = new QVector2(pointsInWorld[i, j].X, pointsInWorld[i, j].Y);
                    vv[i, j] = new QVector2(pointsInView[i, j].X, pointsInView[i, j].Y);
                }
            }
            SetCalibrationPoints(vv, ww);
        }
        public void SetCalibrationPoints(int iRow, int iCol, PointF pointInView, PointF pointInWorld)
        {
            if (iRow < 0 || iRow >= m_iRows || iCol < 0 || iCol >= m_iCols)
                return;
            m_pointsInView[iRow, iCol] = new QVector2(pointInView.X / SCALE_OF_VIEW, pointInView.Y / SCALE_OF_VIEW);
            m_pointsInWorld[iRow, iCol] = new QVector2(pointInWorld.X / SCALE_OF_WORLD, pointInWorld.Y / SCALE_OF_WORLD);
        }
        public PointF[,] PointsInWorld
        {
            get
            {
                var pts = new PointF[m_iRows, m_iCols];
                for (int r = 0; r < m_iRows; r++)
                {
                    for (int c = 0; c < m_iCols; c++)
                    {
                        var w = m_pointsInWorld[r, c] * SCALE_OF_WORLD;
                        pts[r, c] = ToPointF(w);
                    }
                }
                return pts;
            }
        }
        public PointF[,] PointsInView
        {
            get
            {
                var pts = new PointF[m_iRows, m_iCols];
                for (int r = 0; r < m_iRows; r++)
                {
                    for (int c = 0; c < m_iCols; c++)
                    {
                        var v = m_pointsInView[r, c] * SCALE_OF_VIEW;
                        pts[r, c] = ToPointF(v);
                    }
                }
                return pts;
            }
        }
        #endregion

        public void GetCalibrationPoints(out QVector[,] pointsInView, out QVector[,] pointsInWorld)
        {
            pointsInWorld = new QVector[m_iRows, m_iCols];
            pointsInView = new QVector[m_iRows, m_iCols];
            try
            {
                for (int i = 0; i < m_iRows; i++)
                {
                    for (int j = 0; j < m_iCols; j++)
                    {
                        pointsInWorld[i, j] = m_pointsInWorld[i, j] * SCALE_OF_WORLD;
                        pointsInView[i, j] = m_pointsInView[i, j] * SCALE_OF_VIEW;
                    }
                }
                _roundToHalfPixels(m_pointsInView);
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                _LOG(ex);
            }
        }
        public void SetCalibrationPoints(QVector[,] pointsInView, QVector[,] pointsInWorld)
        {
            m_iRows = Math.Min(pointsInView.GetUpperBound(0), pointsInWorld.GetUpperBound(0)) + 1;
            m_iCols = Math.Min(pointsInView.GetUpperBound(1), pointsInWorld.GetUpperBound(1)) + 1;

            _initArrays();
            _roundToHalfPixels(pointsInView);

            SCALE_OF_VIEW = _autoScale(pointsInView);
            SCALE_OF_WORLD = _autoScale(pointsInWorld);

            for (int i = 0; i < m_iRows; i++)
            {
                for (int j = 0; j < m_iCols; j++)
                {
                    m_pointsInWorld[i, j] = pointsInWorld[i, j] / SCALE_OF_WORLD;
                    m_pointsInView[i, j] = pointsInView[i, j] / SCALE_OF_VIEW;
                }
            }
        }
        public bool BuildTransformFormula()
        {
            //	vx0 = |m11 m12 m13 m14|  [1 wx0 wy0 wxy0]t
            //	vx1 = |m11 m12 m13 m14|  [1 wx1 wy1 wxy1]t
            //	vx2 = |m11 m12 m13 m14|  [1 wx2 wy2 wxy2]t
            //	vx3 = |m11 m12 m13 m14|  [1 wx3 wy3 wxy3]t

            //	vy0 = |m21 m22 m23 m24|  [1 x0 y0 xy0]t
            //	vy1 = |m21 m22 m23 m24|  [1 x1 y1 xy1]t
            //	vy2 = |m21 m22 m23 m24|  [1 x2 y2 xy2]t
            //	vy3 = |m21 m22 m23 m24|  [1 x3 y3 xy3]t

            //	| 1 wx0 wy0 wxy0 | m11 |   | vx0 |
            //	| 1 wx1 wy1 wxy1 | m12 |   | vx1 |
            //	| 1 wx2 wy2 wxy2 | m13 | = | vx2 | 
            //	| 1 wx3 wy3 wxy3 | m14 |   | vx3 |

            //	| 1 wx0 wy0 wxy0 | m21 |   | vy0 |
            //	| 1 wx1 wy1 wxy1 | m22 |   | vy1 |
            //	| 1 wx2 wy2 wxy2 | m23 | = | vy2 | 
            //	| 1 wx3 wy3 wxy3 | m24 |   | vy3 |

            try
            {
                for (int i = 0; i < m_iRows - 1; i++)
                {
                    for (int j = 0; j < m_iCols - 1; j++)
                    {
                        var ptsV = new QVector[] {
                            m_pointsInView[i, j],
                            m_pointsInView[i, j+1],
                            m_pointsInView[i+1, j+1],
                            m_pointsInView[i+1, j]
                        };

                        var ptsW = new QVector[] {
                            m_pointsInWorld[i, j],
                            m_pointsInWorld[i, j+1],
                            m_pointsInWorld[i+1, j+1],
                            m_pointsInWorld[i+1, j]
                        };

                        JetEazy.QUtilities.QUtility.SafeDisposeObject(m_matriceW2V[i, j]);
                        JetEazy.QUtilities.QUtility.SafeDisposeObject(m_matriceV2W[i, j]);

                        _buildMatrix(out m_matriceW2V[i, j], ptsV, ptsW);   // World To View
                        _buildMatrix(out m_matriceV2W[i, j], ptsW, ptsV);   // View To World
                    }
                }

                _verifyNodePoints();
                return true;
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                _LOG(ex);
                throw ex;
            }

            return false;
        }
        public void TransformWorldToView(QVector pointWorld, out QVector pointView)
        {
            int iRow;
            int iCol;

            pointWorld = pointWorld / SCALE_OF_WORLD;
            _getWorldZone(out iRow, out iCol, pointWorld);

            Mat mat = m_matriceW2V[iRow, iCol];

#if(OPT_USING_BI_LINEAR)
            double wx = pointWorld.x;
            double wy = pointWorld.y;
            double wxy = wx * wy;
            double x = mat[0, 0] + mat[0, 1] * wx + mat[0, 2] * wy + mat[0, 3] * wxy;
            double y = mat[1, 0] + mat[1, 1] * wx + mat[1, 2] * wy + mat[1, 3] * wxy;
            pointView = new QVector(2);
            pointView.x = x * SCALE_OF_VIEW;
            pointView.y = y * SCALE_OF_VIEW;
#else

            //////using (Mat w = new Mat(3, 1, MAT_TYPE))
            //////using (Mat v = new Mat(3, 1, MAT_TYPE))
            //////{
            //////}
            //////w[0, 0] = pointWorld.x;
            //////w[1, 0] = pointWorld.y;
            //////w[2, 0] = 1.0;
            //////Mat v = mat * w;
            //////double x = v[0, 0];
            //////double y = v[1, 0];
            //////double t = v[2, 0];
            //////x /= t;
            //////y /= t;
            //////w.Dispose();
            //////v.Dispose();

            double x;
            double y;
            //using (Mat src = new Mat(1, 1, MatrixType.F32C2))
            //using (Mat dst = new Mat(1, 1, MatrixType.F32C2))
            using (var src = new Mat(1, 1, MatType.CV_32FC2))
            using (var dst = new Mat(1, 1, MatType.CV_32FC2))
            {
                //src.Set2D(0, 0, new CvScalar(pointWorld.x, pointWorld.y));
                src.At<Vec2f>(0, 0) = new Vec2f((float)pointWorld.x, (float)pointWorld.y);
                Cv2.PerspectiveTransform(src, dst, mat);
                //var s = dst.Get2D(0, 0);
                //x = s.Val0;
                //y = s.Val1;
                var v = dst.At<Vec2f>(0, 0);
                x = v[0];
                y = v[1];
            }

            pointView = new QVector2(x, y);
            //pointView.x = x * SCALE_OF_VIEW;
            //pointView.y = y * SCALE_OF_VIEW;
#endif



        }
        public void TransformViewToWorld(QVector pointView, out QVector pointWorld)
        {
            int iRow;
            int iCol;

            pointView = pointView / SCALE_OF_VIEW;
            _getViewZone(out iRow, out iCol, pointView);

            Mat mat = m_matriceV2W[iRow, iCol];
#if(OPT_USING_BI_LINEAR)
            double vx = pointView.x;
            double vy = pointView.y;
            double vxy = vx * vy;
            double x = mat[0, 0] + mat[0, 1] * vx + mat[0, 2] * vy + mat[0, 3] * vxy;
            double y = mat[1, 0] + mat[1, 1] * vx + mat[1, 2] * vy + mat[1, 3] * vxy;
            pointWorld = new QVector(2);
            pointWorld.x = x * SCALE_OF_WORLD;
            pointWorld.y = y * SCALE_OF_WORLD;
#else
            //////Mat v = new Mat(3, 1, MAT_TYPE);
            //////v[0, 0] = pointView.x;
            //////v[1, 0] = pointView.y;
            //////v[2, 0] = 1.0;
            //////Mat w = mat * v;
            //////double x = w[0, 0];
            //////double y = w[1, 0];
            //////double t = w[2, 0];
            //////x /= t;
            //////y /= t;
            //////w.Dispose();
            //////v.Dispose();

            double x;
            double y;
            using (var src = new Mat(1, 1, MatType.CV_32FC2))
            using (var dst = new Mat(1, 1, MatType.CV_32FC2))
            {
                //src.Set2D(0, 0, new CvScalar(pointView.x, pointView.y));
                src.At<Vec2f>(0, 0) = new Vec2f((float)pointView.x, (float)pointView.y);
                Cv2.PerspectiveTransform(src, dst, mat);
                //CvScalar s = dst.Get2D(0, 0);
                //x = s.Val0;
                //y = s.Val1;
                var v = dst.At<Vec2f>(0, 0);
                x = v[0];
                y = v[1];
            }
            pointWorld = new QVector2(x, y);
#endif
        }

        public QVector ToLocal(QVector w, QVector vMotor = null)
        {
            QVector v;
            TransformWorldToView(w, out v);
            _roundToHalfPixels(v);
            return v;
        }
        public QVector ToWorld(QVector v, QVector vMotor = null)
        {
            QVector w;
            _roundToHalfPixels(v);
            TransformViewToWorld(v, out w);
            return w;
        }
        public PointF ToLocal(PointF ptWorld, QVector vMotor = null)
        {
            QVector v = ToVector(ptWorld);
            return ToPointF(ToLocal(v, vMotor));
        }
        public PointF ToWorld(PointF ptLocal, QVector vMotor = null)
        {
            QVector v = ToVector(ptLocal);
            return ToPointF(ToWorld(v, vMotor));
        }
        public RectangleF ToLocal(RectangleF ptWorld, QVector vMotor = null)
        {
            PointF pt1 = new PointF(ptWorld.Left, ptWorld.Top);
            PointF pt2 = new PointF(ptWorld.Right, ptWorld.Bottom);
            pt1 = ToLocal(pt1, vMotor);
            pt2 = ToLocal(pt2, vMotor);
            float w = Math.Abs(pt2.X - pt1.X);
            float h = Math.Abs(pt2.Y - pt1.Y);
            float x = (pt2.X + pt1.X - w) / 2f;
            float y = (pt2.Y + pt1.Y - h) / 2f;
            return new RectangleF(x, y, w, h);
        }
        public RectangleF ToWorld(RectangleF ptLocal, QVector vMotor = null)
        {
            PointF pt1 = new PointF(ptLocal.Left, ptLocal.Top);
            PointF pt2 = new PointF(ptLocal.Right, ptLocal.Bottom);
            pt1 = ToWorld(pt1, vMotor);
            pt2 = ToWorld(pt2, vMotor);
            float w = Math.Abs(pt2.X - pt1.X);
            float h = Math.Abs(pt2.Y - pt1.Y);
            float x = (pt2.X + pt1.X - w) / 2f;
            float y = (pt2.Y + pt1.Y - h) / 2f;
            return new RectangleF(x, y, w, h);
        }

        public void Load(string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName);
            if (string.Compare(ext, ".bin", true) == 0 || string.Compare(ext, ".jdb", true) == 0)
            {
                try
                {
                    LoadBin(fileName);
                }
                catch (Exception ex)
                {
                    //JetEazy.LoggerClass.Instance.WriteException(ex);
                    _LOG(ex);
                    System.Diagnostics.Trace.WriteLine("CxCamCoordTransform.LoadBin : Exception = " + ex.Message);
                }
            }
            else
            {
                //> LoadBin(fileName + ".bin");
                LoadIni(fileName);
            }
        }
        public void Save(string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName);
            if (string.Compare(ext, ".bin", true) == 0 || string.Compare(ext, ".jdb", true) == 0)
            {
                SaveBin(fileName);
            }
            else
            {
                SaveIni(fileName);
                //> SaveBin(fileName + ".bin");
                //> LoadBin(fileName + ".bin");
            }
        }

        public void LoadIni(string strIniFileName)
        {
            m_name = System.IO.Path.GetFileName(strIniFileName);

            string str = null;

#if(OPT_USING_BI_LINEAR)
            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, "Scale", "World");
            if (!string.IsNullOrEmpty(str)) double.TryParse(str, out SCALE_OF_WORLD);
            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, "Scale", "View");
            if (!string.IsNullOrEmpty(str)) double.TryParse(str, out SCALE_OF_VIEW);
#endif

            JetEazy.Win32.Win32Ini.Load(ref m_iRows, strIniFileName, "Dimensions", "Rows");
            JetEazy.Win32.Win32Ini.Load(ref m_iCols, strIniFileName, "Dimensions", "Cols");

            bool bEmptyMatrix = false;
            if (m_iRows < 2) { m_iRows = 2; bEmptyMatrix = true; }
            if (m_iCols < 2) { m_iCols = 2; bEmptyMatrix = true; }

            _initArrays();

            if (!bEmptyMatrix)
            {
                _load(m_matriceW2V, strIniFileName, "MATRIX");
                _load(m_matriceV2W, strIniFileName, "MATRIX_V2W");
            }

            _load(m_pointsInView, strIniFileName, "ViewPoints");
            _load(m_pointsInWorld, strIniFileName, "WorldPoints");
        }
        public void SaveIni(string strIniFileName)
        {
            JetEazy.Win32.Win32Ini.Save(SCALE_OF_WORLD, strIniFileName, "Scale", "World");
            JetEazy.Win32.Win32Ini.Save(SCALE_OF_VIEW, strIniFileName, "Scale", "View");
            JetEazy.Win32.Win32Ini.Save(m_iRows, strIniFileName, "Dimensions", "Rows");
            JetEazy.Win32.Win32Ini.Save(m_iCols, strIniFileName, "Dimensions", "Cols");

            _save(m_matriceW2V, strIniFileName, "MATRIX");
            _save(m_matriceV2W, strIniFileName, "MATRIX_V2W");
            _save(m_pointsInView, strIniFileName, "ViewPoints");
            _save(m_pointsInWorld, strIniFileName, "WorldPoints");
        }

        public static QVector ToVector(PointF pt, double scale)
        {
            QVector v = new QVector(2);
            v[0] = pt.X * scale;
            v[1] = pt.Y * scale;
            return v;
        }
        public static QVector ToVector(PointF pt)
        {
            QVector v = new QVector(2);
            v[0] = pt.X;
            v[1] = pt.Y;
            return v;
        }
        public static PointF ToPointF(QVector v, double scale)
        {
            PointF pt = new PointF((float)(v[0] * scale), (float)(v[1] * scale));
            return pt;
        }
        public static PointF ToPointF(QVector v)
        {
            PointF pt = new PointF((float)v[0], (float)v[1]);
            return pt;
        }

        #region PRIVATE_FUNCTIONS

        private void _initArrays()
        {
            if (m_iRows < 2 || m_iCols < 2)
                throw new Exception("Rows or Cols is too small.");

            m_pointsInView = new QVector[m_iRows, m_iCols];
            m_pointsInWorld = new QVector[m_iRows, m_iCols];

            for (int r = 0; r < m_iRows; r++)
            {
                for (int c = 0; c < m_iCols; c++)
                {
                    m_pointsInView[r, c] = new QVector2(r, c) * 0.01;
                    m_pointsInWorld[r, c] = new QVector2(r, c) * 0.01;
                }
            }

            _disposeMatrice();
            _initMatrice();
        }

#if (OPT_USING_BI_LINEAR)
            private static float[] m_arrUnitMatData32F = new float[] {
                0f, 1f, 0f, 0f,
                0f, 0f, 1f, 0f
            };
            private static double[] m_arrUnitMatData64F = new double[] {
                0, 1, 0, 0,
                0, 0, 1, 0
            };
#endif

        private void _initMatrice()
        {
            System.Diagnostics.Trace.Assert(MAT_TYPE == MatType.CV_32FC1);

            m_matriceW2V = new Mat[m_iRows - 1, m_iCols - 1];
            m_matriceV2W = new Mat[m_iRows - 1, m_iCols - 1];

            for (int i = 0; i < m_iRows - 1; i++)
            {
                for (int j = 0; j < m_iCols - 1; j++)
                {
#if (OPT_USING_BI_LINEAR)
                        if (m_matriceW2V[i, j] == null)
                            m_matriceW2V[i, j] = new Mat(2, 4, MAT_TYPE, m_arrUnitMatData32F, true);

                        if (m_matriceV2W[i, j] == null)
                            m_matriceV2W[i, j] = new Mat(2, 4, MAT_TYPE, m_arrUnitMatData32F, true);
#else
                    if (m_matriceW2V[i, j] == null)
                        m_matriceW2V[i, j] = Mat.Eye(3, 3, MAT_TYPE);

                    if (m_matriceV2W[i, j] == null)
                        m_matriceV2W[i, j] = Mat.Eye(3, 3, MAT_TYPE);
#endif

                    //> _unifyMatrix(m_matriceW2V[i, j]);
                    //> _unifyMatrix(m_matriceV2W[i, j]);
                }
            }
        }
        private void _unifyMatrix(Mat mat)
        {
            //double x = mat[0, 0] * 1 + mat[0, 1] * wx + mat[0, 2] * wy + mat[0, 3] * wxy;
            //double y = mat[1, 0] * 1 + mat[1, 1] * wx + mat[1, 2] * wy + mat[1, 3] * wxy;

            //>> mat[0, 0] = 0; mat[0, 1] = 1; mat[0, 2] = 0; mat[0, 3] = 0;
            //>> mat[1, 0] = 0; mat[1, 1] = 0; mat[1, 2] = 1; mat[1, 3] = 0;

            mat.At<float>(0, 0) = 0;
            mat.At<float>(0, 1) = 1;
            mat.At<float>(0, 2) = 0;
            mat.At<float>(0, 3) = 0;

            mat.At<float>(1, 0) = 0;
            mat.At<float>(1, 1) = 0;
            mat.At<float>(1, 2) = 1;
            mat.At<float>(1, 3) = 0;
        }
        private void _assertUnifyMatrix(Mat mat)
        {
            System.Diagnostics.Trace.Assert(
                //double x = mat[0, 0] * 1 + mat[0, 1] * wx + mat[0, 2] * wy + mat[0, 3] * wxy;
                //double y = mat[1, 0] * 1 + mat[1, 1] * wx + mat[1, 2] * wy + mat[1, 3] * wxy;
                //mat[0, 0] == 0 && mat[0, 1] == 1 && mat[0, 2] == 0 && mat[0, 3] == 0 &&
                //mat[1, 0] == 0 && mat[1, 1] == 0 && mat[1, 2] == 1 && mat[1, 3] == 0
                mat.At<float>(0, 0) == 0 &&
                mat.At<float>(0, 1) == 1 &&
                mat.At<float>(0, 2) == 0 &&
                mat.At<float>(0, 3) == 0 &&
                mat.At<float>(1, 0) == 0 &&
                mat.At<float>(1, 1) == 0 &&
                mat.At<float>(1, 2) == 1 &&
                mat.At<float>(1, 3) == 0
            );
        }
        private void _disposeMatrice()
        {
            _disposeMatrice(m_matriceW2V);
            m_matriceW2V = null;
            _disposeMatrice(m_matriceV2W);
            m_matriceV2W = null;
        }
        private void _disposeMatrice(Mat[,] matrice)
        {
            if (matrice != null)
            {
                int iRows = matrice.GetUpperBound(0) + 1;
                int iCols = matrice.GetUpperBound(1) + 1;
                for (int i = 0; i < iRows; i++)
                {
                    for (int j = 0; j < iCols; j++)
                    {
                        JetEazy.QUtilities.QUtility.SafeDisposeObject(matrice[i, j]);
                        matrice[i, j] = null;
                    }
                }
            }
        }
        private void _buildMatrix(out Mat matT, QVector[] ptsV, QVector[] ptsW)
        {
#if (OPT_USING_BI_LINEAR)
                //	vx0 = |m11 m12 m13 m14|  w[1 x0 y0 xy0]t
                //	vx1 = |m11 m12 m13 m14|  w[1 x1 y1 xy1]t
                //	vx2 = |m11 m12 m13 m14|  w[1 x2 y2 xy2]t
                //	vx3 = |m11 m12 m13 m14|  w[1 x3 y3 xy3]t

                //	vy0 = |m21 m22 m23 m24|  w[1 x0 y0 xy0]t
                //	vy1 = |m21 m22 m23 m24|  w[1 x1 y1 xy1]t
                //	vy2 = |m21 m22 m23 m24|  w[1 x2 y2 xy2]t
                //	vy3 = |m21 m22 m23 m24|  w[1 x3 y3 xy3]t

                //	w| 1 x0 y0 xy0 | m11 |   | vx0 |
                //	w| 1 x1 y1 xy1 | m12 |   | vx1 |
                //	w| 1 x2 y2 xy2 | m13 | = | vx2 | 
                //	w| 1 x3 y3 xy3 | m14 |   | vx3 |

                //	w| 1 x0 y0 xy0 | m21 |   | vy0 |
                //	w| 1 x1 y1 xy1 | m22 |   | vy1 |
                //	w| 1 x2 y2 xy2 | m23 | = | vy2 | 
                //	w| 1 x3 y3 xy3 | m24 |   | vy3 |
                Mat mat1XY = new Mat(4, 4, MAT_TYPE);
                Mat matVX = new Mat(4, 1, MAT_TYPE);
                Mat matVY = new Mat(4, 1, MAT_TYPE);

                for (int i = 0; i < 4; i++)
                {
                    double vx = ptsV[i].x;
                    double vy = ptsV[i].y;
                    double wx = ptsW[i].x;
                    double wy = ptsW[i].y;

                    mat1XY[i, 0] = 1;
                    mat1XY[i, 1] = wx;
                    mat1XY[i, 2] = wy;
                    mat1XY[i, 3] = wx * wy;

                    matVX[i] = vx;
                    matVY[i] = vy;
                }

                Mat matInv = new Mat(4, 4, MAT_TYPE);


                mat1XY.Invert(matInv, InvertMethod.Normal);

                Mat mat1R = matInv * matVX;
                Mat mat2R = matInv * matVY;

                matT = new Mat(2, 4, MAT_TYPE);
                for (int j = 0; j < 4; j++)
                {
                    matT[0, j] = mat1R[j];
                    matT[1, j] = mat2R[j];
                }

                JetEazy.QUtilities.QUtility.SafeDisposeObject(mat1XY);
                JetEazy.QUtilities.QUtility.SafeDisposeObject(matVX);
                JetEazy.QUtilities.QUtility.SafeDisposeObject(matVY);
                JetEazy.QUtilities.QUtility.SafeDisposeObject(matInv);
                JetEazy.QUtilities.QUtility.SafeDisposeObject(mat1R);
                JetEazy.QUtilities.QUtility.SafeDisposeObject(mat2R);
#else
            int len = Math.Min(ptsV.Length, ptsW.Length);
            Point2f[] src = new Point2f[len];
            Point2f[] dst = new Point2f[len];
            for (int i = 0; i < len; i++)
            {
                src[i] = new Point2f((float)ptsW[i].x, (float)ptsW[i].y);
                dst[i] = new Point2f((float)ptsV[i].x, (float)ptsV[i].y);
            }
            matT = Cv2.GetPerspectiveTransform(src, dst);

            ////var xs = src[0].X;
            ////var ys = src[0].Y;
            ////var xd = dst[0].X;
            ////var yd = dst[0].Y;
            ////var vs = new Mat(3, 1, MAT_TYPE);
            ////vs[0, 0] = xs;
            ////vs[1, 0] = ys;
            ////vs[2, 0] = 1;
            ////var vd = matT * vs;
            ////var x = vd[0, 0];
            ////var y = vd[1, 0];
            ////var t = vd[2, 0];
            ////x /= t;
            ////y /= t;
            ////var deltaX = Math.Abs(x - xd);
            ////var deltaY = Math.Abs(y - yd);
            ////System.Diagnostics.Trace.WriteLine(string.Format("deltaXY=({0:0.00}, {1:0.00})", deltaX, deltaY));
#endif
        }
        private void _getWorldZone(out int iRow, out int iCol, QVector pointWorld)
        {
            _getZone(out iRow, out iCol, pointWorld, m_pointsInWorld);
        }
        private void _getViewZone(out int iRow, out int iCol, QVector pointView)
        {
            var gridPoints = m_pointsInView;

            _getZone(out iRow, out iCol, pointView, gridPoints);

            int[] delta = new int[] { 0, -1, 1 };
            int rowE1 = m_iRows - 1;
            int colE1 = m_iCols - 1;

            foreach (int dR in delta)
            {
                int r = iRow + dR;
                if (r < 0 || r >= rowE1)
                    continue;

                foreach (int dC in delta)
                {
                    int c = iCol + dC;
                    if (c < 0 || c >= colE1)
                        continue;

                    var nodes = _getGridNodes(gridPoints, r, c);
                    if (_inZone(pointView, nodes))
                    {
                        iRow = r;
                        iCol = c;
                        return;
                    }
                }
            }
        }
        private void _getZone(out int rowID, out int colID, QVector ptT, QVector[,] gridPoints)
        {
#if (true)
            int rowE = m_iRows;
            int colE = m_iCols;

            rowID = 0;
            colID = 0;

            #region SEARCH_ROW
            for (int r = 0; r < rowE - 1; r++)
            {
                rowID = r;
                for (int c = 0; c < colE; c++)
                {
                    if (ptT.y < gridPoints[r + 1, c].y)
                    {
                        r = int.MaxValue / 2;
                        break;
                    }
                }
            }
            #endregion

            #region SEARCH_COL
            for (int c = 0; c < colE - 1; c++)
            {
                colID = c;
                if (ptT.x < gridPoints[rowID, c + 1].x)
                {
                    break;
                }
            }
            #endregion

            if (rowID >= rowE)
                rowID = rowE - 1;

            if (colID >= colE)
                colID = colE - 1;
#else
                int rowE = m_iRows;
                int colE = m_iCols;
                rowID = 0;
                colID = 0;

                for (int r = 0; r < rowE - 1; r++)
                {
                    for (int c = 0; c < colE - 1; c++)
                    {
                        var quad = new QVector[] {
                            gridPoints[r,c],
                            gridPoints[r,c+1],
                            gridPoints[r+1,c+1],
                            gridPoints[r+1,c],
                        };

                        if (_inZone(ptT, quad))
                        {
                            rowID = r;
                            colID = c;
                            return;
                        }
                    }
                }
#endif
        }
        private QVector[] _getGridNodes(QVector[,] gridPoints, int rowID, int colID)
        {
            var nodes = new QVector[4];
            nodes[0] = _getGridNode(gridPoints, rowID, colID);
            nodes[1] = _getGridNode(gridPoints, rowID, colID + 1);
            nodes[2] = _getGridNode(gridPoints, rowID + 1, colID + 1);
            nodes[3] = _getGridNode(gridPoints, rowID + 1, colID);
            return nodes;
        }
        private QVector _getGridNode(QVector[,] gridPoints, int rowID, int colID)
        {
            QVector node;
            if (rowID < 0 || colID < 0 || rowID >= m_iRows || colID >= m_iCols)
            {
                int r = Math.Min(Math.Max(rowID, 0), m_iRows - 1);
                int c = Math.Min(Math.Max(colID, 0), m_iCols - 1);
                node = new QVector(gridPoints[r, c]);

                if (rowID >= m_iRows)
                    node.x = double.MaxValue / 2;
                if (rowID < 0)
                    node.x = double.MinValue / 2;

                if (colID >= m_iCols)
                    node.y = double.MaxValue / 2;
                if (colID < 0)
                    node.y = double.MinValue / 2;
            }
            else
            {
                node = new QVector(gridPoints[rowID, colID]);
            }
            return node;
        }
        private bool _inZone(QVector P, QVector[] V)
        {
            int n = V.Length;

            int cn = 0;    // the  crossing number counter

            // loop through all edges of the polygon
            for (int k = 0; k < n; k++)
            {
                // edge from V[k]  to V[k+1]
                int k1 = (k + 1) % n;

                if (((V[k].y <= P.y) && (V[k1].y > P.y))     // an upward crossing
                 || ((V[k].y > P.y) && (V[k1].y <= P.y)))
                { // a downward crossing
                  // compute  the actual edge-ray intersect x-coordinate
                    var vt = (P.y - V[k].y) / (V[k1].y - V[k].y);
                    if (P.x < V[k].x + vt * (V[k1].x - V[k].x)) // P.x < intersect
                        ++cn;   // a valid crossing of y=P.y right of P.x
                }
            }

            return (cn & 1) != 0;    // 0 if even (out), and 1 if  odd (in)
        }

        private double _autoScale(QVector[,] vv)
        {
#if (OPT_USING_BI_LINEAR)
                return 1;

                double s = double.MinValue;
                foreach (QVector v in vv)
                {
                    s = Math.Max(s, Math.Abs(v[0]));
                    s = Math.Max(s, Math.Abs(v[1]));
                }

                if (s < 10.0)
                    return 1.0;

                double pow = Math.Log10(s);
                pow = Math.Truncate(pow);
                s = Math.Pow(10.0, pow);

                ////// double n = Math.Round(s / 10.0);
                ////// s = n * 10.0;

                return s;
#else
            return 1;
#endif
        }

        private void _load(QVector[,] pts, string strIniFileName, string strAppName)
        {
            for (int i = 0; i < m_iRows; i++)
            {
                for (int j = 0; j < m_iCols; j++)
                {
                    QVector v = new QVector(2);
                    double x = 0;
                    double y = 0;
                    _loadItem(ref x, strIniFileName, strAppName, "X", i, j);
                    _loadItem(ref y, strIniFileName, strAppName, "Y", i, j);
                    v.x = x;
                    v.y = y;
                    pts[i, j] = v;
                }
            }
        }
        private void _save(QVector[,] pts, string strIniFileName, string strAppName)
        {
            for (int i = 0; i < m_iRows; i++)
            {
                for (int j = 0; j < m_iCols; j++)
                {
                    double x = pts[i, j].x;
                    double y = pts[i, j].y;
                    _saveItem(x, strIniFileName, strAppName, "X", i, j);
                    _saveItem(y, strIniFileName, strAppName, "Y", i, j);
                }
            }
        }

        private void _load(Mat[,] mx, string strIniFileName, string strAppName)
        {
            for (int i = 0; i < m_iRows - 1; i++)
            {
                for (int j = 0; j < m_iCols - 1; j++)
                {
                    _load(mx[i, j], strIniFileName, strAppName + "_" + i + "_" + j);
                }
            }
        }
        private void _save(Mat[,] mx, string strIniFileName, string strAppName)
        {
            for (int i = 0; i < m_iRows - 1; i++)
            {
                for (int j = 0; j < m_iCols - 1; j++)
                {
                    _save(mx[i, j], strIniFileName, strAppName + "_" + i + "_" + j);
                }
            }
        }
        private void _load(Mat mx, string strIniFileName, string strAppName)
        {
            int iRows = mx.Rows;
            int iCols = mx.Cols;
            for (int i = 0; i < iRows; i++)
            {
                for (int j = 0; j < iCols; j++)
                {
                    double value = 0;
                    _loadItem(ref value, strIniFileName, strAppName, "M", i, j);
                    //mx[i, j] = value;
                    mx.At<float>(i, j) = (float)value;
                }
            }
        }
        private void _save(Mat mx, string strIniFileName, string strAppName)
        {
            int iRows = mx.Rows;
            int iCols = mx.Cols;
            for (int i = 0; i < iRows; i++)
            {
                for (int j = 0; j < iCols; j++)
                {
                    //double value = mx[i, j];
                    double value = mx.At<float>(i, j);
                    _saveItem(value, strIniFileName, strAppName, "M", i, j);
                }
            }
        }

        private void _loadItem(ref double value, string strIniFileName, string strAppName, string strKey, int iRow, int iCol)
        {
            string strKeyA = strKey + "_" + iRow + "_" + iCol;
            string strValue = "";
            JetEazy.Win32.Win32Ini.Load(ref strValue, strIniFileName, strAppName, strKeyA);
            double.TryParse(strValue, out value);
        }
        private void _saveItem<T>(T value, string strIniFileName, string strAppName, string strKey, int iRow, int iCol)
        {
            string strKeyA = strKey + "_" + iRow + "_" + iCol;
            string strValue = value.ToString();
            JetEazy.Win32.Win32Ini.Save(strValue, strIniFileName, strAppName, strKeyA);
        }

        #endregion

        private void _roundToHalfPixels(QVector[,] vv)
        {
            //////foreach (QVector v in vv)
            //////{
            //////    _roundToHalfPixels(v);
            //////}
        }
        private void _roundToHalfPixels(QVector v)
        {
            //////v.x = Math.Round(v.x, 1);
            //////v.y = Math.Round(v.y, 1);
        }
        private void _verifyNodePoints()
        {
            for (int row = 0; row < m_iRows; row++)
            {
                for (int col = 0; col < m_iCols; col++)
                {
                    int rv, cv, rw, cw;
                    var vs = m_pointsInView[row, col];
                    var ws = m_pointsInWorld[row, col];

                    try
                    {
                        _getViewZone(out rv, out cv, vs);
                        _getWorldZone(out rw, out cw, ws);
                        var w = ToWorld(vs);
                        var v = ToLocal(w);
                        var delta = v - vs;
                        if (delta.x > 0.25 || delta.y > 0.25)
                        {
                            System.Diagnostics.Trace.WriteLine("DEBUG");
                        }
                    }
                    catch (Exception ex)
                    {
                        //JetEazy.LoggerClass.Instance.WriteException(ex);
                        _LOG(ex);
                        System.Diagnostics.Trace.WriteLine("DEBUG");
                    }
                }
            }
        }

        public void GetViewZone(out int row, out int col, QVector ptViewA)
        {
            var pt = ptViewA / SCALE_OF_VIEW;
            _getViewZone(out row, out col, pt);
        }
        public void GetWorldZone(out int row, out int col, QVector ptWorldA)
        {
            var pt = ptWorldA / SCALE_OF_WORLD;
            _getWorldZone(out row, out col, pt);
        }

        #region LOG
        public static Action<Exception> ExLogFunc = null;
        static void _LOG(Exception ex)
        {
#if (OPT_RESERVED)
            JetEazy.LoggerClass.Instance.WriteException(ex);
#endif
            ExLogFunc?.Invoke(ex);
        }
        #endregion
    }
}
