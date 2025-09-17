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
using JetEazy.Utils;
using LaserAlignDX.Model.Coords;
using OpenCvSharp;
using System;
using System.Windows.Media;
using ZXing;


namespace LaserAlignDX.Model.Transforms
{
    /// <summary>
    /// 透視投影座標轉換
    /// </summary>
    public interface ITransform
    {
        QCoord Trans(QVector pt);
        QCoord InvTrans(QVector pt);

        //QCoord Trans(QCoord coord);
        //QCoord InvTrans(QCoord coord);
    }


    /// <summary>
    /// 透視投影座標轉換
    /// </summary>
    public class QTransform : ITransform, IDisposable
    {
        public const int N_POINTS = 4;

        #region PROTECTED_DATA
        protected Mat _mat;
        protected Mat _matInv;
        protected QCoord[] _srcRefs;
        protected QCoord[] _dstRefs;
        #endregion

        public QTransform(string name, string srcUnit, string dstUnit)
        {
            Name = name == null ? "" : name;

            _srcRefs = new QCoord[N_POINTS];
            _dstRefs = new QCoord[N_POINTS];

            for (int i = 0; i < N_POINTS; i++)
            {
                int ry = i / (N_POINTS / 2);
                int cx = i % (N_POINTS / 2);
                _srcRefs[i] = new QCoord(ry, cx, srcUnit);
                _dstRefs[i] = new QCoord(ry, cx, dstUnit);
            }
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
        public QCoord GetSrcRef(int i)
        {
            return _srcRefs[i];
        }
        public QCoord GetDstRef(int i)
        {
            return _dstRefs[i];
        }

        public void SetSrcRef(int i, QCoord pt)
        {
            _srcRefs[i] = pt;
            SrcDynamicRanges(reset: true);
        }
        public void SetDstRef(int i, QCoord pt)
        {
            _dstRefs[i] = pt;
            DstDynamiceRanges(reset: true);
        }

        public bool Build()
        {
            bool isFirstTime = _mat == null;

            #region 條件檢查
            if (_srcRefs.Length != _srcRefs.Length || _srcRefs.Length < N_POINTS)
            {
                throw new ArgumentException($"校正點數必須相同都是 {N_POINTS} 點!");
            }
            #endregion

            #region 建立轉換矩陣
            _mat?.Dispose();
            _mat = null;
            _matInv?.Dispose();
            _matInv = null;

            // 正規化數據
            //QCoord.Normalize(_srcRefs, inplace: true);
            //QCoord.Normalize(_dstRefs, inplace: true);
            //var srcPts = Array.ConvertAll(_srcRefs, v => new Point2f((float)v.X, (float)v.Y));
            //var dstPts = Array.ConvertAll(_dstRefs, v => new Point2f((float)v.X, (float)v.Y));
            var srcPts = Normalize(_srcRefs, SrcDynamicRanges());
            var dstPts = Normalize(_dstRefs, DstDynamiceRanges());

            // 正轉換
            _mat = Cv2.FindHomography(srcPts, dstPts);
            //_mat = Cv2.GetPerspectiveTransform(srcPts, dstPts);

            // 逆轉換
            //_matInv = Cv2.GetPerspectiveTransform(dstPts, srcPts);
            _matInv = Cv2.FindHomography(dstPts, srcPts);
            #endregion

            // 脫離正規化
            //QCoord.DeNormalize(_srcRefs, inplace: true);
            //QCoord.DeNormalize(_dstRefs, inplace: true);

            #region 判定轉換矩陣的品質
            bool ok = _mat != null && _matInv != null;
            if (ok && !isFirstTime)
            {
                double det = _mat.Determinant();
                double det2 = _matInv.Determinant();

                GaUtil.LOG($"Matrix[{Name}] det1= {det:0.000000}");
                GaUtil.LOG($"Matrix[{Name}] det2= {det2:0.000000}");

                ok = Math.Abs(det) > 1e-9;
                if (!ok)
                {
                    //>>> throw new ArgumentException($"校正點選取不當, 造成建立的 矩陣 det 太小 {det} !");
                    GaUtil.LOG("校正點選取不當, 造成建立的 轉換矩陣 det 太小!", System.Drawing.Color.Red);
                }
            }
            #endregion

            return ok;
        }
        
        public bool Build(QCoord[] _srcRefs, QCoord[] _dstRefs)
        {
            bool isFirstTime = _mat == null;

            #region 條件檢查
            if (_srcRefs.Length != _srcRefs.Length || _srcRefs.Length < N_POINTS)
            {
                throw new ArgumentException($"校正點數必須相同都是 {N_POINTS} 點!");
            }
            #endregion

            #region 建立轉換矩陣
            _mat?.Dispose();
            _mat = null;
            _matInv?.Dispose();
            _matInv = null;

            // 正規化數據
            //QCoord.Normalize(_srcRefs, inplace: true);
            //QCoord.Normalize(_dstRefs, inplace: true);
            //var srcPts = Array.ConvertAll(_srcRefs, v => new Point2f((float)v.X, (float)v.Y));
            //var dstPts = Array.ConvertAll(_dstRefs, v => new Point2f((float)v.X, (float)v.Y));
            var srcPts = Normalize(_srcRefs, SrcDynamicRanges());
            var dstPts = Normalize(_dstRefs, DstDynamiceRanges());

            // 正轉換
            _mat = Cv2.FindHomography(srcPts, dstPts);
            //_mat = Cv2.GetPerspectiveTransform(srcPts, dstPts);

            // 逆轉換
            //_matInv = Cv2.GetPerspectiveTransform(dstPts, srcPts);
            _matInv = Cv2.FindHomography(dstPts, srcPts);
            #endregion

            // 脫離正規化
            //QCoord.DeNormalize(_srcRefs, inplace: true);
            //QCoord.DeNormalize(_dstRefs, inplace: true);

            #region 判定轉換矩陣的品質
            bool ok = _mat != null && _matInv != null;
            if (ok && !isFirstTime)
            {
                double det = _mat.Determinant();
                double det2 = _matInv.Determinant();

                GaUtil.LOG($"Matrix[{Name}] det1= {det:0.000000}");
                GaUtil.LOG($"Matrix[{Name}] det2= {det2:0.000000}");

                ok = Math.Abs(det) > 1e-9;
                if (!ok)
                {
                    //>>> throw new ArgumentException($"校正點選取不當, 造成建立的 矩陣 det 太小 {det} !");
                    GaUtil.LOG("校正點選取不當, 造成建立的 轉換矩陣 det 太小!", System.Drawing.Color.Red);
                }
            }
            #endregion

            return ok;
        }

        QCoord ITransform.Trans(QVector pt)
        {
            if (_mat == null || pt == null)
                return null;

            //if (pt is QCoord c)
            //    return Trans(c);

            var coord = new QCoord(pt.X, pt.Y, _srcRefs[0].UNIT);
            return Trans(coord);
        }
        QCoord ITransform.InvTrans(QVector pt)
        {
            if (_matInv == null || pt == null)
                return null;

            //if (pt is QCoord c)
            //    return InvTrans(c);

            var coord = new QCoord(pt.X, pt.Y, _dstRefs[0].UNIT);
            return InvTrans(coord);
        }

        public QCoord Trans(QCoord coord)
        {
            if (_mat == null || coord == null)
                return coord;

            //var order = Math.Max(1, coord.ORDER);
            //var pt = new Point2d(coord.X / order, coord.Y / order);
            //var pts = Cv2.PerspectiveTransform(new[] { pt }, _mat);
            //var ret = new QCoord(pts[0].X * order, pts[0].Y * order, coord.ORDER, coord.UNIT);
            //return ret;

            var rets = Trans(new[] { coord });
            return rets[0];
        }
        public QCoord InvTrans(QCoord coord)
        {
            if (_matInv == null || coord == null)
                return coord;

            //var order = Math.Max(1, coord.ORDER);
            //var pt = new Point2d(coord.X / order, coord.Y / order);
            //var pts = Cv2.PerspectiveTransform(new[] { pt }, _matInv);
            //var ret = new QCoord(pts[0].X * order, pts[0].Y * order, coord.ORDER, coord.UNIT);
            //return ret;

            var rets = InvTrans(new[] { coord });
            return rets[0];
        }
        
        public QCoord[] Trans(QCoord[] coords)
        {
            if (_mat == null || coords == null || coords.Length == 0)
                return coords;

            var srcRanges = SrcDynamicRanges();
            var dstRanges = DstDynamiceRanges();
            var srcPts = Normalize(coords, srcRanges);

            //var unit = coords[0].UNIT;
            //var order = coords[0].ORDER;
            //var srcPts = Array.ConvertAll(coords, c => new Point2d(c.X / order, c.y / order));

            var dstPts = Cv2.PerspectiveTransform(srcPts, _mat);

            //var dstUnit = _dstRefs[0].UNIT;
            //var dstOrder = _dstRefs[0].ORDER;
            //var ret = Array.ConvertAll(dstPts, pt => new QCoord(pt.X * dstOrder, pt.Y * dstOrder, dstOrder, dstUnit));

            var ret = DeNormalize(dstPts, dstRanges, _dstRefs[0]);
            return ret;
        }
        public QCoord[] InvTrans(QCoord[] coords)
        {
            if (_matInv == null || coords == null || coords.Length == 0)
                return coords;

            var srcRanges = SrcDynamicRanges();
            var dstRanges = DstDynamiceRanges();
            var dstPts = Normalize(coords, dstRanges);

            //var unit = coords[0].UNIT;
            //var order = coords[0].ORDER;
            //var dstPts = Array.ConvertAll(coords, c => new Point2d(c.X / order, c.y / order));

            var srcPts = Cv2.PerspectiveTransform(dstPts, _matInv);

            //var srcUnit = _srcRefs[0].UNIT;
            //var srcOrder = _srcRefs[0].ORDER;
            //var ret = Array.ConvertAll(dstPts, pt => new QCoord(pt.X * srcOrder, pt.Y * srcOrder, order, srcUnit));

            var ret = DeNormalize(srcPts, srcRanges, _srcRefs[0]);
            return ret;
        }

        public virtual void Load(string iniFileName, string sectName)
        {
            if (sectName == null)
                sectName = Name;

            for (int i = 0; i < N_POINTS; i++)
            {
                _srcRefs[i].Load(iniFileName, sectName, $"SrcRef_{i}");
                _dstRefs[i].Load(iniFileName, sectName, $"DstRef_{i}");
            }
        }
        public virtual void Save(string iniFileName, string sectName)
        {
            if (sectName == null)
                sectName = Name;

            for (int i = 0; i < N_POINTS; i++)
            {
                _srcRefs[i].Save(iniFileName, sectName, $"SrcRef_{i}");
                _dstRefs[i].Save(iniFileName, sectName, $"DstRef_{i}");
            }
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
            var srcUnit = T0._srcRefs[0].UNIT;
            //var dstOrder = T._dstRefs[0].ORDER;
            var dstUnit = T._dstRefs[0].UNIT;

            var newTrans = new QTransform("MERGED", srcUnit, dstUnit)
            {
                _mat = M,
                _matInv = invM,
                _srcRefs = T0._srcRefs,
                _dstRefs = T._dstRefs
            };

            return newTrans;
        }
        #endregion

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
                _srcRanges = createRanges(_srcRefs);
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
                _dstRanges = createRanges(_dstRefs);
            }
            return _dstRanges;
        }
        DynamicRange[] createRanges(QCoord[] coords)
        {
            int NDim = coords[0].Length;

            var v_min = coords[0];
            var v_max = new QCoord(v_min);

            foreach (var coord in coords)
            {
                for (int i = 0; i < NDim; i++)
                    v_max[i] = Math.Max(v_max[i], coord[i]);
            }

            var ranges = new DynamicRange[NDim];
            for (int i = 0; i < NDim; i++)
            {
                ranges[i] = new DynamicRange(v_min[i], v_max[i]);
            }

            return ranges;
        }
        #endregion

        Point2d[] Normalize(QCoord[] coords, DynamicRange[] ranges)
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
        QCoord[] DeNormalize(Point2d[] points, DynamicRange[] ranges, QCoord ptRef)
        {
            var coords = Array.ConvertAll(points, (p) =>
            {
                double x = ranges[0].DeNormalize(p.X);
                double y = ranges[1].DeNormalize(p.Y);
                var coord = new QCoord(x, y, ptRef.UNIT);
                return coord;
            });
            return coords;
        }
    }
}
