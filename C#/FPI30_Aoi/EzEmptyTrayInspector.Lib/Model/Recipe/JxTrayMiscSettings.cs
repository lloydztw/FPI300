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
    public class JxTrayMiscSettings : JxContainer
    {
        public JxInt FullRows = new JxInt("Full Rows", "滿盤 行數", 10, Range.C255);
        public JxInt FullCols = new JxInt("Full Cols", "滿盤 列數", 5, Range.C255);
        public JxBool DebugDump = new JxBool("Debug Dump", false, description: "輸出調適影像檔");

        public JxTrayMiscSettings() : base("Tray Settings", "空盤 全域設定")
        {
        }
        public override void OnBindingSubItems()
        {
            //綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                FullRows,
                FullCols,
                DebugDump,
            });
            base.OnBindingSubItems();
        }
    }
}
