#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


namespace LaserAlignDX.Model.Coords
{
    using P = TravellerCoords.P;

    /// <summary>
    /// 空台上的格點 (mm)
    /// </summary>
    public class PlcGridPoints
    {
        #region PROTECTED_DATA
        protected P _org = new P(0, 0);
        #endregion

        public PlcGridPoints()
        {

        }
        public PlcGridPoints(int rows = 22, int cols = 7, double pitchX = 9.30, double pitchY = 10.64)
        {
            Rows = rows;
            Cols = cols;
            PitchX = pitchX;
            PitchY = pitchY;
        }
        public void Offset(double dx, double dy)
        {
            _org.X = dx;
            _org.Y = dy;
        }

        public int Rows
        {
            get;
            set;
        }
        public int Cols
        {
            get;
            set;
        }

        public double PitchX
        {
            get; set;
        }
        public double PitchY
        {
            get; set;
        }

        public P GetGridPoint(int row, int col)
        {
            if (row < 0)
                row = Rows + row;
            if (col < 0)
                col = Cols + col;

            double x = PitchX * col + _org.X;
            double y = PitchY * row + _org.Y;
            //double x = _org.X;
            //double y = _org.Y;
            //for (int i = 0; i < col; i++) x += PitchX;
            //for (int i = 0; i < row; i++) y += PitchY;
            return new P(x, y);
        }
        public P this[int row, int col]
        {
            get => GetGridPoint(row, col);
        }
    }
}
