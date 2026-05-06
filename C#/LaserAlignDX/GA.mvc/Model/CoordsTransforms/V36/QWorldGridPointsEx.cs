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

using EzAoiEmptyTrayInspector.Model;
using JetEazy.Match;
using JetEazy.QMath;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LaserAlignDX.Model.Coords.V36
{
    /// <summary>
    /// 理想的格點 (mm)
    /// </summary>
    internal class QWorldGridPointsEx : IWorldGridPoints
    {
        #region PROTECTED_DATA
        protected QVector[,] _gridNodes;
        protected QVector[,] _offsets;
        #endregion

        public QWorldGridPointsEx(int rows, int cols, double pitchX, double pitchY)
        {
            Config(rows, cols, pitchX, pitchY);
        }
        public QWorldGridPointsEx(IWorldGridPoints src = null)
        {
            if (src == null)
            {
                Config(22, 7, 9.6, 10.5);
                return;
            }

            int rows = src.Rows;
            int cols = src.Cols;
            Config(rows, cols, src.PitchX, src.PitchY);

            var srcOffsets = (src as QWorldGridPointsEx)?._offsets;
            if (srcOffsets != null)
            {
                var dst = _offsets = new QVector[rows, cols];
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        var offset = safeGet(srcOffsets, r, c);
                        dst[r, c] = offset != null ? new QVector(offset) : new QVector2(0, 0);
                    }
                }
            }
        }

        /// <summary>
        /// 一般常規組態
        /// </summary>
        public void Config(int rows, int cols, double pitchX, double pitchY)
        {
            Rows = rows;
            Cols = cols;
            PitchX = pitchX;
            PitchY = pitchY;
            generateNodes();
        }

        #region PUBLILC_SEGEMENTS_FUNCTIONS
        public bool HasOffsets()
        {
            return _offsets != null;
        }

        /// <summary>
        /// 外掛不連續區塊
        /// </summary>
        public void SetSegments(EzBlocsGrid camGrid, IList<JxTraySegItem> segsList)
        {
            if (segsList == null || segsList.Count <= 1)
            {
                _offsets = null;
                return;
            }

            if (_gridNodes == null)
            {
                throw new Exception("請先調用 Config(rows, cols, pitchX, pitchY)!");
                return;
            }

            int rows = camGrid.Rows;
            int cols = camGrid.Cols;
            rows = Math.Min(rows, Rows);
            cols = Math.Min(cols, Cols);

            //!!! 不要在此 改變 Rows, Cols !!!
            //if (rows != Rows || cols != Cols)
            //{
            //    Rows = rows; Cols = cols;
            //    generateNodes();
            //}

            _offsets = new QVector[rows, cols];

            for (int id = 1; id < segsList.Count; id++)
            {
                var seg = segsList[id];
                var segRect = seg.BoundBox.Value;
                var segOffset = new QVector2((double)seg.OffsetX.Value, (double)seg.OffsetY.Value);

                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        var camBloc = camGrid.Get(row, col);
                        if (camBloc != null && segRect.Contains(camBloc.CenterX, camBloc.CenterY))
                            _offsets[row, col] = segOffset;
                        else
                            _offsets[row, col] = new QVector2(0, 0);
                    }
                }
            }
        }

        /// <summary>
        /// 取得不連續區塊的 Offset
        /// </summary>
        public QVector GetSegmentOffset(int row, int col)
        {
            return safeGet(_offsets, row, col);
        }
        #endregion

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
            var coord = safeGet(_gridNodes, row, col);
            var offset = safeGet(_offsets, row, col);
            if (coord != null && offset != null)
                return coord + offset;
            return coord;
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
            generateNodes();

            bool hasOffsets = false;
            JetEazy.Win32.Win32Ini.Load(ref hasOffsets, iniFile, sectName, "HasSegOffsets");
            if (hasOffsets)
                loadOffsetsTable(iniFile);
        }
        public void Save(string iniFile, string sectName)
        {
            if (sectName == null)
                sectName = GetType().Name;

            JetEazy.Win32.Win32Ini.Save(Rows, iniFile, sectName, "Rows");
            JetEazy.Win32.Win32Ini.Save(Cols, iniFile, sectName, "Cols");
            JetEazy.Win32.Win32Ini.Save(PitchX, iniFile, sectName, "PitchX");
            JetEazy.Win32.Win32Ini.Save(PitchY, iniFile, sectName, "PitchY");

            bool hasOffsets = _offsets != null;
            JetEazy.Win32.Win32Ini.Save(hasOffsets, iniFile, sectName, "HasSegOffsets");
            if (hasOffsets)
                saveOffsetsTable(iniFile);
        }

        #region PRIVATE_FILE_FUNCTIONS
        void loadOffsetsTable(string fileName)
        {
            _offsets = null;

            fileName = System.IO.Path.ChangeExtension(fileName, "_SegsOffsetTable.dat");
            if (!System.IO.File.Exists(fileName))
                return;

            _offsets = new QVector[Rows, Cols];
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    _offsets[r, c] = new QVector2(0, 0);

            var lines = System.IO.File.ReadAllLines(fileName);
            foreach (var line in lines)
            {
                var strs = line.Split(',');
                if (strs.Length >= 4 &&
                    int.TryParse(strs[0], out var row) &&
                    int.TryParse(strs[1], out var col) &&
                    double.TryParse(strs[2], out var x) &&
                    double.TryParse(strs[3], out var y) &&
                    row < Rows && col < Cols &&
                    row >= 0 && col >= 0)
                {
                    _offsets[row, col] = new QVector2(x, y);
                }
            }
        }
        void saveOffsetsTable(string fileName)
        {
            var lines = new List<string>(); 
            for(int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    var offset = safeGet(_offsets, row, col);
                    var x = offset != null ? offset.X : 0;
                    var y = offset != null ? offset.Y : 0;
                    lines.Add($"{row}, {col}, {x:0.0000}, {y:0.0000}");
                }
            }
            fileName = System.IO.Path.ChangeExtension(fileName, "_SegsOffsetTable.dat");
            System.IO.File.WriteAllLines(fileName, lines);
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        QVector safeGet(QVector[,] arr, int row, int col)
        {
            if (arr == null)
                return null;

            while (row < 0)
                row = Rows + row;

            while (col < 0)
                col = Cols + col;

            if (row < Rows && col < Cols)
                return arr[row, col];
            else
                return null;
        }
        void generateNodes()
        {
            int rows = Rows;
            int cols = Cols;
            var pitchX = PitchX;
            var pitchY = PitchY;

            _gridNodes = new QVector[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    double x = c * pitchX;
                    double y = r * pitchY;
                    _gridNodes[r, c] = new QVector2(x, y);
                }
            }
        }
        #endregion
    }
}
