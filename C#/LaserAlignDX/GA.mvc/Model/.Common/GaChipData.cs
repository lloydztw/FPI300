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
using OpenCvSharp;
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

#if (OPT_OLD_LINE_BORDER_FUNCTIONS)
        /// <summary>
        /// 抓到的邊線 (左上右下) (單位 pixels) (FullFov Cammera Coordinates)
        /// (圖示用)
        /// </summary>
        public EzLSD.LineSegment[] LineSegments
        {
            get
            {
                var lines = new EzLSD.LineSegment[4];
                LineBorderPairs.Get(EdgeBorder.Left, out lines[0]);
                LineBorderPairs.Get(EdgeBorder.Top, out lines[1]);
                LineBorderPairs.Get(EdgeBorder.Right, out lines[2]);
                LineBorderPairs.Get(EdgeBorder.Bottom, out lines[3]);
                return lines;
            }
            set
            {
                var lines = value;
                if (lines == null || lines.Length < 4) return;
                LineBorderPairs.Set(EdgeBorder.Left, lines[0]);
                LineBorderPairs.Set(EdgeBorder.Top, lines[1]);
                LineBorderPairs.Set(EdgeBorder.Right, lines[2]);
                LineBorderPairs.Set(EdgeBorder.Bottom, lines[3]);
            }
        }
        /// <summary>
        /// 邊線拉框 (左上右下) (單位 pixels) (FullFov Cammera Coordinates)
        /// </summary>
        public QvBox2D[] LineBorderBoxes //{ get; set; } = new QvBox2D[4];
        {
            get
            {
                var borderBoxes = new QvBox2D[4];
                LineBorderPairs.Get(EdgeBorder.Left, out borderBoxes[0]);
                LineBorderPairs.Get(EdgeBorder.Top, out borderBoxes[1]);
                LineBorderPairs.Get(EdgeBorder.Right, out borderBoxes[2]);
                LineBorderPairs.Get(EdgeBorder.Bottom, out borderBoxes[3]);
                return borderBoxes;
            }
            set
            {
                var borderBoxes = value;
                if (borderBoxes == null || borderBoxes.Length < 4) return;
                LineBorderPairs.Set(EdgeBorder.Left, borderBoxes[0]);
                LineBorderPairs.Set(EdgeBorder.Top, borderBoxes[1]);
                LineBorderPairs.Set(EdgeBorder.Right, borderBoxes[2]);
                LineBorderPairs.Set(EdgeBorder.Bottom, borderBoxes[3]);
            }
        }
#endif

        /// <summary>
        /// 邊線拉框 配對
        /// </summary>
        /// <remarks>
        /// 單位 pixels (FullFov Cammera Coordinates)
        /// </remarks>
        public readonly LineBorderPairsCollection LineBorderPairs = new LineBorderPairsCollection();

        /// <summary>
        /// 計算晶粒 尺寸量測 之 邊界多邊形
        /// </summary>
        /// <remarks>
        /// 單位 pixels (FullFov Cammera Coordinates)
        /// </remarks>
        public void CalcChipBoundaryPolygon(out Point2f[] polygonPoints)
        {
            polygonPoints = null;

            #region CASE_1_如果有抓到四邊線
            var quadLines = LineBorderPairs.GetQuadLineSegments();
            if (quadLines != null && quadLines.Length >= 4)
            {
                var quadCorners = new List<QVector>();
                for (int i = 0, NP = quadLines.Length; i < NP; i++)
                {
                    int j = (i + 1) % NP;
                    var line1 = quadLines[i];
                    var line2 = quadLines[j];
                    if (line1 == null || line2 == null) continue;
                    var pt = line1.CalcIntersectedPoint(line2);
                    if (pt == null) continue;
                    quadCorners.Add(pt);
                }

                if (quadCorners.Count >= 4)
                {
                    polygonPoints = Array.ConvertAll(quadCorners.ToArray(), p => new OpenCvSharp.Point2f((float)p.X, (float)p.Y));
                    return;
                }
            }
            #endregion

            #region CASE2_使用_ChipQuad2D
            var chipQuad = this.ChipQuad2D;
            if (chipQuad != null)
            {
                polygonPoints = Array.ConvertAll(chipQuad.Corners, p => new OpenCvSharp.Point2f((float)p.X, (float)p.Y));
            }
            #endregion`
        }

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

        ///<summary>
        /// 瑕疵區塊
        /// </summary>
        public QvBox2D[] DefectBlobs { get; set; } = null;

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


#if (OPT_拉出至獨立檔案)
    public class GaChipDimension
    {
        public bool IsSimpleQuad { get; set; } = true;

        /// <summary>
        /// 量測結果 (單位 mm)
        /// </summary>
        public readonly Dictionary<string, float> Measurements = new Dictionary<string, float>();
        
        /// <summary>
        /// 量測 Pass / NG
        /// </summary>
        public readonly Dictionary<string, bool> MeasureResults = new Dictionary<string, bool>();

        /// <summary>
        /// 量測點對 (單位 pixels)
        /// </summary>
        public readonly Dictionary<string, QVector[]> MeasureCamPtPairs = new Dictionary<string, QVector[]>();

        public void Reset()
        {
            Measurements.Clear();
            MeasureCamPtPairs.Clear();
            MeasureResults.Clear();
        }

        public bool IsAllPass()
        {
            //int passCount = 0;
            //if (PassNgResults != null)
            //{
            //    foreach (var pass in PassNgResults)
            //    {
            //        if (pass)
            //            passCount++;
            //    }
            //}
            //return passCount >= 2;

            if (MeasureResults.Count == 0)
                return false;

            foreach (var pass in MeasureResults.Values)
            {
                if (!pass)
                    return false;
            }

            return true;
        }

        #region 量測點相關函式
        public IEnumerable<QVector> IterMeasureCamPoint()
        {
            foreach (var key in MeasureCamPtPairs.Keys)
            {
                var pts = MeasureCamPtPairs[key];
                if (pts != null)
                {
                    foreach (var pt in pts)
                    {
                        if (pt != null)
                            yield return pt;
                    }
                }
            }
        }
        public QVector[] GetMeasureCamPointsQuad()
        {
            if (MeasureCamPtPairs.TryGetValue("X", out QVector[] ptsH) &&
                MeasureCamPtPairs.TryGetValue("Y", out QVector[] ptsV))
            {
                var pts = new QVector[]
                {
                    ptsH?[0],
                    ptsV?[0],
                    ptsH?[1],
                    ptsV?[1],
                };
                //foreach (var pt in pts)
                //{
                //    if (pt == null)
                //        return null;
                //}
                return pts;
            }
            return null;
        }
        public void SetMeasureCamPointsQuad(QVector[] pts)
        {
            if (pts.Length >= 4)
            {
                if (!MeasureCamPtPairs.TryGetValue("X", out QVector[] ptsH))
                    ptsH = new QVector[2];
                if (!MeasureCamPtPairs.TryGetValue("Y", out QVector[] ptsV))
                    ptsV = new QVector[2];
                ptsH[0] = pts[0];
                ptsV[0] = pts[1];
                ptsH[1] = pts[2];
                ptsV[1] = pts[3];
            }
        }
        #endregion

        #region SIMPLE_QUAD_接口
        /// <summary>
        /// 量測结果: 晶粒尺寸X (單位 mm)
        /// </summary>
        public float ChipWidth
        {
            get => Measurements.TryGetValue("X", out var width) ? width : 0f;
            set => Measurements["X"] = value;
        }
        /// <summary>
        /// 量測结果: 晶粒尺寸Y (單位 mm)
        /// </summary>
        public float ChipHeight
        {
            get => Measurements.TryGetValue("Y", out var width) ? width : 0f;
            set => Measurements["Y"] = value;
        }
        /// <summary>
        /// 尺寸量測點 (左上右下) (單位 pixels) 
        /// (FullFov Cammera Coordinates)
        /// (顯示繪圖用)
        /// </summary>
        public QVector[] DimMeasurePoints
        {
            get => GetMeasureCamPointsQuad();
            set => SetMeasureCamPointsQuad(value);
        }
        /// <summary>
        /// 取得晶粒 像素 長寬 (單位 pixels) (GUI 顯示用)
        /// </summary>
        public bool GetPixelSize(out double pixWidth, out double pixHeight)
        {
            pixWidth = 0;
            pixHeight = 0;

            //// 0:左, 1:上, 2:右, 3:下
            //var pts = DimMeasurePoints;
            //if (pts == null)
            //    return false;

            //if (pts[0] != null && pts[2] != null)
            //    pixWidth = Math.Round((pts[0] - pts[2]).NormLength, 1);
            //if (pts[1] != null && pts[3] != null)
            //    pixHeight = Math.Round((pts[1] - pts[3]).NormLength, 1);

            if (MeasureCamPtPairs.TryGetValue("X", out QVector[] pts) && pts.Length > 1 && pts[0] != null && pts[1] != null)
            {
                pixWidth = Math.Round((pts[0] - pts[1]).NormLength, 1);
            }
            if (MeasureCamPtPairs.TryGetValue("Y", out pts) && pts.Length > 1 && pts[0] != null && pts[1] != null)
            {
                pixHeight = Math.Round((pts[0] - pts[1]).NormLength, 1);
            }

            return pixWidth > 0 && pixHeight > 0;
        }
#if(false)
        /// <summary>
        /// Runtime Results
        /// </summary>
        public bool[] PassNgResults
        {
            get
            {
                //foreach(var kv in MeasureResults)
                return MeasureResults.Values.ToArray();
            }
        }
#endif
        #endregion
    }
#endif

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

        /// <summary>
        /// 邊隙兩兩平均
        /// </summary>
        public double GetAveGap(EdgeBorder eb)
        {
            switch (eb)
            {
                case EdgeBorder.Left:
                    return (LU.X + LD.X) / 2.0;
                case EdgeBorder.Right:
                    return (RU.X + RD.X) / 2.0;
                case EdgeBorder.Top:
                    return (LU.Y + RU.Y) / 2.0;
                case EdgeBorder.Bottom:
                    return (LD.Y + RD.Y) / 2.0;
            }
            return 0;
        }

        /// <summary>
        /// 邊隙差異
        /// </summary>
        public double GetAveGapDiff(EdgeBorder e1 = EdgeBorder.Left, EdgeBorder e2 = EdgeBorder.Right)
        {
            var diff = Math.Abs(GetAveGap(e1) - GetAveGap(e2));
            return diff;
        }

        #region 力成版_四角_PAD_中心_到_邊線_的距離
        public double[] S => _S;
        public double S1 { get => _S[0]; set => _S[0] = value; }
        public double S2 { get => _S[1]; set => _S[1] = value; }
        public double S3 { get => _S[2]; set => _S[2] = value; }
        public double S4 { get => _S[3]; set => _S[3] = value; }
        public double S5 { get => _S[4]; set => _S[4] = value; }
        public double S6 { get => _S[5]; set => _S[5] = value; }
        public double S7 { get => _S[6]; set => _S[6] = value; }
        public double S8 { get => _S[7]; set => _S[7] = value; }
        #endregion

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

        public bool[] Check(out bool isPass, QVector min, QVector max, double maxGapDiff)
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

            // 檢查 左右邊隙 差值
            if (maxGapDiff > 0)
            {
                IsGapDiffNG = GetAveGapDiff() > maxGapDiff;
                if (IsGapDiffNG)
                    isPass = false;
            }

            this.PassNgResults = results;
            return results;
        }

        /// <summary>
        /// Runtime Results
        /// </summary>
        public bool[] PassNgResults
        {
            get;
            private set;
        }

        public bool IsGapDiffNG
        {
            get; 
            private set;
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
            return passCount >= 8 && !IsGapDiffNG;
        }
    }
}
