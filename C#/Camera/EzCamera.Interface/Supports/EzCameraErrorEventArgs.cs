#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-04-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using System;

namespace EzCamera.Interface
{
    /// <summary>
    /// 相機異常事件參數
    /// </summary>
    public class EzCameraErrorEventArgs : EventArgs
    {
        public EzCameraError Error;
        public Exception Exception;
        public EzCameraErrorEventArgs(EzCameraError err, Exception ex = null)
        {
            Error = err;
            Exception = ex;
        }
    }
}
