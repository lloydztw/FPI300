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


namespace EzAoiEmptyTrayInspector.Model
{
    internal class ResultDebug
    {
        public static void DUMP(EzBlocsGrid grid)
        {
            if (grid == null)
                return;

            _TRACE("EzBlocsGrid :");
            for (int r = grid.RowMin; r < grid.RowMax; r++)
            {
                string line = $"[{r:00}] ";
                for (int c = grid.ColMin; c < grid.ColMax; c++)
                {
                    string symbol = " ";
                    var bloc = grid.Get(r, c);
                    if (bloc != null)
                    {
                        bool isSucker = EzEmptyTrayResult.IsSucker(bloc);
                        if (isSucker)
                            symbol = "_";
                        else
                            symbol = "x";
                    }
                    line += " " + symbol;
                }
                _TRACE(line);
            }
        }

        public static void DUMP(EzEmptyTrayResult result)
        {
            if (result == null)
                return;

            _TRACE("EzEmptyTrayResult :");
            int fullRows = result.FullRows;
            int fullCols = result.FullCols;
            for (int r = 0; r < fullRows; r++)
            {
                string line = $"[{r:00}] ";
                for (int c = 0; c < fullCols; c++)
                {
                    result.GetBlocByRowCol(r, c, out EzBloc bloc, out bool isOK);
                    string symbol = " ";
                    if (bloc != null)
                    {
                        if (isOK)
                            symbol = "_";
                        else
                            symbol = "x";
                    }
                    line += " " + symbol;
                }
                _TRACE(line);
            }
        }

        static void _TRACE(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }
    }
}
