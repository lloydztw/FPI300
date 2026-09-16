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

using JetEazy.QMath;
using Newtonsoft.Json;


namespace LaserAlignDX.Model.Coords.V35
{
    /// <summary>
    /// 理想的格點 (mm)
    /// </summary>
    internal class QWorldGridPoints : IWorldGridPoints
    {
        #region PROTECTED_DATA
        protected QVector _org = new QVector(0, 0);
        #endregion

        public QWorldGridPoints()
        {
            Config(22, 7, 9.6, 10.5);
        }
        public QWorldGridPoints(int rows, int cols, double pitchX, double pitchY)
        {
            Config(rows, cols, pitchX, pitchY);
        }

        public void Config(int rows, int cols, double pitchX, double pitchY)
        {
            Rows = rows;
            Cols = cols;
            PitchX = pitchX;
            PitchY = pitchY;
        }
        void Offset(double dx, double dy)
        {
            _org.X = dx;
            _org.Y = dy;
        }

        public int Rows
        {
            get;
            private set;
        }
        public int Cols
        {
            get;
            private set;
        }
        public double PitchX
        {
            get; 
            private set;
        }
        public double PitchY
        {
            get; 
            private set;
        }

        [JsonIgnore]
        public QVector this[int row, int col]
        {
            get => Get(row, col);
        }
        public QVector Get(int row, int col)
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
            return new QVector(x, y);
        }

        public void Load(string iniFile, string sectName)
        {
            if (sectName == null)
                sectName = GetType().Name;
            int rows = 0, cols = 0;
            double px = 0, py = 0;
            JetEazy.Win32.Win32Ini.Load(ref rows, iniFile, sectName, "Rows");
            JetEazy.Win32.Win32Ini.Load(ref cols, iniFile, sectName, "Cols");
            JetEazy.Win32.Win32Ini.Load(ref px, iniFile, sectName, "PitchX");
            JetEazy.Win32.Win32Ini.Load(ref py, iniFile, sectName, "PitchY");
            if (rows > 0) Rows = rows;
            if (cols > 0) Cols = cols;
            if (px > 0) PitchX = px;
            if (py > 0) PitchY = py;
        }
        public void Save(string iniFile, string sectName)
        {
            if (sectName == null)
                sectName = GetType().Name;
            JetEazy.Win32.Win32Ini.Save(Rows, iniFile, sectName, "Rows");
            JetEazy.Win32.Win32Ini.Save(Cols, iniFile, sectName, "Cols");
            JetEazy.Win32.Win32Ini.Save(PitchX, iniFile, sectName, "PitchX");
            JetEazy.Win32.Win32Ini.Save(PitchY, iniFile, sectName, "PitchY");
        }
    }
}
