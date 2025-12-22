#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-07 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector.Model;
using JetEazy.EzImage;
using JetEazy.Match;
using JetEazy.QvMath;
using LaserAlignDX.AoiModel.Calib;
using OpenCvSharp;
using System;


namespace LaserAlignDX.AoiModel
{
    public interface ICalibAoiModel : IDisposable
    {
        /// <summary>
        /// 設定全域校正參數 (由 caller 維護 recipe 生命週期)
        /// </summary>
        void SetRecipe(JxCalibRecipe recipe);

        /// <summary>
        /// Caller 必須維護 fullfovImg 與 recipe 生命週期
        /// </summary>
        EzBlocsGrid FetchBoardGrid(CarrierEnum carrierID, Mat fullfovImg, JxCalibRecipe recipe);

        /// <summary>
        /// Caller 必須維護 fullfovImg 與 recipe 生命週期 
        /// </summary>
        QvQuad2D[] FetchInkMarks(CarrierEnum carrierID, SuckerRowEnum suckerID, Mat fullfovImg, JxCalibRecipe recipe);

        bool AdjustBadNodes(CarrierEnum carrierID, EzBlocsGrid grid);

#if (OPT_DEBUG)
        ///// <summary>
        ///// 計算 空載台 格位 本身誤差
        ///// </summary>
        ///// <param name="carrierID">載台編號</param>
        ///// <param name="grid">像測格位點 (單位 pixels)</param>
        ///// <param name="dir">方向, 0: X, 1: Y, 2: XY, 其他: OFF</param>
        void ScanSelfErrors(CarrierEnum carrierID, EzBlocsGrid grid, int dir, out double maxErr, out int maxErrRow, out int maxRowCol);
#endif

#if (OPT_DEPRECATED_AFTER_VER_3200)
        void ResetAndClear();
        ErrCodes BuildGoldenGridTemplate(object dummy, IEzImage ezImage);
        void SetRecipe(EzAoiEmptyTrayInspector.Model.JxAoiRecipe recipe);
        MatchResult FetchGridNodes(CarrierEnum carrierID, Mat fullfovImg, bool refine = true);
#endif
    }
}