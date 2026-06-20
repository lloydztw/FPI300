#region AUTHOR
/*
 * EzPlc.Fatek
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-05 LeTian Chang: Integration with EzIO
 * 2012-12-04 LeTian Chang: ReOpen UART when communication failed.
 * 2012-06-22 LeTian Chang: Revised for more robust over RS232 connection.
 * 2008-07-01 LeTian Chang: Creation.
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzComm;
using EzComm.Uart;
using EzComm.Utils;
using EzPlc.Fatek.Comm.Api;
using EzPlc.Fatek.Comm.Sim;
using System;
using QxNums = JetEazy.QxNums;


namespace EzPlc.Fatek.Comm
{
    public class FatekCommFactory
    {
        #region PRIVATE_STATIC_DATA
        private static QxObjFactory<string> _factory = new QxObjFactory<string>();
        #endregion

        public static IxFatekComm OpenPlcComm(EzUartSettings settings, int stationID = 1)
        {
            // 嘗試找出已經註冊的物件; 如果沒有找到, 則其內部 會自動調用生成函式, 生成新物件, 並自動註冊.
            var fatekComm = _factory.Instance(
                    KEY(settings),
                    (arg) => CreateAndConnectComm(settings, stationID)
                );
            return fatekComm;
        }
        public static IxFatekApi OpenPlcApi(EzUartSettings settings, int stationID = 1)
        {
            var fatekComm = OpenPlcComm(settings, stationID);
            if (fatekComm != null)
            {
                var fatekApi = new FatekAPI(fatekComm);
                return fatekApi;
            }
            return null;
        }
        public static void DisposeAll()
        {
            _factory.DisposeAll();
        }

        #region PRIVATE_FUNCTIONS
        static IxFatekComm CreateAndConnectComm(EzUartSettings settings, int stationID)
        {
            IxFatekComm plcComm = null;
            Exception lastEx = null;

            for (int retry = 0; retry < 5; retry++)
            {
                try
                {
                    plcComm = InstanceComm(settings, stationID);
                    System.Threading.Thread.Sleep(500);

                    #region 測試連線
                    var fatekApi = new FatekAPI(plcComm);
                    bool bRunning = fatekApi.IsRunning();
                    if (!bRunning)
                    {
                        fatekApi.Run();
                        System.Threading.Thread.Sleep(500);
                        fatekApi.Echo("Run, FATEK!");
                    }
                    #endregion

                    // 為新物件, 掛上自動反註冊機制
                    Hook(plcComm);
                    return plcComm;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message + $" @ COM{settings.ComPort}");
                    lastEx = ex;
                }

                try
                {
                    plcComm?.Dispose();
                    plcComm = null;
                }
                catch
                {
                    plcComm = null;
                }
            }

            string errMsg = string.Format(
                "[Error] CreateAndConnectComm( comPort = {0} ) : {1}", settings.ComPort,
                QxNums.GetEnumDescription(UartCommErr.Device_Connect_Failed)
            );

            var exx = new ApplicationException(errMsg, lastEx);
            throw exx;
        }
        static IxFatekComm InstanceComm(EzUartSettings settings, int stationID)
        {
            if (settings.IsSim)
            {
                var fatekSim = new FatekUartHostSim(settings, stationID);
                return fatekSim;
            }
            else
            {
                var fatekComm = new FatekUartCommHost(settings, stationID);
                return fatekComm;
            }
        }
        static void Unregister(IxFatekComm plcComm)
        {
            if (plcComm != null)
            {
                var key = KEY(plcComm);
                _factory.Unregister(key);
            }
        }
        static void Hook(IxFatekComm plcComm)
        {
            if (plcComm is QxPtr<IxFatekComm> obj)
                obj.OnFinalDisposing += (s, e) => Unregister(s as IxFatekComm);
        }
        static string KEY(IxFatekComm plcComm)
        {
            return plcComm != null ? plcComm.KeyName : "";
        }
        static string KEY(EzUartSettings settings)
        {
            return settings != null ? EzUartSettings.GetKeyName("Fatek", settings.ComPort, settings.IsSim) : "";
        }
        #endregion

        /// <summary>
        /// 統一掛載 Uart 通訊異常 EventHandler
        /// </summary>
        public static void HookEventHandler(EventHandler<UartErrEventArgs> h)
        {
            foreach (var kv in _factory.EnumKeyAndObjs())
            {
                // 使用安全轉型
                if (kv.Value is IxFatekComm plcComm)
                {
                    plcComm.OnError += h;
                }
            }
        }
    }
}

