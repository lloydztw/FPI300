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


namespace EzEmptyTrayInspector.Model
{
    /// <summary>
    /// To be continued
    /// </summary>
    public class MatchResult
    {
        public int ID
        {
            get; internal set;
        }
        public EzBlocsGrid Grid
        {
            get; internal set;
        }
        public IList<EzBloc> Blocs
        {
            get; internal set;
        }
        public double RotateAngle
        {
            // Degrees
            get; internal set;
        }
        public double TotalSeconds
        {
            get; internal set;
        }

        public MatchResult(int iD, EzBlocsGrid grid = null, IList<EzBloc> blocs = null, double totalSeconds = 0)
        {
            ID = iD;
            Grid = grid;
            Blocs = blocs;
            TotalSeconds = totalSeconds;
        }

        public static string FormatString(MatchResult e, bool isMultiLines)
        {
            var grid = e?.Grid;
            double ms = e != null ? e.TotalSeconds * 1000 : 0;
            double angle = e != null ? e.RotateAngle : 0;

            string msg;
            if (grid == null)
            {
                msg = "No result.";
            }
            else if (!isMultiLines)
            {
                grid.GetResultCounts(out int totalCount, out int majorCount, out int predCount);
                msg = $"格點={grid.Rows}x{grid.Cols}, A={angle:0.00}°, N={totalCount}, M={majorCount}, P={predCount}, Time={(int)ms}ms";
            }
            else
            {
                grid.GetResultCounts(out int totalCount, out int majorCount, out int predCount);
                msg = $"格點 = {grid.Rows} x {grid.Cols}";
                msg += $"\n\rTotal = {totalCount}";
                msg += $"\n\rMajor = {majorCount}";
                msg += $"\n\rPred = {predCount}";
                msg += $"\n\rAngle = {angle:0.00}°";
                msg += $"\n\rTime = {(int)ms} ms";
            }
            return msg;
        }
        public override string ToString()
        {
            return FormatString(this, false);
        }
    }
}
