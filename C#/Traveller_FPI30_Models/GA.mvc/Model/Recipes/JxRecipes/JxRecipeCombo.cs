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
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using LeTian.JxProps;
using Newtonsoft.Json;
using Traveller106;

namespace LaserAlignDX.Mvc.Model.Recipe
{
    /// <summary>
    /// 統合 
    ///     RecipeFPIX3Class 
    ///     RecipeParaGridClass
    ///     JxAoiRecipe (空盤檢測參數)
    /// </summary>
    public class JxRecipeCombo: JxContainer
    {
        #region GAARA_RECIPE
        [JsonIgnore]
        public RecipeFPIX3Class xRecipe => RecipeFPIX3Class.Instance;
        [JsonIgnore]
        public RecipeParaGridClass xParamGrid => RecipeParaGridClass.Instance;
        #endregion

        public JxAoiRecipe EmptyTrayParams = new JxAoiRecipe() { Name = "EmptyTray.Vision", Description = "(1) 空盤檢測設定" };
        public JxRecipeParaGrid GaGridParams = new JxRecipeParaGrid() { Name = "GaGrid.Vision", Description = "(2) 晶粒陣列設定" };
        
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                EmptyTrayParams,
                GaGridParams,
            });
            base.OnBindingSubItems();
        }

        public override void Load(string dummyFileName)
        {
            CarrierEnum C = dummyFileName == "C2" ? CarrierEnum.C2 : CarrierEnum.C1;
            string emptyTrayRecipeFile = LtAoiFactory.RcpGetRecipeFileName(null, C);
            LtDebug.LOG.Debug($"載入 [空盤參數] {emptyTrayRecipeFile}");
            EmptyTrayParams.Load(emptyTrayRecipeFile);
            GaGridParams.Load(null);
            Modified = false;
        }
        public override void Save(string dummyFileName)
        {
            CarrierEnum C = dummyFileName == "C2" ? CarrierEnum.C2 : CarrierEnum.C1;
            string emptyTrayRecipeFile = LtAoiFactory.RcpGetRecipeFileName(null, C);
            LtDebug.LOG.Debug($"寫入 [空盤參數] {emptyTrayRecipeFile}");
            EmptyTrayParams.Save(emptyTrayRecipeFile);
            GaGridParams.Save(null);
            Modified = false;
        }
    }
}
