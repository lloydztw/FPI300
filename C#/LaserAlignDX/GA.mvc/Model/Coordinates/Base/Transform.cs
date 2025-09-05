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


namespace LaserAlignDX.Model.Coords
{
    public interface ITransform
    {
        QVector Trans(QVector coord);
        QVector InvTrans(QVector coord);
    }


    /// <summary>
    /// 透視投影座標轉換 (通用型)
    /// </summary>
    public class Transform : ITransform, IDisposable
    {
        #region PROTECTED_DATA
        protected Mat _mat;
        protected Mat _matInv;
        protected QVector[] _srcRefs;
        protected QVector[] _dstRefs;
        #endregion

        public virtual void Dispose()
        {
            _mat?.Dispose();
            _mat = null;
            _matInv?.Dispose();
            _matInv = null;
        }

        public QVector[] srcRef
        {
            get => _srcRefs;
        }
        public QVector[] dstRef
        {
            get => _dstRefs;
        }

        protected bool Build(QVector[] srcRefs, QVector[] dstRefs)
        {
            _srcRefs = Array.ConvertAll(srcRefs, v => new QVector(v));
            _dstRefs = Array.ConvertAll(dstRefs, v => new QVector(v));

            System.Diagnostics.Trace.Assert(_srcRefs.Length == _dstRefs.Length && _srcRefs.Length >= 4);
            var ptsSrc = Array.ConvertAll(_srcRefs, v => new Point2f((float)v.X, (float)v.Y));
            var ptsDst = Array.ConvertAll(_dstRefs, v => new Point2f((float)v.X, (float)v.Y));

            _mat?.Dispose();
            _matInv?.Dispose();

            _mat = Cv2.GetPerspectiveTransform(ptsSrc, ptsDst);
            _matInv = new Mat();

            Cv2.Invert(_mat, _matInv);
            
            return true;
        }

        protected QVector[] Trans(params QVector[] src)
        {
            if (_mat == null)
                return src;
            var ptsSrc = Array.ConvertAll(src, v => new Point2d(v.X, v.Y));
            var ptsDst = Cv2.PerspectiveTransform(ptsSrc, _mat);
            return Array.ConvertAll(ptsDst, pt => new QVector(pt.X, pt.Y));
        }
        protected QVector[] InvTrans(params QVector[] src)
        {
            var ptsSrc = Array.ConvertAll(src, v => new Point2d(v.X, v.Y));
            var ptsDst = Cv2.PerspectiveTransform(ptsSrc, _matInv);
            return Array.ConvertAll(ptsDst, pt => new QVector(pt.X, pt.Y));
        }

        QVector ITransform.Trans(QVector coord)
        {
            if (_mat == null || coord == null)
                return coord;
            var pts = Cv2.PerspectiveTransform(new[] { new Point2d(coord.X, coord.Y) }, _mat);
            return new QVector(pts[0].X, pts[0].Y);
        }
        QVector ITransform.InvTrans(QVector coord)
        {
            if (_mat == null || coord == null)
                return coord;
            var pts = Cv2.PerspectiveTransform(new[] { new Point2d(coord.X, coord.Y) }, _matInv);
            return new QVector(pts[0].X, pts[0].Y);
        }
    }


    /// <summary>
    /// 透視投影座標轉換 (指定座標系)
    /// </summary>
    public class Transform<SrcT, DstT> : Transform, ITransform where SrcT : QCoord, new() where DstT : QCoord, new()
    {
        const int N_POINTS = 4;

        public Transform()
        {
            _srcRefs = new SrcT[N_POINTS];
            _dstRefs = new DstT[N_POINTS];
            for (int i = 0; i < N_POINTS; i++)
            {
                int r = i / 2;
                int c = i % 2;
                _srcRefs[i] = new SrcT() { X = c, Y = r };
                _srcRefs[i] = new DstT() { X = c, Y = r };
            }
        }
        public SrcT SrcRef(int i)
        {
            return (SrcT)_srcRefs[i];
        }
        public DstT DstRef(int i)
        {
            return (DstT)_dstRefs[i];
        }

        public bool Build(SrcT[] srcRefs, DstT[] dstRefs)
        {
            // NORMALIZE
            foreach (var srcRef in srcRefs)
                srcRef.Normalize(inplace: true);
            foreach (var dstRef in dstRefs)
                dstRef.Normalize(inplace: true);

            bool ok = base.Build(srcRefs, dstRefs);

            // DeNORMALIZE
            foreach (var srcRef in srcRefs)
                srcRef.DeNormalize(inplace: true);
            foreach (var dstRef in dstRefs)
                dstRef.DeNormalize(inplace: true);

            return ok;
        }
        public DstT Trans(SrcT src)
        {
            var srcN = src.Normalize(inplace: false);
            var dsts = base.Trans(src);
            var v = dsts[0];
            var ret = new DstT() { X = v.X, Y = v.Y };
            ret.DeNormalize();
            return ret;
        }
        public SrcT InvTrans(DstT dst)
        {
            var dstN = dst.Normalize(inplace: false);
            var srcs = base.InvTrans(dstN);
            var v = srcs[0];
            var ret = new SrcT() { X = v.X, Y = v.Y };
            ret.DeNormalize();
            return ret;
        }

        QVector ITransform.Trans(QVector coord)
        {
            if (_mat == null || coord == null)
                return coord;
            var pts = Cv2.PerspectiveTransform(new[] { new Point2d(coord.X, coord.Y) }, _mat);
            return new QVector(pts[0].X, pts[0].Y);
        }
        QVector ITransform.InvTrans(QVector coord)
        {
            if (_mat == null || coord == null)
                return coord;
            var pts = Cv2.PerspectiveTransform(new[] { new Point2d(coord.X, coord.Y) }, _matInv);
            return new QVector(pts[0].X, pts[0].Y);
        }

        public virtual void Load(string iniFileName, string sectName)
        {
            for (int i = 0; i < N_POINTS; i++)
            {
                SrcRef(i)?.Load(iniFileName, sectName, $"SrcRef_{i}");
                DstRef(i)?.Load(iniFileName, sectName, $"DstRef_{i}");
            }
        }
        public virtual void Save(string iniFileName, string sectName)
        {
            for (int i = 0; i < N_POINTS; i++)
            {
                SrcRef(i)?.Save(iniFileName, sectName, $"SrcRef_{i}");
                DstRef(i)?.Save(iniFileName, sectName, $"DstRef_{i}");
            }
        }

        #region PRIVATE_FUNCTIONS
#if (false)
        T[] Normalize<T>(T[] pts, bool inplace) where T : QCoord, new()
        {
            if (inplace)
            {
                foreach (var pt in pts)
                    pt.Normalize(inplace: true);
                return pts;
            }
            else
            {
                int N = pts.Length;
                var ret = new T[N];
                for (int i = 0; i < N; i++)
                {
                    var v = pts[i].Normalize(inplace: false);
                    ret[i] = new T() { X = v.X, Y = v.Y };
                }
                return ret;
            }
        }
        T[] DeNormalize<T>(T[] pts, bool inplace) where T : QCoord, new()
        {
            if (inplace)
            {
                foreach (var pt in pts)
                    pt.DeNormalize(inplace: true);
                return pts;
            }
            else
            {
                int N = pts.Length;
                var ret = new T[N];
                for (int i = 0; i < N; i++)
                {
                    var v = pts[i].DeNormalize(inplace: false);
                    ret[i] = new T() { X = v.X, Y = v.Y };
                }
                return ret;
            }
        }
#endif
        #endregion
    }
}
