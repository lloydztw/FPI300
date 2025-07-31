using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using JetEazy.BasicSpace;
using JetEazy.Interface;

namespace JetEazy.ControlSpace.MotionSpace
{

    public enum MotionTypeEnum
    {
        AXIS,
        ROTATION,
    }

    public enum MotionEnum : int
    {
        COUNT = 16,

        M0 = 0,
        M1 = 1,
        M2 = 2,

        M3 = 3,
        M4 = 4,
        M5 = 5,

        M6 = 6,
        M7 = 7,
        M8 = 8,

        M9 = 9,
        M10 = 10,
        M11 = 11,
        M12 = 12,
        M13 = 13,
        M14 = 14,
        M15 = 15,


    }

    public enum SpeedTypeEnum : int
    {
        HOMESLOW,
        HOMEHIGH,

        MANUALSLOW,
        MANUAL,

        GOSLOW,
        GO,
    }

    public abstract class GeoMotionClass : IAxis
    {
        #region INI Access Functions
        [DllImport("kernel32")]
        protected static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]

        //private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal,
        //    int size, string filePath);

        protected static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal,
            int size, string filePath);

        protected static void WriteINIValue(string section, string key, string value, string filepath)
        {
            WritePrivateProfileString(section, key, value, filepath);
        }
        protected static string ReadINIValue(string section, string key, string defaultvaluestring, string filepath)
        {
            string retStr = "";

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

        public bool IsInitialOK = false;
        public bool IsNoUseMotor = false;
        /// <summary>
        /// 马达数据转换模式 0：int模式 1：float模式读取设定位置使用
        /// </summary>
        public int MotionMMMode = 0;

        /// <summary>
        /// 模擬位置
        /// </summary>
        public int SIMPositionStep = 0;
        /// <summary>
        /// 模擬速度
        /// </summary>
        public int SIMSpeed = 0;

        public abstract bool IsHaveBreakOption { get; }
        public abstract bool IsHaveSVOnOption { get; }
        public abstract bool IsHaveResetOption { get; }
        public abstract bool IsHaveRulerOption { get; }

        public abstract bool IsHome { get; set; }
        public abstract bool IsOnSite { get;}
        public abstract bool IsSVOn { get;}
        public abstract bool IsBreack { get;}
        public abstract bool IsError { get; }
        public abstract bool IsOK { get; }
        //public bool IsOK
        //{
        //    get
        //    {
        //        bool ret = IsHome;

        //        ret &= IsOnSite;

        //        if (ret)
        //        {
        //            if (!IsInitialOK)
        //            {
        //                IsInitialOK = true;
        //            }
        //        }

        //        return ret;
        //    }
        //}
        public abstract string PositionNowString { get;}
        public abstract float PositionNow { get; }
        public abstract string RulerPositionNowString { get; }
        public abstract float RulerPositionNow { get; }

        protected int ONEMMSTEP = 625;
        protected int RULERONEMMSTEP = 625;

        public double MANUALSPEED = 0;
        public double MANUALSLOWSPEED = 0;

        public double GOSPEED = 0;
        public double GOSLOWSPEED = 0;

        public double HOMEHIGHSPEED = 0;
        public double HOMESLOWSPEED = 0;

        protected int RATIO = 50;
        public abstract int StepPositionNow { get; }
        public abstract int StepPositionSet { set; }
        public abstract int RulerStepPositionNow { get; }

        //public abstract float StepPositionNowfloat { get; }
        public abstract float StepPositionSetfloat { set; }
        //public abstract float RulerStepPositionNowfloat { get; }

        public float TESTPOSITION = 0f;
        public float READYPOSITION = 0f;
        public float SOFTUPPERBOUND = 0f;
        public float SOFTLOWERBOUND = 0f;

        public bool IsReachLimit
        {
            get
            {
                return IsReachUpperBound || IsReachLowerBound;
            }
        }
        public abstract bool IsReachHomeBound { get; }
        public abstract bool IsReachUpperBound { get; }
        public abstract bool IsReachLowerBound { get; }
        public MotionEnum MOTIONNAME = MotionEnum.M0;
        public string MOTIONALIAS = "M0";
        public string MOTIONUNIT = "mm";

        protected MotionTypeEnum MOTIONTYPE = MotionTypeEnum.AXIS;

        protected JzTimes myTime = new JzTimes();
        public GeoMotionClass()
        {

        }
        public abstract void Go(float position);
        public abstract void Home();
        public abstract void SVOn();
        public abstract void Reset();
        public abstract void Break();
        public abstract void Forward();
        public abstract void Backward();
        public abstract void Stop();
        public abstract void LoadData();
        public abstract void SaveData();

        public abstract void SetSpeed(SpeedTypeEnum speedtype);
        //public abstract void SetSpeedSoft(SpeedTypeEnum speedtype);
        public abstract double GetSpeed(SpeedTypeEnum speedtype);
        //public abstract long GetSpeedSoft(SpeedTypeEnum speedtype);

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

        protected UInt32 HEX32(string HexStr)
        {
            return System.Convert.ToUInt32(HexStr, 16);
        }
        protected Int32 HEXSigned32(string HexStr)
        {
            return System.Convert.ToInt32(HexStr, 16);
        }

        protected UInt16 HEX16(string HexStr)
        {
            return System.Convert.ToUInt16(HexStr, 16);
        }


        public abstract void Go(double frompos, double offset);
        public abstract void SetManualSpeed(int val);
        public abstract void SetActionSpeed(int val);
        public abstract double GetPos();
        public abstract string GetStatus();
        public abstract double GetInitPosition();
        public abstract void Go(int posindex, float position);
        public abstract bool IsOnSitePos(int posindex);
        public abstract void SetPos(int posindex, float position);
        public abstract void GoPos(int posindex);
        public abstract float GetSetPos(int posindex);
    }
}
