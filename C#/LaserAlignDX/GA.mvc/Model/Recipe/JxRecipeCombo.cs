#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-09 重新設計校正架構 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector.Model;
using LeTian.JxProps;
using Traveller106;

namespace LaserAlignDX.Mvc.Model.Recipe
{
    /// <summary>
    /// 對 Gaara xRecipe 的包裝
    /// </summary>
    public class JxRecipeCombo: JxContainer
    {
        public JxAoiRecipe EmptyTrayParams = new JxAoiRecipe() { Name = "EmptyTray.Vision", Description = "(1) 空盤檢測設定" };
        public JxRecipeParaGrid GaGridParams = new JxRecipeParaGrid() { Name = "GaChip.Vision", Description = "(2) 晶粒檢測設定" };
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                EmptyTrayParams,
                GaGridParams,
            });
            base.OnBindingSubItems();
        }

        public override void Load(string fileName)
        {
            string emptyTrayRecipeFile = LtAoiFactory.RcpGetRecipeFileName(null);
            EmptyTrayParams.Load(emptyTrayRecipeFile);
            GaGridParams.Load(fileName);
            Modified = false;
        }
        public override void Save(string fileName)
        {
            string emptyTrayRecipeFile = LtAoiFactory.RcpGetRecipeFileName(null);
            EmptyTrayParams.Save(emptyTrayRecipeFile);
            GaGridParams.Save(fileName);
            Modified = false;
        }
    }
}
