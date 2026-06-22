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
    /// <summary>
    /// For the internal State Classes
    /// </summary>
    public interface IStateMachineImp : IStateMachine
    {
        void ChangeState(object stateNew, object arg = null, bool force = false);
        void NotifyStateChanged(object sender);
    }
}
