#region AUTHOR
/*
 * EzIO.Mem
 * Copyright (C) 2023
 * 2013-07-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Runtime.InteropServices;
using System.Text;


namespace EzIO.Mem.Utils
{
    public class Win32Ini
    {
        #region INI Access Functions
        [DllImport("kernel32")]
        protected static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        protected static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public static void Write(string section, string key, string value, string iniFileName)
        {
            WritePrivateProfileString(section, key, value, iniFileName);
        }
        public static string Read(string section, string key, string defaultvaluestring, string iniFileName)
        {
            int N = 512;    // 100
            var sb = new StringBuilder(N);
            var n = GetPrivateProfileString(section, key, "", sb, N, iniFileName);

            string retStr = sb.ToString().Trim();

            if (string.IsNullOrEmpty(retStr))
                retStr = defaultvaluestring;
            else
                retStr = retStr.Split('/')[0]; //把說明排除掉

            return retStr;
        }
        #endregion
    }
}
