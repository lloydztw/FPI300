#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-04 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Match;
using System.Collections.Generic;

namespace LaserAlignDX.AoiModel
{
    public static class Zigzag
    {
        public static IEnumerable<(int, int)> IterZigzag(int fullRows, int fullCols)
        {
            for (int row = 0; row < fullRows; row++)
            {
                if (row % 2 == 0)
                {
                    for (int col = 0; col < fullCols; col++)
                    {
                        yield return (row, col);
                    }
                }
                else
                {
                    for (int col = fullCols - 1; col > -1; col--)
                    {
                        yield return (row, col);
                    }
                }
            }
        }
        public static IEnumerable<(int, int, EzBloc)> IterZigzag(this EzBlocsGrid grid)
        {
            if (grid == null)
                yield break;

            int fullRows = grid.Rows;
            int fullCols = grid.Cols;
            foreach ((int r, int c) in IterZigzag(fullRows, fullCols))
            {
                yield return (r, c, grid.Get(r, c));
            }
        }
    }
}
