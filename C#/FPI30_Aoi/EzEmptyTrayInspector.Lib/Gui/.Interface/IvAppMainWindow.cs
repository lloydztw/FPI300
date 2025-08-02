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

using EzCamera.GUI;
using System.Windows.Forms;
using AwFramework;


namespace EzDualMatch.GUI
{
    public interface IvAppMainWindow : IView
    {
        /// <summary>
        /// 相機實時影像顯示窗
        /// </summary>
        IvCameraViewer camLiveViewer { get; }

        /// <summary>
        /// 相機屬性參數設定視窗
        /// </summary>
        IvCameraPanel camPropertyPanel { get; }

        IvSingleMatchView matchView { get; }

        /// wndCoordInfo: 倍率,座標,顏色,Fps.
        /// wndBlinker: LiveMode 閃爍燈號. 
        /// <summary>
        /// 倍率,座標,顏色,Fps.
        /// </summary>
        Control lblCamCoordInfo { get; }

        /// <summary>
        /// LiveMode 閃爍燈號.
        /// </summary>
        Control lblCamBlinker { get; }

        /// <summary>
        /// 禎數顯示
        /// </summary>
        Control lblCamFrameCount { get; }

        /// <summary>
        /// 開啟相機選擇
        /// </summary>
        Button btnOpenCamera { get; }
    }
}
