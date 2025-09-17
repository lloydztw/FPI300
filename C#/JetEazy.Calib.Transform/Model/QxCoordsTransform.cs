#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-08 §ïª© (by LeTian Chang)
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


namespace JetEazy.CoordsTransform
{
    public partial class QxCoordsTransform : ITransform
    {
        #region CONFIG
        const int N_DIMS = 2;
        MatType MAT_TYPE = MatType.CV_64FC1;
        private double SCALE_OF_WORLD => 1.0;
        private double SCALE_OF_VIEW => 1.0;
        #endregion

        #region PRIVATE_DATA
        private static int m_iCount = 0;
        private string m_name;
        private int m_id;
        #endregion

        #region PRIVATE_DATA
        private int _zoneRows = 2;
        private int _zoneCols = 2;
        private QVector[,] _dstPoints;
        private QVector[,] _srcPoints;
        private Mat[,] _mats;           // World to View
        private Mat[,] _matsInv;        // View to World
        #endregion

        public QxCoordsTransform(string name = null)
        {
            m_id = m_iCount++;
            m_name = name;
            initZonePoints();
        }
        public QxCoordsTransform(QxCoordsTransform src)
        {
            m_id = m_iCount++;
            CopyFrom(src);
        }
        public void Dispose()
        {
            disposeMatrice();
        }
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(m_name))
                    return $"{GetType().Name}_{m_id}";
                return m_name;
            }
        }

        #region CLONE_AND_COPY
        public void CopyFrom(QxCoordsTransform src)
        {
            if (src == null)
            {
                initZonePoints();
                return;
            }

            //SCALE_OF_WORLD = src.SCALE_OF_WORLD;
            //SCALE_OF_VIEW = src.SCALE_OF_VIEW;

            _zoneRows = src._zoneRows;
            _zoneCols = src._zoneCols;

            initZonePoints();

            for (int i = 0; i < _zoneRows; i++)
            {
                for (int j = 0; j < _zoneCols; j++)
                {
                    _dstPoints[i, j] = new QVector(src._dstPoints[i, j]);
                    _srcPoints[i, j] = new QVector(src._srcPoints[i, j]);
                }
            }

            for (int i = 0; i < _zoneRows - 1; i++)
            {
                for (int j = 0; j < _zoneCols - 1; j++)
                {
                    if (src._mats[i, j] != null)
                    {
                        //JetEazy.QUtilities.QUtility.SafeDisposeObject(_mats[i, j]);
                        _mats[i, j]?.Dispose();
                        _mats[i, j] = src._mats[i, j].Clone();
                    }
                    if (src._matsInv[i, j] != null)
                    {
                        //JetEazy.QUtilities.QUtility.SafeDisposeObject(_matsInv[i, j]);
                        _matsInv[i, j]?.Dispose();
                        _matsInv[i, j] = src._matsInv[i, j].Clone();
                    }
                }
            }
        }
#if (false)
        public IxCoordTransform Clone()
        {
            QxCoordsTransform obj = new QxCoordsTransform(this);
            return obj;
        }
        object ICloneable.Clone()
        {
            return new QxCoordsTransform(this);
        }
        public override string ToString()
        {
            if (m_name != null)
                return m_name;
            string str = base.ToString();
            return str;
        }
#endif
        #endregion

        public void GetCalibrationPoints(out QVector[,] srcPoints, out QVector[,] dstPoints)
        {
            dstPoints = new QVector[_zoneRows, _zoneCols];
            srcPoints = new QVector[_zoneRows, _zoneCols];
            try
            {
                for (int i = 0; i < _zoneRows; i++)
                {
                    for (int j = 0; j < _zoneCols; j++)
                    {
                        dstPoints[i, j] = new QVector(_dstPoints[i, j]);
                        srcPoints[i, j] = new QVector(_srcPoints[i, j]);
                    }
                }
                _roundToHalfPixels(_srcPoints);
            }
            catch (Exception ex)
            {
                _ERROR(ex, "GetCalibrationPoints");
            }
        }
        public void SetCalibrationPoints(QVector[,] srcPoints, QVector[,] dstPoints)
        {
            _zoneRows = Math.Min(srcPoints.GetUpperBound(0), dstPoints.GetUpperBound(0)) + 1;
            _zoneCols = Math.Min(srcPoints.GetUpperBound(1), dstPoints.GetUpperBound(1)) + 1;

            initZonePoints();
            _roundToHalfPixels(srcPoints);

            for (int i = 0; i < _zoneRows; i++)
            {
                for (int j = 0; j < _zoneCols; j++)
                {
                    _dstPoints[i, j] = dstPoints[i, j];
                    _srcPoints[i, j] = srcPoints[i, j];
                }
            }
        }

        public bool Build()
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
                for (int i = 0; i < _zoneRows - 1; i++)
                {
                    for (int j = 0; j < _zoneCols - 1; j++)
                    {
                        var ptsV = new QVector[] {
                            _srcPoints[i, j],
                            _srcPoints[i, j+1],
                            _srcPoints[i+1, j+1],
                            _srcPoints[i+1, j]
                        };

                        var ptsW = new QVector[] {
                            _dstPoints[i, j],
                            _dstPoints[i, j+1],
                            _dstPoints[i+1, j+1],
                            _dstPoints[i+1, j]
                        };

                        _mats[i, j]?.Dispose();
                        _matsInv[i, j]?.Dispose();

                        _buildMatrix(out _mats[i, j], ptsV, ptsW);   // World To View
                        _buildMatrix(out _matsInv[i, j], ptsW, ptsV);   // View To World
                    }
                }

                _verifyNodePoints();
                return true;
            }
            catch (Exception ex)
            {
                _ERROR(ex, "Build");
                throw ex;
                return false;
            }
        }
        public bool CheckBuildCondition(out double det, out double det2)
        {
            throw new NotImplementedException();
        }

        public QVector Trans(QVector pt)
        {
            return null;
        }
        public QVector InvTrans(QVector pt)
        {
            return null;
        }

        public QVector ToWorld(QVector pt)
        {
            return pt;
        }
        public QVector ToLocal(QVector pt)
        {
            return pt;
        }

        public void Load(string fileName)
        {
            //string ext = System.IO.Path.GetExtension(fileName);
            //if (string.Compare(ext, ".bin", true) == 0 || string.Compare(ext, ".jdb", true) == 0)
            //{
            //    try
            //    {
            //        LoadBin(fileName);
            //    }
            //    catch (Exception ex)
            //    {
            //        //JetEazy.LoggerClass.Instance.WriteException(ex);
            //        _LOG(ex);
            //        System.Diagnostics.Trace.WriteLine("CxCamCoordTransform.LoadBin : Exception = " + ex.Message);
            //    }
            //}
            //else
            //{
            //    //> LoadBin(fileName + ".bin");
            //    LoadIni(fileName);
            //}
        }
        public void Save(string fileName)
        {
            //string ext = System.IO.Path.GetExtension(fileName);
            //if (string.Compare(ext, ".bin", true) == 0 || string.Compare(ext, ".jdb", true) == 0)
            //{
            //    SaveBin(fileName);
            //}
            //else
            //{
            //    SaveIni(fileName);
            //    //> SaveBin(fileName + ".bin");
            //    //> LoadBin(fileName + ".bin");
            //}
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

            JetEazy.Win32.Win32Ini.Load(ref _zoneRows, strIniFileName, "Dimensions", "Rows");
            JetEazy.Win32.Win32Ini.Load(ref _zoneCols, strIniFileName, "Dimensions", "Cols");

            bool bEmptyMatrix = false;
            if (_zoneRows < 2) { _zoneRows = 2; bEmptyMatrix = true; }
            if (_zoneCols < 2) { _zoneCols = 2; bEmptyMatrix = true; }

            initZonePoints();

            if (!bEmptyMatrix)
            {
                _load(_mats, strIniFileName, "MATRIX");
                _load(_matsInv, strIniFileName, "MATRIX_V2W");
            }

            _load(_srcPoints, strIniFileName, "ViewPoints");
            _load(_dstPoints, strIniFileName, "WorldPoints");
        }
        public void SaveIni(string strIniFileName)
        {
            JetEazy.Win32.Win32Ini.Save(SCALE_OF_WORLD, strIniFileName, "Scale", "World");
            JetEazy.Win32.Win32Ini.Save(SCALE_OF_VIEW, strIniFileName, "Scale", "View");
            JetEazy.Win32.Win32Ini.Save(_zoneRows, strIniFileName, "Dimensions", "Rows");
            JetEazy.Win32.Win32Ini.Save(_zoneCols, strIniFileName, "Dimensions", "Cols");

            _save(_mats, strIniFileName, "MATRIX");
            _save(_matsInv, strIniFileName, "MATRIX_V2W");
            _save(_srcPoints, strIniFileName, "ViewPoints");
            _save(_dstPoints, strIniFileName, "WorldPoints");
        }


        #region PRIVATE_FUNCTIONS

        private void initZonePoints()
        {
            if (_zoneRows < 2 || _zoneCols < 2)
                throw new Exception("Rows or Cols is too small.");

            _srcPoints = new QVector[_zoneRows, _zoneCols];
            _dstPoints = new QVector[_zoneRows, _zoneCols];

            for (int r = 0; r < _zoneRows; r++)
            {
                for (int c = 0; c < _zoneCols; c++)
                {
                    _srcPoints[r, c] = new QVector2(r, c) * 0.01;
                    _dstPoints[r, c] = new QVector2(r, c) * 0.01;
                }
            }

            disposeMatrice();
            initMatrice();
        }
        private void initMatrice()
        {
            System.Diagnostics.Trace.Assert(MAT_TYPE == MatType.CV_64FC1);

            _mats = new Mat[_zoneRows - 1, _zoneCols - 1];
            _matsInv = new Mat[_zoneRows - 1, _zoneCols - 1];

            for (int i = 0; i < _zoneRows - 1; i++)
            {
                for (int j = 0; j < _zoneCols - 1; j++)
                {
                    if (_mats[i, j] == null)
                        _mats[i, j] = Mat.Eye(3, 3, MAT_TYPE);
                    if (_matsInv[i, j] == null)
                        _matsInv[i, j] = Mat.Eye(3, 3, MAT_TYPE);
                }
            }
        }
        private void disposeMatrice()
        {
            disposeMatrice(_mats);
            _mats = null;
            disposeMatrice(_matsInv);
            _matsInv = null;
        }
        private void disposeMatrice(Mat[,] matrice)
        {
            if (matrice != null)
            {
                int rows = matrice.GetUpperBound(0) + 1;
                int cols = matrice.GetUpperBound(1) + 1;
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        matrice[i, j]?.Dispose();
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

                //mat1XY[i, 0] = 1;
                //mat1XY[i, 1] = wx;
                //mat1XY[i, 2] = wy;
                //mat1XY[i, 3] = wx * wy;

                //matVX[i] = vx;
                //matVY[i] = vy;

                mat1XY.Set(i, 0, 1f);
                mat1XY.Set(i, 1, (float)wx);
                mat1XY.Set(i, 2, (float)wy);
                mat1XY.Set(i, 3, (float)(wx * wy));

                matVX.Set(i, (float)vx);
                matVY.Set(i, (float)vy);
            }

            Mat matInv = new Mat(4, 4, MAT_TYPE);

            //mat1XY.Invert(matInv, InvertMethod.Normal);
            Cv2.Invert(mat1XY, matInv, DecompTypes.LU);

            Mat mat1R = matInv * matVX;
            Mat mat2R = matInv * matVY;

            matT = new Mat(2, 4, MAT_TYPE);
            for (int j = 0; j < 4; j++)
            {
                //matT[0, j] = mat1R[j];
                //matT[1, j] = mat2R[j];
                matT.Set(0, j, mat1R.Get<float>(j));
                matT.Set(1, j, mat2R.Get<float>(j));
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
            _getZone(out iRow, out iCol, pointWorld, _dstPoints);
        }
        private void _getViewZone(out int iRow, out int iCol, QVector pointView)
        {
            var gridPoints = _srcPoints;

            _getZone(out iRow, out iCol, pointView, gridPoints);

            int[] delta = new int[] { 0, -1, 1 };
            int rowE1 = _zoneRows - 1;
            int colE1 = _zoneCols - 1;

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
            int rowE = _zoneRows;
            int colE = _zoneCols;

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
            if (rowID < 0 || colID < 0 || rowID >= _zoneRows || colID >= _zoneCols)
            {
                int r = Math.Min(Math.Max(rowID, 0), _zoneRows - 1);
                int c = Math.Min(Math.Max(colID, 0), _zoneCols - 1);
                node = new QVector(gridPoints[r, c]);

                if (rowID >= _zoneRows)
                    node.x = double.MaxValue / 2;
                if (rowID < 0)
                    node.x = double.MinValue / 2;

                if (colID >= _zoneCols)
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
            for (int i = 0; i < _zoneRows; i++)
            {
                for (int j = 0; j < _zoneCols; j++)
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
            for (int i = 0; i < _zoneRows; i++)
            {
                for (int j = 0; j < _zoneCols; j++)
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
            for (int i = 0; i < _zoneRows - 1; i++)
            {
                for (int j = 0; j < _zoneCols - 1; j++)
                {
                    _load(mx[i, j], strIniFileName, strAppName + "_" + i + "_" + j);
                }
            }
        }
        private void _save(Mat[,] mx, string strIniFileName, string strAppName)
        {
            for (int i = 0; i < _zoneRows - 1; i++)
            {
                for (int j = 0; j < _zoneCols - 1; j++)
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
            for (int row = 0; row < _zoneRows; row++)
            {
                for (int col = 0; col < _zoneCols; col++)
                {
                    int rv, cv, rw, cw;
                    var vs = _srcPoints[row, col];
                    var ws = _dstPoints[row, col];

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
                        _ERROR(ex, "_verifyNodePoints");
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


    }

    partial class QxCoordsTransform
    {
        #region PRIVATE_DYNAMIC_RANGE_FUNCTIONS
        private DynamicRange[] _srcRanges;
        private DynamicRange[] _dstRanges;
        DynamicRange[] SrcDynamicRanges(bool reset = false)
        {
            if (reset)
            {
                return _srcRanges = null;
            }
            if (_srcRanges == null)
            {
                //_srcRanges = createRanges(_srcRefs);
            }
            return _srcRanges;
        }
        DynamicRange[] DstDynamiceRanges(bool reset = false)
        {
            if (reset)
            {
                return _dstRanges = null;
            }
            if (_dstRanges == null)
            {
                //_dstRanges = createRanges(_dstRefs);
            }
            return _dstRanges;
        }
        DynamicRange[] createRanges(IEnumerable<QVector> coords)
        {
            QVector v_min = new QVector();  //double.MaxValue, double.MaxValue);
            QVector v_max = new QVector();  //double.MinValue, double.MinValue);

            for (int i = 0; i < N_DIMS; i++)
            {
                v_min[i] = double.MaxValue;
                v_max[i] = double.MinValue;
            }

            foreach (var coord in coords)
            {
                for (int i = 0; i < N_DIMS; i++)
                {
                    v_min[i] = Math.Max(v_min[i], coord[i]);
                    v_max[i] = Math.Max(v_max[i], coord[i]);
                }
            }

            var ranges = new DynamicRange[N_DIMS];
            for (int i = 0; i < N_DIMS; i++)
            {
                ranges[i] = new DynamicRange(v_min[i], v_max[i]);
            }

            return ranges;
        }
        #endregion

        public static Point2d[] Normalize(QVector[] coords, DynamicRange[] ranges)
        {
            var pts = Array.ConvertAll(coords, (c) =>
            {
                double x = ranges[0].Normalize(c.X);
                double y = ranges[1].Normalize(c.Y);
                var pt = new Point2d(x, y);
                return pt;
            });
            return pts;
        }
        public static QVector[] DeNormalize(Point2d[] points, DynamicRange[] ranges, params object[] args)
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

    partial class QxCoordsTransform
    {
        public static Action<Exception> ExLogFunc = null;

        #region LOG
        static NLog.Logger _LOG = NLog.LogManager.GetCurrentClassLogger();
        static void _ERROR(Exception ex, string tag)
        {
            _LOG.Error(ex, tag);
            ExLogFunc?.Invoke(ex);
        }
        #endregion
    }
}
