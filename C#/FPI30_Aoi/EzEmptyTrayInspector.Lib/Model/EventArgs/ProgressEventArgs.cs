#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;


namespace EzEmptyTrayInspector.Model
{
    public class ProgressEventArgs : EventArgs
    {
        public string Message;
        public int Total;
        public int Step;

        public ProgressEventArgs(string message, int total = 0, int step = 0)
        {
            Message = message;
            Total = total;
            Step = step;
        }
    }
}
