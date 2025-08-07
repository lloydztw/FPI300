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
    }
}
