#region AUTHOR
/****************************************************************************
 *                                                                          
 * Copyright (c) 2013 LETIAN CHANG All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$    
 *	        20130711 LeTian Chang : Creation         
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/
#endregion

using System;


namespace EzComm.Utils
{
    public class ExResult
    {
        public int Error
        {
            get;
            private set;
        }
        public string Context
        {
            get;
            private set;
        }
        public Exception Ex
        {
            get;
            private set;
        }
        
        public ExResult(int err, string msg, Exception ex = null)
        {
            if (err != 0 || ex != null)
            {
                System.Diagnostics.Debug.WriteLine($"ExResult (err={(EzComm.Uart.UartCommErr)err}) {msg}");
            }

            Error = err;
            Context = msg;
            Ex = ex;
        }

        public static implicit operator int(ExResult rsp)
        {
            return rsp == null ? rsp.Error : 0;
        }
        public static readonly ExResult OK = new ExResult(0, null, null);
    }
}
