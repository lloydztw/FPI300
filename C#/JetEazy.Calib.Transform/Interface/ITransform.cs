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
using System;


namespace JetEazy.Transform
{
    /// <summary>
    /// 座標轉換
    /// </summary>
    public interface ITransform : IDisposable
    {
        QVector Trans(QVector pt);
        QVector InvTrans(QVector pt);

        ICalibGridPoints GetCalibGridPoints();
        ICalibCornerPoints GetCalibCornerPoints();

        bool Build();
        bool CheckBuildCondition(out double det, out double det2);

        void Load(string filename); 
        void Save(string filename);
    }
}
