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

namespace Paso.Machine.States
{
    class S_Motor_Fatal : S_MotorBase
    {
        public override bool IsFatal => true;
        public override void OnActive(IStateMachineImp fsm, object arg)
        {
            base.OnActive(fsm, arg);
            _canAcceptTick = true;
        }
    }
}
