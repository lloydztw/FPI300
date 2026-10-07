using JetEazy.CCDSpace;
using JetEazy.ControlSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TravellerMINIX6.OPSpace
{
    public class CameraConfig
    {

        #region INI Access Functions
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        void WriteINIValue(string section, string key, string value, string filepath)
        {
            WritePrivateProfileString(section, key, value, filepath);
        }
        string ReadINIValue(string section, string key, string defaultvaluestring, string filepath)
        {
            string retStr = "";

            StringBuilder temp = new StringBuilder(200);
            int Length = GetPrivateProfileString(section, key, "", temp, 200, filepath);

            retStr = temp.ToString();

            if (retStr == "")
                retStr = defaultvaluestring;
            //else
            //    retStr = retStr.Split('/')[0]; //把說明排除掉

            return retStr;

        }
        #endregion

        string INIFILE = "CAMERA.INI";


        private static readonly CameraConfig _instance = new CameraConfig();
        public static CameraConfig Instance
        {
            get
            {
                return _instance;
            }
        }

        public int COUNT = 2;

        public CameraPara[] cameras;

        public void Initial(string epath)
        {
            INIFILE = epath + "\\CAMERA.ini";
            Load();
        }

        public void Load()
        {
            COUNT = int.Parse(ReadINIValue("Camera Basic", "COUNT", COUNT.ToString(), INIFILE));

            cameras = new CameraPara[COUNT];
            int i = 0;
            while (i < COUNT)
            {
                cameras[i] = new CameraPara();
                cameras[i].Index = int.Parse(ReadINIValue("Camera " + i.ToString(), "Index", cameras[i].Index.ToString(), INIFILE));
                cameras[i].SerialNumber = ReadINIValue("Camera " + i.ToString(), "SerialNumber", cameras[i].SerialNumber.ToString(), INIFILE);
                cameras[i].IsDebug = ReadINIValue("Camera " + i.ToString(), "IsDebug", cameras[i].IsDebug.ToString(), INIFILE) == "1";
                cameras[i].Rotate = int.Parse(ReadINIValue("Camera " + i.ToString(), "Rotate", cameras[i].Rotate.ToString(), INIFILE));
                cameras[i].CfgPath = INIFILE.Replace("\\CAMERA.ini", "");
                cameras[i].CfgPath = ReadINIValue("Camera " + i.ToString(), "CfgPath", cameras[i].CfgPath.ToString(), INIFILE);
                cameras[i].CameraType = ReadINIValue("Camera " + i.ToString(), "CameraType", cameras[i].CameraType.ToString(), INIFILE);
                cameras[i].IsFly = ReadINIValue("Camera " + i.ToString(), "IsFly", cameras[i].IsDebug.ToString(), INIFILE) == "1";

                i++;
            }
        }
        public void Save()
        {

            WriteINIValue("Camera Basic", "COUNT", COUNT.ToString(), INIFILE);

            //cameras = new CameraPara[COUNT];
            int i = 0;
            while (i < COUNT)
            {
                WriteINIValue("Camera " + i.ToString(), "Index", cameras[i].Index.ToString(), INIFILE);
                WriteINIValue("Camera " + i.ToString(), "SerialNumber", cameras[i].SerialNumber.ToString(), INIFILE);
                WriteINIValue("Camera " + i.ToString(), "IsDebug", (cameras[i].IsDebug ? "1" : "0"), INIFILE);
                WriteINIValue("Camera " + i.ToString(), "Rotate", cameras[i].Rotate.ToString(), INIFILE);
                WriteINIValue("Camera " + i.ToString(), "CfgPath", cameras[i].CfgPath.ToString(), INIFILE);
                WriteINIValue("Camera " + i.ToString(), "CameraType", cameras[i].CameraType.ToString(), INIFILE);
                WriteINIValue("Camera " + i.ToString(), "IsFly", (cameras[i].IsFly ? "1" : "0"), INIFILE);

                i++;
            }

        }
    }
}
