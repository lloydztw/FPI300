#region AUTHOR
/*
 * EzPlc.Fatek
 * 
 * Copyright (C) 2026
 * 2026-04-05 LeTian Chang : Integration with EzIO
 * 2013-07-11 LeTian Chang : 重整
 * 2019-11-30 LeTian Chang : Creation   
 *  
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzComm;
using EzIO.Mem;

namespace EzPlc.Fatek
{
    /// <summary>
    /// FatekIoDevice 對 IoDevice 與 EzUartSettings 的擴增函式群
    /// </summary>
    public static class FatekIoDeviceExtensions
    {
        /// <summary>
        /// 從 IoDevice 取得 FatekIoMemory
        /// </summary>
        public static IoMemory GetFatekIoMemory(this IoDevice device)
        {
            //為 IoDevice 擴充 GetFatekIoMemory Method
            return EzPlcFatekFactory.GetIoMemory(device);
        }

        /// <summary>
        /// 從 EzUartSettings 取得 FatekIoDevice KeyName
        /// </summary>
        public static string GetFatekKeyName(this EzUartSettings settings)
        {
            //為 EzUartSettings 擴充 GetFatekKeyName
            if (settings != null)
                return EzUartSettings.GetKeyName("Fatek", settings.ComPort, settings.IsSim);
            else
                return "Fatek";
        }
    }
}

