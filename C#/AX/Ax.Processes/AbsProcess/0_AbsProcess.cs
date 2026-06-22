#region AUTHOR
/*
 * AbsProcess
 * Copyright (C) 2023
 * 2023-09-03 revised by LetTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using Amir.StateMachine;
using System;

namespace AX.Processes.Zero
{
    /// <summary>
    /// <br/>AbsProcess
    /// <br/>@LETIAN: 2023-09-06 First Creation
    /// </summary>
    public abstract class AbsProcess : CStateMachine, IxProcess
    {
        #region NLOG
        // NOTE:
        //  不要做靜態初始化，而是要等到窗體的Load事件時才初始化Logger物件，
        //  且保證該窗體是 【首個使用】NLog 的 Class !!!
        //  這是因為NLog是在首次被使用時，才載入配置文件的。
        static NLog.Logger _logger = null;
        protected static NLog.ILogger LOG
        {
            get
            {
                if (_logger == null)
                    _logger = NLog.LogManager.GetCurrentClassLogger();
                return _logger;
            }
        }
        #endregion

        protected int DEFAULT_DURATION = 100;

        #region PRIVATE_TIMING_DATA
        DateTime _timStart = DateTime.Now;
        DateTime _timInterval = DateTime.Now;
        int _nextDuration = 100;
        bool _activated = false;
        #endregion

        #region TIMING_FUNCTIONS
        bool isIntervalTimeUp()
        {
            var ts = DateTime.Now - _timInterval;
            return (ts.TotalMilliseconds >= _nextDuration);
        }
        void resetIntervalTime()
        {
            _timInterval = DateTime.Now;
        }
        #endregion

        public event EventHandler OnBegin;
        public event EventHandler OnEnd;

        public AbsProcess()
        {
        }
        public virtual string Name
        {
            get => GetType().Name;
            //>>> protected set { }
        }

        public double EllapseMilliSeconds
        {
            get => (DateTime.Now - _timStart).TotalMilliseconds;
            protected set { _timStart = DateTime.Now; }
        }
        public bool IsActivated
        {
            get => _activated;
        }
        public int ExitCode
        {
            get; protected set;
        }

        protected virtual void start()
        {
            if (!_activated)
            {
                // 防止 ReEntry
                _activated = true;
                // 防止 初始化期間 Tick 做動
                _timInterval = DateTime.MaxValue;
                // 初始化
                ExitCode = -1;
                OnBegin?.Invoke(this, null);
                // 正式 Timing 開始
                _timStart = DateTime.Now;
                _timInterval = _timStart;
            }
        }
        protected virtual void ternimate(int exitCode = 0)
        {
            if (_activated)
            {                
                _activated = false;
                ExitCode = exitCode;
                OnEnd?.Invoke(this, null);
            }
        }

        public virtual void Tick()
        {
            if (_activated && isIntervalTimeUp())
            {
                OnIntervalTick(State);
                resetIntervalTime();
            }
        }
        
        protected virtual void OnIntervalTick(object state)
        {
        }       
        protected virtual void SetNext(object nextState, int nextDuration = -1)
        {
            SetNextDuration(nextDuration);
            changeState(nextState);

            // Auto-start
            // if (nextState != null && !_activated)
            //    start();
        }
        protected void SetNextDuration(int nextDuration = -1)
        {
            _nextDuration = nextDuration >= 0
                ? nextDuration
                : DEFAULT_DURATION;
        }
    }
}
