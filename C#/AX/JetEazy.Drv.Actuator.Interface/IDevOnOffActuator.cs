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


namespace JetEazy.Actuactor
{
    /// <summary>
    /// 氣壓缸 or 真空 等 單點驅動裝置
    /// </summary>
    public interface IDevOnOffActuator : IDevActuator
    {
        /// <summary>
        /// On/Off
        /// </summary>
        bool Action { get; set; }

        bool IsSensorOn { get; }
        bool IsSensorOff { get; }

        bool Has2ndSensor { get; }
        bool Is2ndSensorOn { get; }
        bool Is2ndSensorOff { get; }
    }
}
