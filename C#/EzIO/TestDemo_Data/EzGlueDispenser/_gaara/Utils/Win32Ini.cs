using System.Runtime.InteropServices;
using System.Text;

namespace VictoryGaara
{
    public class Win32Ini
    {
        #region INI Access Functions
        [DllImport("kernel32")]
        protected static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        protected static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public static void WriteINIValue(string section, string key, string value, string filepath)
        {
            WritePrivateProfileString(section, key, value, filepath);
        }
        public static string ReadINIValue(string section, string key, string defaultvaluestring, string filepath)
        {
            string retStr = "";
            //StringBuilder temp = new StringBuilder(100);
            StringBuilder temp = new StringBuilder(512);
            int Length = GetPrivateProfileString(section, key, "", temp, 512, filepath);

            retStr = temp.ToString();

            if (retStr == "")
                retStr = defaultvaluestring;
            else
                retStr = retStr.Split('/')[0]; //把說明排除掉

            return retStr;

        }
        #endregion
    }
}
