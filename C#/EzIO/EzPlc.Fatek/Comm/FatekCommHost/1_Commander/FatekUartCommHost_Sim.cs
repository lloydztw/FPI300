#region AUTHOR
/*
 * EzPlc.Fatek
 * Copyright (C) 2026
 * 2026-04-05 LeTian Chang: Integration with EzIO
 * 2019-11-30 LeTian Chang: Creation.
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzComm;
using EzComm.Uart;
using EzComm.Utils;
using System;
using System.IO.Ports;


namespace EzPlc.Fatek.Comm.Sim
{
    /// <summary>
    /// Fatek 通訊指令發送 模擬器 
    /// </summary>
    public partial class FatekUartHostSim : QxPtr<IxFatekComm>, IxFatekComm
    {
        public event EventHandler<UartErrEventArgs> OnError;

        internal FatekUartHostSim(int comPort, int baudRate, int timeout, int maxRetryCnt, int stationID)
            : this(new EzUartSettings(comPort, baudRate, timeout:timeout, maxRetryCnt: maxRetryCnt), stationID)
        {
        }
        internal FatekUartHostSim(EzUartSettings settings, int stationID)
        {
            base.m_obj = this;
            ComPort = settings.ComPort;
            StationID = Math.Max(stationID, 1);
        }
        public object Sync
        {
            get => this;
        }

        CAsyncUART IxFatekComm.GetSerialPort()
        {
            return null;
        }

        public string KeyName => EzUartSettings.GetKeyName("Fatek", ComPort, IsSim);
        public override string ToString()
        {
            return KeyName;
        }

        public bool IsSim
        {
            get => true;
        }
        public int ComPort
        {
            get;
            private set;
        }
        public int StationID
        {
            get;
            private set;
        }

        public CUartCmdResult SendCmd(FkCmd fkCmd, int timeout = 0, int priority = 0)
        {
            return new CUartCmdResult(ExResult.OK);
        }
        public void PostCmd(FkCmd fkCmd, int timeout, int priority, Action<CUartCmdResult> OnCompleted)
        {
        }
    }
}
