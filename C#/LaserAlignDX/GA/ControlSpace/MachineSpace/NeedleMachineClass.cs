using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.ControlSpace;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.ControlSpace.PLCSpace;
using System;
using System.Threading.Tasks;
using Traveller106.ControlSpace.IOSpace;
using VsCommon.ControlSpace.IOSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace VsCommon.ControlSpace.MachineSpace
{
    public class NeedleMachineClass : GeoMachineClass
    {
        const int MSDuriation = 10;

        public NeedleIOClass PLCIO;
        public EventClass EVENT;

        public NeedleMachineClass(Machine_EA machineea, string opstr, string workpath, bool isnouseplc)
        {
            IsNoUseIO = isnouseplc;

            myMachineEA = machineea;
            //VERSION = version;
            //OPTION = option;

            WORKPATH = workpath;

            GetOPString(opstr);

            MainProcess = new ProcessClass();

            myJzTimes = new JzTimes();
            myJzTimes.Cut();
        }
        public override void GetOPString(string opstr)
        {
            string[] strs = opstr.Split(',');

            PLCCount = int.Parse(strs[0]);
            MotionCount = int.Parse(strs[1]);
            if (strs.Length > 2)
                ProjectorCount = int.Parse(strs[2]);
            if (strs.Length > 3)
                BarcodeM3DCount = int.Parse(strs[3]);
            if (PLCCount > 0)
                PLCCollection = new VsCommPLC[PLCCount];

            //if (PLCCount > 0)
            //    PLCCollection = new FatekPLCClass[PLCCount];

            if (MotionCount > 0)
                PLCMOTIONCollection = new PLCMotionClass[MotionCount];
            if (ProjectorCount > 0)
                ModbusRTUClassCollection = new ModbusRTUClass[ProjectorCount];
            if (BarcodeM3DCount > 0)
                EzBarcodeM3DHelperCollection = new EzBarcodeM3DHelper[BarcodeM3DCount];
        }
        public override bool Initial(bool isnouseio, bool isnousemotor)
        {
            int i = 0;
            bool ret = true;

            IsNoUseIO = isnouseio;
            IsNoUseMotor = isnousemotor;

            i = 0;
            while (i < PLCCount)
            {
                PLCCollection[i] = new VsCommPLC();

                //@LETIAN: for off-line simulation
                if (true || !isnouseio)
                    ret &= PLCCollection[i].Open(WORKPATH + "\\" + myMachineEA.ToString() + "\\PLCCONTROL" + i.ToString() + ".INI", isnouseio);

                PLCCollection[i].Name = "PLC" + i.ToString();
                PLCCollection[i].ReadAction += ReadAction;
                PLCCollection[i].ReadListAction += DispensingMachineClass_ReadListAction;
                PLCCollection[i].ReadListUintAction += DispensingMachineClass_ReadListUintAction;
                PLCCollection[i].CommErrorStringAction += DispensingPLC_CommErrorStringAction;
                PLCCollection[i].ReadListUshortAction += MainLSMachineClass_ReadListUshortAction;
                i++;
            }

            i = 0;
            while (i < MotionCount)
            {
                PLCMOTIONCollection[i] = new PLCMotionClass();
                PLCMOTIONCollection[i].Intial(WORKPATH + "\\" + myMachineEA.ToString(), (MotionEnum)i, PLCCollection, IsNoUseMotor);

                i++;
            }
            i = 0;
            while (i < ProjectorCount)
            {
                ModbusRTUClassCollection[i] = new ModbusRTUClass();

                //@LETIAN: for off-line simulation
                if (true || !isnouseio)
                    ret &= ModbusRTUClassCollection[i].Open(WORKPATH + "\\" + myMachineEA.ToString() + "\\PROJECTORCONTROL" + i.ToString() + ".INI", isnouseio);

                ModbusRTUClassCollection[i].Name = "PROJECTOR" + i.ToString();
                i++;
            }

            i = 0;
            while (i < BarcodeM3DCount)
            {
                EzBarcodeM3DHelperCollection[i] = new EzBarcodeM3DHelper();

                //@LETIAN: for off-line simulation
                if (true || !isnouseio)
                    ret &= EzBarcodeM3DHelperCollection[i].Open(WORKPATH + "\\" + myMachineEA.ToString() + "\\BarcodeCONTROL" + i.ToString() + ".INI", isnouseio);

                EzBarcodeM3DHelperCollection[i].Name = "Barcode" + i.ToString();
                i++;
            }

            PLCIO = new NeedleIOClass();
            PLCIO.Initial(WORKPATH + "\\" + myMachineEA.ToString(), PLCCollection);

            //@LETIAN: 2022/07/05 加入 EVENTClass 成員 logPath 初始化設定
            //          (目前只用於第三站)
            EVENT = new EventClass(WORKPATH + "\\EVENT.jdb", Traveller106.Universal.LOG_ALARM_EVENT_PATH);

            return ret;
        }

        private void MainLSMachineClass_ReadListUshortAction(ushort[] readbuffer, string operationstring, string myname)
        {
            switch (myname)
            {
                case "PLC0":
                    PLC0ReadAction(readbuffer, operationstring);
                    break;
            }
        }
        void PLC0ReadAction(ushort[] readbuffer, string operationstring)
        {
            if (operationstring.Contains("Get MW"))
            {
                string[] vs = operationstring.Split('W');
                PLC0GetMWFX(readbuffer, int.Parse(vs[1]));
            }

            //switch (operationstring)
            //{
               
            //    case "Get MW1400":
            //        PLC0GetMWFX(readbuffer, 1400);
            //        break;
            //    case "Get MW1060":
            //        PLC0GetMWFX(readbuffer, 1060);
            //        break;
            //    case "Get MW1540":
            //        PLC0GetMWFX(readbuffer, 1540);
            //        break;
            //    case "Get MW1636":
            //        PLC0GetMWFX(readbuffer, 1636);
            //        break;
            //    case "Get MW2016":
            //        PLC0GetMWFX(readbuffer, 2016);
            //        break;
            //    case "Get MW0":
            //        PLC0GetMWFX(readbuffer, 0);
            //        break;
            //}
        }
        void PLC0GetMWFX(ushort[] readbuffer, int index)
        {
            int i = 0;
            while (i < readbuffer.Length)
            {
                ushort ison = readbuffer[i];
                PLCCollection[0].IOData.SetMWData(index + i, (short)ison);

                int j = 0;
                while (j < 16)
                {
                    bool isonj = ((ison >> j) % 2) == 1;

                    PLCCollection[0].IOData.SetMXBit(index + i * 16 + j, isonj);

                    j++;
                }

                i++;
            }
        }

        private void DispensingPLC_CommErrorStringAction(string str)
        {
            MachineCommError(str);
        }

        private void DispensingMachineClass_ReadListUintAction(short[] readbuffer, string operationstring, string myname)
        {
            switch (myname)
            {
                case "PLC0":
                    PLC0ReadAction(readbuffer, operationstring);
                    break;
            }
        }
        void PLC0ReadAction(short[] readbuffer, string operationstring)
        {
            //if (operationstring.Contains("Get MW"))
            //{
            //    string[] vs = operationstring.Split('W');
            //    PLC0GetMWFX(int.Parse(vs[1]), readbuffer);
            //}
            //switch (operationstring)
            //{
            //    case "Get MW0":
            //        PLC0GetMW0000(readbuffer);
            //        break;
            //    case "Get MW1000":
            //        PLC0GetMW1000(readbuffer);
            //        break;
            //    case "Get MW1300":
            //        PLC0GetMW1300(readbuffer);
            //        break;
            //    case "Get MW1320":
            //        PLC0GetMW1320(readbuffer);
            //        break;
            //}
        }
        void PLC0GetMW0000(short[] readbuffer)
        {
            int i = 0;
            while (i < readbuffer.Length)
            {
                short ison = readbuffer[i];
                PLCCollection[0].IOData.SetMWData(0 + i, ison);

                //UInt32 GetInt = ison;
                int j = 0;
                while (j < 16)
                {
                    bool isonj = ((ison >> j) % 2) == 1;

                    PLCCollection[0].IOData.SetMXBit(0 + i * 16 + j, isonj);

                    j++;
                }

                i++;
            }
        }
        void PLC0GetMW1000(short[] readbuffer)
        {
            int i = 0;
            while (i < readbuffer.Length)
            {
                short ison = readbuffer[i];
                PLCCollection[0].IOData.SetMWData(1000 + i, ison);

                i++;
            }
        }
        void PLC0GetMW1300(short[] readbuffer)
        {
            int i = 0;
            while (i < readbuffer.Length)
            {
                short ison = readbuffer[i];
                PLCCollection[0].IOData.SetMWData(1300 + i, ison);

                i++;
            }
        }
        void PLC0GetMW1320(short[] readbuffer)
        {
            int i = 0;
            while (i < readbuffer.Length)
            {
                short ison = readbuffer[i];
                PLCCollection[0].IOData.SetMWData(1320 + i, ison);

                i++;
            }
        }

        private void DispensingMachineClass_ReadListAction(bool[] readbuffer, string operationstring, string myname)
        {
            switch (myname)
            {
                case "PLC0":
                    PLC0ReadAction(readbuffer, operationstring);
                    break;
            }
        }
        void PLC0ReadAction(bool[] readbuffer, string operationstring)
        {
            if (operationstring.Contains("Get QX"))
            {
                string[] vs = operationstring.Split('X');
                PLC0GetQXFX(int.Parse(vs[1]), readbuffer);
            }
            else if (operationstring.Contains("Get IX"))
            {
                //string[] vs = operationstring.Split('X');
                PLC0GetIX0(readbuffer);
            }
            //switch (operationstring)
            //{
            //    case "Get QX0":
            //        PLC0GetQX0(readbuffer);
            //        break;
            //    case "Get QX1016":
            //        PLC0GetQX1016(readbuffer);
            //        break;
            //    case "Get QX1144":
            //        PLC0GetQXFX(1144, readbuffer);
            //        break;
            //    case "Get QX1272":
            //        PLC0GetQXFX(1272, readbuffer);
            //        break;
            //    case "Get QX1400":
            //        PLC0GetQXFX(1400, readbuffer);
            //        break;
            //    case "Get QX1528":
            //        PLC0GetQXFX(1528, readbuffer);
            //        break;
            //    case "Get QX1656":
            //        PLC0GetQXFX(1656, readbuffer);
            //        break;
            //    case "Get QX1784":
            //        PLC0GetQXFX(1784, readbuffer);
            //        break;
            //    case "Get QX2296":
            //        PLC0GetQXFX(2296, readbuffer);
            //        break;
            //    case "Get IX0":
            //        PLC0GetIX0(readbuffer);
            //        break;
            //}
        }
        void PLC0GetQXFX(int iaddress, bool[] readbuffer)
        {

            int i = 0;
            while (i < readbuffer.Length)
            {
                bool ison = readbuffer[i];
                PLCCollection[0].IOData.SetQXBit(iaddress * 8 + i, ison);

                i++;
            }
        }
        void PLC0GetIX0(bool[] readbuffer)
        {

            int i = 0;
            while (i < readbuffer.Length)
            {
                bool ison = readbuffer[i];
                PLCCollection[0].IOData.SetIXBit(0 * 8 + i, ison);

                i++;
            }
        }

        private void ReadAction(char[] readbuffer, string operationstring, string myname)
        {
            switch (myname)
            {
                case "PLC0":
                    PLC0ReadAction(readbuffer, operationstring);
                    break;
            }
        }
        void PLC0ReadAction(char[] readbuffer, string operationstring)
        {
            switch (operationstring)
            {
                case "Get All M":
                    PLC0GetAllMEX(readbuffer);
                    break;
                case "Get All M1":
                    PLC0GetAllMEX1(readbuffer);
                    break;
                case "Get All X":
                    PLC0GetAllX(readbuffer);
                    break;
                case "Get All Y":
                    PLC0GetAllY(readbuffer);
                    break;
                case "Get Data P1X":
                    PLC0GetData(readbuffer, 102, 2);
                    break;
                case "Get Data P2X":
                    PLC0GetData(readbuffer, 202, 2);
                    break;
                case "Get Data P3X":
                    PLC0GetData(readbuffer, 302, 2);
                    break;
                case "Get Data SP1X":
                    PLC0GetData(readbuffer, 104, 2);
                    break;
                case "Get Data SP2X":
                    PLC0GetData(readbuffer, 204, 2);
                    break;
                case "Get Data SP3X":
                    PLC0GetData(readbuffer, 304, 2);
                    break;
            }
        }
        void PLC0GetAllX(char[] readbuffer)
        {
            String Str = new string(readbuffer, 6, 10); //X0000

            UInt32 GetInt = HEX32(Str);
            int i = 0;
            while (i < 32)
            {
                bool ison = ((GetInt >> i) % 2) == 1;

                PLCCollection[0].IOData.SetXBit(0 + i, ison);

                i++;
            }

            //UInt32 GetInt = HEX32(Str.Substring(0, 4));
            //int i = 0;

            //while (i < Str.Length)
            //{
            //    bool ison = Str.Substring(i, 1) == "1";

            //    PLCCollection[0].IOData.SetXBit(i, ison);

            //    i++;
            //}
        }
        void PLC0GetAllY(char[] readbuffer)
        {
            String Str = new string(readbuffer, 6, 10); //Y0000
            UInt32 GetInt = HEX32(Str);
            // string Yio = Convert.ToString(GetInt, 2);
            int i = 0;
            while (i < 32)
            {
                bool ison = ((GetInt >> i) % 2) == 1;

                PLCCollection[0].IOData.SetYBit(0 + i, ison);

                i++;
            }

            //UInt32 GetInt = HEX32(Str.Substring(0, 4));
            //int i = 0;

            //while (i < Str.Length)
            //{
            //    //bool ison = (GetInt & (1 << i)) == (1 << i);
            //    bool ison = Str.Substring(i, 1) == "1";

            //    PLCCollection[0].IOData.SetYBit(i, ison);

            //    i++;
            //}
        }
        void PLC0GetAllMEX(char[] readbuffer)
        {
            String Str = new string(readbuffer, 6, 8); //M0048

            UInt32 GetInt = HEX32(Str);
            int i = 0;

            while (i < 32)
            {
                bool ison = ((GetInt >> i) % 2) == 1;

                PLCCollection[0].IOData.SetMBit(0 + i, ison);

                i++;
            }

            Str = new string(readbuffer, 14, 8); //M00
            GetInt = HEX32(Str);
            i = 0;

            while (i < 32)
            {
                bool ison = ((GetInt >> i) % 2) == 1;

                PLCCollection[0].IOData.SetMBit(96 + i, ison);
                i++;
            }

            Str = new string(readbuffer, 22, 8); //M1200
            GetInt = HEX32(Str);
            i = 0;

            while (i < 32)
            {
                bool ison = ((GetInt >> i) % 2) == 1;

                PLCCollection[0].IOData.SetMBit(192 + i, ison);
                i++;
            }

            Str = new string(readbuffer, 30, 8); //M1200
            GetInt = HEX32(Str);
            i = 0;

            while (i < 32)
            {
                bool ison = ((GetInt >> i) % 2) == 1;

                PLCCollection[0].IOData.SetMBit(224 + i, ison);
                i++;
            }

            Str = new string(readbuffer, 38, 8); //M1200
            GetInt = HEX32(Str);
            i = 0;

            while (i < 32)
            {
                bool ison = ((GetInt >> i) % 2) == 1;

                PLCCollection[0].IOData.SetMBit(304 + i, ison);
                i++;
            }

        }
        void PLC0GetAllMEX1(char[] readbuffer)
        {
            String Str = new string(readbuffer, 6, 8); //M0048

            UInt32 GetInt = HEX32(Str);
            int i = 0;

            while (i < 32)
            {
                bool ison = ((GetInt >> i) % 2) == 1;

                PLCCollection[0].IOData.SetMBit(32 + i, ison);

                i++;
            }
        }
        void PLC0GetData(char[] readbuffer, int offsetaddress, int wordcount)
        {
            String Str = new string(readbuffer, 6, 4 * wordcount);
            UInt16 GetInt = 0;
            int i = 0;
            while (i < wordcount)
            {
                GetInt = HEX16(Str.Substring(i * 4, 4));
                PLCCollection[0].IOData.SetDData(offsetaddress + i, (int)GetInt);

                i++;
            }
        }

        public override void Tick()
        {
            if (myJzTimes.msDuriation < MSDuriation)
                return;

            CheckEvent();

            myJzTimes.Cut();
        }

        public override void GoHome()
        {
        }


        #region Alarms Define

        bool AlarmWarningTrigered = false;
        int AlarmWarningnow = 0;
        public int IsAlarmWarning
        {
            get
            {
                return AlarmWarningnow;
            }
            set
            {
                if (AlarmWarningnow != value)
                {
                    if (value != 0)
                        AlarmWarningTrigered = true;
                    else
                        AlarmWarningTrigered = false;

                    AlarmWarningnow = value;
                }
                else
                    AlarmWarningTrigered = false;
            }
        }

        bool AlarmSeriousTrigered = false;
        int AlarmSeriousnow = 0;
        public int IsAlarmSerious
        {
            get
            {
                return AlarmSeriousnow;
            }
            set
            {
                if (AlarmSeriousnow != value)
                {
                    if (value != 0)
                        AlarmSeriousTrigered = true;
                    else
                        AlarmSeriousTrigered = false;

                    AlarmSeriousnow = value;
                }
                else
                    AlarmSeriousTrigered = false;
            }
        }

        bool AlarmCommonTrigered = false;
        int AlarmCommonnow = 0;
        public int IsAlarmCommon
        {
            get
            {
                return AlarmCommonnow;
            }
            set
            {
                if (AlarmCommonnow != value)
                {
                    if (value != 0)
                        AlarmCommonTrigered = true;
                    else
                        AlarmCommonTrigered = false;

                    AlarmCommonnow = value;
                }
                else
                    AlarmCommonTrigered = false;
            }
        }

        bool EMCTrigered = false;
        bool IsEMCnow = false;
        public bool IsEMC
        {
            get
            {
                return IsEMCnow;
            }
            set
            {
                if (IsEMCnow != value)
                {
                    if (value)
                        EMCTrigered = true;
                    else
                        EMCTrigered = false;

                    IsEMCnow = value;
                }
                else
                    EMCTrigered = false;
            }
        }

        bool m_ClearAlarming = false;
        public bool ClearAlarm
        {
            set
            {
                if (value)
                {
                    AlarmSeriousnow = 0;
                    AlarmCommonnow = 0;

                    m_ClearAlarming = true;
                }
                Task task = new Task(() =>
                {
                    while (m_ClearAlarming)
                    {
                        //PLCIO.CLEARALARMS = true;
                        //System.Threading.Thread.Sleep(200);
                        //PLCIO.CLEARALARMS = false;

                        m_ClearAlarming = false;
                    }
                });
                task.Start();
            }
        }

        #endregion

        public override void CheckEvent()
        {
            if (m_ClearAlarming)
                return;

            //IsAlarmWarning = PLCIO.IntAlarmsWarning;
            //if (AlarmWarningTrigered)
            //{
            //    OnTrigger(MachineEventEnum.ALARM_WARNING);
            //}

            //IsAlarmSerious = PLCIO.IntAlarmsSerious;
            ////if (AlarmSeriousTrigered || IsAlarmSerious)
            //if (AlarmSeriousTrigered)
            //{
            //    OnTrigger(MachineEventEnum.ALARM_SERIOUS);
            //}

            //IsAlarmCommon = PLCIO.IntAlarmsCommon;
            ////if (AlarmCommonTrigered || IsAlarmCommon)
            //if (AlarmCommonTrigered)
            //{
            //    OnTrigger(MachineEventEnum.ALARM_COMMON);
            //}

            IsEMC = PLCIO.ADR_ISEMC;// || PLCIO.ADR_ISSCREEN;
            if (EMCTrigered)
            {
                OnTrigger(MachineEventEnum.EMC);
            }

            foreach (VsCommPLC plc in PLCCollection)
            {
                plc.Tick();
            }
            //foreach (ModbusRTUClass plc in ModbusRTUClassCollection)
            //{
            //    plc.Tick();
            //}
        }
        public override void GetStart(bool isdirect, bool isnouseplc)
        {
            throw new NotImplementedException();
        }
        public override void SetDelayTime()
        {
            throw new NotImplementedException();
        }
        public override void MainProcessTick()
        {
            throw new NotImplementedException();
        }
        public void PLCRetry()
        {
            foreach (VsCommPLC plc in PLCCollection)
            {
                plc.RetryConn();
            }
            //foreach (ModbusRTUClass plc in ModbusRTUClassCollection)
            //{
            //    plc.RetryConn();
            //}
        }

        public override void Close()
        {
            foreach (VsCommPLC plc in PLCCollection)
            {
                plc.Close();
            }
            //foreach (ModbusRTUClass plc in ModbusRTUClassCollection)
            //{
            //    plc.Close();
            //}
        }
        public override string PLCFps()
        {
            string str = string.Empty;
            foreach (VsCommPLC plc in PLCCollection)
            {
                str += plc.SerialCount.ToString() + ",";
            }
            return str;
        }

        public override void SetNormalTemp(bool ebTemp)
        {
            foreach (VsCommPLC plc in PLCCollection)
            {
                plc.SetNormalTemp(ebTemp);
            }
        }

        public delegate void MachineCommErrorStringHandler(string str);
        public event MachineCommErrorStringHandler MachineCommErrorStringAction;
        public void MachineCommError(string str)
        {
            if (MachineCommErrorStringAction != null)
            {
                MachineCommErrorStringAction(str);
            }
        }
    }
}
