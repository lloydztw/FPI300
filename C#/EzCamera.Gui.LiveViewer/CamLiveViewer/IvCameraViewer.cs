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

using EzCamera.Interface;
using JetEazy.ImageViewerEx;
using System.Drawing;
using System.Windows.Forms;

namespace EzCamera.GUI
{
    /// <summary>
    /// 相機即時影像顯示視窗
    /// </summary>
    public interface IvCameraViewer : IvImageViewer
    {
        /// <summary>
        /// 已經綁定之 IEzCamera
        /// </summary>
        IEzCamera Camera { get; }

        /// <summary>
        /// 綁定 IEzCamera
        /// </summary>
        void AttachLiveSource(IEzCamera camera);

        /// <summary>
        /// 解綁 IEzCamera
        /// </summary>
        void DetachLiveSource();

        /// <summary>
        /// 指定以下顯示之目標視窗元件:
        /// wndCoordInfo: 倍率,座標,顏色,Fps.
        /// wndBlinker: LiveMode 閃爍燈號.
        /// 註記: wndXxx, 代所有繼承自 Control 之視窗元件, 
        ///       如 Label, TextBox, Button, ... 等等
        ///       都可以套用.
        /// </summary>
        void Attach(Control wndCoordInfo, Control wndBlinker = null);

        /// <summary>
        /// 重新建立縮放尺度之座標系統轉換
        /// </summary>
        void RebuildViewport();
        
        #region 其他 GUI 設定屬性
        /// <summary>
        /// 格線
        /// </summary>
        bool GridLinesVisible { get; set; }

        /// <summary>
        /// 尺標
        /// </summary>
        bool RulerVisible { get; set; }

        /// <summary>
        /// 十字線
        /// </summary>
        bool CrosshairsVisible { get; set; }

        /// <summary>
        /// 十字線顏色
        /// </summary>
        Color CrosshairsColor { get; set; }
        #endregion
    }
}
