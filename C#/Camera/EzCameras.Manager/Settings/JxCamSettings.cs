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

using LeTian.JxProps;


namespace EzCamera.Settings
{
    /// <summary>
    /// 相機參數設定
    /// <br/>【說明】
    /// <br/> JxContainer 內可以包含各種 "基礎欄位", 也可以包含另一個 JxContainer 形成巢狀結構.
    /// <br/> "基礎欄位" 皆繼承自 IProp, 有以下已實作可用的 Class:
    /// <br/>  JxBool:   對應 bool 與 CheckBox
    /// <br/>  JxText:   對應 string 與 TextBox 或 Label
    /// <br/>  JxNumber: 對應 decimal 與 NumericUpDown
    /// <br/>  JxEnumSet: 對應 有限長度的 Array 與 ComboBox
    /// </summary>
    public class JxCamSettings : JxContainer
    {
        public JxBool Inverse = new JxBool("Inverse", "影像反向");
        public JxNumber ExposureTime = new JxNumber("ExposureTime", 10, "曝光時間", new Range(2, 2000, 1));
        public JxNumber HardwareGain = new JxNumber("HardwareGain", 0, "相機增益", new Range(0, 15, 0.2m, 2));
        public JxNumber Brightness = new JxNumber("Brightness", "影像亮度", 0, new Range(-100, 100));
        public JxNumber Contrast = new JxNumber("Contrast", "影像對比", 0, new Range(-100, 100));
        public JxEnumSet<int> Rotation = new JxEnumSet<int>("Rotation", "旋轉", new int[] { 0, 90, 180, 270 });
        
        public JxCamSettings()
        {
            Name = "Camera";
            Description = "相機設定";
        }
        public override void OnBindingSubItems()
        {
            //綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                Inverse,
                ExposureTime,
                HardwareGain,
                Brightness,
                Contrast,
                Rotation,
            });
            base.OnBindingSubItems();
        }
    }
}
