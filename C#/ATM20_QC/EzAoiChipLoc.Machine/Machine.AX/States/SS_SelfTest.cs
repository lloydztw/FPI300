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
using System;
using MoState = Paso.Machine.States.S_Base;
using S = Paso.Machine.CxPasoStates;

namespace Paso.Machine.States
{
    class S_Self_Test : MoState
    {
        EventHandler _callback;
        public override void OnActive(IStateMachineImp fsm, object arg)
        {
            base.OnActive(fsm, arg);
            _callback = arg as EventHandler;
        }
        public override void Tick()
        {
            if (_stateMachine.State == this)
            {
                _stateMachine.ChangeState(S.Ready);
                _callback?.Invoke(this, null);
            }
        }
    }
}
