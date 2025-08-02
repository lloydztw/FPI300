using JetEazy.BasicSpace;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.ControlSpace;
using JetEazy;
using LaserAlignDX.ControlSpace.IOSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using TravellerMINIX6.ProcessSpace;
using VsCommon.ControlSpace.MachineSpace;
using JetEazy.ControlSpace.PLCSpace;

namespace LaserAlignDX.ControlSpace.MachineSpace
{
    public class MainX2MachineClass : GeoMachineClass
    {
        const int MSDuriation = 10;

        public MainX2IOClass PLCIO;

        public MainX2MachineClass(Machine_EA machineea, string opstr, string workpath, bool isnouseplc)
        {
            IsNoUseIO = isnouseplc;

            myMachineEA = machineea;
            //VERSION = version;
            //OPTION = option;
            //CAMACT = actionMode;
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
            LightCount = int.Parse(strs[2]);

            if (PLCCount > 0)
                PLCCollection = new VsCommPLC[PLCCount];

            if (MotionCount > 0)
                PLCMOTIONCollection = new PLCMotionClass[MotionCount];

            if (LightCount > 0)
                LightCollection = new JetEazy.ControlSpace.PLCSpace.VsLight[LightCount];
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

                if (!isnouseio)
                    ret &= PLCCollection[i].Open(WORKPATH + "\\" + myMachineEA.ToString() + "\\PLCCONTROL" + i.ToString() + ".INI", isnouseio);

                PLCCollection[i].Name = "PLC" + i.ToString();
                PLCCollection[i].ReadAction += ReadAction;

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
            while (i < LightCount)
            {
                LightCollection[i] = new JetEazy.ControlSpace.PLCSpace.VsLight();

                if (!isnouseio)
                    ret &= LightCollection[i].Open(WORKPATH + "\\" + myMachineEA.ToString() + "\\LIGHT" + i.ToString() + ".INI", isnouseio);

                LightCollection[i].Name = "LIGHT" + i.ToString();
                //LightCollection[i].ReadAction += ReadAction;

                i++;
            }

            PLCIO = new MainX2IOClass();
            PLCIO.Initial(WORKPATH + "\\" + myMachineEA.ToString(), PLCCollection);

            return ret;
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
                case "Get All X":
                    PLC0GetAllX(readbuffer);
                    break;
                case "Get All Y":
                    PLC0GetAllY(readbuffer);
                    break;
            }
        }
        void PLC0GetAllX(char[] readbuffer)
        {
            String Str = new string(readbuffer, 6, 8); //X0000

            //UInt32 GetInt = HEX32(Str);
            //int i = 0;
            //while (i < 32)
            //{
            //    bool ison = ((GetInt >> i) % 2) == 1;

            //    PLCCollection[0].IOData.SetXBit(0 + i, ison);

            //    i++;
            //}

            //UInt32 GetInt = HEX32(Str.Substring(0, 4));
            int i = 0;

            while (i < Str.Length)
            {
                bool ison = Str.Substring(i, 1) == "1";

                PLCCollection[0].IOData.SetXBit(i, ison);

                i++;
            }
        }
        void PLC0GetAllY(char[] readbuffer)
        {
            String Str = new string(readbuffer, 6, 8); //Y0000
                                                       // UInt32 GetInt = HEX32(Str);
                                                       //// string Yio = Convert.ToString(GetInt, 2);
                                                       // int i = 0;
                                                       // while (i < 32)
                                                       // {
                                                       //     bool ison = ((GetInt >> i) % 2) == 1;

            //     PLCCollection[0].IOData.SetYBit(0 + i, ison);

            //     i++;
            // }

            //UInt32 GetInt = HEX32(Str.Substring(0, 4));
            int i = 0;

            while (i < Str.Length)
            {
                //bool ison = (GetInt & (1 << i)) == (1 << i);
                bool ison = Str.Substring(i, 1) == "1";

                PLCCollection[0].IOData.SetYBit(i, ison);

                i++;
            }
        }
        void PLC0GetAllMEX(char[] readbuffer)
        {
            String Str = new string(readbuffer, 6, 8); //M0048

            UInt32 GetInt = HEX32(Str);
            int i = 0;

            while (i < 32)
            {
                bool ison = ((GetInt >> i) % 2) == 1;

                PLCCollection[0].IOData.SetMBit(48 + i, ison);

                i++;
            }

            //Str = new string(readbuffer, 14, 8); //M00
            //GetInt = HEX32(Str);
            //i = 0;

            //while (i < 32)
            //{
            //    bool ison = ((GetInt >> i) % 2) == 1;

            //    PLCCollection[0].IOData.SetMBit(1040 + i, ison);
            //    i++;
            //}

            //Str = new string(readbuffer, 22, 8); //M1200
            //GetInt = HEX32(Str);
            //i = 0;

            //while (i < 32)
            //{
            //    bool ison = ((GetInt >> i) % 2) == 1;

            //    PLCCollection[0].IOData.SetMBit(1200 + i, ison);
            //    i++;
            //}

        }
        public override void Tick()
        {
            if (myJzTimes.msDuriation < MSDuriation)
                return;

            CheckEvent();

            myJzTimes.Cut();
        }

        bool IsOldStart = false;
        bool IsOldEMC = false;
        bool IsInitHiveclient = false;
        /// <summary>
        /// 记录当前的机器状态
        /// </summary>
        MachineState m_machinestate_tmp = MachineState.Running;
        public MachineState GetCurrentMachineState
        {
            get { return m_machinestate_tmp; }
        }
        public bool MachineStateForPlannedDown
        {
            set
            {
                m_machinestate_tmp = (value ? MachineState.Planned_downtime : MachineState.Idle);
                //PLCIO.Green = (int)m_machinestate_tmp == 1;
                //PLCIO.Yellow = (int)m_machinestate_tmp == 2;
                //PLCIO.Blue = (int)m_machinestate_tmp == 3;
                //PLCIO.White = (int)m_machinestate_tmp == 4;
                //PLCIO.Red = (int)m_machinestate_tmp == 5;
                //HIVECLIENT.Hiveclient_MachineState((int)m_machinestate_tmp, !IsInitHiveclient);
            }
        }
        /// <summary>
        /// 設定5色燈狀態，任何情況只有一個燈亮
        /// </summary>
        /// <param name="m_machinestate">狀態</param>
        public void SetMachineState(MachineState m_machinestate)
        {
            //將來吧
            if (m_machinestate_tmp == MachineState.Planned_downtime)
                return;

            if (m_machinestate_tmp != m_machinestate)
            {
                //PLCIO.Green = (int)m_machinestate == 1;
                //PLCIO.Yellow = (int)m_machinestate == 2;
                //PLCIO.Blue = (int)m_machinestate == 3;
                //PLCIO.White = (int)m_machinestate == 4;
                //PLCIO.Red = (int)m_machinestate == 5;

                //HIVECLIENT.Hiveclient_MachineState((int)m_machinestate, !IsInitHiveclient);

                if (!IsInitHiveclient)
                    IsInitHiveclient = true;
                //将切换的状态缓存下来，以便达到状态变化时再改变灯的颜色。
                m_machinestate_tmp = m_machinestate;
            }
        }

        public void SetLight(string lightstr)
        {
            if (lightstr == "")
            {
                PLCIO.TopLight = false;
                PLCIO.FrontLight = false;
                PLCIO.BackLight = false;
            }
            else
            {
                string[] strs = lightstr.Split(',');

                if (strs.Length < 3)
                {
                    lightstr = "1,1,1";
                    strs = lightstr.Split(',');
                }

                PLCIO.TopLight = strs[0] == "1";
                PLCIO.FrontLight = strs[1] == "1";
                PLCIO.BackLight = strs[2] == "1";

                if (strs.Length > 6)
                {
                    int ir = int.Parse(strs[3]);
                    int ig = int.Parse(strs[4]);
                    int ib = int.Parse(strs[5]);
                    int iw = int.Parse(strs[6]);
                    //LightCollection[0].SetRGBWValue(ir, ig, ib, iw);
                }

            }
        }
        public override void GoHome()
        {
        }
        public override void CheckEvent()
        {
            ////Trigger Start
            //bool isstartnow = PLCIO.IsGetImage; //PLCIO.IsStart;
            ////switch (CAMACT)
            ////{
            ////    case CameraActionMode.CAM_MOTOR_LINESCAN:
            ////    case CameraActionMode.CAM_MOTOR_MODE2:
            ////        isstartnow = PLCIO.IsGetImage;
            ////        break;
            ////    default:
            ////        break;
            ////}

            //if (isstartnow && IsOldStart != isstartnow)
            //{
            //    OnTrigger(MachineEventEnum.AUTOSTART);
            //}
            //IsOldStart = isstartnow;

            ////Trigger EMC
            //bool isemcnow = PLCIO.IsUPSError;

            //if (isemcnow && IsOldEMC != isemcnow)
            //{
            //    OnTrigger(MachineEventEnum.EMC);
            //}
            //IsOldEMC = isemcnow;

            //Check If PLC Disconnect
            foreach (VsCommPLC plc in PLCCollection)
            {
                plc.Tick();
            }
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
        }

        public override void Close()
        {
            if (PLCCollection != null)
            {
                foreach (VsCommPLC plc in PLCCollection)
                {
                    plc.Close();
                }
            }
        }
        public override string PLCFps()
        {
            string str = string.Empty;
            if (PLCCollection != null)
            {
                foreach (VsCommPLC plc in PLCCollection)
                {
                    str += plc.SerialCount.ToString() + ",";
                }
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
    }
}
