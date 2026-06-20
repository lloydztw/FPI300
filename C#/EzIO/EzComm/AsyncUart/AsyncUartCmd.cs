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
using System.Threading;
using QxNums = JetEazy.QxNums;


namespace EzComm.Uart
{
    public class CUartCmd
    {
        #region PRIVATE_DATA
        ManualResetEventSlim m_completedEvent = new ManualResetEventSlim(false);
        byte[] m_cmdBytes;
        CUartCmdResult m_result = null;
        Func<string, CUartCmdResult> m_responseParser;
        #endregion

        public static int DefaultTimeOut = 3000;

        protected CUartCmd()
        {
        }
        public CUartCmd(string cmd, Func<string, CUartCmdResult> responseParser, int retryCnt = 0, int timeout = 0)
        {
            CmdBytes = EzComm.Utils.Conversion.ToBytes(cmd);
            config(responseParser, retryCnt, timeout);
        }
        public CUartCmd(byte[] cmd, Func<string, CUartCmdResult> responseParser, int retryCnt = 0, int timeout = 0)
        {
            CmdBytes = cmd;
            config(responseParser, retryCnt, timeout);
        }
        protected void config(Func<string, CUartCmdResult> responseParser, int retryCnt = 0, int timeout = 0)
        {
            System.Diagnostics.Trace.Assert(responseParser != null);
            m_responseParser = responseParser;
            Timeout = timeout > 0 ? timeout : DefaultTimeOut;
            RetryCnt = retryCnt;
            //IsSent = false;
        }

        public override string ToString()
        {
            return EzComm.Utils.Conversion.ToReadableStr(m_cmdBytes);
        }
        public byte[] CmdBytes
        {
            get
            {
                return m_cmdBytes;
            }
            protected set
            {
                m_cmdBytes = value;
            }
        }
        public int Timeout
        {
            get;
            private set;
        }

        public virtual byte Priority
        {
            get;
            set;
        }
        public int RetryCnt
        {
            get;
            private set;
        }
        public Action<CUartCmdResult> OnCompleted;
        
        internal int handleResponse(string responseStr)
        {
            var resultNew = OnResponsed(responseStr);

            //> UartException.ASSERT(resultNew != null);
            if (resultNew == null)
                resultNew = CUartCmdResult.NullResult(this);

            resultNew.OwnerCmd = this;
            var err = resultNew.Error;

            //> JetEazy.Tasks.LockFreeUpdate(ref m_result, r => result);
            _safeUpdate(ref m_result, resultNew);

            m_completedEvent.Set();

            if (OnCompleted != null)
                OnCompleted(m_result);

            return err;
        }
        protected virtual CUartCmdResult OnResponsed(string responseStr)
        {
            UartException.ASSERT(m_responseParser != null);
            var resultNew = m_responseParser(responseStr);
            return resultNew;
        }

        public CUartCmdResult Result
        {
            get
            {
                return m_result;
            }
        }
        public int Error
        {
            get
            {
                var resultSnap = m_result;
                return (resultSnap != null) ? resultSnap.Error : -1;
            }
        }

        public bool Wait(int reserved = 0)
        {
            bool ok = m_completedEvent.Wait(Timeout);

            if (!ok)
            {
                // LAST CHANCE to snapshot the m_result
                Thread.MemoryBarrier();
                if (m_result != null && m_result.Error == 0)
                    return true;

                // JetEazy.Tasks.LockFreeUpdate(ref m_result, r => CUartCmdResult.Timeout(this));
                _safeUpdate(ref m_result, CUartCmdResult.Timeout(this));
                return false;
            }

            return true;
        }
        public bool IsCompleted
        {
            get
            {
                return (m_result != null);
            }
        }

        internal void ResetTiming()
        {
            //> m_tm0 = DateTime.Now;
            m_completedEvent.Reset();
        }
        internal void SetRetryAgain()
        {
            Reset(true);
        }
        internal void Reset(bool isRetry = false)
        {
            m_completedEvent.Reset();
            _safeUpdate(ref m_result, null);
            ResetTiming();
            RetryCnt = isRetry ? (RetryCnt + 1) : 0;
        }

        #region PRIVATE_FUNCTIONS
        /// <summary>
        /// Thread-safe update resultDst from resultSrc
        /// </summary>
        /// <param name="resultDst"></param>
        /// <param name="resultSrc"></param>
        private void _safeUpdate(ref CUartCmdResult resultDst, CUartCmdResult resultSrc)
        {
            TaskUtil.LockFreeUpdate(ref resultDst, r => resultSrc);
        }
        #endregion
    }

    

    public class CUartCmdResult : EzComm.Utils.ExResult
    {
        public static CUartCmdResult NullResult(CUartCmd owner)
        {
            return new CUartCmdResult(UartCommErr.Null_Result, owner);
        }
        public static CUartCmdResult Timeout(CUartCmd owner)
        {
            return new CUartCmdResult(UartCommErr.Timeout, owner);
        }

        public CUartCmdResult(UartCommErr err, CUartCmd owner = null, Exception ex = null)
            : base((int)err, QxNums.GetEnumDescription(err), ex)
        {
            OwnerCmd = owner;
        }

        public CUartCmdResult(EzComm.Utils.ExResult src)
            : base(src.Error, src.Context, src.Ex)
        {
        }

        CUartCmdResult(int err, string context)
            : base(err, context)
        {
        }

        public CUartCmd OwnerCmd
        {
            get;
            internal set;
        }
    }
}

