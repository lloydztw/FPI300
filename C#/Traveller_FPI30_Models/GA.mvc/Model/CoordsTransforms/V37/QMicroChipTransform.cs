#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-10-24 優化 (by LeTian Chang)
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
using LaserAlignDX.Model.Coords.Support;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using ErrorCodes = LaserAlignDX.Mvc.Model.ErrorCodes;

namespace LaserAlignDX.Model.Coords.V37
{
    /// <summary>
    /// 透視投影座標轉換
    /// </summary>
    internal partial class QMicroChipTransform : QRegistable, IMicroChipTransform
    {
        #region PRIVATE_DATA
        QVector _mmPerPixel = new QVector(1.0, 1.0);
        ITransform _localTrf = null;
        ITransform _padsTrf = null;     // 只使用其點位座標
        #endregion

        public QMicroChipTransform(string name, string srcUnit, string dstUnit)
        {
            Name = name;
            _localTrf = new QTransform(name, srcUnit, dstUnit);
            _padsTrf = new QTransform(name + "_PAD", "pix", "pix");
            Register(this);
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
            Unregister(this);
        }

        public override string Name
        {
            get;
            protected set;
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
            bool ok = err == ErrorCodes.OK;
            return ok;
        }
        public void Load(string filename)
        {
            if (filename == null)
                filename = getRecipeIniFileName();

            _localTrf?.Load(filename);
            _padsTrf?.Load(filename);

            string sectName = Name + "_extra_settings";
            //_roiCenterPt?.LoadIni(filename, sectName, "roiCenterPt");
            _mmPerPixel?.LoadIni(filename, sectName, "mmPerPixel");
        }
        public void Save(string filename)
        {
            if (filename == null)
                filename = getRecipeIniFileName();

            _localTrf?.Save(filename);
            _padsTrf?.Save(filename);

            string sectName = Name + "_extra_settings";
            //_roiCenterPt?.SaveIni(filename, sectName, "roiCenterPt");
            _mmPerPixel?.SaveIni(filename, sectName, "mmPerPixel");
        }

        ErrorCodes buildTrfs()
        {
            ErrorCodes err = ErrorCodes.OK;
            var trfs = new[] { _localTrf, _padsTrf };
            var errs = new[] { ErrorCodes.ERR_WEAK_LINE_CONDITION, ErrorCodes.ERR_WEAK_PADS_CONDITION };
            int idx = 0;
            foreach (var trf in trfs)
            {
                if (trf != null)
                {
                    bool ok = trf.Build();
                    trf.CheckBuildCondition(out var det1, out var det2);
                    GaUtil.LOG($"MATRIX_{trf.Name} det1 = {det1:0.000000}");
                    GaUtil.LOG($"MATRIX_{trf.Name} det2 = {det2:0.000000}");
                    if (!ok && err == ErrorCodes.OK)
                        err = errs[idx];
                }
                idx++;
            }
            return err;
        }

        public bool UseAveGaps4
        {
            get; set;
        }
        public ErrorCodes BuildMicroTransform(SizeF targetDim, EzLSD.LineSegment[] lines, GaChipData chipData)
        {
            int NP = 4;

            //(0) Check Lines Condition
            var err = check(lines, chipData);
            if (err != ErrorCodes.OK)
                return err;

            //(1) ROI offset
            var chipQuad2D = chipData.ChipQuad2D;
            var runtimeCenter = new QVector(chipQuad2D.Center);

            //(2) MeasurePoints (local scope) : 左0 上1 右2 下3 
            var camMeasurePts = getDimMeasurePoints(lines, chipQuad2D);
            camMeasurePts = Array.ConvertAll(camMeasurePts, p => p - runtimeCenter);

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
            if (padsGrid != null && _padsTrf != null)
            {
                EzBlocsGridAnalyzer.CalcRotatedBox2D(padsGrid, out var padsBox2D, false);
                var padsCamCorners = Array.ConvertAll(padsBox2D.Corners, c => new QVector(c.X, c.Y) - runtimeCenter);
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
            var L = camMeasurePts[0];
            var T = camMeasurePts[1];
            var R = camMeasurePts[2];
            var B = camMeasurePts[3];
            var qL = _localTrf.Trans(L);
            var qT = _localTrf.Trans(T);
            var qR = _localTrf.Trans(R);
            var qB = _localTrf.Trans(B);
            var w_pix = Math.Abs(L.X - R.X);
            var h_pix = Math.Abs(T.Y - B.Y);
            var w_mm = Math.Abs(qL.X - qR.X);
            var h_mm = Math.Abs(qT.Y - qB.Y);
            double w_res = w_mm / (w_pix + 1e-6);
            double h_res = h_mm / (h_pix + 1e-6);
            _mmPerPixel = new QVector(w_res, h_res);
            #endregion

            return err;
        }
        public ErrorCodes CalcChipDimension(out SizeF dimension, EzLSD.LineSegment[] lines, GaChipData chipData, bool includePadGaps)
        {
            return CalcChipDimension_PadTrf(out dimension, lines, chipData, includePadGaps);
        }

        public ErrorCodes BuildMicroTransform(SizeF targetDim, LineBorderPairsCollection lineBorderPairs, GaChipData chipData, RectangleF regionRoi)
        {
            //(0) 檢查
            var chipQuad = chipData?.ChipQuad2D;
            if (chipQuad == null)
                return ErrorCodes.ERR_NO_CHIP_LOCATION;

            //(1) 利用 chipQuad 建構 quadLines (FullFov Coordinates)
            int NP = 4;
            var quadLines = new EzLSD.LineSegment[NP];
            var corners = chipQuad.Corners;
            for (int i = 0; i < NP; i++)
            {
                int iPrev = i == 0 ? corners.Length - 1 : i - 1;
                quadLines[i] = new EzLSD.LineSegment(corners[i], corners[iPrev]);
            }

            //(2) 根據 lineBorderPairs["X"] 修改 quadLines
            if (lineBorderPairs.TryGetValue("X", out var pairX))
            {
                targetDim.Width = pairX.TargetDist;
                var L = pairX.LineSegments[0]?.Clone();
                var R = pairX.LineSegments[1]?.Clone();
                if (lineBorderPairs.IsLocal)
                {
                    L?.Offset(regionRoi.X, regionRoi.Y);    // Offset to FullFov coordinates
                    R?.Offset(regionRoi.X, regionRoi.Y);    // Offset to FullFov coordinates
                }
                if (L != null) quadLines[0] = L;            // LEFT
                if (R != null) quadLines[2] = R;            // RIGHT
            }

            //(3) 根據 lineBorderPairs["Y"] 修改 quadLines
            if (lineBorderPairs.TryGetValue("Y", out var pairY))
            {
                targetDim.Height = pairY.TargetDist;
                var T = pairY.LineSegments[0]?.Clone();
                var B = pairY.LineSegments[1]?.Clone();
                if (lineBorderPairs.IsLocal)
                {
                    T?.Offset(regionRoi.X, regionRoi.Y);    // Offset to FullFov coordinates
                    B?.Offset(regionRoi.X, regionRoi.Y);    // Offset to FullFov coordinates
                }
                if (T != null) quadLines[1] = T;            // TOP
                if (B != null) quadLines[3] = B;            // BOTTOM
            }

            //(4) 建立 MicroTransform
            ///NOTE: 重複利用原來的 MicroTransform (with _padsTrf==null) 來計算轉換矩陣
            var backup = _padsTrf;
            _padsTrf = null;

            var err = this.BuildMicroTransform(targetDim, quadLines, chipData);

            _padsTrf = backup;
            return ErrorCodes.OK;
        }
        public ErrorCodes CalcChipMeasurements(out Dictionary<string, float> results, LineBorderPairsCollection lineBorderPairs, GaChipData chipData, ITransform globalTrf = null)
        {
            /*
                [CalcChipMeasurements]
                    │
                    ▼ (呼叫內部私有方法)
                [CalcChipDimension_PadTrf2]
                       ├─► 1.getDimMeasurePoints (計算各對邊線與晶片中心線的交點 pixels)
                       ├─► 2.Shift to Local (扣除晶片中心座標 point -Center)
                       ├─► 3.Pads Transform (若有 PadsGrid，透過 runtimePadTrf 進行晶片旋轉 / 拉伸補償)
                       ├─► 4.Local Transform (_localTrf: 將 pixels 轉為實體物理 mm)
                       └─► 5.計算兩兩距離(NormLength) 並存入 results 字典(四捨五入至小數點後 3 位)
            */

            return CalcChipDimension_ChipTrf2(out results, lineBorderPairs, chipData, globalTrf);
        }

        #region PRIVATE_CALC_DIMENSION_FUNCTIONS
        ErrorCodes CalcChipDimension_PadTrf(out SizeF dimension, EzLSD.LineSegment[] lines, GaChipData chipData, bool includePadGaps)
        {
            dimension = SizeF.Empty;
            ITransform runtimePadTrf = null;

            try
            {
                //(0) check lines condition
                var err = check(lines, chipData);
                if (err != ErrorCodes.OK)
                    return err;

                //(1) chipQuad2D
                var chipQuad2D = chipData.ChipQuad2D;
                var runtimeChipCenter = new QVector(chipQuad2D.Center);

                //(2) 取得 camMeasurePoints (pixels)
                var camMeasurePoints = getDimMeasurePoints(lines, chipQuad2D);
                //chipData.ChipDimension.DimMeasurePoints = camMeasurePoints;
                chipData.ChipDimension.UpdateMeasurement("X", camMeasurePoints[0], camMeasurePoints[2]);
                chipData.ChipDimension.UpdateMeasurement("Y", camMeasurePoints[1], camMeasurePoints[3]);

                //(3) 平移到 LOCAL (以 runtimeChipCenter 當原點)
                var measurePoints = Array.ConvertAll(camMeasurePoints, pt => (pt != null) ? pt - runtimeChipCenter : runtimeChipCenter);

                //(4) PAD Transform (runtime)
                var padsGrid = chipData.PadsGrid;
                if (padsGrid != null)
                {
                    int NP = 4;

                    //(3.1) Golden PadsCorner (local scop)
                    var goldenPadsCorners = _padsTrf?.GetCalibCornerPoints()?.GetAll(isSrc: true);
                    if (goldenPadsCorners != null)
                    {
                        //(3.1.*) SHIFT 調整大角度排序
                        if (Math.Abs(chipQuad2D.Angle) > 70)
                            goldenPadsCorners = _SHIFT(goldenPadsCorners, chipQuad2D.Angle > 0 ? 1 : -1);

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

                //(5) local transform
                measurePoints = transform(measurePoints, _localTrf);

                //(6) 計算距離
                double W = (measurePoints[0] - measurePoints[2]).NormLength;
                double H = (measurePoints[1] - measurePoints[3]).NormLength;

                //(7) Chip Width and Height
                dimension.Width = (float)Math.Round(W, 3);
                dimension.Height = (float)Math.Round(H, 3);
                //(7.1) 存入 chipData
                chipData.ChipDimension.ChipWidth = dimension.Width;
                chipData.ChipDimension.ChipHeight = dimension.Height;

                //(8) 計算邊隙
                if (includePadGaps)
                    err = CalcPadEdgeGaps(lines, chipData, runtimePadTrf);

                return err;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "CalcChipDimension_PadTrf");
                return ErrorCodes.ERR_EDGE_DIM_CALCULATION;
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

#if (OPT_OLD_IMPLEMENTS)
        ErrorCodes _CalcChipDimension_PadTrf2_000(out Dictionary<string, float> results, LineBorderPairsCollection lineBorderPairs, GaChipData chipData)
        {
            results = new Dictionary<string, float>();
            ITransform runtimePadTrf = null;

            try
            {
                //(1) chipQuad2D
                var chipQuad2D = chipData?.ChipQuad2D;
                if (chipQuad2D == null)
                    return ErrorCodes.ERR_NO_CHIP_LOCATION;
                var runtimeChipCenter = new QVector(chipQuad2D.Center);

                //(2) 取得 camMeasurePointPairs (pixels)
                var camMeasurePoints = getDimNamedMeasurePoints(out var keyNames, lineBorderPairs, chipQuad2D);
                chipData.ChipDimension.DimMeasurePoints = camMeasurePoints;

                //(3) 平移到 LOCAL (以 runtimeChipCenter 當原點)
                var measurePoints = Array.ConvertAll(camMeasurePoints, pt => (pt != null) ? pt - runtimeChipCenter : runtimeChipCenter);

                //(4) PAD Transform (runtime)
                var padsGrid = chipData.PadsGrid;
                if (padsGrid != null)
                {
                    int NP = 4;

                    //(4.1) Golden PadsCorner(local scop)
                    var goldenPadsCorners = _padsTrf?.GetCalibCornerPoints()?.GetAll(isSrc: true);
                    if (goldenPadsCorners != null)
                    {
                        //(4.1.*) SHIFT 調整大角度排序
                        if (Math.Abs(chipQuad2D.Angle) > 70)
                            goldenPadsCorners = _SHIFT(goldenPadsCorners, chipQuad2D.Angle > 0 ? 1 : -1);

                        //(4.2) runtimePadTrf
                        runtimePadTrf = new QTransform("RunTimePadTrf");

                        //(4.3) Runtime PadsCorners (local scope)
                        EzBlocsGridAnalyzer.CalcRotatedBox2D(padsGrid, out var runtimePadsBox2D, false);
                        var runPadsCorners = Array.ConvertAll(runtimePadsBox2D.Corners, c => new QVector(c.X, c.Y) - runtimeChipCenter);

                        //(4.4) Runtime calibration
                        var trfCalib = runtimePadTrf.GetCalibCornerPoints();
                        for (int i = 0; i < NP; i++)
                            trfCalib.Set(i, runPadsCorners[i], goldenPadsCorners[i]);

                        //(4.5) Build Transform
                        bool ok = runtimePadTrf.Build();

                        //(4.6) 再次將 measurePoints 投影
                        if (ok)
                            measurePoints = transform(measurePoints, runtimePadTrf);
                    }
                }

                //(5) local transform
                measurePoints = transform(measurePoints, _localTrf);

                //(6) 計算距離
                for (int i = 0, N = measurePoints.Length; i < N; i += 2)
                {
                    //(6.0) Name
                    string name = keyNames[i];
                    
                    //(6.1) 兩兩距離
                    double dist = (measurePoints[i] - measurePoints[i + 1]).NormLength;

                    //(6.2) 存入 results
                    results[name] = (float)Math.Round(dist, 3);
                }

                return ErrorCodes.OK;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "CalcChipDimension_PadTrf2");
                return ErrorCodes.ERR_EDGE_DIM_CALCULATION;
            }
            finally
            {
                //(*) CleanUp
                runtimePadTrf?.Dispose();
                runtimePadTrf = null;
            }
        }
        QVector[] _getDimNamedMeasurePoints_000(out string[] keyNames, LineBorderPairsCollection pairs, QvQuad2D chipQuad)
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

                var measurePointsDict = new Dictionary<QVector, string>();
                var measurePoints = new List<QVector>();
                var keyNamesList = new List<string>();
                foreach (var keyName in pairs.Keys)
                {
                    var pair = pairs[keyName];
                    bool isHoriz = keyName.StartsWith("X");
                    var lineMid = isHoriz ? lineV : lineH;
                    var p1 = pair.LineSegments[0].CalcIntersectedPoint(lineMid);
                    var p2 = pair.LineSegments[1].CalcIntersectedPoint(lineMid);
                    measurePoints.Add(p1); keyNamesList.Add(keyName);
                    measurePoints.Add(p2); keyNamesList.Add(keyName);
                }

                keyNames = keyNamesList.ToArray();
                return measurePoints.ToArray();
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "getDimMeasurePointsDict 異常");
                keyNames = new string[0];
                return null;
            }
        }
#endif

        ErrorCodes CalcChipDimension_ChipTrf2(out Dictionary<string, float> results, LineBorderPairsCollection lineBorderPairs, GaChipData chipData, ITransform globalTrf)
        {
            // 目前暫時強制 停用 globalTrf
            globalTrf = null;

            results = new Dictionary<string, float>();

            ITransform runtimePadTrf = null;

            try
            {
                //(1) chipQuad
                var chipQuad2D = chipData?.ChipQuad2D;
                if (chipQuad2D == null)
                    return ErrorCodes.ERR_NO_CHIP_LOCATION;

                var runtimeChipCenter = new QVector(chipQuad2D.Center);

                //(2) 取得 camMeasurePoints (pixels)
                var camMeasurePoints = getDimNamedMeasurePoints(out var keyNames, lineBorderPairs, chipQuad2D);
                if (camMeasurePoints == null || camMeasurePoints.Length == 0 || camMeasurePoints.Length % 2 != 0)
                {
                    return ErrorCodes.ERR_WEAK_LINE_CONDITION;
                }

                //(2.*) 保存 camMeasurePoints
                //>>> chipData.ChipDimension.DimMeasurePoints = camMeasurePoints;
                for (int i = 0; i < camMeasurePoints.Length - 1; i += 2)
                {
                    chipData.ChipDimension.UpdateMeasurement(
                        keyNames[i], 
                        camMeasurePoints[i],
                        camMeasurePoints[i + 1]
                    );
                }

                //(3) 平移到 LOCAL (以 runtimeChipCenter 當原點)
                var measurePoints = globalTrf != null ?
                    camMeasurePoints :
                    Array.ConvertAll(camMeasurePoints, pt => pt != null ? pt - runtimeChipCenter : runtimeChipCenter);

                //(4) PAD Transform 補償 (目前停用)
                if (false)  // && globalTrf == null)
                {
                    var padsGrid = chipData.PadsGrid;
                    if (padsGrid != null)
                    {
                        int NP = 4;
                        var goldenPadsCorners = _padsTrf?.GetCalibCornerPoints()?.GetAll(isSrc: true);
                        if (goldenPadsCorners != null)
                        {
                            if (Math.Abs(chipQuad2D.Angle) > 70)
                                goldenPadsCorners = _SHIFT(goldenPadsCorners, chipQuad2D.Angle > 0 ? 1 : -1);

                            runtimePadTrf = new QTransform("RunTimePadTrf");

                            EzBlocsGridAnalyzer.CalcRotatedBox2D(padsGrid, out var runtimePadsBox2D, false);
                            var runPadsCorners = Array.ConvertAll(runtimePadsBox2D.Corners, c => new QVector(c.X, c.Y) - runtimeChipCenter);

                            var trfCalib = runtimePadTrf.GetCalibCornerPoints();
                            for (int i = 0; i < NP; i++)
                                trfCalib.Set(i, runPadsCorners[i], goldenPadsCorners[i]);

                            if (runtimePadTrf.Build())
                                measurePoints = transform(measurePoints, runtimePadTrf);
                        }
                    }
                }

                //(5) Local Transform
                var trf = globalTrf != null ? globalTrf : _localTrf;
                measurePoints = transform(measurePoints, trf);

                //(6) 計算距離
                for (int i = 0; i < measurePoints.Length - 1; i += 2)
                {
                    string name = keyNames[i];
                    double dist = (measurePoints[i] - measurePoints[i + 1]).NormLength;
                    results[name] = (float)Math.Round(dist, 3);
                }

                return ErrorCodes.OK;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "CalcChipDimension_PadTrf2");
                return ErrorCodes.ERR_EDGE_DIM_CALCULATION;
            }
            finally
            {
                runtimePadTrf?.Dispose();
            }
        }
        QVector[] getDimNamedMeasurePoints(out string[] keyNames, LineBorderPairsCollection lineBorderPairs, QvQuad2D chipQuad)
        {
            var measurePoints = new List<QVector>();
            var keyNamesList = new List<string>();

            try
            {
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
                        measurePoints.Add(p0); keyNamesList.Add(keyName);
                        measurePoints.Add(p1); keyNamesList.Add(keyName);
                    }
                }

                keyNames = keyNamesList.ToArray();
                return measurePoints.ToArray();
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "getDimMeasurePoints 異常");
                keyNames = Array.Empty<string>();
                return Array.Empty<QVector>();
            }
        }
        #endregion

        #region PRIVAE_CALC_PAD_EDGE_GAP_FUNCTIONS
#if (OPT_CALC_GAPS_000)
        ErrorCodes CalcPadEdgeGaps_000_none_S(EzLSD.LineSegment[] edgeLines, GaChipData chipData, ITransform runtimePadTrf)
        {
            try
            {
                ErrorCodes err;

                //(0) check
                err = check(edgeLines, chipData);
                if (err != ErrorCodes.OK)
                    return err;

                //(1) 收集量測點 (單位 pixels)
                err = getPadGapMeasurePoints(out QVector[] gapMeaturePts, edgeLines, chipData);
                if (err != ErrorCodes.OK)
                    return err;

                //(2) 紀錄量測點(使用複製, 以防止被後續運算改動) (單位 pixels)
                chipData.PadEdgeGaps.GapMeasurePoints = Array.ConvertAll(gapMeaturePts, p => new QVector(p));

                //(3) 平移到 LOCAL (以 runtimeChipCenter 當原點)
                var runtimeChipCenter = chipData.ChipQuad2D.Center;
                gapMeaturePts = Array.ConvertAll(gapMeaturePts, p => p != null ? p - runtimeChipCenter : runtimeChipCenter);

                //(4) Pad Transform (pix to pix)
                gapMeaturePts = transform(gapMeaturePts, runtimePadTrf);

                //(5) Local Transform (pix to mm)
                gapMeaturePts = transform(gapMeaturePts, _localTrf);

                //(6) 計算距離 並直接 存入 chipData
                //    順序: LDX, LUX, LUY, RUY, RUX, RDX, RDY, LDY
                //------------------------------------------------
                //       LUY    RUY  
                //          
                //  LUX  [P0]   [P1]  RUX
                //
                //  LDX  [P3]   [P2]  RDX
                //
                //       LDY    RDY
                //------------------------------------------------

                int idx = 0;
                // LDX
                var tp0 = gapMeaturePts[idx++];
                var tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.LD.X = Math.Round((tp0 - tp1).NormLength, 3);
                // LUX
                tp0 = gapMeaturePts[idx++];
                tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.LU.X = Math.Round((tp0 - tp1).NormLength, 3);
                // LUY
                tp0 = gapMeaturePts[idx++];
                tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.LU.Y = Math.Round((tp0 - tp1).NormLength, 3);
                // RUY
                tp0 = gapMeaturePts[idx++];
                tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.RU.Y = Math.Round((tp0 - tp1).NormLength, 3);
                // RUX
                tp0 = gapMeaturePts[idx++];
                tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.RU.X = Math.Round((tp0 - tp1).NormLength, 3);
                // RDX
                tp0 = gapMeaturePts[idx++];
                tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.RD.X = Math.Round((tp0 - tp1).NormLength, 3);
                // RDY
                tp0 = gapMeaturePts[idx++];
                tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.RD.Y = Math.Round((tp0 - tp1).NormLength, 3);
                // LDY
                tp0 = gapMeaturePts[idx++];
                tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.LD.Y = Math.Round((tp0 - tp1).NormLength, 3);

                return ErrorCodes.OK;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "CalcPadEdgeGaps");
                return ErrorCodes.ERR_EDGE_GAP_CALCULATION;
            }
        }
        ErrorCodes getPadGapMeasurePoints(out QVector[] gapMeasurePoints, EzLSD.LineSegment[] edgeLines, GaChipData chipData)
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
            gapMeasurePoints = null;

            //(1) GRID
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
                // 欄位:     gi, gj,  pi, pj     
                new int[] {  0,  5,   3,  2  },
                new int[] {  1,  4,   0,  1  },
                new int[] {  2,  7,   0,  3  },
                new int[] {  3,  6,   1,  2  },
            };

            gapMeasurePoints = new QVector[NP * 4];

            foreach (int[] idxs in indexTable)
            {
                int gi = idxs[0];
                int gj = idxs[1];
                int pi = idxs[2];
                int pj = idxs[3];

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

                int ii = gi * 2;
                int jj = gj * 2;
                //gapMeasurePoints[ii] = Pi;
                //gapMeasurePoints[ii + 1] = Gi;
                //gapMeasurePoints[jj] = Pj;
                //gapMeasurePoints[jj + 1] = Gj;
                gapMeasurePoints[ii] = Gi;
                gapMeasurePoints[ii + 1] = Pi;
                gapMeasurePoints[jj] = Gj;
                gapMeasurePoints[jj + 1] = Pj;
            }

            return ErrorCodes.OK;
        }
#endif
        ErrorCodes CalcPadEdgeGaps(EzLSD.LineSegment[] edgeLines, GaChipData chipData, ITransform runtimePadTrf)
        {
            try
            {
                ErrorCodes err;

                //(0) check
                err = check(edgeLines, chipData);
                if (err != ErrorCodes.OK)
                    return err;

                //(1) 收集量測點 (單位 pixels)
                err = getPadGapMeasurePoints(out QVector[] gapMeaturePts, out QVector[] padCornerCenters, edgeLines, chipData);
                if (err != ErrorCodes.OK)
                    return err;

                //(2) 紀錄量測點(使用複製, 以防止被後續運算改動) (單位 pixels)
                chipData.PadEdgeGaps.GapMeasurePoints = Array.ConvertAll(gapMeaturePts, p => new QVector(p));

                //(3) 平移到 LOCAL (以 runtimeChipCenter 當原點)
                var runtimeChipCenter = chipData.ChipQuad2D.Center;
                gapMeaturePts = Array.ConvertAll(gapMeaturePts, p => p != null ? p - runtimeChipCenter : runtimeChipCenter);
                padCornerCenters = Array.ConvertAll(padCornerCenters, p => p != null ? p - runtimeChipCenter : runtimeChipCenter);

                //(4) Pad Transform (pix to pix)
                gapMeaturePts = transform(gapMeaturePts, runtimePadTrf);
                padCornerCenters = transform(padCornerCenters, runtimePadTrf);

                //(5) Local Transform (pix to mm)
                gapMeaturePts = transform(gapMeaturePts, _localTrf);
                padCornerCenters = transform(padCornerCenters, _localTrf);

                //(6) 計算距離 並直接 存入 chipData
                //    順序: LDX, LUX, LUY, RUY, RUX, RDX, RDY, LDY
                //------------------------------------------------
                //            (S3)   (S1)
                //            LUY    RUY  
                //          
                //  (S4) LUX  [P0]   [P1]  RUX (S2)
                //
                //  (S8) LDX  [P3]   [P2]  RDX (S6)
                //
                //            LDY    RDY
                //            (S7)   (S5)  
                //------------------------------------------------

                int idx = 0;
                var PC = padCornerCenters;
                // LDX (S8)
                var tp0 = gapMeaturePts[idx++];
                var tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.LD.X = Math.Round((tp0 - tp1).NormLength, 3);
                chipData.PadEdgeGaps.S8 = Math.Round((tp0 - PC[3]).NormLength, 3);
                // LUX (S4)
                tp0 = gapMeaturePts[idx++];
                tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.LU.X = Math.Round((tp0 - tp1).NormLength, 3);
                chipData.PadEdgeGaps.S4 = Math.Round((tp0 - PC[0]).NormLength, 3);
                // LUY (S3)
                tp0 = gapMeaturePts[idx++];
                tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.LU.Y = Math.Round((tp0 - tp1).NormLength, 3);
                chipData.PadEdgeGaps.S3 = Math.Round((tp0 - PC[0]).NormLength, 3);
                // RUY (S1)
                tp0 = gapMeaturePts[idx++];
                tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.RU.Y = Math.Round((tp0 - tp1).NormLength, 3);
                chipData.PadEdgeGaps.S1 = Math.Round((tp0 - PC[1]).NormLength, 3);
                // RUX (S2)
                tp0 = gapMeaturePts[idx++];
                tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.RU.X = Math.Round((tp0 - tp1).NormLength, 3);
                chipData.PadEdgeGaps.S2 = Math.Round((tp0 - PC[1]).NormLength, 3);
                // RDX (S6)
                tp0 = gapMeaturePts[idx++];
                tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.RD.X = Math.Round((tp0 - tp1).NormLength, 3);
                chipData.PadEdgeGaps.S6 = Math.Round((tp0 - PC[2]).NormLength, 3);
                // RDY (S5)
                tp0 = gapMeaturePts[idx++];
                tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.RD.Y = Math.Round((tp0 - tp1).NormLength, 3);
                chipData.PadEdgeGaps.S5 = Math.Round((tp0 - PC[2]).NormLength, 3);
                // LDY (S7)
                tp0 = gapMeaturePts[idx++];
                tp1 = gapMeaturePts[idx++];
                chipData.PadEdgeGaps.LD.Y = Math.Round((tp0 - tp1).NormLength, 3);
                chipData.PadEdgeGaps.S7 = Math.Round((tp0 - PC[3]).NormLength, 3);

                // 每一邊線, 取平均值
                if (this.UseAveGaps4)
                {
                    MakeAveX(chipData.PadEdgeGaps.LD, chipData.PadEdgeGaps.LU);
                    MakeAveY(chipData.PadEdgeGaps.LU, chipData.PadEdgeGaps.RU);
                    MakeAveX(chipData.PadEdgeGaps.RU, chipData.PadEdgeGaps.RD);
                    MakeAveY(chipData.PadEdgeGaps.RD, chipData.PadEdgeGaps.LD);
                }

                return ErrorCodes.OK;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "CalcPadEdgeGaps");
                return ErrorCodes.ERR_EDGE_GAP_CALCULATION;
            }
        }
        ErrorCodes getPadGapMeasurePoints(out QVector[] gapMeasurePoints, out QVector[] padCornerCenters, EzLSD.LineSegment[] edgeLines, GaChipData chipData)
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
            gapMeasurePoints = null;
            padCornerCenters = null;

            //(1) GRID
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
            padCornerCenters = Array.ConvertAll(padsMidCorners, c => new QVector(c));

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
                // 欄位:     gi, gj,  pi, pj     
                new int[] {  0,  5,   3,  2  },
                new int[] {  1,  4,   0,  1  },
                new int[] {  2,  7,   0,  3  },
                new int[] {  3,  6,   1,  2  },
            };

            gapMeasurePoints = new QVector[NP * 4];

            foreach (int[] idxs in indexTable)
            {
                int gi = idxs[0];
                int gj = idxs[1];
                int pi = idxs[2];
                int pj = idxs[3];

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

                int ii = gi * 2;
                int jj = gj * 2;
                //gapMeasurePoints[ii] = Pi;
                //gapMeasurePoints[ii + 1] = Gi;
                //gapMeasurePoints[jj] = Pj;
                //gapMeasurePoints[jj + 1] = Gj;
                gapMeasurePoints[ii] = Gi;
                gapMeasurePoints[ii + 1] = Pi;
                gapMeasurePoints[jj] = Gj;
                gapMeasurePoints[jj + 1] = Pj;
            }

            return ErrorCodes.OK;
        }
        #endregion

        #region CALC_HELPER_FUNCTIONS
        void MakeAveX(QVector p1, QVector p2)
        {
            var x = (p1.X + p2.X) / 2.0;
            p1.X = p2.X = x;
        }
        void MakeAveY(QVector p1, QVector p2)
        {
            var y = (p1.Y + p2.Y) / 2.0;
            p1.Y = p2.Y = y;
        }
        #endregion

        #region HELPER_FUNCTIONS
        ErrorCodes check(EzLSD.LineSegment[] lines, GaChipData chipData)
        {
            if (chipData == null || chipData.ChipQuad2D == null)
                return ErrorCodes.ERR_NO_CHIP_LOCATION;
            return checkLines(lines);
        }
        ErrorCodes checkLines(EzLSD.LineSegment[] lines)
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
            return count >= 4 ? ErrorCodes.OK : ErrorCodes.ERR_WEAK_LINE_CONDITION;
        }
        QVector[] transform(QVector[] points, ITransform trf)
        {
            if (points == null || trf == null)
                return points;
            var results = Array.ConvertAll(points, p => p != null ? trf.Trans(p) : null);
            return results;
        }
        QVector[] _SHIFT(QVector[] corners, int dir, bool inplace = false)
        {
            if (corners == null || dir == 0) 
                return corners;

            int NP = corners.Length;
            var results = inplace ? corners : new QVector[NP];

            if (dir < 0)
            {
                var tmp = corners[0];
                for (int i = 1; i < NP; i++)
                    results[i - 1] = corners[i];
                results[NP - 1] = tmp;
            }
            else if (dir > 0)
            {
                var tmp = corners[NP - 1];
                for (int i = NP - 1; i > 0; i--)
                    results[i] = corners[i - 1];
                results[0] = tmp;
            }
            return results;
        }
        #endregion

        #region LOG_FUNCTIONS
        void _LOG_ERROR(Exception ex, string msg)
        {
            GaUtil.LOG_ERROR(ex, msg);
        }
        #endregion

        #region INI_FILE_HELPER_FUNCTIONS
        private string getRecipeIniFileName()
        {
            var _xRecipe = RecipeFPIX3Class.Instance;
            CarrierEnum C = _xRecipe.ActiveCarrierID;
            string path = System.IO.Path.GetDirectoryName(_xRecipe.INIFILE);
            string iniFileName = System.IO.Path.Combine(path, $"micro_transform@{C}.ini");
            return iniFileName;
        }
        #endregion
    }
}
