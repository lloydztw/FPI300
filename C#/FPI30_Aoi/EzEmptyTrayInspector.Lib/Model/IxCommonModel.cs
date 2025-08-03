#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2023-08-23 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;


namespace EzAoiEmptyTrayInspector.Model
{
    public interface IxCommonModel : IDisposable
    {
        event EventHandler OnStateChanged;

        int ID { get; }
        object State { get; }
        bool IsReady();
        bool IsError();
        bool IsSafeToExit();
    }
}
