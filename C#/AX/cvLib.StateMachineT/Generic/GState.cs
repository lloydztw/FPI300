
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


using System.Collections.Generic;

namespace Amir.StateMachine.Generic
{
    public class CState<CmdT> : CState
    {
        #region TRANSIENT_TABLE
        internal Dictionary<CmdT, IState> m_transTable = null;
        public void addTransient(CmdT cmd, IState nextState)
        {
            if (m_transTable == null)
                m_transTable = new Dictionary<CmdT, IState>();
            m_transTable.Add(cmd, nextState);
        }
        public static void transient(IState state, CmdT cmd, IState nextState)
        {
            ((CState<CmdT>)state).addTransient(cmd, nextState);
        }
        #endregion

        public CState(string name) : base(name) { }

#if(false)
        public override bool CanAccept(IStateMachineImp machine, object cmd)
        {
            return CanAccept(machine, (CmdT)cmd);
        }
        public override void OnRequest(IStateMachineImp machine, object cmd, object arg)
        {
            OnRequest(machine, (CmdT)cmd, arg);
        }
#endif

        public virtual bool CanAccept(IStateMachineImp machine, CmdT cmd)
        {
            if (m_transTable != null && m_transTable.ContainsKey(cmd))
                return true;
            return false;
        }
        public virtual void OnRequest(IStateMachineImp machine, CmdT cmd, object arg)
        {
            if (m_transTable != null)
            {
                if (m_transTable.TryGetValue(cmd, out IState nextState))
                {
                    machine.ChangeState(nextState, arg);
                }
            }
        }
    }
}
