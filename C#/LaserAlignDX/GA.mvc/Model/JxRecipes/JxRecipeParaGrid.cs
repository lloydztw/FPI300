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

using LaserAlignDX.BasicSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.JxProps;
using LeTian.JxProps.PropertyMeta;
using Newtonsoft.Json;
using System.Drawing;

namespace LaserAlignDX.Mvc.Model.Recipe
{
    public abstract class JxGaBase : JxContainer
    {
        #region GAARA_RECIPES
        [JsonIgnore]
        public static RecipeFPIX3Class xRecipe => RecipeFPIX3Class.Instance;
        [JsonIgnore]
        public static RecipeParaGridClass xParaGrid => RecipeParaGridClass.Instance;
        #endregion

        public override void Load(string fileName)
        {
            //由上層 container 處理
            //Modified = false;
        }
        public override void Save(string fileName)
        {
            //由上層 container 處理
            //Modified = false;
        }
    }

    /// <summary>
    /// 對 Gaara RecipeParaGridClass 的包裝
    /// </summary>
    public class JxRecipeParaGrid : JxGaBase
    {
        public JxRecipeParaGrid_Cate1 Cate1 = new JxRecipeParaGrid_Cate1() { Name = "Cate1", Description = "01.基础设定" };
        public JxRecipeParaGrid_Cate2 Cate2 = new JxRecipeParaGrid_Cate2() { Name = "Cate2", Description = "02.其他设定" };
        public JxRecipeParaGrid_Cate3 Cate3 = new JxRecipeParaGrid_Cate3() { Name = "Cate3", Description = "03.位置矩阵设定" };

        //public JxBitmap ChipGoldenRegionBmp = new JxBitmap("ChipGoldenRegionBmp", "晶粒區域樣本(Region)");
        //public JxBase<RectangleF> ChipGoldenRegionRect = new JxBase<RectangleF>("ChipGoldenRegionRect", "晶粒區域(隱藏)");

        public JxRecipeParaGrid()
        {
            //ChipGoldenRegionBmp.OnModified += (s, e) =>
            //{
            //    var old = xRecipe.bmpprinttemplate;
            //    xRecipe.bmpprinttemplate = (Bitmap)ChipGoldenRegionBmp.Value?.Clone();
            //    old?.Dispose();
            //};
            //ChipGoldenRegionRect.OnModified += (s, e) =>
            //{
            //    xRecipe.xRectRegionPrint = ChipGoldenRegionRect.Value;
            //};
        }
        public override void OnBindingSubItems()
        {
            //// ChipGoldenRectionBmp 對應到 xRecipe.bmpprinttemplate
            //ChipGoldenRegionBmp.Value = (Bitmap)xRecipe?.bmpprinttemplate?.Clone();
            //// ChipGoldenRegionRect 對應到 xRecipe.xRectRegionPrint
            //ChipGoldenRegionRect.Value = xRecipe.xRectRegionPrint;

            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                //ChipGoldenRegionBmp,
                //ChipGoldenRegionRect,
                Cate1,
                Cate2,
                Cate3,
            });
            base.OnBindingSubItems();
        }
    }

    /// <summary>
    /// 對 Gaara RecipeParaGridClass (Cate1) 的包裝
    /// </summary>
    public class JxRecipeParaGrid_Cate1 : JxGaBase
    {
        JxMetaContainer _metaData = new JxMetaContainer(xParaGrid, "00.基础设定");
        public override void OnBindingSubItems()
        {
            BindItems(_metaData.GetBrowsableProps());
            base.OnBindingSubItems();
        }
    }

    /// <summary>
    /// 對 Gaara RecipeParaGridClass (Cate2) 的包裝
    /// </summary>
    public class JxRecipeParaGrid_Cate2 : JxGaBase
    {
        public JxEnum<LightChannelEnum> LightChannel = new JxEnum<LightChannelEnum>("LightChannel", "A01.灯光通道");
        public JxInt LightAmp = JxInt.C255("LightAmp", "A02.灯光值");
        public JxRecipeParaGrid_Cate2()
        {
            LightChannel.Value = xParaGrid.xChNum;
            LightAmp.Value = xParaGrid.xChValue;
            LightChannel.OnModified += (s, e) => xParaGrid.xChNum = LightChannel.Value;
            LightAmp.OnModified += (s, e) => xParaGrid.xChValue = LightAmp.Value;
        }
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                LightChannel,
                LightAmp,
            });
            base.OnBindingSubItems();
        }
    }

    /// <summary>
    /// 對 Gaara RecipeParaGridClass (Cate3) 的包裝
    /// </summary>
    public class JxRecipeParaGrid_Cate3 : JxGaBase
    {
        JxMetaContainer _metaData = new JxMetaContainer(xParaGrid, "03.位置矩阵设定");

        public override void OnBindingSubItems()
        {
            BindItems(_metaData.GetBrowsableProps());
            base.OnBindingSubItems();
        }
    }
}
