#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-07-04 改版 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Windows.Forms;

namespace JetEazy.Lang
{
    public static class EzContextMenuTranslator
    {
        #region LANGUAGE
        private static QxLang _lang => QxLang.Instance("gui");
        private static string _T(string text)
        {
            var t = _lang?.Translate(text);
            if (!string.IsNullOrEmpty(t))
                return t;
            return text;
        }
        #endregion

        /// <summary>
        /// 翻譯整個 ContextMenuStrip（包含所有子選單項目）
        /// </summary>
        public static void Translate(this ContextMenuStrip menu)
        {
            if (menu == null) return;
            
            //if (_lang == null || _lang.LanguageID == 1)
            //    return;

            foreach (ToolStripItem item in menu.Items)
            {
                TranslateItem(item);
            }
        }

        /// <summary>
        /// 遞迴翻譯選單項目
        /// </summary>
        private static void TranslateItem(ToolStripItem item)
        {
            if (item == null) return;

            // 1. 翻譯當前項目的文字 (排除分隔線 StripSeparator)
            if (!(item is ToolStripSeparator) && !string.IsNullOrEmpty(item.Name))
            {
                // 💡 關鍵修正：直接拿控制項的 Name (永遠不會變的唯一 ID) 去反查字典
                item.Text = _T(item.Name);
            }

            // 2. 如果這是一個有子選單的項目 (ToolStripMenuItem)，遞迴進去翻譯子項目
            if (item is ToolStripMenuItem menuItem && menuItem.HasDropDownItems)
            {
                foreach (ToolStripItem subItem in menuItem.DropDownItems)
                {
                    TranslateItem(subItem); // 遞迴呼叫
                }
            }
        }
    }
}