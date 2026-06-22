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
    public interface IState
    {
        int Priority { get; }
        void OnActive(IStateMachineImp fsm, object arg);

#if(false)
        bool CanAccept(IStateMachineImp fsm, object cmd);
        void OnRequest(IStateMachineImp fsm, object cmd, object arg);
#endif
    }
}
