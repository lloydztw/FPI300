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

using System;
using System.ComponentModel;
using QxNums = JetEazy.QxNums;


namespace EzComm.Uart
{
    public class UartException : Exception
    {
        protected UartException()
        {
        }

        public UartException(UartCommErr err, Exception ex, bool isFatal, string extraMsg = null)
            : base(QxNums.GetEnumDescription(err), ex)
        {
            Err = err;
            IsFatal = isFatal;
            ExtraMsg = extraMsg;
        }
        public UartCommErr Err
        {
            get;
            private set;
        }
        public bool IsFatal
        {
            get;
            protected set;
        }
        public string ExtraMsg
        {
            get;
            private set;
        }
        public Exception Excp
        {
            get { return this.InnerException; }
        }

        internal static void ASSERT(bool condition)
        {
            if (!condition)
            {
                System.Diagnostics.Trace.Assert(condition);
                System.Diagnostics.Trace.WriteLine("DEBUG: ASSERT");
            }
        }
    }



    public class UartErrEventArgs : DoWorkEventArgs
    {        
        public UartErrEventArgs(UartException ex) : base(ex)
        {
        }
        public UartException Excp
        {
            get { return (UartException)Argument; }
        }
        public UartCommErr Err
        {
            get { return Excp.Err; }
        }
        public bool IsFatal
        {
            get { return Excp.IsFatal; }
        }
        public string Message
        {
            get { return Excp.ExtraMsg; }
        }

        private System.Threading.ManualResetEvent m_waitEv = new System.Threading.ManualResetEvent(false);
        public void Ack(bool cancel)
        {
            this.Cancel = cancel;
            m_waitEv.Set();
        }
        public void WaitAck(int timeout = -1)
        {
            m_waitEv.WaitOne(timeout);
        }
    }
}