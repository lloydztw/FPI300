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
    public interface IxTickDriven
    {
        void Connect(IxTickSource src);
        void Tick(object sender, EventArgs e);
    }
}
