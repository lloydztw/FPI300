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

using Amir.StateMachine;
using JetEazy.Drivers.Motor;
using JetEazy.QMath;
using CxPasoMotorVector = JetEazy.QMath.QVector;
using ErrCode = AX.MotorErrorCode;
using MoState = AX.Machine.States.MxState;

namespace AX.Machine.States
{
    class S_Ready : MoState
    {
        public override void OnActive(IStateMachineImp fsm, object arg)
        {
            base.OnActive(fsm, arg);
            _hostMachine?.lamp(green: true, yellow: false, red: false);
        }
        
        public override void Tick()
        {
            if (_hostMachine?.State == this)
            {
                _hostMachine?.handleErrorAndChangeState(
                        _hostMachine.IterMotorActuator()
                    );
            }
        }
        public override void Home(int axisId)
        {
            //_stateMachine.ChangeState(S.MotorHome, axisId);
            _hostMachine?.startHome(axisId);
        }
        public override void MotorMove(int axisID, double speed)
        {
            //var args = new object[] { axisID, speed };
            //_stateMachine.ChangeState(S.MotorMove, args);
            _hostMachine?.startMove(axisID, speed);
        }
        public override void MotorMoveTo(int axisID, double pos, double speed)
        {
            //var args = new object[] { axisID, pos, speed };
            //_stateMachine.ChangeState(S.MotorMoveTo, args);
            _hostMachine?.startMoveTo(axisID, pos, speed);
        }
        public void MultiAxesMoveTo(QVector mvDst)
        {
            //_stateMachine.ChangeState(S.MultiMoveTo, mvDst);
            _hostMachine?.startMultiMoveTo(mvDst);
        }        
    }
}
