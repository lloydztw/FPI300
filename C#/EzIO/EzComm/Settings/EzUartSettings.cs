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

using System.IO.Ports;

namespace EzComm
{
    public class EzUartSettings
    {
        public int ComPort;
        public int BaudRate;
        public int DataBits;
        public Parity Parity;
        public StopBits StopBits;
        public int Timeout;
        public int MaxRetryCount;
        public bool IsSim;
        public bool UsingRandomSim;

        public EzUartSettings(int comPort = 1,
                              int baudRate = 115200,
                              int dataBits = 7,
                              Parity parity = Parity.Even,
                              StopBits stopBits = StopBits.One,
                              int timeout = 1000,
                              int maxRetryCnt = 3,
                              bool isSim = false)
        {
            this.ComPort = comPort;
            this.BaudRate = baudRate;
            this.DataBits = dataBits;
            this.Parity = parity;
            this.StopBits = stopBits;
            this.Timeout = timeout > 0 ? timeout : 3000;
            this.MaxRetryCount = maxRetryCnt > 0 ? maxRetryCnt : 3;
            this.IsSim = isSim || comPort==0 || baudRate == 0; 
        }

        public static string GetKeyName(string vendor, int comPort, bool isSim)
        {
            return isSim ? $"{vendor}[Com={comPort}(SIM)]" : $"{vendor}[Com={comPort}]";
        }
        public override string ToString()
        {
            return GetKeyName("", ComPort, IsSim);
        }
    }
}

