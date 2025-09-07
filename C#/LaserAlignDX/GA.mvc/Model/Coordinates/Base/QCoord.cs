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
using System;


namespace LaserAlignDX.Model.Coords
{
    /// <summary>
    /// 座標系 抽象類別
    /// </summary>
    public class QCoord : QVector
    {
        public virtual int ORDER
        {
            get; 
            protected set;
        } = 1;
        public virtual string UNIT
        {
            get; protected set;
        } = "pix";
        public override string ToString()
        {
            string str = $"({X:0.000}, {Y:0.000}) {UNIT}";
            return str;
        }

        public QCoord()
        {
        }
        public QCoord(QCoord c) : base(c)
        {
            this.ORDER = c.ORDER;
            this.UNIT = c.UNIT;
        }
        public QCoord(QVector v) : base(v)
        {
        }
        public QCoord(double x, double y) : base(x, y)
        {
        }
        public QCoord(double x, double y, int order, string unit) : base(x, y)
        {
            this.ORDER = order;
            this.UNIT = unit;
        }

        public QCoord Normalize(bool inplace = false)
        {
            if (inplace)
            {
                if (ORDER > 1)
                {
                    for (int i = 0, N = this.Length; i < N; i++)
                        this[i] /= ORDER;
                }
                return this;
            }
            else
            {
                var v =  this / ORDER;
                return new QCoord(v) { ORDER = this.ORDER, UNIT = this.UNIT };
            }
        }
        public QCoord DeNormalize(bool inplace = false)
        {
            if (inplace)
            {
                for (int i = 0, N = this.Length; i < N; i++)
                {
                    this[i] *= ORDER;
                }
                return this;
            }
            else
            {
                var v = this * ORDER;
                return new QCoord(v) { ORDER = this.ORDER, UNIT = this.UNIT };
            }
        }
        public static QCoord[] Normalize(QCoord[] coords, bool inplace = false)
        {
            if (coords == null || coords.Length == 0)
                return coords;

            if (inplace)
            {
                foreach (var c in coords)
                    c?.Normalize(inplace: true);
                return coords;
            }
            else
            {
                var ret = Array.ConvertAll(coords, c => c?.Normalize(inplace: false));
                return ret;
            }
        }
        public static QCoord[] DeNormalize(QCoord[] coords, bool inplace = false)
        {
            if (coords == null || coords.Length == 0)
                return coords;

            if (inplace)
            {
                foreach (var c in coords)
                    c?.DeNormalize(inplace: true);
                return coords;
            }
            else
            {
                var ret = Array.ConvertAll(coords, c => c?.DeNormalize(inplace: false));
                return ret;
            }
        }

        public virtual void Load(string iniFileName, string sectName, string keyName)
        {
            string str = "";
            JetEazy.Win32.Win32Ini.Load(ref str, iniFileName, sectName, keyName);
            var strs = str.Split(',');

            int i = 0;
            if (strs.Length > i) UNIT = strs[i++].Trim();
            if (strs.Length > i) if (int.TryParse(strs[i++], out int order)) {} //ORDER = order;
            if (strs.Length > i) if (double.TryParse(strs[i++], out double vx)) X = vx;
            if (strs.Length > i) if (double.TryParse(strs[i++], out double vy)) Y = vy;
        }
        public virtual void Save(string iniFileName, string sectName, string keyName)
        {
            string str = $"{UNIT},{ORDER},{X:0.000000},{Y:0.000000}";
            JetEazy.Win32.Win32Ini.Save(str, iniFileName, sectName, keyName);
        }
    }
}
