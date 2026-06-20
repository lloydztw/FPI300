#region AUTHOR
/****************************************************************************
 *                                                                          
 * Copyright (c) 2012 Jet Eazy Corp. All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$
 *          20120622 LeTian Chang: Revised for the more robust connection.
 *	        20080701 LeTian Chang: Creation         
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/
#endregion


using EzComm.Utils;
using System;
using System.IO.Ports;
using System.Threading.Tasks;
using JetEazy.LOG;
using QxNums = JetEazy.QxNums;


namespace EzComm.Uart
{
    public class CAsyncUART : IDisposable
    {
        public const bool OPT_EXTERNAL_CHKSUM = true;
        public const bool OPT_RSP_INCLUDE_ETX = true;

        #region PRIVATE_MEMBERS
        private int N_COMMAND_TRY = 3;
        private byte C_STX = 0x02;
        private byte C_ETX = 0x03;
        private int m_timeout;        
        #endregion

        public event EventHandler<UartErrEventArgs> OnError;

        public CAsyncUART(int comPort, int baudRate, int dataBits, Parity p, StopBits s, int timeout, int maxRetrys = 0)
        {
            optLogEnabled = false;

            cqInitCmdQueue();
            comm_Init(comPort, baudRate, dataBits, p, s, timeout);

            if (maxRetrys > 0)
                N_COMMAND_TRY = maxRetrys;
        }
        public void ConfigTerminators(byte stx, byte etx)
        {
            C_STX = stx;
            C_ETX = etx;
        }

        public bool optLogEnabled
        {
            get;
            set;
        }

        public int ComPort
        {
            get;
            private set;
        }
        public int Timeout
        {
            get
            {
                return m_timeout;
            }
            set
            {
                if (value > 0)
                    CUartCmd.DefaultTimeOut = m_timeout = value;
                else
                    m_timeout = CUartCmd.DefaultTimeOut;

                if (m_uart != null)
                {
                    m_uart.WriteTimeout = m_timeout;
                    m_uart.ReadTimeout = m_timeout;
                }
            }
        }

        public bool IsOpen
        {
            get
            {
                return (m_uart != null && m_uart.IsOpen);
            }
        }
        public void Open()
        {
            comm_Open();
        }
        public void Close()
        {
            comm_Close();
        }
        public void Dispose()
        {
            comm_Dispose();
        }

        public Task<CUartCmdResult> SendCmdAsync(string cmdStr, Func<string, CUartCmdResult> responseParser, int retryCnt = 0, int timeout = 0)
        {
            var cmd = new CUartCmd(cmdStr, responseParser, retryCnt, timeout);
            return SendCmdAsync(cmd, 0);
        }
        public CUartCmd PostCmd(string cmdStr, Func<string, CUartCmdResult> responseParser, int retryCnt = 0, int timeout = 0)
        {
            var cmd = new CUartCmd(cmdStr, responseParser, retryCnt, timeout);
            PostCmd(cmd, 0);
            return cmd;
        }

        public Task<CUartCmdResult> SendCmdAsync(CUartCmd cmd, int priority)
        {
            //////cmd.Reset();
            //////cmd.Priority = priority;
            //////m_cmdQueue.Enqueue(cmd, priority);
            //////TriggerTX();

            PostCmd(cmd, priority);

            return new Task<CUartCmdResult>(() =>
            {
                if (m_uart == null || !m_uart.IsOpen)
                    return CUartCmdResult.NullResult(cmd);

                bool isTimeout = !cmd.Wait();
                var result = cmd.Result;

                #region RETRY_AND_WAIT
                int waitCnt = 0;
                while (isTimeout || result == null)
                {
                    if (++waitCnt >= N_COMMAND_TRY)
                        break;

                    _LOG("SendCmdAsync [Timeout]: ",
                        string.Format("waits again [{0}/{1}] @ {2}",
                        waitCnt, N_COMMAND_TRY, cmd), true);

                    System.Threading.Thread.Yield();

                    isTimeout = !cmd.Wait();
                    result = cmd.Result;
                }
                #endregion

                UartException.ASSERT(!(isTimeout && result == null));
                //> UartException.ASSERT(result != null);

                if (result == null)
                    result = CUartCmdResult.NullResult(cmd);

                return result;
            });
        }
        public void PostCmd(CUartCmd cmd, int priority)
        {
            cmd.Reset();
            cmd.Priority = (byte)priority;
            m_cmdQueue.Enqueue(cmd, priority);
            TriggerTX();
        }
        
        protected void TriggerTX(bool forceTrigger = false)
        {
            Task.Factory.StartNew((arg) =>
                {
                    //lock (m_commSync)       //>>>@ TriggerTX()
                    //try
                    {
                        bool needToTrigger = (bool)arg;

                        if (!needToTrigger)
                        {
                            if (!m_isCommDispatching)
                                needToTrigger = true;
                        }

                        if (needToTrigger)
                        {
                            var cmd = cqCheckOutNextQueuedCmd();
                            if (cmd != null)
                            {
                                _LOG("TX [Trigger]: ", cmd);
                                comm_Write(cmd.CmdBytes);
                            }
                        }
                    }
                    //catch(Exception ex)
                    //{
                    //    _HANDLE(ex, UartCommErr.Device_Comm_Write_Failed, true, "TriggerTX", false);
                    //}
                },
                forceTrigger
            );
        }


        #region COMM_FUNCTIONS
        object m_commSync = new object();
        SerialPort m_uart = null;
        string m_commAccumStr = null;
        bool m_isCommDispatching = false;
        
        static bool comm_isInAvailablePortList(SerialPort comm)
        {
            if (comm != null)
            {
                string[] allPortNames = System.IO.Ports.SerialPort.GetPortNames();

                //int idx = Array.IndexOf(allPortNames, comm.PortName);
                //System.Diagnostics.Debug.WriteLine(idx);

                foreach (string portName in allPortNames)
                {
                    if (portName == comm.PortName)
                        return true;
                }
            }
            return false;
        }
        SerialPort comm_Init(int comPort, int baudRate, int dataBits, Parity p, StopBits s, int timeout)
        {
            _LOG_INIT(comPort);

            SerialPort comm = new SerialPort();

            try
            {
                _TEST(UartCommErr.Device_Comm_Init_Failed);

                this.ComPort = comPort;
                comm.PortName = $"COM{comPort}";
                comm.BaudRate = baudRate;
                comm.DataBits = dataBits;
                comm.Parity = p;
                comm.StopBits = s;

                if (timeout > 0)
                {
                    m_timeout = timeout;
                    comm.WriteTimeout = timeout;
                    comm.ReadTimeout = timeout;
                }
                else
                {
                    timeout = m_timeout;
                    comm.ReadTimeout = 60 * 1000;
                    comm.WriteTimeout = 60 * 1000;
                }

                comm.ReadBufferSize = 1024;
                comm.WriteBufferSize = 512;

                comm.DataReceived += new SerialDataReceivedEventHandler(comm_OnDataReceived);
            }
            catch (Exception ex)
            {
                JetEazy.QUtilities.QUtility.SafeDisposeObject(comm);
                comm = null;
                _HANDLE(ex, UartCommErr.Device_Comm_Init_Failed, true);
            }

            m_uart = comm;

            return comm;
        }
        void comm_Open()
        {
            try
            {
                lock (m_commSync)           //>>>@ com_Open
                {
                    if (IsOpen)
                        return;

                    _TEST(UartCommErr.Device_Comm_Open_Failed);

                    if (!comm_isInAvailablePortList(m_uart))
                    {
                    }

                    if (m_uart != null)
                    {
                        m_uart.Open();
                        m_uart.DiscardInBuffer();
                        m_uart.DiscardOutBuffer();
                        m_isCommDispatching = false;
                        m_commAccumStr = null;
                    }
                }
            }
            catch (Exception ex)
            {
                comm_Close();
                _HANDLE(ex, UartCommErr.Device_Comm_Open_Failed, true, null, true);
            }
        }
        void comm_Close()
        {
            try
            {
                lock (m_commSync)           //>>>@ Close
                {
                    if (IsOpen)
                    {
                        _TEST(UartCommErr.Device_Comm_Close_Failed);
                        if (m_uart != null && m_uart.IsOpen)
                            m_uart.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _HANDLE(ex, UartCommErr.Device_Comm_Close_Failed, true, null, true);
            }
        }
        void comm_Dispose()
        {
            comm_Close();
            try
            {
                if (m_uart != null)
                {
                    _TEST(UartCommErr.Device_Comm_Dispose_Failed);
                    m_uart.Dispose();
                    m_uart = null;
                }
            }
            catch (Exception ex)
            {
                _HANDLE(ex, UartCommErr.Device_Comm_Dispose_Failed, true);
            }
        }
        void comm_OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string responseStr = null;

            #region READ_IN_DATA_BUF
            //try
            {
                lock (m_commSync)           //>>>@ comm_OnDataReceived
                {
                    //_TEST(UartCommErr.Device_Comm_Callback_Failed_RX);

                    if (!IsOpen)
                        return;

                    byte[] snapshotBuf;
                    int N = comm_Read(out snapshotBuf);

                    for (int i = 0; i < N; i++)
                    {
                        var b = snapshotBuf[i];

                        if (b == C_STX)
                        {
                            m_commAccumStr = null;                          //>>>@ stx @ OnDataReceived
                            m_commAccumStr += Convert.ToChar(b);
                        }
                        else if (b == C_ETX)
                        {
                            if (OPT_RSP_INCLUDE_ETX)
                                m_commAccumStr += Convert.ToChar(b);

                            var isCheckSumOk = OPT_EXTERNAL_CHKSUM ?
                                ChkSum.examCheckSum(m_commAccumStr, OPT_RSP_INCLUDE_ETX) :
                                true;

                            if (isCheckSumOk)
                            {
                                responseStr = m_commAccumStr;               //>>>@ ETX && check-sum ok @ OnDataReceived
                                m_uart.DiscardInBuffer();                   //>>>@ ETX && check-sum ok @ OnDataReceived
                                m_commAccumStr = null;                      //>>>@ continue to accummulate bytes in buf.
                            }
                            else
                            {
                                responseStr = null;
                                m_commAccumStr = null;
                            }
                        }
                        else
                        {
                            m_commAccumStr += Convert.ToChar(b);
                        }
                    }
                }
            }
            //catch (Exception ex)
            //{
            //    _HANDLE(ex, UartCommErr.Device_Comm_Callback_Failed_RX, true);
            //    return;
            //}
            #endregion

            if (responseStr == null)
                return;

            m_isCommDispatching = true;

            cqHandleResponse(responseStr);

            var cmd = cqCheckOutNextQueuedCmd();

            #region DISPATCH_QUEUED_CMD
            //try
            {
                //_TEST(UartCommErr.Device_Comm_Callback_Failed_TX);

                if (cmd != null)
                {
                    _LOG("TX: ", cmd);
                    if (!comm_Write(cmd.CmdBytes))
                        m_isCommDispatching = false;
                }
                else
                {
                    m_isCommDispatching = false;
                }
            }
            //catch (Exception ex)
            //{
            //    _HANDLE(ex, UartCommErr.Device_Comm_Callback_Failed_TX, true);
            //    m_isCommDispatching = false;
            //}
            #endregion
        }
        int comm_Read(out byte[] data)
        {
            try
            {
                lock (m_commSync)           //>>>@ comm_Read
                {
                    if (IsOpen)
                    {
                        _TEST(UartCommErr.Device_Comm_Read_Failed);

                        int N = m_uart.BytesToRead;
                        data = new byte[N];
                        int actualN = m_uart.Read(data, 0, N);
                        //> _LOG("CommRX:", Conversion.ToReadableStr(data, 0, N));
                        return actualN;
                    }
                }
            }
            catch (Exception ex)
            {
                comm_Close();
                _HANDLE(ex, UartCommErr.Device_Comm_Read_Failed, true, null, false);
            }

            data = new byte[0];
            return 0;
        }
        bool comm_Write(byte[] data)
        {
            try
            {
                if (data == null || data.Length == 0)
                    return false;

                lock (m_commSync)           //>>>@ comm_Write
                {
                    if (IsOpen)
                    {
                        _TEST(UartCommErr.Device_Comm_Write_Failed);

                        //_LOG("CommTX:", Conversion.ToReadableStr(data));
                        m_uart.DiscardOutBuffer();      
                        m_uart.DiscardInBuffer();       
                        m_uart.Write(data, 0, data.Length);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                comm_Close();
                _HANDLE(ex, UartCommErr.Device_Comm_Write_Failed, true, null, false);
                return false;
            }
        }
        #endregion


        #region CMD_QUEUE
        EzPriorityQueue<CUartCmd> m_cmdQueue = null;
        void cqInitCmdQueue()
        {
            if (m_cmdQueue == null)
            {
                //>>> int N = Enum.GetValues(typeof(CmdPriority)).Length;
                //>>> m_cmdQueue = new EzPriorityQueue<CUartCmd>(N);
                var arr = (CmdPriority[])Enum.GetValues(typeof(CmdPriority));
                int[] priorities = Array.ConvertAll(arr, p => (int)p);
                m_cmdQueue = new EzPriorityQueue<CUartCmd>(priorities);
            }
        }
        bool cqHandleResponse(string responseStr)
        {
            try
            {                
                if (responseStr == null)
                    return false;

                var cmd = m_cmdQueue.ActiveItem;
                if (cmd == null)
                    return false;

                _TEST(UartCommErr.Device_Comm_Callback_Failed_Handle_RSP);
                int err = cmd.handleResponse(responseStr);

                _TEST(UartCommErr.CmdRetry_Overflow, cmd);
                if (err == 0)
                {
                    _LOG("RX: ", responseStr);

                    m_cmdQueue.DismissActiveItem(cmd);

                    if (cmd.Priority == (int)CmdPriority.RoundRobin)
                    {
                        cmd.Reset();
                        m_cmdQueue.Enqueue(cmd, cmd.Priority);
                    }

                    return true;
                }
                else
                {
                    _LOG("RX: [garbage]: ", responseStr);

                    if (cmd.RetryCnt < N_COMMAND_TRY)
                    {
                        _LOG(string.Format("Retry [{0}]: ", cmd.RetryCnt), cmd, true);
                        cmd.SetRetryAgain();
                        m_cmdQueue.DismissActiveItem(cmd);
                        m_cmdQueue.Enqueue(cmd, cmd.Priority);
                    }
                    else
                    {
                        m_cmdQueue.DismissActiveItem(cmd);
                        var errMsg = string.Format("(Retry > {0}) @ {1}", N_COMMAND_TRY, cmd.ToString());
                        //var ex = new ApplicationException(errMsg);
                        _HANDLE(null, UartCommErr.CmdRetry_Overflow, false, errMsg);
                    }
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                _HANDLE(ex, UartCommErr.Device_Comm_Callback_Failed_Handle_RSP, true);
            }

            return false;
        }
        CUartCmd cqCheckOutNextQueuedCmd()
        {
            var cmd = m_cmdQueue.ActiveItem;

            if (cmd == null)
            {
                //int N = m_cmdQueue.TotalPriorities;
                //for (int pri = 0; pri < N; pri++)
                //{
                //    cmd = m_cmdQueue.PopOutNextActive(pri);
                //    if (cmd != null)
                //        return cmd;
                //}

                foreach (var cmdNext in m_cmdQueue.PopOutNextActive())
                {
                    if (cmdNext != null)
                        return cmdNext;
                }
            }

            return cmd;
        }
        #endregion


        #region DUMP_AND_LOG
        private QxLog m_log = new QxLog();
        private void _LOG_INIT(int comPort)
        {
            if (m_log != null)
            {
                m_log.UserName = this.GetType().Name;
                m_log.Path = "D:\\paso.log\\com_" + comPort.ToString();
            }
        }
        private void _LOG(string tag, CUartCmd cmd, bool forceDump = false)
        {
            if ((forceDump || optLogEnabled) && cmd != null)
            {
                _LOG(tag, cmd.ToString(), forceDump);
            }
        }
        private void _LOG(string tag, string msg, bool forceDump = false)
        {
            if (forceDump || optLogEnabled)
            {
                m_log.Log(tag + EzComm.Utils.Conversion.ToReadableStr(msg));
            }
        }
        public bool _HANDLE(Exception ex, UartCommErr err, bool isFatal, string extraMsg = null, bool throwEx = false)
        {
            var totalMsg = new System.Text.StringBuilder();
            totalMsg.Append($"[COM{ComPort}]");

            if (ex is UartException)
            {
                UartException exu = (UartException)ex;
                err = exu.Err;
                isFatal = exu.IsFatal;
                extraMsg = exu.ExtraMsg;
                ex = exu.Excp;
            }

            string errTxt = QxNums.GetEnumDescription(err);
            totalMsg.Append(errTxt);

            if (extraMsg != null)
            {
                totalMsg.Append(" : ");
                totalMsg.Append(extraMsg.Replace(errTxt, ""));
            }

            if (ex != null)
            {
                totalMsg.Append(" : ");
                totalMsg.Append(ex.Message);
                if (ex.StackTrace != null)
                    totalMsg.Append("\n\r" + ex.StackTrace.Replace("於", "\n\r @ "));
            }

            var msg = totalMsg.ToString();
            if (m_log != null)
                m_log.Log(msg);

            var excp = new UartException(err, ex, isFatal, msg);
            var ev = new UartErrEventArgs(excp);
            ev.Cancel = true;

            if (OnError != null)
            {
                OnError(this, ev);
                //> ev.WaitAck();
            }

            bool go = !ev.Cancel;

            if (!go)
            {
                if (throwEx)
                    throw excp;
            }

            return go;
        }
        #endregion


        #region STRESS_TEST
        DateTime m_stressTestTm0 = DateTime.Now;
        void _TEST(UartCommErr err, CUartCmd cmd = null)
        {
            //double secs = (DateTime.Now - m_stressTestTm0).TotalSeconds;

            //switch (err)
            //{
            //    //case UartCommErr.Device_Comm_Open_Failed:
            //    //    throw new UartException(err, null, true, "StressTest");
            //    //    break;

            //    //case UartCommErr.Device_Comm_Read_Failed:
            //    //    if (m_uart.IsOpen && secs > 15)
            //    //        throw new UartException(err, null, true, "StressTest");
            //    //    break;

            //    //case UartCommErr.Device_Comm_Write_Failed:
            //    //    if (m_uart.IsOpen && secs > 15)
            //    //        throw new UartException(err, null, true, "StressTest");
            //    //    break;

            //        //case UartCommErr.CmdRetry_Overflow:
            //        //    if (m_uart.IsOpen && secs > 15)
            //        //    {
            //        //        if (cmd != null)
            //        //            break;
            //        //    }
            //        //    break;
            //}
        }
        #endregion


        public SerialPort GetSerialPort()
        {
            return m_uart;
        }
    }
}

