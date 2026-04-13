#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2023-09-11 LeTian Chang, Revision.
 *      2008-12-01 LeTian Chang, Creation.
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.ComponentModel;


namespace JetEazy.Drivers.Motor
{
    public enum MotorState : int
    {
        [Description("軸卡尚未初始化")]
        Unknown = -1,

        Ready = 0,

        [Description("回HOME")]
        Homing,

        [Description("定位移動中")]
        P_Moving,

        [Description("移動中")]
        V_Moving,

        [Description("軸卡極限")]
        LimitedStop,

        /// <summary>
        ///目前應用狀況: EmergencyStop 內部都強制不使用, 
        ///統一交由外部 IoCtrl Emg Button 處理
        /// </summary>
        [Description("軸卡Emg")]
        EmergencyStop,
        
        [Description("馬達Alarm(須重開電)")]
        DriverAlarm,

        /// <summary>
        ///目前沒有用到 MotorState.Error
        /// </summary>
        [Description("軸卡其他異常")]
        Error,

        #region FOR_COMPATIBILITY
#if(false)
            Idle = Ready,
            AutoMovingHome = Homing,
            AutoMoving = P_Moving,
            ManualMoving = V_Moving,
            ManualMovingForward = V_Moving + 11,
            ManualMovingBackward = V_Moving + 12,
            LightGateDetected = LimitedStop,
#endif
        #endregion
    }

}
