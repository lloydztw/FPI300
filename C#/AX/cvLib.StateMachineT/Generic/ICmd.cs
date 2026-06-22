#region AUTHOR
/*
 * LeTian.JxProps
 * Copyright (C) 2023
 * 2023-09-03 revised by LetTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using System;

namespace Amir.StateMachine
{
    public interface ICmd
    {
        bool CanAccept(object cmd);
        void Request(object cmd, object arg);
    }

    public interface ICmdState
    { 
        bool CanAccept(IStateMachineImp fsm, object cmd);
        void OnRequest(IStateMachineImp fsm, object cmd, object arg);
    }
}
