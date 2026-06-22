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

namespace AX.Processes.Two
{
    /// <summary>
    /// <br/>AbsProcess
    /// <br/>@LETIAN: 2023-09-06 First Creation
    /// </summary>
    public class BaseProcess : One.BaseProcess
    {
        public event EventHandler<ProcessEventArgs> OnError;
        public event EventHandler<ProcessEventArgs> OnMessage;
        public event EventHandler<ProcessEventArgs> OnCompleted;

        #region PROPERTIES
        string _lastError;
        #endregion

        public string LastError
        {
            get => _lastError;
        }
        protected override void start()
        {
            if (!IsActivated)
            {
                _lastError = null;
                base.start();
            }
        }

        protected void SetError(string err, Action nextState = null, bool overwrite = true)
        {
            if (!overwrite && _lastError != null)
                return;

            if (_lastError != err)
            {
                _lastError = err;
                LOG?.Error(err);
            }

            if (nextState != null)
                SetNext(nextState, err);
        }

        #region NOTIFICATIONS
        protected void notifyError(string error)
        {
            var e = new ProcessEventArgs() { Message = error };
            OnError?.Invoke(this, e);
        }
        protected void notifyMessage(string message)
        {
            var e = new ProcessEventArgs() { Message = message };
            OnMessage?.Invoke(this, e);
        }
        protected void notifyMessage(ProcessEventArgs e)
        {
            OnMessage?.Invoke(this, e);
        }
        protected void notifyCompleted(ProcessEventArgs e = null)
        {
            if (e == null)
            {
                e = new ProcessEventArgs()
                {
                    Message = "Completed!"
                };
            }
            OnCompleted?.Invoke(this, e);
        }
        #endregion
    }
}
