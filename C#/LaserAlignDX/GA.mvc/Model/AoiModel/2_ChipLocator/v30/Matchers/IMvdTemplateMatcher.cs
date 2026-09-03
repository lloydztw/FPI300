#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 開始重整優化 Gaara 原來的 MvdFindClass (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using JetEazy.Match;
using JetEazy.QvMath;
using System;
using System.Drawing;
using RecipeParams = LaserAlignDX.OPSpace.RecipeSpace.InspectX3ParaClass;


namespace LaserAlignDX.AoiModel
{
    public interface IMvdTemplateMatcher : IDisposable
    {
        /// <summary>
        /// 設定參數
        /// </summary>
        void SetRecipeParams(RecipeParams recipeParams);

        /// <summary>
        /// 樣板長寬大小 (必須於調用 Train 之後, 才有有效值!)
        /// </summary>
        Size TemplateSize { get; }

        /// <summary>
        /// 樣板特徵外廓 (必須於調用 Train 之後, 才有有效值!)
        /// </summary>
        QvQuad2D GoldenQuad2D { get; }

        /// <summary>
        /// 訓練
        /// </summary>
        bool Train(Bitmap bmpTemplate);

        /// <summary>
        /// 執行匹配 
        /// </summary>
        bool RunMatch(Bitmap bmpScene);

        /// <summary>
        /// 取得定位後 Chip 上面 PAD 的資訊
        /// </summary>
        EzBlocsGrid GetResultPadsGrid();

        /// <summary>
        /// 外廓結果
        /// </summary>
        QvQuad2D GetResultQuad2D();

        /// <summary>
        /// 調試用
        /// </summary>
        object GetResultDetails();

        ///// <summary>
        ///// 結果 (即將廢除)
        ///// </summary>
        //List<xFindResult> xResults { get; }
    }
}
