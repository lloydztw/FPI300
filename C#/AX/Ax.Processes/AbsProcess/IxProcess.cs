#region AUTHOR
/*
 * IxProcess
 * Copyright (C) 2023
 * 2023-09-03 created by LetTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Actuactor;
using System;

namespace AX.Processes
{
    /// <summary>
    /// <br/>IxProcess 
    /// <br/>@LETIAN: 20220619 First Creation
    /// </summary>
    public interface IxProcess : IxTickDriven
    {
        event EventHandler OnStateChanged;
        event EventHandler OnBegin;
        event EventHandler OnEnd;
        
        //event EventHandler<ProcessEventArgs> OnError;
        //event EventHandler<ProcessEventArgs> OnMessage;
        //event EventHandler<ProcessEventArgs> OnCompleted;

        //>>> IxTickDriven <<<
        // void Tick();

        double EllapseMilliSeconds { get; }
        bool IsActivated { get; }
    }
}
