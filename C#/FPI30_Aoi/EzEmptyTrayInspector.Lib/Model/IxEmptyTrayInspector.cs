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


using JetEazy.EzImage;
using System;
using System.Drawing;

using AOI_RECIPE = EzEmptyTrayInspector.Model.JxAoiRecipe;
using AOI_RESULT = EzEmptyTrayInspector.Model.EzEmptyTrayResult;


namespace EzEmptyTrayInspector.Model
{
    public interface IxEmptyTrayInspector : IxCommonModel
    {
        event EventHandler<MatchResultEventArgs> OnMatched;
        event EventHandler<AoiResultEventArgs> OnFinalResulted;

        /// <summary>
        /// 清除上一次結果
        /// </summary>
        void ResetAndClear(SideID sideId = SideID.All);

        /// <summary>
        /// 設定參數
        /// </summary>
        void SetRecipe(AOI_RECIPE recipe);

        /// <summary>
        /// 讓 Recipe Editor 調適試跑使用
        /// </summary>
        void TryApplyFilters(SideID sideId, IEzImage img, JxRotAngleSettings settings, out object result);

        /// <summary>
        /// 從 巨圖 (IEzImage) 擷取 吸嘴圖形 當 Golden Template 
        /// </summary>
        bool CropGoldenTemplate(SideID sideId, IEzImage largeImg, Rectangle rect);

        ErrCodes CanMatch(SideID sideId, IEzImage img);
        void RunMatch(SideID sideId, IEzImage img, string dumpPath = null);
        MatchResult GetMatchResult(SideID sideId);

        /// <summary>
        /// 是否可以 執行 所有 AOI 運算
        /// </summary>
        ErrCodes CanRunAll(IEzImage imgA, IEzImage imgB = null);

        /// <summary>
        /// 執行 所有 AOI 運算
        /// </summary>
        void RunAll(IEzImage imgA, IEzImage imgB = null, string outputFile = null, string dumpPath = null, bool wait = false);

        ///// <summary>
        ///// 檢測結果
        ///// </summary>
        AOI_RESULT GetResult();
    }
}
