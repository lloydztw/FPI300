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


namespace EzComm
{
    public class EzTcpIpSettings
    {
        public string IP;
        public int Port;
        public int Timeout;
        public int RetryDelay;
        public int MaxRetryCount;
        public bool IsSim;
        public bool UsingRandomSim;

        public EzTcpIpSettings(string ip = "127.0.0.1",
                               int port = 8080,
                               int timeout = 1000,
                               int retryDelay = 10,
                               int maxRetryCount = 3,
                               bool isSim = false)
        {
            this.IP = ip;
            this.Port = port;
            this.Timeout = timeout; // > 0 ? timeout : 1000;
            this.RetryDelay = retryDelay;   // > 0 ? retryDelay : 10;
            this.MaxRetryCount = maxRetryCount; // > 0 ? maxRetryCount : 3;
            this.IsSim = isSim;
        }

        public void NormalizeByDefaultValues(int port, int timeout, int retryDelay, int maxRetryCount)
        {
            if (string.IsNullOrEmpty(IP))
                IP = "127.0.0.1";
            if (Port <= 0)
                Port = port;
            if (Timeout <= 0)
                Timeout = timeout;
            if (RetryDelay <= 0)
                RetryDelay = retryDelay;
            if (MaxRetryCount <= 0)
                MaxRetryCount = maxRetryCount;
        }

        public static string GetKeyName(string vendor, string ip, int port, bool isSim)
        {
            return isSim ? $"{vendor}(SIM)\\{ip}:{port}" : $"{vendor}\\{ip}:{port}";
        }

        public override string ToString()
        {
            return GetKeyName("", IP, Port, IsSim);
        }
    }
}

