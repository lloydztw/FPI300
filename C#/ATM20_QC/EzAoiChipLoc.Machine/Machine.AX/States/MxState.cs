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


using Amir.StateMachine;
using JetEazy.Actuactor;
using System;

namespace AX.Machine.States
{
    public abstract class MxState : CState
    {
        #region NLOG
        // NOTE:
        //  不要做靜態初始化，而是要等到窗體的Load事件時才初始化Logger物件，
        //  且保證該窗體是 【首個使用】NLog 的 Class !!!
        //  這是因為NLog是在首次被使用時，才載入配置文件的。
        //static NLog.Logger _logger = null;
        //protected static NLog.ILogger LOG
        //{
        //    get
        //    {
        //        if (_logger == null)
        //            _logger = NLog.LogManager.GetCurrentClassLogger();
        //        return _logger;
        //    }
        //}
        #endregion

        #region DISPLAY_TEXT
        object _subState;
        public override string ToString()
        {
            if (_subState == null)
                return base.ToString();
            return base.ToString() + $" : {_subState}";
        }
        internal void resetSubState(object subState = null)
        {
            _subState = subState;
        }
        protected void notifySubState(object subState)
        {
            if (_subState != subState)
            {
                _subState = subState;
                _stateMachine.NotifyStateChanged(this);
            }
        }
        #endregion

        #region RUN_TIME_CACHE_DATA
        protected MxStates S => MxStates.Instance;
        protected IStateMachineImp _stateMachine;
        protected AxMultiAxesMachine _hostMachine => _stateMachine as AxMultiAxesMachine;
        #endregion

        public MxState(string name = null) : base(name)
        {
            if (name == null)
                m_id = m_id.Replace("S_", "");
        }
        
        public override int Priority => 0;
        public virtual bool IsError => false;
        public virtual bool IsFatal => false;

        public override void OnActive(IStateMachineImp fsm, object arg)
        {
            _stateMachine = fsm;
        }
        
        public virtual void OnMotorStateChanged(object sender, EventArgs e)
        {
            _hostMachine?.checkEndOfMotionAndChangeState(
                    _hostMachine?.GetAllMotorAcuators()
                );
        }
        public virtual void Tick()
        {
            // _hostMachine 已經 Tick
            // 所有 MotorActuactors,
            // 基本上, 其 state
            // 可以不用處理 Tick !!!
        }

        public virtual void Stop()
        {
            // 預設: 直接 call 每一軸 Stop
            _hostMachine?.stopActuators(_hostMachine?.GetAllMotorAcuators());
        }
        public virtual void EmgStop()
        {
            // 預設: 直接 call 每一軸 EmgStop
            _hostMachine?.emgStopActuators(_hostMachine?.GetAllMotorAcuators());
        }
        
        public virtual void Home(int axisId)
        { 
        }
        public virtual void MotorMove(int axisId, double speed)
        { 
        }
        public virtual void MotorMoveTo(int axisId, double pos, double speed)
        {
        }

#if(false)
        protected void stopActuators(params IDevMotorActuator[] actuators)
        {
            //if (actuators != null)
            //{
            //    foreach (var a in actuators)
            //        a?.Stop();
            //}
            _hostMachine?.stopActuators(actuators);
        }
        protected void emgStopActuators(params IDevMotorActuator[] actuators)
        {
            //if (actuators != null)
            //{
            //    foreach (var a in actuators)
            //        a?.EmgStop();
            //}
            _hostMachine?.emgStopActuators(actuators);
        }
#endif
    }
}
