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
    /// 所有參數設定
    /// </summary>
    public class JxAoiRecipe : JxContainer
    {
        const string _NAME = "EzEmptyTrayAoiRecipe";
        const string _DESC = "空盤檢測參數設定";

        public JxTrayVisionSettings VisionSettings = new JxTrayVisionSettings();
        public JxTrayMiscSettings TrayMiscSettings = new JxTrayMiscSettings();

        public JxAoiRecipe()
        {
            Name = _NAME;
            Description = _DESC;
        }
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                VisionSettings,
                TrayMiscSettings,
            });
            base.OnBindingSubItems();
        }

        #region HELPER_FUNCTIONS
        public JxTrayVisionSettings GetSiteSettings(int id = 0)
        {
            //if (id == 0) return Vision;
            //else if (id == 1) return SideB;
            //else return null;
            return VisionSettings;
        }
        public JxTempMatchSettings GetMatchSettings(int id = 0)
        {
            //if (id == 0) return Vision.Match;
            //else if (id == 1) return SideB.Match;
            //else return null;
            return VisionSettings.Match;
        }
        public bool IsDebugDumpEnabled()
        {
            return TrayMiscSettings.DebugDump;
        }
        #endregion
    }
}
