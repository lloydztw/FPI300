#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-11 重新整理 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace JetEazy.Utils
{
    public class WinIni
    {
        public static int DIGITS = 6;

        #region WIN32_API
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        #endregion

        public static void WriteINIValue(string section, string key, string value, string iniFile)
        {
            WritePrivateProfileString(section, key, value, iniFile);
        }
        public static string ReadINIValue(string section, string key, string defaultValue, string iniFile)
        {
            string retStr = "";

            StringBuilder temp = new StringBuilder(4096);
            int Length = GetPrivateProfileString(section, key, "", temp, 4096, iniFile);

            retStr = temp.ToString();

            if (retStr == "")
                retStr = defaultValue;
            //else
            //    retStr = retStr.Split('/')[0]; //把說明排除掉

            return retStr;
        }

        public static void Write(string iniFile, string section, string key, string value)
        {
            WriteINIValue(section, key, value, iniFile);
        }
        public static void Read(string iniFile, string section, string key, string defaultValue, out string value)
        {
            value = ReadINIValue(section, key, defaultValue, iniFile);
        }

        public static void Write(string iniFile, string section, string key, bool value)
        {
            WriteINIValue(section, key, value ? "1" : "0", iniFile);
        }
        public static void Read(string iniFile, string section, string key, bool defaultValue, out bool value)
        {
            var str = ReadINIValue(section, key, defaultValue ? "1" : "0", iniFile);
            value = str == "1";
        }

        public static void Write(string iniFile, string section, string key, int value)
        {
            WriteINIValue(section, key, value.ToString(), iniFile);
        }
        public static void Read(string iniFile, string section, string key, int defaultValue, out int value)
        {
            var str = ReadINIValue(section, key, defaultValue.ToString(), iniFile);
            if (!int.TryParse(str, out value))
                value = defaultValue;
        }

        public static void Write(string iniFile, string section, string key, double value)
        {
            value = Math.Round(value, DIGITS);
            WriteINIValue(section, key, value.ToString(), iniFile);
        }
        public static void Read(string iniFile, string section, string key, double defaultValue, out double value)
        {
            var str = ReadINIValue(section, key, defaultValue.ToString(), iniFile);
            if (!double.TryParse(str, out value))
                value = defaultValue;
            value = Math.Round(value, DIGITS);
        }

        public static void Write(string iniFile, string section, string key, float value)
        {
            Write(iniFile, section, key, (double)value);
        }
        public static void Read(string iniFile, string section, string key, float defaultValue, out float value)
        {
            Read(iniFile, section, key, (double)defaultValue, out double v);
            value = (float)v;
        }

        public static void Write(string iniFile, string section, string key, params int[] values)
        {
            string[] strs = Array.ConvertAll(values, v => v.ToString());
            string str = string.Join(",", strs);
            WriteINIValue(section, key, str, iniFile);
        }
        public static bool Read(string iniFile, string section, string key, out int[] values)
        {
            var str = ReadINIValue(section, key, "", iniFile);
            string[] strs = str.Split(',');
            values = new int[strs.Length];

            bool ok = true;
            for (int i = 0, len = strs.Length; i < len; i++)
                ok &= int.TryParse(strs[i].Trim(), out values[i]);

            return ok;
        }
        public static void Write(string iniFile, string section, string key, params float[] values)
        {
            string[] strs = Array.ConvertAll(values, v => Math.Round(v, DIGITS).ToString());
            string str = string.Join(",", strs);
            WriteINIValue(section, key, str, iniFile);
        }
        public static bool Read(string iniFile, string section, string key, out float[] values)
        {
            var str = ReadINIValue(section, key, "", iniFile);
            string[] strs = str.Split(',');
            values = new float[strs.Length];

            bool ok = true;
            for (int i = 0, len = strs.Length; i < len; i++)
                ok &= float.TryParse(strs[i].Trim(), out values[i]);

            return ok;
        }

        public static void Write(string iniFile, string section, string key, PointF value)
        {
            Write(iniFile, section, key, value.X, value.Y);
        }
        public static void Read(string iniFile, string section, string key, PointF defaultValue, out PointF value)
        {
            bool ok = Read(iniFile, section, key, out float[] values);
            if (ok && values.Length >= 2)
            {
                value = new PointF(values[0], values[1]);
            }
            else
            {
                value = defaultValue;
            }
        }
        public static void Write(string iniFile, string section, string key, Point value)
        {
            Write(iniFile, section, key, value.X, value.Y);
        }
        public static void Read(string iniFile, string section, string key, Point defaultValue, out Point value)
        {
            bool ok = Read(iniFile, section, key, out int[] values);
            if (ok && values.Length >= 2)
            {
                value = new Point(values[0], values[1]);
            }
            else
            {
                value = defaultValue;
            }
        }

        public static void Write(string iniFile, string section, string key, RectangleF value)
        {
            Write(iniFile, section, key, value.X, value.Y, value.Width, value.Height);
        }
        public static void Read(string iniFile, string section, string key, RectangleF defaultValue, out RectangleF value)
        {
            bool ok = Read(iniFile, section, key, out float[] values);
            if (ok && values.Length >= 4)
            {
                value = new RectangleF(values[0], values[1], values[2], values[3]);
            }
            else
            {
                value = defaultValue;
            }
        }
        public static void Write(string iniFile, string section, string key, Rectangle value)
        {
            Write(iniFile, section, key, value.X, value.Y, value.Width, value.Height);
        }
        public static void Read(string iniFile, string section, string key, Rectangle defaultValue, out Rectangle value)
        {
            bool ok = Read(iniFile, section, key, out int[] values);
            if (ok && values.Length >= 4)
            {
                value = new Rectangle(values[0], values[1], values[2], values[3]);
            }
            else
            {
                value = defaultValue;
            }
        }
    }
}
