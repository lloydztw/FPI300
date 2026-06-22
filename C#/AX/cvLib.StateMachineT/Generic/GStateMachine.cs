namespace cvLib.FSM.Generic
{
    public abstract class CStateMachine<CmdT> : CStateMachine, IStateMachine<CmdT>
    {
        bool IStateMachine.CanAccept(object cmd)
        {
            return CanAccept((CmdT)cmd);
        }
        void IStateMachine.Request(object cmd, params object[] args)
        {
            Request((CmdT)cmd, args);
        }

        public virtual bool CanAccept(CmdT cmd)
        {
            var state = (IState<CmdT>)ActiveState;
            if (state != null)
                return state.CanAccept(this, cmd);
            return false;
        }
        public virtual void Request(CmdT cmd, params object[] args)
        {
            var state = (IState<CmdT>)ActiveState;
            state?.OnRequest(this, cmd, args);
        }
    }
}
