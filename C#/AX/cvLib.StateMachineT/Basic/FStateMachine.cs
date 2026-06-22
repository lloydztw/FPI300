using System;

namespace cvLib.FSM
{
    public abstract class CStateMachine : IStateMachine
    {
        #region AsyncInvoker
        public delegate IAsyncResult AsyncInvokeFunc(Delegate method, params object[] args);
        private AsyncInvokeFunc m_asyncInvokeFunc = null;
        public void Attach(AsyncInvokeFunc func)
        {
            m_asyncInvokeFunc = func;
        }
        #endregion

        public event EventHandler OnStateChanged;

        #region PRIVATE_DATA
        private object m_sync;
        private IState m_state = null;
        #endregion

        public CStateMachine()
        {
            m_sync = this;
            CObject.init(AppDomain.CurrentDomain.FriendlyName);
        }
        public virtual void Dispose()
        {
            //virtual method
        }

        public IState ActiveState
        {
            get { return m_state; }
        }      
        public bool IsTransient()
        {
            return m_state == null;
        }

        internal virtual void changeState(IState newState, params object[] args)
        {
            /////////////////////////////////////////////////////////////
            // CAUTIONS: Do NOT lock this function
            /////////////////////////////////////////////////////////////
            if (m_state != newState)
            {
                _TRACE("changeState", newState);

                //(1) Make Transient by set ActiveState = null
                m_state = null;

                //(2) Async Invoke _setState
                Action<IState, object[]> func = _setState;
                if (m_asyncInvokeFunc != null)
                    m_asyncInvokeFunc(func, newState, args);
                else
                    func.BeginInvoke(newState, args, null, null);
            }
        }
        internal virtual void notifyStateChanged(object sender)
        {
            //===============================================
            //NOTE:
            //  由 Message Handler 負責 Async
            //===============================================
            //NOTE: 
            //  可以直接 OnStateChanged.BeginInvoke,
            //  但是如果有很多 OnStateChanged listeners 時,
            //  會有問題 throw exception.
            //===============================================

            if (OnStateChanged != null)
            {
                // EventHandler func = _issueStateChanged;
                // if (m_asyncInvokeFunc != null)
                //    m_asyncInvokeFunc(func, sender, EventArgs.Empty);
                // else
                //    func.BeginInvoke(sender, EventArgs.Empty, null, null);
                OnStateChanged(this, EventArgs.Empty);
            }
        }

        #region OPTIONAL_COMMAND_FUNCTIONS
        bool IStateMachine.CanAccept(object cmd)
        {
            return false;
        }
        void IStateMachine.Request(object cmd, params object[] args)
        {
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void _setState(IState stateNew, object[] args)
        {
            bool isChanged = false;

            lock (m_sync)
            {
                if (m_state != stateNew)
                {
                    m_state = stateNew;

                    _TRACE("OnActive", stateNew);

                    ((CState)m_state).OnActive(this, args);

                    isChanged = true;
                }
            }

            if (isChanged)
                notifyStateChanged(this);
        }
        private void _issueStateChanged(object sender, EventArgs e)
        {
            if (OnStateChanged != null)
                OnStateChanged(sender, e);
        }
        #endregion

        #region TRACE_DEBUG_FUNCTIONS
        private void _TRACE(string funcName, object state)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(
                $"### cvlib ##### {GetType().Name}.{funcName}({state})"
            );
#endif
        }
        #endregion
    }
}
