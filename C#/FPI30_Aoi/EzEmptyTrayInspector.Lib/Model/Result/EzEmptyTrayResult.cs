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
using System.Collections.Generic;

namespace EzAoiEmptyTrayInspector.Model
{
    public class EzEmptyTrayResult
    {
        #region PRIVATE_DATA
        private MatchResult _matchResult;
        private EzBlocsGrid _grid => _matchResult?.Grid;
        private IList<EzBloc> _suckerBlocs => _matchResult?.Blocs;
        private IList<EzBloc> _outGridBlocs => _matchResult?.OutGridBlocs;
        #endregion

        public EzEmptyTrayResult(MatchResult matchResult, double totalSconds)
        {
            _matchResult = matchResult;
            TotalSeconds = totalSconds;
            NgCount = calc_ng_count();
        }

        #region INTERNAL_DATA
        internal EzBlocsGrid Grid
        {
            get { return _grid; }
        }
        internal MatchResult MatchResult
        {
            get { return _matchResult; }
        }
        #endregion

        /// <summary>
        /// 辨識耗時秒數
        /// </summary>
        public double TotalSeconds
        {
            get;
            private set;
        }

        ///// <summary>
        ///// 異常錯誤碼
        ///// </summary>
        //public ErrCodes Error
        //{
        //    get;
        //    internal set;
        //}

        /// <summary>
        /// 滿盤行數
        /// </summary>
        public int FullRows
        {
            get;
            internal set;
        }
        /// <summary>
        /// 滿盤列數
        /// </summary>
        public int FullCols
        {
            get;
            internal set;
        }
        /// <summary>
        /// 滿盤數量
        /// </summary>
        public int FullCount
        {
            get => FullRows * FullCols;
        }
        /// <summary>
        /// 異常數量
        /// </summary>
        public int NgCount
        {
            get;
            private set;
        }
        /// <summary>
        /// 實際吸嘴數
        /// </summary>
        public int ActualSuckersNumber
        {
            get => _suckerBlocs != null ? _suckerBlocs.Count : 0;
        }

        /// <summary>
        /// PASS or FAIL
        /// </summary>
        public bool IsPass()
        {
            return ActualSuckersNumber >= FullCount;
        }

        /// <summary>
        /// 枚舉 異常區塊 (Abnormal Blocs)
        /// </summary>
        public IEnumerable<EzBloc> IterAbnormalBlocs()
        {
            if (_grid != null)
            {
                foreach (var bloc in _grid.IterPredictedBlocs())
                {
                    if (bloc != null)
                        yield return bloc;
                }
            }
            if (_outGridBlocs != null)
            {
                foreach (var bloc in _outGridBlocs)
                {
                    if (bloc != null) 
                        yield return bloc;
                }
            }
        }
        /// <summary>
        /// 枚舉 吸嘴區塊 (Sucker Blocs)
        /// </summary>
        public IEnumerable<EzBloc> IterSuckerBlocs()
        {
            if (_suckerBlocs != null)
            {
                foreach (var bloc in _suckerBlocs)
                {
                    if (bloc != null)
                        yield return bloc;
                }
            }
        }
        /// <summary>
        /// 枚舉 格點 (Grid Blocs)
        /// </summary>
        public IEnumerable<EzBloc> IterGridBlocs()
        {
            if (_grid != null)
            {
                foreach (var bloc in _grid.IterBlocs())
                {
                    if (bloc != null)
                        yield return bloc;
                }
            }
        }
        /// <summary>
        /// 根據 [row, col] 取出定位區塊
        /// </summary>
        public void GetBlocByRowCol(int row, int col, out EzBloc bloc, out bool isSucker)
        {
            if (_grid != null)
            {
                bloc = _grid.Get(row, col);
                isSucker = EzBlocsGrid.IsSolidBloc(bloc);
            }
            else
            {
                bloc = null;
                isSucker = false;
            }
        }



        #region DISPLAY_STRING
        public static string FormatString(EzEmptyTrayResult e, bool usingMultiLines = false)
        {
            var grid = e?._grid;

            string msg;

            if (grid == null || e == null)
            {
                msg = "No result.";
                return msg;
            }

            double ms = e != null ? e.TotalSeconds * 1000 : 0;
            bool isPass = e.IsPass();
            msg = isPass ? "PASS" : "NG";
            msg = $"[AOI 空盤檢測 = {msg}], 異常數={e.NgCount}, 吸嘴數={e.ActualSuckersNumber}, 滿盤數={e.FullCount}, 耗時={(int)ms}ms";
            //>>> msg = $"{isPass?}格點={grid.Rows}x{grid.Cols}, 滿盤數={e.FullCount}, M={majorCount}, P={predCount}, Time={(int)ms}ms";

            if (usingMultiLines)
                msg = msg.Replace(", ", "\n\r");

            return msg;
        }
        public override string ToString()
        {
            return FormatString(this, false);
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        int calc_ng_count()
        {
            if (_grid != null)
            {
                _grid.RowMin = 0;
                _grid.ColMin = 0;
            }

            int ngCount = 0;
            foreach (var b in IterAbnormalBlocs())
            {
                if (b != null)
                {
                    b.Score = 0;
                    ngCount++;
                }
            }
            return ngCount;
        }
        #endregion
    }
}
