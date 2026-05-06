#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-09-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Match;
using System;
using System.Collections.Generic;
using System.Text;

namespace EzAoiEmptyTrayInspector.Model
{
    public class EzBlocsGridSerializer
    {
        const string VERSION = "V3";
        const char SEP_HEADER = '#';
        const char SEP_BLOCS = ':';

        public string Serialize(EzBlocsGrid grid)
        {
            if (grid == null)
                return "";

            StringBuilder sb = new StringBuilder();

            int rows = grid.Rows;
            int cols = grid.Cols;
            var pitch = grid.GetPitch();

            // HEADER
            sb.Append($"{rows},{cols},{pitch.X:0.000},{pitch.Y:0.000},{VERSION}").Append(SEP_HEADER);

            // BLOCS
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var bloc = grid.Get(r, c);
                    if (bloc == null) continue;
                    sb.Append(Serialize(r, c, bloc)).Append(SEP_BLOCS);
                }
            }

            return sb.ToString().TrimEnd(SEP_BLOCS);
        }
        public bool Deserialize(string str, out EzBlocsGrid grid)
        {
            grid = null;

            try
            {
                if (string.IsNullOrEmpty(str))
                    return false;

                while (true)
                {
                    //(1) HEADER 嘗試取得 rows, cols, pitchX, pitchY, version
                    var strs = str.Split(SEP_HEADER);
                    if (strs.Length < 2)
                        break;

                    var header = strs[0];
                    var headStrs = header.Split(',');
                    if (headStrs.Length < 5)
                        break;

                    var ver = headStrs[4].Trim();

                    if (!int.TryParse(headStrs[0], out int rows) ||
                        !int.TryParse(headStrs[1], out int cols) ||
                        !double.TryParse(headStrs[2], out double pitchX) ||
                        !double.TryParse(headStrs[3], out double pitchY))
                        break;

                    //if (string.Compare(ver, VERSION) < 0)
                    //    break;

                    if (rows <= 0 || cols <= 0)
                        break;

                    //(2) place holder
                    var blocsHolder = new EzBloc[rows, cols];
                    EzBloc bloc1 = null;

                    //(3) Deserialize Blocs
                    var lines = strs[1].Split(SEP_BLOCS);
                    foreach (var line in lines)
                    {
                        if (Deserialize(line, out int row, out int col, out var bloc) && bloc != null)
                            blocsHolder[row, col] = bloc1 = bloc;
                    }

                    //(4) Grid Builder
                    var builder = new EzBlocsGridBuilder();
                    grid = builder.BuildEmptyGrid(new List<EzBloc>() { bloc1 }, 1, 1, new JetEazy.QMath.QVector2(pitchX, pitchY));
                    for (int r = 0; r < rows; r++)
                        for (int c = 0; c < cols; c++)
                            grid.Set(r, c, blocsHolder[r, c]);

                    grid.ColMin = 0;
                    grid.RowMin = 0;
                    grid.RebuildRowColTags();
                    return true;
                }


                //(5) 使用舊的 Deserializer
                var ssOld = new V0.EzBlocsGridSerializer();
                return ssOld.Deserialize(str, out grid);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return false;
        }

        #region PRIVATE_FUNCTIONS
        string Serialize(int row, int col, EzBloc bloc)
        {
            if (bloc != null)
                return $"{row},{col},{bloc.Rect.X},{bloc.Rect.Y},{bloc.Rect.Width},{bloc.Rect.Height},{bloc.Score:0.000}";
            return "";
        }
        bool Deserialize(string str, out int row, out int col, out EzBloc bloc)
        {
            bloc = null;
            row = -1;
            col = -1;

            if (string.IsNullOrEmpty(str))
                return false;

            var strs = str.Split(',');
            if (strs.Length < 7)
                return false;

            bool ok = true;
            int i = 0;
            ok &= int.TryParse(strs[i++], out row);
            ok &= int.TryParse(strs[i++], out col);
            ok &= int.TryParse(strs[i++], out int x);
            ok &= int.TryParse(strs[i++], out int y);
            ok &= int.TryParse(strs[i++], out int w);
            ok &= int.TryParse(strs[i++], out int h);
            ok &= double.TryParse(strs[i++], out double score);
            if (ok)
                bloc = new EzBloc(new System.Drawing.Rectangle(x, y, w, h), score);
            ok &= (row >= 0 && col >= 0 && bloc != null);
            return ok;
        }
        #endregion
    }
}
