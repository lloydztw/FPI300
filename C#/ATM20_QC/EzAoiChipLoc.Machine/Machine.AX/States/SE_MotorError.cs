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
using JetEazy;
using JetEazy.Actuactor;
using System;
using ErrCode = AX.MotorErrorCode;

namespace AX.Machine.States
{
    /// <summary>
    /// 每次只專注一個最嚴重的異常,
    /// 並交給個別的 Motor Actuator 
    /// 處理 !!! 
    /// </summary>
    class S_MotorError : MxState
    {
        #region PRIVATE_DATA
        ErrCode _theWorstError = ErrCode.Ex_Motor_Error;
        IDevMotorActuator _theWorstAxis;
        #endregion

        #region DISPLAY_TEXT
        public override string ToString()
        {
            if (_theWorstAxis != null)
            {
                var name = _theWorstAxis.Name;
                var state = _theWorstAxis.State;
                return $"{name} 馬達異常: {state}";
            }
            else
            {
                var msg = QxNums.GetEnumDescription(_theWorstError);
                return msg;
            }
        }
        #endregion

        public override int Priority
        {
            get => (int)_theWorstError;
        }
        public override bool IsError
        {
            get
            {
                if (_theWorstAxis != null)
                    return _theWorstAxis.IsException;
                return true;
            }
        }
        public override bool IsFatal
        {
            get
            {
                if (_theWorstAxis != null)
                    return _theWorstAxis.IsFatalError;
                return true;
            }
        }

        public override void OnActive(IStateMachineImp fsm, object arg)
        {
            base.OnActive(fsm, arg);
            
            _hostMachine?.lamp(false, false, true);

            if (parse(arg, out int axisId, out ErrCode err))
            {
                // 第一個異常碼
                _theWorstError = err;
                var axis = _hostMachine.GetMotorAcuator(axisId);
                checkErrorAndChangeState(axis);
            }
            else
            {
                // 重新全部掃描
                var actuators = _hostMachine.GetAllMotorAcuators();
                checkErrorAndChangeState(actuators);
            }
        }
        public override void OnMotorStateChanged(object sender, EventArgs e)
        {
            if (_hostMachine == null ||
                _hostMachine.State != this)
                return;

            // 每次只專注一個最嚴重的異常
            if (_hostMachine.isEndOfMotion(_theWorstAxis))
            {
                checkErrorAndChangeState(_theWorstAxis);
                return;
            }

            // 直接反應 DevMotorActuator 的 復位 狀態
            var stateText = _theWorstAxis?.State?.ToString();
            if (stateText != null && stateText.Contains("復位"))
            {
                string text = "復位中 @ " + QxNums.GetEnumDescription(_theWorstError);
                _stateMachine.ChangeState(S.MotorRecover, text, true);
            }
        }

        public override void MotorMove(int axisId, double speed)
        {
            // 允許 Jog Move / Stop
            var actuator = _hostMachine?.GetMotorAcuator(axisId);
            actuator?.Move(speed);
        }
        public override void MotorMoveTo(int axisId, double pos, double speed)
        {
            // 不允許 MoveTo
            //////var actuator = _hostMachine?.GetMotorAcuator(axisId);
            //////actuator?.MoveTo(pos, speed);
        }

        bool parse(object arg, out int axisId, out ErrCode err)
        {
            axisId = -1;
            err = _theWorstError;

            if (arg is object[] args && args.Length >= 2)
            {
                if (args[0] is int id)
                    axisId = id;
                if (args[1] is ErrCode e && e > err)
                    err = e;
            }

            return axisId >= 0;
        }
        void checkErrorAndChangeState(params IDevMotorActuator[] actuators)
        {
            if (_hostMachine == null ||
                _hostMachine.State != this)
                return;

            _hostMachine.checkTheWorstError(
                    actuators,
                    out ErrCode theWorstError,
                    out IDevMotorActuator theWorstAxis,
                    out bool isAnyError,
                    out bool isAnyFatal
                );

            if (!isAnyError && !isAnyFatal)
            {
                _stateMachine.ChangeState(S.Ready, null, true);
            }
            else
            {
                if (_theWorstError != theWorstError ||
                    _theWorstAxis != theWorstAxis)
                {
                    _theWorstError = theWorstError;
                    _theWorstAxis = theWorstAxis;
                    _stateMachine.NotifyStateChanged(this);

                    // Temperally 
                    //_actuatorsInMotion = new IDevMotorActuator[]
                    //{
                    //    _theWorstAxis
                    //};
                }
            }
        }
    }
}
