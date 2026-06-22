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
using MoState = AX.Machine.States.MxState;

namespace AX.Machine.States
{
    abstract class S_MotorBase : MoState
    {
        #region PRIVATE_RUN_TIME_DATA
        protected IDevMotorActuator[] _actuatorsInMotion;
        //protected bool _canAcceptTick = false;
        #endregion

        public override void OnActive(IStateMachineImp fsm, object arg)
        {
            base.OnActive(fsm, arg);
            _hostMachine?.lamp(false, true, false);
        }
        public override void Tick()
        {
            //========================================
            // 所有 motor actuators tick 交給
            // _hostMachine 處理 !
            //----------------------------------------
            // 此處可以依需求增加顯示 
            // actuator 內部狀態資訊.
            //========================================
        }

        public override void Stop()
        {
            base.Stop();
            //>>> stopActuators(_actuatorsInMotion);
            checkEndOfMoationAndChangeState(_actuatorsInMotion);
        }
        public override void EmgStop()
        {
            //>>> NOT verified yet! <<<
            // 暴力終止 !!!
            base.emgStopActuators();
            //_canAcceptTick = false;
            //>>> emgStopActuators(_actuatorsInMotion);
            checkEndOfMoationAndChangeState(_actuatorsInMotion);
            detachActuators();
        }
        public override void OnMotorStateChanged(object sender, EventArgs e)
        {
            // Just check the end of motion
            // 運動狀態 (S_MotorBase states),
            // 只需要關注 Actuactor 是否運行結束.
            // 並根據 ErrCode 轉移狀態.
            checkEndOfMoationAndChangeState(_actuatorsInMotion);
        }

        protected IDevMotorActuator[] prepareActuators(int axisId)
        {
            resetSubState();

            //var availableActuators = _hostMachine.MotorActuators;
            //int N = availableActuators.Length;

            IDevMotorActuator[] results;

            // All Actuator
            if (axisId < 0)
            {
                results = _hostMachine.GetAllMotorAcuators();
            }
            // Single Actuator
            else
            {
                var a = _hostMachine.GetMotorAcuator(axisId);
                if (a != null)
                    results = new IDevMotorActuator[] { a };
                else
                    results = null;
            }

            // Check Empty
            int count = 0;
            if (results != null)
            {
                foreach (var a in results)
                {
                    if (a != null && a.Motor != null)
                    {
                        count++;
                    }
                }
            }

            _actuatorsInMotion = count > 0 ? results : null;
            return _actuatorsInMotion;
        }
        protected void detachActuators()
        {
            //_canAcceptTick = false;
            _actuatorsInMotion = null;
            resetSubState();
        }
        protected bool isPrepared
        {
            get => _actuatorsInMotion != null;
        }

#if(false)
        protected void stopActuators(params IDevMotorActuator[] actuators)
        {
            if (actuators != null)
            {
                foreach (var a in actuators)
                    a?.Stop();
            }
        }
        protected void emgStopActuators(params IDevMotorActuator[] actuators)
        {
            if (actuators != null)
            {
                foreach (var a in actuators)
                    a?.EmgStop();
            }
        }
#endif

        protected bool checkEndOfMoationAndChangeState(params IDevMotorActuator[] actuators)
        {
            bool isEnd = _hostMachine != null && _hostMachine.checkEndOfMotionAndChangeState(actuators);
            if (isEnd)
            {
                //_canAcceptTick = false;
                detachActuators();
            }
            return isEnd;
        }
    }
}
