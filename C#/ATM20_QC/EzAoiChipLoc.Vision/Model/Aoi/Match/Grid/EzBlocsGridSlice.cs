#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-09 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QxCollections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace JetEazy.Match
{
    /// <summary>
    /// 保留: 內部使用
    /// </summary>
    internal class SubGrid : IxGridMap<EzBloc>
    {
        #region PRIVATE_DATA
        EzBlocsGrid _owner;
        int _rowBegin;      // @ _owner 座標
        int _colBegin;      // @ _owner 座標
        int _rowEnd;        // @ _owner 座標
        int _colEnd;        // @ _owner 座標
        #endregion

        #region CACHE
        int _actualCount;
        #endregion

        public SubGrid(EzBlocsGrid owner, int rowBegin, int colBegin, int rowEnd, int colEnd)
        {
            _owner = owner;
            _rowBegin = rowBegin;
            _colBegin = colBegin;
            _rowEnd = rowEnd;
            _colEnd = colEnd;
            _actualCount = scan_actual_count();
        }

        #region IMPLEMENTATION_OF_IxGridMap

        public int RowMin
        {
            get => 0;   // readonly
            set {}
        }
        public int ColMin
        {
            get => 0;   // readonly
            set { }
        }
        public int RowMax => Rows;
        public int ColMax => Cols;
        public int Rows => _rowEnd - _rowBegin;
        public int Cols => _colEnd - _colBegin;
        public int ActualCount => _actualCount;

        public EzBloc this[int iRow, int iCol] 
        { 
            get => _owner[iRow + _rowBegin, iCol + _colBegin]; 
            set => _owner[iRow + _rowBegin, iCol + _colBegin] = value; 
        }
        public EzBloc Get(int iRow, int iCol)
        {
            return _owner?.Get(iRow + _rowBegin, iCol + _colBegin);
        }
        public EzBloc Set(int iRow, int iCol, EzBloc value)
        {
            return _owner?.Set(iRow + _rowBegin, iCol + _colBegin, value);
        }

        public IxSeq<EzBloc> this[int iRow]
        {
            get => _owner[iRow + _rowBegin];
            set => _owner[iRow + _rowBegin] = value;
        }
        public IxSeq<EzBloc> GetRow(int iRow)
        {
            return _owner?.GetRow(iRow + _rowBegin);
        }
        public IxSeq<EzBloc> SetRow(int iRow, IxSeq<EzBloc> rowNew)
        {
            return _owner?.SetRow(iRow + _rowBegin, rowNew);
        }
        public IxSeq<EzBloc> SeekToAvailableRow(ref int iRowID)
        {
            iRowID += _rowBegin;
            var ret = _owner?.SeekToAvailableRow(ref iRowID);
            iRowID -= _rowBegin;
            return ret;
        }

        void IxGridMap<EzBloc>.Rescan(FuncGridMapScanAction action)
        {
            _owner?.Rescan(action);
        }
        void IxGridMap<EzBloc>.Clear()
        {
            _owner?.Clear();
        }
        void IxGridMap<EzBloc>.Copy(IxGridMap<EzBloc> from)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<EzBloc> GetEnumerator()
        {
            return GetLinearEnumerator();
        }
        public IxGridMapEnumerator<EzBloc> GetLinearEnumerator()
        {
            //throw new NotImplementedException();
            return new QxGmLinearEnumerator<EzBloc>(this);
        }
        public IxGridMapEnumerator<EzBloc> GetZigzagEnumerator()
        {
            //throw new NotImplementedException();
            return new QxGmZigzagEnumerator<EzBloc>(this);
        }
        
        public EzBloc ReverseSeekToAvailableElement(int iRow, ref int iCol)
        {
            iRow += _rowBegin;
            iCol += _colBegin;
            var ret = _owner.ReverseSeekToAvailableElement(iRow, ref iCol);
            iRow -= _rowBegin;
            iCol -= _colBegin;
            return ret;
        }
        public EzBloc ReverseSeekToAvailableElement(ref int iRow, int iCol)
        {
            iRow += _rowBegin;
            iCol += _colBegin;
            var ret = _owner.ReverseSeekToAvailableElement(ref iRow, iCol);
            iRow -= _rowBegin;
            iCol -= _colBegin;
            return ret;
        }
        public EzBloc SeekToAvailableElement(int iRow, ref int iCol)
        {
            iRow += _rowBegin;
            iCol += _colBegin;
            var ret = _owner.SeekToAvailableElement(ref iRow, iCol);
            iRow -= _rowBegin;
            iCol -= _colBegin;
            return ret;
        }
        public EzBloc SeekToAvailableElement(ref int iRow, int iCol)
        {
            iRow += _rowBegin;
            iCol += _colBegin;
            var ret = _owner.SeekToAvailableElement(ref iRow, iCol);
            iRow -= _rowBegin;
            iCol -= _colBegin;
            return ret;
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        int scan_actual_count()
        {
            int count = 0;
            for (int r = _rowBegin; r < _rowEnd; r++)
                for (int c = _colBegin; c < _colEnd; c++)
                    if (_owner?.Get(r, c) != null)
                        count++;
            return count;
        }
        #endregion
    }


    #region GRID_ENUMERATORS

    class QxGmLinearEnumerator<T> : IxGridMapEnumerator<T>, IEnumerator<T>, IDisposable, IEnumerator
    {
        protected IxGridMap<T> m_owner;
        protected IxSeq<T> m_currentRow;
        protected T m_current;
        protected int m_iRow;
        protected int m_iCol;
        public IxGridMap<T> Owner => m_owner;
        public int Row => m_iRow;
        public int Col => m_iCol;
        object IEnumerator.Current => m_current;
        public T Current => m_current;
        public bool IsBegin
        {
            get
            {
                if (m_owner == null)
                {
                    return true;
                }

                if (m_iRow <= m_owner.RowMin && m_current == null)
                {
                    return true;
                }

                return false;
            }
        }
        public bool IsEnd
        {
            get
            {
                if (m_owner == null)
                {
                    return true;
                }

                if (m_iRow >= m_owner.RowMax && m_current == null)
                {
                    return true;
                }

                return false;
            }
        }
        public QxGmLinearEnumerator(IxGridMap<T> owner)
        {
            m_owner = owner;
            Reset();
        }
        public void Dispose()
        {
        }
        public void Reset()
        {
            m_currentRow = null;
            m_current = default(T);
            m_iRow = m_owner.RowMin - 1;
            m_iCol = m_owner.ColMin - 1;
        }
        public virtual bool MoveNext()
        {
            while (m_iRow < m_owner.RowMax)
            {
                if (m_currentRow == null)
                {
                    m_currentRow = m_owner.SeekToAvailableRow(ref m_iRow);
                    if (m_currentRow == null)
                    {
                        m_iRow++;
                        continue;
                    }

                    m_iCol = m_owner.ColMin - 1;
                }

                if (m_currentRow != null)
                {
                    m_iCol++;
                    m_current = m_currentRow.SeekToAvailableElement(ref m_iCol);
                    if (m_current != null)
                    {
                        return true;
                    }

                    m_currentRow = null;
                    m_iRow++;
                }
            }

            return false;
        }
        public virtual bool MovePrev()
        {
            while (m_iRow >= m_owner.RowMin)
            {
                if (m_currentRow == null)
                {
                    m_currentRow = _ReverseSeekToAvailableRow(ref m_iRow);
                    if (m_currentRow == null)
                    {
                        m_iRow--;
                        continue;
                    }

                    m_iCol = m_owner.ColMax;
                }

                if (m_currentRow != null)
                {
                    m_iCol--;
                    m_current = m_currentRow.ReverseSeekToAvailableElement(ref m_iCol);
                    if (m_current != null)
                    {
                        return true;
                    }

                    m_currentRow = null;
                    m_iRow--;
                }
            }

            return false;
        }
        public virtual bool MoveTo(int iRow, int iCol)
        {
            m_iRow = iRow;
            m_iCol = iCol;
            m_currentRow = m_owner.GetRow(m_iRow);
            m_current = m_owner.Get(iRow, iCol);
            return m_current != null;
        }
        protected IxSeq<T> _ReverseSeekToAvailableRow(ref int iRowID)
        {
            while (iRowID >= m_owner.RowMin)
            {
                IxSeq<T> row = m_owner.GetRow(iRowID);
                if (row != null && row.ElementsCount > 0)
                {
                    return row;
                }

                iRowID--;
            }

            return null;
        }
    }

    class QxGmZigzagEnumerator<T> : QxGmLinearEnumerator<T>
    {
        public QxGmZigzagEnumerator(IxGridMap<T> owner)
            : base((IxGridMap<T>)owner)
        {
        }
        public override bool MoveNext()
        {
            while (m_iRow < m_owner.RowMax)
            {
                bool flag = _isReversedRow(m_iRow);
                if (m_currentRow == null)
                {
                    m_currentRow = m_owner.SeekToAvailableRow(ref m_iRow);
                    if (m_currentRow == null)
                    {
                        m_iRow++;
                        continue;
                    }

                    flag = _isReversedRow(m_iRow);
                    m_iCol = (flag ? m_owner.ColMax : (m_owner.ColMin - 1));
                }

                if (m_currentRow != null)
                {
                    if (flag)
                    {
                        m_iCol--;
                        m_current = m_currentRow.ReverseSeekToAvailableElement(ref m_iCol);
                    }
                    else
                    {
                        m_iCol++;
                        m_current = m_currentRow.SeekToAvailableElement(ref m_iCol);
                    }

                    if (m_current != null)
                    {
                        return true;
                    }

                    m_currentRow = null;
                    m_iRow++;
                }
            }

            return false;
        }
        public override bool MovePrev()
        {
            while (m_iRow >= m_owner.RowMin)
            {
                bool flag = _isReversedRow(m_iRow);
                if (m_currentRow == null)
                {
                    m_currentRow = _ReverseSeekToAvailableRow(ref m_iRow);
                    if (m_currentRow == null)
                    {
                        m_iRow--;
                        continue;
                    }

                    flag = _isReversedRow(m_iRow);
                    m_iCol = (flag ? m_owner.ColMax : (m_owner.ColMin - 1));
                }

                if (m_currentRow != null)
                {
                    if (flag)
                    {
                        m_iCol--;
                        m_current = m_currentRow.ReverseSeekToAvailableElement(ref m_iCol);
                    }
                    else
                    {
                        m_iCol++;
                        m_current = m_currentRow.SeekToAvailableElement(ref m_iCol);
                    }

                    if (m_current != null)
                    {
                        return true;
                    }

                    m_currentRow = null;
                    m_iRow--;
                }
            }

            return false;
        }
        private bool _isReversedRow(int iRowID)
        {
            return ((iRowID - m_owner.RowMin) & 1) != 0;
        }
    }

    #endregion
}
