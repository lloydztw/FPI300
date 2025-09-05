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
using System.Drawing;


namespace EzAoiEmptyTrayInspector.Model
{
    using JxRect = JxBase<Rectangle>;

    /// <summary>
    /// 模板影像比對設定
    /// </summary>
    public class JxTempMatchSettings : JxContainer
    {
        public JxRect GoldenBox = new JxRect("GoldenBox", "參考框(唯讀)(隱藏)");
        public JxBitmap GoldenBmp = new JxBitmap("GoldenBmp", "影像比對樣本");
        public JxNumber GoldenRefAngle = new JxNumber("GoldenRefAngle", "參考角度(唯讀)(隱藏)");
        public JxNumber ScoreThres = new JxNumber("ScoreThres", "比對閥值", 0.8m, new Range(0m, 1m, 0.01m, 2));
        public JxNumber ScoreThresLow = new JxNumber("ScoreThresLow", "比對閥值(下限)", 0.5m, new Range(0m, 1m, 0.01m, 2));
        public JxInt Iterations = new JxInt("Iterations", "比對疊代次數", 1, new Range(1, 100));
        public JxBool RemoveOverlap = new JxBool("RemoveOverlap", "消除重疊(隱藏)", true);
        
        public JxBool UseGrid = new JxBool("UseGrid", "建立網格", true);

        public JxTempMatchSettings() 
            : this(null, null)
        {
        }
        public JxTempMatchSettings(string name, string description = null)
        {
            Name = string.IsNullOrEmpty(name) ? "Match" : name;
            Description = string.IsNullOrEmpty(description) ? "影像比對設定" : description;
        }
        public override void OnBindingSubItems()
        {
            //綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                GoldenBox,
                GoldenBmp,
                GoldenRefAngle,

                ScoreThres,
                ScoreThresLow,
                Iterations,

                RemoveOverlap,
                UseGrid,
            });
            base.OnBindingSubItems();
        }
    }
}
