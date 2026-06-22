#region AUTHOR
/*
 * BaseProcess
 * Copyright (C) 2023
 * 2023-09-03 revised by LetTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;

namespace AX.Processes.One
{
    /// <summary>
    /// <br/>Process
    /// <br/>@LETIAN: 2023-09-06 First Creation
    /// </summary>
    public class BaseProcess : Zero.AbsProcess
    {
        #region TICK_STATE
        public class TickState
        {
            public TickState(Action func, string name = null)
            {
                TickFunc = func;
                Name = name ?? (func?.Method.Name);
            }
            protected TickState()
            {
            }
            public string Name
            {
                get; protected set;
            }
            public Action TickFunc
            {
                get; protected set;
            }
            public bool Equals(Action tickFunc)
            {
                return TickFunc == tickFunc;
            }
            public override string ToString()
            {
                return Name;
            }
        }
        TickState _state => base.State as TickState;
        #endregion

        protected override void OnIntervalTick(object state)
        {
            var tickFunc = _state?.TickFunc;
            if (tickFunc != null)
                tickFunc();
        }
        protected override void SetNext(object nextState, int nextDuration = -1)
        {
            // 屏蔽 base class function
            SetNext((Action)nextState, null, nextDuration);
        }
        
        protected void SetNext(TickState nextState, int nextDuration = -1)
        {
            SetNextDuration(nextDuration);
            var old = _state?.TickFunc;
            if (old != nextState.TickFunc)
            {
                changeState(nextState);
            }
        }
        protected void SetNext(Action nextState, string stateName = null, int nextDuration = -1)
        {
            SetNextDuration(nextDuration);

            var old = _state?.TickFunc;
            if (old != nextState)
            {
                changeState(new TickState(nextState, stateName));
            }
        }
    }
}
