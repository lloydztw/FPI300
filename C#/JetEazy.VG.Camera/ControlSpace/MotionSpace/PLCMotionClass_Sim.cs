using JetEazy.ControlSpace.PLCSpace;
using System;
using System.Collections.Generic;

namespace JetEazy.ControlSpace.MotionSpace
{
    public partial class PLCMotionSim : PLCMotionClass
    {
        public override bool IsHaveBreakOption
        {
            get
            {
                //return ADDRESSARRAY[(int)MotionAddressEnum.ADR_BREAK].SiteNo != -1;
                return false;
            }
        }
        public override bool IsHaveSVOnOption
        {
            get
            {
                //return ADDRESSARRAY[(int)MotionAddressEnum.ADR_SVON].SiteNo != -1;
                return false;
            }
        }
        public override bool IsHaveResetOption
        {
            get
            {
                //return ADDRESSARRAY[(int)MotionAddressEnum.ADR_RESET].SiteNo != -1;
                return false;
            }
        }
        public override bool IsHaveRulerOption
        {
            get
            {
                //return ADDRESSARRAY[(int)MotionAddressEnum.ADR_RULERSTEPPOSITIONNOW].SiteNo != -1;
                return false;
            }
        }

        public override void Intial(string path, MotionEnum motionname, VsCommPLC[] plc, bool isnousemotor)
        {
            // 模擬
            isnousemotor = true;
            base.Intial(path, motionname, plc, isnousemotor);
            // 再次確保是模擬模式
            IsNoUseMotor = true;
            // 模擬 1mm = 1000 脈波數 (定位精度 0.001 mm)
            ONEMMSTEP = 1000;
        }

        public override void LoadData()
        {
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISHOME] = new AddressClass(ReadINIValue("Status Address", MotionAddressEnum.ADR_ISHOME.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISONSITE] = new AddressClass(ReadINIValue("Status Address", MotionAddressEnum.ADR_ISONSITE.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISREACHUPPERLIMIT] = new AddressClass(ReadINIValue("Status Address", MotionAddressEnum.ADR_ISREACHUPPERLIMIT.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISREACHLOWERLIMIT] = new AddressClass(ReadINIValue("Status Address", MotionAddressEnum.ADR_ISREACHLOWERLIMIT.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISSVON] = new AddressClass(ReadINIValue("Status Address", MotionAddressEnum.ADR_ISSVON.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISBREAK] = new AddressClass(ReadINIValue("Status Address", MotionAddressEnum.ADR_ISBREAK.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISERROR] = new AddressClass(ReadINIValue("Status Address", MotionAddressEnum.ADR_ISERROR.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISREACHHOME] = new AddressClass(ReadINIValue("Status Address", MotionAddressEnum.ADR_ISREACHHOME.ToString(), "", INIFILE));


            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_GO] = new AddressClass(ReadINIValue("Operation Address", MotionAddressEnum.ADR_GO.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_HOME] = new AddressClass(ReadINIValue("Operation Address", MotionAddressEnum.ADR_HOME.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_FORWARD] = new AddressClass(ReadINIValue("Operation Address", MotionAddressEnum.ADR_FORWARD.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_BACKWARD] = new AddressClass(ReadINIValue("Operation Address", MotionAddressEnum.ADR_BACKWARD.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_SVON] = new AddressClass(ReadINIValue("Operation Address", MotionAddressEnum.ADR_SVON.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_BREAK] = new AddressClass(ReadINIValue("Operation Address", MotionAddressEnum.ADR_BREAK.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_RESET] = new AddressClass(ReadINIValue("Operation Address", MotionAddressEnum.ADR_RESET.ToString(), "", INIFILE));

            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_GOSPEED] = new AddressClass(ReadINIValue("Data Address", MotionAddressEnum.ADR_GOSPEED.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_MANUALSPEED] = new AddressClass(ReadINIValue("Data Address", MotionAddressEnum.ADR_MANUALSPEED.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_HOMESLOWSPEED] = new AddressClass(ReadINIValue("Data Address", MotionAddressEnum.ADR_HOMESLOWSPEED.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_HOMEHIGHSPEED] = new AddressClass(ReadINIValue("Data Address", MotionAddressEnum.ADR_HOMEHIGHSPEED.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_STEPPOSITIONNOW] = new AddressClass(ReadINIValue("Data Address", MotionAddressEnum.ADR_STEPPOSITIONNOW.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_STEPPOSITIONSET] = new AddressClass(ReadINIValue("Data Address", MotionAddressEnum.ADR_STEPPOSITIONSET.ToString(), "", INIFILE));
            //ADDRESSARRAY[(int)MotionAddressEnum.ADR_RULERSTEPPOSITIONNOW] = new AddressClass(ReadINIValue("Data Address", MotionAddressEnum.ADR_RULERSTEPPOSITIONNOW.ToString(), "", INIFILE));

            //MOTIONTYPE = (MotionTypeEnum)Enum.Parse(typeof(MotionTypeEnum), ReadINIValue("Parameters", MotionAddressEnum.MOTIONTYPE.ToString(), MOTIONTYPE.ToString(), INIFILE), false);
            //ONEMMSTEP = int.Parse(ReadINIValue("Parameters", MotionAddressEnum.ONEMMSTEP.ToString(), ONEMMSTEP.ToString(), INIFILE));
            //RULERONEMMSTEP = int.Parse(ReadINIValue("Parameters", MotionAddressEnum.RULERONEMMSTEP.ToString(), RULERONEMMSTEP.ToString(), INIFILE));
            //MANUALSPEED = double.Parse(ReadINIValue("Parameters", MotionAddressEnum.MANUALSPEED.ToString(), MANUALSPEED.ToString(), INIFILE));
            //MANUALSLOWSPEED = double.Parse(ReadINIValue("Parameters", MotionAddressEnum.MANUALSLOWSPEED.ToString(), MANUALSLOWSPEED.ToString(), INIFILE));
            //GOSPEED = double.Parse(ReadINIValue("Parameters", MotionAddressEnum.GOSPEED.ToString(), GOSPEED.ToString(), INIFILE));
            //GOSLOWSPEED = double.Parse(ReadINIValue("Parameters", MotionAddressEnum.GOSLOWSPEED.ToString(), GOSLOWSPEED.ToString(), INIFILE));
            //HOMEHIGHSPEED = double.Parse(ReadINIValue("Parameters", MotionAddressEnum.HOMEHIGHSPEED.ToString(), HOMEHIGHSPEED.ToString(), INIFILE));
            //HOMESLOWSPEED = double.Parse(ReadINIValue("Parameters", MotionAddressEnum.HOMESLOWSPEED.ToString(), HOMESLOWSPEED.ToString(), INIFILE));
            //RATIO = int.Parse(ReadINIValue("Parameters", MotionAddressEnum.RATIO.ToString(), RATIO.ToString(), INIFILE));
            //READYPOSITION = float.Parse(ReadINIValue("Parameters", MotionAddressEnum.READYPOSITION.ToString(), READYPOSITION.ToString(), INIFILE));
            //SOFTUPPERBOUND = float.Parse(ReadINIValue("Parameters", MotionAddressEnum.SOFTUPPERBOUND.ToString(), SOFTUPPERBOUND.ToString(), INIFILE));
            //SOFTLOWERBOUND = float.Parse(ReadINIValue("Parameters", MotionAddressEnum.SOFTLOWERBOUND.ToString(), SOFTLOWERBOUND.ToString(), INIFILE));

            //MOTIONALIAS = ReadINIValue("Parameters", "MOTIONALIAS", MOTIONNAME.ToString(), INIFILE);
            //MOTIONUNIT = ReadINIValue("Parameters", "MOTIONUNIT", MOTIONUNIT.ToString(), INIFILE);
            
            base.LoadData();
        }
        public override void SaveData()
        {
            // 模擬模式 暫時 不改變原來的 INI
            //base.SaveData();
        }

        public override bool IsHome
        {
            get
            {
                //return _simCurrentPositionStep == 0 || IsOnSite;
                return _hasBeenHome;
            }
            set
            {
            }
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

        /// <summary>
        /// INP
        /// </summary>
        public override bool IsOnSite
        {
            get
            {
                //return base.IsOnSite;
                return _simCurrentPositionStep == _simTargetPositionStep;
            }
        }
        public override bool IsSVOn
        {
            get
            {
                //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISSVON];
                //if (string.IsNullOrEmpty(address.Address0))
                //    return false;
                //return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
                //return PLC[address.SiteNo].IOData.GetBit(address.Address0);
                return true;
            }
        }
        public override bool IsBreack
        {
            get
            {
                //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISBREAK];
                //if (string.IsNullOrEmpty(address.Address0))
                //    return false;
                //return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
                //return !PLC[address.SiteNo].IOData.GetBit(address.Address0);
                return false;
            }
        }
        public override bool IsError
        {
            get
            {
                //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISERROR];
                //if (string.IsNullOrEmpty(address.Address0))
                //    return false;
                //return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
                //return PLC[address.SiteNo].IOData.GetBit(address.Address0);
                return false;
            }
        }
        
        public override string PositionNowString
        {
            get
            {
                //return PositionNow.ToString();
                return PositionNow.ToString("0.000");
            }
        }
        public override float PositionNow
        {
            get
            {
                //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_STEPPOSITIONNOW];
                //if (address.SiteNo < 0)
                //    return 0f;

                //string readvari = PLC[address.SiteNo].ReadVari(address.Address0).ToLower();
                //if (string.IsNullOrEmpty(readvari))
                //    return 0f;

                //float.TryParse(readvari, out var stepposition);
                //return stepposition;

                ////if (MotionMMMode == 1)
                ////{
                ////    AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_STEPPOSITIONNOW];

                ////    short setH = PLC[address.SiteNo].IOData.GetMW(address.Address0);
                ////    short setL = PLC[address.SiteNo].IOData.GetMW(address.Address1);

                ////    //接收plc数据
                ////    byte[] bytesH = BitConverter.GetBytes(setH);
                ////    byte[] bytesL = BitConverter.GetBytes(setL);

                ////    byte[] d = new byte[bytesH.Length + bytesL.Length];
                ////    Array.Copy(bytesH, 0, d, 0, bytesH.Length);
                ////    Array.Copy(bytesL, 0, d, bytesH.Length, bytesL.Length);

                ////    float myFloat = BitConverter.ToSingle(d, 0);
                ////    return myFloat;
                ////}
                ////else if (MotionMMMode == 2)
                ////{
                ////    AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_STEPPOSITIONNOW];
                ////    return float.Parse(PLC[address.SiteNo].ReadVari(address.Address0).ToLower());
                ////}

                ////return (float)StepPositionNow / (float)ONEMMSTEP;
                ///

                if (ONEMMSTEP == 0)
                    return 0;
                double pos = (double)StepPositionNow / ONEMMSTEP;
                return (float)pos;
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
                //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_RULERSTEPPOSITIONNOW];
                //if (address.SiteNo != -1)
                //    return float.Parse(PLC[address.SiteNo].ReadVari(address.Address0).ToLower());
                //return 0;
                ////if (MotionMMMode == 1)
                ////{
                ////    AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_RULERSTEPPOSITIONNOW];

                ////    short setH = PLC[address.SiteNo].IOData.GetMW(address.Address0);
                ////    short setL = PLC[address.SiteNo].IOData.GetMW(address.Address1);

                ////    //接收plc数据
                ////    byte[] bytesH = BitConverter.GetBytes(setH);
                ////    byte[] bytesL = BitConverter.GetBytes(setL);

                ////    byte[] d = new byte[bytesH.Length + bytesL.Length];
                ////    Array.Copy(bytesH, 0, d, 0, bytesH.Length);
                ////    Array.Copy(bytesL, 0, d, bytesH.Length, bytesL.Length);

                ////    float myFloat = BitConverter.ToSingle(d, 0);
                ////    return myFloat;
                ////}

                if (RULERONEMMSTEP == 0)
                    return 0;
                else
                    return (float)RulerStepPositionNow / (float)RULERONEMMSTEP;
            }
        }
        public override int RulerStepPositionNow
        {
            get => 0;
        }
        
        public override int StepPositionNow
        {
            get => _simCurrentPositionStep;
        }

        /// <summary>
        /// 貌似沒用到
        /// </summary>
        public override int StepPositionSet
        {
            set
            {
                //SIMPositionStep = value;
                double pos = value * ONEMMSTEP;
                simSetTargetPos((float)pos);
            }
        }
        public override float StepPositionSetfloat
        {
            set
            {
                //此處 StepPositionSetfloat 的語意已經變成, 直接設定 目的地 的 物理位置 (mm) 而非 脈波數
                simSetTargetPos(value);
            }
        }

        public override bool IsReachHomeBound
        {
            get
            {
                //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISREACHHOME];
                //if (string.IsNullOrEmpty(address.Address0))
                //    return false;
                //return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
                //return !PLC[address.SiteNo].IOData.GetBit(address.Address0);

                return _simCurrentPositionStep == 0;
            }
        }
        public override bool IsReachUpperBound
        {
            get
            {
                //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISREACHUPPERLIMIT];
                //if (string.IsNullOrEmpty(address.Address0))
                //    return false;
                //return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
                //return !PLC[address.SiteNo].IOData.GetBit(address.Address0);

                return GetPos() > SIM_POS_LIMIT_MAX;
            }
        }
        public override bool IsReachLowerBound
        {
            get
            {
                //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISREACHLOWERLIMIT];
                //if (string.IsNullOrEmpty(address.Address0))
                //    return false;
                //return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
                //return !PLC[address.SiteNo].IOData.GetBit(address.Address0);

                return GetPos() < SIM_POS_LIMIT_MIN;
            }
        }

        public override void Go(float position)
        {
            simGo(position);
            //StepPositionSetfloat = position;

            //switch (MotionMMMode)
            //{
            //    case 1:
            //        StepPositionSetfloat = position;
            //        break;
            //    default:

            //        //@LETIAN: 原先的寫法會有數值誤差!!!
            //        //   案例 position = 36.35 會被處理成 3634 (36.34) 
            //        //old code >>> int setposition = (int)(position * (float)ONEMMSTEP);
            //        int setposition = (int)Math.Round((double)position * ONEMMSTEP);
            //        StepPositionSet = setposition;

            //        break;
            //}

            //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_GO];
            //PLC[address.SiteNo].WriteVari(address.Address0, "true");

            ////Task task = new Task(new Action(() =>
            ////{
            ////    System.Threading.Thread.Sleep(500);
            ////    //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_GO];
            ////    PLC[address.SiteNo].SetIO(false, address.Address0);
            ////}));
            ////task.Start();
        }
        public override void Home()
        {
            //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_HOME];
            //PLC[address.SiteNo].WriteVari(address.Address0, "true");
            simHome();
        }
        public override void SVOn()
        {
            //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_SVON];
            //if (address.SiteNo != -1)
            //    PLC[address.SiteNo].WriteVari(address.Address0, "true");
        }
        public override void Reset()
        {
            //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_RESET];
            //if(address.SiteNo != -1)
            //    PLC[address.SiteNo].WriteVari(address.Address0, "true");
        }
        public override void Break()
        {
            //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_BREAK];
            //if (address.SiteNo != -1)
            //    PLC[address.SiteNo].WriteVari(address.Address0, "true");
        }
        public override void Forward()
        {
            //if (IsNoUseMotor)
            //{
            //    SIMPositionStep++;
            //    return;
            //}

            //AddressClass forwardaddress = ADDRESSARRAY[(int)MotionAddressEnum.ADR_FORWARD];
            //PLC[forwardaddress.SiteNo].WriteVari(forwardaddress.Address0, "true");

            //AddressClass backwardaddress = ADDRESSARRAY[(int)MotionAddressEnum.ADR_BACKWARD];
            //PLC[backwardaddress.SiteNo].WriteVari(backwardaddress.Address0, "false");

            simForward();
        }
        public override void Backward()
        {
            //if (IsNoUseMotor)
            //{
            //    SIMPositionStep--;
            //    return;
            //}

            //AddressClass forwardaddress = ADDRESSARRAY[(int)MotionAddressEnum.ADR_FORWARD];
            //PLC[forwardaddress.SiteNo].WriteVari(forwardaddress.Address0, "false");

            //AddressClass backwardaddress = ADDRESSARRAY[(int)MotionAddressEnum.ADR_BACKWARD];
            //PLC[backwardaddress.SiteNo].WriteVari(backwardaddress.Address0, "true");

            simBackward();
        }
        public override void Stop()
        {
            //AddressClass forwardaddress = ADDRESSARRAY[(int)MotionAddressEnum.ADR_FORWARD];
            //PLC[forwardaddress.SiteNo].WriteVari(forwardaddress.Address0, "false");

            //AddressClass backwardaddress = ADDRESSARRAY[(int)MotionAddressEnum.ADR_BACKWARD];
            //PLC[backwardaddress.SiteNo].WriteVari(backwardaddress.Address0, "false");

            simStop();
        }
        public override void SetSpeed(SpeedTypeEnum speedtype)
        {
            //MotionAddressEnum motionaddress;
            double speedvalue = 0;

            switch (speedtype)
            {
                case SpeedTypeEnum.HOMEHIGH:
                    //motionaddress = MotionAddressEnum.ADR_HOMEHIGHSPEED;
                    //speedvalue = HOMEHIGHSPEED;
                    HOMEHIGHSPEED = speedvalue;
                    break;
                case SpeedTypeEnum.HOMESLOW:
                    //motionaddress = MotionAddressEnum.ADR_HOMESLOWSPEED;
                    //speedvalue = HOMESLOWSPEED;
                    HOMESLOWSPEED = speedvalue;
                    break;
                case SpeedTypeEnum.MANUALSLOW:
                    //motionaddress = MotionAddressEnum.ADR_MANUALSPEED;
                    speedvalue = MANUALSLOWSPEED;
                    break;
                case SpeedTypeEnum.MANUAL:
                    //motionaddress = MotionAddressEnum.ADR_MANUALSPEED;
                    //speedvalue = MANUALSPEED;
                    MANUALSPEED = speedvalue;
                    break;
                case SpeedTypeEnum.GOSLOW:
                    //motionaddress = MotionAddressEnum.ADR_GOSPEED;
                    //speedvalue = GOSLOWSPEED;
                    GOSLOWSPEED = speedvalue;
                    break;
                case SpeedTypeEnum.GO:
                    //motionaddress = MotionAddressEnum.ADR_GOSPEED;
                    //speedvalue = GOSPEED;
                    GOSPEED = speedvalue;
                    break;
                default:
                    break;
            }

            _simSpeed = speedvalue;
        }
        public override double GetSpeed(SpeedTypeEnum speedtype)
        {
            //MotionAddressEnum motionaddress = MotionAddressEnum.ADR_HOMEHIGHSPEED;
            double speedvalue = 0;

            switch (speedtype)
            {
                case SpeedTypeEnum.HOMEHIGH:
                    //motionaddress = MotionAddressEnum.ADR_HOMEHIGHSPEED;
                    speedvalue = HOMEHIGHSPEED;
                    break;
                case SpeedTypeEnum.HOMESLOW:
                    //motionaddress = MotionAddressEnum.ADR_HOMESLOWSPEED;
                    speedvalue = HOMESLOWSPEED;
                    break;
                case SpeedTypeEnum.MANUALSLOW:
                    //motionaddress = MotionAddressEnum.ADR_MANUALSPEED;
                    speedvalue = MANUALSLOWSPEED;
                    break;
                case SpeedTypeEnum.MANUAL:
                    //motionaddress = MotionAddressEnum.ADR_MANUALSPEED;
                    speedvalue = MANUALSPEED;
                    break;
                case SpeedTypeEnum.GOSLOW:
                    //motionaddress = MotionAddressEnum.ADR_GOSPEED;
                    speedvalue = GOSLOWSPEED;
                    break;
                case SpeedTypeEnum.GO:
                    //motionaddress = MotionAddressEnum.ADR_GOSPEED;
                    speedvalue = GOSPEED;
                    break;
            }

            _simSpeed = speedvalue;
            return _simSpeed;
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
        public override double GetInitPosition()
        {
            return READYPOSITION;
        }

        public override void Go(int posindex, float position)
        {
            SetPos(posindex, position);
            GoPos(posindex);
        }
        public override void SetPos(int posindex, float position)
        {
            //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_STEPPOSITIONSET];
            //if (address.SiteNo != -1)
            //    PLC[address.SiteNo].WriteVari(address.Address0, position.ToString());
            ////AddressClass address = new AddressClass(address1.ToString(), posindex * 2);
            //////发送给plc数据
            ////byte[] hex = BitConverter.GetBytes(position);
            ////byte[] h = new byte[2];
            ////byte[] l = new byte[2];

            ////h[0] = hex[0];
            ////h[1] = hex[1];
            ////l[0] = hex[2];
            ////l[1] = hex[3];

            ////ushort setH = BitConverter.ToUInt16(h, 0);
            ////ushort setL = BitConverter.ToUInt16(l, 0);

            ////PLC[address.SiteNo].SetData_ushort(setH, address.Address0);
            ////PLC[address.SiteNo].SetData_ushort(setL, address.Address1);

            if (_simPosTable.ContainsKey(posindex))
                _simPosTable[posindex] = position;
            else
                _simPosTable.Add(posindex, position);
        }
        public override void GoPos(int posindex)
        {
            //AddressClass address2 = ADDRESSARRAY[(int)MotionAddressEnum.ADR_GO];
            ////AddressClass address3 = new AddressClass(address2.ToString(), posindex);
            ////PLC[address3.SiteNo].SetIO(true, address3.Address0);
            //if (address2.SiteNo != -1)
            //    PLC[address2.SiteNo].WriteVari(address2.Address0, "true");

            if (_simPosTable.TryGetValue(posindex, out var pos))
                Go(pos);
        }
        public override bool IsOnSitePos(int posindex)
        {
            //AddressClass address = ADDRESSARRAY[(int)MotionAddressEnum.ADR_ISONSITE];
            //if (string.IsNullOrEmpty(address.Address0))
            //    return false;
            ////AddressClass address1 = new AddressClass(address.ToString(), posindex);
            ////return PLC[address1.SiteNo].IOData.GetBit(address1.Address0);
            //return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";

            return _simPosTable.ContainsKey(posindex);
        }
        public override float GetSetPos(int posindex)
        {
            //AddressClass address1 = ADDRESSARRAY[(int)MotionAddressEnum.ADR_STEPPOSITIONSET];
            //return float.Parse(PLC[address1.SiteNo].ReadVari(address1.Address0).ToLower());
            ////AddressClass address = new AddressClass(address1.ToString(), posindex * 2);

            ////if (MotionMMMode == 1)
            ////{
            ////    PLC[address.SiteNo].GetData(address.Address0);
            ////    PLC[address.SiteNo].GetData(address.Address1);

            ////    short setH = PLC[address.SiteNo].IOData.GetMW(address.Address0);
            ////    short setL = PLC[address.SiteNo].IOData.GetMW(address.Address1);

            ////    //接收plc数据
            ////    byte[] bytesH = BitConverter.GetBytes(setH);
            ////    byte[] bytesL = BitConverter.GetBytes(setL);

            ////    byte[] d = new byte[bytesH.Length + bytesL.Length];
            ////    Array.Copy(bytesH, 0, d, 0, bytesH.Length);
            ////    Array.Copy(bytesL, 0, d, bytesH.Length, bytesL.Length);

            ////    float myFloat = BitConverter.ToSingle(d, 0);
            ////    return myFloat;
            ////}
            ////Int32 ret = 0;
            ////ret = PLC[address.SiteNo].IOData.GetData(address.Address0);
            ////return (float)ret / (float)ONEMMSTEP;

            if (_simPosTable.TryGetValue(posindex, out var pos))
                return pos;

            return 0;
        }
    }

    partial class PLCMotionSim
    {
        double SIM_POS_LIMIT_MIN = -9999.0;
        double SIM_POS_LIMIT_MAX = 9999.0;

        #region PRIVATE_SIM_RUNTIME_DATA
        Dictionary<int, float> _simPosTable = new Dictionary<int, float>();
        double _simSpeed;
        int _simTargetPositionStep;
        int _simCurrentPositionStep
        {
            get;
            set;
        }
        bool _hasBeenHome = true;
        volatile bool _simRunFlag = false;
        #endregion

        void simSetTargetPos(float pos)
        {
            _simCurrentPositionStep = (int)((double)pos / ONEMMSTEP);
        }
        void simHome()
        {
            if (_simRunFlag)
                return;

            _simRunFlag = true;
            _hasBeenHome = false;

            var action = new Action(() =>
            {
                // 模擬脫離 ORG
                if (_simCurrentPositionStep == 0)
                    simGo(35f, async: false);
                
                // 模擬回 HOME
                System.Threading.Thread.Sleep(10);
                simGo(0f, async: false);

                _simCurrentPositionStep = 0;
                _hasBeenHome = true;
                _simRunFlag = false;
            });

            action.BeginInvoke(null, null);
        }
        void simGo(float pos, bool async = true)
        {
            if (_simRunFlag && async)
                return;

            _simRunFlag = true;

            if (ONEMMSTEP <= 1)
                ONEMMSTEP = 1000;

            if (Math.Abs(_simSpeed) < 1.0)
                _simSpeed = 10.0;

            int stepsPerSecond = (int)Math.Abs(_simSpeed * ONEMMSTEP);
            int targetStep = (int)(pos * ONEMMSTEP);
            if (targetStep >= _simCurrentPositionStep)
            {
                _simTargetPositionStep = Math.Min(targetStep, (int)(SIM_POS_LIMIT_MAX * ONEMMSTEP) + 1);
            }
            else
            {
                _simTargetPositionStep = Math.Max(targetStep, (int)(SIM_POS_LIMIT_MIN * ONEMMSTEP) - 1);
                stepsPerSecond = -stepsPerSecond;
            }

            var action = new Action(() =>
            {
                var tmStart = DateTime.Now;

                while (true)
                {
                    bool isCompleted = 
                        (stepsPerSecond >=0 && _simCurrentPositionStep >= _simTargetPositionStep) ||
                        (stepsPerSecond < 0 && _simCurrentPositionStep <= _simTargetPositionStep);

                    // Motion Completed
                    if (isCompleted)
                    {
                        _simCurrentPositionStep = _simTargetPositionStep;
                        break;
                    }
                    // User Stop
                    else if (!_simRunFlag)
                    {
                        _simTargetPositionStep = _simCurrentPositionStep;
                        break;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(10);
                        var ts = DateTime.Now - tmStart;
                        int steps = (int)(stepsPerSecond * ts.TotalSeconds);
                        _simCurrentPositionStep += steps;
                    }
                }

                if (async)
                    _simRunFlag = false;
            });

            if (async)
                action.BeginInvoke(null, null);
            else
                action();
        }
        void simForward()
        {
            //_simTargetPositionStep++;
            //_simCurrentPositionStep++;
            simGo((float)SIM_POS_LIMIT_MAX);
        }
        void simBackward()
        {
            //_simTargetPositionStep--;
            //_simCurrentPositionStep--;
            simGo((float)SIM_POS_LIMIT_MIN);
        }
        void simStop()
        {
            _simRunFlag = false;
        }
    }
}

