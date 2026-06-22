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
    public interface IStateMachine : IDisposable
    { 
        event EventHandler OnStateChanged;

        object State { get; }
        bool IsTransient();

        //bool CanAccept(object cmd);
        //void Request(object cmd, object arg);
    }
}
