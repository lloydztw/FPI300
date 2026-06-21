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
using LeTian.JxProps;
using LeTian.JxProps.Gui;
using System.Windows.Forms;


namespace JetEazy.GUI
{
    public partial class GjxCameraPropertyPanel : UserControl, IView
    {
        public GjxCameraPropertyPanel()
        {
            InitializeComponent();

            HandleCreated += new System.EventHandler((sender, e) =>
            {
                frmOwner = FindForm();
            });
        }

        #region GUI_LINKS
        public Form frmOwner { get; private set; }
        Control IView.Window => this;

        //Control IvCameraPanel.lblCamDeviceInfo => lblDeviceInfo;
        //CheckBox IvCameraPanel.chkInverse => null;
        //NumericUpDown IvCameraPanel.numBrightness => null;
        //NumericUpDown IvCameraPanel.numContrast => null;
        //NumericUpDown IvCameraPanel.numHardwareGain => null;
        //NumericUpDown IvCameraPanel.numExposureTime => null;
        //ComboBox IvCameraPanel.cboCamRotation => null;
        ////Control IvCameraPanel.lblFrameCount => null;

        //Button IvCameraPanel.btnStop => btnStop;
        //Button IvCameraPanel.btnLiveMode => btnLiveMode;
        //Button IvCameraPanel.btnSnapshot => btnSnapshot;
        //Button IvCameraPanel.btnBrowse => btnBrowse;
        #endregion

        /// <summary>
        /// Property 屬性視窗 (JX)
        /// <br/> 調用其 BuildGuiCtrls(IProp property) 函式
        /// <br/> 會將 property 裡面所有定義的欄位資料,
        /// <br/> 自動生成對應的 NumericUpDown, CheckBox, TextBox, ... 等等 GUI 元件.
        /// <br/>  (此演示範例, 所有繼承自 IProp 的 Class 都以 Jx*Settings 命名)
        /// <br/>  (需要安裝 LeTian.JxProps.dll 與 LeTian.JxProps.Gui.dll)
        /// </summary>
        public IxPropsViewer PropsViewer => gwPanePropsViewer1;

        public void Attach(IProp property)
        {
            PropsViewer.BuildGuiCtrls(property);
        }
    }
}
