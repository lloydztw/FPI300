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


using cvLib;
using System;
using System.Threading;

namespace Amir.StateMachine
{
    public abstract class CStateMachine : IStateMachineImp
    {
        #region AsyncInvoker
        public delegate IAsyncResult AsyncInvokeFunc(Delegate method, params object[] args);
        public void AttachInvoker(AsyncInvokeFunc func)
        {
            m_asyncInvokeFunc = func;
        }
        private AsyncInvokeFunc m_asyncInvokeFunc = null;
        #endregion

        public event EventHandler OnStateChanged;

        #region PRIVATE_DATA
        private object m_sync = new object();
        private object m_state = null;
        #endregion

        public CStateMachine()
        {
            CObject.init(AppDomain.CurrentDomain.FriendlyName);
        }
        public virtual void Dispose()
        {
            //virtual method
        }

        public object State
        {
            get { return m_state; }
        }
        public bool IsTransient()
        {
            return m_state == null;
        }

#if(false)
        bool IStateMachine.CanAccept(object cmd)
        {
            if (m_state is IState sts)
                return sts.CanAccept(this, cmd);
            return false;
        }
        void IStateMachine.Request(object cmd, object arg)
        {
            if (m_state is IState sts)
                sts.OnRequest(this, cmd, arg);
        }
#endif

        void IStateMachineImp.ChangeState(object stateNew, object arg, bool force = false)
        {
            /////////////////////////////////////////////////////////////
            // CAUTIONS: Do NOT lock this function
            /////////////////////////////////////////////////////////////
            if (m_state != stateNew)
            {
                var stsOld = m_state as IState;
                var stsNew = stateNew as IState;
                if (!force)
                {
                    #region COMPARE_PRIORITY
                    int oldPriority = stsOld != null ? stsOld.Priority : 0;
                    int newPriority = stsNew != null ? stsNew.Priority : 0;
                    if (newPriority < oldPriority)
                        return;
                    #endregion
                }

                if (stsNew != null || OnStateChanged != null)
                {
                    _TRACE("ChangeStateA", m_state, stateNew);

                    //(1) Make Transient
                    m_state = null;

                    //(2) Async Call _setState
                    Action<object, object> func = _setState;
                    if (m_asyncInvokeFunc != null)
                        m_asyncInvokeFunc(func, stateNew, arg);
                    else
                        func.BeginInvoke(stateNew, arg, null, null);
                }
                else
                {
                    //(3) Directly assign new state
                    _TRACE("ChangeStateD", m_state, stateNew);
                    m_state = stateNew;
                }
            }
        }
        void IStateMachineImp.NotifyStateChanged(object sender)
        {
            if (OnStateChanged != null)
            {
                //===============================================
                //NOTE: 
                //  可以直接 OnStateChanged.BeginInvoke,
                //  但是如果有很多 OnStateChanged listeners 時,
                //  會有問題 throw exception.
                //===============================================
                // EventHandler func = _issueStateChanged;
                // if (m_asyncInvokeFunc != null)
                //    m_asyncInvokeFunc(func, sender, EventArgs.Empty);
                // else
                //    func.BeginInvoke(sender, EventArgs.Empty, null, null);
                OnStateChanged(this, EventArgs.Empty);
            }
        }

        #region PROTECTED_FUNCTIONS
        protected void changeState(object stateNew, object arg = null, bool force = false)
        {
            ((IStateMachineImp)this).ChangeState(stateNew, arg, force);
        }
        protected void notifyStateChanged(object sender)
        {
            ((IStateMachineImp)this).NotifyStateChanged(sender);
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void _setState(object stateNew, object arg)
        {
            bool isChanged = false;

            lock (m_sync)
            {
                if (m_state != stateNew)
                {
                    var old = m_state;
                    m_state = stateNew;
                    if (m_state is IState sts)
                    {
                        _TRACE("OnActive", old, sts);
                        sts.OnActive(this, arg);
                    }
                    isChanged = true;
                }
            }

            if (isChanged)
                _issueStateChanged(this, null);
        }
        private void _issueStateChanged(object sender, EventArgs e)
        {
            if (OnStateChanged != null)
                OnStateChanged(sender, e);
        }
        #endregion

        #region TRACE_DEBUG_FUNCTIONS
        private void _TRACE(string funcName, object state, object newState)
        {
#if (DEBUG && false)
            System.Diagnostics.Debug.WriteLine(
                "### cvlib ### {0} ### {1}.{2} : {3} => {4}", 
                    Thread.CurrentThread.ManagedThreadId,
                    GetType().Name, funcName, state, newState
            );
#endif
        }
        #endregion
    }
}
