#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Collections.Generic;
using CELL = LaserAlignDX.OPSpace.RegionCellX3Class;

namespace LaserAlignDX.Model
{
    public interface IxDispTextFormatter
    {
        string Format(CELL cell);
        string Format(IEnumerable<CELL> cells);
    }
}