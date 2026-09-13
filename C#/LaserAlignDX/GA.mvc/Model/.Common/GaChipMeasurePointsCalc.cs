#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-09-13 重整 (by LeTian Chang)
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
using System.Linq;

namespace LaserAlignDX.AoiModel
{
    /// <summary>
    /// 量測點計算器
    /// </summary>
    public static class GaChipMeasurePointsCalc
    {
        /// <summary>
        /// 計算 4邊線 上面的 尺寸量測點位 (順序: 左邊線, 上邊線, 右邊線, 下邊線)
        /// </summary>
        /// <remarks>
        /// 此函式 座標 都是 Fullfov Camera Coordinates
        /// </remarks>
        public static ErrorCodes CalcDimMeasurePoints(this GaChipData chipData, EzLSD.LineSegment[] edgeLines, out QVector[] measurePoints)
        {
            measurePoints = null;

            try
            {
                //(1) chipQuad
                var chipQuad = chipData?.ChipQuad2D;
                if (chipQuad == null)
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
        /// 計算 具名邊線對 上面的 量測點 (順序由 lineBorderPairs.Keys 決定)
        /// </summary>
        /// <remarks>
        /// 邊線 (lineBorderPairs) 與 晶粒外邊 (chipQuad) 都是 Fullfov Camera Coordinates
        /// </remarks>
        public static ErrorCodes CalcDimNamedMeasurePoints(this GaChipData chipData, LineBorderPairsCollection lineBorderPairs, out QVector[] allMeasurePoints, out string[] keyNames)
        {
            #region OLD_CODE
            //var measurePointsList = new List<QVector>();
            //var keyNamesList = new List<string>();

            //try
            //{
            //    var corners = chipQuad.Corners;
            //    var L = (corners[0] + corners[3]) / 2.0;
            //    var R = (corners[1] + corners[2]) / 2.0;
            //    var T = (corners[0] + corners[1]) / 2.0;
            //    var B = (corners[3] + corners[2]) / 2.0;
            //    var vectH = R - L;
            //    var vectV = B - T;

            //    if (lineBorderPairs != null)
            //    {
            //        foreach ((var keyName, var pair) in lineBorderPairs.IterPairs())
            //        {
            //            //var keyName = kvp.Key;
            //            //var pair = kvp.Value;

            //            // 防禦：檢查 pair 與 LineSegments 是否完整
            //            if (pair == null || pair.LineSegments == null || pair.LineSegments.Length < 2)
            //                continue;

            //            if (pair.LineSegments[0] == null || pair.LineSegments[1] == null)
            //                continue;

            //            var line0 = pair.LineSegments[0];
            //            var line1 = pair.LineSegments[1];

            //            bool isHoriz = keyName.StartsWith("X");
            //            var vect = isHoriz ? vectH : vectV;

            //            var p0 = line0.GetMidPoint();
            //            var lineCross = new EzLSD.LineSegment(p0, p0 + vect);
            //            var p1 = lineCross.CalcIntersectedPoint(line1);

            //            measurePointsList.Add(p0); keyNamesList.Add(keyName);
            //            measurePointsList.Add(p1); keyNamesList.Add(keyName);
            //        }
            //    }

            //    keyNames = keyNamesList.ToArray();
            //    allMeasurePoints = measurePointsList.ToArray();
            //    return allMeasurePoints.Length;
            //}
            //catch (Exception ex)
            //{
            //    _LOG_ERROR(ex, "getDimMeasurePoints 異常");
            //    keyNames = new string[0];
            //    allMeasurePoints = new QVector[0];
            //    return 0;
            //}
            #endregion

            var err = CalcDimNamedMeasurePoints(chipData, lineBorderPairs, out var allMeasurePointsDict);
            if (err == ErrorCodes.OK)
            {
                allMeasurePoints = allMeasurePointsDict.Keys.ToArray();
                keyNames = Array.ConvertAll(allMeasurePoints, p => allMeasurePointsDict[p]);
            }
            else
            {
                allMeasurePoints = new QVector[0];
                keyNames = new string[0];
            }
            return err;
        }

        /// <summary>
        /// 計算 具名邊線對 上面的 量測點 (順序由 lineBorderPairs.Keys 決定)
        /// </summary>
        /// <remarks>
        /// 邊線 (lineBorderPairs) 與 晶粒外邊 (chipQuad) 都是 Fullfov Camera Coordinates
        /// </remarks>
        public static ErrorCodes CalcDimNamedMeasurePoints(this GaChipData chipData, LineBorderPairsCollection lineBorderPairs, out Dictionary<QVector, string> allMeasurePoints)
        {
            allMeasurePoints = new Dictionary<QVector, string>();

            try
            {
                //(1) chipQuad
                var chipQuad = chipData?.ChipQuad2D;
                if (chipQuad == null)
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
                        //var keyName = kvp.Key;
                        //var pair = kvp.Value;

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

                        allMeasurePoints[p0] = keyName;
                        allMeasurePoints[p1] = keyName;
                    }
                }

                return ErrorCodes.OK;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "CalcDimNamedMeasurePoints 異常");
                return ErrorCodes.ERR_NO_CHIP_PADS;
            }
        }

        /// <summary>
        /// 計算出 邊隙量測 點位 (edgeLines 順序: 左邊線, 上邊線, 右邊線, 下邊線) 
        /// </summary>
        /// <remarks>
        /// 此函式 座標 都是 Fullfov Camera Coordinates
        /// </remarks>
        public static ErrorCodes CalcGapMeasurePointPairs(this GaChipData chipData, EzLSD.LineSegment[] edgeLines, out Dictionary<GapEnum, QVector[]> gapMeasurePointPairs, out QVector[] cornerPadCenters)
        {
            //------------------------------------------------
            //(0) 順序: LDX, LUX, LUY, RUY, RUX, RDX, RDY, LDY
            //------------------------------------------------
            //      g2    g3
            //
            //  g1 [P0]  [P1] g4
            //
            //  g0 [P3]  [P2] g5
            //
            //      g7    g6
            //------------------------------------------------

            int NP = 4;
            gapMeasurePointPairs = null;
            cornerPadCenters = null;

            //(1) PADs GRID (Fullfov Coordinates)
            var padsGrid = chipData?.PadsGrid;
            if (padsGrid == null)
                return ErrorCodes.ERR_NO_CHIP_PADS;

            //(2) PAD CORNERs : 0左上, 1右上, 2右下, 3左下
            EzBlocsGridAnalyzer.CalcQuad2D(padsGrid, out QvQuad2D padsBoundaryQuad, true);
            EzBlocsGridAnalyzer.CalcQuad2D(padsGrid, out QvQuad2D padsMidQuad, false);

            //(2.1) 檢查例外狀況
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
            //(2.2) [P0], [P1], [P2], [P3] 中心點
            cornerPadCenters = Array.ConvertAll(padsMidCorners, c => new QVector(c));

            //------------------------------------------------
            //(3) 計算量測點位
            //    順序: LDX, LUX, LUY, RUY, RUX, RDX, RDY, LDY
            //------------------------------------------------
            //      g2    g3
            //
            //  g1 [P0]  [P1] g4
            //
            //  g0 [P3]  [P2] g5
            //
            //      g7    g6
            //------------------------------------------------
            var indexTable = new int[][]
            {
                //欄位:     gi, pi, pj, gj                                          
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

                var Pi = padsMidCorners[pi];
                var Pj = padsMidCorners[pj];
                var padLine = new EzLSD.LineSegment(Pi, Pj);
                var Gi = padLine.CalcIntersectedPoint(edgeLines[gi / 2]);
                var Gj = padLine.CalcIntersectedPoint(edgeLines[gj / 2]);

                var lenOutter = (padsBoundaryCorners[pi] - padsBoundaryCorners[pj]).NormLength;
                var len = (Pi - Pj).NormLength;
                var ext = (lenOutter - len) / 2.0;
                var U = (Pi - Pj) / len;
                Pi = Pi + U * ext;
                Pj = Pj - U * ext;

                //int ii = gi * 2;
                //int jj = gj * 2;
                //gapMeasurePointPairs[ii] = Gi;
                //gapMeasurePointPairs[ii + 1] = Pi;
                //gapMeasurePointPairs[jj] = Gj;
                //gapMeasurePointPairs[jj + 1] = Pj;

                gapMeasurePointPairs[iGap] = new[] { Gi, Pi };
                gapMeasurePointPairs[jGap] = new[] { Gj, Pj };
            }

            return ErrorCodes.OK;
        }

        #region LOG_FUNCTIONS
        static void _LOG_ERROR(Exception ex, string msg)
        {
            GaUtil.LOG_ERROR(ex, msg);
        }
        #endregion
    }
}
