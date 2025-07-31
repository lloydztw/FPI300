using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using JetEazy.ControlSpace;
using JetEazy.ControlSpace.PLCSpace;

namespace VsCommon.ControlSpace.IOSpace
{

    public enum AlarmsEnum : int
    {
        ALARMSCOUNT = 3,
        ALARMS_ADR_SERIOUS = 0,
        ALARMS_ADR_COMMON = 1,
        ALARMS_ADR_WARNING = 2,
    }

    public abstract class GeoIOClass
    {
        #region INI Access Functions
        [DllImport("kernel32")]
        protected static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        protected static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        protected static void WriteINIValue(string section, string key, string value, string filepath)
        {
            WritePrivateProfileString(section, key, value, filepath);
        }
        protected static string ReadINIValue(string section, string key, string defaultvaluestring, string filepath)
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

        protected string INIFILE = "";

        protected AddressClass[] ADDRESSARRAY;
        protected VsCommPLC[] PLC;
        //protected OptionEnum OPTION;

        protected AddressClass[] ADDRESSARRAY_INPUT;
        protected AddressClass[] ADDRESSARRAY_OUTPUT;
        public PLCAlarmsClass[] PLCALARMS;

        public GeoIOClass()
        {
        }
        
        public AddressClass GetGaAddress(string in_out, int index)
        {
            //@LETIAN: for integration
            if (in_out == "INPUT")
                return ADDRESSARRAY_INPUT[index];
            else if (in_out == "OUTPUT")
                return ADDRESSARRAY_OUTPUT[index];
            else
                return null;
        }

        public abstract void LoadData();
        public abstract void SaveData();
        
        protected string ValueToHEX(long Value, int Length)
        {
            return ("00000000" + Value.ToString("X")).Substring(("00000000" + Value.ToString("X")).Length - Length, Length);
        }
        protected void ReadData(ref string DataStr, string FileName)
        {
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None);
            StreamReader Srr = new StreamReader(fs, Encoding.Default);

            DataStr = Srr.ReadToEnd();

            Srr.Close();
            Srr.Dispose();
        }
        //當有Input Trigger時，產生OnTrigger
        public delegate void TriggerHandler(string eventstring);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(String eventstring)
        {
            if (TriggerAction != null)
            {
                TriggerAction(eventstring);
            }
        }
        protected Int32 HEXSigned32(string HexStr)
        {
            return System.Convert.ToInt32(HexStr, 16);
        }

    }
}
