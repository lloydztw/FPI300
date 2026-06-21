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
using System.Threading;

namespace EzAoiChipLocQC.Model
{
    public class ProgressEventArgs : EventArgs
    {
        /// <summary>
        /// sender 要通知給 receiver 的訊息.
        /// </summary>
        public string Message;

        /// <summary>
        /// 進度條總量
        /// </summary>
        public int Total;

        /// <summary>
        /// 目前進度
        /// </summary>
        public int Step;

        /// <summary>
        /// 擴充數據
        /// </summary>
        public object Tag;

        /// <summary>
        /// Cancel Flag: 
        /// 必要時可以由 receiver 端 
        /// 來設定來通知 sender 是否要中斷 process
        /// </summary>
        public bool Cancel = false;

        /// <summary>
        /// 必要時可以由 Reseiver 端, 來卡住 sender 進度.
        /// </summary>
        public ManualResetEvent GoControlByClient = null;

        public ProgressEventArgs(string message, int total = 0, int step = 0)
        {
            Message = message;
            Total = total;
            Step = step;
        }
    }
}
