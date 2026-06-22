namespace cvLib.StateMachine.Generic
{
    public interface IState<CmdT> : IState
    {
        bool CanAccept(IStateMachineImp machine, CmdT cmd);
        void OnRequest(IStateMachineImp machine, CmdT cmd, params object[] args);
    }
}
