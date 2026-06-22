namespace cvLib.FSM
{
    public class CState : CObject<string>, IState
    {
        public CState(string name) : base(name)
        {
            if (name == null)
                base.m_id = this.GetType().Name;
        }

        public virtual void OnActive(IStateMachine machine, params object[] args)
        {
        }

        #region OPTIONAL_CMD_OPERATIONS
        public virtual bool CanAccept(IStateMachine machine, object cmd)
        {
            return false;
        }
        public virtual void OnRequest(IStateMachine machine, object cmd, params object[] args)
        {
        }
        #endregion
    }
}
