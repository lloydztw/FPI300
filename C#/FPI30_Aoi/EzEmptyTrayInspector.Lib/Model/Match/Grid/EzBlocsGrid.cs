#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-02 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QMath;
using JetEazy.QxCollections;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;


namespace JetEazy.Match
{
    /// <summary>
    /// NOTE: 因為 EzBloc 不需要 Dispose, 所以 EzBlocsGrid 也不需要 Dispose
    /// </summary>
    public class EzBlocsGrid : QxGridMap<EzBloc>
    {
        #region PRIVATE_DATA
        Rectangle _boundary = new Rectangle(0, 0, 100, 100);
        QVector _pitch = new QVector(1.0, 1.0);
        #endregion

        internal EzBlocsGrid(Rectangle boundary, QVector pitch, int nRows, int nCols)
        {
            _boundary = boundary;
            _pitch = pitch;
            base.RowMin = 0;
            base.ColMin = 0;
            for (int r = 0; r < nRows; r++)
                for (int c = 0; c < nCols; c++)
                    this.Set(r, c, null);
        }
        internal EzBlocsGrid(Rectangle boundary, QVector pitch, IxGridMap<EzBloc> src) 
            : this(src)
        {
            _boundary = boundary;
            _pitch = pitch;
        }
        internal EzBlocsGrid(IxGridMap<EzBloc> src)
        {
            base.RowMin = 0;
            base.ColMin = 0;
            int nRows = src.Rows;
            int nCols = src.Cols;
            for (int r = 0; r < nRows; r++)
                for (int c = 0; c < nCols; c++)
                    base.Set(r, c, src.Get(r, c));
        }
        internal EzBlocsGrid()
        {
        }

        public void GetRowCol(int x, int y, out int iRow, out int iCol)
        {
            // 不太準 !!!
            x -= _boundary.X;
            y -= _boundary.Y;
            iCol = (int)Math.Truncate(x / _pitch.X);
            iRow = (int)Math.Truncate(y / _pitch.Y);
        }
        public void GetCellRect(int iRow, int iCol, ref Rectangle rc)
        {
            // 不太準 !!!
            var x = (int)Math.Round(iCol * _pitch.X) + _boundary.X;
            var y = (int)Math.Round(iRow * _pitch.Y) + _boundary.Y;
            rc.X = x;
            rc.Y = y;
            rc.Width = (int)_pitch.X;
            rc.Height = (int)_pitch.Y;
        }
        public void GetResultCounts(out int totalCount, out int majorCount, out int predCount)
        {
            totalCount = 0;
            majorCount = 0;
            predCount = 0;
            foreach (var bloc in this)
            {
                if (bloc == null) 
                    continue;
                totalCount++;
                if (IsSolidBloc(bloc))
                    majorCount++;
                else
                    predCount++;
            }
        }
        public void RescanCounts()
        {
            // redundant function
            Rescan(null);
        }
        public void RebuildRowColTags()
        {
            this.RowMin = 0;
            this.ColMin = 0;
            this.Rescan((r, c, b) =>
            {
                if (b != null && b is EzBloc bloc && bloc.Tag is QuadLinkNode node)
                    node.rowCol = new QxRowCol(r, c);
            });
        }
        public Rectangle GetBoundary()
        {
            return _boundary;
        }
        public QVector GetPitch()
        {
            return _pitch;
        }

        public static bool IsSolidBloc(EzBloc bloc)
        {
            return bloc?.Tag is QuadLinkNode;
        }
        public IEnumerable<EzBloc> IterBlocs()
        {
            foreach (var bloc in this)
            {
                if (bloc != null)
                    yield return bloc;
            }
        }
        public IEnumerable<EzBloc> IterMajorBlocs()
        {
            foreach (var bloc in this)
            {
                if (bloc != null && IsSolidBloc(bloc))
                    yield return bloc;
            }
        }
        public IEnumerable<EzBloc> IterPredictedBlocs()
        {
            foreach (var bloc in this)
            {
                if (bloc != null && !IsSolidBloc(bloc))
                    yield return bloc;
            }
        }
        public int GetMajorCount()
        {
            int count = 0; 
            foreach(var bloc in IterMajorBlocs())
                count++;
            return count;
        }

        public EzBlocsGrid Slice(int r0, int c0, int r1, int c1)
        {
            var rows = r1 - r0;
            var cols = c1 - c0;
            if (rows <= 0 || cols <= 0)
                return null;
            
            System.Diagnostics.Debug.Assert(r0 >= RowMin || c0 >= ColMin);
            System.Diagnostics.Debug.Assert(r1 <= RowMax || c1 >= ColMax);

            var subGrid = new EzBlocsGrid(_boundary, _pitch, rows, cols);
            for (int r = r0; r < r1; r++)
                for (int c = c0; c < c1; c++)
                    subGrid.Set(r - r0, c - c0, this.Get(r, c));

            var bound = EzBlocsGridBuilder.get_boundary(subGrid.IterBlocs());
            subGrid._boundary = bound;
            subGrid.RowMin = 0;
            subGrid.ColMin = 0;
            return subGrid;
        }

        /// <summary>
        /// 2025-10-06 新增
        /// </summary>
        public void Offset(float dx, float dy)
        {
            foreach(var bloc in IterBlocs())
            {
                bloc?.Offset(dx, dy);
            }
            _boundary.Offset((int)Math.Round(dx), (int)Math.Round(dy));
        }

        /// <summary>
        /// 順序: 左上, 右上, 右下, 左下
        /// (2025-10-15 新增)
        /// </summary>
        public EzBloc[] GetCornerBlocs()
        {
            int r = this.Rows - 1;
            int c = this.Cols - 1;
            var corners = new[]
            {
                this.Get(0,0),     //左上
                this.Get(0,c),     //右上
                this.Get(r,c),     //右下
                this.Get(r,0),     //左下
            };
            return corners;
        }
    }
}
