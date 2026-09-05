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
using OpenCvSharp;
using System;
using System.Drawing;

using AOI_RECIPE = EzAoiEmptyTrayInspector.Model.JxAoiRecipe;
using AOI_RESULT = EzAoiEmptyTrayInspector.Model.EzEmptyTrayResult;


namespace EzAoiEmptyTrayInspector.Model
{
    public interface IxEmptyTrayInspector : IxCommonModel
    {
        event EventHandler<MatchResultEventArgs> OnMatched;
        event EventHandler<AoiResultEventArgs> OnFinalResulted;

        int AddRef();

        /// <summary>
        /// 清除上一次結果
        /// </summary>
        void ResetAndClear(SideID sideId = SideID.All);

        /// <summary>
        /// 設定參數
        /// </summary>
        void SetRecipe(AOI_RECIPE recipe);
        
        /// <summary>
        /// 目前參數
        /// </summary>
        AOI_RECIPE GetRecipe();

        /// <summary>
        /// 讓 Recipe Editor 調用
        /// </summary>
        /// <remarks>
        /// 如果都沒有 pre filters, 則輸出原本的 src image
        /// </remarks>
        Mat TryApplyPreFilters(Mat src);

        /// <summary>
        /// 讓 Recipe Editor 調用
        /// </summary>
        void TryApplyRotationFilters(SideID sideId, IEzImage img, JxRotAngleSettings settings, out object result);

        /// <summary>
        /// 從 巨圖 擷取 吸嘴圖形 當 Golden Template 
        /// <br/> 於 Recipe Editor 中使用
        /// </summary>
        bool CropGoldenTemplate(SideID sideId, IEzImage largeImg, Rectangle goldenRect);

        /// <summary>
        /// 根据 吸嘴 golden image 建立 整盤 格線定位點 
        /// <br/> 於 Recipe Editor 中使用
        /// <br/> 返回值 Err, suggestRows, suggestCols
        /// </summary>
        (ErrCodes, int, int) BuildGoldenGridTemplate(SideID sideId, IEzImage largeIm, int targetRows, int targetCols);

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

        /// <summary>
        /// 執行 所有 AOI 運算
        /// </summary>
        void RunAll(Bitmap bmp, bool wait = true, string dumpPath = null);

        ///// <summary>
        ///// 檢測結果
        ///// </summary>
        AOI_RESULT GetResult();
    }
}
