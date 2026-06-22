#region AUTHOR
/*
 * <FileName>
 * 
 * Copyright (C) 2023
 * 2023-09-03 revised by LetTian Chang
 * 2012-03-22 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * letian@jeteazy.com
 * lloydz.tw@gmail.com.tw
 * 
 */
#endregion

using AX.Machine.States;
using AX.Motor;
using JetEazy;
using JetEazy.Actuactor;
using JetEazy.Drivers.IOCtrl;
using JetEazy.Drivers.Motor;
using JetEazy.QMath;
using System;
using System.Collections.Generic;
using System.Threading;
using BaseMachine = AX.Processes.Zero.AbsProcess;

namespace AX.Machine
{
    public partial class AxMultiAxesMachine : BaseMachine, IxMultiAxesMachine
    {
        public event EventHandler OnTicked;
        //public event EventHandler OnMotionCompleted;

        #region STATES
        private ReaderWriterLockSlim _stateLock = new ReaderWriterLockSlim();
        public MxStates S => MxStates.Instance;
        MxState _state => base.State as MxState;
        #endregion

        public AxMultiAxesMachine(IDrvMotorAxis[] drvMotors, IDrvCommonIoCtrl ioCtrl)
        {
            _ioCtrl = ioCtrl;
            buildMotorMetaData(drvMotors);
            changeState(S.Init);
            base.start();
        }
        public override void Dispose()
        {
            if (IsMoving)
            {
                Stop();
                System.Threading.Thread.Sleep(500);
            }
            disposeMotorMetaData();

            base.ternimate();

            _ioCtrl = null;
        }
        public int ID
        {
            get;
            private set;
        }

        public bool IsInit
        {
            get => _state == S.Init;
        }
        public bool IsReady
        {
            get => _state == S.Ready;
        }
        public bool IsMoving
        {
            get
            {
                bool isStill = (IsReady || IsException || IsFatalError || _state == S.Init);
                return !isStill;
            }
        }
        public bool IsException
        {
            get
            {
                //var s = _state;
                //return s != null && s.IsError;
                _stateLock.EnterReadLock();
                try
                {
                    var s = _state;
                    return s != null && s.IsError;
                }
                finally
                {
                    _stateLock.ExitReadLock();
                }
            }
        }
        public bool IsFatalError
        {
            get
            {
                _stateLock.EnterReadLock();
                try
                {
                    var s = _state;
                    return s != null && s.IsFatal;
                }
                finally
                {
                    _stateLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// RESERVED
        /// </summary>
        public bool SafetyCheckEnabled
        {
            get => false;
            set { }
        }
       
        public void Stop()
        {
            _state?.Stop();
            //if (m_drvControlBox != null)
            //    m_drvControlBox.Stop();
        }
        public void EmgStop()
        {
            _state?.EmgStop();
            //if (m_drvControlBox != null)
            //    m_drvControlBox.Stop();
        }
        public void Home(int axisId)
        {
            _state?.Home(axisId);
        }
        public void MotorMove(int axisId, double speed)
        {
            _state?.MotorMove(axisId, speed);
        }
        public void MotorMoveTo(int axisId, double pos, double speed)
        {
            _state?.MotorMoveTo(axisId, pos, speed);
        }
    }

    partial class AxMultiAxesMachine
    {
        #region MOTOR_META
        class MotorMeta
        {
            public IDevMotorActuator Actuator;
            public AxMotorCtrl AxisCtrl;
        }
        #endregion

        #region DEVICE_AND_DRIVERS
        object _sync = new object();
        Dictionary<int, MotorMeta> _motorsDict;
        IDevMotorActuator[] _motorActuators;
        IDrvCommonIoCtrl _ioCtrl;
        #endregion

        #region BRAKE
        bool isSafeToReleaseBreak()
        {
            // 無剎車
            return true;
        }
        #endregion

        #region PROPERTIES
        public int TotalMotorsCount
        {
            get { return _motorsDict.Count; }
        }
        public IEnumerable<IDevMotorActuator> IterMotorActuator()
        {
            // Takes a snapshot of _motorsDict
            var dict = _motorsDict;
            if (dict != null)
            {
                foreach (var item in dict)
                {
                    var actuator = item.Value?.Actuator;
                    if (actuator != null)
                        yield return actuator;
                }
            }
        }
        public IEnumerable<IDrvMotorAxis> IterMotor()
        {
            foreach (var a in IterMotorActuator())
                if (a != null)
                    yield return a.Motor;
        }
        public IDevMotorActuator[] GetAllMotorAcuators()
        {
            // Takes a snapshot of _motorsDict
            //var dict = _motorsDict;
            //if (dict != null)
            //{
            //    var list = new List<IDevMotorActuator>();
            //    foreach (var item in dict)
            //    {
            //        var a = item.Value?.Actuator;
            //        if (a != null)
            //            list.Add(a);
            //    }
            //    return list.ToArray();
            //}
            return _motorActuators;
        }
        public IDevMotorActuator GetMotorAcuator(int axisId)
        {
            // Takes a snapshot of _motorsDict
            var dict = _motorsDict;
            if (dict != null &&
                dict.TryGetValue(axisId, out MotorMeta meta))
                return meta?.Actuator;
            return null;
        }
        public IDrvCommonIoCtrl IoCtrl => _ioCtrl;
        internal IDrvIoButtons IoButtons => _ioCtrl;
        #endregion

        void buildMotorMetaData(IDrvMotorAxis[] motors)
        {
            lock (_sync)
            {
                if (_motorsDict == null)
                {
                    var dict = new Dictionary<int, MotorMeta>();
                    var list = new List<IDevMotorActuator>();

                    foreach (var motor in motors)
                    {
                        if (motor == null)
                            continue;
                        
                        if (dict.ContainsKey(motor.ID))
                            continue;

                        buildMotorMetaData(motor, out MotorMeta motorMeta);
                        motorMeta.Actuator.OnStateChanged += OnMotorStateChanged;

                        dict.Add(motor.ID, motorMeta);
                        list.Add(motorMeta.Actuator);
                    }
                    _motorsDict = dict;
                    _motorActuators = list.ToArray();
                }
            }
        }
        void buildMotorMetaData(IDrvMotorAxis motor, out MotorMeta motorMeta)
        {
            lock (_sync)
            {
                var axisCtrl = new AxMotorCtrl(motor);
                axisCtrl.Attach(
                    ioCtrl: _ioCtrl,
                    brakeSafetyCheck: isSafeToReleaseBreak,
                    host: null
                );
                var actuator = new DevMotorActuator(axisCtrl);
                motorMeta = new MotorMeta()
                {
                    Actuator = actuator,
                    AxisCtrl = axisCtrl
                };
            }
        }
        void disposeMotorMetaData()
        {
            lock (_sync)
            {
                _motorActuators = null;
                var dict = _motorsDict;
                if (dict != null)
                {
                    foreach (var axisId in dict.Keys)
                    {
                        var meta = dict[axisId];
                        //>>> dict[axisId] = null;
                        var actuator = meta?.Actuator;
                        if (actuator != null)
                        {
                            if (actuator.IsMoving)
                                actuator.EmgStop();         // exit at moving
                            actuator.Dispose();
                        }
                    }
                }
                _motorsDict = null;
            }
        }

        protected override void OnIntervalTick(object arg)
        {
            _ioCtrl?.DoTick();
            tickActuators(_motorActuators);
            _state?.Tick();
            OnTicked?.Invoke(this, EventArgs.Empty);
        }
        private void OnMotorStateChanged(object sender, EventArgs e)
        {
            _state?.OnMotorStateChanged(sender, e);
            //>>> autoChangeToRecoveryState();
        }

        internal void tickActuators(params IDevMotorActuator[] actuators)
        {
            if (actuators != null)
            {
                foreach (var a in actuators)
                    a?.Tick();
            }
        }
        internal void stopActuators(params IDevMotorActuator[] actuators)
        {
            if (actuators != null)
            {
                foreach (var a in actuators)
                    a?.Stop();
            }
        }
        internal void emgStopActuators(params IDevMotorActuator[] actuators)
        {
            if (actuators != null)
            {
                foreach (var a in actuators)
                    a?.EmgStop();
            }
        }
        
        internal void startHome(int axisId)
        {
            if (axisId >= 0)
            {
                // 單軸
                var a = GetMotorAcuator(axisId);
                if (a != null)
                {
                    changeState(S.MotorHome, a.Name);
                    a?.Home();
                }
            }
            else
            {
                // 全軸
                changeState(S.MotorHome, "All");
                foreach (var a in IterMotorActuator())
                    a?.Home();
            }
        }
        internal void startMove(int axisId, double speed)
        {
            // 單軸 Only
            var a = GetMotorAcuator(axisId);
            if (a != null)
            {
                var text = a.Name + AxCtrl.FormatTag(a.Motor, speed);
                changeState(S.MotorMove, text);
                a?.Move(speed);
            }
        }
        internal void startMoveTo(int axisId, double pos, double speed)
        {
            // 單軸 Only
            var a = GetMotorAcuator(axisId);
            if (a != null)
            {
                var text = a.Name + AxCtrl.FormatTag(a.Motor, pos, speed);
                changeState(S.MotorMoveTo, text);
                a?.MoveTo(pos, speed);
            }
        }
        internal void startMultiMoveTo(QVector mvDst)
        {
            changeState(S.MotorError, MotorErrorCode.ERR_SYSTEM_Error);
        }
        internal void gotoHomeNotCompletedError(int axisId)
        {
            handleErrorAndChangeState(MotorErrorCode.Ex_Motor_Home_Not_Completed, axisId);
        }

        internal bool isEndOfMotion(IDevMotorActuator actuator)
        {
            if (actuator == null ||
                actuator.IsReady ||
                actuator.IsException ||
                actuator.IsFatalError)
                return true;
            return false;
        }

        internal bool areAllEndOfMotion(params IDevMotorActuator[] actuators)
        {
            bool allDone = true;

            if (actuators == null)
                return allDone;

            foreach (var a in actuators)
            {
                if (a == null)
                    continue;

                if (!isEndOfMotion(a))
                {
                    allDone = false;
                    break;
                }
            }

            return allDone;
        }

        internal void checkTheWorstError(
                IEnumerable<IDevMotorActuator> actuators,
                out MotorErrorCode theWorstError,
                out IDevMotorActuator theWorstAxis,
                out bool isAnyError,
                out bool isAnyFatal)
        {
            theWorstError = MotorErrorCode.NoErr;
            theWorstAxis = null;
            isAnyError = false;
            isAnyFatal = false;

            if (actuators != null)
            {
                foreach (var actuator in actuators)
                {
                    if (actuator == null)
                        continue;

                    isAnyFatal |= actuator.IsFatalError;
                    isAnyError |= actuator.IsException;

                    if (_motorsDict.TryGetValue(actuator.ID, out MotorMeta meta))
                    {
                        var ax = meta?.AxisCtrl;
                        if (ax != null)
                        {
                            var err = ax.CheckAnyException();
                            if (theWorstError < err)
                            {
                                theWorstError = err;
                                theWorstAxis = actuator;
                            }
                        }
                    }
                }
            }
        }

        internal bool checkEndOfMotionAndChangeState(params IDevMotorActuator[] actuators)
        {
            bool endOfMotion = false;
            if (areAllEndOfMotion(actuators))
            {
                checkTheWorstError(actuators,
                        out MotorErrorCode theWorstError,
                        out IDevMotorActuator theWorstAxis,
                        out bool isAnyError,
                        out bool isAnyFatal);

                if (!isAnyError && !isAnyFatal)
                {
                    changeState(S.Ready);
                    endOfMotion = true;
                }
                else
                {
                    var id = theWorstAxis != null ? theWorstAxis.ID : -1;
                    handleErrorAndChangeState(theWorstError, id);
                    endOfMotion = true;
                }
            }

            return endOfMotion;
        }
        internal void handleErrorAndChangeState(IEnumerable<IDevMotorActuator> actuators)
        {
            checkTheWorstError(
                    actuators,
                    out MotorErrorCode theWorstError,
                    out IDevMotorActuator theWorstAxis,
                    out bool isAnyError,
                    out bool isAnyFatal
                );    

            if (!isAnyError && !isAnyFatal)
            {
                changeState(S.Ready);
            }
            else
            {
                var id = theWorstAxis != null ? theWorstAxis.ID : -1;
                handleErrorAndChangeState(theWorstError, id);
            }
        }
        internal void handleErrorAndChangeState(MotorErrorCode err, int axisId = -1)
        {
            if (err == MotorErrorCode.NoErr)
            {
                changeState(S.Ready);
            }
            else
            {
                var msg = $"AxisId= {axisId} : {QxNums.GetEnumDescription(err)}";
                LOG?.Error(msg);
                var args = new object[] { axisId, err };
                changeState(S.MotorError, args);
            }
        }

        internal void lamp(bool green, bool yellow, bool red)
        {
            if (_ioCtrl != null)
                _ioCtrl.Lamp(green, yellow, red);
        }
        internal void buzzer(bool on)
        {
            if (_ioCtrl != null)
                _ioCtrl.AlarmBeepEnabled = on;
        }

        //internal void notifyCompleted()
        //{
        //    OnMotionCompleted?.Invoke(this, null);
        //}
        internal double getDefaultSpeed(int axisId)
        {
            var motor = GetMotorAcuator(axisId)?.Motor;
            if (motor != null)
            {
                motor.GetMotionSpeeds(out double hi, out double lo, MotorUnitMode.Physical);
                double speed = motor.GetPMoveSpeed(MotorUnitMode.Physical);
                if (Math.Abs(speed) < 1e-6)
                    speed = speed >= 0 ? lo : -lo;
                return speed;
            }
            return 0;
        }
    }

    partial class AxMultiAxesMachine
    {
        #region PERSISTENCE
        public void Load(string iniFileName)
        {
            iniFileName = normalizeFileName(iniFileName);
            foreach (var a in IterMotorActuator())
            {
                if (a != null)
                    loadMotorSettings(iniFileName, a.Motor);
            }
        }
        public void Save(string iniFileName)
        {
            iniFileName = normalizeFileName(iniFileName);
            foreach (var a in IterMotorActuator())
            {
                if (a != null)
                    saveMotorSettings(iniFileName, a.Motor);
            }
        }
        void loadMotorSettings(string iniFileName, IDrvMotorAxis drvMotor)
        {
            if (drvMotor != null)
            {
                string sectName = $"MotorAxis_{drvMotor.ID}";

                bool isTheta = drvMotor.Name.Contains("θ") || drvMotor.Unit == "deg";
                double lo = 0.0;
                double hi = 0.0;

                JetEazy.Win32.Win32Ini.Load(ref hi, iniFileName, sectName, "HomeSpeedHI");
                JetEazy.Win32.Win32Ini.Load(ref lo, iniFileName, sectName, "HomeSpeedLO");
                hi = normalizeSpeed(hi, true, isTheta);
                lo = normalizeSpeed(lo, false, isTheta);
                drvMotor.SetHomeSpeeds(hi, lo, MotorUnitMode.Physical);

                JetEazy.Win32.Win32Ini.Load(ref hi, iniFileName, sectName, "MotionSpeedHI");
                JetEazy.Win32.Win32Ini.Load(ref lo, iniFileName, sectName, "MotionSpeedLO");
                hi = normalizeSpeed(hi, true, isTheta);
                lo = normalizeSpeed(lo, false, isTheta);
                drvMotor.SetMotionSpeeds(hi, lo, MotorUnitMode.Physical);
            }
        }
        void saveMotorSettings(string iniFileName, IDrvMotorAxis drvMotor)
        {
            if (drvMotor != null)
            {
                string sectName = $"MotorAxis_{drvMotor.ID}";

                drvMotor.GetHomeSpeeds(out double hi, out double lo, MotorUnitMode.Physical);
                JetEazy.Win32.Win32Ini.Save(hi, iniFileName, sectName, "HomeSpeedHI");
                JetEazy.Win32.Win32Ini.Save(lo, iniFileName, sectName, "HomeSpeedLO");

                drvMotor.GetMotionSpeeds(out hi, out lo, MotorUnitMode.Physical);
                JetEazy.Win32.Win32Ini.Save(hi, iniFileName, sectName, "MotionSpeedHI");
                JetEazy.Win32.Win32Ini.Save(lo, iniFileName, sectName, "MotionSpeedLO");
            }
        }
        string normalizeFileName(string fileName)
        {
            if (fileName == null)
            {
                return System.IO.Path.GetTempFileName() + "//"
                     + AppDomain.CurrentDomain.FriendlyName + "_"
                     + GetType().Name
                     + ".ini";
            }
            return fileName;
        }
        double normalizeSpeed(double speed, bool hi, bool isTheta)
        {
            if (Math.Abs(speed) < 1e-6)
            {
                if(isTheta)
                {
                    var rpm = 360.0 / 60;
                    return hi ? rpm * 3 : rpm;
                }
                else
                {
                    return hi ? 50 : 5;
                }
            }
            return speed;
        }
        #endregion
    }
}
