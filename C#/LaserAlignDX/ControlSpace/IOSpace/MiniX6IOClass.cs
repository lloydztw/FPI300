
//#define HC_Q1_1300D
//#define FATEK
//#define FX3U

using JetEazy.ControlSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


using VsCommon.ControlSpace.IOSpace;

namespace VsCommon.ControlSpace.IOSpace
{
    public enum ADRMiniX6 : int
    {
        COUNT = 8,

        ADR_ISEMC = 0,
        ADR_ISRESET = 1,
        ADR_ISSTART = 2,
        ADR_ISSTOP = 3,

        ADR_YELLOW = 4,
        ADR_GREEN = 5,
        ADR_RED = 6,
        ADR_BUZZER = 7,
    }

#if HC_Q1_1300D
    public class NeedleIOClass : GeoIOClass
    {

        public NeedleIOClass()
        {   

        }
        public void Initial(string path, JetEazy.ControlSpace.PLCSpace.VsCommPLC[] plc)
        {
            PLC = plc;

            INIFILE = path + "\\IO.INI";

            LoadData();

        }
        public override void LoadData()
        {
           
        }

        public override void SaveData()
        {
            
        }

        public bool ADR_ISEMC
        {
            get { return !GetQXQB("0:IX0.3"); }
        }
        public bool ADR_ISAUTO_AND_MANUAL
        {
            get { return GetQXQB("0:IX0.4"); }
        }
        public bool ADR_ISSTART
        {
            get { return GetQXQB("0:IX0.0"); }
        }
        public bool ADR_ISSTOP
        {
            get { return GetQXQB("0:IX0.1"); }
        }
        public bool ADR_ISVACC
        {
            get { return GetQXQB("0:IX0.2"); }
        }

        public bool ADR_RED
        {
            get
            {
                return GetQXQB("0:QX0.0");
            }
            set
            {
                SetQXQB("0:QX0.0", value);
            }
        }
        public bool ADR_GREEN
        {
            get
            {
                return GetQXQB("0:QX0.1");
            }
            set
            {
                SetQXQB("0:QX0.1", value);
            }
        }
        public bool ADR_YELLOW
        {
            get
            {
                return GetQXQB("0:QX0.2");
            }
            set
            {
                SetQXQB("0:QX0.2", value);
            }
        }
        public bool ADR_BRAKE
        {
            get
            {
                return GetQXQB("0:QX0.3");
            }
            set
            {
                SetQXQB("0:QX0.3", value);
            }
        }


        public bool GetQXQB(string addStr)
        {
            AddressClass address = new AddressClass(addStr);
            return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        }
        public void SetQXQB(int index, bool ison)
        {
            string addr = "0:QB" + index.ToString() + ".0";
            SetQXQB(addr, ison);
        }
        public void SetQXQB(string addStr, bool ison)
        {
            AddressClass address = new AddressClass(addStr);
            PLC[address.SiteNo].SetIO(ison, address.Address0);
        }
        public short GetMW(string addStr)
        {
            AddressClass address = new AddressClass(addStr);
            return PLC[address.SiteNo].IOData.GetMW(address.Address0);
        }

        public void SetMW(string addStr, float value)
        {
            AddressClass address = new AddressClass(addStr);
            PLC[address.SiteNo].SetData(value, address);
        }
        public void SetMW(string addStr, int value)
        {
            AddressClass address = new AddressClass(addStr);
            PLC[address.SiteNo].SetData(value, address);
        }

    }
#endif

#if FATEK
    public class MiniX6IOClass : GeoIOClass
    {

        public MiniX6IOClass()
        {

        }
        public void Initial(string path, JetEazy.ControlSpace.PLCSpace.VsCommPLC[] plc)
        {

            ADDRESSARRAY = new AddressClass[(int)ADRMiniX6.COUNT];
            PLCALARMS = new PLCAlarmsClass[(int)AlarmsEnum.ALARMSCOUNT];

            PLC = plc;

            INIFILE = path + "\\IO.INI";

            LoadData();

        }
        public override void LoadData()
        {

            ADDRESSARRAY[(int)ADRMiniX6.ADR_ISEMC] = new AddressClass(ReadINIValue("Status Address", "ADR_ISEMC", "", INIFILE));
            ADDRESSARRAY[(int)ADRMiniX6.ADR_ISRESET] = new AddressClass(ReadINIValue("Status Address", "ADR_ISRESET", "", INIFILE));
            ADDRESSARRAY[(int)ADRMiniX6.ADR_ISSTART] = new AddressClass(ReadINIValue("Status Address", "ADR_ISSTART", "", INIFILE));
            ADDRESSARRAY[(int)ADRMiniX6.ADR_ISSTOP] = new AddressClass(ReadINIValue("Status Address", "ADR_ISSTOP", "", INIFILE));


            ADDRESSARRAY[(int)ADRMiniX6.ADR_RED] = new AddressClass(ReadINIValue("Operation Address", "ADR_RED", "", INIFILE));
            ADDRESSARRAY[(int)ADRMiniX6.ADR_GREEN] = new AddressClass(ReadINIValue("Operation Address", "ADR_GREEN", "", INIFILE));
            ADDRESSARRAY[(int)ADRMiniX6.ADR_YELLOW] = new AddressClass(ReadINIValue("Operation Address", "ADR_YELLOW", "", INIFILE));
            ADDRESSARRAY[(int)ADRMiniX6.ADR_BUZZER] = new AddressClass(ReadINIValue("Operation Address", "ADR_BUZZER", "", INIFILE));


            #region 读取csv- ALARM

            string alarm0_path = INIFILE.Replace("IO.INI", "ALARMIO0.csv");
            System.IO.StreamReader _sr = null;
            try
            {
                _sr = new System.IO.StreamReader(alarm0_path);
                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON] = new PLCAlarmsClass("D00502:M1168");
                //PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON] = new PLCAlarmsClass("MW0000:MX0.0,MW0001:MX2.0,MW0002:MX4.0,MW0003:MX6.0,MW0004:MX8.0,MW0005:MX10.0,MW0006:MX12.0,MW0007:MX14.0");

                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS] = new PLCAlarmsClass("D00500:M1152");
                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_WARNING] = new PLCAlarmsClass("D00502:M1168");
                string strRead = string.Empty;
                while (!_sr.EndOfStream)
                {
                    strRead = _sr.ReadLine();
                    string[] strs = strRead.Split(',').ToArray();
                    if (strs.Length >= 4)
                    {
                        switch (strs[0])
                        {
                            case "COMMON":
                                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON].PLCAlarmsAddDescription("0," + strs[2] + "," + strs[3]);
                                break;
                            case "SERIOUS":
                                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS].PLCAlarmsAddDescription("0," + strs[2] + "," + strs[3]);
                                break;
                            case "WARNING":
                                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_WARNING].PLCAlarmsAddDescription("0," + strs[2] + "," + strs[3]);
                                break;
                        }
                    }
                }

                _sr.Close();
                _sr.Dispose();
                _sr = null;
            }
            catch
            {

            }

            if (_sr != null)
                _sr.Dispose();

            #endregion

        }

        public override void SaveData()
        {

        }

        public bool IsAlarmsSerious
        {
            get
            {
                AddressClass address = new AddressClass("0:M1060");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }
        
        public bool IsAlarmsCommon
        {
            get
            {
                AddressClass address = new AddressClass("0:M1065");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }
        //public bool IsAlarmsWarning
        //{
        //    get
        //    {
        //        AddressClass address = new AddressClass("0:D00420");
        //        return PLC[address.SiteNo].IOData.GetData(address.Address0);
        //    }
        //}
        //public int IntAlarmsSerious
        //{
        //    get
        //    {
        //        AddressClass address = new AddressClass("0:D00500");
        //        return PLC[address.SiteNo].IOData.GetData(address.Address0);
        //    }
        //}

        //public int IntAlarmsCommon
        //{
        //    get
        //    {
        //        AddressClass address = new AddressClass("0:D00502");
        //        return PLC[address.SiteNo].IOData.GetData(address.Address0);
        //    }
        //}
        //public int IntAlarmsWarning
        //{
        //    get
        //    {
        //        AddressClass address = new AddressClass("0:D00420");
        //        return PLC[address.SiteNo].IOData.GetData(address.Address0);
        //    }
        //}
        public bool CLEARALARMS
        {
            get
            {
                return GetBit(1081);
            }
            set
            {
                SetBit(1081, value);
            }
        }
        public bool GetAlarmsAddress(int iSiteNo, string strAddress)
        {
            return PLC[iSiteNo].IOData.GetBit(strAddress);
        }
        public bool ADR_ISPAUSE
        {
            get
            {
                return GetBit(1056);
            }
        }

        public bool ADR_ISEMC
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_ISEMC];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }
        public bool ADR_ISRESET
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_ISRESET];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }
        public bool ADR_ISSTART
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_ISSTART];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }
        public bool ADR_ISSTOP
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_ISSTOP];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }


        public bool ADR_RED
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_RED];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_RED];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_GREEN
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_GREEN];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_GREEN];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_YELLOW
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_YELLOW];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_YELLOW];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }


        public bool ADR_BUZZER
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_BUZZER];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_BUZZER];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }

        public bool ADR_RESET
        {
            get
            {
                AddressClass address = new AddressClass("0:M1001");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = new AddressClass("0:M1001");
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_RESETING
        {
            get
            {
                AddressClass address = new AddressClass("0:M1062");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            //set
            //{
            //    AddressClass address = new AddressClass("0:M1062");
            //    PLC[address.SiteNo].SetIO(value, address.Address0);
            //}
        }
        public bool ADR_RESETCOMPLETE
        {
            get
            {
                AddressClass address = new AddressClass("0:M1063");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            //set
            //{
            //    AddressClass address = new AddressClass("0:M1063");
            //    PLC[address.SiteNo].SetIO(value, address.Address0);
            //}
        }

        public bool ADR_LineScanStart
        {
            get
            {
                AddressClass address = new AddressClass("0:M1118");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = new AddressClass("0:M1118");
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_LineScaning
        {
            get
            {
                AddressClass address = new AddressClass("0:M1119");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }
        public bool ADR_LineScanComplete
        {
            get
            {
                AddressClass address = new AddressClass("0:M1120");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }

        public bool ADR_TrayLineScanStart
        {
            get
            {
                AddressClass address = new AddressClass("0:M1130");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = new AddressClass("0:M1130");
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_TrayLineScaning
        {
            get
            {
                AddressClass address = new AddressClass("0:M1131");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }
        public bool ADR_TrayLineScanComplete
        {
            get
            {
                AddressClass address = new AddressClass("0:M1132");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }


        public bool ADR_LinePCToPlcSign
        {
            get
            {
                AddressClass address = new AddressClass("0:M1124");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = new AddressClass("0:M1125");
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        /// <summary>
        /// 线扫不出盘结束的时候关掉这个点位
        /// </summary>
        public bool ADR_LinePCToPlcSign2
        {
            get
            {
                AddressClass address = new AddressClass("0:M1125");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = new AddressClass("0:M1124");
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }


        //模组2
        /// <summary>
        /// 供料有无料
        /// </summary>
        public bool t2sr_ishavenoproduct
        {
            get { return GetBit(1032); }
        }
        /// <summary>
        /// 供料满料提醒
        /// </summary>
        public bool t2sr_ishavenoproduct_notice
        {
            get { return GetBit(1031); }
        }
        /// <summary>
        /// 平台有料1
        /// </summary>
        public bool t2sr_ishavenoproduct_carrier_1
        {
            get { return GetBit(1029); }
        }
        /// <summary>
        /// 平台有料2
        /// </summary>
        public bool t2sr_ishavenoproduct_carrier_2
        {
            get { return GetBit(1030); }
        }
        /// <summary>
        /// 收料满料
        /// </summary>
        public bool t2sr_isfullproduct
        {
            get { return GetBit(1033); }
        }
        /// <summary>
        /// 收料无料
        /// </summary>
        public bool t2sr_isfullnoproduct
        {
            get { return GetBit(1034); }
        }

        /// <summary>
        /// 再次扫码
        /// </summary>
        public bool RetryGetBarcode
        {
            get
            {
                return GetBit(1146);
            }
            set
            {
                SetBit(1146, value);
            }
        }
        /// <summary>
        /// 再次绑定
        /// </summary>
        public bool RetryBind
        {
            get
            {
                return GetBit(1147);
            }
            set
            {
                SetBit(1147, value);
            }
        }

        #region 供料收料載台氣缸狀態控制

        public bool IsFeedOut
        {
            get
            {
                return GetBit(1025);
            }
        }
        public bool IsFeedIn
        {
            get
            {
                return GetBit(1026);
            }
        }
        public bool IsTakeOut
        {
            get
            {
                return GetBit(1027);
            }
        }
        public bool IsTakeIn
        {
            get
            {
                return GetBit(1028);
            }
        }
        public bool IsLevelClamp
        {
            get
            {
                return GetBit(1021);
            }
        }
        public bool IsLevelLoosen
        {
            get
            {
                return GetBit(1022);
            }
        }
        public bool IsVericalClamp
        {
            get
            {
                return GetBit(1023);
            }
        }
        public bool IsVericalLoosen
        {
            get
            {
                return GetBit(1024);
            }
        }

        public bool CyFeed
        {
            get
            {
                return IsFeedOut && !IsFeedIn;
            }
            set
            {
                SetBit(1093, value);
                SetBit(1094, !value);
            }
        }
        public bool CyTake
        {
            get
            {
                return IsTakeOut && !IsTakeIn;
            }
            set
            {
                SetBit(1095, value);
                SetBit(1096, !value);
            }
        }
        public bool CyLevel
        {
            get
            {
                return IsLevelClamp && !IsLevelLoosen;
            }
            set
            {
                SetBit(1089, value);
                SetBit(1090, !value);
            }
        }
        public bool CyVerical
        {
            get
            {
                return IsVericalClamp && !IsVericalLoosen;
            }
            set
            {
                SetBit(1091, value);
                SetBit(1092, !value);
            }
        }

        #endregion
        public int GetAlmValue
        {
            get
            {
                return GetDValue(0);
            }
        }
        public bool IsWarningGetbarcode
        {
            get
            {
                return GetBit(1069);
            }
        }
        public bool IsWarningBindbarcode
        {
            get
            {
                return GetBit(1070);
            }
        }

        /// <summary>
        /// 强制终止plc中的流程
        /// </summary>
        public bool ForceStopPlcProcess
        {
            set
            {
                SetBit(1007, value);
            }
        }
        /// <summary>
        /// 屏蔽門禁
        /// </summary>
        public bool BypassDoor
        {
            get
            {
                return GetBit(1071);
            }
            set
            {
                SetBit(1071, value);
            }
        }

        public bool GetBit(int index)
        {
            if (index < 0)
                return false;
            string addr = "0:M" + index.ToString("0000");
            return GetBit(addr);
        }
        public bool GetBit(string addStr)
        {
            AddressClass address = new AddressClass(addStr);
            return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        }
        public void SetBit(int index, bool ison)
        {
            string addr = "0:M" + index.ToString("0000");
            AddressClass address = new AddressClass(addr);
            PLC[address.SiteNo].SetIO(ison, address.Address0);
        }
        public void SetBit(string addStr, bool ison)
        {
            AddressClass address = new AddressClass(addStr);
            PLC[address.SiteNo].SetIO(ison, address.Address0);
        }
        //public short GetMW(string addStr)
        //{
        //    AddressClass address = new AddressClass(addStr);
        //    return PLC[address.SiteNo].IOData.GetMW(address.Address0);
        //}

        public int GetData(string addStr)
        {
            AddressClass address = new AddressClass(addStr);
            return HEXSigned32(ValueToHEX(PLC[address.SiteNo].IOData.GetData(address.Address0), 4));
        }
        public int GetDValue(int index)
        {
            string addr = "0:D" + index.ToString("00000");
            return GetData(addr);
        }
        public int GetRValue(int index)
        {
            string addr = "0:R" + index.ToString("00000");
            return GetData(addr);
        }
    }
#endif

#if FX3U
    public class MiniX6IOClass : GeoIOClass
    {

        public MiniX6IOClass()
        {

        }
        public void Initial(string path, JetEazy.ControlSpace.PLCSpace.VsCommPLC[] plc)
        {

            ADDRESSARRAY = new AddressClass[(int)ADRMiniX6.COUNT];
            PLCALARMS = new PLCAlarmsClass[(int)AlarmsEnum.ALARMSCOUNT];

            PLC = plc;

            INIFILE = path + "\\IO.INI";

            LoadData();

        }
        public override void LoadData()
        {

            ADDRESSARRAY[(int)ADRMiniX6.ADR_ISEMC] = new AddressClass(ReadINIValue("Status Address", "ADR_ISEMC", "", INIFILE));
            ADDRESSARRAY[(int)ADRMiniX6.ADR_ISRESET] = new AddressClass(ReadINIValue("Status Address", "ADR_ISRESET", "", INIFILE));
            ADDRESSARRAY[(int)ADRMiniX6.ADR_ISSTART] = new AddressClass(ReadINIValue("Status Address", "ADR_ISSTART", "", INIFILE));
            ADDRESSARRAY[(int)ADRMiniX6.ADR_ISSTOP] = new AddressClass(ReadINIValue("Status Address", "ADR_ISSTOP", "", INIFILE));


            ADDRESSARRAY[(int)ADRMiniX6.ADR_RED] = new AddressClass(ReadINIValue("Operation Address", "ADR_RED", "", INIFILE));
            ADDRESSARRAY[(int)ADRMiniX6.ADR_GREEN] = new AddressClass(ReadINIValue("Operation Address", "ADR_GREEN", "", INIFILE));
            ADDRESSARRAY[(int)ADRMiniX6.ADR_YELLOW] = new AddressClass(ReadINIValue("Operation Address", "ADR_YELLOW", "", INIFILE));
            ADDRESSARRAY[(int)ADRMiniX6.ADR_BUZZER] = new AddressClass(ReadINIValue("Operation Address", "ADR_BUZZER", "", INIFILE));


            #region 读取csv- ALARM

            string alarm0_path = INIFILE.Replace("IO.INI", "ALARMIO0.csv");
            System.IO.StreamReader _sr = null;
            try
            {
                _sr = new System.IO.StreamReader(alarm0_path);
                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON] = new PLCAlarmsClass("D00004:M0256");
                //PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON] = new PLCAlarmsClass("MW0000:MX0.0,MW0001:MX2.0,MW0002:MX4.0,MW0003:MX6.0,MW0004:MX8.0,MW0005:MX10.0,MW0006:MX12.0,MW0007:MX14.0");

                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS] = new PLCAlarmsClass("D00000:M0224,D00002:M0240");
                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_WARNING] = new PLCAlarmsClass("D00004:M0256");
                string strRead = string.Empty;
                while (!_sr.EndOfStream)
                {
                    strRead = _sr.ReadLine();
                    string[] strs = strRead.Split(',').ToArray();
                    if (strs.Length >= 4)
                    {
                        switch (strs[0])
                        {
                            case "COMMON":
                                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON].PLCAlarmsAddDescription("0," + strs[2] + "," + strs[3]);
                                break;
                            case "SERIOUS":
                                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS].PLCAlarmsAddDescription("0," + strs[2] + "," + strs[3]);
                                break;
                            case "WARNING":
                                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_WARNING].PLCAlarmsAddDescription("0," + strs[2] + "," + strs[3]);
                                break;
                        }
                    }
                }

                _sr.Close();
                _sr.Dispose();
                _sr = null;
            }
            catch
            {

            }

            if (_sr != null)
                _sr.Dispose();

            #endregion

        }

        public override void SaveData()
        {

        }

        public bool IsAlarmsSerious
        {
            get
            {
                //AddressClass address = new AddressClass("0:M1060");
                //return PLC[address.SiteNo].IOData.GetBit(address.Address0);

                return GetBit(220) || GetBit(221);
            }
        }

        public bool IsAlarmsCommon
        {
            get
            {
                //AddressClass address = new AddressClass("0:M1065");
                //return PLC[address.SiteNo].IOData.GetBit(address.Address0);
                return GetBit(222);
            }
        }
        //public bool IsAlarmsWarning
        //{
        //    get
        //    {
        //        AddressClass address = new AddressClass("0:D00420");
        //        return PLC[address.SiteNo].IOData.GetData(address.Address0);
        //    }
        //}
        //public int IntAlarmsSerious
        //{
        //    get
        //    {
        //        AddressClass address = new AddressClass("0:D00500");
        //        return PLC[address.SiteNo].IOData.GetData(address.Address0);
        //    }
        //}

        //public int IntAlarmsCommon
        //{
        //    get
        //    {
        //        AddressClass address = new AddressClass("0:D00502");
        //        return PLC[address.SiteNo].IOData.GetData(address.Address0);
        //    }
        //}
        //public int IntAlarmsWarning
        //{
        //    get
        //    {
        //        AddressClass address = new AddressClass("0:D00420");
        //        return PLC[address.SiteNo].IOData.GetData(address.Address0);
        //    }
        //}
        public bool CLEARALARMS
        {
            get
            {
                return GetBit(223);
            }
            set
            {
                SetBit(223, value);
            }
        }
        public bool GetAlarmsAddress(int iSiteNo, string strAddress)
        {
            if (string.IsNullOrEmpty(strAddress))
                return false;
            return PLC[iSiteNo].IOData.GetBit(strAddress);
        }
        public bool ADR_ISPAUSE
        {
            get
            {
                return GetBit(3);
            }
        }

        public bool ADR_ISEMC
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_ISEMC];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }
        public bool ADR_ISRESET
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_ISRESET];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }
        public bool ADR_ISSTART
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_ISSTART];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }
        public bool ADR_ISSTOP
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_ISSTOP];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }


        public bool ADR_RED
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_RED];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_RED];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_GREEN
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_GREEN];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_GREEN];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_YELLOW
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_YELLOW];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_YELLOW];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_BUZZER
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_BUZZER];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMiniX6.ADR_BUZZER];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }

        public bool ADR_RESET
        {
            get
            {
                //AddressClass address = new AddressClass("0:M1001");
                //return PLC[address.SiteNo].IOData.GetBit(address.Address0);
                return GetBit(192);
            }
            set
            {
                //AddressClass address = new AddressClass("0:M1001");
                //PLC[address.SiteNo].SetIO(value, address.Address0);
                SetBit(192, value);
            }
        }
        public bool ADR_RESETING
        {
            get
            {
                //AddressClass address = new AddressClass("0:M1062");
                //return PLC[address.SiteNo].IOData.GetBit(address.Address0);
                return GetBit(193);
            }
            //set
            //{
            //    AddressClass address = new AddressClass("0:M1062");
            //    PLC[address.SiteNo].SetIO(value, address.Address0);
            //}
        }
        public bool ADR_RESETCOMPLETE
        {
            get
            {
                //AddressClass address = new AddressClass("0:M1063");
                //return PLC[address.SiteNo].IOData.GetBit(address.Address0);
                return GetBit(194);
            }
            //set
            //{
            //    AddressClass address = new AddressClass("0:M1063");
            //    PLC[address.SiteNo].SetIO(value, address.Address0);
            //}
        }

        public bool ADR_LineScanStart
        {
            get
            {
                AddressClass address = new AddressClass("0:M0186");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = new AddressClass("0:M0186");
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_LineScaning
        {
            get
            {
                AddressClass address = new AddressClass("0:M0187");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }
        public bool ADR_LineScanComplete
        {
            get
            {
                AddressClass address = new AddressClass("0:M0188");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }

        public bool ADR_TrayLineScanStart
        {
            get
            {
                AddressClass address = new AddressClass("0:M0180");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = new AddressClass("0:M0180");
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_TrayLineScaning
        {
            get
            {
                AddressClass address = new AddressClass("0:M0181");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }
        public bool ADR_TrayLineScanComplete
        {
            get
            {
                AddressClass address = new AddressClass("0:M0182");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }


        public bool ADR_LinePCToPlcSign
        {
            get
            {
                AddressClass address = new AddressClass("0:M0014");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = new AddressClass("0:M0015");
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        /// <summary>
        /// 线扫不出盘结束的时候关掉这个点位
        /// </summary>
        public bool ADR_LinePCToPlcSign2
        {
            get
            {
                AddressClass address = new AddressClass("0:M0015");
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = new AddressClass("0:M0014");
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }


        //模组2
        /// <summary>
        /// 供料有无料
        /// </summary>
        public bool t2sr_ishavenoproduct
        {
            get { return GetBit(42); }
        }
        /// <summary>
        /// 供料满料提醒
        /// </summary>
        public bool t2sr_ishavenoproduct_notice
        {
            get { return GetBit(41); }
        }
        /// <summary>
        /// 平台有料1
        /// </summary>
        public bool t2sr_ishavenoproduct_carrier_1
        {
            get { return GetBit(43); }
        }
        /// <summary>
        /// 平台有料2
        /// </summary>
        public bool t2sr_ishavenoproduct_carrier_2
        {
            get { return GetBit(44); }
        }
        /// <summary>
        /// 收料满料
        /// </summary>
        public bool t2sr_isfullproduct
        {
            get { return GetBit(45); }
        }
        /// <summary>
        /// 收料无料
        /// </summary>
        public bool t2sr_isfullnoproduct
        {
            get { return GetBit(46); }
        }

        /// <summary>
        /// 再次扫码
        /// </summary>
        public bool RetryGetBarcode
        {
            get
            {
                return GetBit(58);
            }
            set
            {
                SetBit(58, value);
            }
        }
        /// <summary>
        /// 再次绑定
        /// </summary>
        public bool RetryBind
        {
            get
            {
                return GetBit(59);
            }
            set
            {
                SetBit(59, value);
            }
        }

        #region 供料收料載台氣缸狀態控制

        public bool IsFeedOut
        {
            get
            {
                return GetBit(33);
            }
        }
        public bool IsFeedIn
        {
            get
            {
                return GetBit(34);
            }
        }
        public bool IsTakeOut
        {
            get
            {
                return GetBit(39);
            }
        }
        public bool IsTakeIn
        {
            get
            {
                return GetBit(40);
            }
        }
        /// <summary>
        /// 水平夹紧
        /// </summary>
        public bool IsLevelClamp
        {
            get
            {
                return GetBit(37);
            }
        }
        /// <summary>
        /// 水平松开
        /// </summary>
        public bool IsLevelLoosen
        {
            get
            {
                return GetBit(38);
            }
        }
        public bool IsVericalClamp
        {
            get
            {
                return GetBit(35);
            }
        }
        public bool IsVericalLoosen
        {
            get
            {
                return GetBit(36);
            }
        }

        public bool CyFeed
        {
            get
            {
                return IsFeedOut && !IsFeedIn;
            }
            set
            {
                SetBit(16, value);
                SetBit(17, !value);
            }
        }
        public bool CyTake
        {
            get
            {
                return IsTakeOut && !IsTakeIn;
            }
            set
            {
                SetBit(22, value);
                SetBit(23, !value);
            }
        }
        public bool CyLevel
        {
            get
            {
                return IsLevelClamp && !IsLevelLoosen;
            }
            set
            {
                SetBit(20, value);
                SetBit(21, !value);
            }
        }
        public bool CyVerical
        {
            get
            {
                return IsVericalClamp && !IsVericalLoosen;
            }
            set
            {
                SetBit(18, value);
                SetBit(19, !value);
            }
        }

        #endregion
        public int GetAlmValue
        {
            get
            {
                return GetDValue(0) + GetDValue(2);
            }
        }
        public bool IsWarningGetbarcode
        {
            get
            {
                return GetBit(256);
            }
        }
        public bool IsWarningBindbarcode
        {
            get
            {
                return GetBit(257);
            }
        }

        /// <summary>
        /// 强制终止plc中的流程
        /// </summary>
        public bool ForceStopPlcProcess
        {
            set
            {
                SetBit(53, value);
            }
        }
        /// <summary>
        /// 屏蔽門禁
        /// </summary>
        public bool BypassDoor
        {
            get
            {
                return GetBit(47);
            }
            set
            {
                SetBit(47, value);
            }
        }
        /// <summary>
        /// plc試運行
        /// </summary>
        public bool PlcTestRun
        {
            get
            {
                return GetBit(51);
            }
            set
            {
                SetBit(51, value);
            }
        }
        public bool ADR_ISAUTO_AND_MANUAL
        {
            get
            {
                return GetBit(52);
            }
            set
            {
                SetBit(52, value);
            }
        }
        

        public bool GetBit(int index)
        {
            if (index < 0)
                return false;
            string addr = "0:M" + index.ToString("0000");
            return GetBit(addr);
        }
        public bool GetBit(string addStr)
        {
            AddressClass address = new AddressClass(addStr);
            return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        }
        public void SetBit(int index, bool ison)
        {
            string addr = "0:M" + index.ToString("0000");
            AddressClass address = new AddressClass(addr);
            PLC[address.SiteNo].SetIO(ison, address.Address0);
        }
        public void SetBit(string addStr, bool ison)
        {
            AddressClass address = new AddressClass(addStr);
            PLC[address.SiteNo].SetIO(ison, address.Address0);
        }
        //public short GetMW(string addStr)
        //{
        //    AddressClass address = new AddressClass(addStr);
        //    return PLC[address.SiteNo].IOData.GetMW(address.Address0);
        //}

        public int GetData(string addStr)
        {
            AddressClass address = new AddressClass(addStr);
            return HEXSigned32(ValueToHEX(PLC[address.SiteNo].IOData.GetData(address.Address0), 4));
        }
        public int GetDValue(int index)
        {
            string addr = "0:D" + index.ToString("00000");
            return GetData(addr);
        }
        public int GetRValue(int index)
        {
            string addr = "0:R" + index.ToString("00000");
            return GetData(addr);
        }
    }
#endif

}
