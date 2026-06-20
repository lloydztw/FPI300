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
using System;

namespace JetEazy.Drivers.PLC.Fatek.Comm
{
    public class FkResult
    {
        public int Err
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
        public FkResult(int err, string msg, Exception ex = null)
        {
            Err = err;
            Context = msg;
            Ex = ex;
        }

        public static implicit operator int(FkResult rsp)
        {
            return rsp == null ? rsp.Err : 0;
        }
        public static readonly FkResult OK = new FkResult(0, null, null);
    }
}
