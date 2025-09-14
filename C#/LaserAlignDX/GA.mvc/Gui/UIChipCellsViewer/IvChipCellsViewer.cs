#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-25 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Collections.Generic;
using System.Windows.Forms;

using CELL = LaserAlignDX.OPSpace.RegionCellX3Class;

namespace LaserAlignDX.UISpace.ChipCellsViewer
{
    public interface IvChipCellsViewer
    {
        Control Window { get; }
        
        bool IsActive { get; set; }

        bool HasImage();

        void Reset();

        /// <summary>
        /// Caller 必須負責 fullFovImage 生命週期
        /// </summary>
        void UpdateImageSrc(object fullfovImage, string srcName = null);
        
        void UpdateCells(IEnumerable<CELL> cells, int mode = 0);
    }
}
