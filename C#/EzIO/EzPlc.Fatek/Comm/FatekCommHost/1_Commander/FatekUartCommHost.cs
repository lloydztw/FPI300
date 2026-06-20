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


namespace EzPlc.Fatek.Comm
{
    /// <summary>
    /// Fatek 通訊指令發送器 
    /// <br/> 支援 非同步 與 同步阻塞式 兩種模式 之 指令發送
    /// </summary>
    public partial class FatekUartCommHost : QxPtr<IxFatekComm>, IxFatekComm
    {
        #region NLOG
        private static NLog.Logger s_nlog = null;
        private static NLog.Logger _LOG
        {
            get
            {
                if (s_nlog == null)
                {
                    s_nlog = NLog.LogManager.GetCurrentClassLogger();
                }
                return s_nlog;
            }
        }
        #endregion

        #region PRIVATE_MEMBERS
        private readonly object _cmdLock = new object();
        private CAsyncUART _uart = null;
        private int _stationID;
        private int _timeout;
        #endregion

        public event EventHandler<UartErrEventArgs> OnError;

        internal FatekUartCommHost(int comPort, int baudRate, int timeout, int maxRetryCnt, int stationID)
            : this(new EzUartSettings(comPort, baudRate, timeout:timeout, maxRetryCnt: maxRetryCnt), stationID)
        {
        }
        internal FatekUartCommHost(EzUartSettings settings, int stationID)
        {
            lock (_cmdLock)
            {
                base.m_obj = this;
                this._stationID = Math.Max(stationID, 1);
                initUart(settings.ComPort, settings.BaudRate, settings.Timeout, settings.MaxRetryCount);
                _timeout = settings.Timeout;
            }
        }
        public object Sync
        {
            get => _cmdLock;
        }

        CAsyncUART IxFatekComm.GetSerialPort()
        {
            return _uart;
        }
        protected override void OnDisposing()
        {
            base.OnDisposing();
            disposeUart();
        }

        public string KeyName => EzUartSettings.GetKeyName("Fatek", ComPort, IsSim);
        public override string ToString()
        {
            return KeyName;
        }

        public bool IsSim
        {
            get => false;
        }
        public int ComPort
        {
            get => _uart!=null ? _uart.ComPort : 0;
        }
        public int StationID
        {
            get => _stationID;
        }

        public CUartCmdResult SendCmd(FkCmd fkCmd, int timeout = 0, int priority = 0)
        {
            try
            {
                lock (_cmdLock)
                {
                    if (timeout <= 0) timeout = _timeout;
                    var result = _sendCmd(fkCmd, timeout, priority);
                    return result;
                }
            }
            catch (FatekErrorResultException ex)
            {
                // [CAUTION]: 不要在 m_cmdSync lock 裡面 call _HANDLE !!!
                _HANDLE(ex, true);
                return ex.Result;
            }
        }
        public void PostCmd(FkCmd fkCmd, int timeout, int priority, Action<CUartCmdResult> OnCompleted)
        {
            lock (_cmdLock)
            {
                _postCmd(fkCmd, timeout, priority, OnCompleted);
            }
        }

        #region MAJOR_IMPLEMENTATIONS
        private CUartCmdResult _sendCmd(FkCmd fkCmd, int timeout = 0, int priority = 0)
        {
            CUartCmd uartCmd = null;

            try
            {
                if (_uart == null || fkCmd == null)
                    return CUartCmdResult.NullResult(null);

                fkCmd.changeStationID(_stationID);

                uartCmd = new CmdAdapter(fkCmd, timeout);

                var tsk = _uart.SendCmdAsync(uartCmd, priority);

                tsk.RunSynchronously();      
                
                var result = tsk.Result;

                //> UartException.ASSERT(result != null);
                if (result == null || result.Error!=0)
                {
                    if (result == null)
                        result = CUartCmdResult.NullResult(uartCmd);

                    // 統一經由 FatekErrorResultException 由外部處理 result.Error
                    throw new FatekErrorResultException(result, "FatekUartHost.sendCmd");
                }

                return result;
            }
            catch (Exception ex)
            {
                // 統一經由 FatekErrorResultException 由外部處理 result.Error
                if (ex is FatekErrorResultException)
                    throw ex;

                // 統一經由 FatekErrorResultException 由外部處理 result.Error
                var result = new CUartCmdResult(UartCommErr.Misc_Exception, uartCmd, ex);
                throw new FatekErrorResultException(result, "FatekUartHost.sendCmd_ex");
            }
        }
        private void _postCmd(FkCmd fkCmd, int timeout, int priority, Action<CUartCmdResult> OnCompleted)
        {
            if (_uart == null || fkCmd == null)
                return;

            fkCmd.changeStationID(_stationID);
            _uart.PostCmd(new CmdAdapter(fkCmd, timeout, OnCompleted), priority);
        }
        #endregion

        #region PRIVATE_UART_FUNCTIONS
        private bool initUart(int comPort, int baudrate, int timeout, int maxRetryCnt)
        {
            if (_uart == null)
            {
                FkCmd.OPT_INTERNAL_CHKSUM = !CAsyncUART.OPT_EXTERNAL_CHKSUM;
                FkCmd.OPT_RSP_INCLUDE_ETX = CAsyncUART.OPT_RSP_INCLUDE_ETX;

                _uart = new CAsyncUART(comPort, baudrate, 7, Parity.Even, StopBits.One, timeout, maxRetryCnt);
                _uart.ConfigTerminators((byte)FatekCommConsts.STX, (byte)FatekCommConsts.ETX);
                _uart.Open();

                if (!_uart.IsOpen)
                {
                    //> System.Diagnostics.Trace.WriteLine("@@@  CFatekPLC: Uart can not be open!");
                    _LOG_ERROR($"[Error] can not open ComPort={comPort}, Baudrate={baudrate}");
                    _uart.Dispose();
                    _uart = null;
                }

                if (_uart != null)
                {
                    _uart.OnError += (s, e) => OnError?.Invoke(this, e);

                    //try
                    //{
                    //    //string ret = Echo("Hello, FATEK!");
                    //    //System.Diagnostics.Trace.WriteLine("@@@ CFatekPLC: echo= " + ret);
                    //}
                    //catch
                    //{
                    //    _uart.Close();
                    //    _uart.Dispose();
                    //    _uart = null;
                    //}
                }
            }
            return (_uart != null && _uart.IsOpen);
        }
        private void disposeUart()
        {
            if (_uart != null)
            {
                try { _uart.Close(); }
                catch { }
                try { _uart.Dispose(); }
                catch { }
                _uart = null;
            }
        }
        #endregion

        #region PRIVATE_DUMP_FUNCTIONS
        private void _LOG_ERROR(string msg)
        {
            if (_stationID > 1)
                msg = $"[SID={_stationID}] : " + msg;
            _LOG.Error(msg);
        }
        #endregion

        #region EXCEPTION_HANDLERS
        public class FatekErrorResultException : Exception
        {
            public FatekErrorResultException(CUartCmdResult result, string tag)
                : base(tag)
            {
                Result = result;
            }
            public CUartCmdResult Result
            {
                get;
                private set;
            }
        }
        internal bool _HANDLE(FatekErrorResultException fkExcp, bool isFatal, bool throwEx = false)
        {
            bool go = !isFatal;

            var result = fkExcp.Result;
            var tag = fkExcp.Message;
            var err = result.Error;
            var innerEx = result.Ex;

            if (_uart == null)
            {
                _LOG_ERROR(tag + " : " + MSG.FormatErrorMsg(result));
            }
            else
            {
                if(MSG.IsUartCommErr(err))
                {
                    // Timeout 還是嘗試讓 User 決定是否重新 Retry.
                    if (UartCommErr.Timeout == (UartCommErr)err)
                        isFatal = false;

                    go = _uart._HANDLE(innerEx, (UartCommErr)err, isFatal, MSG.FormatErrorMsg(result));
                }
                else
                {
                    go = _uart._HANDLE(innerEx, UartCommErr.Misc_Exception, isFatal, MSG.FormatErrorMsg(result));
                }
            }

            if (!go && throwEx)
            {
                throw (innerEx != null) ? innerEx : fkExcp;
            }

            return go;
        }
        #endregion
    }

    partial class FatekUartCommHost
    {
        #region CmdAdapter
        /// <summary>
        /// 橋接 FkCmd (永宏命令) 至 CUardCmd
        /// </summary>
        class CmdAdapter : CUartCmd
        {
            FkCmd m_fkCmd;
            public CmdAdapter(FkCmd cmd, int timeout, Action<CUartCmdResult> OnCompleted = null)
            {
                m_fkCmd = cmd;
                base.OnCompleted = OnCompleted;
                base.CmdBytes = cmd.CmdBytes;
                base.config(new Func<string, CUartCmdResult>((rspStr) =>
                    {
                        var fkResult = m_fkCmd.HandleResponse(rspStr);
                        if (fkResult == null)
                            return CUartCmdResult.NullResult(this);
                        else
                            return new CUartCmdResult(fkResult);
                    }),
                    0, timeout);
            }
            public override byte Priority
            {
                get
                {
                    return m_fkCmd != null ?
                        m_fkCmd.Priority :
                        base.Priority;
                }
                set
                {
                    base.Priority = value;
                    if (m_fkCmd != null)
                        m_fkCmd.Priority = value;
                }
            }
        }
        #endregion
    }
}
