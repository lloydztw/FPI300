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

namespace EzAoiChipLocQC.Model
{
    /// <summary>
    /// 所有參數設定
    /// </summary>
    public class JxQcRecipe : JxContainer
    {
        const string _DESC = "晶粒偏位檢測參數設定";
        static int _debugCount = 0;

        public JxTrayDimSettings TrayDimSettings = new JxTrayDimSettings();
        public JxCamSettings CamSettings = new JxCamSettings();
        public JzLightSettings LightSettings = new JzLightSettings();
        public JxQcVisionSettings VisionSettings = new JxQcVisionSettings();
        public JxTraySegGrpSettings TraySegGrpSettings = new JxTraySegGrpSettings();

        public JxQcRecipe()
        {
            Description = _DESC;
            System.Diagnostics.Debug.WriteLine($"{GetType().Name} [{Name}] 建構 {++_debugCount}");
        }
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                CamSettings,
                LightSettings,
                VisionSettings,
                TrayDimSettings,
                //TraySegGrpSettings,
            });
            base.OnBindingSubItems();
        }
        protected override void OnDisposing()
        {
            base.OnDisposing();
            System.Diagnostics.Debug.WriteLine($"{GetType().Name} [{Name}] 卸載 {--_debugCount}");
        }

        #region HELPER_FUNCTIONS
        public JxQcVisionSettings GetSiteSettings(int id = 0)
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
            return TrayDimSettings.DebugDump;
        }
        #endregion

        public override void Load(string fileName)
        {
            base.Load(fileName);
        }
        public override void Save(string fileName)
        {
            base.Save(fileName);
        }

        #region PRIVATE_FUNCTIONS
        #endregion
    }
}
