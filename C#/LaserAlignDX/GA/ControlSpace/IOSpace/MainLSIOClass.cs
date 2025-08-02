using JetEazy.ControlSpace;
using System;
using System.Linq;
using VsCommon.ControlSpace.IOSpace;

namespace Traveller106.ControlSpace.IOSpace
{
    public class MainLSIOClass : GeoIOClass
    {
        const int INPUT_COUNT = 32;
        const int ONEMMSTEP = 100;


        public MainLSIOClass()
        {

        }
        public void Initial(string path, JetEazy.ControlSpace.PLCSpace.VsCommPLC[] plc)
        {
            //ADDRESSARRAY = new AddressClass[(int)DispensingAddressEnum.COUNT];
            ADDRESSARRAY_INPUT = new AddressClass[INPUT_COUNT];
            PLCALARMS = new PLCAlarmsClass[(int)AlarmsEnum.ALARMSCOUNT];

            PLC = plc;

            INIFILE = path + "\\IO.INI";

            LoadData();

        }
        public override void LoadData()
        {

            int i = 0;
            while (i < INPUT_COUNT)
            {
                ADDRESSARRAY_INPUT[i] = new AddressClass(ReadINIValue("Status Address", "IX" + i.ToString("000"), "", INIFILE));
                i++;
            }


            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_UVTOP] = new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_UVTOP.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_UVBOTTOM] = new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_UVBOTTOM.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_FINTOP] = new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_FINTOP.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_FINBOTTOM] = new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_FINBOTTOM.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_ATTRACT] = new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_SWITCH_ATTRACT.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_MATERIALBASE] = new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_SWITCH_MATERIALBASE.ToString(), "", INIFILE));

            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_UV] =
            //    new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_SWITCH_UV.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_DISPENSING] =
            //    new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_SWITCH_DISPENSING.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_LIGHT] =
            //    new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_SWITCH_LIGHT.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_AXIS_BREAK] =
            //    new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_AXIS_BREAK.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_RED] =
            //    new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_RED.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_YELLOW] =
            //    new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_YELLOW.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_GREEN] =
            //    new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_GREEN.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_BUZZER] =
            //   new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_BUZZER.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_ELECTROMAGNET] =
            //   new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_ELECTROMAGNET.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_FAN] =
            //   new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_FAN.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SMALL_LIGHT] =
            //   new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_SMALL_LIGHT.ToString(), "", INIFILE));

            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_RESET_START] =
            //  new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_RESET_START.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_RESETING] =
            //  new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_RESETING.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_RESET_COMPLETE] =
            //  new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_RESET_COMPLETE.ToString(), "", INIFILE));

            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_CONTROLBOX] =
            //  new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_CONTROLBOX.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)DispensingAddressEnum.ADR_POGO_PIN] =
            //  new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_POGO_PIN.ToString(), "", INIFILE));


            #region ALARMS INI

            #region 读取csv- ALARM

            string alarm0_path = INIFILE.Replace("IO.INI", "ALARMIO0.csv");
            System.IO.StreamReader _sr = null;
            try
            {
                _sr = new System.IO.StreamReader(alarm0_path);
                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON] = new PLCAlarmsClass("MW0000:MX0.0,MW0001:MX2.0,MW0002:MX4.0,MW0003:MX6.0,MW0004:MX8.0,MW0005:MX10.0,MW0006:MX12.0,MW0007:MX14.0");
                //PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON] = new PLCAlarmsClass("MW0000:MX0.0,MW0001:MX2.0,MW0002:MX4.0,MW0003:MX6.0,MW0004:MX8.0,MW0005:MX10.0,MW0006:MX12.0,MW0007:MX14.0");

                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS] = new PLCAlarmsClass("MW0008:MX16.0");
                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_WARNING] = new PLCAlarmsClass("MW0009:MX18.0");
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

            //int iindex = 0;
            //string str = "";
            ////string str_alarms = ReadINIValue("Parameters", "ALARMS_ADR_SERIOUS", "", INIFILE.Replace("IO.INI", "ALARMIO0.INI"));
            //PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS] = 
            //    new PLCAlarmsClass(ReadINIValue("Parameters", AlarmsEnum.ALARMS_ADR_SERIOUS.ToString(), "", INIFILE.Replace("IO.INI", "ALARMIO0.INI")));
            //foreach (PLCAlarmsItemClass item in PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS].PLCALARMSLIST)
            //{
            //    iindex = 0;
            //    while (iindex < 16)
            //    {
            //        str = ReadINIValue(item.ADR_Address, "Bit " + iindex.ToString(), "", INIFILE.Replace("IO.INI", "ALARMIO0.INI"));
            //        PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS].PLCAlarmsAddDescription("0," + item.CovertToNormalAddress(iindex) + "," + str);
            //        iindex++;
            //    }
            //}
            //PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON] = 
            //    new PLCAlarmsClass(ReadINIValue("Parameters", AlarmsEnum.ALARMS_ADR_COMMON.ToString(), "", INIFILE.Replace("IO.INI", "ALARMIO0.INI")));
            //foreach (PLCAlarmsItemClass item in PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON].PLCALARMSLIST)
            //{
            //    iindex = 0;
            //    while (iindex < 16)
            //    {
            //        str = ReadINIValue(item.ADR_Address, "Bit " + iindex.ToString(), "", INIFILE.Replace("IO.INI", "ALARMIO0.INI"));
            //        PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON].PLCAlarmsAddDescription("0," + item.CovertToNormalAddress(iindex) + "," + str);
            //        iindex++;
            //    }
            //}

            #endregion

        }

        public override void SaveData()
        {

        }

        short m_IsDoorOpenTmp = 0;
        public bool IsDoorOpen
        {
            get
            {
                AddressClass address = new AddressClass("0:MW0007");
                //if(m_IsDoorOpenTmp != )
                return PLC[address.SiteNo].IOData.GetMW(address.Address0) != 0;
            }
        }
        public bool IsPLCPause
        {
            get
            {
                return GetQXQB(1615);
            }
        }
        public bool IsAlarmsSerious
        {
            get
            {
                AddressClass address = new AddressClass("0:MW0008");
                return PLC[address.SiteNo].IOData.GetMW(address.Address0) != 0;
            }
        }
        public int IntAlarmsSerious
        {
            get
            {
                AddressClass address = new AddressClass("0:MW0008,MW0010");
                return PLC[address.SiteNo].IOData.GetMW(address.Address0)
                            + PLC[address.SiteNo].IOData.GetMW(address.Address1);
            }
        }
        public bool IsAlarmsCommon
        {
            get
            {
                AddressClass address = new AddressClass("0:MW0000,MW0001");
                AddressClass address1 = new AddressClass("0:MW0002,MW0003");
                AddressClass address2 = new AddressClass("0:MW0004,MW0005");
                AddressClass address3 = new AddressClass("0:MW0006");
                //AddressClass address3 = new AddressClass("0:MW0006,MW0007");
                return PLC[address.SiteNo].IOData.GetMW(address.Address0) != 0
                            || PLC[address.SiteNo].IOData.GetMW(address.Address1) != 0
                            || PLC[address1.SiteNo].IOData.GetMW(address1.Address0) != 0
                            || PLC[address1.SiteNo].IOData.GetMW(address1.Address1) != 0
                            || PLC[address2.SiteNo].IOData.GetMW(address2.Address0) != 0
                            || PLC[address2.SiteNo].IOData.GetMW(address2.Address1) != 0
                            || PLC[address3.SiteNo].IOData.GetMW(address3.Address0) != 0;
                            //|| PLC[address3.SiteNo].IOData.GetMW(address3.Address1) > 0;
                //|| PLC[address1.SiteNo].IOData.GetMW(address1.Address0) > 0;
            }
        }
        public int IntAlarmsCommon
        {
            get
            {
                AddressClass address = new AddressClass("0:MW0000,MW0001");
                AddressClass address1 = new AddressClass("0:MW0002,MW0003");
                AddressClass address2 = new AddressClass("0:MW0004,MW0005");
                AddressClass address3 = new AddressClass("0:MW0006");
                //AddressClass address3 = new AddressClass("0:MW0006,MW0007");
                return PLC[address.SiteNo].IOData.GetMW(address.Address0) 
                            + PLC[address.SiteNo].IOData.GetMW(address.Address1) 
                            + PLC[address1.SiteNo].IOData.GetMW(address1.Address0) 
                            + PLC[address1.SiteNo].IOData.GetMW(address1.Address1) 
                            + PLC[address2.SiteNo].IOData.GetMW(address2.Address0) 
                            + PLC[address2.SiteNo].IOData.GetMW(address2.Address1) 
                            + PLC[address3.SiteNo].IOData.GetMW(address3.Address0) ;
                //|| PLC[address3.SiteNo].IOData.GetMW(address3.Address1) > 0;
                //|| PLC[address1.SiteNo].IOData.GetMW(address1.Address0) > 0;
            }
        }
        public int IntAlarmsWarning
        {
            get
            {
                AddressClass address = new AddressClass("0:MW0009");
                return PLC[address.SiteNo].IOData.GetMW(address.Address0);
            }
        }
        public bool IsAlarmsWarning
        {
            get
            {
                return IntAlarmsWarning != 0;
            }
        }
        public bool IsWarningGetbarcode
        {
            get
            {
                return GetQXQB("0:MX18.0")
                            || GetQXQB("0:MX18.1")
                             || GetQXQB("0:MX18.2")
                              || GetQXQB("0:MX18.3")
                               || GetQXQB("0:MX18.4");
            }
        }
        public bool IsWarningBindbarcode
        {
            get
            {
                return GetQXQB("0:MX18.5")
                            || GetQXQB("0:MX18.6")
                             || GetQXQB("0:MX18.7")
                              || GetQXQB("0:MX19.0")
                               || GetQXQB("0:MX19.1");
            }
        }

        public bool ADR_ISEMC
        {
            get { return !GetQXQB("0:IX0.0"); }
        }
        public bool ADR_ISRESET
        {
            get { return GetQXQB("0:IX0.1"); }
        }
        public bool ADR_ISSTART
        {
            get { return GetQXQB("0:IX0.2"); }
        }
        public bool ADR_ISPAUSE
        {
            get { return GetQXQB("0:IX0.3"); }
        }
        public bool ADR_ISSTOP
        {
            get { return GetQXQB("0:IX0.4"); }
        }
        public bool ADR_ISDOOR
        {
            get { return GetQXQB("0:IX0.5"); }
        }
        //public bool ADR_ISSCREEN
        //{
        //    get { return !GetInputIndex(18) && !ADR_BYPASS_SCREEN; }
        //}
        public bool CLEARALARMS
        {
            get { return GetQXQB("0:QB1603.0"); }
            set
            {
                SetQXQB("0:QB1603.0", value);
            }
        }
        public bool ADR_CHANGE_MANUAL_AUTO
        {
            get { 
                return GetMW("0:MW2030") == 1; }
            set
            {
                SetMW("0:MW2030", (value ? 1 : 0));
            }
        }

        public bool ADR_RED
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
        public bool ADR_GREEN
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
        public bool ADR_BUZZER
        {
            get
            {
                return GetQXQB("0:QX0.4");
            }
            set
            {
                SetQXQB("0:QX0.4", value);
            }
        }


        public bool GetAlarmsAddress(int iSiteNo, string strAddress)
        {
            return PLC[iSiteNo].IOData.GetBit(strAddress);
        }
        public bool GetQXQB(int index)
        {
            if (index < 0)
                return false;
            string addr = "0:QB" + index.ToString() + ".0";
            return GetQXQB(addr);
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

        /// <summary>
        /// 真空泵
        /// </summary>
        public bool PLCVaccum
        {
            get
            {
                return GetQXQB("0:QX7.3");
            }
            set
            {
                SetQXQB("0:QX7.3", value);
            }
        }

        #region 测试点位
        /// <summary>
        /// 测试开始
        /// </summary>
        public bool TestLineScanOnce
        {
            set
            {
                SetQXQB("0:QB2000.0", value);
            }
        }
        public int TestLineScanOnceSpeed
        {
            set
            {
                SetMW("0:MW3000", value);
            }
        }
        /// <summary>
        /// 抓图时机
        /// </summary>
        public bool TestLineScanOnceImageStart
        {
            get
            {
                return GetQXQB("0:QB1550.0");
            }
            set
            {
                SetQXQB("0:QB1550.0", value);
            }
        }
        /// <summary>
        /// 测试完成
        /// </summary>
        public bool TestLineScanOnceComplete
        {
            get
            {
                return GetQXQB("0:QB1551.0");
            }
        }

        #endregion

        /// <summary>
        /// 抓图完成并通知plc
        /// </summary>
        public bool m2_GetImageOK
        {
            get { return GetQXQB("0:QB1607.0"); }
            set { SetQXQB("0:QB1604.0", value); }
        }
        /// <summary>
        /// 停止plc中所有流程
        /// </summary>
        public bool SetPLCProcessStop
        {
            set { SetQXQB("0:QB1605.0", value); }
        }
        /// <summary>
        /// plc初始化完成
        /// </summary>
        public bool GetPLCInitComplete
        {
            get { return GetQXQB("0:QB1606.0"); }
            //set { SetQXQB("0:QB1605", value); }
        }

        public short GetCmdPLCIndex
        {
            get { return GetMW("0:MW2022"); }
        }
        /// <summary>
        /// 当前plc的步数 抓取线扫图片的当前index
        /// </summary>
        public short GetLineScanStep
        {
            get { return GetMW("0:MW1543"); }
        }

        /// <summary>
        /// 再次扫码
        /// </summary>
        public bool RetryGetBarcode
        {
            get { return GetQXQB("0:QB2328.0"); }
            set { SetQXQB("0:QB2328.0", value); }
        }
        /// <summary>
        /// 再次绑定
        /// </summary>
        public bool RetryBind
        {
            get { return GetQXQB("0:QB2329.0"); }
            set { SetQXQB("0:QB2329.0", value); }
        }

        /// <summary>
        /// 控制plc写入保持型数据
        /// </summary>
        public bool PlcWriteHoldData
        {
            get { return GetQXQB("0:QB1632.0"); }
            set { SetQXQB("0:QB1632.0", value); }
        }

        #region 轨道的检知信号
        //模组1

        /// <summary>
        /// 供料有无料
        /// </summary>
        public bool t1sr_ishavenoproduct
        {
            get { return !GetQXQB("0:IX9.2"); }
        }
        /// <summary>
        /// 供料无料提醒
        /// </summary>
        public bool t1sr_ishavenoproduct_notice
        {
            get { return !GetQXQB("0:IX9.3"); }
        }
        /// <summary>
        /// 平台有料1
        /// </summary>
        public bool t1sr_ishavenoproduct_carrier_1
        {
            get { return GetQXQB("0:IX9.4"); }
        }
        /// <summary>
        /// 平台有料2
        /// </summary>
        public bool t1sr_ishavenoproduct_carrier_2
        {
            get { return GetQXQB("0:IX9.5"); }
        }
        /// <summary>
        /// 收料满料
        /// </summary>
        public bool t1sr_isfullproduct
        {
            get { return !GetQXQB("0:IX9.6"); }
        }


        //模组2
        /// <summary>
        /// 供料有无料
        /// </summary>
        public bool t2sr_ishavenoproduct
        {
            get { return !GetQXQB("0:IX11.2"); }
        }
        /// <summary>
        /// 供料满料提醒
        /// </summary>
        public bool t2sr_ishavenoproduct_notice
        {
            get { return !GetQXQB("0:IX11.3"); }
        }
        /// <summary>
        /// 平台有料1
        /// </summary>
        public bool t2sr_ishavenoproduct_carrier_1
        {
            get { return GetQXQB("0:IX11.6"); }
        }
        /// <summary>
        /// 平台有料2
        /// </summary>
        public bool t2sr_ishavenoproduct_carrier_2
        {
            get { return GetQXQB("0:IX11.7"); }
        }
        /// <summary>
        /// 收料满料
        /// </summary>
        public bool t2sr_isfullproduct
        {
            get { return !GetQXQB("0:IX11.5"); }
        }
        /// <summary>
        /// 收料无料
        /// </summary>
        public bool t2sr_isfullnoproduct
        {
            get { return !GetQXQB("0:IX11.4"); }
        }

        //模组3
        /// <summary>
        /// 供料有无料
        /// </summary>
        public bool t3sr_ishavenoproduct
        {
            get { return !GetQXQB("0:IX13.2"); }
        }
        /// <summary>
        /// 供料无料提醒
        /// </summary>
        public bool t3sr_ishavenoproduct_notice
        {
            get { return !GetQXQB("0:IX13.3"); }
        }
        /// <summary>
        /// 平台有料1
        /// </summary>
        public bool t3sr_ishavenoproduct_carrier_1
        {
            get { return GetQXQB("0:IX13.4"); }
        }
        /// <summary>
        /// 平台有料2
        /// </summary>
        public bool t3sr_ishavenoproduct_carrier_2
        {
            get { return GetQXQB("0:IX13.5"); }
        }
        /// <summary>
        /// 收料满料
        /// </summary>
        public bool t3sr_isfullproduct
        {
            get { return !GetQXQB("0:IX13.6"); }
        }


        //模组4
        /// <summary>
        /// 供料有无料
        /// </summary>
        public bool t4sr_ishavenoproduct
        {
            get { return !GetQXQB("0:IX15.2"); }
        }
        /// <summary>
        /// 供料无料提醒
        /// </summary>
        public bool t4sr_ishavenoproduct_notice
        {
            get { return !GetQXQB("0:IX15.3"); }
        }
        /// <summary>
        /// 平台有料1
        /// </summary>
        public bool t4sr_ishavenoproduct_carrier_1
        {
            get { return GetQXQB("0:IX15.4"); }
        }
        /// <summary>
        /// 平台有料2
        /// </summary>
        public bool t4sr_ishavenoproduct_carrier_2
        {
            get { return GetQXQB("0:IX15.5"); }
        }
        /// <summary>
        /// 收料满料
        /// </summary>
        public bool t4sr_isfullproduct
        {
            get { return !GetQXQB("0:IX15.6"); }
        }

        #endregion

        #region 其他检知放置处

        /// <summary>
        /// 检查平台是否有tray 
        /// </summary>
        public bool IsAllTrayHaveCarrier
        {
            get
            {
                return t1sr_ishavenoproduct_carrier_1 || t1sr_ishavenoproduct_carrier_2 ||
                               t2sr_ishavenoproduct_carrier_1 || t2sr_ishavenoproduct_carrier_2 ||
                               t3sr_ishavenoproduct_carrier_1 || t3sr_ishavenoproduct_carrier_2 ||
                               t4sr_ishavenoproduct_carrier_1 || t4sr_ishavenoproduct_carrier_2;
            }
        }

        /// <summary>
        /// 自动/手动 切换 off手动 on自动
        /// </summary>
        public bool IsAutoManual
        {
            get { return GetQXQB("0:IX0.5"); }
        }

        public bool Cmd1Complete
        {
            get { return GetQXQB("0:QB2302.0"); }
            set { SetQXQB("0:QB2302.0", value); }
        }
        public int Cmd1Step
        {
            get { return _getData("0:MW2016"); }
        }
        public bool Cmd2Complete
        {
            get { return GetQXQB("0:QB2303.0"); }
            set { SetQXQB("0:QB2303.0", value); }
        }
        public int Cmd2Step
        {
            get { return _getData("0:MW2017"); }
        }
        public bool ProcessStart
        {
            //get { return GetQXQB("0:QB2304.0"); }
            set { SetQXQB("0:QB2304.0", value); }
        }
        public bool Processing
        {
            get { return GetQXQB("0:QB2305.0"); }
            //set { SetQXQB("0:QB2305.0", value); }
        }
        public bool ProcesComplete
        {
            get { return GetQXQB("0:QB2306.0"); }
            //set { SetQXQB("0:QB2306.0", value); }
        }
        int _getData(string addStr)
        {
            AddressClass address = new AddressClass(addStr);
            PLC[address.SiteNo].GetData(address.Address0);
            //if (string.IsNullOrEmpty(address.Address1))
            //    PLC[address.SiteNo].GetData(address.Address1);
            Int32 ret = 0;
            ret = PLC[address.SiteNo].IOData.GetData(address.Address0);
            return ret;
        }

        #endregion

    }
}
