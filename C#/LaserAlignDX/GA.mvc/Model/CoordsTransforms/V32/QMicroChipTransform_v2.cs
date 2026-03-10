#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-10-07 優化 (by LeTian Chang)
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
using JetEazy.Transform;
using JetEazy.Utils;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using ErrCodes = LaserAlignDX.Mvc.Model.ErrCodes;


namespace LaserAlignDX.Model.Coords
{
    /// <summary>
    /// 透視投影座標轉換
    /// </summary>
    public partial class QMicroChipTransform : ITransform
    {
        #region PRIVATE_DATA
        QVector _roiCenterPt = new QVector(0, 0);      // camera coordinates (pixels)
        QVector _mmPerPixel = new QVector(1.0, 1.0);
        ITransform _localTrf = null;
        ITransform _padsTrf = null;     // 只使用其點位座標
        #endregion

        public QMicroChipTransform(string name, string srcUnit, string dstUnit)
        {
            _localTrf = new QTransform(name, srcUnit, dstUnit);
            _padsTrf = new QTransform(name + "_PAD", srcUnit, dstUnit);
        }
        public QMicroChipTransform(string name) : this(name, "pix", "mm")
        {
        }
        public void Dispose()
        {
            _localTrf?.Dispose();
            _localTrf = null;
            _padsTrf?.Dispose();
            _padsTrf = null;
        }

        public string Name
        {
            get => _localTrf.Name;
        }
        public ICalibGridPoints GetCalibGridPoints()
        {
            return _localTrf.GetCalibGridPoints();
        }
        public ICalibCornerPoints GetCalibCornerPoints()
        {
            return _localTrf.GetCalibCornerPoints();
        }
        public bool CheckBuildCondition(out double det, out double det2)
        {
            return _localTrf.CheckBuildCondition(out det, out det2);
        }
        QVector ITransform.Trans(QVector pt)
        {
            var q = _localTrf.Trans(pt);
            return q;
        }
        QVector ITransform.InvTrans(QVector pt)
        {
            var q = _localTrf.InvTrans(pt);
            return q;
        }

        public bool Build()
        {
            var err = buildTrfs();
            bool ok = err == ErrCodes.OK;
            return ok;
        }
        public void Load(string filename)
        {
            if (filename == null)
                filename = getRecipeIniFileName();

            _localTrf?.Load(filename);
            _padsTrf?.Load(filename);

            string sectName = Name + "_extra_settings";
            _roiCenterPt?.LoadIni(filename, sectName, "roiCenterPt");
            _mmPerPixel?.LoadIni(filename, sectName, "mmPerPixel");
        }
        public void Save(string filename)
        {
            if (filename == null)
                filename = getRecipeIniFileName();

            _localTrf?.Save(filename);
            _padsTrf?.Save(filename);

            string sectName = Name + "_extra_settings";
            _roiCenterPt?.SaveIni(filename, sectName, "roiCenterPt");
            _mmPerPixel?.SaveIni(filename, sectName, "mmPerPixel", fixDigit: false);
        }

        ErrCodes buildTrfs()
        {
            ErrCodes err = ErrCodes.OK;
            var trfs = new[] { _localTrf, _padsTrf };
            var errs = new[] { ErrCodes.ERR_WEAK_LINE_CONDITION, ErrCodes.ERR_WEAK_PADS_CONDITION };
            int idx = 0;
            foreach (var trf in trfs)
            {
                if (trf != null)
                {
                    bool ok = trf.Build();
                    trf.CheckBuildCondition(out var det1, out var det2);
                    GaUtil.LOG($"MATRIX_{trf.Name} det1 = {det1:0.000000}");
                    GaUtil.LOG($"MATRIX_{trf.Name} det2 = {det2:0.000000}");
                    if (!ok && err == ErrCodes.OK)
                        err = errs[idx];
                }
                idx++;
            }
            return err;
        }

        public ErrCodes BuildMicroTransform(SizeF targetDim, EzLSD.LineSegment[] lines, GaChipData chipData)
        {
            int NP = 4;

            //(0) Check Lines Condition
            var err = check(lines, chipData);
            if (err != ErrCodes.OK)
                return err;

            //(1) ROI offset
            var chipQuad2D = chipData.ChipQuad2D;
            _roiCenterPt = new QVector(chipQuad2D.Center);

            //(2) MeasurePoints (local scope) : 左0 上1 右2 下3 
            var camMeasurePts = getDimMeasurePoints(lines, chipQuad2D);
            camMeasurePts = Array.ConvertAll(camMeasurePts, p => p - _roiCenterPt);

            //(3) DST POINTS (local scope)
            double W = targetDim.Width;
            double H = targetDim.Height;
            var dstMidPoints = new QVector[]
            {
                new QVector(0, H/2),            //mid LEFT
                new QVector(W/2, 0),            //mid TOP
                new QVector(W, H/2),            //mid RIGHT
                new QVector(W/2, H),            //mid Bottom
            };

            //(4) localTrf (local scope)
            #region localTrf
            var impCalib = _localTrf.GetCalibCornerPoints();
            for (int i = 0; i < NP; i++)
                impCalib.Set(i, camMeasurePts[i], dstMidPoints[i]);
            _localTrf.Build();
            #endregion

            //(5) padsTrf (local scope)
            #region padsTrf
            var padsGrid = chipData.PadsGrid;
            if (padsGrid != null)
            {
                EzBlocsGridAnalyzer.CalcRotatedBox2D(padsGrid, out var padsBox2D, false);
                var padsCamCorners = Array.ConvertAll(padsBox2D.Corners, c => new QVector(c.X, c.Y) - _roiCenterPt);
                var padsCalib = _padsTrf.GetCalibCornerPoints();
                for (int i = 0; i < NP; ++i)
                {
                    var dst = _localTrf.Trans(padsCamCorners[i]);
                    padsCalib.Set(i, padsCamCorners[i], dst);
                }
            }
            #endregion

            //(6) build
            err = buildTrfs();

            //(7) verify
            #region VERIFY
            if (true)
            {
                var microWorldPts = new QVector[NP];
                for (int i = 0; i < NP; i++)
                {
                    var p = _localTrf.Trans(camMeasurePts[i]);
                    var d = (dstMidPoints[i] - p).NormLength;
                    microWorldPts[i] = p;
                    System.Diagnostics.Debug.WriteLine($"Micro_Err[{i}] = {d:0.000} mm");
                }
                W = (microWorldPts[0] - microWorldPts[2]).NormLength;
                H = (microWorldPts[1] - microWorldPts[3]).NormLength;
                System.Diagnostics.Debug.WriteLine($"Micro_W_Err = {(targetDim.Width - W):0.000} mm");
                System.Diagnostics.Debug.WriteLine($"Micro_H_Err = {(targetDim.Height - H):0.000} mm");
            }
            #endregion

            //(8) 計算 mm per pixel
            #region 計算_mm_per_pixel
            var c0 = camMeasurePts[0];
            var c2 = camMeasurePts[2];  c2.Y = c0.Y;
            var c1 = camMeasurePts[1];
            var c3 = camMeasurePts[3];  c3.X = c1.X;
            var q0 = _localTrf.Trans(c0);
            var q2 = _localTrf.Trans(c2);
            var q1 = _localTrf.Trans(c1);
            var q3 = _localTrf.Trans(c3);
            var w_pix = (c0 - c2).NormLength;
            var h_pix = (c1 - c3).NormLength;
            var w_mm = (q0 - q2).NormLength;
            var h_mm = (q1 - q3).NormLength;
            double w_res = w_mm / (w_pix + 1e-6);
            double h_res = h_mm / (h_pix + 1e-6);
            _mmPerPixel = new QVector(w_res, h_res);
            #endregion

            return err;
        }
        public ErrCodes CalcChipDimension(out SizeF dimension, EzLSD.LineSegment[] lines, GaChipData chipData, bool includePadGaps)
        {
            return CalcChipDimension_PadTrf(out dimension, lines, chipData, includePadGaps);
        }

        #region PRIVATE_CALC_DIMENSION_FUNCTIONS
#if(false)
        ErrCodes CalcChipDimension_001(out SizeF dimension, EzLSD.LineSegment[] lines, GaChipData chipData, bool includePadGaps)
        {
            dimension = SizeF.Empty;

            //(0) check lines condition
            var err = check(lines, chipData);
            if (err != ErrCodes.OK)
                return err;

            //(1) ROI offset
            var cc = chipData.ChipQuad2D.Center;
            var runtimeChipCenter = new QVector(cc.X, cc.Y);

            //(2) 取得 measurePts (local scope) 順序: 左0 上1 右2 下3
            var localLines = Array.ConvertAll(lines, line => new EzLSD.LineSegment(line.P1 - runtimeChipCenter, line.P2 - runtimeChipCenter));
            var workLines = transform(localLines, _localTrf);
            var dimMeasurePts = getMidMeasurePoints(workLines);

            // (1) PAD 比例修正
            double scaleW = 1.0;
            double scaleH = 1.0;
            var padsGrid = chipData.PadsGrid;
            if (padsGrid != null)
            {
                // Golden PadsCorner (local scop)
                var goldenPadsCorners = _padsTrf?.GetCalibCornerPoints()?.GetAll(isSrc: true);
                if (goldenPadsCorners != null)
                {
                    var goldenMpts = getMidMeasurePoints(goldenPadsCorners);
                    var goldenPadW = (goldenMpts[0] - goldenMpts[2]).NormLength;
                    var goldenPadH = (goldenMpts[1] - goldenMpts[3]).NormLength;

                    // Runtime PadsCorners (local scope)
                    EzBlocsGridAnalyzer.CalcRotatedBox2D(padsGrid, out var runtimePadsBox2D, false);
                    var runPadsCorners = Array.ConvertAll(runtimePadsBox2D.Corners, c => new QVector(c.X, c.Y) - runtimeChipCenter);
                    var runMpts = getMidMeasurePoints(runPadsCorners);
                    var runPadW = (runMpts[0] - runMpts[2]).NormLength;
                    var runPadH = (runMpts[1] - runMpts[3]).NormLength;

                    scaleW = (goldenPadW / runPadW);
                    scaleH = (goldenPadH / runPadH);
                }
            }

            // (2) 計算距離
            double W = (dimMeasurePts[0] - dimMeasurePts[2]).NormLength;
            double H = (dimMeasurePts[1] - dimMeasurePts[3]).NormLength;
            W *= scaleW;
            H *= scaleH;

            // (3) Results
            dimension.Width = (float)Math.Round(W, 3);
            dimension.Height = (float)Math.Round(H, 3);
            return ErrCodes.OK;
        }
        ErrCodes CalcChipDimension_002(out SizeF dimension, EzLSD.LineSegment[] lines, GaChipData chipData, bool includePadGaps)
        {
            dimension = SizeF.Empty;

            //(0) check lines condition
            var err = check(lines, chipData);
            if (err != ErrCodes.OK)
                return err;

            //(1) ROI offset
            var chipQuad2D = chipData.ChipQuad2D;
            var runtimeChipCenter = new QVector(chipQuad2D.Center);

            //(2) 取得 measurePts (local scope) 順序: 左0 上1 右2 下3
            var localLines = Array.ConvertAll(lines, line => new EzLSD.LineSegment(line.P1 - runtimeChipCenter, line.P2 - runtimeChipCenter));
            var workLines = transform(localLines, _localTrf);
            var dimMeasurePts = getMidMeasurePoints(workLines);
            //(2.1) 將 camera measurePts (pixels) 存入 chipData
            chipData.ChipDimension.DimMeasurePoints = _GetDimMeasurePoints(lines, chipData.ChipQuad2D);

            //(3) PAD Transform (runtime)
            int NP = 4;
            ITransform runtimePadTrf = null;
            var padsGrid = chipData.PadsGrid;
            if (padsGrid != null)
            {
                //(3.1) Golden PadsCorner (local scop)
                var goldenPadsCorners = _padsTrf?.GetCalibCornerPoints()?.GetAll(isSrc: true);
                if (goldenPadsCorners != null)
                {
                    //(3.2) Runtime PadsCorners (local scope)
                    runtimePadTrf = new QTransform("RunTimePadTrf");
                    if (true)
                    {
                        EzBlocsGridAnalyzer.CalcRotatedBox2D(padsGrid, out var runtimePadsBox2D, false);
                        var runPadsCorners = Array.ConvertAll(runtimePadsBox2D.Corners, c => new QVector(c.X, c.Y) - runtimeChipCenter);
                        var trfCalib = runtimePadTrf.GetCalibCornerPoints();
                        for (int i = 0; i < NP; i++)
                            trfCalib.Set(i, runPadsCorners[i], goldenPadsCorners[i]);
                        bool ok = runtimePadTrf.Build();
                        if (!ok)
                        {
                            runtimePadTrf.Dispose();
                            return CalcChipDimension_001(out dimension, lines, chipData, includePadGaps);
                        }
                    }

                    //(3.3) 再次將 dimMeasurePts 投影
                    for (int i = 0; i < NP; i++)
                        dimMeasurePts[i] = runtimePadTrf.Trans(dimMeasurePts[i]);
                }
            }

            //(4) 計算距離
            double W = (dimMeasurePts[0] - dimMeasurePts[2]).NormLength;
            double H = (dimMeasurePts[1] - dimMeasurePts[3]).NormLength;

            //(5) Chip Width and Height
            dimension.Width = (float)Math.Round(W, 3);
            dimension.Height = (float)Math.Round(H, 3);
            //(5.1) 存入 chipData
            chipData.ChipDimension.ChipWidth = dimension.Width;
            chipData.ChipDimension.ChipHeight = dimension.Height;

            //(6) 計算邊隙
            if (includePadGaps)
            {
                err = CalcPadEdgeGaps(workLines, lines, chipData, runtimePadTrf);
            }

            //(*) CleanUp
            runtimePadTrf?.Dispose();
            runtimePadTrf = null;
            return ErrCodes.OK;
        }
#endif
        ErrCodes CalcChipDimension_PadTrf(out SizeF dimension, EzLSD.LineSegment[] lines, GaChipData chipData, bool includePadGaps)
        {
            dimension = SizeF.Empty;
            ITransform runtimePadTrf = null;

            try
            {
                //(0) check lines condition
                var err = check(lines, chipData);
                if (err != ErrCodes.OK)
                    return err;

                //(1) chipQuad2D
                var chipQuad2D = chipData.ChipQuad2D;
                var runtimeChipCenter = chipQuad2D.Center;

                //(2) 取得 camMeasurePoints (pixels)
                var camMeasurePoints = getDimMeasurePoints(lines, chipQuad2D);
                chipData.ChipDimension.DimMeasurePoints = camMeasurePoints;

                //(3) 平移到 LOCAL
                var measurePoints = Array.ConvertAll(camMeasurePoints, pt => (pt != null) ? pt - runtimeChipCenter : runtimeChipCenter);
                measurePoints = transform(measurePoints, _localTrf);

                //(4) PAD Transform (runtime)
                int NP = 4;
                var padsGrid = chipData.PadsGrid;
                if (padsGrid != null)
                {
                    //(3.1) Golden PadsCorner (local scop)
                    var goldenPadsCorners = _padsTrf?.GetCalibCornerPoints()?.GetAll(isSrc: true);
                    if (goldenPadsCorners != null)
                    {
                        //(3.2) runtimePadTrf
                        runtimePadTrf = new QTransform("RunTimePadTrf");
                        //(3.3) Runtime PadsCorners (local scope)
                        EzBlocsGridAnalyzer.CalcRotatedBox2D(padsGrid, out var runtimePadsBox2D, false);
                        var runPadsCorners = Array.ConvertAll(runtimePadsBox2D.Corners, c => new QVector(c.X, c.Y) - runtimeChipCenter);
                        //(3.4) Runtime calibration
                        var trfCalib = runtimePadTrf.GetCalibCornerPoints();
                        for (int i = 0; i < NP; i++)
                            trfCalib.Set(i, runPadsCorners[i], goldenPadsCorners[i]);
                        //(3.5) Build Transform
                        bool ok = runtimePadTrf.Build();
                        //(3.6) 再次將 measurePoints 投影
                        if (ok)
                            measurePoints = transform(measurePoints, runtimePadTrf);
                    }
                }

                //(4) 計算距離
                double W = (measurePoints[0] - measurePoints[2]).NormLength;
                double H = (measurePoints[1] - measurePoints[3]).NormLength;

                //(5) Chip Width and Height
                dimension.Width = (float)Math.Round(W, 3);
                dimension.Height = (float)Math.Round(H, 3);
                //(5.1) 存入 chipData
                chipData.ChipDimension.ChipWidth = dimension.Width;
                chipData.ChipDimension.ChipHeight = dimension.Height;

                //(6) 計算邊隙
                if (includePadGaps)
                    err = CalcPadEdgeGaps(null, lines, chipData, runtimePadTrf);
                return err;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "CalcChipDimension_PadTrf");
                return ErrCodes.ERR_EDGE_DIM_CALCULATION;
            }
            finally
            {
                //(*) CleanUp
                runtimePadTrf?.Dispose();
                runtimePadTrf = null;
            }
        }
        QVector[] getDimMeasurePoints(EzLSD.LineSegment[] lines, QvQuad2D chipQuad)
        {
            try
            {
                // 0 1
                // 3 2
                var corners = chipQuad.Corners;

                var L = (corners[0] + corners[3]) / 2.0;
                var R = (corners[1] + corners[2]) / 2.0;
                var T = (corners[0] + corners[1]) / 2.0;
                var B = (corners[3] + corners[2]) / 2.0;

                var lineH = new EzLSD.LineSegment(L, R);
                var lineV = new EzLSD.LineSegment(T, B);

                L = lines[0].CalcIntersectedPoint(lineH);   // 左
                T = lines[1].CalcIntersectedPoint(lineV);   // 上
                R = lines[2].CalcIntersectedPoint(lineH);   // 右
                B = lines[3].CalcIntersectedPoint(lineV);   // 下

                // 順序: 左上右下
                var measurePts = new QVector[] { L, T, R, B };
                return measurePts;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "GetDimMeasurePoints (H) 異常");
                return null;
            }
        }
        #endregion

        #region PRIVAE_CALC_PAD_EDGE_GAP_FUNCTIONS
        ErrCodes CalcPadEdgeGaps(EzLSD.LineSegment[] worldLines, EzLSD.LineSegment[]  camLines, GaChipData chipData, ITransform runtimePadTrf)
        {
            try
            {
                //(1) 取出 padGapMeasurePts (pixels)
                var err = getPadGapMeasurePoints(out var gapMeaturePts, chipData);
                if (err != ErrCodes.OK)
                    return err;

                //(2) 收集量測點 (單位 pixels)
                var camPoints = new List<QVector>();
                if (camLines != null)
                {
                    err = getPadGapMeasurePointPairs(out var gapMeasurePointPairs, camLines, gapMeaturePts);
                    if (err != ErrCodes.OK)
                        return err;
                    foreach (var pair in gapMeasurePointPairs)
                        foreach (var p in pair)
                            camPoints.Add(p);
                    chipData.PadEdgeGaps.GapMeasurePoints = camPoints.ToArray();
                }

                //(3) 轉換到 world (單位 mm)
                //(3.1) ROI offset
                var runtimeChipCenter = chipData.ChipQuad2D.Center;
                gapMeaturePts = Array.ConvertAll(camPoints.ToArray(), p => p != null ? p - runtimeChipCenter : null);
                //(3.2) Transform to world (local)
                gapMeaturePts = transform(gapMeaturePts, _localTrf);
                //(3.3) PadTransform
                gapMeaturePts = transform(gapMeaturePts, runtimePadTrf);
                //(4) 計算距離 並直接 存入 chipData
                //    順序: LDX, LUX, LUY, RUY, RUX, RDX, RDY, LDY
                int idx = 0;
                // LDX
                var p0 = gapMeaturePts[idx++];
                var p1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.LD.X = (float)Math.Round((p0 - p1).NormLength, 3);
                // LUX
                p0 = gapMeaturePts[idx++];
                p1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.LU.X = (float)Math.Round((p0 - p1).NormLength, 3);
                // LUY
                p0 = gapMeaturePts[idx++];
                p1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.LU.Y = (float)Math.Round((p0 - p1).NormLength, 3);
                // RUY
                p0 = gapMeaturePts[idx++];
                p1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.RU.Y = (float)Math.Round((p0 - p1).NormLength, 3);
                // RUX
                p0 = gapMeaturePts[idx++];
                p1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.RU.X = (float)Math.Round((p0 - p1).NormLength, 3);
                // RDX
                p0 = gapMeaturePts[idx++];
                p1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.RD.X = (float)Math.Round((p0 - p1).NormLength, 3);
                // RDY
                p0 = gapMeaturePts[idx++];
                p1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.RD.Y = (float)Math.Round((p0 - p1).NormLength, 3);
                // LDY
                p0 = gapMeaturePts[idx++];
                p1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.LD.Y = (float)Math.Round((p0 - p1).NormLength, 3);

                return ErrCodes.OK;
            }
            catch(Exception ex)
            {
                _LOG_ERROR(ex, "CalcPadEdgeGaps");
                return ErrCodes.ERR_EDGE_GAP_CALCULATION;
            }
        }
        ErrCodes getPadGapMeasurePoints(out QVector[] gapMeasurePoints, GaChipData chipData)
        {
            // NOTE: EzBlob 需要使用 QvBox2D 來提高定位精度 !!!
            //(0) 順序: LDX, LUX, LUY, RUY, RUX, RDX, RDY, LDY
            gapMeasurePoints = new QVector[8];

            //(1) GRID
            var padsGrid = chipData?.PadsGrid;
            if (padsGrid == null)
                return ErrCodes.ERR_NO_CHIP_PADS;

            //(2) PAD CORNERs : 0左上, 1右上, 2右下, 3左下
            var padCornerBlocs = padsGrid.GetCornerBlocs();
            var padCornerBoxes = Array.ConvertAll(padCornerBlocs, pb => pb?.ExtraBox2D);
            int NP = padCornerBoxes.Length;
            for (int i = 0; i < NP; i++)
            {
                if (padCornerBoxes[i] == null)
                    return ErrCodes.ERR_LACK_CHIP_PAD_CORNER;
            }

            //(3) 晶格角點: 0左上, 1右上, 2右下, 3左下
            var measurePts = new List<QVector>();
            for (int k = 0; k < NP; k++)
            {
                var k1 = k == 0 ? NP - 1 : (k - 1) % NP;
                var k2 = (k + 1) % NP;
                //System.Diagnostics.Debug.WriteLine("{0} : [{1}, {2}, {3}]", k, k1, k, k2);
                var pts = Array.ConvertAll(padCornerBoxes[k].Corners, c => new QVector(c.X, c.Y));
                var p1 = (pts[k] + pts[k1]) / 2.0;
                var p2 = (pts[k] + pts[k2]) / 2.0;
                measurePts.Add(p1);
                measurePts.Add(p2);
            }
            var pLast = measurePts[measurePts.Count - 1];
            measurePts.RemoveAt(measurePts.Count - 1);
            measurePts.Insert(0, pLast);

            gapMeasurePoints = measurePts.ToArray();
            return ErrCodes.OK;
        }
        ErrCodes getPadGapMeasurePointPairs(out QVector[][] gapMeasurePointPairs, EzLSD.LineSegment[] lines, QVector[] gapMeasurePoints)
        {
            // NOTE: EzBlob 需要使用 QvBox2D 來提高定位精度 !!!
            //(0) default return values
            int NP2 = gapMeasurePoints.Length;
            gapMeasurePointPairs = new QVector[NP2][];

            //(1) check lines
            var err = checkLines(lines);
            if (err != ErrCodes.OK)
            {
                _LOG_ERROR(new Exception(GaUtil.GetEnumDescription(err)), "getPadGapMeasurePointPairs");
                return err;
            }

            //(2) 各邊線 對應 點位 (每一邊兩點)
            var list = new List<QVector[]>();
            for (int i = 0; i < NP2; i += 2)
            {
                var k = (i / 2) % 4;
                var line = lines[k];
                var p1 = gapMeasurePoints[i];
                var q1 = line.CalcTheNearestPoint(p1);
                list.Add(new[] { p1, q1 });
                var p2 = gapMeasurePoints[i + 1];
                var q2 = line.CalcTheNearestPoint(p2);
                list.Add(new[] { p2, q2 });
            }

            gapMeasurePointPairs = list.ToArray();
            return ErrCodes.OK;
        }
        #endregion

        #region HELPER_FUNCTIONS
        ErrCodes check(EzLSD.LineSegment[] lines, GaChipData chipData)
        {
            if (chipData == null || chipData.ChipQuad2D == null)
                return ErrCodes.ERR_NO_CHIP_LOCATION;
            return checkLines(lines);
        }
        ErrCodes checkLines(EzLSD.LineSegment[] lines)
        {
            int count = 0;
            if (lines != null)
            {
                foreach (var line in lines)
                {
                    if (line != null)
                        count++;
                }
            }
            return count >= 4 ? ErrCodes.OK : ErrCodes.ERR_WEAK_LINE_CONDITION;
        }
        QVector[] getMidMeasurePoints(EzLSD.LineSegment[] lines)
        {
            // 順序: 左上右下
            var measurePts = new QVector[4];

            #region 寬方向
            bool okW = false;
            try
            {
                var line0 = lines[0];     //左邊線
                var line2 = lines[2];     //右邊線
                if (line0 != null && line2 != null)
                {
                    //(1) 取點
                    var P = line0.GetMidPoint();
                    var Q = line2.CalcTheNearestPoint(P);

                    //(2) Result
                    measurePts[0] = P;
                    measurePts[2] = Q;
                    okW = true;
                }
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "GetDimMeasurePoints (W) 異常");
                okW = false;
            }
            #endregion

            #region 高方向
            bool okH = false;
            try
            {
                var line1 = lines[1];     //上邊線
                var line3 = lines[3];     //下邊線
                if (line1 != null && line3 != null)
                {
                    //(1) 取點
                    var P = line1.GetMidPoint();
                    var Q = line3.CalcTheNearestPoint(P);

                    //(2) Result
                    measurePts[1] = P;
                    measurePts[3] = Q;
                    okH = true;
                }
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "GetDimMeasurePoints (H) 異常");
                okH = false;
            }
            #endregion

            return measurePts;
        }
        QVector[] transform(QVector[] points, ITransform trf)
        {
            if (points == null || trf == null)
                return points;
            var results = Array.ConvertAll(points, p => p != null ? trf.Trans(p) : null);
            return results;
        }
        #endregion

        #region LOG_FUNCTIONS
        void _LOG_ERROR(Exception ex, string msg)
        {
            GaUtil.LOG_ERROR(ex, msg);
        }
        #endregion

        private string getRecipeIniFileName()
        {
            var _xRecipe = RecipeFPIX3Class.Instance;
            CarrierEnum C = _xRecipe.ActiveCarrierID;
            string path = System.IO.Path.GetDirectoryName(_xRecipe.INIFILE);
            string iniFileName = System.IO.Path.Combine(path, $"micro_transform@{C}.ini");
            return iniFileName;
        }
    }
}
