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


namespace Amir.StateMachine
{
    public interface IxState : IState
    {
        bool CanAccept(IStateMachineImp machine, object cmd);
        void OnRequest(IStateMachineImp machine, object cmd, object arg);
    }
}
