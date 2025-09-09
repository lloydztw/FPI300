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
using System.Drawing;

namespace LaserAlignDX.Mvc.Model.Recipe
{
    public abstract class JxGaBase : JxContainer
    {
        protected RecipeFPIX3Class xRecipe => RecipeFPIX3Class.Instance;
        protected RecipeParaGridClass xParaGrid => RecipeParaGridClass.Instance;
    }

    /// <summary>
    /// 對 Gaara RecipeParaGridClass 的包裝
    /// </summary>
    public class JxRecipeParaGrid : JxGaBase
    {
        public JxRecipeParaGrid_Cate1 Cate1 = new JxRecipeParaGrid_Cate1() { Name = "Cate1", Description = "01.基础设定" };
        public JxRecipeParaGrid_Cate2 Cate2 = new JxRecipeParaGrid_Cate2() { Name = "Cate2", Description = "02.其他设定" };
        public JxRecipeParaGrid_Cate3 Cate3 = new JxRecipeParaGrid_Cate3() { Name = "Cate3", Description = "03.位置矩阵设定" };
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                Cate1,
                Cate2,
                //Cate3,
            });
            base.OnBindingSubItems();
        }

        public override void Load(string fileName)
        {
            //xRecipe.Load(); //<<< 由上層調用
            Cate1.Load(fileName);
            Cate2.Load(fileName);
            Cate3.Load(fileName);
            Modified = false;
        }
        public override void Save(string fileName)
        {
            Cate1.Save(fileName);
            Cate2.Save(fileName);
            //Cate3.Save(fileName);
            //xRecipe.Save(); //<<< 由上層調用
            Modified = false;
        }
    }

    /// <summary>
    /// 對 Gaara RecipeParaGridClass (Cate1) 的包裝
    /// </summary>
    public class JxRecipeParaGrid_Cate1 : JxGaBase
    {
        public JxBitmap ChipGoldenBmp = new JxBitmap("ChipGoldenBmp", "A00.晶粒模板樣本");
        public JxNumber Angle = new JxNumber("Angle", "A00.阵列角度", range: new Range(-180m, 180m, 1, 2));
        public JxInt LeftTopX = new JxInt("LeftTopX", "A04.左上角X (pix)", range: new Range(0, 50000));
        public JxInt LeftTopY = new JxInt("LeftTopY", "A04.左上角Y (pix)", range: new Range(0, 50000));
        public JxNumber ChipWidth = new JxNumber("ChipWidth", "A07.Chip寬度 (mm)", 10m, range: new Range(1m, 1000m, 0.1m, 3));
        public JxNumber ChipHeight = new JxNumber("ChipHeight", "A08.Chip高度 (mm)", 10m, range: new Range(1m, 1000m, 0.1m, 3));
        public JxInt ExtendX = new JxInt("ExtendX", "A09.外擴X (pix)", 500, range: new Range(1, 10000));
        public JxInt ExtendY = new JxInt("ExtendY", "A10.外擴Y (pix)", 500, range: new Range(1, 10000));
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                ChipGoldenBmp,
                //Angle,
                //LeftTopX,
                //LeftTopY,
                ChipWidth,
                ChipHeight,
                ExtendX,
                ExtendY
            });
            base.OnBindingSubItems();
        }

        public override void Load(string fileName)
        {
            ChipGoldenBmp.Value = (Bitmap)xRecipe.bmpprinttemplate.Clone();

            Angle.Value = (decimal)xRecipe.xAngle;
            LeftTopX.Value = xParaGrid.xLeftTopX;
            LeftTopY.Value = xParaGrid.xLeftTopY;
            ChipWidth.Value = (decimal)xRecipe.xChipWidth;
            ChipHeight.Value = (decimal)xRecipe.xChipHeight;
            ExtendX.Value = xRecipe.xExtendx;
            ExtendY.Value = xRecipe.xExtendy;
            Modified = false;
        }
        public override void Save(string fileName)
        {
            xRecipe.xAngle = (float)Angle.Value;
            xParaGrid.xLeftTopX = LeftTopX.Value;
            xParaGrid.xLeftTopY = LeftTopY.Value;
            xRecipe.xChipWidth = (float)ChipWidth.Value;
            xRecipe.xChipHeight = (float)ChipHeight.Value;
            xRecipe.xExtendx = ExtendX.Value;
            xRecipe.xExtendy = ExtendY.Value;
            Modified = false;
        }
    }

    /// <summary>
    /// 對 Gaara RecipeParaGridClass (Cate2) 的包裝
    /// </summary>
    public class JxRecipeParaGrid_Cate2 : JxGaBase
    {
        public JxEnum<LightChannelEnum> LightChannel = new JxEnum<LightChannelEnum>("LightChannel", "A01.灯光通道");
        public JxInt LightAmp = JxInt.C255("LightAmp", "A02.灯光值");
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                LightChannel,
                LightAmp,
            });
            base.OnBindingSubItems();
        }

        public override void Load(string fileName)
        {
            LightChannel.Value = (LightChannelEnum)xParaGrid.xChNum;
            LightAmp.Value = xParaGrid.xChValue;
            Modified = false;
        }
        public override void Save(string fileName)
        {
            xParaGrid.xChNum = (int)LightChannel.Value;
            xParaGrid.xChValue = LightAmp.Value;
            Modified = false;
        }
    }

    /// <summary>
    /// 對 Gaara RecipeParaGridClass (Cate3) 的包裝
    /// </summary>
    public class JxRecipeParaGrid_Cate3 : JxGaBase
    {
        public JxNumber RealLeftX = new JxNumber("ReadLeftX", "A01.起点X (mm) (唯讀)", range: new Range(-3000, 3000, 1, 3));
        public JxNumber RealLeftY = new JxNumber("ReadLeftY", "A02.起点Y (mm) (唯讀)", range: new Range(-3000, 3000, 1, 3));
        public JxNumber RealOffsetX = new JxNumber("RealOffsetX", "A03.横向间距 (mm) (唯讀)", range: new Range(-3000, 3000, 1, 3));
        public JxNumber RealOffsetY = new JxNumber("RealOffsetY", "A04.纵向间距 (mm) (唯讀)", range: new Range(-3000, 3000, 1, 3));
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                RealLeftX,
                RealLeftY,
                RealOffsetX,
                RealOffsetY,
            });
            base.OnBindingSubItems();
        }

        public override void Load(string fileName)
        {
            RealLeftX.Value = (decimal)xRecipe.xRealLeftX;
            RealLeftY.Value = (decimal)xRecipe.xRealLeftY;
            RealOffsetX.Value = (decimal)xRecipe.xRealOffsetX;
            RealOffsetY.Value = (decimal)xRecipe.xRealOffsetY;
            Modified = false;
        }
        public override void Save(string fileName)
        {
            //xRecipe.xRealLeftX = (float)xRecipe.xRealLeftX;
            //xRecipe.xRealLeftY = (float)xRecipe.xRealLeftY;
            //xRecipe.xRealOffsetX = (float)xRecipe.xRealOffsetX;
            //xRecipe.xRealOffsetY = (float)xRecipe.xRealOffsetY;
            Modified = false;
        }
    }
}
