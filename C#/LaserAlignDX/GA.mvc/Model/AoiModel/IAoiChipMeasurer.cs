#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-08-10 把 Aoi Models 從 Recipe 分離 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using LaserAlignDX.BasicSpace;
using LaserAlignDX.OPSpace;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using VisionDesigner;

namespace LaserAlignDX.AoiModel
{
    public interface IAoiChipMeasurer : IDisposable
    {
        void SetCellGroups(GaCellsGroup[] cellGroups);

        void Run(Bitmap sceneBmp = null);

        /// <summary>
        /// 參數調試用
        /// </summary>
        void TryMeasureOneChip(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi);

        /// <summary>
        /// 參數調試用
        /// </summary>
        bool TryFindLineSegment(EdgeBorder eBorder, Bitmap bmpSrc, RectangleF roiRect, out CMvdLineSegmentF resultLine);

        ///// <summary>
        ///// 參數調試用
        ///// </summary>
        //void TryApplyFilters(Bitmap bmpSrc, out Bitmap bmpFeature, RectangleF? roiRect = null);

        /// <summary>
        /// 參數調試用 (保留)
        /// </summary>
        void AnalyzeGoldenData();
    }
}