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
using JetEazy.Match;
using OpenCvSharp;


namespace LaserAlignDX.AoiModel
{
    public interface ICalibAoiModel : IxEmptyTrayInspector
    {
        MatchResult FetchGridNodes(CarrierEnum carrierID, Mat fullfovImg, bool refine = true);
        bool AdjustBadNodes(CarrierEnum carrierID, EzBlocsGrid grid);
        
        ///// <summary>
        ///// 計算 空載台 格位 本身誤差
        ///// </summary>
        ///// <param name="carrierID">載台編號</param>
        ///// <param name="grid">像測格位點 (單位 pixels)</param>
        ///// <param name="dir">方向, 0: X, 1: Y, 2: XY, 其他: OFF</param>
        //void ScanSelfErrors(CarrierEnum carrierID, EzBlocsGrid grid, int dir, out double maxErr, out int maxErrRow, out int maxRowCol);
    }
}