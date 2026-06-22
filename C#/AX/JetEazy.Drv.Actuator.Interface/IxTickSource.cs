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

namespace JetEazy.Actuactor
{
    public interface IxTickSource : IDisposable
    {
        void Connect(params IxTickDriven[] tickees);
        void Disconnect(params IxTickDriven[] tickees);
        bool Enabled { get; set; }
    }
}
