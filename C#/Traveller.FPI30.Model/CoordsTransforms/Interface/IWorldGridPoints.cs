#region AUTHOR
/*
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * REVISION:
 *
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 *      2025-08-13 初稿 (by LeTian Chang)
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 */
#endregion

using JetEazy.QMath;

namespace LaserAlignDX.Model.Coords
{
    /// <summary>
    /// 理想的格點 (mm)
    /// </summary>
    public interface IWorldGridPoints
    {
        int Rows { get; }
        int Cols { get; }
        
        double PitchX { get; }
        double PitchY { get; }
        
        void Config(int rows, int cols, double pitchX, double pitchY);
        QVector Get(int row, int col);

        void Load(string iniFile, string sectName);
        void Save(string iniFile, string sectName);
    }
}