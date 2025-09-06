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

        public QTransform(string name = null)
        {
            Name = name == null ? "" : name;

            _srcRefs = new QCoord[N_POINTS];
            _dstRefs = new QCoord[N_POINTS];
            for (int i = 0; i < N_POINTS; i++)
            {
                int r = i / (N_POINTS / 2);
                int c = i % (N_POINTS / 2);
                _srcRefs[i] = new QCoord(r, c);
                _dstRefs[i] = new QCoord(r, c);
            }

            Build();
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
        }
        public void SetDstRef(int i, QCoord pt)
        {
            _dstRefs[i] = pt;
        }

        public bool Build()
        {
            #region 條件檢查
            if (_srcRefs.Length != _srcRefs.Length || _srcRefs.Length < N_POINTS)
            {
                throw new ArgumentException($"校正點數必須相同都是 {N_POINTS} 點!");
            }
            #endregion

            // 正規化數據
            QCoord.Normalize(_srcRefs, inplace: true);
            QCoord.Normalize(_dstRefs, inplace: true);

            #region 建立轉換矩陣
            _mat?.Dispose();
            _mat = null;
            _matInv?.Dispose();
            _matInv = null;

            // 正轉換
            var srcPts = Array.ConvertAll(_srcRefs, v => new Point2f((float)v.X, (float)v.Y));
            var dstPts = Array.ConvertAll(_dstRefs, v => new Point2f((float)v.X, (float)v.Y));
            _mat = Cv2.GetPerspectiveTransform(srcPts, dstPts);

            // 逆轉換
            _matInv = new Mat();
            Cv2.Invert(_mat, _matInv);
            #endregion

            // 脫離正規化
            QCoord.DeNormalize(_srcRefs, inplace: true);
            QCoord.DeNormalize(_dstRefs, inplace: true);

            #region 判定轉換矩陣的品質
            bool ok = _mat != null && _matInv != null;
            if (ok)
            {
                double det = _mat.Determinant();
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

            var coord = new QCoord(pt.X, pt.Y, _srcRefs[0].ORDER, _srcRefs[0].UNIT);
            return Trans(coord);
        }
        QCoord ITransform.InvTrans(QVector pt)
        {
            if (_matInv == null || pt == null)
                return null;

            //if (pt is QCoord c)
            //    return InvTrans(c);

            var coord = new QCoord(pt.X, pt.Y, _dstRefs[0].ORDER, _dstRefs[0].UNIT);
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

            var unit = coords[0].UNIT;
            var order = coords[0].ORDER;

            var srcPts = Array.ConvertAll(coords, c => new Point2d(c.X / order, c.y / order));
            var dstPts = Cv2.PerspectiveTransform(srcPts, _mat);

            var ret = Array.ConvertAll(dstPts, pt => new QCoord(pt.X * order, pt.Y * order, order, unit));
            return ret;
        }
        public QCoord[] InvTrans(QCoord[] coords)
        {
            if (_matInv == null || coords == null || coords.Length == 0)
                return coords;

            var unit = coords[0].UNIT;
            var order = coords[0].ORDER;

            var srcPts = Array.ConvertAll(coords, c => new Point2d(c.X / order, c.y / order));
            var dstPts = Cv2.PerspectiveTransform(srcPts, _matInv);

            var ret = Array.ConvertAll(dstPts, pt => new QCoord(pt.X * order, pt.Y * order, order, unit));
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

            Build();
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

        public static QTransform Merge(params QTransform[] transforms)
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

            var newTrans = new QTransform()
            {
                _mat = M,
                _matInv = invM,
                _srcRefs = T0._srcRefs,
                _dstRefs = T._dstRefs
            };

            return newTrans;
        }
    }
}
