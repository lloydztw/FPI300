#region AUTHOR
/*
 * EzPlc.Fatek
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-05 LeTian Chang: Integration with EzIO
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzComm.Utils;
using EzIO.Mem;
using PlcCommSettings = EzComm.EzUartSettings;
using PlcIoDevice = EzPlc.Fatek.FatekIoDevice;
using PlcIoMemory = EzPlc.Fatek.FatekIoMemory;

namespace EzPlc.Fatek
{
    /// <summary>
    /// EzPlc.Fatek 最外層的 Factory Class, 一切由此生成.
    /// </summary>
    public class EzFatekFactory
    {
        #region PRIVATE_DATA
        static QxObjFactory<string> _devFactory = new QxObjFactory<string>();
        static QxObjFactory<object> _memFactory = new QxObjFactory<object>();
        #endregion

        public static IoDevice OpenDevice(PlcCommSettings settings, int stationID = 1)
        {
            var key = KEY(settings);

            // 嘗試從 _devFactory 找出已經註冊的 IoDevice 物件;
            // 如果沒有找到, 則其內部 會自動調用生成函式, 生成新物件, 並自動註冊.
            var device = _devFactory.Instance(key, (arg) =>
            {
                var dev = new PlcIoDevice(settings, stationID);
                System.Diagnostics.Debug.Assert(key == KEY(dev));
                Hook(dev);  // 為新物件, 掛上 自動解除註冊 機制
                return dev;
            });

            return device;
        }
        public static IoMemory GetIoMemory(IoDevice deivce, bool usingHost32 = false)
        {
            if (deivce == null)
                return null;

            var key = KEY(deivce);

            // 嘗試從 _memFactory 找出已經註冊的 IoMemory 物件;
            // 如果沒有找到, 則其內部 會自動調用生成函式, 生成新物件, 並自動註冊.
            // 自動解除註冊 會由 device 處理.
            var ioMem = _memFactory.Instance(key, (arg) =>
            {
                var mem = new PlcIoMemory(usingHost32);
                mem.BindDevice(deivce);
                return mem;
            });

            return ioMem;
        }
        public static void DisposeAll()
        {
            _devFactory.DisposeAll();
            _memFactory.DisposeAll();
        }

        #region PRIVATE_DATA
        static void Hook(IoDevice device)
        {
            if (device != null)
                device.OnFinalDisposing += (s, e) => Unregister(s as IoDevice);
        }
        static void Unregister(IoDevice device)
        {
            if (device != null)
            {
                var key = KEY(device);
                _devFactory.Unregister(key);
                _memFactory.Unregister(key);
            }
        }
        static string KEY(PlcCommSettings settings)
        {
            return settings == null ? string.Empty : 
                   PlcCommSettings.GetKeyName("Fatek", settings.ComPort, settings.IsSim);
        }
        static string KEY(IoDevice device)
        {
            return device?.KeyName ?? string.Empty;
        }
        #endregion
    }



    /// <summary>
    /// Fatek 對 IoDevice 的擴增函式群
    /// </summary>
    public static class EzFatekExtensions
    {
        /// <summary>
        /// 從 IoDevice 取得 FatekIoMemory
        /// </summary>
        public static IoMemory GetFatekIoMemory(this IoDevice device)
        {
            return EzFatekFactory.GetIoMemory(device);
        }
    }
}

