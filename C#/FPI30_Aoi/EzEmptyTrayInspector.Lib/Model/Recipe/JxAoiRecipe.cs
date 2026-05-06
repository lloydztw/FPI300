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
        const string _DESC = "空盤檢測參數設定";
        static int _debugCount = 0;

        public JxTrayMiscSettings TrayMiscSettings = new JxTrayMiscSettings();
        public JxTrayVisionSettings VisionSettings = new JxTrayVisionSettings();
        public JxTraySegGrpSettings TraySegGrpSettings = new JxTraySegGrpSettings();

        public JxAoiRecipe()
        {
            Description = _DESC;
            System.Diagnostics.Debug.WriteLine($"{GetType().Name} [{Name}] 建構 {++_debugCount}");
            if (TraySegGrpSettings == null)
                TraySegGrpSettings = new JxTraySegGrpSettings();
        }
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                TrayMiscSettings,
                VisionSettings,
                TraySegGrpSettings,
            });
            base.OnBindingSubItems();
        }
        protected override void OnDisposing()
        {
            base.OnDisposing();
            System.Diagnostics.Debug.WriteLine($"{GetType().Name} [{Name}] 卸載 {--_debugCount}");
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

        public override void Load(string fileName)
        {
            //>>> 嘗試載入舊格式
            migrateLoad(fileName);
            //>>> 參數版本升級, 進行必要的遷移處理.
            migrateBoundBox();
        }
        public override void Save(string fileName)
        {
            base.Save(fileName);
        }

        #region PRIVATE_FUNCTIONS
        void migrateLoad(string fileName)
        {
            if (!System.IO.File.Exists(fileName))
                return;

            try
            {
                // 如果是新版本
                bool isCurrentVersion = System.IO.File.ReadAllText(fileName).Contains(TraySegGrpSettings.GetType().Name);
                if (isCurrentVersion)
                {
                    base.Load(fileName);
                    return;
                }
            }
            catch
            {
            }

            try
            {
                using (var old = new V0.JxAoiRecipe())
                {
                    old.Load(fileName);
                    TrayMiscSettings.CopyFrom(old.TrayMiscSettings);
                    VisionSettings.CopyFrom(old.VisionSettings);
                    TrayMiscSettings.Modified = false;
                    VisionSettings.Modified = false;
                }
            }
            catch
            {
            }
        }
        void migrateBoundBox()
        {
            var defaultRect = VisionSettings.Match.BoundBox.Value;
            var segsList = TraySegGrpSettings.SegsList;
            if (segsList.Count == 0)
            {
                var item = new JxTraySegItem(0);
                item.BoundBox.Value = defaultRect;
                segsList.Add(item);
            }
            else
            {
                if (segsList.Count >= 1 && segsList[0].BoundBox.Value == System.Drawing.Rectangle.Empty)
                {
                    segsList[0].BoundBox.Value = defaultRect;
                }
            }
        }
        #endregion
    }
}
