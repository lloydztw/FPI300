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
    /// IEzLiveImageSource
    /// </summary>
    public interface IEzTriggerSource
    {
        /// <summary>
        /// 離開事件之後, LiveImage 會被 Sender 自動 Dispose() 而釋放.
        /// 事件接受者, 不需對 LiveImage 調用 Dispose().
        /// </summary>
        event EventHandler<EzLiveImageEventArgs> OnLiveImage;
        event EventHandler OnLiveModeChanged;

        /// <summary>
        /// 是否為連續取像模式
        /// </summary>
        bool IsLiveMode();

        /// <summary>
        /// 開始連續取像
        /// </summary>
        void StartLiveMode();

        /// <summary>
        /// 停止連續取像
        /// </summary>
        void StopLiveMode();

    }
}
