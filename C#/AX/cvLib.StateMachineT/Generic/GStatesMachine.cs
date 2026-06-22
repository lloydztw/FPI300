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


namespace Amir.StateMachine.Generic
{
    public abstract class CStateMachine<CmdT> : CStateMachine, IStateMachine<CmdT>
    {
#if(false)
        bool IStateMachine.CanAccept(object cmd)
        {
            return CanAccept((CmdT)cmd);
        }
        void IStateMachine.Request(object cmd, object arg)
        {
            Request((CmdT)cmd, arg);
        }
#endif

        public virtual bool CanAccept(CmdT cmd)
        {
            var state = (CState<CmdT>)State;
            if (state != null)
                return state.CanAccept(this, cmd);
            return false;
        }
        public virtual void Request(CmdT cmd, object arg)
        {
            var state = (CState<CmdT>)State;
            state?.OnRequest(this, cmd, arg);
        }
    }
}
