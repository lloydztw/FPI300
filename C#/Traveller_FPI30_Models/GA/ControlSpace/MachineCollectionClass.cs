
//#define FATEK
//#define FX3U


using JetEazy;
using VsCommon.ControlSpace.MachineSpace;

namespace VsCommon.ControlSpace
{
    public class MachineCollectionClass
    {
        VersionEnum VERSION;
        OptionEnum OPTION;

        public GeoMachineClass MACHINE;
        public MachineCollectionClass()
        {

        }

        public void Intial(VersionEnum version,OptionEnum option, GeoMachineClass machine)
        {
            VERSION = version;
            OPTION = option;

            MACHINE = machine;

            MotorSpeed();
            //WriteConfigToPLC();
            //WriteToPlcModulePosition();

            MACHINE.TriggerAction += MACHINE_TriggerAction;
        }

        private void MACHINE_TriggerAction(MachineEventEnum machineevent, object obj)
        {
            OnTrigger(machineevent);
        }


        public void MotorSpeed()
        {
            //foreach (PLCMotionClass MOTION in MACHINE.PLCMOTIONCollection)
            //{
            //    MOTION.SetSpeed(SpeedTypeEnum.HOMESLOW);
            //    MOTION.SetSpeed(SpeedTypeEnum.HOMEHIGH);
            //    MOTION.SetSpeed(SpeedTypeEnum.MANUAL);
            //    MOTION.SetSpeed(SpeedTypeEnum.GO);
            //}
        }

        public void WriteConfigToPLC()
        {

#if FATEK
            //MACHINE.PLCCollection[0].SetData(INI.Instance.linescan_Xpos_start, 106);//线扫X起点位置
            //MACHINE.PLCCollection[0].SetData(INI.Instance.linescan_Xpos_end, 108);//线扫X终点位置
            //MACHINE.PLCCollection[0].SetData(INI.Instance.linescan_Ypos_start, 302);//线扫Y起点位置
            //MACHINE.PLCCollection[0].SetData(INI.Instance.linescan_Zpos_start, 402);//线扫Z起点位置

            //MACHINE.PLCCollection[0].SetData(INI.Instance.LineScanTestSpeed, 114);//线扫速度
            //MACHINE.PLCCollection[0].SetData(INI.Instance.plc_loopCount, 60);//跑线次数
            //MACHINE.PLCCollection[0].SetData(INI.Instance.plc_quiverCount, 56);//抖动次数

            ////這裏的位置寫入需要對下plc點位

            //MACHINE.PLCCollection[0].SetData(INI.Instance.feed_ypos, 102);//
            //MACHINE.PLCCollection[0].SetData(INI.Instance.feed_zposlow, 204);//
            //MACHINE.PLCCollection[0].SetData(INI.Instance.feed_zposhigh, 202);//

            //MACHINE.PLCCollection[0].SetData(INI.Instance.take_ypos, 104);//
            //MACHINE.PLCCollection[0].SetData(INI.Instance.take_zposlow, 206);//
            //MACHINE.PLCCollection[0].SetData(INI.Instance.take_zposhigh, 208);//
#endif

#if FX3U
            //MACHINE.PLCCollection[0].SetData(INI.Instance.linescan_Xpos_start, 216);//线扫X起点位置
            //MACHINE.PLCCollection[0].SetData(INI.Instance.linescan_Xpos_end, 228);//线扫X终点位置
            //MACHINE.PLCCollection[0].SetData(INI.Instance.linescan_Ypos_start, 274);//线扫Y起点位置
            //MACHINE.PLCCollection[0].SetData(INI.Instance.linescan_Zpos_start, 304);//线扫Z起点位置

            //MACHINE.PLCCollection[0].SetData((float)INI.Instance.LineScanTestSpeed, 222);//线扫速度
            ////MACHINE.PLCCollection[0].SetData(INI.Instance.plc_loopCount, 60);//跑线次数
            ////MACHINE.PLCCollection[0].SetData(INI.Instance.plc_quiverCount, 56);//抖动次数

            ////這裏的位置寫入需要對下plc點位

            //MACHINE.PLCCollection[0].SetData(INI.Instance.feed_ypos, 214);//
            //MACHINE.PLCCollection[0].SetData(INI.Instance.feed_zposlow, 248);//
            //MACHINE.PLCCollection[0].SetData(INI.Instance.feed_zposhigh, 246);//

            //MACHINE.PLCCollection[0].SetData(INI.Instance.take_ypos, 218);//
            //MACHINE.PLCCollection[0].SetData(INI.Instance.take_zposlow, 252);//
            //MACHINE.PLCCollection[0].SetData(INI.Instance.take_zposhigh, 250);//
            //MACHINE.PLCCollection[0].SetData(INI.Instance.plc_loopCount, 22, 1);//

#endif

        }
        public void WriteToPlcRecipe()
        {
            //MACHINE.PLCCollection[0].SetData(RecipeTrayClass.Instance.chip_captureCount, 1542, 1);
            //MACHINE.PLCCollection[0].SetData((float)RecipeTrayClass.Instance.chip_linescanoffset, 1544);

        }
        public int GetMotorCount()
        {
            return MACHINE.PLCMOTIONCollection.Length;
        }


        public string GetPosition()
        {
            string posstr = "";


            return posstr;

        }
        public void GoPosition(string opstr)
        {

        }

        public void GoHome()
        {
        }

        public void Close()
        {
            MACHINE.Close();
        }
        public string PLCFps()
        {
            return MACHINE.PLCFps();
        }

        public void Tick()
        {
            MACHINE.Tick();
        }

        public delegate void TriggerHandler(MachineEventEnum machineevent);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(MachineEventEnum machineevent)
        {
            if (TriggerAction != null)
            {
                TriggerAction(machineevent);
            }
        }


    }
}
