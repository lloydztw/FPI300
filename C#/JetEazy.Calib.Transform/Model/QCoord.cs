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


namespace JetEazy.Transform
{
    /// <summary>
    /// 座標
    /// </summary>
    internal class QCoord : QVector
    {
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
            this.UNIT = c.UNIT;
        }
        public QCoord(QVector v) : base(v)
        {
        }
        public QCoord(double x, double y) : base(x, y)
        {
        }
        public QCoord(double x, double y, string unit) : base(x, y)
        {
            this.UNIT = unit;
        }

#if(OPT_RESERVED)
        public QCoord Normalize(bool inplace = false)
        {
            

            if (inplace)
            {
                if (ORDER > 1)
                {
                    for (int i = 0, N = this.Length; i < N; i++)
                    {
                        this[i] /= ORDER;
                    }
                }
                return this;
            }
            else
            {
                //var v =  this / ORDER;
                //return new QCoord(v) { ORDER = this.ORDER, UNIT = this.UNIT };
                int order = Math.Max(this.ORDER, 1);
                return new QCoord()
                {
                    X = this.X / order,
                    Y = this.Y / order,
                    ORDER = this.ORDER,
                    UNIT = this.UNIT,
                };
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
                //var v = this * ORDER;
                //return new QCoord(v) { ORDER = this.ORDER, UNIT = this.UNIT };
                int order = Math.Max(this.ORDER, 1);
                return new QCoord()
                {
                    X = this.X * order,
                    Y = this.Y * order,
                    ORDER = this.ORDER,
                    UNIT = this.UNIT,
                };
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
#endif

        public virtual void Load(string iniFileName, string sectName, string keyName)
        {
            string str = "";
            JetEazy.Win32.Win32Ini.Load(ref str, iniFileName, sectName, keyName);
            var strs = str.Split(',');

            int i = 0;
            if (strs.Length > i) UNIT = strs[i++].Trim();
            if (strs.Length > i) if (int.TryParse(strs[i++].Trim(), out int len)) {}
            if (strs.Length > i) if (double.TryParse(strs[i++].Trim(), out double vx)) X = vx;
            if (strs.Length > i) if (double.TryParse(strs[i++].Trim(), out double vy)) Y = vy;
        }
        public virtual void Save(string iniFileName, string sectName, string keyName)
        {
            string str = $"{UNIT}, {Length}, {X:0.000000}, {Y:0.000000}";
            JetEazy.Win32.Win32Ini.Save(str, iniFileName, sectName, keyName);
        }
    }
}
