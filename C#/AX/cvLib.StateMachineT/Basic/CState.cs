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

namespace Amir.StateMachine
{
    public class CState : CObject<string>, IState
    {
        public CState(string name) : base(name)
        {
            if (name == null)
                base.m_id = this.GetType().Name;
        }
        public virtual int Priority => 0;
        public virtual void OnActive(IStateMachineImp fsm, object arg)
        {
        }

#if(false)
        public virtual bool CanAccept(IStateMachineImp machine, object cmd)
        {
            return false;
        }
        public virtual void OnRequest(IStateMachineImp machine, object cmd, object arg)
        {
        }
#endif
    }
}
