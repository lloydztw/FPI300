using EzAoiEmptyTrayInspector.Model;
using System.Collections.Generic;


namespace LaserAlignDX.RunSpace
{
    public static class Zigzag
    {
        public static IEnumerable<(int,int)> IterZigzag(this EzEmptyTrayResult result)
        {
            if (result == null)
                yield break;

            int fullRows = result.FullRows;
            int fullCols = result.FullCols;
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
    }
}
