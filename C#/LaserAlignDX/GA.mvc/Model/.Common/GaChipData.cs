#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-10-05 精算優化 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using JetEazy.Match;
using JetEazy.QMath;
using JetEazy.QvMath;
using LeTian.AoiLib;

namespace LaserAlignDX.Model
{
    public class GaChipData
    {
        /// <summary>
        /// 晶粒定位結果 (單位 pixels) (Cammera Coordinates)
        /// </summary>
        public QvBox2D LocBox2D { get; set; }

        /// <summary>
        /// 格點型晶粒的 PAD 格點 (單位 pixels) (Cammera Coordinates)
        /// </summary>
        public EzBlocsGrid PadGrids { get; set; }

        /// <summary>
        /// 抓到的邊線 (左上右下) (單位 pixels) (Cammera Coordinates)
        /// (圖示用)
        /// </summary>
        public EzLSD.LineSegment[] LineSegments { get; set; }

        /// <summary>
        /// 尺寸量測點 (左上右下) (單位 pixels) (Cammera Coordinates)
        /// (除錯用)
        /// </summary>
        public QVector[] MeasureCalcPoints { get; set; }
    }
}
