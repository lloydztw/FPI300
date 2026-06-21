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

using LeTian.JxProps;

namespace EzAoiChipLocQC.Model
{
    /// <summary>
    /// 外廓 旋轉角度 定位設定
    /// </summary>
    public class JxRotAngleSettings : JxContainer
    {
        public JxBool Enabled = new JxBool("Enabled", false, "啟用");
        public JxNumber Threshold = new JxNumber("Threshold", "外廓門限值", 0, new Range(0m, 300m, 10m, 0));
        public JxNumber Threshold2 = new JxNumber("Threshold2", "外廓門限值2", 300, new Range(1m, 1000m, 10m, 0));

        public JxRotAngleSettings() 
            : this(null, null)
        {
        }
        public JxRotAngleSettings(string name, string description)
        {
            Name = string.IsNullOrEmpty(name) ? "RotAngle" : name;
            Description = string.IsNullOrEmpty(description) ? "旋轉角度定位" : description;
        }
        public override void OnBindingSubItems()
        {
            //綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                Enabled,
                Threshold,
                Threshold2,
            });
            base.OnBindingSubItems();
        }
    }
}
