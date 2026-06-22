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
using System;

namespace AX.Machine.States
{
    /// <summary>
    /// 復位,
    /// 交給個別的 Motor Actuator 顯示資訊 !!!
    /// </summary>
    class S_Motor_Recover : S_MotorBase
    {
        public override void OnActive(IStateMachineImp fsm, object arg)
        {
            base.OnActive(fsm, arg);
            _actuatorsInMotion = _hostMachine.GetAllMotorAcuators();
        }
        public override void OnMotorStateChanged(object sender, EventArgs e)
        {
            // Just check the end of motion
            // 運動狀態 (S_MotorBase states),
            // 只需要關注 Actuactor 是否運行結束.
            // 並根據 ErrCode 轉移狀態.
            checkEndOfMoationAndChangeState(_actuatorsInMotion);
        }
    }
}
