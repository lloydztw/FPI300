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

using JetEazy.Match;
using JetEazy.QMath;
using System.Drawing;


namespace EzEmptyTrayInspector.Model
{
    /// <summary>
    /// 公式:  A + cell_offsets[r,c] + global_offset = B
    /// </summary>
    public class EzDualTransform
    {
        #region PRIVATE_MEMBERS
        QVector _global_offset = new QVector(0, 0);
        QVector[,] _cell_offsets = null;
        EzBlocsGrid _gridA;
        EzBlocsGrid _gridB;
        #endregion

        public int Rows
        {
            get => _gridA != null ? _gridA.Rows : 0;
        }
        public int Cols
        {
            get => _gridA != null ? _gridA.Cols : 0;
        }

        /// <summary>
        /// 公式:  A + cell_offsets[r,c] + global_offset = B
        /// 公式:  cell_offsets[r,c] =  B - A - global_offset
        /// </summary>
        public EzDualTransform(MatchResult[] matchResults, JxAoiRecipe recipe)
        {
            AveOffset = new QVector(0, 0);
            BuildCellOffsets(matchResults, recipe);
        }

        /// <summary>
        /// 公式:  A + cell_offsets[r,c] + global_offset = B
        /// 公式:  cell_offsets[r,c] =  B - A - global_offset
        /// </summary>
        public void BuildCellOffsets(MatchResult[] matchResults, JxAoiRecipe recipe)
        {
            int gx = recipe?.Misc.GlobalOffsetX ?? 0;
            int gy = recipe?.Misc.GlobalOffsetY ?? 0;
            var global_offset = new QVector(gx, gy);

            if (matchResults == null)
                throw new System.Exception("Error : 沒有 match 結果!");

            var gridA = matchResults[0]?.Grid;
            var gridB = matchResults[1]?.Grid;

            if (gridA == null)
                throw new System.Exception("Error : 網格 A 為 null !");
            if (gridB == null)
                throw new System.Exception("Error : 網格 B 為 null !");

            if (gridA.Rows != gridB.Rows || gridA.Cols != gridB.Cols)
                throw new System.Exception("Error : 網格 rows cols 數目不一致!");

            gridA.RebuildRowColTags();
            gridB.RebuildRowColTags();
            int rows = gridA.Rows;
            int cols = gridA.Cols;
            var cell_offsets = new QVector[rows, cols];

            int solidCount = 0;
            int emptyCount = 0;
            double dx_ave = 0;
            double dy_ave = 0;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var blocA = gridA.Get(r, c);
                    var blocB = gridB.Get(r, c);

                    if (blocA == null || blocB == null)
                    {
                        // 破洞 (事後填入平均值
                        cell_offsets[r, c] = null;
                        emptyCount++;
                    }
                    else
                    {
                        var dx = blocB.Rect.X - blocA.Rect.X;
                        var dy = blocB.Rect.Y - blocA.Rect.Y;
                        cell_offsets[r, c] = new QVector(dx, dy);
                        dx_ave += dx;
                        dy_ave += dy;
                        solidCount++;
                    }
                }
            }

            if (solidCount == 0)
                throw new System.Exception("Error : 網格 A B 沒有任何配對 !");

            dx_ave /= solidCount;
            dy_ave /= solidCount;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var offsetBA = cell_offsets[r, c];
                    if (offsetBA == null)
                        offsetBA = new QVector(dx_ave, dy_ave);
                    cell_offsets[r, c] = offsetBA - global_offset;
                }
            }

            // 存入 memebers
            _cell_offsets = cell_offsets;
            _global_offset = global_offset;
            _gridA = gridA;
            _gridB = gridB;
            AveOffset = new QVector(dx_ave, dy_ave);
        }

        public void GetCellOffset(int row, int col, out Point offset)
        {
            var offsetV = new QVector(_cell_offsets[row, col]);
            offset = new Point((int)offsetV.X, (int)offsetV.Y);
        }
        public void GetCellOffset(int row ,int col, out QVector offset)
        {
            offset = new QVector(_cell_offsets[row, col]);
        }
        public QVector AveOffset
        {
            get;
            private set;
        }
    }
}
