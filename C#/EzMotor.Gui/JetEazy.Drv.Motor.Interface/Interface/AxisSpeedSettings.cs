using System;

namespace JetEazy.Drivers.Motor
{
    public class AxisSpeedSettings
    {
        public double Vstart = -1;
        public double Vmax = -1;
        public double Tacc = -1;
        public double Tdec = -1;
        public object Ext = null;

        public AxisSpeedSettings Clone()
        {
            AxisSpeedSettings obj = (AxisSpeedSettings)this.MemberwiseClone();
            return obj;
        }
        public void SmartCopyFrom(AxisSpeedSettings src)
        {
            if (src.Vstart >= 0) Vstart = src.Vstart;
            if (src.Vmax >= 0) Vmax = src.Vmax;
            if (src.Tacc >= 0) Tacc = src.Tacc;
            if (src.Tdec >= 0) Vstart = src.Tdec;
            if (src.Ext != null) Ext = src.Ext;
        }

        public void Load(string strIniFileName, string strAppName)
        {
            string str = null;
            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "Vstart");
            double.TryParse(str, out Vstart);

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "Vmax");
            double.TryParse(str, out Vmax);

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "Tacc");
            double.TryParse(str, out Tacc);

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "Tdec");
            double.TryParse(str, out Tdec);
        }
        public void Save(string strIniFileName, string strAppName)
        {
            JetEazy.Win32.Win32Ini.Save(Vstart, strIniFileName, strAppName, "Vstart");
            JetEazy.Win32.Win32Ini.Save(Vmax, strIniFileName, strAppName, "Vmax");
            JetEazy.Win32.Win32Ini.Save(Tacc, strIniFileName, strAppName, "Tacc");
            JetEazy.Win32.Win32Ini.Save(Tdec, strIniFileName, strAppName, "Tdec");
        }
    }
}
