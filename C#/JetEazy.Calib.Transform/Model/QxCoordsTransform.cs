#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-02 優化 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QMath;
using JetEazy.Transform.Support;
using OpenCvSharp;
using System;
using System.Security.Cryptography;


namespace JetEazy.Transform
{
    /// <summary>
    /// 透視投影座標轉換
    /// <br/> 多點校正: 使用 多節點區域 GetPerspectiveTransform 建構 線性轉換矩陣群
    /// <br/> 座標轉換: 使用 PerspectiveTransform 進行計算
    /// </summary>
    public partial class QxCoordsTransform : ITransform, ICalibCornerPoints, ICalibGridPoints
    {
        #region CONFIG
        const int N_DIMS = 2;
        MatType MAT_TYPE = MatType.CV_64FC1;
        #endregion

        #region PRIVATE_DATA
        private static int m_iCount = 0;
        private string m_name;
        private int m_id;
        #endregion

        #region PRIVATE_KERNEL_DATA
        private QVector[,] _dstPoints;
        private QVector[,] _srcPoints;
        private Mat[,] _mats;           // SRC to DST
        private Mat[,] _matsInv;        // DST to SRC
        private string _srcUnit;
        private string _dstUnit;
        #endregion

        public QxCoordsTransform(string name, string srcUnit, string dstUnit)
        {
            m_id = m_iCount++;
            m_name = name ?? "";
            _srcUnit = srcUnit;
            _dstUnit = dstUnit;
            initZonePointsAndMatrices();
        }
        public QxCoordsTransform(string name) : this(name, "pix", "mm")
        {
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
            //NOTE: 尚未驗證過 !!!
            if (src == null || src._zoneRows < 2 || src._zoneCols < 2)
            {
                initZonePointsAndMatrices();
                return;
            }

            #region COPY_SRC_DST_POINTS
            int rows = src._zoneRows;
            int cols = src._zoneCols;
            _srcPoints = new QVector[rows, cols];
            _dstPoints = new QVector[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    _srcPoints[i, j] = new QVector(src._srcPoints[i, j]);
                    _dstPoints[i, j] = new QVector(src._dstPoints[i, j]);
                }
            }
            syncZoneDimensions();
            rebuildZoneManagers();
            #endregion

            #region COPY_MATRICES
            disposeMatrice();
            initMatrice(true);

            for (int i = 0; i < rows - 1; i++)
            {
                for (int j = 0; j < cols - 1; j++)
                {
                    var srcMat = src._mats[i, j];
                    if (srcMat != null)
                    {
                        _mats[i, j]?.Dispose();
                        _mats[i, j] = srcMat.Clone();
                    }
                    var srcMatInv = src._matsInv[i, j];
                    if (srcMatInv != null)
                    {
                        _matsInv[i, j]?.Dispose();
                        _matsInv[i, j] = srcMatInv.Clone();
                    }
                }
            }
            #endregion
        }
        #endregion

        public ICalibCornerPoints GetCalibCornerPoints()
        {
            return this;
        }
        public ICalibGridPoints GetCalibGridPoints()
        {
            return this;
        }

        #region ROW_COL_INDEXING_FUNCTIONS
        void getRowsCols(object[,] src, out int rows, out int cols)
        {
            //rows = v.GetUpperBound(0) + 1;
            //cols = v.GetUpperBound(1) + 1;
            rows = src.GetLength(0);
            cols = src.GetLength(1);
        }
        bool getCornerRowCol(object[,] src, int index, out int r, out int c)
        {
            getRowsCols(src, out int rows, out int cols);
            int R = rows - 1;
            int C = cols - 1;
            switch (index)
            {
                case 0: r = 0; c = 0; break;    // 左上
                case 1: r = 0; c = C; break;    // 右上
                case 2: r = R; c = C; break;    // 左下
                case 3: r = R; c = 0; break;    // 右下
                default:
                    throw new Exception("Invalide Corner Index !!!");
                    return false;
            }
            return true;
        }
        #endregion

        #region CALIB_CORNER_POINTS
        int ICalibCornerPoints.Counts => 4;
        QVector[] ICalibCornerPoints.GetAll(bool isSrc)
        {
            var pts = isSrc ? _srcPoints : _dstPoints;
            //getRowsCols(pts, out int rows, out int cols);
            int r = _zoneRows - 1;
            int c = _zoneCols - 1;
            return new QVector[]
            {
                new QVector(pts[0,0]),          //左上
                new QVector(pts[0,c]),          //右下
                new QVector(pts[r,c]),          //右下
                new QVector(pts[r,0]),          //左下
            };
        }
        void ICalibCornerPoints.Get(int cornerIndex, out QVector src, out QVector dst)
        {
            getCornerRowCol(_srcPoints, cornerIndex, out int r, out int c);
            src = new QVector(_srcPoints[r, c]);
            dst = new QVector(_dstPoints[r, c]);
        }
        void ICalibCornerPoints.Set(int cornerIndex, QVector src, QVector dst)
        {
            getCornerRowCol(_srcPoints, cornerIndex, out int r, out int c);
            _srcPoints[r, c] = src;
            _dstPoints[r, c] = dst;

            //syncZoneDimensions();
            rebuildZoneManagers();
            SrcDynamicRanges(reset: true);
            DstDynamicRanges(reset: true);
        }
        #endregion

        #region CALIB_GRID_POINTS
        int ICalibGridPoints.Rows => _srcPoints.GetLength(0);
        int ICalibGridPoints.Cols => _srcPoints.GetLength(1);
        void ICalibGridPoints.SetAll(QVector[,] srcPoints, QVector[,] dstPoints)
        {
            checkPointsCondition(srcPoints, dstPoints);

            getRowsCols(srcPoints, out int rows, out int cols);
            _srcPoints = new QVector[rows, cols];
            _dstPoints = new QVector[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var src = srcPoints[r, c];
                    var dst = dstPoints[r, c];
                    bool ok = src != null && dst != null;
                    _srcPoints[r, c] = ok ? new QVector(src) : null;    // new QVector(srcPoints[r, c]);
                    _dstPoints[r, c] = ok ? new QVector(dst) : null;    // new QVector(dstPoints[r, c]);
                }
            }

            syncZoneDimensions();
            rebuildZoneManagers();

            SrcDynamicRanges(reset: true);
            DstDynamicRanges(reset: true);
        }
        void ICalibGridPoints.Update(int row, int col, QVector src, QVector dst)
        {
            getRowsCols(_srcPoints, out int rows, out int cols);
            _srcPoints[row, col] = src;
            _dstPoints[row, col] = dst;

            //syncZoneDimensions();
            rebuildZoneManagers();
            SrcDynamicRanges(reset: true);
            DstDynamicRanges(reset: true);
        }
        void ICalibGridPoints.Get(int row, int col, out QVector src, out QVector dst)
        {
            src = new QVector(_srcPoints[row, col]);
            dst = new QVector(_dstPoints[row, col]);
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        void checkPointsCondition(QVector[,] srcPoints, QVector[,] dstPoints)
        {
            getRowsCols(srcPoints, out int rows, out int cols);
            getRowsCols(srcPoints, out int rows2, out int cols2);
            if (rows != rows2 || cols != cols2)
                throw new Exception("校正點位數量 必須一致!");
            if (rows < 2 || cols < 2)
                throw new ArgumentException($"校正點數 必須大於 2x2 點!");
        }
        #endregion

        public bool Build()
        {
            try
            {
                // 1. 強制重置 Dynamic Range，確保計算範圍包含所有新點位
                SrcDynamicRanges(reset: true);
                DstDynamicRanges(reset: true);

                // 2. 檢查點位有效性
                checkPointsCondition(_srcPoints, _dstPoints);
                syncZoneDimensions();

                // 3. 預先跑 正規化數據, 讓系統抓到 整體的 dynamic range，
                var dummy1 = DynamicRangeUtil.Normalize(_srcPoints, SrcDynamicRanges());
                var dummy2 = DynamicRangeUtil.Normalize(_dstPoints, DstDynamicRanges());

                disposeMatrice();
                initMatrice(false);

                for (int i = 0; i < _zoneRows - 1; i++)
                {
                    for (int j = 0; j < _zoneCols - 1; j++)
                    {
                        var srcNodes = new QVector[] {
                            _srcPoints[i, j],
                            _srcPoints[i, j+1],
                            _srcPoints[i+1, j+1],
                            _srcPoints[i+1, j]
                        };

                        var dstNodes = new QVector[] {
                            _dstPoints[i, j],
                            _dstPoints[i, j+1],
                            _dstPoints[i+1, j+1],
                            _dstPoints[i+1, j]
                        };

                        var srcPts = DynamicRangeUtil.Normalize2f(srcNodes, SrcDynamicRanges());
                        var dstPts = DynamicRangeUtil.Normalize2f(dstNodes, DstDynamicRanges());

                        _mats[i, j]?.Dispose();
                        _matsInv[i, j]?.Dispose();

                        _mats[i, j] = Cv2.GetPerspectiveTransform(srcPts, dstPts);
                        _matsInv[i, j] = Cv2.GetPerspectiveTransform(dstPts, srcPts);
                    }
                }

                rebuildZoneManagers();
                _verifyNodePoints();
                return true;
            }
            catch (Exception ex)
            {
                _ERROR(ex, "Build");
                throw;
            }
        }
        public bool CheckBuildCondition(out double det, out double det2)
        {
            det = double.MaxValue;
            det2 = double.MaxValue;
            bool isAllOk = true;

            for (int r = 0; r < _zoneRows - 1; r++)
            {
                for (int c = 0; c < _zoneCols - 1; c++)
                {
                    // 計算正向與逆向矩陣的行列式，確保矩陣非奇異 (Non-singular)
                    double d1 = (_mats[r, c] == null || _mats[r, c].Empty()) ? 0.0 : _mats[r, c].Determinant();
                    double d2 = (_matsInv[r, c] == null || _matsInv[r, c].Empty()) ? 0.0 : _matsInv[r, c].Determinant();

                    det = Math.Min(det, Math.Abs(d1));
                    det2 = Math.Min(det2, Math.Abs(d2));

                    if (Math.Abs(d1) < 1e-9 || Math.Abs(d2) < 1e-9) 
                        isAllOk = false;
                }
            }
            return isAllOk && (_mats != null);
        }

        public QVector Trans(QVector pt)
        {
            if (pt == null) return null;

            try
            {
                // 1. 尋找點所在的 View 網格區域 (r, c)
                _getSrcZone(out int r, out int c, pt);

                // 2. 取得 正向轉換 (View to World) 矩陣
                var matrix = _mats?[r, c];
                if (matrix == null)
                    return pt;

                // 3. 參考 QTransform 流程：Normalize -> Transform -> DeNormalize
                var srcRanges = SrcDynamicRanges();
                var dstRanges = DstDynamicRanges();

                // 4. 正規化 src (view) 座標
                var srcPts = Normalize(new[] { pt }, srcRanges);

                // 5. 執行透視轉換 (使用 OpenCV)
                var dstPts = Cv2.PerspectiveTransform(srcPts, matrix);

                // 6. 反正規化回 dst (world) 座標
                var rets = DeNormalize(dstPts, dstRanges);
                return rets[0];
            }
            catch (Exception ex)
            {
                _ERROR(ex, "Trans");
                return pt;
            }
        }
        public QVector InvTrans(QVector pt)
        {
            if (pt == null) return null;

            try
            {
                // 1. 尋找點所在的 dst (world) 網格區域 (r, c)
                _getDstZone(out int r, out int c, pt);

                // 2. 取得 逆向轉換 (World to View) 矩陣
                var matInv = _matsInv?[r, c];
                if (matInv == null)
                    return pt;

                // 3. 參考 QTransform 流程：Normalize -> Transform -> DeNormalize
                var srcRanges = SrcDynamicRanges();
                var dstRanges = DstDynamicRanges();

                // 4. 正規化 dst (world) 座標
                var dstPts = Normalize(new[] { pt }, dstRanges);

                // 5. 執行透視轉換 (使用 OpenCV)
                var srcPts = Cv2.PerspectiveTransform(dstPts, matInv);

                // 6. 反正規化回 src (view) 座標
                var rets = DeNormalize(srcPts, srcRanges);
                return rets[0];
            }
            catch (Exception ex)
            {
                _ERROR(ex, "InvTrans");
                return pt;
            }
        }

        #region PRIVATE_MAT_FUNCTIONS
        private void initZonePointsAndMatrices()
        {
            if (_zoneRows < 2 || _zoneCols < 2)
            {
                _zoneRows = _zoneCols = 2;
                _srcPoints = new QVector[2, 2];
                _dstPoints = new QVector[2, 2];
                for (int r = 0; r < _zoneRows; r++)
                {
                    for (int c = 0; c < _zoneCols; c++)
                    {
                        _srcPoints[r, c] = new QVector2(c, r) * 0.1;    // adjust fill a small point
                        _dstPoints[r, c] = new QVector2(c, r) * 0.1;    // adjust fill a small point
                    }
                }

                syncZoneDimensions();
                rebuildZoneManagers();
            }

            disposeMatrice();
            initMatrice(true);
        }
        private void initMatrice(bool setEyes = true)
        {
            int rows = _zoneRows;
            int cols = _zoneCols;

            System.Diagnostics.Trace.Assert(MAT_TYPE == MatType.CV_64FC1);
            System.Diagnostics.Trace.Assert(rows >= 2 && cols >= 2);

            _mats = new Mat[rows - 1, cols - 1];
            _matsInv = new Mat[rows - 1, cols - 1];

            if (setEyes)
            {
                for (int i = 0; i < rows - 1; i++)
                {
                    for (int j = 0; j < cols - 1; j++)
                    {
                        if (_mats[i, j] == null)
                            _mats[i, j] = Mat.Eye(3, 3, MAT_TYPE);
                        if (_matsInv[i, j] == null)
                            _matsInv[i, j] = Mat.Eye(3, 3, MAT_TYPE);
                    }
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
        #endregion

        #region PRIVATE_PERCISE_CHECK_FUNCTIONS
        private void _roundToHalfPixels(QVector[,] vv)
        {
            //NOTE: 在大多數現代高精度系統中，不建議開啟此功能!
        }
        private void _verifyNodePoints()
        {
            for (int row = 0; row < _zoneRows; row++)
            {
                for (int col = 0; col < _zoneCols; col++)
                {
                    var srcNodePt = _srcPoints[row, col];
                    var dstNodePt = _dstPoints[row, col];

                    try
                    {
                        _getSrcZone(out int rv, out int cv, srcNodePt);
                        _getDstZone(out int rw, out int cw, dstNodePt);

                        var dstPt = Trans(srcNodePt);
                        var srcPt = InvTrans(dstPt);

                        var delta = srcPt - srcNodePt;
                        if (delta.x > 0.25 || delta.y > 0.25)
                        {
                            _LOG.Error(
                                    $"[{row},{col}] Delta=({delta.x:0.00}, {delta.y:0.00})" +
                                    $"@ SrcNode=({srcNodePt.x:0.0}, {srcNodePt.y:0.0}) " +
                                    $"@ DstNode=({dstNodePt.x:0.000}, {dstNodePt.y:0.000}) "
                                );
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
        #endregion

        public void Load(string fileName)
        {
            // 只載入 src/dst 點位
            LoadIni(fileName);

            syncZoneDimensions();
            rebuildZoneManagers();
            initMatrice(true);
        }
        public void Save(string fileName)
        {
            // 只保存 src/dst 點位
            SaveIni(fileName);
        }
    }

    partial class QxCoordsTransform
    {
        #region PRIVATE_ZONE_MGR_DATA
        private int _zoneRows = 0;
        private int _zoneCols = 0;
        private ZoneManager _srcZoneManager;
        private ZoneManager _dstZoneManager;
        #endregion



        #region PRIVATE_NODE_RETRIEVAL_FUNCTIONS
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
        #endregion

        #region PRIVATE_ZONE_FUNCTIONS
        private void syncZoneDimensions()
        {
            int rows = _srcPoints != null ? _srcPoints.GetLength(0) : 0;
            int cols = _srcPoints != null ? _srcPoints.GetLength(1) : 0;
            if (_dstPoints != null)
            {
                rows = Math.Min(rows, _dstPoints.GetLength(0));
                cols = Math.Min(cols, _dstPoints.GetLength(1));
            }
            _zoneRows = rows;
            _zoneCols = cols;
        }
        private void rebuildZoneManagers()
        {
            _srcZoneManager = new ZoneManager(_zoneRows, _zoneCols, _srcPoints);
            _dstZoneManager = new ZoneManager(_zoneRows, _zoneCols, _dstPoints);
        }
        private bool _getSrcZone(out int r, out int c, QVector vs)
        {
            // 使用 ZoneManager 進行高效搜尋
            return _srcZoneManager.FindZone(vs, out r, out c, (pt, row, col) => _inZone(pt, row, col, isSrc: true));
        }
        private bool _getDstZone(out int r, out int c, QVector vd)
        {
            return _dstZoneManager.FindZone(vd, out r, out c, (pt, row, col) => _inZone(pt, row, col, isSrc: false));
        }
        private bool _inZone(QVector pt, int r, int c, bool isSrc)
        {
            // 取得該區域的四個頂點
            var pts = _getGridNodes(isSrc ? _srcPoints : _dstPoints, r, c);
            bool inside = false;
            int j = pts.Length - 1;

            for (int i = 0; i < pts.Length; i++)
            {
                // 判斷射線是否穿過 pts[i] 與 pts[j] 組成的邊
                if (((pts[i].y > pt.y) != (pts[j].y > pt.y)) &&
                    (pt.x < (pts[j].x - pts[i].x) * (pt.y - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x))
                {
                    inside = !inside; // 找到一個交點，反轉狀態
                }
                j = i;
            }
            return inside;
        }
        #endregion

        #region PRIVATE_ZONEL_FUNCTIONS_OLD
#if (false)
        private void __getDstZone(out int iRow, out int iCol, QVector pointInDstZone)
        {
            __getZone(out iRow, out iCol, pointInDstZone, _dstPoints);
        }
        private void __getSrcZone(out int iRow, out int iCol, QVector pointInSrcZone)
        {
            var gridPoints = _srcPoints;

            __getZone(out iRow, out iCol, pointInSrcZone, gridPoints);

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
                    if (__inZone(pointInSrcZone, nodes))
                    {
                        iRow = r;
                        iCol = c;
                        return;
                    }
                }
            }
        }
        private void __getZone(out int rowID, out int colID, QVector ptT, QVector[,] gridPoints)
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
        private bool __inZone(QVector P, QVector[] V)
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
#endif
        #endregion

        public void GetSrcZone(out int row, out int col, QVector srcPt)
        {
            _getSrcZone(out row, out col, srcPt);
        }
        public void GetDstZone(out int row, out int col, QVector dstPt)
        {
            _getDstZone(out row, out col, dstPt);
        }
    }

    partial class QxCoordsTransform
    {
        #region PRIVATE_DYNAMIC_RANGE_FUNCTIONS
        private DynamicRange[] _srcRanges;
        private DynamicRange[] _dstRanges;
#if (OLD)
        DynamicRange[] __SrcDynamicRanges(bool reset = false)
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
        DynamicRange[] __DstDynamicRanges(bool reset = false)
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
        DynamicRange[] __createRanges(IEnumerable<QVector> coords)
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
#endif
        /// <summary>
        /// 取得來源端(View)的動態範圍，若為空則根據 _srcPoints 自動建立
        /// </summary>
        DynamicRange[] SrcDynamicRanges(bool reset = false)
        {
            if (reset)
            {
                _srcRanges = null;
                return null;
            }
            // 仿照 QTransform: 自動根據當前點位建立 Range
            if (_srcRanges == null && _srcPoints != null)
            {
                _srcRanges = createRanges(_srcPoints);
            }
            return _srcRanges;
        }
        /// <summary>
        /// 取得目標端(World)的動態範圍，若為空則根據 _dstPoints 自動建立
        /// </summary>
        DynamicRange[] DstDynamicRanges(bool reset = false)
        {
            if (reset)
            {
                _dstRanges = null;
                return null;
            }
            // 仿照 QTransform: 自動根據當前點位建立 Range
            if (_dstRanges == null && _dstPoints != null)
            {
                _dstRanges = createRanges(_dstPoints);
            }
            return _dstRanges;
        }
        /// <summary>
        /// 遍歷二維網格點位，找出 X, Y 的最大與最小值以建立正規化範圍
        /// </summary>
        DynamicRange[] createRanges(QVector[,] coords)
        {
            if (coords == null) return null;

            // 初始化極值
            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;

            // 遍歷二維陣列中的所有點
            foreach (var coord in coords)
            {
                if (coord == null) continue;

                // 仿照 QTransform 使用索引或屬性存取
                minX = Math.Min(minX, coord.x);
                maxX = Math.Max(maxX, coord.x);
                minY = Math.Min(minY, coord.y);
                maxY = Math.Max(maxY, coord.y);
            }

            // 建議在 createRanges 加入的保護邏輯
            if (maxX <= minX) maxX = minX + 1;
            if (maxY <= minY) maxY = minY + 1;

            // 建立二維範圍陣列 (N_DIMS = 2)
            return new DynamicRange[]
            {
                new DynamicRange(minX, maxX),
                new DynamicRange(minY, maxY)
            };
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

        #region LOG
        public static Action<Exception> ExLogFunc = null;
        static NLog.Logger _LOG = NLog.LogManager.GetCurrentClassLogger();
        static void _ERROR(Exception ex, string tag)
        {
            _LOG.Error(ex, tag);
            ExLogFunc?.Invoke(ex);
        }
        #endregion
    }
}
