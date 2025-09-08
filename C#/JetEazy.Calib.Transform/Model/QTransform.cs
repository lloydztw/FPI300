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
    /// <summary>
    /// 透視投影座標轉換
    /// </summary>
    public partial class QTransform : ITransform, ICalibCornerPoints, ICalibGridPoints
    {
        public const int N_DIMS = 2;

        #region PROTECTED_DATA
        internal Mat _mat;
        internal Mat _matInv;
        internal QVector[,] _srcPoints;
        internal QVector[,] _dstPoints;
        #endregion

        #region PRIVATE_DATA
        private string _srcUnit;
        private string _dstUnit;
        #endregion

        public QTransform(string name, string srcUnit, string dstUnit)
        {
            Name = name ?? "";
            _srcUnit = srcUnit;
            _dstUnit = dstUnit;
            _srcPoints = initDefaultPoints(100, _srcUnit);
            _dstPoints = initDefaultPoints(100, _dstUnit);
            Build();
        }
        public QTransform(string name) : this(name, "pix", "mm")
        {
        }
        public virtual void Dispose()
        {
            _mat?.Dispose();
            _mat = null;
            _matInv?.Dispose();
            _matInv = null;
        }
        public string Name
        {
            get;
            set;
        }

        #region KP_INDEXING_FUNCTIONS
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

        int ICalibCornerPoints.Counts => 4;
        QVector[] ICalibCornerPoints.GetAll(bool isSrc)
        {
            var pts = isSrc ? _srcPoints : _dstPoints;
            getRowsCols(pts, out int rows, out int cols);
            int r = rows - 1;
            int c = cols - 1;
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

            SrcDynamicRanges(reset: true);
            DstDynamicRanges(reset: true);
        }

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
                    _srcPoints[r, c] = new QVector(srcPoints[r, c]);
                    _dstPoints[r, c] = new QVector(dstPoints[r, c]);
                }
            }

            SrcDynamicRanges(reset: true);
            DstDynamicRanges(reset: true);
        }
        void ICalibGridPoints.Update(int row, int col, QVector src, QVector dst)
        {
            getRowsCols(_srcPoints, out int rows, out int cols);
            _srcPoints[row, col] = src;
            _dstPoints[row, col] = dst;

            SrcDynamicRanges(reset: true);
            DstDynamicRanges(reset: true);
        }
        void ICalibGridPoints.Get(int row, int col, out QVector src, out QVector dst)
        {
            src = new QVector(_srcPoints[row, col]);
            dst = new QVector(_dstPoints[row, col]);
        }

        public bool Build()
        {
            //bool isFirstTime = _mat == null;

            // 條件檢查
            checkPointsCondition(_srcPoints, _dstPoints);

            // 準備建立轉換矩陣
            _mat?.Dispose();
            _mat = null;
            _matInv?.Dispose();
            _matInv = null;

            // 正規化數據
            var srcPts = Normalize(_srcPoints, SrcDynamicRanges());
            var dstPts = Normalize(_dstPoints, DstDynamicRanges());

            // 正轉換
            //_mat = Cv2.GetPerspectiveTransform(srcPts, dstPts);
            _mat = Cv2.FindHomography(srcPts, dstPts);

            // 逆轉換
            //_matInv = Cv2.GetPerspectiveTransform(dstPts, srcPts);
            _matInv = Cv2.FindHomography(dstPts, srcPts);

            // 判定轉換矩陣的品質
            bool ok = CheckBuildCondition(out var det, out var det2);
            return ok;
        }
        public bool CheckBuildCondition(out double det, out double det2)
        {
            // 判定轉換矩陣的品質
            det = _mat == null ? 0.0 : _mat.Determinant();
            det2 = _matInv == null ? 0.0 : _matInv.Determinant();
            bool ok = Math.Abs(det) > 1e-9 && Math.Abs(det2) > 1e-9;
            return ok;
        }

        public QVector Trans(QVector coord)
        {
            if (_mat == null || coord == null)
                return coord;
            var rets = Trans(new[] { coord });
            return rets[0];
        }
        public QVector InvTrans(QVector coord)
        {
            if (_matInv == null || coord == null)
                return coord;
            var rets = InvTrans(new[] { coord });
            return rets[0];
        }
        public QVector[] Trans(QVector[] coords)
        {
            if (_mat == null || coords == null || coords.Length == 0)
                return coords;

            var srcRanges = SrcDynamicRanges();
            var dstRanges = DstDynamicRanges();
            var srcPts = Normalize(coords, srcRanges);

            //var unit = coords[0].UNIT;
            //var order = coords[0].ORDER;
            //var srcPts = Array.ConvertAll(coords, c => new Point2d(c.X / order, c.y / order));

            var dstPts = Cv2.PerspectiveTransform(srcPts, _mat);

            //var dstUnit = _dstRefs[0].UNIT;
            //var dstOrder = _dstRefs[0].ORDER;
            //var ret = Array.ConvertAll(dstPts, pt => new QCoord(pt.X * dstOrder, pt.Y * dstOrder, dstOrder, dstUnit));

            var ret = DeNormalize(dstPts, dstRanges);
            return ret;
        }
        public QVector[] InvTrans(QVector[] coords)
        {
            if (_matInv == null || coords == null || coords.Length == 0)
                return coords;

            var srcRanges = SrcDynamicRanges();
            var dstRanges = DstDynamicRanges();
            var dstPts = Normalize(coords, dstRanges);

            //var unit = coords[0].UNIT;
            //var order = coords[0].ORDER;
            //var dstPts = Array.ConvertAll(coords, c => new Point2d(c.X / order, c.y / order));

            var srcPts = Cv2.PerspectiveTransform(dstPts, _matInv);

            //var srcUnit = _srcRefs[0].UNIT;
            //var srcOrder = _srcRefs[0].ORDER;
            //var ret = Array.ConvertAll(dstPts, pt => new QCoord(pt.X * srcOrder, pt.Y * srcOrder, order, srcUnit));

            var ret = DeNormalize(srcPts, srcRanges);
            return ret;
        }

        #region PRIVATE_FUNCTIONS
        QVector[,] initDefaultPoints(double v, string unit)
        {
            return new QVector[2, 2]
            {
                { new QCoord(0, 0, unit), new QCoord(0, v, unit) },
                { new QCoord(v, 0, unit), new QCoord(v, v, unit) },
            };
        }
        void checkPointsCondition(QVector[,] srcPoints, QVector[,] dstPoints)
        {
            getRowsCols(srcPoints, out int rows, out int cols);
            getRowsCols(srcPoints, out int rows2, out int cols2);
            if (rows != rows2 || cols != cols2)
                throw new Exception("校正點位數量 必須一致!");
            if (rows < 2 || cols <2)
                throw new ArgumentException($"校正點數 必須大於 2x2 點!");
        }

        #endregion

        public virtual void Load(string filename)
        {
            //LoadJson(filename);
            //LoadBin(filename);
            this.LoadIni(filename);
        }
        public virtual void Save(string filename)
        {
            //SaveJson(filename);
            //SaveBin(filename);
            this.SaveIni(filename);
        }


    }

    partial class QTransform
    {
        #region PRIVATE_DYNAMIC_RANGES_DATA
        private DynamicRange[] _srcRanges;
        private DynamicRange[] _dstRanges;
        #endregion

        #region DYNAMIC_RANGE_FUNCTIONS
        DynamicRange[] SrcDynamicRanges(bool reset = false)
        {
            if (reset)
            {
                return _srcRanges = null;
            }
            if (_srcRanges == null)
            {
                _srcRanges = createRanges(_srcPoints);
            }
            return _srcRanges;
        }
        DynamicRange[] DstDynamicRanges(bool reset = false)
        {
            if (reset)
            {
                return _dstRanges = null;
            }
            if (_dstRanges == null)
            {
                _dstRanges = createRanges(_srcPoints);
            }
            return _dstRanges;
        }
        DynamicRange[] createRanges(QVector[,] coords)
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
        public static Point2d[] Normalize(QVector[,] coords, DynamicRange[] ranges)
        {
            var pts = new List<Point2d>();
            foreach(var c in coords)
            {
                double x = ranges[0].Normalize(c.X);
                double y = ranges[1].Normalize(c.Y);
                pts.Add(new Point2d(c.X, c.Y));
            }
            return pts.ToArray();
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

        #region RESERVED
        static QTransform Merge(params QTransform[] transforms)
        {
            int N = transforms.Length;

            if (N == 0)
                return null;

            if (N == 1)
                return transforms[0];

            // M = M[N-1] * M[N-2] * ... * M[1] * M[0]
            var T0 = transforms[0];
            var T = transforms[N - 1];
            var M = T._mat.Clone();
            for (int i = N - 2; i >= 0; i--)
            {
                var old = M;
                M = M * transforms[i]._mat;
                old?.Dispose();
            }

            var invM = new Mat();
            Cv2.Invert(M, invM);

            //var srcOrder = T0._srcRefs[0].ORDER;
            //var srcUnit = T0._srcRefs[0].UNIT;
            //var dstOrder = T._dstRefs[0].ORDER;
            //var dstUnit = T._dstRefs[0].UNIT;

            var newTrans = new QTransform("MERGED")
            {
                _mat = M,
                _matInv = invM,
                _srcPoints = T0._srcPoints,
                _dstPoints = T._dstPoints
            };

            return newTrans;
        }
        #endregion
    }
}
