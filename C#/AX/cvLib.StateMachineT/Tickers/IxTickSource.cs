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
    public interface IxTickSource
    {
        event EventHandler Tick;
        int Interval { get; set; }
        bool Enabled { get; set; }
    }
}
