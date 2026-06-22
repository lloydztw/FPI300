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
using System.Diagnostics;

namespace AX.Machine.States
{
    class S_Motor_Move : S_MotorBase
    {
        public override void OnActive(IStateMachineImp fsm, object arg)
        {
            base.OnActive(fsm, arg);
            if (!isPrepared)
            {
                // Prepare
                IDevMotorActuator[] actuators = null;
                if (parse(arg, out int axisId, out double speed))
                    actuators = prepareActuators(axisId);

                // Empty
                if (actuators == null)
                {
                    _stateMachine.ChangeState(S.Ready);
                    return;
                }

                if (actuators.Length != 1)
                {
                    Debug.Assert(actuators.Length == 1, "只允許單軸操作!");
                    detachActuators();
                    _stateMachine.ChangeState(S.Ready);
                    return;
                }

                // The ONE
                var actuator = actuators[0];

                // DisplayText
                var unit = actuator.Motor.Unit;
                var text = $"{actuator.Name} (v: {speed:0.00} {unit}/s)";
                resetSubState(text);

                // Execute "Move"
                actuator.Move(speed);
                //_canAcceptTick = true;
            }
        }
        
        bool parse(object arg, out int axisId, out double speed)
        {
            axisId = -1;
            speed = 0;

            if (arg is object[] args)
            {
                if (args.Length > 0 && args[0] is int id)
                    axisId = id;
                if (args.Length > 1 && args[1] is double v)
                    speed = v;
                if (Math.Abs(speed) < 1e-6)
                    speed = _hostMachine.getDefaultSpeed(axisId);
            }

            // 只允許單軸操作
            bool ok = axisId >= 0;
            ok &= Math.Abs(speed) > 0.0001;
            return ok;
        }
    }
}
