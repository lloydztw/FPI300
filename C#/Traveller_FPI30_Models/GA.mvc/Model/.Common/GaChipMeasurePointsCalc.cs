#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-09-13 重整 (by LeTian Chang)
 *      2026-09-14 漏洞修補與重構：增加空值防護、例外攔截、邊界檢核並消除重複運算 (by AI Assistant)
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
using JetEazy.Utils;
using LaserAlignDX.Model;
using LaserAlignDX.Mvc.Model;
using LeTian.AoiLib;
using System;
using System.Collections.Generic;

namespace LaserAlignDX.AoiModel
{
    /// <summary>
    /// 量測點計算器
    /// </summary>
    public static class GaChipMeasurePointsCalc
    {
        /// <summary>
        /// 計算 尺寸量測 基本 4點位 (順序: 左, 上, 右, 下)
        /// </summary>
        /// <param name="edgeLines">輸入: Runtime 抓到的 邊線 (順序: 左邊線, 上邊線, 右邊線, 下邊線)</param>
        /// <param name="measurePoints">輸出: 尺寸量測 所有點位</param>
        /// <returns>錯誤碼</returns>
        /// <remarks>此函式座標都是 Fullfov Camera Coordinates</remarks>
        public static ErrorCodes CalcDimMeasurePoints(this GaChipData chipData, EzLSD.LineSegment[] edgeLines, out QVector[] measurePoints)
        {
            measurePoints = null;

            try
            {
                if (chipData == null)
                    return ErrorCodes.ERR_NO_CHIP_LOCATION;

                if (edgeLines == null || edgeLines.Length < 4 || edgeLines[0] == null || edgeLines[1] == null || edgeLines[2] == null || edgeLines[3] == null)
                    return ErrorCodes.ERR_NO_CHIP_PADS;

                //(1) chipQuad
                var chipQuad = chipData.ChipQuad2D;
                if (chipQuad == null || chipQuad.Corners == null || chipQuad.Corners.Length < 4)
                    return ErrorCodes.ERR_NO_CHIP_LOCATION;

                //(2) 左上, 右上, 右下, 左下 
                var corners = chipQuad.Corners;

                var L = (corners[0] + corners[3]) / 2.0;
                var R = (corners[1] + corners[2]) / 2.0;
                var T = (corners[0] + corners[1]) / 2.0;
                var B = (corners[3] + corners[2]) / 2.0;

                var lineH = new EzLSD.LineSegment(L, R);
                var lineV = new EzLSD.LineSegment(T, B);

                L = edgeLines[0].CalcIntersectedPoint(lineH);   // 左
                T = edgeLines[1].CalcIntersectedPoint(lineV);   // 上
                R = edgeLines[2].CalcIntersectedPoint(lineH);   // 右
                B = edgeLines[3].CalcIntersectedPoint(lineV);   // 下

                // 順序: 左上右下
                measurePoints = new QVector[] { L, T, R, B };
                return ErrorCodes.OK;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "CalcDimMeasurePoints 異常");
                return ErrorCodes.ERR_NO_CHIP_PADS;
            }
        }


        /// <summary>
        /// 計算 尺寸量測 具名點位對
        /// </summary>
        /// <param name="lineBorderPairs">輸入: Runtime 抓到的 具名 邊線對</param>
        /// <param name="measurePointPairs">輸出: 尺寸量測 具名點位對</param>
        /// <returns>錯誤碼</returns>
        /// <remarks>此函式座標都是 Fullfov Camera Coordinates</remarks>
        public static ErrorCodes CalcDimNamedMeasurePointPairs(this GaChipData chipData, LineBorderPairsCollection lineBorderPairs, out Dictionary<string, QVector[]> measurePointPairs)
        {
            measurePointPairs = new Dictionary<string, QVector[]>();

            try
            {
                if (chipData == null)
                    return ErrorCodes.ERR_NO_CHIP_LOCATION;

                //(1) chipQuad
                var chipQuad = chipData.ChipQuad2D;
                if (chipQuad == null || chipQuad.Corners == null || chipQuad.Corners.Length < 4)
                    return ErrorCodes.ERR_NO_CHIP_LOCATION;

                var corners = chipQuad.Corners;
                var L = (corners[0] + corners[3]) / 2.0;
                var R = (corners[1] + corners[2]) / 2.0;
                var T = (corners[0] + corners[1]) / 2.0;
                var B = (corners[3] + corners[2]) / 2.0;
                var vectH = R - L;
                var vectV = B - T;

                if (lineBorderPairs != null)
                {
                    foreach ((var keyName, var pair) in lineBorderPairs.IterPairs())
                    {
                        // 防禦：檢查 pair 與 LineSegments 是否完整
                        if (pair == null || pair.LineSegments == null || pair.LineSegments.Length < 2)
                            continue;

                        if (pair.LineSegments[0] == null || pair.LineSegments[1] == null)
                            continue;

                        var line0 = pair.LineSegments[0];
                        var line1 = pair.LineSegments[1];

                        bool isHoriz = keyName.StartsWith("X");
                        var vect = isHoriz ? vectH : vectV;

                        var p0 = line0.GetMidPoint();
                        var lineCross = new EzLSD.LineSegment(p0, p0 + vect);
                        var p1 = lineCross.CalcIntersectedPoint(line1);

                        measurePointPairs[keyName] = new[] { p0, p1 };
                    }
                }

                return ErrorCodes.OK;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "CalcDimNamedMeasurePointPairs 異常");
                return ErrorCodes.ERR_NO_CHIP_PADS;
            }
        }

        /// <summary>
        /// 計算 尺寸量測 所有點位
        /// </summary>
        /// <param name="lineBorderPairs">輸入: Runtime 抓到的 具名 邊線對</param>
        /// <param name="allMeasurePoints">輸出: 尺寸量測 所有點位</param>
        /// <param name="keyNames">輸出: 點位 對應的 量測名稱</param>
        /// <returns>錯誤碼</returns>
        /// <remarks>此函式座標都是 Fullfov Camera Coordinates</remarks>
        public static ErrorCodes CalcDimNamedMeasurePoints(this GaChipData chipData, LineBorderPairsCollection lineBorderPairs, out QVector[] allMeasurePoints, out string[] keyNames)
        {
            var err = CalcDimNamedMeasurePointPairs(chipData, lineBorderPairs, out var measurePointPairs);
            if (err == ErrorCodes.OK)
            {
                var keyNamesList = new List<string>();
                var measurePtsList = new List<QVector>();

                foreach (var kv in measurePointPairs)
                {
                    var keyName = kv.Key;
                    var pair = kv.Value;
                    if (pair != null && pair.Length >= 2)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            measurePtsList.Add(pair[i]);
                            keyNamesList.Add(keyName);
                        }
                    }
                }

                allMeasurePoints = measurePtsList.ToArray();
                keyNames = keyNamesList.ToArray();
            }
            else
            {
                allMeasurePoints = new QVector[0];
                keyNames = new string[0];
            }
            return err;
        }


        /// <summary>
        /// 計算出 邊隙量測 之 具名點位對
        /// </summary>
        /// <param name="edgeLines">輸入: 邊線順序 (左邊線, 上邊線, 右邊線, 下邊線)</param>
        /// <param name="gapMeasurePointPairs">輸出: 邊隙量測 之 具名點位對</param>
        /// <param name="cornerPadCenters">輸出: 晶粒四角格點 (左上, 右上, 右下, 左下)</param>
        /// <returns>錯誤碼</returns>
        /// <remarks>此函式座標都是 Fullfov Camera Coordinates</remarks>
        public static ErrorCodes CalcDefaultGapMeasurePointPairs(this GaChipData chipData, EzLSD.LineSegment[] edgeLines, out Dictionary<GapEnum, QVector[]> gapMeasurePointPairs, out QVector[] cornerPadCenters)
        {
            gapMeasurePointPairs = null;
            cornerPadCenters = null;

            try
            {
                if (chipData == null)
                    return ErrorCodes.ERR_NO_CHIP_LOCATION;

                if (edgeLines == null || edgeLines.Length < 4)
                    return ErrorCodes.ERR_LACK_CHIP_PAD_CORNER;

                // 重複利用 CalcCornerPadCenters 同時取得「中心點」與「外框邊界點」，避免重複呼叫 CalcQuad2D
                var err = chipData.CalcCornerPadCenters(out cornerPadCenters, out var padsBoundaryCorners);
                if (err != ErrorCodes.OK)
                    return err;

                //------------------------------------------------
                // 計算量測點位
                // 順序: LDX, LUX, LUY, RUY, RUX, RDX, RDY, LDY
                //------------------------------------------------
                var indexTable = new int[][]
                {
                    new int[] {  0,  3, 2,  5, (int)GapEnum.LDX, (int)GapEnum.RDX, },
                    new int[] {  1,  0, 1,  4, (int)GapEnum.LUX, (int)GapEnum.RUX, },
                    new int[] {  2,  0, 3,  7, (int)GapEnum.LUY, (int)GapEnum.LDY, },
                    new int[] {  3,  1, 2,  6, (int)GapEnum.RUY, (int)GapEnum.RDY, },
                };

                gapMeasurePointPairs = new Dictionary<GapEnum, QVector[]>();

                foreach (int[] idxs in indexTable)
                {
                    int gi = idxs[0];
                    int pi = idxs[1];
                    int pj = idxs[2];
                    int gj = idxs[3];
                    var iGap = (GapEnum)idxs[4];
                    var jGap = (GapEnum)idxs[5];

                    var Pi = cornerPadCenters[pi];
                    var Pj = cornerPadCenters[pj];
                    var padLine = new EzLSD.LineSegment(Pi, Pj);

                    var edgeLineI = edgeLines[gi / 2];
                    var edgeLineJ = edgeLines[gj / 2];

                    var Gi = edgeLineI != null ? padLine.CalcIntersectedPoint(edgeLineI) : null;
                    var Gj = edgeLineJ != null ? padLine.CalcIntersectedPoint(edgeLineJ) : null;

                    var lenOutter = (padsBoundaryCorners[pi] - padsBoundaryCorners[pj]).NormLength;
                    var len = (Pi - Pj).NormLength;
                    if (len > 0)
                    {
                        var ext = (lenOutter - len) / 2.0;
                        var U = (Pi - Pj) / len;
                        Pi = Pi + U * ext;
                        Pj = Pj - U * ext;
                    }

                    gapMeasurePointPairs[iGap] = new[] { Gi, Pi };
                    gapMeasurePointPairs[jGap] = new[] { Gj, Pj };
                }

                return ErrorCodes.OK;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "CalcDefaultGapMeasurePointPairs 異常");
                return ErrorCodes.ERR_LACK_CHIP_PAD_CORNER;
            }
        }

        /// <summary>
        /// 計算出 邊隙量測 所有 點位
        /// </summary>
        public static ErrorCodes CalcDefaultGapMeasurePoints(this GaChipData chipData, EzLSD.LineSegment[] edgeLines, out QVector[] measurePoints, out QVector[] cornerPadCenters)
        {
            measurePoints = null;
            cornerPadCenters = null;

            var err = CalcDefaultGapMeasurePointPairs(chipData, edgeLines, out var pairs, out cornerPadCenters);
            if (err != ErrorCodes.OK)
            {
                return err;
            }

            var list = new List<QVector>();
            foreach (var gap in IterGaps())
            {
                if (pairs.TryGetValue(gap, out var pairPts) && pairPts != null)
                {
                    list.AddRange(pairPts);
                }
            }

            measurePoints = list.ToArray();
            return ErrorCodes.OK;
        }

        /// <summary>
        /// 計算出 Runtime 邊隙量測 所有 點位
        /// </summary>
        public static ErrorCodes CalcRuntimeGapMeasurePoints(this GaChipData chipData, EzLSD.LineSegment[] edgeLines, out QVector[] measurePoints, out QVector[] cornerPadCenters)
        {
            measurePoints = null;
            cornerPadCenters = null;

            try
            {
                if (chipData == null)
                    return ErrorCodes.ERR_NO_CHIP_LOCATION;

                if (edgeLines == null || edgeLines.Length < 4)
                    return ErrorCodes.ERR_LACK_CHIP_PAD_CORNER;

                var gapRuntimePairs = chipData.GapBorderPairs;
                if (gapRuntimePairs == null)
                {
                    return CalcDefaultGapMeasurePoints(chipData, edgeLines, out measurePoints, out cornerPadCenters);
                }

                // 計算 P0, P1, P2, P3
                var err = CalcCornerPadCenters(chipData, out cornerPadCenters, out var _);
                if (err != ErrorCodes.OK)
                {
                    return err;
                }

                // 計算 LDX, LUX, LUY, RUY, RUX, RDX, RDY, LDY
                var result = new List<QVector>();
                foreach (var gap in IterGaps())
                {
                    string gapKey = gap.ToString();
                    if (!gapRuntimePairs.ContainsKey(gapKey) || gapRuntimePairs[gapKey] == null ||
                        gapRuntimePairs[gapKey].LineSegments == null || gapRuntimePairs[gapKey].LineSegments.Length == 0)
                    {
                        result.Add(null);
                        result.Add(null);
                        continue;
                    }

                    var padLine = gapRuntimePairs[gapKey].LineSegments[0];
                    var P = padLine?.GetMidPoint();

                    var borderId = (int)gap.GetBorderID();
                    EzLSD.LineSegment edgeLine = null;
                    if (borderId >= 0 && borderId < edgeLines.Length)
                    {
                        edgeLine = edgeLines[borderId];
                    }

                    var G = (P != null && edgeLine != null) ? edgeLine.CalcTheNearestPoint(P) : null;

                    result.Add(G);
                    result.Add(P);
                }

                measurePoints = result.ToArray();
                return ErrorCodes.OK;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "CalcRuntimeGapMeasurePoints 異常");
                return ErrorCodes.ERR_LACK_CHIP_PAD_CORNER;
            }
        }


        /// <summary>
        /// 計算出 Corner Pad Centers 與 Boundary Corners
        /// </summary>
        /// <returns>錯誤碼</returns>
        /// <remarks>此函式座標都是 Fullfov Camera Coordinates</remarks>
        static ErrorCodes CalcCornerPadCenters(this GaChipData chipData, out QVector[] cornerPadCenters, out QVector[] boundaryCorners)
        {
            cornerPadCenters = null;
            boundaryCorners = null;

            try
            {
                if (chipData == null)
                    return ErrorCodes.ERR_NO_CHIP_LOCATION;

                //(1) PADs GRID (Fullfov Coordinates)
                var padsGrid = chipData.PadsGrid;
                if (padsGrid == null)
                    return ErrorCodes.ERR_NO_CHIP_PADS;

                //(2) PAD CORNERs : 0左上, 1右上, 2右下, 3左下
                EzBlocsGridAnalyzer.CalcQuad2D(padsGrid, out QvQuad2D padsBoundaryQuad, true);
                EzBlocsGridAnalyzer.CalcQuad2D(padsGrid, out QvQuad2D padsMidQuad, false);

                //(2.1) 檢查例外狀況
                int NP = 4;
                var padsBoundaryCorners = padsBoundaryQuad?.Corners;
                var padsMidCorners = padsMidQuad?.Corners;
                if (padsBoundaryCorners == null || padsBoundaryCorners.Length < NP ||
                    padsMidCorners == null || padsMidCorners.Length < NP)
                    return ErrorCodes.ERR_LACK_CHIP_PAD_CORNER;

                for (int i = 0; i < NP; i++)
                {
                    if (padsBoundaryCorners[i] == null || padsMidCorners[i] == null)
                        return ErrorCodes.ERR_LACK_CHIP_PAD_CORNER;
                }

                //(2.2) 輸出結果
                boundaryCorners = Array.ConvertAll(padsBoundaryCorners, c => new QVector(c));
                cornerPadCenters = Array.ConvertAll(padsMidCorners, c => new QVector(c));

                return ErrorCodes.OK;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "CalcCornerPadCenters 異常");
                return ErrorCodes.ERR_LACK_CHIP_PAD_CORNER;
            }
        }

        /// <summary>
        /// 順序: LDX, LUX, LUY, RUY, RUX, RDX, RDY, LDY
        /// </summary>
        static IEnumerable<GapEnum> IterGaps()
        {
            yield return GapEnum.LDX;
            yield return GapEnum.LUX;
            yield return GapEnum.LUY;
            yield return GapEnum.RUY;

            yield return GapEnum.RUX;
            yield return GapEnum.RDX;
            yield return GapEnum.RDY;
            yield return GapEnum.LDY;
        }


        #region LOG_FUNCTIONS
        static void _LOG_ERROR(Exception ex, string msg)
        {
            GaUtil.LOG_ERROR(ex, msg);
        }
        #endregion
    }
}
