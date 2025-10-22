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
using System;
using System.Collections.Generic;
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
        /// 樣板 Quad2D (單位 pixels) (GoldenFov Cammera Coordinates)
        /// </summary>
        public QvQuad2D GoldenQuad2D { get; set; } = null;

        /// <summary>
        /// 晶粒定位結果 (單位 pixels) (FullFov Cammera Coordinates)
        /// </summary>
        public QvQuad2D ChipQuad2D { get; set; } = null;

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
        /// 定位後之世界座標
        /// </summary>
        public readonly GaChipCoordindates ChipCoords = new GaChipCoordindates();

        /// <summary>
        /// 量測後之物理尺寸
        /// </summary>
        public readonly GaChipDimension ChipDimension = new GaChipDimension();

        /// <summary>
        /// PAD 邊隙 (單位 mm)
        /// </summary>
        public readonly GaPadEdgeGaps PadEdgeGaps = new GaPadEdgeGaps();

        /// <summary>
        /// 調試用 之 額外資料
        /// </summary>
        public object DebugRigidBodyData { get; set; } = null;
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
        /// 量測结果: 晶粒宽度 (單位 mm)
        /// </summary>
        public float ChipWidth { get; set; } = 0;
        /// <summary>
        /// 量測结果: 晶粒高度 (單位 mm)
        /// </summary>
        public float ChipHeight { get; set; } = 0;
        /// <summary>
        /// 尺寸量測點 (左上右下) (單位 pixels) 
        /// (FullFov Cammera Coordinates)
        /// (顯示繪圖用)
        /// </summary>
        public QVector[] DimMeasurePoints { get; set; } = null;
    }


    public class GaPadEdgeGaps
    {
        /// <summary>
        /// 左上 邊隙 (單位 mm)
        /// </summary>
        public PointF LU = new PointF(0, 0);
        /// <summary>
        /// 右上 邊隙 (單位 mm)
        /// </summary>
        public PointF RU = new PointF(0, 0);
        /// <summary>
        /// 右下 邊隙 (單位 mm)
        /// </summary>
        public PointF RD = new PointF(0, 0);
        /// <summary>
        /// 左下 邊隙 (單位 mm)
        /// </summary>
        public PointF LD = new PointF(0, 0);

        public float GetGapSize(EdgeBorder e)
        {
            double value = 0f;
            switch (e)
            {
                case EdgeBorder.Left:
                    value = (LU.X + LD.X) / 2f; 
                    break;
                case EdgeBorder.Right:
                    value = (RU.X + RD.X) / 2f;
                    break;
                case EdgeBorder.Top:
                    value = (LU.Y + RU.Y) / 2f;
                    break;
                case EdgeBorder.Bottom:
                    value = (LD.Y + RD.Y) / 2f;
                    break;
            }
            return (float)Math.Round(value, 3);
        }
        public IEnumerable<PointF> IterItems()
        {
            yield return LU;
            yield return RU;
            yield return RD;
            yield return LD;
        }

        /// <summary>
        /// 尺寸量測點 (左上右下) (單位 pixels) 
        /// (FullFov Cammera Coordinates)
        /// (顯示繪圖用)
        /// </summary>
        public QVector[] GapMeasurePoints { get; set; }
    }
}
