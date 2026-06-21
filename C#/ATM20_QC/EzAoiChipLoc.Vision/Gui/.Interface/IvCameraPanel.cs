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

using AwFramework;
using LeTian.JxProps.Gui;
using System.Windows.Forms;


namespace EzDualMatch.GUI
{
    public interface IvCameraPanel : IView
    {
        Control lblCamDeviceInfo { get; }

        #region 相機屬性參數_傳統GUI元件
        CheckBox chkInverse { get; }
        NumericUpDown numBrightness { get; }
        NumericUpDown numContrast { get; }
        NumericUpDown numHardwareGain { get; }
        NumericUpDown numExposureTime { get; }
        ComboBox cboCamRotation { get; }
        #endregion

        /// <summary>
        /// Property 屬性視窗 (JX)
        /// <br/> 調用其 BuildGuiCtrls(IProp property) 函式,
        /// <br/> 會將 property 裡面所有定義的欄位資料,
        /// <br/> 自動生成對應的 NumericUpDown, CheckBox, TextBox, ... 等等 GUI 元件.
        /// <br/>  (此演示範例, 所有繼承自 IProp 的 Class 都以 Jx*Settings 命名)
        /// <br/>  (需要安裝 LeTian.JxProps.dll 與 LeTian.JxProps.Gui.dll)
        /// </summary>
        IxPropsViewer PropsViewer { get; }

        #region 按鈕
        Button btnStop { get; }
        Button btnLiveMode { get; }
        Button btnSnapshot { get; }
        Button btnBrowse { get; }
        #endregion
    }
}
