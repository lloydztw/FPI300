#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
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

using AwFramework;
using JetEazy.ImageViewerEx;
using OpenCvSharp;


namespace EzAoiEmptyTrayInspector.Gui
{
    public interface IvSingleMatchView : IView
    {
        IvImageViewer ImageViewer { get; }

        void UpdateImageSrcName(string srcName);
        void UpdateMatchState(object state);
        void UpdateStatusInfo(string msg);

        /// <summary>
        /// 2026-0905 新增 function
        /// </summary>
        void UpdateImage(Mat srcImg, string srcName, bool disposeSrc);
    }
}
