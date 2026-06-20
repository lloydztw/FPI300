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

using System.Collections.Generic;
using System.IO.Ports;
using System.Text;


namespace EzComm.Uart
{
    internal class CUartBuf
    {
        #region PRIVATE_DATA
        private bool m_isJustKeepOneRX = true;
        private StringBuilder m_buf = new StringBuilder();
        private List<string> m_rxStrings = new List<string>();
        #endregion

        public byte STX
        {
            get;
            set;
        }
        public byte ETX
        {
            get;
            set;
        }

        public void Reset()
        {
            m_buf = new StringBuilder();
            m_rxStrings.Clear();
        }
        public int ReadIn(SerialPort com)
        {
            //int N = com.BytesToRead;
            //byte[] snapshotBuf = new byte[N];
            //com.Read(snapshotBuf, 0, N);

            int N = com.BytesToRead;
            byte[] snapshotBuf = new byte[N];
            int actualReadCount = com.Read(snapshotBuf, 0, N);

            if (N > 0)
            {
                for (int i = 0; i < actualReadCount; i++)
                {
                    byte b = snapshotBuf[i];
                    if (b == STX)
                    {
                        m_buf = new StringBuilder();
                        m_buf.Append((char)b);
                    }
                    else if (snapshotBuf[i] == ETX)
                    {
                        //================================
                        //>>> The ETX must be excluded !!!
                        //================================
                        if (m_isJustKeepOneRX)
                            m_rxStrings.Clear();

                        m_rxStrings.Add(m_buf.ToString());
                        m_buf = new StringBuilder();
                    }
                    else
                    {
                        m_buf.Append((char)b);
                    }
                }
            }

            return m_rxStrings.Count;
        }

        public List<string> RxStrings
        {
            get { return m_rxStrings; }
        }
        public void DiscardRxStrings(int idx)
        {
            if (m_isJustKeepOneRX || idx < 0)
                m_rxStrings.Clear();
            else
                m_rxStrings.RemoveRange(0, idx + 1);
        }
    }
}

