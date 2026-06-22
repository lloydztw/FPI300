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


namespace Amir.StateMachine.Generic
{
    public interface IStateMachine<CmdT> : IStateMachine
    {
        bool CanAccept(CmdT cmd);
        void Request(CmdT cmd, object arg);
    }
}
