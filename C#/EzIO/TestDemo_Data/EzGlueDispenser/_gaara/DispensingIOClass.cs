using Ez.IOCtrl.Common;
using Ez.Plc.Hcfa;
using System;
using System.Collections.Generic;
using VsCommon.ControlSpace.IOSpace;

namespace Eazy_Project_III.ControlSpace.IOSpace
{
    using AddressClass = HcfaAddress;


    #region ENUMS

    public enum DispensingUI : int
    {
        UICOUNT = 19,
    }

    public enum DispensingAddressEnum : int
    {
        COUNT = 22,

        ADR_UVTOP = 0,
        ADR_UVBOTTOM = 1,
        ADR_FINTOP = 2,
        ADR_FINBOTTOM = 3,
        ADR_SWITCH_ATTRACT = 4,
        ADR_SWITCH_MATERIALBASE = 5,

        ADR_SWITCH_UV = 6,
        ADR_SWITCH_DISPENSING = 7,
        ADR_SWITCH_LIGHT = 8,
        ADR_AXIS_BREAK = 9,
        ADR_RED = 10,
        ADR_YELLOW = 11,
        ADR_GREEN = 12,
        ADR_BUZZER = 13,
        /// <summary>
        /// 电磁铁
        /// </summary>
        ADR_ELECTROMAGNET = 14,
        ADR_FAN = 15,
        ADR_SMALL_LIGHT = 16,
        ADR_CONTROLBOX = 17,
        ADR_POGO_PIN = 18,

        //I00=17,
        ADR_RESET_START = 19,
        ADR_RESETING = 20,
        ADR_RESET_COMPLETE = 21,
    }

    #endregion


    public class DispensingIOClass : GeoIOClass
    {
        const int INPUT_COUNT = 32;
        //const int ONEMMSTEP = 100;


        public DispensingIOClass()
        {

        }

#if(false)
        public void Initial(string path, JetEazy.ControlSpace.PLCSpace.VsCommPLC[] plc)
        {
            ADDRESSARRAY = new AddressClass[(int)DispensingAddressEnum.COUNT];
            ADDRESSARRAY_INPUT = new AddressClass[INPUT_COUNT];
            PLCALARMS = new PLCAlarmsClass[(int)AlarmsEnum.ALARMSCOUNT];
            PLC = plc;
            INIFILE = path + "\\IO.INI";
            LoadData();
        }
#endif

        public List<EzAddress> Load(string iniFileName = null, List<EzAddress> results = null)
        {
            if (iniFileName == null)
                iniFileName = "D:\\JETEAZY\\PROJECT-DISPENSING\\WORK\\DISPENSING\\IO.INI";

            if (results == null)
                results = new List<EzAddress>();

            load_status_addrs(iniFileName, INPUT_COUNT, results);
            load_op_addrs(iniFileName, results);

#if DEBUG && false
            foreach (var addr in results)
            {
                System.Console.WriteLine(addr);
            }
#endif

#if DEBUG
            var results2 = load_op_addrs(iniFileName);
            var results3 = load_op_old(iniFileName);
            //> System.Console.WriteLine();
            System.Diagnostics.Trace.Assert(results2.Count == results3.Count);
            int N = results2.Count;
            for (int i = 0; i < N; i++)
            {
                var a = results2[i];
                var b = results3[i];
                //>>> System.Console.WriteLine(a);
                if (!EzAddress.AreEqual(a, b))
                {
                    System.Console.WriteLine(a);
                    System.Console.WriteLine(b);
                }
                System.Diagnostics.Trace.Assert(EzAddress.AreEqual(a, b));
            }
#endif

            return results;
        }
        private List<EzAddress> load_status_addrs(string iniFileName, int inputCount, List<EzAddress> results = null)
        {
            if (results == null)
                results = new List<EzAddress>();

            //> var ADDRESSARRAY_INPUT = new EzAddress[INPUT_COUNT];

            for (int i = 0; i < inputCount; i++)
            {
                var str = ReadINIValue("Status Address", "IX" + i.ToString("000"), "", iniFileName);
                var address = new AddressClass(str);
                results.Add(address);

                //> ADDRESSARRAY_INPUT[i] = address;
            }

            return results;
        }
        private List<EzAddress> load_op_addrs(string iniFileName, List<EzAddress> results = null)
        {
            if (results == null)
                results = new List<EzAddress>();

            var enumValues = Enum.GetValues(typeof(DispensingAddressEnum));
            foreach (DispensingAddressEnum e in enumValues)
            {
                if (e == DispensingAddressEnum.COUNT)
                    continue;
                string str = ReadINIValue("Operation Address", e.ToString(), "", iniFileName);
                var addr = new AddressClass(str);
                results.Add(addr);
            }
            
            return results;
        }
        private List<EzAddress> load_op_old(string iniFileName, List<EzAddress> results = null)
        {
            if (results == null)
                results = new List<EzAddress>();

            int N = (int)DispensingAddressEnum.COUNT;
            var ADDRESSARRAY = new EzAddress[N];
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_UVTOP] = new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_UVTOP.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_UVBOTTOM] = new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_UVBOTTOM.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_FINTOP] = new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_FINTOP.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_FINBOTTOM] = new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_FINBOTTOM.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_ATTRACT] = new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_SWITCH_ATTRACT.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_MATERIALBASE] = new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_SWITCH_MATERIALBASE.ToString(), "", iniFileName));

            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_UV] =
                new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_SWITCH_UV.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_DISPENSING] =
                new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_SWITCH_DISPENSING.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_LIGHT] =
                new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_SWITCH_LIGHT.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_AXIS_BREAK] =
                new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_AXIS_BREAK.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_RED] =
                new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_RED.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_YELLOW] =
                new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_YELLOW.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_GREEN] =
                new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_GREEN.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_BUZZER] =
               new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_BUZZER.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_ELECTROMAGNET] =
               new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_ELECTROMAGNET.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_FAN] =
               new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_FAN.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SMALL_LIGHT] =
               new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_SMALL_LIGHT.ToString(), "", iniFileName));

            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_RESET_START] =
              new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_RESET_START.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_RESETING] =
              new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_RESETING.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_RESET_COMPLETE] =
              new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_RESET_COMPLETE.ToString(), "", iniFileName));

            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_CONTROLBOX] =
              new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_CONTROLBOX.ToString(), "", iniFileName));
            ADDRESSARRAY[(int)DispensingAddressEnum.ADR_POGO_PIN] =
              new AddressClass(ReadINIValue("Operation Address", DispensingAddressEnum.ADR_POGO_PIN.ToString(), "", iniFileName));

            results.AddRange(ADDRESSARRAY);
            return results;
        }



        public override void LoadData()
        {
        }
        public override void SaveData()
        {
            //>throw new System.NotImplementedException();
        }



#if (false)

        void LoadAlarms()
        {
#if (false)
#region ALARMS INI

#region 读取csv- ALARM

            string alarm0_path = INIFILE.Replace("IO.INI", "ALARMIO0.csv");
            System.IO.StreamReader _sr = null;
            try
            {
                _sr = new System.IO.StreamReader(alarm0_path);
                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON] = new PLCAlarmsClass("MW0000:MX0.0,MW0001:MX2.0,MW0002:MX4.0");
                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS] = new PLCAlarmsClass("MW0003:MX6.0");
                string strRead = string.Empty;
                while (!_sr.EndOfStream)
                {
                    strRead = _sr.ReadLine();
                    string[] strs = strRead.Split(',').ToArray();
                    if (strs.Length >= 4)
                    {
                        switch(strs[0])
                        {
                            case "COMMON":
                                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON].PLCAlarmsAddDescription("0," + strs[2] + "," + strs[3]);
                                break;
                            case "SERIOUS":
                                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS].PLCAlarmsAddDescription("0," + strs[2] + "," + strs[3]);
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
#endif
        }

        

        public bool IsAlarmsSerious
        {
            get
            {
                //AddressClass address = new AddressClass("0:MW0003");
                //return PLC[address.SiteNo].IOData.GetMW(address.Address0) > 0;
                return false;
            }
        }
        public bool IsAlarmsCommon
        {
            get
            {
                //AddressClass address = new AddressClass("0:MW0000,MW0001");
                //AddressClass address1 = new AddressClass("0:MW0002");
                //return PLC[address.SiteNo].IOData.GetMW(address.Address0) > 0 || PLC[address.SiteNo].IOData.GetMW(address.Address1) > 0
                //            || PLC[address1.SiteNo].IOData.GetMW(address1.Address0) > 0;
                return false;
            }
        }

        public bool CLEARALARMS
        {
            get
            {
                AddressClass address = new AddressClass(IOConstClass.QB1523);
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = new AddressClass(IOConstClass.QB1523);
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }

        public bool GetAlarmsAddress(int iSiteNo, string strAddress)
        {
            return PLC[iSiteNo].IOData.GetBit(strAddress);
        }

        /// <summary>
        /// 离散输入点的总数
        /// </summary>
        public int GetInputCount
        {
            get { return ADDRESSARRAY_INPUT.Length; }
        }
        /// <summary>
        /// 离散输入点的信号
        /// </summary>
        /// <param name="eindex">点位</param>
        /// <returns></returns>
        public bool GetInputIndex(int eindex)
        {
            //get
            //{
            AddressClass address = ADDRESSARRAY_INPUT[eindex];
            return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            //}
            //set
            //{
            //    AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_UVBOTTOM];
            //    PLC[address.SiteNo].SetIO(value, address.Address0);
            //}
        }
        /// <summary>
        /// 线圈读取
        /// </summary>
        /// <param name="eindex">点位</param>
        /// <returns></returns>
        public bool GetOutputIndex(int eindex)
        {
            //get
            //{
            AddressClass address = ADDRESSARRAY[eindex];
            return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            //}
            //set
            //{
            //    AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_UVBOTTOM];
            //    PLC[address.SiteNo].SetIO(value, address.Address0);
            //}
        }
        /// <summary>
        /// 线圈设定
        /// </summary>
        /// <param name="eindex">点位</param>
        /// <param name="value">开关</param>
        public void SetOutputIndex(int eindex, bool value)
        {
            //get
            //{
            //AddressClass address = ADDRESSARRAY[eindex];
            //return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            //}
            //set
            //{
            AddressClass address = ADDRESSARRAY[eindex];
            PLC[address.SiteNo].SetIO(value, address.Address0);
            //}
        }

        public void SetIO(string eAddress, bool eValue)
        {
            AddressClass address = new AddressClass(eAddress);
            PLC[address.SiteNo].SetIO(eValue, address.Address0);
        }
        public bool GetIO(string eAddress)
        {
            AddressClass address = new AddressClass(eAddress);
            return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        }


        /// <summary>
        /// 设定某地址的值
        /// </summary>
        /// <param name="eAddress">地址</param>
        /// <param name="eValue">值</param>
        public void SetMWIndex(int eAddress, int eValue)
        {
            long setH = eValue >> 16;
            long setL = eValue % 65536;
            AddressClass address = new AddressClass("0:MW" + eAddress);
            PLC[address.SiteNo].SetData(ValueToHEX(setL, 4), address.Address0);
        }
        /// <summary>
        /// 获取某地址的值
        /// </summary>
        /// <param name="eAddress">地址</param>
        /// <returns>返回 值</returns>
        public int GetMWIndex(int eAddress)
        {
            AddressClass address = new AddressClass("0:MW" + eAddress);
            return PLC[address.SiteNo].IOData.GetData(address.Address0);
        }


#region MOTOR CONTROL


#region 模组相关操作

        /// <summary>
        /// 模组位置设定
        /// </summary>
        /// <param name="eModuleIndex">模组名称</param>
        /// <param name="eIndex">0-9个位置</param>
        /// <param name="ePosString">格式X,Y,Z 数据</param>
        /// <param name="bWritePos0">是否寫入位置0 爲了防止No Ready</param>
        public void ModulePositionSet(ModuleName eModuleIndex, int eIndex, string ePosString, bool bWritePos0 = true)
        {
            JetEazy.GdxCore3.GdxCore.Trace("ModulePosition.Set", null, eModuleIndex, eIndex, ePosString);

            switch (eModuleIndex)
            {
                case ModuleName.MODULE_PICK:
                    MotorPickPosition(eIndex, ePosString);
                    if (bWritePos0)
                        MotorPickPosition(0, ePosString);
                    break;
                case ModuleName.MODULE_DISPENSING:
                    MotorDispensingPosition(eIndex, ePosString);
                    if (bWritePos0)
                        MotorDispensingPosition(0, ePosString);
                    break;
                case ModuleName.MODULE_ADJUST:
                    MotorPickCaliPosition(eIndex, ePosString);
                    if (bWritePos0)
                        MotorPickCaliPosition(0, ePosString);
                    break;
            }
        }
        /// <summary>
        /// 模组位置GO定位
        /// </summary>
        /// <param name="eModuleIndex">模组名称</param>
        /// <param name="eIndex">0-9个位置</param>
        public void ModulePositionGO(ModuleName eModuleIndex, int eIndex)
        {
            JetEazy.GdxCore3.GdxCore.Trace("ModulePosition.GO", null, eModuleIndex, eIndex);

            switch (eModuleIndex)
            {
                case ModuleName.MODULE_PICK:
                    MotorPickStart(eIndex);
                    break;
                case ModuleName.MODULE_DISPENSING:
                    MotorDispensingStart(eIndex);
                    break;
                case ModuleName.MODULE_ADJUST:
                    MotorPickCaliStart(eIndex);
                    break;
            }
        }
        /// <summary>
        /// 模组位置定位完成信号
        /// </summary>
        /// <param name="eModuleIndex">模组名称</param>
        /// <param name="eIndex">0-9个位置</param>
        /// <returns>完成信号true </returns>
        public bool ModulePositionIsComplete(ModuleName eModuleIndex, int eIndex)
        {
            JetEazy.GdxCore3.GdxCore.Trace("ModulePosition.IsComplete.Wait", null, eModuleIndex, eIndex);

            bool ret = false;
            switch (eModuleIndex)
            {
                case ModuleName.MODULE_PICK:
                    ret = MotorPickComplete(eIndex);
                    break;
                case ModuleName.MODULE_DISPENSING:
                    ret = MotorDispensingComplete(eIndex);
                    break;
                case ModuleName.MODULE_ADJUST:
                    ret = MotorPickCaliComplete(eIndex);
                    break;
            }
            return ret;

        }


        public void ModulePositionReady(ModuleName eModuleIndex, int eIndex)
        {
            JetEazy.GdxCore3.GdxCore.Trace("ModulePosition.Ready", null, eModuleIndex, eIndex);

            switch (eModuleIndex)
            {
                case ModuleName.MODULE_PICK:
                    MotorPickHomeStart(eIndex);
                    break;
                case ModuleName.MODULE_DISPENSING:
                    MotorDispensingHomeStart(eIndex);
                    break;
                case ModuleName.MODULE_ADJUST:
                    MotorPickCaliHomeStart(eIndex);
                    break;
            }
        }
        public bool ModulePositionIsReadyComplete(ModuleName eModuleIndex, int eIndex)
        {
            JetEazy.GdxCore3.GdxCore.Trace("ModulePosition.ReadyComplete.Wait", null, eModuleIndex, eIndex);

            bool ret = false;
            switch (eModuleIndex)
            {
                case ModuleName.MODULE_PICK:
                    ret = MotorPickHomeComplete(eIndex);
                    break;
                case ModuleName.MODULE_DISPENSING:
                    ret = MotorDispensingHomeComplete(eIndex);
                    break;
                case ModuleName.MODULE_ADJUST:
                    ret = MotorPickCaliHomeComplete(eIndex);
                    break;
            }
            return ret;

        }

#endregion


        /// <summary>
        /// 模组1 同动点位 位置 地址MW 1100 1120 1140 开始  0-9
        /// </summary>
        /// <param name="eindex">0-9 可以进行10个位置设定</param>
        private void MotorPickPosition(int eindex, string epos)
        {
            string[] mypos = epos.Split(',').ToArray();
            int ix = (int)(double.Parse(mypos[0]) * ONEMMSTEP);
            int iy = (int)(double.Parse(mypos[1]) * ONEMMSTEP);
            int iz = (int)(double.Parse(mypos[2]) * ONEMMSTEP);

            int iaddressx = 1100 + eindex;
            AddressClass addressx = new AddressClass("0:MW" + iaddressx.ToString());

            long setH = ix >> 16;
            long setL = ix % 65536;

            PLC[addressx.SiteNo].SetData(ValueToHEX(setL, 4), addressx.Address0);
            //PLC[addressx.SiteNo].SetData(ValueToHEX(setH, 4), addressx.Address1);

            int iaddressy = 1110 + eindex;
            AddressClass addressy = new AddressClass("0:MW" + iaddressy.ToString());

            setH = iy >> 16;
            setL = iy % 65536;

            PLC[addressy.SiteNo].SetData(ValueToHEX(setL, 4), addressy.Address0);
            //PLC[addressy.SiteNo].SetData(ValueToHEX(setH, 4), addressy.Address1);

            int iaddressz = 1120 + eindex;
            AddressClass addressz = new AddressClass("0:MW" + iaddressz.ToString());

            setH = iz >> 16;
            setL = iz % 65536;

            PLC[addressz.SiteNo].SetData(ValueToHEX(setL, 4), addressz.Address0);
            //PLC[addressz.SiteNo].SetData(ValueToHEX(setH, 4), addressz.Address1);

        }

        /// <summary>
        /// 模组1 同动点位 启动 地址1340 开始  0-9
        /// </summary>
        /// <param name="eindex">0-9 可以进行10个位置定位</param>
        private void MotorPickStart(int eindex)
        {
            int iaddress = 1400 + eindex;
            AddressClass address = new AddressClass("0:QB" + iaddress.ToString() + ".0");
            PLC[address.SiteNo].SetIO(true, address.Address0);
        }
        /// <summary>
        /// 模组1 同动点位 完成信号 地址1360 开始  0-9
        /// </summary>
        /// <param name="eindex">0-9 读取进行10个位置定位完成信号</param>
        /// <returns></returns>
        private bool MotorPickComplete(int eindex)
        {
            int iaddress = 1420 + eindex;
            AddressClass address = new AddressClass("0:QB" + iaddress.ToString() + ".0");
            //PLC[address.SiteNo].SetIO(true, address.Address0);
            JetEazy.GdxCore3.GdxCore.Trace("Motor.Pick.Complete.IO.Wait", null, address.Address0);
            return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        }

        private void MotorPickHomeStart(int eindex)
        {
            int iaddress = 1620 + eindex;
            AddressClass address = new AddressClass("0:QB" + iaddress.ToString() + ".0");
            PLC[address.SiteNo].SetIO(true, address.Address0);
        }
        private bool MotorPickHomeComplete(int eindex)
        {
            int iaddress = 1640 + eindex;
            AddressClass address = new AddressClass("0:QB" + iaddress.ToString() + ".0");
            //PLC[address.SiteNo].SetIO(true, address.Address0);
            JetEazy.GdxCore3.GdxCore.Trace("Motor.Pick.Home.Complete.IO.Wait", null, address.Address0);
            return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        }


        /// <summary>
        /// 模组2 微调 同动点位 位置 地址MW 1160 1180 1200 开始  0-9
        /// </summary>
        /// <param name="eindex">0-9 可以进行10个位置设定</param>
        private void MotorDispensingPosition(int eindex, string epos)
        {
            string[] mypos = epos.Split(',').ToArray();
            int ix = (int)(double.Parse(mypos[0]) * ONEMMSTEP);
            int iy = (int)(double.Parse(mypos[1]) * ONEMMSTEP);
            int iz = (int)(double.Parse(mypos[2]) * ONEMMSTEP);

            int iaddressx = 1130 + eindex;
            AddressClass addressx = new AddressClass("0:MW" + iaddressx.ToString());

            long setH = ix >> 16;
            long setL = ix % 65536;

            PLC[addressx.SiteNo].SetData(ValueToHEX(setL, 4), addressx.Address0);
            //PLC[addressx.SiteNo].SetData(ValueToHEX(setH, 4), addressx.Address1);

            int iaddressy = 1140 + eindex;
            AddressClass addressy = new AddressClass("0:MW" + iaddressy.ToString());

            setH = iy >> 16;
            setL = iy % 65536;

            PLC[addressy.SiteNo].SetData(ValueToHEX(setL, 4), addressy.Address0);
            //PLC[addressy.SiteNo].SetData(ValueToHEX(setH, 4), addressy.Address1);

            int iaddressz = 1150 + eindex;
            AddressClass addressz = new AddressClass("0:MW" + iaddressz.ToString());

            setH = iz >> 16;
            setL = iz % 65536;

            PLC[addressz.SiteNo].SetData(ValueToHEX(setL, 4), addressz.Address0);
            //PLC[addressz.SiteNo].SetData(ValueToHEX(setH, 4), addressz.Address1);

        }

        /// <summary>
        /// 模组2 微调 同动点位 启动 地址1380 开始  0-9
        /// </summary>
        /// <param name="eindex">0-9 可以进行10个位置定位</param>
        private void MotorDispensingStart(int eindex)
        {
            int iaddress = 1440 + eindex;
            AddressClass address = new AddressClass("0:QB" + iaddress.ToString() + ".0");
            PLC[address.SiteNo].SetIO(true, address.Address0);
        }
        /// <summary>
        /// 模组2 微调 同动点位 完成信号 地址1400 开始  0-9
        /// </summary>
        /// <param name="eindex">0-9 读取进行10个位置定位完成信号</param>
        /// <returns></returns>
        private bool MotorDispensingComplete(int eindex)
        {
            int iaddress = 1460 + eindex;
            AddressClass address = new AddressClass("0:QB" + iaddress.ToString() + ".0");
            //PLC[address.SiteNo].SetIO(true, address.Address0);
            JetEazy.GdxCore3.GdxCore.Trace("Motor.Dispensing.Complete.IO.Wait", null, address.Address0);
            return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        }

        private void MotorDispensingHomeStart(int eindex)
        {
            int iaddress = 1660 + eindex;
            AddressClass address = new AddressClass("0:QB" + iaddress.ToString() + ".0");
            PLC[address.SiteNo].SetIO(true, address.Address0);
        }
        private bool MotorDispensingHomeComplete(int eindex)
        {
            int iaddress = 1680 + eindex;
            AddressClass address = new AddressClass("0:QB" + iaddress.ToString() + ".0");
            //PLC[address.SiteNo].SetIO(true, address.Address0);
            JetEazy.GdxCore3.GdxCore.Trace("Motor.Dispensing.Home.Complete.IO.Wait", null, address.Address0);
            return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        }


        /// <summary>
        /// 模组3 点胶 同动点位 位置 地址MW 1220 1240 1260 开始  0-9
        /// </summary>
        /// <param name="eindex">0-9 可以进行10个位置设定</param>
        private void MotorPickCaliPosition(int eindex, string epos)
        {
            string[] mypos = epos.Split(',').ToArray();
            int ix = int.Parse(mypos[0]);
            int iy = int.Parse(mypos[1]);
            int iz = int.Parse(mypos[2]);

            int iaddressx = 1160 + eindex;
            AddressClass addressx = new AddressClass("0:MW" + iaddressx.ToString());

            long setH = ix >> 16;
            long setL = ix % 65536;

            PLC[addressx.SiteNo].SetData(ValueToHEX(setL, 4), addressx.Address0);
            //PLC[addressx.SiteNo].SetData(ValueToHEX(setH, 4), addressx.Address1);

            int iaddressy = 1170 + eindex;
            AddressClass addressy = new AddressClass("0:MW" + iaddressy.ToString());

            setH = iy >> 16;
            setL = iy % 65536;

            PLC[addressy.SiteNo].SetData(ValueToHEX(setL, 4), addressy.Address0);
            //PLC[addressy.SiteNo].SetData(ValueToHEX(setH, 4), addressy.Address1);

            int iaddressz = 1180 + eindex;
            AddressClass addressz = new AddressClass("0:MW" + iaddressz.ToString());

            setH = iz >> 16;
            setL = iz % 65536;

            PLC[addressz.SiteNo].SetData(ValueToHEX(setL, 4), addressz.Address0);
            //PLC[addressz.SiteNo].SetData(ValueToHEX(setH, 4), addressz.Address1);

        }

        /// <summary>
        /// 模组3 点胶 同动点位 启动 地址1420 开始  0-9
        /// </summary>
        /// <param name="eindex">0-9 可以进行10个位置定位</param>
        private void MotorPickCaliStart(int eindex)
        {
            int iaddress = 1480 + eindex;
            AddressClass address = new AddressClass("0:QB" + iaddress.ToString() + ".0");
            PLC[address.SiteNo].SetIO(true, address.Address0);
        }
        /// <summary>
        /// 模组3 点胶 同动点位 完成信号 地址1440 开始  0-9
        /// </summary>
        /// <param name="eindex">0-9 读取进行10个位置定位完成信号</param>
        /// <returns></returns>
        private bool MotorPickCaliComplete(int eindex)
        {
            int iaddress = 1500 + eindex;
            AddressClass address = new AddressClass("0:QB" + iaddress.ToString() + ".0");
            //PLC[address.SiteNo].SetIO(true, address.Address0);
            JetEazy.GdxCore3.GdxCore.Trace("Motor.PickCali.Complete.IO.Wait", null, address.Address0);
            return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        }

        private void MotorPickCaliHomeStart(int eindex)
        {
            int iaddress = 1700 + eindex;
            AddressClass address = new AddressClass("0:QB" + iaddress.ToString() + ".0");
            PLC[address.SiteNo].SetIO(true, address.Address0);
        }
        private bool MotorPickCaliHomeComplete(int eindex)
        {
            int iaddress = 1720 + eindex;
            AddressClass address = new AddressClass("0:QB" + iaddress.ToString() + ".0");
            //PLC[address.SiteNo].SetIO(true, address.Address0);
            JetEazy.GdxCore3.GdxCore.Trace("Motor.PickCali.Home.Complete.IO.Wait", null, address.Address0);
            return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        }


#endregion


#region 定义的点位操作 MASK掉 已经通过数值去控制

        public bool ADR_ISEMC
        {
            get { return !GetInputIndex(0); }
        }
        public bool ADR_ISSCREEN
        {
            get { return !GetInputIndex(9); }
        }

        //public bool UVBOTTOM
        //{
        //    get
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_UVBOTTOM];
        //        return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        //    }
        //    set
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_UVBOTTOM];
        //        PLC[address.SiteNo].SetIO(value, address.Address0);
        //    }
        //}
        //public bool FINTOP
        //{
        //    get
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_FINTOP];
        //        return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        //    }
        //    set
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_FINTOP];
        //        PLC[address.SiteNo].SetIO(value, address.Address0);
        //    }
        //}
        //public bool FINBOTTOM
        //{
        //    get
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_FINBOTTOM];
        //        return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        //    }
        //    set
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_FINBOTTOM];
        //        PLC[address.SiteNo].SetIO(value, address.Address0);
        //    }
        //}
        //public bool UVTOP
        //{
        //    get
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_UVTOP];
        //        return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        //    }
        //    set
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_UVTOP];
        //        PLC[address.SiteNo].SetIO(value, address.Address0);
        //    }
        //}
        //public bool SWITCH_ATTRACT
        //{
        //    get
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_ATTRACT];
        //        return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        //    }
        //    set
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_ATTRACT];
        //        PLC[address.SiteNo].SetIO(value, address.Address0);
        //    }
        //}
        //public bool SWITCH_MATERIALBASE
        //{
        //    get
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_MATERIALBASE];
        //        return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        //    }
        //    set
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_MATERIALBASE];
        //        PLC[address.SiteNo].SetIO(value, address.Address0);
        //    }
        //}


        //public bool ADR_SWITCH_UV
        //{
        //    get
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_UV];
        //        return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        //    }
        //    set
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_UV];
        //        PLC[address.SiteNo].SetIO(value, address.Address0);
        //    }
        //}
        //public bool ADR_SWITCH_DISPENSING
        //{
        //    get
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_DISPENSING];
        //        return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        //    }
        //    set
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_DISPENSING];
        //        PLC[address.SiteNo].SetIO(value, address.Address0);
        //    }
        //}
        //public bool ADR_SWITCH_LIGHT
        //{
        //    get
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_LIGHT];
        //        return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        //    }
        //    set
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SWITCH_LIGHT];
        //        PLC[address.SiteNo].SetIO(value, address.Address0);
        //    }
        //}
        //public bool ADR_AXIS_BREAK
        //{
        //    get
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_AXIS_BREAK];
        //        return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        //    }
        //    set
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_AXIS_BREAK];
        //        PLC[address.SiteNo].SetIO(value, address.Address0);
        //    }
        //}
        public bool ADR_RED
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_RED];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_RED];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_YELLOW
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_YELLOW];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_YELLOW];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_GREEN
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_GREEN];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_GREEN];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_BUZZER
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_BUZZER];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_BUZZER];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        //public bool ADR_ELECTROMAGNET
        //{
        //    get
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_ELECTROMAGNET];
        //        return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        //    }
        //    set
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_ELECTROMAGNET];
        //        PLC[address.SiteNo].SetIO(value, address.Address0);
        //    }
        //}
        //public bool ADR_FAN
        //{
        //    get
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_FAN];
        //        return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        //    }
        //    set
        //    {
        //        AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_FAN];
        //        PLC[address.SiteNo].SetIO(value, address.Address0);
        //    }
        //}
        public bool ADR_SMALL_LIGHT
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SMALL_LIGHT];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_SMALL_LIGHT];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }

        public bool ADR_POGO_PIN
        {
            get
            {
                return GetInputIndex(28) && !GetInputIndex(29);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_POGO_PIN];
                PLC[address.SiteNo].SetIO(value, address.Address0);
                if (!string.IsNullOrEmpty(address.Address1))
                    PLC[address.SiteNo].SetIO(!value, address.Address1);
            }
        }

#endregion

#endif
    }



    public class IOConstClass
    {
        //注释 6初始化位置 7避光槽下 8点胶位置 9避光槽上
        //          4相机计算玻璃偏移位置 5 pick吸嘴吸料位置
        //          3放入位置


        /// <summary>
        /// 手动自动模式切换地址 0手动 1自动
        /// </summary>
        public const int MW1090 = 1090;
        /// <summary>
        /// 点胶时间 单位ms
        /// </summary>
        public const int MW1091 = 1091;
        /// <summary>
        /// UV时间 单位s
        /// </summary>
        public const int MW1092 = 1092;

        /// <summary>
        /// 清除报警
        /// </summary>
        public const string QB1523 = "0:QB1523.0";

        /// <summary>
        /// 点胶启动 ON启动 OFF完成
        /// </summary>
        public const string QB1541 = "0:QB1541.0";

        /// <summary>
        /// 吸嘴吸料和到达相机测试玻璃偏移位置启动 ON启动 OFF完成
        /// </summary>
        public const string QB1542 = "0:QB1542.0";

        /// <summary>
        /// 吸嘴吸料和到达放入位置启动 ON启动 OFF完成
        /// </summary>
        public const string QB1543 = "0:QB1543.0";

        /// <summary>
        /// 避光槽位置启动 ON启动 OFF完成
        /// </summary>
        public const string QB1550 = "0:QB1550.0";

        /// <summary>
        /// 一鍵點膠測試 ON启动 OFF完成
        /// </summary>
        public const string QB1551 = "0:QB1551.0";

        public const string QB1625 = "0:QB1625.0";
        public const string QB1665 = "0:QB1665.0";
    }
}
