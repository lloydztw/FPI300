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
    /// 即時影像事件參數
    /// <br/> (1) 發送者 (Sender) 負責 LiveImage 的生命週期.
    /// <br/> (2) 接受者 (EventHandler) 不要對 LiveImage 調用 Dispose().
    /// </summary>
    public class EzLiveImageEventArgs : EventArgs
    {
        /// <summary>
        /// 即時影像:
        /// (1) 如果 IsOpenCV == False, 可以直接將 LiveImage 轉型為 Bitmap, 
        /// (2) 如果 IsOpenCV == True, 可以直接將 LiveImage 轉型為 OpenCvSharp.Mat.
        /// </summary>
        public object LiveImage;

        /// <summary>
        /// 定義 LiveImage 為 OpenCvSharp.Mat 或者是 System.Drawing.Bitmap.
        /// </summary>
        public bool IsOpenCV;

        /// <summary>
        /// 建構式
        /// </summary>
        public EzLiveImageEventArgs(object liveImage, bool isOpenCV = false)
        {
            LiveImage = liveImage;
            IsOpenCV = isOpenCV;
        }
    }
}
