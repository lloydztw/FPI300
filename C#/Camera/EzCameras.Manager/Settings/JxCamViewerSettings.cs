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
    /// 相機即時影像視窗之參數設定
    /// </summary>
    public class JxCamViewerSettings : JxContainer
    {
        public JxBool ShowGridLines = new JxBool("GridLines", "顯示格線");
        public JxBool ShowCross = new JxBool("Cross", "顯示十字標");
        public JxBool ShowRuler = new JxBool("Ruler", "顯示尺度");
        public JxCamViewerSettings()
        {
            Name = "Camera Viewer";
            Description = "相機畫面設定";
        }
        public override void OnBindingSubItems()
        {
            //綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                ShowGridLines,
                ShowCross,
                ShowRuler,
            });
            base.OnBindingSubItems();
        }
    }
}
