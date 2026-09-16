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

using System;
using System.Collections.Generic;
using System.Drawing;

namespace LaserAlignDX.AoiModel
{
    public interface IAoiBadConnInspector : IDisposable
    {
        void SetCellGroups(GaCellsGroup[] cellGroups);

        void Run(Bitmap sceneBmp = null);

        /// <summary>
        /// 參數調試用
        /// </summary>
        bool TryApplyBadConnFilters(Bitmap bmpSrc, out Bitmap bmpResult, IEnumerable<RectangleF> rects);
    }
}