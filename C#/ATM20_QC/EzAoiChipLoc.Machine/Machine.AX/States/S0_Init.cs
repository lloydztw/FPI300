#region AUTHOR
/*
 * <FileName>
 * 
 * Copyright (C) 2023
 * 2023-09-03 revised by LetTian Chang
 * 2012-03-22 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * letian@jeteazy.com
 * lloydz.tw@gmail.com.tw
 * 
 */
#endregion


namespace AX.Machine.States
{
    class S_Init : MxState
    {
        public override void Home(int axisId)
        {
            //>>> _stateMachine.ChangeState(S.MotorHome, axisId);
            _hostMachine?.startHome(axisId);
        }
        public override void MotorMove(int axisId, double speed)
        {
            _hostMachine?.gotoHomeNotCompletedError(axisId);
        }
        public override void MotorMoveTo(int axisId, double pos, double speed)
        {
            _hostMachine?.gotoHomeNotCompletedError(axisId);
        }
    }
}
