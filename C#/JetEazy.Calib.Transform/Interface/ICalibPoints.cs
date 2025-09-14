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


namespace JetEazy.Transform
{
    public interface ICalibGridPoints
    {
        int Rows { get; }
        int Cols { get; }

        void SetAll(QVector[,] srcs, QVector[,] dsts);
        void Update(int row, int col, QVector src, QVector dst);
        void Get(int row, int col, out QVector src, out QVector dst);
    }

    public interface ICalibCornerPoints
    {
        int Counts { get; }
        QVector[] GetAll(bool isSrc);
        void Get(int cornerIndex, out QVector src, out QVector dst);
        void Set(int cornerIndex, QVector src, QVector dst);
    }
}
