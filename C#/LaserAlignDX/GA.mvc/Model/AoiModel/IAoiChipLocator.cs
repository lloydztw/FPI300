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

using LaserAlignDX.Model;
using LaserAlignDX.OPSpace;
using System;
using System.Drawing;

namespace LaserAlignDX.AoiModel
{
    public interface IAoiChipLocator : IDisposable
    {
        GaCellsGroup[] CellGroups { get; }

        bool Train(Bitmap goldenImage, params object[] args);

        void Run(Bitmap sceneBmp = null);

        /// <summary>
        /// 提供 給 參數編輯 使用
        /// </summary>
        bool LocateOneChip(Bitmap cellBmp, ref RectangleF cellRoi, out GaChipData chipData);

        /// <summary>
        /// 調試用
        /// </summary>
        bool TryLocateOneChip(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi);

        /// <summary>
        /// 釋放 CellGroups
        /// </summary>
        void DisposeCellGroups();

        MvdCompositeChipMatcher GetTemplateMatcher();
    }
}