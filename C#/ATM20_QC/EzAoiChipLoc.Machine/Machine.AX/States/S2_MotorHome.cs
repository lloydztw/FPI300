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

namespace AX.Machine.States
{
    class S_Motor_Home : S_MotorBase
    {
        public override void OnActive(IStateMachineImp fsm, object arg)
        {
            base.OnActive(fsm, arg);

            if (!isPrepared)
            {
                int axisId = parse(arg);

                // Prepare
                var actuators = prepareActuators(axisId);

                // Empty Motors                
                if (actuators == null)
                {
                    _stateMachine.ChangeState(S.Init);
                    return;
                }

                // DisplayText
                var text = actuators.Length > 1 ? actuators[0].Name : "All";
                resetSubState(text);

                // Execute Home
                foreach (var actuator in actuators)
                {
                    actuator.Home();
                }

                // AcceptTick
                //_canAcceptTick = true;
            }
        }
        
        int parse(object arg)
        {
            if (arg is int id)
                return id;
            return -1;
        }
    }
}
