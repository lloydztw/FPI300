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
using System;
using System.Collections.Generic;
using System.Drawing;


namespace LaserAlignDX.Model
{
    public class GaChipData
    {
        public bool IsEmpty()
        {
            return ChipQuad2D == null;
        }

        /// <summary>
        /// 是否位於 格位範圍之外
        /// </summary>
        public bool IsOutGrid { get; set; } = false;

        /// <summary>
        /// 運算截圖時的 Roi (單位 pixels) (FullFov Cammera Coordinates)
        /// </summary>
        public RectangleF CellRoi { get; set; }

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
        /// <summary>
        /// 傾斜程度 (晶粒踩腳嚴重程度 0.0 ~ 1.0)
        /// </summary>
        public float TiltRatio { get; set; } = 0;
        /// <summary>
        /// 是否為踩腳 (Runtime)
        /// </summary>
        public bool IsStampede { get; set; } = false;
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
        public QVector[] DimMeasurePoints { get; set; }
        /// <summary>
        /// 取得晶粒 像素 長寬 (單位 pixels)
        /// </summary>
        public bool GetPixelSize(out double pixWidth, out double pixHeight)
        {
            pixWidth = 0;
            pixHeight = 0;

            // 0:左, 1:上, 2:右, 3:下
            var pts = DimMeasurePoints;
            if (pts == null)
                return false;

            if (pts[0] != null && pts[2] != null)
                pixWidth = Math.Round((pts[0] - pts[2]).NormLength, 1);
            if (pts[1] != null && pts[3] != null)
                pixHeight = Math.Round((pts[1] - pts[3]).NormLength, 1);

            return pixWidth > 0 && pixHeight > 0;
        }

        /// <summary>
        /// Runtime Results
        /// </summary>
        public bool[] PassNgResults
        {
            get;
            set;
        }
        /// <summary>
        /// Runtime Results
        /// </summary>
        public bool IsAllPass()
        {
            int passCount = 0;
            if (PassNgResults != null)
            {
                foreach (var pass in PassNgResults)
                {
                    if (pass)
                        passCount++;
                }
            }
            return passCount >= 2;
        }
    }


    public class GaPadEdgeGaps
    {
        #region PRIVATE_DATA
        QVector[] _gaps = new QVector[]
        {
            new QVector(0, 0),
            new QVector(0, 0),
            new QVector(0, 0),
            new QVector(0, 0),
        };
        double[] _S = new double[8];
        #endregion

        /// <summary>
        /// 左上 邊隙 (單位 mm)
        /// </summary>
        public QVector LU => _gaps[0];
        /// <summary>
        /// 右上 邊隙 (單位 mm)
        /// </summary>
        public QVector RU => _gaps[1];
        /// <summary>
        /// 右下 邊隙 (單位 mm)
        /// </summary>
        public QVector RD => _gaps[2];
        /// <summary>
        /// 左下 邊隙 (單位 mm)
        /// </summary>
        public QVector LD => _gaps[3];

        public double[] S => _S;
        public double S1 { get => _S[0]; set => _S[0] = value; }
        public double S2 { get => _S[1]; set => _S[1] = value; }
        public double S3 { get => _S[2]; set => _S[2] = value; }
        public double S4 { get => _S[3]; set => _S[3] = value; }
        public double S5 { get => _S[4]; set => _S[4] = value; }
        public double S6 { get => _S[5]; set => _S[5] = value; }
        public double S7 { get => _S[6]; set => _S[6] = value; }
        public double S8 { get => _S[7]; set => _S[7] = value; }

        public IEnumerable<QVector> IterItems()
        {
            yield return LU;
            yield return RU;
            yield return RD;
            yield return LD;
        }

        /// <summary>
        /// 尺寸量測點: 順序: LDX(2), LUX(2), LUY(2), RUY(2), RUX(2), RDX(2), RDY(2), LDY(2)
        /// (單位 pixels) (FullFov Cammera Coordinates)
        /// (顯示繪圖用)
        /// </summary>
        public QVector[] GapMeasurePoints { get; set; }

        public bool[] Check(out bool isPass, QVector min, QVector max)
        {
            isPass = true;

            // 順序: LDX, LUX, LUY, RUY, RUX, RDX, RDY, LDY
            bool[] results = new bool[8];
            int i = 0;

            isPass &= results[i++] = min.X <= LD.X && LD.X <= max.X;
            isPass &= results[i++] = min.X <= LU.X && LU.X <= max.X;

            isPass &= results[i++] = min.Y <= LU.Y && LU.Y <= max.Y;
            isPass &= results[i++] = min.Y <= RU.Y && RU.Y <= max.Y;

            isPass &= results[i++] = min.X <= RU.X && RU.X <= max.X;
            isPass &= results[i++] = min.X <= RD.X && RD.X <= max.X;

            isPass &= results[i++] = min.Y <= RD.Y && RD.Y <= max.Y;
            isPass &= results[i++] = min.Y <= LD.Y && LD.Y <= max.Y;

            this.PassNgResults = results;
            return results;
        }

        /// <summary>
        /// Runtime Results
        /// </summary>
        public bool[] PassNgResults
        {
            get;
            set;
        }
        /// <summary>
        /// Runtime Results
        /// </summary>
        public bool IsAllPass()
        {
            int passCount = 0;
            if (PassNgResults != null)
            {
                foreach (var pass in PassNgResults)
                {
                    if (pass)
                        passCount++;
                }
            }
            return passCount >= 8;
        }
    }
}
