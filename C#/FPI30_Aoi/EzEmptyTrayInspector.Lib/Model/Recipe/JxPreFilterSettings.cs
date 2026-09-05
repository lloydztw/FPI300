#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-09-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using LeTian.JxProps;


namespace EzAoiEmptyTrayInspector.Model
{
    /// <summary>
    /// 影像前處理
    /// </summary>
    public class JxPreFilterSettings : JxContainer
    {
        public JxInt Brightness = new JxInt("Brightness", "亮度", 0, new Range(-100, 100));
        public JxInt Contrast = new JxInt("Contrast", "對比", 0, new Range(-100, 100));

        public JxPreFilterSettings() : base("Pre-Filters", "影像前處理")
        {
        }
        public override void OnBindingSubItems()
        {
            //>>> 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                Brightness,
                Contrast,
            });
            base.OnBindingSubItems();
        }
    }
}
