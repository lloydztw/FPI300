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

using EzComm.Uart;
using System;
using System.IO.Ports;
using ErrEventArgs = EzComm.Uart.UartErrEventArgs;
using Result = EzComm.Uart.CUartCmdResult;


namespace EzPlc.Fatek.Comm
{
    /// <summary>
    /// Fatek 通訊指令發送 介面
    /// </summary>
    public interface IxFatekComm : IDisposable
    {
        /// <summary>
        /// 異常事件
        /// </summary>
        event EventHandler<ErrEventArgs> OnError;

        object Sync { get; }

        /// <summary>
        /// 是否為模擬
        /// </summary>
        bool IsSim { get; }

        /// <summary>
        /// 索引字串
        /// </summary>
        string KeyName { get; }

        /// <summary>
        /// 機台編號 (以 1 為起始 編號)
        /// </summary>
        int StationID { get; }

        /// <summary>
        /// 下達指令 (sync) (同步阻塞式調用)
        /// </summary>
        /// <param name="cmd">永宏命令</param>
        /// <param name="timeout">0:代表使用系統默認值</param>
        /// <param name="priority">0:代表使用系統默認值</param>
        /// <returns>就地調用後,返回結果.</returns>
        Result SendCmd(FkCmd cmd, int timeout = 0, int priority = 0);

        /// <summary>
        /// 下達指令 (async) 非同步
        /// </summary>
        /// <param name="cmd">永宏命令</param>
        /// <param name="timeout">0:代表使用系統默認值</param>
        /// <param name="priority">0:代表使用系統默認值</param>
        /// <param name="OnCompleted">命令執行後之回調函式</param>
        void PostCmd(FkCmd cmd, int timeout = 0, int priority = 0, Action<Result> OnCompleted = null);

        /// <summary>
        /// 如果通訊層是使用微軟的串口
        /// </summary>
        CAsyncUART GetSerialPort();
    }
}
