using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JetEazy;
using JetEazy.ControlSpace;
using JetEazy.ControlSpace.MotionSpace;
using MotoCan;
using JetEazy.ControlSpace.MotoCanSpace;


namespace JetEazy.ControlSpace.MotionSpace
{
    public class CanMotionClass : GeoMotionClass
    {
        CanMotoControl MOTION;
        CanDeviceMotor.CCanMotor CANMOTOR;
        bool IsReverse = false;
        //int MOTIONNO = 0;

        public override bool IsHaveBreakOption
        {
            get
            {
                return false;
            }
        }
        public override bool IsHaveSVOnOption
        {
            get
            {
                return false;
            }
        }
        public override bool IsHaveResetOption
        {
            get
            {
                return false;
            }
        }

        public override bool IsHaveRulerOption
        {
            get
            {
                return false;
            }
        }

        public CanMotionClass()
        {

        }

        //public void Initial(string path,MotionEnum motionname,CanMotoControl cammotioncontrol, int motionno)
        //{
        //    MOTIONNAME = motionname;

        //    INIFILE = path + "\\Motion" + (int)MOTIONNAME + ".INI";

        //    MOTION = cammotioncontrol;
        //    MOTIONNO = motionno;

        //    LoadData();
        //}
        public void Initial(string path, MotionEnum motionname, CanMotoControl cammotioncontrol, CanDeviceMotor.CCanMotor canmotor,bool isreverse,bool isnousemotor)
        {
            MOTIONNAME = motionname;
            IsReverse = isreverse;
            IsNoUseMotor = isnousemotor;

            INIFILE = path + "\\Motion" + (int)MOTIONNAME + ".INI";

            MOTION = cammotioncontrol;
            CANMOTOR = canmotor;

            LoadData();
        }
        public override void LoadData()
        {
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISHOME] = new FATEKAddressClass(ReadINIValue("Status Address", MotionAddressEnum.ADR_ISHOME.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISONSITE] = new FATEKAddressClass(ReadINIValue("Status Address", MotionAddressEnum.ADR_ISONSITE.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISREACHUPPERLIMIT] = new FATEKAddressClass(ReadINIValue("Status Address", MotionAddressEnum.ADR_ISREACHUPPERLIMIT.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISREACHLOWERLIMIT] = new FATEKAddressClass(ReadINIValue("Status Address", MotionAddressEnum.ADR_ISREACHLOWERLIMIT.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISSVON] = new FATEKAddressClass(ReadINIValue("Status Address", MotionAddressEnum.ADR_ISSVON.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISBREAK] = new FATEKAddressClass(ReadINIValue("Status Address", MotionAddressEnum.ADR_ISBREAK.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISERROR] = new FATEKAddressClass(ReadINIValue("Status Address", MotionAddressEnum.ADR_ISERROR.ToString(), "", INIFILE));

            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_GO] = new FATEKAddressClass(ReadINIValue("Operation Address", MotionAddressEnum.ADR_GO.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_HOME] = new FATEKAddressClass(ReadINIValue("Operation Address", MotionAddressEnum.ADR_HOME.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_FORWARD] = new FATEKAddressClass(ReadINIValue("Operation Address", MotionAddressEnum.ADR_FORWARD.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_BACKWARD] = new FATEKAddressClass(ReadINIValue("Operation Address", MotionAddressEnum.ADR_BACKWARD.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_SVON] = new FATEKAddressClass(ReadINIValue("Operation Address", MotionAddressEnum.ADR_SVON.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_BREAK] = new FATEKAddressClass(ReadINIValue("Operation Address", MotionAddressEnum.ADR_BREAK.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_RESET] = new FATEKAddressClass(ReadINIValue("Operation Address", MotionAddressEnum.ADR_RESET.ToString(), "", INIFILE));

            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_GOSPEED] = new FATEKAddressClass(ReadINIValue("Data Address", MotionAddressEnum.ADR_GOSPEED.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_MANUALSPEED] = new FATEKAddressClass(ReadINIValue("Data Address", MotionAddressEnum.ADR_MANUALSPEED.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_HOMESLOWSPEED] = new FATEKAddressClass(ReadINIValue("Data Address", MotionAddressEnum.ADR_HOMESLOWSPEED.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_HOMEHIGHSPEED] = new FATEKAddressClass(ReadINIValue("Data Address", MotionAddressEnum.ADR_HOMEHIGHSPEED.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_STEPPOSITIONNOW] = new FATEKAddressClass(ReadINIValue("Data Address", MotionAddressEnum.ADR_STEPPOSITIONNOW.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_STEPPOSITIONSET] = new FATEKAddressClass(ReadINIValue("Data Address", MotionAddressEnum.ADR_STEPPOSITIONSET.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_RULERSTEPPOSITIONNOW] = new FATEKAddressClass(ReadINIValue("Data Address", MotionAddressEnum.ADR_RULERSTEPPOSITIONNOW.ToString(), "", INIFILE));

            MOTIONTYPE = (MotionTypeEnum)Enum.Parse(typeof(MotionTypeEnum), ReadINIValue("Parameters", MotionAddressEnum.MOTIONTYPE.ToString(), MOTIONTYPE.ToString(), INIFILE), false);
            ONEMMSTEP = int.Parse(ReadINIValue("Parameters", MotionAddressEnum.ONEMMSTEP.ToString(), ONEMMSTEP.ToString(), INIFILE));
            RULERONEMMSTEP = int.Parse(ReadINIValue("Parameters", MotionAddressEnum.RULERONEMMSTEP.ToString(), RULERONEMMSTEP.ToString(), INIFILE));
            MANUALSPEED = int.Parse(ReadINIValue("Parameters", MotionAddressEnum.MANUALSPEED.ToString(), MANUALSPEED.ToString(), INIFILE));
            MANUALSLOWSPEED = int.Parse(ReadINIValue("Parameters", MotionAddressEnum.MANUALSLOWSPEED.ToString(), MANUALSLOWSPEED.ToString(), INIFILE));
            GOSPEED = int.Parse(ReadINIValue("Parameters", MotionAddressEnum.GOSPEED.ToString(), GOSPEED.ToString(), INIFILE));
            GOSLOWSPEED = int.Parse(ReadINIValue("Parameters", MotionAddressEnum.GOSLOWSPEED.ToString(), GOSLOWSPEED.ToString(), INIFILE));
            HOMEHIGHSPEED = int.Parse(ReadINIValue("Parameters", MotionAddressEnum.HOMEHIGHSPEED.ToString(), HOMEHIGHSPEED.ToString(), INIFILE));
            HOMESLOWSPEED = int.Parse(ReadINIValue("Parameters", MotionAddressEnum.HOMESLOWSPEED.ToString(), HOMESLOWSPEED.ToString(), INIFILE));
            RATIO = int.Parse(ReadINIValue("Parameters", MotionAddressEnum.RATIO.ToString(), RATIO.ToString(), INIFILE));
            READYPOSITION = float.Parse(ReadINIValue("Parameters", MotionAddressEnum.READYPOSITION.ToString(), READYPOSITION.ToString(), INIFILE));
            SOFTUPPERBOUND = float.Parse(ReadINIValue("Parameters", MotionAddressEnum.SOFTUPPERBOUND.ToString(), SOFTUPPERBOUND.ToString(), INIFILE));
            SOFTLOWERBOUND = float.Parse(ReadINIValue("Parameters", MotionAddressEnum.SOFTLOWERBOUND.ToString(), SOFTLOWERBOUND.ToString(), INIFILE));
        }
        public override void SaveData()
        {
            WriteINIValue("Parameters", MotionAddressEnum.READYPOSITION.ToString(), READYPOSITION.ToString("0.000"), INIFILE);
        }

        bool _ishome = false;
        bool isreadyhome = false;

        public override bool IsHome
        {
            get
            {
                return _ishome;
            }
            set
            {
                _ishome = value;

            }
        }
        public override bool IsOnSite
        {
            get
            {
                bool isonsite = MOTION.MotorInfo[(int)MOTIONNAME].State == 0;

                if(MOTIONNAME == MotionEnum.M1)
                {
                    MOTIONNAME = MOTIONNAME;
                }


                if(isreadyhome && isonsite)
                {
                    isreadyhome = false;
                    IsHome = true;
                }


                return isonsite;
            }
        }
        //public override bool IsOK
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
        public override bool IsSVOn
        {
            get
            {
                return false;
            }
        }
        public override bool IsBreack
        {
            get
            {
                return false;
            }
        }
        public override bool IsError
        {
            get
            {
                return false;
            }
        }
        public override string PositionNowString
        {
            get
            {
                return PositionNow.ToString("0.000");
            }
        }
        public override float PositionNow
        {
            get
            {
                return (float)MOTION.MotorInfo[(int)MOTIONNAME].CurrentStep / (float)ONEMMSTEP;
            }
        }
        public override string RulerPositionNowString
        {
            get
            {
                return RulerPositionNow.ToString("0.000");
            }
        }
        public override float RulerPositionNow
        {
            get
            {
                if (RULERONEMMSTEP == 0)
                    return 0;
                else
                    return (float)RulerStepPositionNow / (float)RULERONEMMSTEP;
            }
        }
        public override int StepPositionNow
        {
            get
            {
                //ulong ret = 0;
                Int32 ret = 0;

                //FATEKAddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_STEPPOSITIONNOW];

                //ret = PLC[address.SiteNo].IOData.GetData(address.Address1);
                //ret += (ret << 16) + PLC[address.SiteNo].IOData.GetData(address.Address0);


                //ret = HEXSigned32(ValueToHEX(PLC[address.SiteNo].IOData.GetData(address.Address1), 4) + ValueToHEX(PLC[address.SiteNo].IOData.GetData(address.Address0), 4));

                return (int)ret;
            }
        }
        public override int StepPositionSet
        {
            set
            {
                //FATEKAddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_STEPPOSITIONSET];

                //long setH = value >> 16;
                //long setL = value % 65536;

                //PLC[address.SiteNo].SetData(address.Address0, ValueToHEX(setL, 4));
                //PLC[address.SiteNo].SetData(address.Address1, ValueToHEX(setH, 4));
                //PLC[address.SiteNo].SetData(ValueToHEX(setL, 4), address.Address0);
                //PLC[address.SiteNo].SetData(ValueToHEX(setH, 4), address.Address1);
            }
        }
        public override float StepPositionSetfloat
        {
            set
            {
                //if (IsNoUseMotor)
                //{
                //    SIMPositionStep = (int)value;
                //    return;
                //}

                //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_STEPPOSITIONSET];

                ////发送给plc数据
                //byte[] hex = BitConverter.GetBytes(value);
                //byte[] h = new byte[2];
                //byte[] l = new byte[2];

                //h[0] = hex[0];
                //h[1] = hex[1];
                //l[0] = hex[2];
                //l[1] = hex[3];

                //ushort setH = BitConverter.ToUInt16(h, 0);
                //ushort setL = BitConverter.ToUInt16(l, 0);

                //PLC[address.SiteNo].SetData_ushort(setH, address.Address0);
                //PLC[address.SiteNo].SetData_ushort(setL, address.Address1);
            }
        }
        public override int RulerStepPositionNow
        {
            get
            {
                Int32 ret = 0;

                //FATEKAddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_RULERSTEPPOSITIONNOW];

                //ret += PLC[address.SiteNo].IOData.GetData(address.Address1);
                //ret += (ret << 16) + PLC[address.SiteNo].IOData.GetData(address.Address0);

                //ret = HEXSigned32(ValueToHEX(PLC[address.SiteNo].IOData.GetData(address.Address1), 4) + ValueToHEX(PLC[address.SiteNo].IOData.GetData(address.Address0), 4));

                return (int)ret;
            }
        }
        public override bool IsReachHomeBound
        {
            get
            {
                return false;
            }
        }
        public override bool IsReachUpperBound
        {
            get
            {
                return false;
            }
        }
        public override bool IsReachLowerBound
        {
            get
            {
                return false;
            }
        }
        public override void Go(float position)
        {
            uint gopos = (uint)(position * ONEMMSTEP);

            MOTION.MotorPosition(CANMOTOR, gopos);
        }
        public override void Home()
        {
            MOTION.MotorReset(CANMOTOR);


            IsHome = false;
            isreadyhome = true;

        }
        public override void SVOn()
        {
            
        }
        public override void Reset()
        {
            MOTION.MotorReset(CANMOTOR);
        }
        public override void Break()
        {
            
        }
        public override void Forward()
        {
            if(IsReverse)
                MOTION.MotorCCWSteps(CANMOTOR, 999999u);
            else
                MOTION.MotorCWSteps(CANMOTOR, 999999u);
        }
        public override void Backward()
        {
            if(IsReverse)
                MOTION.MotorCWSteps(CANMOTOR, 999999u);
            else
                MOTION.MotorCCWSteps(CANMOTOR, 999999u);
        }
        public override void Stop()
        {
            MOTION.MotorStop(CANMOTOR);
        }
        public override void SetSpeed(SpeedTypeEnum speedtype)
        {
            MotionAddressEnum motionaddress = MotionAddressEnum.ADR_HOMEHIGHSPEED;
            double speedvalue = 0;

            switch (speedtype)
            {
                case SpeedTypeEnum.HOMEHIGH:
                    motionaddress = MotionAddressEnum.ADR_HOMEHIGHSPEED;
                    speedvalue = HOMEHIGHSPEED;
                    break;
                case SpeedTypeEnum.HOMESLOW:
                    motionaddress = MotionAddressEnum.ADR_HOMESLOWSPEED;
                    speedvalue = HOMESLOWSPEED;
                    break;
                case SpeedTypeEnum.MANUALSLOW:
                    motionaddress = MotionAddressEnum.ADR_MANUALSPEED;
                    speedvalue = MANUALSLOWSPEED;
                    break;
                case SpeedTypeEnum.MANUAL:
                    motionaddress = MotionAddressEnum.ADR_MANUALSPEED;
                    speedvalue = MANUALSPEED;
                    break;
                case SpeedTypeEnum.GOSLOW:
                    motionaddress = MotionAddressEnum.ADR_GOSPEED;
                    speedvalue = GOSLOWSPEED;
                    break;
                case SpeedTypeEnum.GO:
                    motionaddress = MotionAddressEnum.ADR_GOSPEED;
                    speedvalue = GOSPEED;
                    break;
            }

            //long setH = speedvalue >> 16;
            //long setL = speedvalue % 65536;

            //FATEKAddressClass address = ADDRESSARRAY[(int)motionaddress];

            //PLC[address.SiteNo].SetData(ValueToHEX(setL, 4), address.Address0);
            //PLC[address.SiteNo].SetData(ValueToHEX(setH, 4), address.Address1);

            MOTION.MotorSetupSpeed(CANMOTOR, (ushort)speedvalue);
        }

        public override double GetSpeed(SpeedTypeEnum speedtype)
        {
            MotionAddressEnum motionaddress = MotionAddressEnum.ADR_HOMEHIGHSPEED;
            double speedvalue = 0;

            switch (speedtype)
            {
                case SpeedTypeEnum.HOMEHIGH:
                    motionaddress = MotionAddressEnum.ADR_HOMEHIGHSPEED;
                    speedvalue = HOMEHIGHSPEED;
                    break;
                case SpeedTypeEnum.HOMESLOW:
                    motionaddress = MotionAddressEnum.ADR_HOMESLOWSPEED;
                    speedvalue = HOMESLOWSPEED;
                    break;
                case SpeedTypeEnum.MANUALSLOW:
                    motionaddress = MotionAddressEnum.ADR_MANUALSPEED;
                    speedvalue = MANUALSLOWSPEED;
                    break;
                case SpeedTypeEnum.MANUAL:
                    motionaddress = MotionAddressEnum.ADR_MANUALSPEED;
                    speedvalue = MANUALSPEED;
                    break;
                case SpeedTypeEnum.GOSLOW:
                    motionaddress = MotionAddressEnum.ADR_GOSPEED;
                    speedvalue = GOSLOWSPEED;
                    break;
                case SpeedTypeEnum.GO:
                    motionaddress = MotionAddressEnum.ADR_GOSPEED;
                    speedvalue = GOSPEED;
                    break;
            }

            if (IsNoUseMotor)
            {
                SIMSpeed = (int)speedvalue;
                return SIMSpeed;
            }

            //long setH = speedvalue >> 16;
            //long setL = speedvalue % 65536;

            //FATEKAddressClass address = ADDRESSARRAY[(int)motionaddress];

            //PLC[address.SiteNo].SetData(ValueToHEX(setL, 4), address.Address0);
            //PLC[address.SiteNo].SetData(ValueToHEX(setH, 4), address.Address1);

            //FATEKAddressClass address = ADDRESSARRAY[(int)motionaddress];

            //if (!string.IsNullOrEmpty(address.Address1))
            //    speedvalue = HEXSigned32(ValueToHEX(PLC[address.SiteNo].IOData.GetData(address.Address1), 4) +
            //                                                      ValueToHEX(PLC[address.SiteNo].IOData.GetData(address.Address0), 4));
            //else
            //    speedvalue = HEXSigned32(ValueToHEX(PLC[address.SiteNo].IOData.GetData(address.Address0), 4));

            

            return speedvalue;
        }


        /// <summary>
        /// 到需要到的位置，frompos 為起始位置，offset 為起始位置的偏移值，負值為反方向。
        /// </summary>
        /// <param name="frompos">起始位置</param>
        /// <param name="offset">起始位置的偏移值</param>
        public override void Go(double frompos, double offset)
        {
            Go((float)(frompos + offset));
        }
        /// <summary>
        /// 設定正轉及反轉的速度
        /// </summary>
        /// <param name="val">速度</param>
        public override void SetManualSpeed(int val)
        {
            MANUALSPEED = val;
            SetSpeed(SpeedTypeEnum.MANUAL);
        }
        /// <summary>
        /// 設定跑到指定位置(Go)的速度。
        /// </summary>
        /// <param name="val"></param>
        public override void SetActionSpeed(int val)
        {
            GOSPEED = val;
            SetSpeed(SpeedTypeEnum.GO);
        }
        /// <summary>
        /// 取得現在的絕對位置
        /// </summary>
        /// <returns></returns>
        public override double GetPos()
        {
            return PositionNow;
        }
        /// <summary>
        /// 取得現在直線運動元件的所有狀態，為 0,0,1,1,0 的格式，之後定義。
        /// </summary>
        /// <returns></returns>
        public override string GetStatus()
        {
            return string.Empty;
        }
        /// <summary>
        /// 取得设定初始化的位置
        /// </summary>
        /// <returns></returns>
        public override double GetInitPosition()
        {
            return READYPOSITION;
        }
        public override bool IsOK
        {
            get
            {
                bool ret = IsHome;
                ret &= IsOnSite;
                return ret;
            }
        }

        public override void Go(int posindex, float position)
        {
            //AddressClass address1 = ADDRESSARRAY[(int)MotionAddressEnum.ADR_STEPPOSITIONSET];
            //AddressClass address = new AddressClass(address1.ToString(), posindex * 2);
            ////发送给plc数据
            //byte[] hex = BitConverter.GetBytes(position);
            //byte[] h = new byte[2];
            //byte[] l = new byte[2];

            //h[0] = hex[0];
            //h[1] = hex[1];
            //l[0] = hex[2];
            //l[1] = hex[3];

            //ushort setH = BitConverter.ToUInt16(h, 0);
            //ushort setL = BitConverter.ToUInt16(l, 0);

            //PLC[address.SiteNo].SetData_ushort(setH, address.Address0);
            //PLC[address.SiteNo].SetData_ushort(setL, address.Address1);


            //AddressClass address2 = ADDRESSARRAY[(int)MotionAddressEnum.ADR_GO];
            //AddressClass address3 = new AddressClass(address2.ToString(), posindex);
            //PLC[address3.SiteNo].SetIO(true, address3.Address0);
        }
        public override void SetPos(int posindex, float position)
        {
            //AddressClass address1 = ADDRESSARRAY[(int)MotionAddressEnum.ADR_STEPPOSITIONSET];
            //AddressClass address = new AddressClass(address1.ToString(), posindex * 2);
            ////发送给plc数据
            //byte[] hex = BitConverter.GetBytes(position);
            //byte[] h = new byte[2];
            //byte[] l = new byte[2];

            //h[0] = hex[0];
            //h[1] = hex[1];
            //l[0] = hex[2];
            //l[1] = hex[3];

            //ushort setH = BitConverter.ToUInt16(h, 0);
            //ushort setL = BitConverter.ToUInt16(l, 0);

            //PLC[address.SiteNo].SetData_ushort(setH, address.Address0);
            //PLC[address.SiteNo].SetData_ushort(setL, address.Address1);
        }
        public override void GoPos(int posindex)
        {
            //AddressClass address2 = ADDRESSARRAY[(int)MotionAddressEnum.ADR_GO];
            //AddressClass address3 = new AddressClass(address2.ToString(), posindex);
            //PLC[address3.SiteNo].SetIO(true, address3.Address0);
        }
        public override bool IsOnSitePos(int posindex)
        {
            return false;
            //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISONSITE];
            //if (string.IsNullOrEmpty(address.Address0))
            //    return false;
            //AddressClass address1 = new AddressClass(address.ToString(), posindex);
            //return PLC[address1.SiteNo].IOData.GetBit(address1.Address0);
        }
        public override float GetSetPos(int posindex)
        {
            //AddressClass address1 = ADDRESSARRAY[(int)MotionAddressEnum.ADR_STEPPOSITIONSET];
            //AddressClass address = new AddressClass(address1.ToString(), posindex * 2);

            //if (MotionMMMode == 1)
            //{
            //    short setH = PLC[address.SiteNo].IOData.GetMW(address.Address0);
            //    short setL = PLC[address.SiteNo].IOData.GetMW(address.Address1);

            //    //接收plc数据
            //    byte[] bytesH = BitConverter.GetBytes(setH);
            //    byte[] bytesL = BitConverter.GetBytes(setL);

            //    byte[] d = new byte[bytesH.Length + bytesL.Length];
            //    Array.Copy(bytesH, 0, d, 0, bytesH.Length);
            //    Array.Copy(bytesL, 0, d, bytesH.Length, bytesL.Length);

            //    float myFloat = BitConverter.ToSingle(d, 0);
            //    return myFloat;
            //}
            //Int32 ret = 0;
            //ret = PLC[address.SiteNo].IOData.GetData(address.Address0);
            //return (float)ret / (float)ONEMMSTEP;
            return 0;
        }
    }
}

