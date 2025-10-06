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
using LaserAlignDX.BasicSpace;
using LeTian.AoiLib;
using System.Drawing;


namespace LaserAlignDX.Model
{
    public class GaChipData
    {
        /// <summary>
        /// 運算截圖時的 Roi (單位 pixels) (FullFov Cammera Coordinates)
        /// </summary>
        public RectangleF Roi { get; set; }

        /// <summary>
        /// 晶粒定位結果 (單位 pixels) (FullFov Cammera Coordinates)
        /// </summary>
        public QvBox2D ChipBox2D { get; set; } = null;

        /// <summary>
        /// 格點型晶粒的 PAD 格點 (單位 pixels) (FullFov Cammera Coordinates)
        /// </summary>
        public EzBlocsGrid PadsGrid { get; set; } = null;

        /// <summary>
        /// 抓到的邊線 (左上右下) (單位 pixels) (FullFov Cammera Coordinates)
        /// (圖示用)
        /// </summary>
        public EzLSD.LineSegment[] LineSegments { get; set; } = new EzLSD.LineSegment[4];

        /// <summary>
        /// 邊線拉框 (左上右下) (單位 pixels) (FullFov Cammera Coordinates)
        /// </summary>
        public QvBox2D[] LineBorderBoxes { get; set; } = new QvBox2D[4];

        /// <summary>
        /// 尺寸量測點 (左上右下) (單位 pixels) (FullFov Cammera Coordinates)
        /// (除錯用)
        /// </summary>
        public QVector[] DimMeasurePoints { get; set; } = new QVector[4];

        /// <summary>
        /// 定位後之世界座標
        /// </summary>
        public readonly GaChipCoordindates ChipCoords = new GaChipCoordindates();

        /// <summary>
        /// 量測後之物理尺寸
        /// </summary>
        public readonly GaChipDimension ChipDimension = new GaChipDimension();
    }


    public class GaChipCoordindates
    {
        /// <summary>
        /// 中心位置 (單位 mm) (world coordinates)
        /// </summary>
        public QVector Centroid { get; set; } = new QVector(0, 0);
        /// <summary>
        /// 旋轉角度 (單位 degree) (world coordinates)
        /// </summary>
        public float Angle { get; set; } = 0;
    }


    public class GaChipDimension
    {
        /// <summary>
        /// 量測结果: 晶粒宽度
        /// </summary>
        public float ChipWidth { get; set; } = 0;
        /// <summary>
        /// 量測结果: 晶粒高度
        /// </summary>
        public float ChipHeight { get; set; } = 0;
        /// <summary>
        /// 量測结果: 格點型晶粒 邊緣厚度 (順序: 左上右下)
        /// </summary>
        public float[] PadEdgeSizes { get; set; } = new float[4];
        /// <summary>
        /// 量測结果: 格點型晶粒 邊緣厚度 左右差 (X方向)
        /// </summary>
        public float PadEdgeDiffX
        {
            get => PadEdgeSizes[(int)EdgeBorder.Left] - PadEdgeSizes[(int)EdgeBorder.Right];
        }
        /// <summary>
        /// 量測结果: 格點型晶粒 邊緣厚度 上下差 (Y方向)
        /// </summary>
        public float PadEdgeDiffY
        {
            get => PadEdgeSizes[(int)EdgeBorder.Top] - PadEdgeSizes[(int)EdgeBorder.Bottom];
        }
    }
}
