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
using JetEazy.OpenCV.Viewer;
using System.Windows.Forms;


namespace EzEmptyTrayInspector.Gui
{
    public interface IvSingleMatchView : IView
    {
        CvzQuickImageViewPanel quickImageViewPanel { get; }
        IvImageViewer ImageViewer { get; }

        //Button btnOpenFile { get; }
        //Button btnRunMatch { get; }
        //Button btnResetClear { get; }
        //Button btnPickGolden { get; }
        //Button btnCombine { get; }

        void UpdateMatchState(object state);
        void UpdateImageSrcName(string srcName);
        void UpdateStatusInfo(string msg);
    }
}
