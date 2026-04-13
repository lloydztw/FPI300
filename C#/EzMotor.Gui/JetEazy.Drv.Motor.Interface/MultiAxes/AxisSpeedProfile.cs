#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2023-09-11 LeTian Chang, Revision.
 *      2008-12-01 LeTian Chang, Creation.
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;


namespace JetEazy.Drivers.Motor
{
    public class AxisSpeedSettings
    {
        const double _SKIP = 0;

        #region CONSTS
            protected enum Fields : int
            {
                Vstart,
                Vmax,
                Tacc,
                Tdec,
                SVacc,
                SVdec,
                END
            };
        #endregion

        #region PRIVATE_DATA
            private double[] m_data = new double[(int)Fields.END];
            protected double this[Fields id]
            {
                get { return m_data[(int)id]; }
                set { m_data[(int)id] = value; }
            }
        #endregion

#if(OPT_ORG)
        public double Vstart = SKIP;
        public double Vmax = SKIP;
        public double Tacc = SKIP;
        public double Tdec = SKIP;
        public double SVacc = SKIP;
        public double SVdec = SKIP;
#endif

        public double MaxRPM
        {
            get;
            set;
        }
        public double Vstart
        {
            get
            {
                return m_data[(int)Fields.Vstart];
            }
            set
            {
                if (!IsSkip(value))
                    m_data[(int)Fields.Vstart] = value;
            }
        }
        public double Vmax
        {
            get
            {
                return m_data[(int)Fields.Vmax];
            }
            set
            {
                if (!IsSkip(value))
                    m_data[(int)Fields.Vmax] = value;
            }
        }
        public double Tacc
        {
            get
            {
                return m_data[(int)Fields.Tacc];
            }
            set
            {
                if (!IsSkip(value))
                    m_data[(int)Fields.Tacc] = value;
            }
        }
        public double Tdec
        {
            get
            {
                return m_data[(int)Fields.Tdec];
            }
            set
            {
                if (!IsSkip(value))
                    m_data[(int)Fields.Tdec] = value;
            }
        }
        public double SVacc
        {
            get
            {
                return _getAutoSV(Vstart, Vmax);
                return m_data[(int)Fields.SVacc];
            }
            set
            {
                if (!IsSkip(value))
                    m_data[(int)Fields.SVacc] = value;
            }
        }
        public double SVdec
        {
            get
            {
                return _getAutoSV(Vstart, Vmax);
                return m_data[(int)Fields.SVdec];
            }
            set
            {
                if (!IsSkip(value))
                    m_data[(int)Fields.SVdec] = value;
            }
        }
        public object Ext
        {
            get;
            set;
        }

        public bool IsAllSkip()
        {
            for (int i = 0; i < (int)Fields.END; i++)
            {
                if (!IsSkip(m_data[i]))
                    return false;
            }
            return true;
        }
        public static bool IsSkip(double v)
        {
            return (v <= _SKIP);
        }

        public void AutoSCurve(bool bForce = false)
        {
            if (bForce || IsSkip(SVacc))
                SVacc = _getAutoSV(Vstart, Vmax);

            if (bForce || IsSkip(SVdec))
                SVacc = _getAutoSV(Vstart, Vmax);
        }
        public void MakeSafeSCurve()
        {
            double sv = _getMaxSV();

            if (SVacc > sv)
                SVacc = sv;

            if (SVacc > sv)
                SVacc = sv;
        }

        public AxisSpeedSettings Clone()
        {
            //> AxisSpeedSettings obj = (AxisSpeedSettings)this.MemberwiseClone();
            var obj = new AxisSpeedSettings();
            Array.Copy(m_data, obj.m_data, (int)Fields.END);
            obj.Ext = this.Ext;
            return obj;
        }
        public void SmartCopyFrom(AxisSpeedSettings src)
        {
#if(OPT_ORG)
            if (src.Vstart >= 0) Vstart = src.Vstart;
            if (src.Vmax >= 0) Vmax = src.Vmax;
            if (src.Tacc >= 0) Tacc = src.Tacc;
            if (src.Tdec >= 0) Vstart = src.Tdec;
            if (src.SVacc >= 0) Tacc = src.SVacc;
            if (src.SVdec >= 0) Vstart = src.SVdec;
            if (src.Ext != null) Ext = src.Ext;
#endif

            for (int i = 0; i < (int)Fields.END; i++)
            {
                if (!IsSkip(src.m_data[i]))
                    this.m_data[i] = src.m_data[i];
            }

            if (null != src.Ext)
                this.Ext = src.Ext;
        }
        public static void SmartSwap(AxisSpeedSettings v, AxisSpeedSettings vRef)
        {
            if (v != null && vRef != null && v != vRef)
            {
                //_smartSwap(ref v.m_Vstart, ref vRef.m_Vstart);
                //_smartSwap(ref v.m_Vmax, ref vRef.m_Vmax);
                //_smartSwap(ref v.m_Tacc, ref vRef.m_Tacc);
                //_smartSwap(ref v.m_Tdec, ref vRef.m_Tdec);

                for (int i = 0; i < (int)Fields.END; i++)
                    _smartSwap(ref v.m_data[i], ref vRef.m_data[i]);
            }
        }
        public static double GetAutoSV(double Vstart, double Vmax)
        {
            return _getAutoSV(Vstart, Vmax);
        }

        public void Load(string strIniFileName, string strAppName)
        {
#if(OPT_ORG)
            string str = null;
            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "Vstart");
            double.TryParse(str, out m_Vstart);

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "Vmax");
            double.TryParse(str, out m_Vmax);

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "Tacc");
            double.TryParse(str, out m_Tacc);

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "Tdec");
            double.TryParse(str, out m_Tdec);

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "SVacc");
            double.TryParse(str, out m_SVacc);

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "SVdec");
            double.TryParse(str, out m_SVdec);
#endif

            for (int i = 0; i < (int)Fields.END; i++)
            {
                string key = ((Fields)i).ToString();
                string s = null;
                JetEazy.Win32.Win32Ini.Load(ref s, strIniFileName, strAppName, key);
                double.TryParse(s, out m_data[i]);
            }
        }
        public void Save(string strIniFileName, string strAppName)
        {
#if(OPT_ORG)
            JetEazy.Win32.Win32Ini.Save(m_Vstart, strIniFileName, strAppName, "Vstart");
            JetEazy.Win32.Win32Ini.Save(m_Vmax, strIniFileName, strAppName, "Vmax");
            JetEazy.Win32.Win32Ini.Save(m_Tacc, strIniFileName, strAppName, "Tacc");
            JetEazy.Win32.Win32Ini.Save(m_Tdec, strIniFileName, strAppName, "Tdec");
            JetEazy.Win32.Win32Ini.Save(m_SVacc, strIniFileName, strAppName, "SVacc");
            JetEazy.Win32.Win32Ini.Save(m_SVdec, strIniFileName, strAppName, "SVdec");
#endif

            for (int i = 0; i < (int)Fields.END; i++)
            {
                string key = ((Fields)i).ToString();
                JetEazy.Win32.Win32Ini.Save(m_data[i], strIniFileName, strAppName, key);
            }
        }

        #region PRIVATE_FUNCTIONS
            static void _smartSwap(ref double v, ref double vRef)
            {
                if (IsSkip(v))
                    v = vRef;
                else
                    vRef = v;
            }
            static double _getAutoSV(double Vstart, double Vmax)
            {
                double sv = (Math.Abs(Vmax) - Math.Abs(Vstart)) / 3;
                if (sv < 0) sv = 0;
                return sv;
            }
            double _getMaxSV()
            {
                double sv = (Math.Abs(Vmax) - Math.Abs(Vstart)) / 2;
                if (sv < 0) sv = 0;
                return sv;
            }
        #endregion
    }
}
